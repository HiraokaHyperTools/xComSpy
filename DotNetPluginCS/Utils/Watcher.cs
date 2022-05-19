using DotNetPlugin.Models.ComDefModel;
using DotNetPlugin.Models.ComDefModel.Utils;
using Managed.x64dbg.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPlugin.Utils
{
    internal class Watcher
    {
        private HashSet<long> hooksInstalled = new HashSet<long>();
        private LocalBpHelper bp = new LocalBpHelper();

        private static readonly Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");

        public Watcher()
        {
        }

        internal bool ComSpyInternal(int argc, string[] argv)
        {
            // https://help.x64dbg.com/en/latest/introduction/Input.html

            var tid = (int)Bridge.DbgValFromString("tid()");

            //Console.WriteLine($"& {argv[0]}");

            if (argv.Length >= 3)
            {
                if (argv[2] == "call")
                {
                    bp.Call(argv[1], tid);
                }
            }

            return true;
        }

        private void InstallHookToInterface(nuint punk, Guid iid, CoClassHint progId)
        {
            var ifsList = DotNetPluginCS.loader.FindAndJoinInterfaces(iid);
            if (ifsList.Any())
            {
                var lastIf = ifsList.Last();
                Console.WriteLine($"# Known: {lastIf.Name}");
            }
            else
            {
                Console.WriteLine($"# Unknown IID!! {iid:B}");

                ifsList = DotNetPluginCS.loader.FindAndJoinInterfaces(IID_IUnknown);
            }

            if (ifsList.Any())
            {
                var vtbl = Bridge.DbgValFromString($"[{FmtHelper.Hex(punk)}]");

                var vfuncIdx = 0;

                foreach (var _ifs in ifsList)
                {
                    var ifs = _ifs;

                    foreach (var _method in ifs.Method)
                    {
                        var method = _method;

                        var funcAddr = Bridge.DbgValFromString($"[[{FmtHelper.Hex(punk)}]+.{IntPtr.Size * vfuncIdx}]");

                        if (hooksInstalled.Add((long)funcAddr))
                        {
                            var fullName = $"_{FmtHelper.Hex(vtbl)}_{ifs.Name}_{method.Name}";

                            //Bridge.DbgCmdExec($"labelset {FmtHelper.Hex(funcAddr)},{ifs.Name}::{method.Name}");
                            Console.WriteLine($"@ {FmtHelper.Hex(funcAddr)} : {ifs.Name}::{method.Name} // {progId}");

                            bp.Install(
                                addr: FmtHelper.Hex(funcAddr),
                                hint: fullName,
                                downCounter: int.MaxValue,
                                () =>
                                {
                                    // on beginning of API
                                    var tid = (int)Bridge.DbgValFromString("tid()");

                                    var retAddr = Bridge.DbgValFromString("[esp]");
                                    var methodArgs = FuncHelper.MethodArgs(method.Param?.Length ?? 0);

                                    bp.Install(
                                        tid: tid,
                                        addr: FmtHelper.Hex(retAddr),
                                        hint: $"After{fullName}",
                                        downCounter: 1,
                                        callback: () =>
                                        {
                                            // after exit of API
                                            var hr = Bridge.DbgValFromString("eax");

                                            var exited = new ExitedExec(
                                                (int)hr,
                                                ifs,
                                                method,
                                                methodArgs,
                                                ValueResolver
                                            );

                                            Process(exited, exited.ProgId ?? progId);
                                        }
                                    );
                                }
                            );
                        }

                        vfuncIdx += 1;
                    }
                }
            }
        }

        internal void DLLIsLoaded(string prefix)
        {
            foreach (var _func in (DotNetPluginCS.loader.GetFuncs())
                .Where(it => true
                    && it.Setup == "1"
                    && it.Name.StartsWith(prefix + ".")
                )
            )
            {
                var func = _func;
                bp.Install(
                    addr: func.Name,
                    hint: $"Begin{func.Name}",
                    downCounter: int.MaxValue,
                    callback: () =>
                    {
                        // on beginning of API
                        var tid = (int)Bridge.DbgValFromString("tid()");

                        var retAddr = Bridge.DbgValFromString("[esp]");
                        var funcArgs = FuncHelper.FuncArgs(func.Param?.Length ?? 0);

                        bp.Install(
                            tid: tid,
                            addr: FmtHelper.Hex(retAddr),
                            hint: $"After{func.Name}",
                            downCounter: 1,
                            callback: () =>
                            {
                                // after exit of API
                                var hr = Bridge.DbgValFromString("eax");

                                var exited = new ExitedExec(
                                    (int)hr,
                                    null,
                                    func,
                                    funcArgs,
                                    ValueResolver
                                );

                                Process(exited, exited.ProgId);
                            }
                        );
                    }
                );
            }
        }

        private string ValueResolver(ValueResolverInput arg)
        {
            if (arg.CType == "BSTR")
            {
                return MemHelper.ReadUTF16From(arg.Parm);
            }
            else if (arg.CType == "BSTR*")
            {
                var va = Bridge.DbgValFromString($"[{FmtHelper.Hex(arg.Parm)}]");
                return MemHelper.ReadUTF16From(va);
            }
            return null;
        }

        private void Process(ExitedExec exited, CoClassHint progId)
        {
            foreach (var outIfs in exited.OutputInterfaces)
            {
                InstallHookToInterface(outIfs.pv, outIfs.iid, progId);
            }
        }
    }
}
