﻿using Managed.x64dbg.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPlugin.Utils
{
    internal static class MemHelper
    {
        internal static Guid? TryReadGuid(UIntPtr va)
        {
            if (va != UIntPtr.Zero)
            {
                return new Guid(ReadMem(va, 16));
            }
            return null;
        }

        internal static byte[] ReadMem(UIntPtr va, int size)
        {
            var ptr = Marshal.AllocCoTaskMem(size);
            try
            {
                var hProcess = Bridge.DbgValFromString("$hProcess");
                IntPtr num;
                var bytes = new byte[size];
                if (TitanEngine.MemoryReadSafe(hProcess, va, ptr, new IntPtr(size), out num))
                {
                    Marshal.Copy(ptr, bytes, 0, size);
                }
                return bytes;
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }
    }
}
