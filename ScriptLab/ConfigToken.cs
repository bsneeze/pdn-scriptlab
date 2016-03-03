using PaintDotNet.Effects;
using System.Collections.Generic;

namespace pyrochild.effects.scriptlab
{
    public class ConfigToken : EffectConfigToken
    {
        public List<ScriptStep> effects;

        public ConfigToken()
        {
            effects = new List<ScriptStep>();
        }

        protected ConfigToken(ConfigToken copyMe)
            : base(copyMe)
        {
            effects = copyMe.effects;
        }

        public override object Clone()
        {
            return new ConfigToken(this);
        }
    }
}