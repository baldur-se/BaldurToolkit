using System;
using System.Xml.Serialization;

namespace BaldurToolkit.AppRunner.Xml
{
    [XmlType(AnonymousType = true)]
    public class AppElement
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Type { get; set; }
    }
}
