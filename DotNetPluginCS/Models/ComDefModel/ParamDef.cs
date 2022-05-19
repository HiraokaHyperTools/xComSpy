using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DotNetPlugin.Models.ComDefModel
{
    public class ParamDef
    {
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public string CType { get; set; }
        [XmlAttribute] public string RefIID { get; set; }
        [XmlAttribute] public string RefMultiQICount { get; set; }
        [XmlAttribute] public string IsCLSID { get; set; }
    }
}
