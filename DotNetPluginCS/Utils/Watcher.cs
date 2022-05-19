using DotNetPlugin.Models.ComDefModel;
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
        private Dictionary<string, LocalCall> callDict = new Dictionary<string, LocalCall>();
        private Dictionary<int, PerThread> threads = new Dictionary<int, PerThread>();
        private HashSet<long> hooksInstalled = new HashSet<long>();

        private static readonly Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");

        public Watcher()
        {
            BpHelper.SetBp(
                addr: "CoCreateInstance",
                name: "EnterCoCreateInstance",
                breakCond: "0",
                command: "ComSpyInternal CoCreateInstance,enter",
                commandCond: "1",
                bpType: "short"
            );

            BpHelper.SetBp(
                addr: "CoCreateInstanceEx",
                name: "EnterCoCreateInstanceEx",
                breakCond: "0",
                command: "ComSpyInternal CoCreateInstanceEx,enter",
                commandCond: "1",
                bpType: "short"
            );

            BpHelper.SetBp(
                addr: "CoGetObject",
                name: "EnterCoGetObject",
                breakCond: "0",
                command: "ComSpyInternal CoGetObject,enter",
                commandCond: "1",
                bpType: "short"
            );

            BpHelper.SetBp(
                addr: "CoGetClassObject",
                name: "EnterCoGetClassObject",
                breakCond: "0",
                command: "ComSpyInternal CoGetClassObject,enter",
                commandCond: "1",
                bpType: "short"
            );
        }

        private class PerThread
        {
            private Stack<CoCre> coCre = new Stack<CoCre>();
            private Stack<CoCreEx> coCreEx = new Stack<CoCreEx>();
            private Stack<CoGetO> coGetO = new Stack<CoGetO>();
            private Stack<CoGetC> coGetC = new Stack<CoGetC>();
            private InstallHookToInterfaceDelegate installHookToInterface;

            public PerThread(InstallHookToInterfaceDelegate installHookToInterface)
            {
                this.installHookToInterface = installHookToInterface;
            }

            internal void EnterCoCreateInstance()
            {
                coCre.Push(new CoCre(installHookToInterface));
            }

            internal void LeaveCoCreateInstance()
            {
                if (coCre.Any())
                {
                    coCre.Pop().Leave();
                }
            }

            internal void EnterCoCreateInstanceEx()
            {
                coCreEx.Push(new CoCreEx(installHookToInterface));
            }

            internal void LeaveCoCreateInstanceEx()
            {
                if (coCreEx.Any())
                {
                    coCreEx.Pop().Leave();
                }
            }

            internal void EnterCoGetObject()
            {
                coGetO.Push(new CoGetO(installHookToInterface));
            }

            internal void LeaveCoGetObject()
            {
                if (coGetO.Any())
                {
                    coGetO.Pop().Leave();
                }
            }

            internal void EnterCoGetClassObject()
            {
                coGetC.Push(new CoGetC(installHookToInterface));
            }

            internal void LeaveCoGetClassObject()
            {
                if (coGetC.Any())
                {
                    coGetC.Pop().Leave();
                }
            }
        }

        private class LocalCall
        {
            internal MethodDef methodDef;
            internal UIntPtr[] methodArgs;
        }

        private class CoCre
        {
            private static UIntPtr[] funcArgs;
            private static Guid? iid;
            private readonly InstallHookToInterfaceDelegate InstallHookToInterface;

            public CoCre(InstallHookToInterfaceDelegate InstallHookToInterface)
            {
                this.InstallHookToInterface = InstallHookToInterface;

                var ret = Bridge.DbgValFromString("[esp]");
                funcArgs = FuncHelper.GetArgs(5);
                var clsid = MemHelper.TryReadGuid(funcArgs[0]);
                iid = MemHelper.TryReadGuid(funcArgs[3]);
                Console.WriteLine($"# CoCreateInstance(\"{clsid:B}\", {FmtHelper.Hex(funcArgs[1])}, {FmtHelper.Hex(funcArgs[2])}, \"{iid:B}\", {FmtHelper.Hex(funcArgs[4])})");

                BpHelper.SetBp(
                    addr: FmtHelper.Hex(ret),
                    name: $"LeaveCoCreateInstance_{FmtHelper.Hex(ret)}",
                    breakCond: "0",
                    command: "ComSpyInternal CoCreateInstance,leave",
                    commandCond: "1",
                    bpType: "ss"
                );
            }

            internal void Leave()
            {
                var hr = Bridge.DbgValFromString("eax");
                var punk = Bridge.DbgValFromString($"[{FmtHelper.Hex(funcArgs[4])}]");
                Console.WriteLine($"# CoCreateInstance = hr:{FmtHelper.Hex(hr)} punk:{FmtHelper.Hex(punk)}");

                if ((int)(long)hr >= 0 && iid.HasValue)
                {
                    InstallHookToInterface(punk, iid.Value);
                }
            }
        }

        private class CoGetO
        {
            private static UIntPtr[] funcArgs;
            private static Guid? iid;
            private readonly InstallHookToInterfaceDelegate InstallHookToInterface;

            public CoGetO(InstallHookToInterfaceDelegate InstallHookToInterface)
            {
                this.InstallHookToInterface = InstallHookToInterface;

                var ret = Bridge.DbgValFromString("[esp]");
                funcArgs = FuncHelper.GetArgs(4);
                iid = MemHelper.TryReadGuid(funcArgs[2]);
                Console.WriteLine($"# CoGetObject({FmtHelper.Hex(funcArgs[0])}, {FmtHelper.Hex(funcArgs[1])}, \"{iid:B}\", {FmtHelper.Hex(funcArgs[3])})");

                BpHelper.SetBp(
                    addr: FmtHelper.Hex(ret),
                    name: $"LeaveCoGetObject_{FmtHelper.Hex(ret)}",
                    breakCond: "0",
                    command: "ComSpyInternal CoGetObject,leave",
                    commandCond: "1",
                    bpType: "ss"
                );
            }

            internal void Leave()
            {
                var hr = Bridge.DbgValFromString("eax");
                var punk = Bridge.DbgValFromString($"[{FmtHelper.Hex(funcArgs[3])}]");
                Console.WriteLine($"# CoGetObject = hr:{FmtHelper.Hex(hr)} punk:{FmtHelper.Hex(punk)}");

                if ((int)(long)hr >= 0 && iid.HasValue)
                {
                    InstallHookToInterface(punk, iid.Value);
                }
            }
        }

        private class CoGetC
        {
            private static UIntPtr[] funcArgs;
            private static Guid? iid;
            private readonly InstallHookToInterfaceDelegate InstallHookToInterface;

            public CoGetC(InstallHookToInterfaceDelegate InstallHookToInterface)
            {
                this.InstallHookToInterface = InstallHookToInterface;

                var ret = Bridge.DbgValFromString("[esp]");
                funcArgs = FuncHelper.GetArgs(5);
                var clsid = MemHelper.TryReadGuid(funcArgs[0]);
                iid = MemHelper.TryReadGuid(funcArgs[3]);
                Console.WriteLine($"# CoGetClassObject(\"{clsid:B}\", {FmtHelper.Hex(funcArgs[1])}, {FmtHelper.Hex(funcArgs[2])}, \"{iid:B}\", {FmtHelper.Hex(funcArgs[4])})");

                BpHelper.SetBp(
                    addr: FmtHelper.Hex(ret),
                    name: $"LeaveCoGetClassObject_{FmtHelper.Hex(ret)}",
                    breakCond: "0",
                    command: "ComSpyInternal CoGetClassObject,leave",
                    commandCond: "1",
                    bpType: "ss"
                );
            }

            internal void Leave()
            {
                var hr = Bridge.DbgValFromString("eax");
                var punk = Bridge.DbgValFromString($"[{FmtHelper.Hex(funcArgs[4])}]");
                Console.WriteLine($"# CoGetClassObject = hr:{FmtHelper.Hex(hr)} punk:{FmtHelper.Hex(punk)}");

                if ((int)(long)hr >= 0 && iid.HasValue)
                {
                    InstallHookToInterface(punk, iid.Value);
                }
            }
        }

        private class CoCreEx
        {
            private static UIntPtr[] funcArgs;
            private readonly InstallHookToInterfaceDelegate InstallHookToInterface;

            public CoCreEx(InstallHookToInterfaceDelegate InstallHookToInterface)
            {
                this.InstallHookToInterface = InstallHookToInterface;

                var ret = Bridge.DbgValFromString("[esp]");
                funcArgs = FuncHelper.GetArgs(6);
                var clsid = MemHelper.TryReadGuid(funcArgs[0]);
                Console.WriteLine($"# CoCreateInstanceEx(\"{clsid:B}\", {FmtHelper.Hex(funcArgs[1])}, {FmtHelper.Hex(funcArgs[2])}, {FmtHelper.Hex(funcArgs[3])}, {FmtHelper.Hex(funcArgs[4])}, {FmtHelper.Hex(funcArgs[5])})");

                BpHelper.SetBp(
                    addr: FmtHelper.Hex(ret),
                    name: $"LeaveCoCreateInstanceEx_{FmtHelper.Hex(ret)}",
                    breakCond: "0",
                    command: "ComSpyInternal CoCreateInstanceEx,leave",
                    commandCond: "1",
                    bpType: "ss"
                );
            }

            internal void Leave()
            {
                var hr = Bridge.DbgValFromString("eax");
                Console.WriteLine($"# CoCreateInstanceEx = hr:{FmtHelper.Hex(hr)}");

                if ((int)(long)hr >= 0)
                {
                    var num = (int)funcArgs[4];

                    for (int x = 0; x < num; x++)
                    {
                        var piid = Bridge.DbgValFromString($"[[{FmtHelper.Hex(funcArgs[5])}]+.{IntPtr.Size * (3 * x + 0)}]");
                        var pitf = Bridge.DbgValFromString($"[[{FmtHelper.Hex(funcArgs[5])}]+.{IntPtr.Size * (3 * x + 1)}]");
                        var qhr = Bridge.DbgValFromString($"[[{FmtHelper.Hex(funcArgs[5])}]+.{IntPtr.Size * (3 * x + 2)}]");

                        if (piid != UIntPtr.Zero)
                        {
                            Console.WriteLine($"#  {FmtHelper.Hex(piid)}, {FmtHelper.Hex(pitf)}, {FmtHelper.Hex(qhr)}");

                            var iid = MemHelper.TryReadGuid(piid);
                            InstallHookToInterface(pitf, iid.Value);
                        }
                    }
                }
            }
        }

        internal bool ComSpyInternal(int argc, string[] argv)
        {
            // https://help.x64dbg.com/en/latest/introduction/Input.html

            var tid = (int)Bridge.DbgValFromString("tid()");
            if (!threads.TryGetValue(tid, out PerThread thread))
            {
                threads[tid] = thread = new PerThread(InstallHookToInterface);
            }

            if (argv.Length >= 3)
            {
                if (argv[1] == "CoCreateInstance")
                {
                    if (argv[2] == "enter")
                    {
                        thread.EnterCoCreateInstance();
                    }
                    else if (argv[2] == "leave")
                    {
                        thread.LeaveCoCreateInstance();
                    }
                }
                else if (argv[1] == "CoCreateInstanceEx")
                {
                    if (argv[2] == "enter")
                    {
                        thread.EnterCoCreateInstanceEx();
                    }
                    else if (argv[2] == "leave")
                    {
                        thread.LeaveCoCreateInstanceEx();
                    }
                }
                else if (argv[1] == "CoGetObject")
                {
                    if (argv[2] == "enter")
                    {
                        thread.EnterCoGetObject();
                    }
                    else if (argv[2] == "leave")
                    {
                        thread.LeaveCoGetObject();
                    }
                }
                else if (argv[1] == "CoGetClassObject")
                {
                    if (argv[2] == "enter")
                    {
                        thread.EnterCoGetClassObject();
                    }
                    else if (argv[2] == "leave")
                    {
                        thread.LeaveCoGetClassObject();
                    }
                }
                else
                {
                    var fullName = argv[1];

                    Console.WriteLine($"& {fullName}, {argv[2]}");

                    if (callDict.TryGetValue(fullName, out LocalCall call))
                    {
                        if (argv[2] == "enter")
                        {
                            //Console.WriteLine($"# Set leave bp");

                            var ret = Bridge.DbgValFromString("[esp]");

                            call.methodArgs = FuncHelper.GetArgs(1 + (call.methodDef.Param?.Length ?? 0));

                            BpHelper.SetBp(
                                addr: FmtHelper.Hex(ret),
                                name: "", //$"{fullName}_{FmtHelper.Hex(ret)}_leave",
                                breakCond: "0",
                                command: $"ComSpyInternal {fullName},leave",
                                commandCond: "1",
                                bpType: "ss"
                            );
                        }
                        else if (argv[2] == "leave")
                        {
                            foreach (var outIf in call.methodDef.GetOutInterfaces())
                            {
                                var iid = MemHelper.TryReadGuid(call.methodArgs[1 + outIf.IIDParam]);
                                if (iid.HasValue)
                                {
                                    var ppv = Bridge.DbgValFromString($"[{FmtHelper.Hex(call.methodArgs[1 + outIf.PPVParam])}]");
                                    if (ppv != UIntPtr.Zero)
                                    {
                                        InstallHookToInterface(ppv, iid.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private delegate void InstallHookToInterfaceDelegate(nuint punk, Guid iid);

        private void InstallHookToInterface(nuint punk, Guid iid)
        {
            var ifList = DotNetPluginCS.comDef.FindAndJoinInterfaces(iid);
            if (ifList.Any())
            {
                var lastIf = ifList.Last();
                Console.WriteLine($"# Known: {lastIf.Name}");
            }
            else
            {
                Console.WriteLine($"# Unknown IID!! {iid:B}");

                ifList = DotNetPluginCS.comDef.FindAndJoinInterfaces(IID_IUnknown);
            }

            if (ifList.Any())
            {
                var vtbl = Bridge.DbgValFromString($"[{FmtHelper.Hex(punk)}]");

                var index = 0;

                foreach (var intf in ifList)
                {
                    foreach (var method in intf.Method)
                    {
                        var func = Bridge.DbgValFromString($"[[{FmtHelper.Hex(punk)}]+.{IntPtr.Size * index}]");

                        if (hooksInstalled.Add((long)func))
                        {
                            var fullName = $"_{FmtHelper.Hex(vtbl)}_{intf.Name}_{method.Name}";

                            Console.WriteLine($"# vtbl({FmtHelper.Hex(vtbl)}) {intf.Name}::{method.Name} = {FmtHelper.Hex(func)}");

                            BpHelper.SetBp(
                                addr: FmtHelper.Hex(func),
                                name: fullName,
                                breakCond: "0",
                                command: $"ComSpyInternal {fullName},enter",
                                commandCond: "1",
                                bpType: "short"
                            );

                            callDict[fullName] = new LocalCall { methodDef = method, methodArgs = null, };
                        }

                        index += 1;
                    }
                }
            }
        }
    }
}
