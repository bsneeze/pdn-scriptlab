using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace pyrochild.effects.common
{
    public static class CommonUtil
    {
        
        public static List<Type> GatherEffects()
        {
            List<Assembly> assemblies = new List<Assembly>();
            List<Type> ec = new List<Type>();

            // PaintDotNet.Effects.dll
            assemblies.Add(Assembly.GetAssembly(typeof(Effect)));

            // TARGETDIR\Effects\*.dll
            string homeDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string effectsDir = Path.Combine(homeDir, "Effects");
            bool dirExists;

            try
            {
                dirExists = Directory.Exists(effectsDir);
            }

            catch
            {
                dirExists = false;
            }

            if (dirExists)
            {
                string fileSpec = "*.dll";
                string[] filePaths = Directory.GetFiles(effectsDir, fileSpec);

                foreach (string filePath in filePaths)
                {
                    Assembly pluginAssembly = null;

                    try
                    {
                        pluginAssembly = Assembly.LoadFrom(filePath);
                        assemblies.Add(pluginAssembly);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            foreach (Assembly a in assemblies)
            {
                try
                {
                    foreach (Type t in a.GetTypes())
                    {
                        if (t.IsSubclassOf(typeof(Effect)) && !t.IsAbstract && !t.IsObsolete(false))
                        {
                            ec.Add(t);
                        }
                    }
                }
                catch { }
            }

            return ec;
        }
    }
}