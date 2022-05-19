using Managed.x64dbg.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPlugin.Utils
{
    internal static class FuncHelper
    {
        internal static UIntPtr[] GetArgs(int n)
        {
            var ptrs = new UIntPtr[n];
            for (int idx = 0; idx < n; idx++)
            {
                ptrs[idx] = Bridge.DbgValFromString($"[esp+.{IntPtr.Size * (1 + idx)}]");
            }
            return ptrs;
        }


    }
}
