using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPlugin.Utils
{
    internal class LocalBpHelper
    {
        private readonly List<Installed> installed = new List<Installed>();

        private class Installed
        {
            internal string addr;

            internal string callKey;

            internal int downCounter;

            /// <summary>
            /// -1 to hit for any threads
            /// </summary>
            internal int tid;

            internal Action callback;
        }

        internal void Install(
            string addr,
            string hint,
            int downCounter,
            Action callback,
            int tid = -1
        )
        {
            var callKey = $"_LBP_{addr}";

            BpHelper.SetBp(
                addr: addr,
                name: "",
                breakCond: "0",
                command: $"ComSpyInternal {callKey},call",
                commandCond: "1",
                bpType: "short"
            );

            installed.Add(
                new Installed
                {
                    addr = addr,
                    callKey = callKey,
                    downCounter = downCounter,
                    callback = callback,
                    tid = tid,
                }
            );
        }

        internal void Call(string callKey, int tid)
        {
            var any = 0;
            var all = 0;
            string addr = null;

            for (int x = 0; x < installed.Count; x++)
            {
                var it = installed[x];

                if (it.callKey == callKey)
                {
                    any += 1;

                    if (it.tid == -1 || it.tid == tid)
                    {
                        it.callback();
                        it.downCounter -= 1;

                        if (it.downCounter <= 0)
                        {
                            installed.RemoveAt(x);
                            all += 1;
                            addr = it.addr;
                            x -= 1;
                        }
                    }
                }
            }

            if (all != 0 && any == all)
            {
                BpHelper.DeleteBp(addr);
            }
        }
    }
}
