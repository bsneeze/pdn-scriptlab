/////////////////////////////////////////////////////////////////////////////////
// paint.net                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, and contributors.                  //
// All Rights Reserved.                                                        //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Diagnostics;
using PaintDotNet.Threading;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace pyrochild.effects.common
{
    internal sealed class EffectRendererWorkItemQueue
        : WorkItemQueue
    {
        private readonly object sync = new object();
        private readonly int maxThreadCount;
        private readonly ConcurrentQueue<Action> queue;
        private int activeThreadCount;
        private long totalEnqueueCount;
        private long totalNotifyCount;
        private ManualResetEvent idleEvent;
        private IDisposable threadCountToken;

        public override int WorkItemCount
        {
            get
            {
                lock (this.sync)
                {
                    return (this.queue == null) ? 0 : Math.Min(this.maxThreadCount, this.queue.Count);
                }
            }
        }

        public EffectRendererWorkItemQueue(
            WorkItemDispatcher dispatcher,
            WorkItemQueuePriority priority,
            int maxThreadCount)
            : base(dispatcher, priority)
        {
            this.maxThreadCount = maxThreadCount;
            this.queue = new ConcurrentQueue<Action>();
            this.idleEvent = new ManualResetEvent(true);

            MultithreadedWorkItemDispatcher asMTWID = dispatcher as MultithreadedWorkItemDispatcher;
            if (asMTWID != null)
            {
                this.threadCountToken = asMTWID.UseThreadCount(maxThreadCount);
            }
        }

        protected override void Dispose(bool disposing)
        {
            DisposableUtil.Free(ref this.threadCountToken, disposing);
            base.Dispose(disposing);
        }

        public void Enqueue(Action workItem)
        {
            Validate.IsNotNull(workItem, nameof(workItem));

            lock (this.sync)
            {
                this.queue.Enqueue(workItem);
                ++this.totalEnqueueCount;
                this.idleEvent.Reset();
            }

            UpdateNotifyWorkItemsQueued();
        }

        public void Join()
        {
            this.idleEvent.WaitOne();
        }

        private void UpdateNotifyWorkItemsQueued()
        {
            int notifyCount;
            lock (this.sync)
            {
                long maxNotifyCount = this.totalEnqueueCount - this.totalNotifyCount;
                int availableThreads = this.maxThreadCount - this.activeThreadCount;
                notifyCount = checked((int)Math.Min(availableThreads, maxNotifyCount));
                this.totalNotifyCount += notifyCount;
                this.activeThreadCount += notifyCount;

                if (this.queue.Count == 0 && this.activeThreadCount == 0)
                {
                    Debug.Assert(this.totalNotifyCount == this.totalEnqueueCount);
                    Debug.Assert(notifyCount == 0);
                    this.idleEvent.Set();
                }
            }

            if (notifyCount > 0)
            {
                NotifyWorkItemsQueued(notifyCount);
            }
        }

        protected override void OnExecuteNextWorkItem()
        {
            Action workItem;
            lock (this.sync)
            {
                if (!this.queue.TryDequeue(out workItem))
                {
                    throw new InternalErrorException();
                }
            }

            try
            {
                workItem();
            }
            catch (Exception ex)
            {
                if (!TryReportException(new WorkItemExceptionInfo<Action>(this, ex, workItem)))
                {
                    throw;
                }
            }
            finally
            {
                lock (this.sync)
                {
                    --this.activeThreadCount;
                }

                UpdateNotifyWorkItemsQueued();
            }
        }
    }
}
