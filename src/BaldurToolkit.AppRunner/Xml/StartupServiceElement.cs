using System;
using System.Xml.Serialization;

namespace BaldurToolkit.AppRunner.Xml
{
    [XmlType(AnonymousType = true)]
    public class StartupServiceElement
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string Environment { get; set; }
    }
}
