using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Managed.x64dbg.SDK
{
    // https://github.com/x64dbg/x64dbg/blob/development/src/bridge/bridgemain.h
    public class Bridge
    {
        public const int GUI_MAX_LINE_SIZE = 65536;
        public const int MAX_LABEL_SIZE = 256;
        public const int MAX_COMMENT_SIZE = 512;
        public const int MAX_MODULE_SIZE = 256;
        public const int MAX_IMPORT_SIZE = 65536;
        public const int MAX_BREAKPOINT_SIZE = 256;
        public const int MAX_CONDITIONAL_EXPR_SIZE = 256;
        public const int MAX_CONDITIONAL_TEXT_SIZE = 256;
        public const int MAX_SCRIPT_LINE_SIZE = 2048;
        public const int MAX_THREAD_NAME_SIZE = 256;
        public const int MAX_WATCH_NAME_SIZE = 256;
        public const int MAX_STRING_SIZE = 512;
        public const int MAX_ERROR_SIZE = 512;
        public const int MAX_SECTION_SIZE = 10;
        public const int MAX_COMMAND_LINE_SIZE = 256;
        public const int MAX_MNEMONIC_SIZE = 64;
        public const int PAGE_SIZE = 4096;

#if AMD64
        private const string dll = "x64bridge.dll";
#else
        private const string dll = "x32bridge.dll";
#endif
        private const CallingConvention cdecl = CallingConvention.Cdecl;

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        private static extern bool GuiGetLineWindow([MarshalAs(UnmanagedType.LPUTF8Str)] string title, IntPtr text);

        public static unsafe bool GuiGetLineWindow([MarshalAs(UnmanagedType.LPUTF8Str)] string title, out string text)
        {
            // alternatively we could implement a custom marshaler (ICustomMarshaler) but that wont't work for ref/out parameters for some reason...
            var textBuffer = Marshal.AllocHGlobal(GUI_MAX_LINE_SIZE);
            try
            {
                var success = GuiGetLineWindow(title, textBuffer);
                text = success ? textBuffer.MarshalToStringUTF8(GUI_MAX_LINE_SIZE) : default;
                return success;
            }
            finally { Marshal.FreeHGlobal(textBuffer); }
        }

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern nuint DbgValFromString([MarshalAs(UnmanagedType.LPUTF8Str)] string @string);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        private static extern bool DbgGetModuleAt(nuint addr, IntPtr text);

        public static unsafe bool DbgGetModuleAt(nuint addr, out string text)
        {
            var textBufferPtr = stackalloc byte[MAX_MODULE_SIZE];
            var success = DbgGetModuleAt(addr, new IntPtr(textBufferPtr));
            text = success ? new IntPtr(textBufferPtr).MarshalToStringUTF8(MAX_MODULE_SIZE) : default;
            return success;
        }

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern nuint DbgModBaseFromName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern bool DbgIsDebugging();

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern bool DbgCmdExec([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern bool DbgCmdExecDirect([MarshalAs(UnmanagedType.LPUTF8Str)] string cmd);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern IntPtr BridgeAlloc(nuint size);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern bool DbgMemRead(UIntPtr va, IntPtr dest, IntPtr size);

        [DllImport(dll, CallingConvention = cdecl, ExactSpelling = true)]
        public static extern void BridgeFree(IntPtr ptr);

        public struct ICONDATA
        {
            public IntPtr data;
            public nuint size;
        }

        // https://github.com/x64dbg/x64dbg/blob/development/src/bridge/bridgelist.h
        public struct ListInfo
        {
            public int count;
            public nuint size;
            public IntPtr data;

            public T[] ToArray<T>(bool success) where T : new()
            {
                if (!success || count == 0 || size == 0)
                    return Array.Empty<T>();
                var list = new T[count];
                var szt = Marshal.SizeOf(typeof(T));
                var sz = checked((int)(size / (nuint)count));
                if (szt != sz)
                    throw new InvalidDataException(string.Format("{0} type size mismatch, expected {1} got {2}!",
                        typeof(T).Name, szt, sz));
                var ptr = data;
                for (var i = 0; i < count; i++)
                {
                    list[i] = (T)Marshal.PtrToStructure(ptr, typeof(T));
                    ptr += sz;
                }
                BridgeFree(data);
                return list;
            }
        }
    }
}
