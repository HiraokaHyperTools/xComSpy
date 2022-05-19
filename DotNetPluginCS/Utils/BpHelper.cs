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
            string bpType
        )
        {
            Bridge.DbgCmdExec($"DeleteBPX {addr}");
            Bridge.DbgCmdExec($"SetBPX {addr},\"{name}\",{bpType}");
            Bridge.DbgCmdExec($"SetBreakpointCondition {addr},\"{breakCond}\"");
            Bridge.DbgCmdExec($"SetBreakpointCommand {addr},\"{command}\"");
            Bridge.DbgCmdExec($"SetBreakpointCommandCondition {addr},\"{commandCond}\"");
        }
    }
}
