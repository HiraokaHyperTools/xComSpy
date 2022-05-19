using Managed.x64dbg.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPlugin.Utils
{
    internal static class BpHelper
    {
        internal static void SetBp(
            string addr,
            string name,
            string breakCond,
            string command,
            string commandCond,
            string bpType,
            bool deleteFirst = true
        )
        {
            if (deleteFirst)
            {
                Bridge.DbgCmdExecDirect($"DeleteBPX {addr}");
            }
            Bridge.DbgCmdExecDirect($"SetBPX {addr},\"{name}\",{bpType}");
            Bridge.DbgCmdExecDirect($"SetBreakpointCondition {addr},\"{breakCond}\"");
            Bridge.DbgCmdExecDirect($"SetBreakpointCommand {addr},\"{command}\"");
            Bridge.DbgCmdExecDirect($"SetBreakpointCommandCondition {addr},\"{commandCond}\"");
        }

        internal static void DeleteBp(
            string addr
        )
        {
            Bridge.DbgCmdExecDirect($"DeleteBPX {addr}");
        }
    }
}
