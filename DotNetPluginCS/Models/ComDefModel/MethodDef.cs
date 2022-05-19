using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DotNetPlugin.Models.ComDefModel
{
    public class MethodDef
    {
        [XmlAttribute] public string Name { get; set; }

        [XmlElement]
        public ParamDef[] Param { get; set; }

        public OutInterface[] GetOutInterfaces()
        {
            var list = new List<OutInterface>();

            if (Param != null)
            {
                for (int x = 0; x < Param.Length; x++)
                {
                    var match = Regex.Match(Param[x].RefIID ?? "", "#(?<idx>\\d+)");
                    if (match.Success)
                    {
                        list.Add(new OutInterface { PPVParam = x, IIDParam = int.Parse(match.Groups["idx"].Value), });
                    }
                }
            }

            return list.ToArray();
        }

        public class OutInterface
        {
            public int IIDParam { get; set; }
            public int PPVParam { get; set; }
        }
    }
}
