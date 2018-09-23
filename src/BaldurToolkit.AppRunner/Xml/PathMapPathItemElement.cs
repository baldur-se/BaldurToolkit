using System;
using System.Xml.Serialization;

namespace BaldurToolkit.AppRunner.Xml
{
    [XmlType(AnonymousType = true)]
    public class PathMapPathItemElement
    {
        [XmlAttribute]
        public string Path { get; set; }
    }
}
