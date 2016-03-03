using PaintDotNet;
using System;
using System.Reflection;

namespace pyrochild.effects
{
    class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author
        {
            get { return ((AssemblyCompanyAttribute)GetType().Assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0]).Company; }
        }

        public string Copyright
        {
            get { return ((AssemblyCopyrightAttribute)GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright; }
        }

        public string DisplayName
        {
            get { return ((AssemblyProductAttribute)GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product; }
        }

        public Version Version
        {
            get { return GetType().Assembly.GetName().Version; }
        }

        public Uri WebsiteUri
        {
            get { return new Uri("http://forums.getpaint.net/index.php?showtopic=7291"); }
        }
    }
}