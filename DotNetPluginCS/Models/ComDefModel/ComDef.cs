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
        public FuncDef[] Func { get; set; }

        [XmlElement]
        public InterfaceDef[] Interface { get; set; }

        internal InterfaceDef[] FindAndJoinInterfaces(Guid iid)
        {
            var list = new List<InterfaceDef>();
            var hit = Interface.SingleOrDefault(intf => intf.GetIID() == iid);
            while (hit != null)
            {
                list.Insert(0, hit);

                hit = Interface.SingleOrDefault(intf => intf.Name == hit.Inherit);
            }
            return list.ToArray();
        }
    }
}
