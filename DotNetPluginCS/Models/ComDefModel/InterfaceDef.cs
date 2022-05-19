using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DotNetPlugin.Models.ComDefModel
{
    public class InterfaceDef
    {
        [XmlAttribute] public string IID { get; set; }
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public string Inherit { get; set; }

        public Guid GetIID() => string.IsNullOrEmpty(IID) ? Guid.Empty : new Guid(IID);

        [XmlElement]
        public MethodDef[] Method { get; set; }
    }
}
