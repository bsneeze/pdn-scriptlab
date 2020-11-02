using PaintDotNet;
using PaintDotNet.Effects;
using pyrochild.effects.common;
using System;
using System.Drawing;

namespace pyrochild.effects.scriptlab
{
    [PluginSupportInfo(typeof(PluginSupportInfo))]
    public class ScriptLab
        : Effect
    {
        private int renderingThreadCount = Math.Max(2, Environment.ProcessorCount);
        private const int tilesPerCpu = 50;

        private ConfigDialog dialog;
        private bool changed = true;

        public ScriptLab()
            : base(StaticName, StaticIcon, StaticSubMenuName, StaticOptions)
        {
        }

        public static string StaticName
        {
            get
            {
                string s = "ScriptLab";
#if DEBUG
                s += " BETA";
#endif
                return s;
            }
        }

        public static string StaticDialogName
        {
            get { return StaticName + " by pyrochild"; }
        }

        public static Bitmap StaticIcon
        {
            get
            {
                return new Bitmap(typeof(ScriptLab), "images.icon.png");
            }
        }

        public static string StaticSubMenuName
        {
            get
            {
                return "Advanced";
            }
        }

        public static EffectOptions StaticOptions
        {
            get
            {
                return new EffectOptions
                {
                    Flags = EffectFlags.SingleThreaded | EffectFlags.Configurable,
                    RenderingSchedule = EffectRenderingSchedule.None
                };
            }
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            dialog = new ConfigDialog();
            return dialog;
        }

        protected override void OnSetRenderInfo(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            if (dialog != null && dialog.DialogResult == System.Windows.Forms.DialogResult.None)
            {
                changed = true;
                dstArgs.Surface.CopySurface(srcArgs.Surface);

                dialog.ClearProgressBars();
            }
            base.OnSetRenderInfo(parameters, dstArgs, srcArgs);
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            ConfigToken token = (ConfigToken)parameters;
            PdnRegion selection = EnvironmentParameters.GetSelectionAsPdnRegion();

            if (changed)
            {
                changed = false;

                if (dialog != null)
                {
                    dialog.SetProgressBarMaximum(token.effects.Count, tilesPerCpu * renderingThreadCount);
                    dialog.EnableOKButton(false);
                }
                using (Surface scratch = new Surface(srcArgs.Size))
                {
                    scratch.CopySurface(srcArgs.Surface);

                    for (int i = 0; i < token.effects.Count; ++i)
                    {
                        ScriptStep step = token.effects[i];
                        Type type = step.EffectType;

                        if (type == null)
                        {
                            if (dialog != null)
                                dialog.IncrementProgressBarValue(i, 1);
                        }
                        else
                        {
                            Effect effect = (Effect)(type.GetConstructor(Type.EmptyTypes).Invoke(new object[0]));
                            effect.Services = Services;
                            effect.EnvironmentParameters = new EffectEnvironmentParameters(
                                step.PrimaryColor,
                                step.SecondaryColor,
                                EnvironmentParameters.BrushWidth,
                                selection,
                                EnvironmentParameters.SourceSurface);

                            BackgroundEffectRenderer ber = new BackgroundEffectRenderer(effect, step.Token, dstArgs, new RenderArgs(scratch), selection, null, tilesPerCpu * renderingThreadCount, renderingThreadCount);
                            ber.RenderedTile += (sender, e) => RenderedTile((BackgroundEffectRenderer)sender, i, e.TileCount);
                            ber.Start();
                            ber.Join();

                            scratch.CopySurface(dstArgs.Surface);
                        }

                        if (IsCancelRequested)
                            return;
                    }
                    if (dialog != null)
                    {
                        dialog.ClearProgressBars();
                        dialog.EnableOKButton(true);
                    }
                }
            }
        }

        private void RenderedTile(BackgroundEffectRenderer sender, int i, int tileCount)
        {
            if (IsCancelRequested)
            {
                sender.AbortAsync();
            }
            else if (dialog != null)
            {
                dialog.IncrementProgressBarValue(i, tileCount);
            }
        }
    }
}