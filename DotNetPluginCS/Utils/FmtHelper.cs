using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPlugin.Utils
{
    internal static class FmtHelper
    {
        internal static string Hex(UIntPtr ptr)
        {
            if (IntPtr.Size == 4)
            {
                return ((uint)ptr).ToString("X8");
            }
            else
            {
                return ((ulong)ptr).ToString("X16");
            }
        }
    }
}
