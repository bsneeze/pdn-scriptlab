using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;

namespace pyrochild.effects.scriptlab
{
    public class ScriptStep
    {
        public string Name;
        public Image Icon;
        public Type EffectType;
        public EffectConfigToken Token;
        public ColorBgra PrimaryColor;
        public ColorBgra SecondaryColor;

        public ScriptStep(string name, Image icon, Type type, EffectConfigToken token, ColorBgra primary, ColorBgra secondary)
        {
            Name = name;
            Icon = icon;
            EffectType = type;
            Token = token;
            PrimaryColor = primary;
            SecondaryColor = secondary;
        }
    }
}