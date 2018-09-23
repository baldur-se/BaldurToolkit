using System;
using System.Xml.Serialization;

namespace BaldurToolkit.AppRunner.Xml
{
    [XmlType(AnonymousType = true)]
    public class PathMapPathElement
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Path { get; set; }

        [XmlElement("Item", IsNullable = false)]
        public PathMapPathItemElement[] Items { get; set; }
    }
}