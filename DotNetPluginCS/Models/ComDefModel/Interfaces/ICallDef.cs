using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPlugin.Models.ComDefModel.Interfaces
{
    public interface ICallDef
    {
        string Name { get; }
        string Print { get; }
        ParamDef[] Param { get; }
    }
}
