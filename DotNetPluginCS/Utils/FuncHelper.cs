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
        /// <summary>
        /// Expose ptr of method args (without this)
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static UIntPtr[] MethodArgs(int n)
        {
            var ptrs = new UIntPtr[n];
            for (int idx = 0; idx < n; idx++)
            {
                ptrs[idx] = Bridge.DbgValFromString($"[esp+.{IntPtr.Size * (2 + idx)}]");
            }
            return ptrs;
        }

        /// <summary>
        /// Expose ptr of func args
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static UIntPtr[] FuncArgs(int n)
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
