using System;
using System.Xml.Serialization;

namespace BaldurToolkit.AppRunner.Xml
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "AppInfo")]
    public class AppInfo
    {
        [XmlElement]
        public AppElement App { get; set; }

        [XmlArray("Modules")]
        [XmlArrayItem("Module", IsNullable = false)]
        public ModuleElement[] Modules { get; set; }

        [XmlArray("Startup")]
        [XmlArrayItem("Service", IsNullable = false)]
        public StartupServiceElement[] StartupServices { get; set; }

        [XmlArray("Configuration")]
        [XmlArrayItem("File", IsNullable = false)]
        public ConfigurationFileElement[] ConfigurationFiles { get; set; }

        [XmlArray("FileSystem")]
        [XmlArrayItem("PathMap", IsNullable = false)]
        public PathMapElement[] PathMaps { get; set; }
    }
}
