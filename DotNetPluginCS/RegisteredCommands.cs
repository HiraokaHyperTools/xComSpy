using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DotNetPlugin.Models.ComDefModel;
using DotNetPlugin.Utils;
using Managed.x64dbg.Script;
using Managed.x64dbg.SDK;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace DotNetPlugin
{
    public static class RegisteredCommands
    {
        private static Watcher watcher;

        public static bool cbNetTestCommand(int argc, string[] argv)
        {
            Console.WriteLine("[.net TEST] .Net test command!");
            string empty = string.Empty;
            string Left = Interaction.InputBox("Enter value pls", "NetTest", "", -1, -1);
            if (Left == null | Operators.CompareString(Left, "", false) == 0)
                Console.WriteLine("[TEST] cancel pressed!");
            else
                Console.WriteLine("[TEST] line: {0}", Left);
            return true;
        }

        public static bool cbDumpProcessCommand(int argc, string[] argv)
        {
            var addr = argc >= 2 ? Bridge.DbgValFromString(argv[1]) : Bridge.DbgValFromString("cip");
            Console.WriteLine("[DotNet TEST] addr: {0}", addr.ToPtrString());
            var modinfo = new Module.ModuleInfo();
            if (!Module.InfoFromAddr(addr, ref modinfo))
            {
                Console.WriteLine("[DotNet TEST] Module.InfoFromAddr failed...");
                return false;
            }
            Console.WriteLine("[DotNet TEST] InfoFromAddr success, base: {0}", modinfo.@base.ToPtrString());
            var hProcess = Bridge.DbgValFromString("$hProcess");
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Executables (*.dll,*.exe)|*.exe|All Files (*.*)|*.*",
                RestoreDirectory = true,
                FileName = modinfo.name
            };
            var result = DialogResult.Cancel;
            var t = new Thread(() => result = saveFileDialog.ShowDialog());
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            if (result == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                if (!TitanEngine.DumpProcess((nint)hProcess, (nint)modinfo.@base, fileName, addr))
                {
                    Console.WriteLine("[DotNet TEST] DumpProcess failed...");
                    return false;
                }
                Console.WriteLine("[DotNet TEST] Dumping done!");
            }
            return true;
        }

        public static bool cbModuleEnum(int argc, string[] argv)
        {
            foreach (var mod in Module.GetList())
            {
                Console.WriteLine("[DotNet TEST] {0} {1}", mod.@base.ToPtrString(), mod.name);
                foreach (var section in Module.SectionListFromAddr(mod.@base))
                    Console.WriteLine("[DotNet TEST]    {0} \"{1}\"", section.addr.ToPtrString(), section.name);
            }
            return true;
        }

        internal static bool cbComSpyInternal(int argc, string[] argv)
        {
            return watcher.ComSpyInternal(argc, argv);
        }

        internal static void RestartWatcher()
        {
            watcher = new Watcher();
        }

        internal static void DLLIsLoaded(string prefix)
        {
            watcher.DLLIsLoaded(prefix);
        }
    }
}
