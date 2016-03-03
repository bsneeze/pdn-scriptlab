using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace pyrochild.effects.common
{
    public static class FileMan
    {
        public static bool LoadFileBinary(string fileName, out object loadedObject)
        {
            return LoadFileBinary(fileName, out loadedObject, true);
        }
        public static bool LoadFileBinary(string fileName, out object loadedObject, bool compressed)
        {
            return LoadFileBinary(fileName, out loadedObject, true, new serialization.SerializationBinder());
        }
        public static bool LoadFileBinary(string fileName, out object loadedObject, bool compressed, SerializationBinder serializationBinder)
        {
            Stream stream = null;

            try
            {
                if (compressed)
                {
                    stream = new GZipStream(new StreamReader(fileName).BaseStream, CompressionMode.Decompress);
                }
                else
                {
                    stream = new StreamReader(fileName).BaseStream;
                }
                BinaryFormatter bf = new BinaryFormatter();
                bf.Binder = serializationBinder;
                loadedObject = bf.Deserialize(stream);
                return true;
            }
            catch (Exception e)
            {
                loadedObject = e;
                return false;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
        }

        public static bool SaveFileBinary(string fileName, object saveMe, out Exception exception)
        {
            return SaveFileBinary(fileName, saveMe, out exception, true);
        }
        public static bool SaveFileBinary(string fileName, object saveMe, out Exception exception, bool compressed)
        {
            Stream stream = null;
            exception = null;

            try
            {
                if (compressed)
                {
                    stream = new GZipStream(new StreamWriter(fileName).BaseStream, CompressionMode.Compress);
                }
                else
                {
                    stream = new StreamWriter(fileName).BaseStream;
                }
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, saveMe);
                
                return true;
            }
            catch (Exception e)
            {
                if (stream != null && File.Exists(fileName))
                {
                    stream.Close();
                    File.Delete(fileName);
                }
                exception = e;
                return false;
            }
            finally
            {
                if (stream != null) stream.Close();
            }
        }
        public static bool LoadFileXML(string fileName, Type type, out object loadedObject)
        {
            return LoadFileXML(fileName, type, out loadedObject, null);
        }
        public static bool LoadFileXML(string fileName, Type type, out object loadedObject, XmlAttributeOverrides xao)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(fileName);
                XmlSerializer xs = new XmlSerializer(type, xao);
                loadedObject = xs.Deserialize(sr.BaseStream);
                sr.Close();
                return true;
            }
            catch (Exception e)
            {
                loadedObject = e;
                return false;
            }
            finally
            {
                if (sr != null) sr.Close();
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            try
            {
                Assembly assembly = Assembly.Load(e.Name);
                if (assembly != null)
                    return assembly;
            }

            catch { }
            return Assembly.GetExecutingAssembly();
        }

        public static bool SaveFileXML(string fileName, object saveMe)
        {
            return SaveFileXML(fileName, saveMe, null);
        }
        public static bool SaveFileXML(string fileName, object saveMe, XmlAttributeOverrides xao)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(fileName);
                XmlSerializer xs = new XmlSerializer(saveMe.GetType(), xao);
                xs.Serialize(sw.BaseStream, saveMe);
                return true;
            }
            catch (Exception e)
            {
                if (sw != null && File.Exists(fileName))
                {
                    sw.Close();
                    File.Delete(fileName);
                }
                saveMe = e;
                return false;
            }
            finally
            {
                if (sw != null) sw.Close();
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            }
        }
    }
}