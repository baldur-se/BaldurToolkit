using System;
using System.Xml.Serialization;

namespace BaldurToolkit.AppRunner.Xml
{
    [XmlType(AnonymousType = true)]
    public class PathMapElement
    {
        [XmlAttribute]
        public string Name { get; set; } = "default";

        [XmlAttribute]
        public string Prefix { get; set; }

        [XmlAttribute]
        public string Extends { get; set; }

        [XmlElement("Path", IsNullable = false)]
        public PathMapPathElement[] Paths { get; set; }
    }
}
