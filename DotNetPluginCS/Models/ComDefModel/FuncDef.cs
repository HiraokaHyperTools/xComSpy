using DotNetPlugin.Models.ComDefModel.Interfaces;
using DotNetPlugin.Models.ComDefModel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DotNetPlugin.Models.ComDefModel
{
    public class FuncDef : ICallDef
    {
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public string Setup { get; set; }
        [XmlAttribute] public string Print { get; set; }
        [XmlAttribute] public string Trace { get; set; }

        [XmlElement]
        public ParamDef[] Param { get; set; }
    }
}
