using System;
using System.Xml.Serialization;

namespace BaldurToolkit.AppRunner.Xml
{
    [XmlType(AnonymousType = true)]
    public class ModuleElement
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string Environment { get; set; }
    }
}
