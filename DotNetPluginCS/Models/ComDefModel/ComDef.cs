using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DotNetPlugin.Models.ComDefModel
{
    [XmlRoot()]
    public class ComDef
    {
        [XmlElement]
        public FuncDef[] Func { get; set; } = new FuncDef[0];

        [XmlElement]
        public InterfaceDef[] Interface { get; set; } = new InterfaceDef[0];
    }
}
