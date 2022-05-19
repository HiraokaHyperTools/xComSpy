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
