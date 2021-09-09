using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;

namespace pyrochild.effects.scriptlab
{
    public class ScriptStep
    {
        public readonly bool EffectAvailable;
        public string Name;
        public Image Icon;
        public EffectConfigToken Token;
        public ColorBgra PrimaryColor;
        public ColorBgra SecondaryColor;
        public Func<Effect> CreateInstance;
        public string EffectKey;

        public ScriptStep(IEffectInfo effectInfo, EffectConfigToken token, ColorBgra primary, ColorBgra secondary)
        {
            Name = effectInfo.Name;
            Icon = effectInfo.Image;
            Token = token;
            PrimaryColor = primary;
            SecondaryColor = secondary;
            CreateInstance = () => effectInfo.CreateInstance();
            EffectAvailable = true;
            EffectKey = effectInfo.Type.FullName + ":" + effectInfo.Name;
        }

        public ScriptStep(string notInstalledName, ColorBgra primary, ColorBgra secondary)
        {
            Name = notInstalledName;
            PrimaryColor = primary;
            SecondaryColor = secondary;
            EffectAvailable = false;
        }
    }
}