using System;
using System.Xml.Serialization;

namespace BaldurToolkit.AppRunner.Xml
{
    [XmlType(AnonymousType = true)]
    public class ConfigurationFileElement
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string Path { get; set; }

        [XmlAttribute]
        public bool Optional { get; set; }

        [XmlAttribute]
        public bool ReloadOnChange { get; set; }

        [XmlAttribute]
        public string Environment { get; set; }
    }
}
