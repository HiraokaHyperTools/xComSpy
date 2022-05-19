using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using DotNetPlugin.Models.ComDefModel;
using DotNetPlugin.Utils;
using Managed.x64dbg.SDK;
using Microsoft.VisualBasic;
using RGiesecke.DllExport;

namespace DotNetPlugin
{
    public class DotNetPluginCS
    {
        private const int MENU_ABOUT = 0;

        internal static LoadComDef loader = new LoadComDef();

        public static bool PluginInit(Plugins.PLUG_INITSTRUCT initStruct)
        {
            Console.WriteLine("[xComSpy] pluginHandle: {0}", Plugins.pluginHandle);

            if (!Plugins._plugin_registercommand(Plugins.pluginHandle, "ComSpyInternal", RegisteredCommands.cbComSpyInternal, true))
                Console.WriteLine("[xComSpy] error registering the \"ComSpyInternal\" command!");

            Plugins._plugin_registercallback(Plugins.pluginHandle, Plugins.CBTYPE.CB_INITDEBUG, (cbType, info) => CBINITDEBUG(cbType, in info.ToStructUnsafe<Plugins.PLUG_CB_INITDEBUG>()));
            Plugins._plugin_registercallback(Plugins.pluginHandle, Plugins.CBTYPE.CB_SYSTEMBREAKPOINT, (cbType, info) => CBSYSTEMBREAKPOINT(cbType, in info.ToStructUnsafe<Plugins.PLUG_CB_SYSTEMBREAKPOINT>()));
            return true;
        }

        public static void PluginStop()
        {
            Plugins._plugin_unregistercallback(Plugins.pluginHandle, Plugins.CBTYPE.CB_INITDEBUG);
            Plugins._plugin_unregistercallback(Plugins.pluginHandle, Plugins.CBTYPE.CB_SYSTEMBREAKPOINT);
        }

        public static void PluginSetup(Plugins.PLUG_SETUPSTRUCT setupStruct)
        {
            Plugins._plugin_menuaddentry(setupStruct.hMenu, 0, "&About...");
        }

        //[DllExport("CBINITDEBUG", CallingConvention.Cdecl)]
        public static void CBINITDEBUG(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_INITDEBUG info)
        {
            var szFileName = info.szFileName;
            Console.WriteLine("[xComSpy] xComSpy debugging of file {0} started!", szFileName);

            loader = new LoadComDef();
        }

        private static void CBSYSTEMBREAKPOINT(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_SYSTEMBREAKPOINT info)
        {
        }

        [DllExport("CBCREATEPROCESS", CallingConvention.Cdecl)]
        public static void CBCREATEPROCESS(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_CREATEPROCESS info)
        {
            var CreateProcessInfo = info.CreateProcessInfo;
            var modInfo = info.modInfo;
            var DebugFileName = info.DebugFileName;
            var fdProcessInfo = info.fdProcessInfo;
            Console.WriteLine("[xComSpy] Create process {0}", info.DebugFileName);

            Console.WriteLine($"# RestartWatcher");
            RegisteredCommands.RestartWatcher();
        }

        [DllExport("CBLOADDLL", CallingConvention.Cdecl)]
        public static void CBLOADDLL(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_LOADDLL info)
        {
            var LoadDll = info.LoadDll;
            var modInfo = info.modInfo;
            var modname = info.modname;

            if (modname != null && modname.Contains('.'))
            {
                var prefix = Path.GetFileNameWithoutExtension(modname);
                RegisteredCommands.DLLIsLoaded(prefix);
            }
        }

        [DllExport("CBMENUENTRY", CallingConvention.Cdecl)]
        public static void CBMENUENTRY(Plugins.CBTYPE cbType, in Plugins.PLUG_CB_MENUENTRY info)
        {
            switch (info.hEntry)
            {
                case MENU_ABOUT:
                    Interaction.MsgBox("Test DotNet Plugins For x64dbg\nCoded By Ahmadmansoor/exetools\n\nxComSpy\nCoded by kenjiuno", MsgBoxStyle.OkOnly, "Info");
                    break;
            }
        }
    }
}
