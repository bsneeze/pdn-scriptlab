using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace pyrochild.effects.scriptlab
{
    /// <summary>
    /// This is the class we serialize - config tokens are converted to and from this
    /// </summary>
    [Serializable]
    public class ScriptLabScript : ICloneable
    {
        private List<string> alleffects;
        private List<object[]> allproperties;
        private List<object[]> allfields;
        private List<string> allnames;
        private List<Pair<ColorBgra, ColorBgra>> allcolors;

        public ScriptLabScript()
        {
            allproperties = new List<object[]>();
            allfields = new List<object[]>();
            alleffects = new List<string>();
            allnames = new List<string>();
            allcolors = new List<Pair<ColorBgra, ColorBgra>>();
        }

        public ScriptLabScript(ScriptLabScript CopyMe)
        {
            allproperties = new List<object[]>(CopyMe.Properties);
            allfields = new List<object[]>(CopyMe.Fields);
            alleffects = new List<string>(CopyMe.Effects);
            allnames = new List<string>(CopyMe.Names);
            allcolors = new List<Pair<ColorBgra, ColorBgra>>(CopyMe.Colors);
        }

        // Shims for making sure we can load old scripts
        public void ForceCompatibility()
        {
            // Older script version did not save color information for each effect
            if (allcolors == null)
            {
                allcolors = new List<Pair<ColorBgra, ColorBgra>>();
                Pair<ColorBgra, ColorBgra> defcolors = new Pair<ColorBgra, ColorBgra>(ColorBgra.Black, ColorBgra.White);
                for (int i = 0; i < alleffects.Count; i++)
                {
                    allcolors.Add(defcolors);
                }
            }
        }

        public void Add(string effect, string name, object[] properties, object[] fields, Pair<ColorBgra, ColorBgra> colors)
        {
            allproperties.Add(properties);
            allfields.Add(fields);
            allnames.Add(name);
            alleffects.Add(effect);
            allcolors.Add(colors);
        }

        public void Remove(int index)
        {
            allproperties.RemoveAt(index);
            allfields.RemoveAt(index);
            alleffects.RemoveAt(index);
            allnames.RemoveAt(index);
            allcolors.RemoveAt(index);
        }

        public void ChangeToken(int index, object[] properties, object[] fields)
        {
            allproperties[index] = properties;
            allfields[index] = fields;
        }

        public void ChangeColors(int index, Pair<ColorBgra, ColorBgra> colors)
        {
            allcolors[index] = colors;
        }

        public static ScriptLabScript FromToken(ConfigToken token)
        {
            ScriptLabScript sls = new ScriptLabScript();

            foreach (ScriptStep step in token.effects)
            {
                if (step.EffectType == null)
                {
                    sls.Add(step.Name, step.Name, new object[0], new object[0], Pair.Create(step.PrimaryColor, step.SecondaryColor));
                }
                else if (step.Token == null)
                {
                    sls.Add(step.EffectType.FullName + ":" + step.Name, step.Name, null, null, Pair.Create(step.PrimaryColor, step.SecondaryColor));
                }
                else if (step.Token is PropertyBasedEffectConfigToken)
                {
                    PropertyBasedEffectConfigToken pbect = step.Token as PropertyBasedEffectConfigToken;

                    object[] properties = new object[pbect.Properties.Count];
                    IEnumerator<Property> enumerator = pbect.Properties.Properties.GetEnumerator();

                    for (int i = 0; i < pbect.Properties.Count; i++)
                    {
                        enumerator.MoveNext();

                        if (enumerator.Current.Value is FontFamily)
                        {
                            properties[i] = (enumerator.Current.Value as FontFamily).Name;
                        }
                        else if (enumerator.Current.Value.GetType().IsSerializable)
                        {
                            properties[i] = enumerator.Current.Value;
                        }
                    }

                    sls.Add(step.EffectType.FullName + ":" + step.Name, step.Name, properties, null, Pair.Create(step.PrimaryColor, step.SecondaryColor));
                }
                else
                {
                    object[][] propertiesAndFields = MembersToObjectArray(step.Token);

                    sls.Add(step.EffectType.FullName + ":" + step.Name, step.Name, propertiesAndFields[0], propertiesAndFields[1], Pair.Create(step.PrimaryColor, step.SecondaryColor));
                }
            }

            return sls;
        }

        /// <summary>
        /// Converts an object into arrays of its properties and fields so it can be serialized.
        /// </summary>
        /// <param name="val">The object to convert</param>
        /// <returns>object[][]{properties, fields}</returns>
        private static object[][] MembersToObjectArray(object val)
        {
            if (val != null)
            {
                Type type = val.GetType();
                PropertyInfo[] pi = type.GetProperties();
                FieldInfo[] fi = type.GetFields();
                object[] properties = new object[pi.Length];
                object[] fields = new object[fi.Length];

                for (int i = 0; i < pi.Length; i++)
                {
                    properties[i] = ObjectifyProperty(val, pi[i]);
                }
                for (int i = 0; i < fi.Length; i++)
                {
                    fields[i] = ObjectifyField(val, fi[i]);
                }
                return new object[][] { properties, fields };
            }
            return null;
        }

        private static object ObjectifyField(object obj, FieldInfo fieldInfo)
        {
            object val = fieldInfo.GetValue(obj);
            if (val != null)
            {
                Type type = val.GetType();

                if (val is FontFamily)
                {
                    return (val as FontFamily).Name;
                }
                else if (val is List<ScriptStep>)
                {
                    return ScriptLabScript.FromToken((ConfigToken)obj);
                }
                else if (IsSerializable(type))
                {
                    return val;
                }
                else
                {
                    return MembersToObjectArray(val);
                }
            }
            return null;
        }

        private static object ObjectifyProperty(object obj, PropertyInfo propertyInfo)
        {
            object val = propertyInfo.GetValue(obj);
            if (val != null)
            {
                Type type = val.GetType();

                if (val is FontFamily)
                {
                    return (val as FontFamily).Name;
                }
                else if (IsSerializable(type))
                {
                    return val;
                }
                else
                {
                    return MembersToObjectArray(val);
                }
            }
            return null;
        }

        /// <summary>
        /// Type.IsSerializable does not check the Type.BaseType etc. This method checks all base types
        /// </summary>
        /// <returns>true if the type and all base types are Serializable, false otherwise</returns>
        private static bool IsSerializable(Type t)
        {
            if (t.BaseType == null)
            {
                return t.IsSerializable;
            }
            else
            {
                return t.IsSerializable && IsSerializable(t.BaseType);
            }
        }

        public ConfigToken ToToken(Dictionary<string,Type> effects, IServiceProvider services, Surface effectSourceSurface)
        {
            ConfigToken token = new ConfigToken();

            for (int i = 0; i < Effects.Count; i++)
            {
                EffectConfigToken stepToken = null;
                Type type;
                if (effects.TryGetValue(Effects[i], out type))
                {
                    Effect effect = (Effect)(type.GetConstructor(Type.EmptyTypes).Invoke(new object[0]));
                    effect.Services = services;
                    effect.EnvironmentParameters = new EffectEnvironmentParameters(allcolors[i].First, allcolors[i].Second, 2, new PdnRegion(effectSourceSurface.Bounds), effectSourceSurface);
                    if (effect.CheckForEffectFlags(EffectFlags.Configurable))
                    {
                        try
                        {
                            EffectConfigDialog dialog = effect.CreateConfigDialog();
                            stepToken = dialog.EffectToken;
                            if (effect is PropertyBasedEffect)
                            {
                                PropertyBasedEffectConfigToken pbect = stepToken as PropertyBasedEffectConfigToken;
                                IEnumerator<Property> enumerator = pbect.Properties.Properties.GetEnumerator();
                                for (int ii = 0; ii < Properties[i].Length; ii++)
                                {
                                    try
                                    {
                                        enumerator.MoveNext();
                                        enumerator.Current.ReadOnly = false;
                                        if (enumerator.Current.Value is FontFamily)
                                        {
                                            enumerator.Current.Value = new FontFamily((string)Properties[i][ii]);
                                        }
                                        else
                                        {
                                            enumerator.Current.Value = Properties[i][ii];
                                        }
                                    }
                                    catch (ReadOnlyException) { }
                                }
                            }
                            else
                            {
                                    Type t = stepToken.GetType();
                                    SetObjectPropertiesAndFields(stepToken, Properties[i], Fields[i], effects, services, effectSourceSurface);
                            }
                        }
                        catch (Exception) { }
                    }
                    token.effects.Add(new ScriptStep(effect.Name, effect.Image, type, stepToken, Colors[i].First, Colors[i].Second));
                }
                else
                {
                    token.effects.Add(new ScriptStep(Effects[i], null, null, null, Colors[i].First, Colors[i].Second));
                }
            }

            return token;
        }

        private static void SetObjectProperty(object obj, PropertyInfo propertyInfo, object val, Dictionary<string, Type> effects, IServiceProvider services, Surface effectSourceSurface)
        {
            if (val != null)
            {
                Type valueType = val.GetType();
                Type propertyType = propertyInfo.PropertyType;
                object setval;
                if (propertyInfo.CanWrite)
                {
                    if (valueType == typeof(object[][]) && propertyType != typeof(object[][]))
                    {
                        // val was not serializable so we decomposed it - now recompose it
                        object[] properties = ((object[][])val)[0];
                        object[] fields = ((object[][])val)[1];
                        setval = propertyInfo.GetValue(obj);

                        SetObjectPropertiesAndFields(setval, properties, fields, effects, services, effectSourceSurface);
                    }
                    else
                    {
                        setval = val;
                    }
                    propertyInfo.SetValue(obj, setval);
                }
            }
        }

        private static void SetObjectField(object obj, FieldInfo fieldInfo, object val, Dictionary<string, Type> effects, IServiceProvider services, Surface effectSourceSurface)
        {
            if (val != null)
            {
                Type valueType = val.GetType();
                Type fieldType = fieldInfo.FieldType;
                object setval;
                if (val is ScriptLabScript)
                {
                    setval = ((ScriptLabScript)val).ToToken(effects, services, effectSourceSurface).effects;
                }
                else if (valueType == typeof(object[][]) && fieldType != typeof(object[][]))
                {
                    // val was not serializable so we decomposed it - now recompose it
                    object[] properties = ((object[][])val)[0];
                    object[] fields = ((object[][])val)[1];
                    setval = fieldInfo.GetValue(obj);

                    SetObjectPropertiesAndFields(setval, properties, fields, effects, services, effectSourceSurface);
                }
                else
                {
                    setval = val;
                }
                fieldInfo.SetValue(obj, setval);
            }
        }

        private static void SetObjectPropertiesAndFields(object val, object[] properties, object[] fields, Dictionary<string, Type> effects, IServiceProvider services, Surface effectSourceSurface)
        {
            Type type = val.GetType();
            PropertyInfo[] pi = type.GetProperties();
            FieldInfo[] fi = type.GetFields();

            for (int i = 0; i < properties.Length; i++)
            {
                SetObjectProperty(val, pi[i], properties[i], effects, services, effectSourceSurface);
            }
            for (int i = 0; i < fields.Length; i++)
            {
                SetObjectField(val, fi[i], fields[i], effects, services, effectSourceSurface);
            }
        }

        public List<string> Effects
        {
            get
            {
                return alleffects;
            }
        }

        public List<object[]> Properties
        {
            get
            {
                return allproperties;
            }
        }

        public List<object[]> Fields
        {
            get
            {
                return allfields;
            }
        }

        public List<string> Names
        {
            get
            {
                return allnames;
            }
        }

        public List<Pair<ColorBgra, ColorBgra>> Colors
        {
            get
            {
                return allcolors;
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            return new ScriptLabScript(this);
        }

        #endregion
    }
}