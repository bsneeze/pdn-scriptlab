using System;

namespace pyrochild.effects.common.serialization
{
    class SerializationBinder : System.Runtime.Serialization.SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type t = null;

            t = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            if (t == null)
            {
                t = typeof(object);
            }
            return t;
        }
    }
}