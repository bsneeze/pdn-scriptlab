/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Collections;
using PaintDotNet.Drawing;
using PaintDotNet.Effects;
using PaintDotNet.Imaging;
using PaintDotNet.Rendering;
using PaintDotNet.Threading;
using System;
using System.Drawing;
using System.Threading;

namespace pyrochild.effects.common
{
    /// <summary>
    /// This class can be used to apply an effect using background worker threads
    /// which raise an event when a certain amount of the effect has been processed.
    /// You can use that event to update a status bar, display a preview of the
    /// rendering so far, or whatever.
    /// 
    /// Since two threads are used for rendering, this will improve performance on
    /// dual processor systems, and possibly on systems that have HyperThreading.
    /// 
    /// This class is NOT SAFE for multithreaded access. Note that the events will 
    /// be raised from arbitrary threads. The only method that is safe to call from
    /// a thread that is not managing Start(), Abort(), and Join() is AbortAsync().
    /// You may then query whether the rendering actually aborted by using the Abort
    /// property. If it returns false, then AbortAsync() was not called in time to
    /// abort anything, which means the rendering completed fully.
    /// </summary>
    internal sealed class BackgroundEffectRenderer
        : IDisposable
    {
        private Effect effect;
        private EffectConfigToken effectToken; // this references the main token that is passed in to the constructor
        private EffectConfigToken effectTokenCopy; // this copy of the token is updated every time you call Start() to make sure it is up to date. This is then passed to the threads, not the original one.
        private IRenderer<ColorAlpha8> clipMaskRenderer; // this is to support antialiased selections, and we don't try to be smart about eliminating redundancy with renderRegion, etc.
        private PdnRegion renderRegion;
        private Rectangle[][] tileRegions;
        private PdnRegion[] tilePdnRegions;
        private int tileCount;
        private EffectRendererWorkItemQueue threadPool;
        private RenderArgs dstArgs;
        private RenderArgs srcArgs;
        private int workerThreads;
        private SynchronizedList<Exception> exceptions = new SynchronizedList<Exception>(new SegmentedList<Exception>());
        private volatile bool aborted = false;

        public event RenderedTileEventHandler RenderedTile;
        private void OnRenderedTile(RenderedTileEventArgs e)
        {
            if (RenderedTile != null)
            {
                RenderedTile(this, e);
            }
        }

        public event EventHandler FinishedRendering;
        private void OnFinishedRendering()
        {
            FinishedRendering.Raise(this);
        }

        public event EventHandler StartingRendering;
        private void OnStartingRendering()
        {
            StartingRendering.Raise(this);
        }

        private sealed class RendererContext
        {
            private BackgroundEffectRenderer ber;
            private int nextTileIndex = -1;

            public RendererContext(BackgroundEffectRenderer ber)
            {
                this.ber = ber;
            }

            public void RendererThreadProc(object token)
            {
                if (token == null)
                {
                    RendererLoop(null);
                }
                else
                {
                    RendererLoop((EffectConfigToken)token);
                }
            }

            private void RendererLoop(EffectConfigToken token)
            {
                try
                {
                    while (RenderNextTile(token))
                    {
                    }
                }

                catch (Exception ex)
                {
                    ber.exceptions.Add(ex);
                }
            }

            public bool RenderNextTile(EffectConfigToken token)
            {
                if (ber.threadShouldStop)
                {
                    ber.effect.SignalCancelRequest();
                    ber.aborted = true;
                    return false;
                }

                int maxTileIndex = ber.tileCount - 1;
                int tileIndex = Interlocked.Increment(ref nextTileIndex);

                if (tileIndex > maxTileIndex)
                {
                    return false;
                }

                RenderTile(token, tileIndex);
                return true;
            }

            private void RenderTile(EffectConfigToken token, int tileIndex)
            {
                Rectangle[] subRegion = ber.tileRegions[tileIndex];
                RenderWithClipMask(ber.effect, token, ber.dstArgs, ber.srcArgs, subRegion, ber.clipMaskRenderer);

                PdnRegion subPdnRegion = ber.tilePdnRegions[tileIndex];

                if (!ber.threadShouldStop)
                {
                    ber.OnRenderedTile(new RenderedTileEventArgs(subPdnRegion, ber.tileCount, tileIndex));
                }
            }
        }

        private static unsafe void RenderWithClipMask(
            Effect effect,
            EffectConfigToken token,
            RenderArgs dstArgs,
            RenderArgs srcArgs,
            Rectangle[] rois,
            IRenderer<ColorAlpha8> clipMaskRenderer)
        {
            // Render the effect
            effect.Render(token, dstArgs, srcArgs, rois);

            if (effect.IsCancelRequested)
            {
                return;
            }

            if (clipMaskRenderer != null)
            {
                RectInt32 bounds = RectangleUtil.Bounds(rois).ToRectInt32();

                if (bounds.HasPositiveArea)
                {
                    // dstArgs = (srcArgs * (1 - clipMask)) + (dstArgs * clipMask)
                    // TODO: optimize, or at least refactor into its own method
                    using (ISurface<ColorAlpha8> clipMask = clipMaskRenderer.ToSurface(bounds))
                    {
                        int width = bounds.Width;
                        int height = bounds.Height;
                        int left = bounds.Left;
                        int top = bounds.Top;
                        int bottom = bounds.Bottom;

                        int dstStride = dstArgs.Surface.Stride;
                        int srcStride = srcArgs.Surface.Stride;
                        int clipMaskStride = clipMask.Stride;

                        ColorBgra* dstNextRowPtr = dstArgs.Surface.GetPointPointer(left, top);
                        ColorBgra* srcNextRowPtr = srcArgs.Surface.GetPointPointer(left, top);
                        byte* clipMaskNextRowPtr = (byte*)clipMask.Scan0;

                        int rows = height;
                        while (rows > 0)
                        {
                            Underwrite(srcNextRowPtr, dstNextRowPtr, clipMaskNextRowPtr, width);

                            dstNextRowPtr = (ColorBgra*)((byte*)dstNextRowPtr + dstStride);
                            srcNextRowPtr = (ColorBgra*)((byte*)srcNextRowPtr + srcStride);
                            clipMaskNextRowPtr = clipMaskNextRowPtr + clipMaskStride;
                            --rows;
                        }
                    }
                }
            }
        }

        private static unsafe void Underwrite(ColorBgra* pSrc1, ColorBgra* pDstSrc2, byte* pSrc2A, int length)
        {
            int skipCount = 0;
            while ((length - skipCount) > 0 && (*(pSrc2A + skipCount)) == 255)
            {
                ++skipCount;
            }

            length -= skipCount;
            pSrc1 += skipCount;
            pDstSrc2 += skipCount;
            pSrc2A += skipCount;

            while (length > 0)
            {
                byte src2A = *pSrc2A;
                if (src2A == 255)
                {
                    // pDstSrc2 already equals what it should be
                }
                else if (src2A == 0)
                {
                    *pDstSrc2 = *pSrc1;
                }
                else
                {
                    *pDstSrc2 = ColorBgra.Blend(*pSrc1, *pDstSrc2, src2A);
                }

                --length;
                ++pSrc1;
                ++pDstSrc2;
                ++pSrc2A;
            }
        }

        private void ThreadFunction()
        {
            if (srcArgs.Surface.Scan0.MaySetAllowWrites)
            {
                srcArgs.Surface.Scan0.AllowWrites = false;
            }

            try
            {
                threadInitialized.Set();

                effect.SetRenderInfo(effectTokenCopy, dstArgs, srcArgs);

                RendererContext rc = new RendererContext(this);

                if (threadShouldStop)
                {
                    effect.SignalCancelRequest();
                }
                else if (tileCount > 0)
                {
                    // Render first tile by itself as an attempt at some extra thread safety guarding, and to support better quick-cancellation
                    rc.RenderNextTile(effectTokenCopy);
                }

                int i;
                WaitCallback rcwc = new WaitCallback(rc.RendererThreadProc);

                for (i = 0; i < workerThreads; ++i)
                {
                    if (threadShouldStop)
                    {
                        effect.SignalCancelRequest();
                        break;
                    }

                    EffectConfigToken token;

                    if (effectTokenCopy == null)
                    {
                        token = null;
                    }
                    else
                    {
                        token = effectTokenCopy.CloneT();
                    }

                    this.threadPool.Enqueue(() => rcwc(token));
                }

                threadPool.Join();

                /*
                if (i == this.workerThreads)
                {
                    OnFinishedRendering();
                }
                 * */
            }

            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            finally
            {
                var threadPoolP = threadPool;

                if (!disposed && threadPoolP != null)
                {
                    try
                    {
                        threadPoolP.Join();
                    }

                    catch (Exception)
                    {
                        // discard
                    }
                }

                OnFinishedRendering();

                var srcArgsP = srcArgs;

                if (srcArgsP != null)
                {
                    var surfaceP = srcArgsP.Surface;

                    if (surfaceP != null)
                    {
                        var scan0P = surfaceP.Scan0;

                        if (scan0P != null)
                        {
                            if (!disposed)
                            {
                                if (scan0P.MaySetAllowWrites)
                                {
                                    try
                                    {
                                        scan0P.AllowWrites = true;
                                    }

                                    catch (ObjectDisposedException)
                                    {
                                        // discard
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private volatile bool threadShouldStop = false;
        private Thread thread = null;
        private ManualResetEvent threadInitialized = null;

        public void Start()
        {
            Abort();
            aborted = false;

            if (effectToken != null)
            {
                try
                {
                    effectTokenCopy = (EffectConfigToken)effectToken.Clone();
                }

                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    effectTokenCopy = null;
                }
            }

            threadShouldStop = false;
            OnStartingRendering();
            thread = new Thread(new ThreadStart(ThreadFunction));
            thread.Name = "BackgroundEffectRenderer";
            threadInitialized = new ManualResetEvent(false);
            thread.Start();
            threadInitialized.WaitOne();
            threadInitialized.Close();
            threadInitialized = null;
        }

        public bool Aborted
        {
            get
            {
                return aborted;
            }
        }

        public void Abort()
        {
            if (thread != null)
            {
                threadShouldStop = true;

                if (effect != null)
                {
                    try
                    {
                        effect.SignalCancelRequest();
                    }

                    catch (Exception)
                    {
                    }
                }

                Join();
                threadPool.Join();
            }
        }

        // This is the only method that is safe to call from another thread
        // If the abort was successful, then get_Aborted will return true
        // after a Join().
        public void AbortAsync()
        {
            threadShouldStop = true;

            var effect = this.effect;
            if (effect != null)
            {
                try
                {
                    effect.SignalCancelRequest();
                }

                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Used to determine whether the rendering fully completed or not, and was not
        /// aborted in any way. You can use this method to sleep until the rendering
        /// finishes. Once this is set to the signaled state you should check the IsDone
        /// property to make sure that the rendering was actually finished, and not
        /// aborted.
        /// </summary>
        public void Join()
        {
            thread.Join();
            DrainExceptions();
        }

        private void DrainExceptions()
        {
            if (exceptions.Count > 0)
            {
                Exception throwMe = exceptions[0];
                exceptions.Clear();
                throw new WorkerThreadException("Worker thread threw an exception", throwMe);
            }
        }

        private Rectangle[] ConsolidateRects(Rectangle[] scans)
        {
            if (scans.Length == 0)
            {
                return Array.Empty<Rectangle>();
            }

            SegmentedList<Rectangle> cons = new SegmentedList<Rectangle>();
            int current = 0;
            cons.Add(scans[0]);

            for (int i = 1; i < scans.Length; ++i)
            {
                if (scans[i].Left == cons[current].Left &&
                    scans[i].Right == cons[current].Right &&
                    scans[i].Top == cons[current].Bottom)
                {
                    Rectangle cc = cons[current];
                    cc.Height = scans[i].Bottom - cons[current].Top;
                    cons[current] = cc;
                }
                else
                {
                    cons.Add(scans[i]);
                    current = cons.Count - 1;
                }
            }

            return cons.ToArrayEx();
        }

        private static Scanline[] GetRegionScanlines(Rectangle[] region)
        {
            int scanCount = 0;

            for (int i = 0; i < region.Length; ++i)
            {
                scanCount += region[i].Height;
            }

            if (scanCount == 0)
            {
                return Array.Empty<Scanline>();
            }

            Scanline[] scans = new Scanline[scanCount];
            int scanIndex = 0;

            foreach (Rectangle rect in region)
            {
                for (int y = 0; y < rect.Height; ++y)
                {
                    scans[scanIndex] = new Scanline(rect.X, rect.Y + y, rect.Width);
                    ++scanIndex;
                }
            }

            return scans;
        }

        private static Rectangle[] ScanlinesToRectangles(Scanline[] scans, int startIndex, int length)
        {
            if (length == 0)
            {
                return Array.Empty<Rectangle>();
            }

            Rectangle[] rects = new Rectangle[length];

            for (int i = 0; i < length; ++i)
            {
                Scanline scan = scans[i + startIndex];
                rects[i] = new Rectangle(scan.X, scan.Y, scan.Length, 1);
            }

            return rects;
        }

        private Rectangle[][] SliceUpRegion(PdnRegion region, int sliceCount, Rectangle layerBounds)
        {
            if (sliceCount <= 0)
            {
                throw new ArgumentOutOfRangeException("sliceCount");
            }

            Rectangle[][] slices = new Rectangle[sliceCount][];
            Rectangle[] regionRects = region.GetRegionScansReadOnlyInt();
            Scanline[] regionScans = GetRegionScanlines(regionRects);

            for (int i = 0; i < sliceCount; ++i)
            {
                int beginScan = (regionScans.Length * i) / sliceCount;
                int endScan = Math.Min(regionScans.Length, (regionScans.Length * (i + 1)) / sliceCount);

                // Try to arrange it such that the maximum size of the first region is 1-pixel tall
                if (sliceCount > 1)
                {
                    if (i == 0)
                    {
                        endScan = Math.Min(endScan, beginScan + 1);
                    }
                    else if (i == 1)
                    {
                        beginScan = Math.Min(beginScan, 1);
                    }
                }

                Rectangle[] newRects = ScanlinesToRectangles(regionScans, beginScan, endScan - beginScan);

                for (int j = 0; j < newRects.Length; ++j)
                {
                    newRects[j].Intersect(layerBounds);
                }

                Rectangle[] consRects = ConsolidateRects(newRects);
                slices[i] = consRects;
            }

            return slices;
        }

        public BackgroundEffectRenderer(
            Effect effect,
            EffectConfigToken effectToken,
            RenderArgs dstArgs,
            RenderArgs srcArgs,
            PdnRegion renderRegion,
            IRenderer<ColorAlpha8> clipMaskRenderer, // may be null
            int tileCount,
            int workerThreads)
        {
            this.effect = effect;
            this.effectToken = effectToken;
            this.dstArgs = dstArgs;
            this.srcArgs = srcArgs;

            this.renderRegion = renderRegion;
            this.renderRegion.Intersect(dstArgs.Bounds);

            this.tileCount = tileCount;
            if (effect.Options.RenderingSchedule == EffectRenderingSchedule.None)
            {
                this.tileCount = 1;
            }

            tileRegions = SliceUpRegion(renderRegion, this.tileCount, dstArgs.Bounds);

            tilePdnRegions = new PdnRegion[tileRegions.Length];
            for (int i = 0; i < tileRegions.Length; ++i)
            {
                PdnRegion pdnRegion = PdnRegion.FromRectangles(tileRegions[i]);
                tilePdnRegions[i] = pdnRegion;
            }

            this.workerThreads = workerThreads;
            if (effect.Options.Flags.HasFlag(EffectFlags.SingleThreaded))
            {
                this.workerThreads = 1;
            }

            this.clipMaskRenderer = clipMaskRenderer;

            threadPool = new EffectRendererWorkItemQueue(
                MultithreadedWorkItemDispatcher.Default,
                WorkItemQueuePriority.Normal,
                workerThreads);
        }

        ~BackgroundEffectRenderer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#pragma warning disable 414
        private volatile bool disposed = false;
        private void Dispose(bool disposing)
        {
            disposed = true;

            if (disposing)
            {
                if (srcArgs != null)
                {
                    srcArgs.Dispose();
                    srcArgs = null;
                }

                if (dstArgs != null)
                {
                    dstArgs.Dispose();
                    dstArgs = null;
                }
            }
        }
    }
}
