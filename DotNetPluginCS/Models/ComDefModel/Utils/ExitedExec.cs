using DotNetPlugin.Models.ComDefModel.Interfaces;
using DotNetPlugin.Utils;
using Managed.x64dbg.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xComSpy.Models.ComDefModel.Utils;

namespace DotNetPlugin.Models.ComDefModel.Utils
{
    public class ExitedExec
    {
        private static readonly Regex numRef = new Regex("^#(?<idx>\\d+)$");
        private TextWriter log = LogToMyFile.Get();

        public List<OutputInterface> OutputInterfaces { get; } = new List<OutputInterface>();
        public CoClassHint ProgId { get; }

        public ExitedExec(
            int hr,
            IParentDef parent,
            ICallDef call,
            UIntPtr[] parms,
            Func<ValueResolverInput, string> valueResolver = null
        )
        {
            var parmDefs = call.Param;

            if (hr >= 0 && (parmDefs?.Any() ?? false))
            {
                for (int iter = 0; iter < parmDefs.Length; iter++)
                {
                    var parmDef = parmDefs[iter];

                    if (!string.IsNullOrEmpty(parmDef.RefIID))
                    {
                        var match = numRef.Match(parmDef.RefIID);
                        if (match.Success)
                        {
                            int idxRefIID = int.Parse(match.Groups["idx"].Value);

                            var iid = MemHelper.TryReadGuid(parms[idxRefIID]);
                            if (iid.HasValue)
                            {
                                var pv = Bridge.DbgValFromString($"[{FmtHelper.Hex(parms[iter])}]");
                                if (pv != UIntPtr.Zero)
                                {
                                    OutputInterfaces.Add(
                                        new OutputInterface
                                        {
                                            iid = iid.Value,
                                            pv = pv,
                                        }
                                    );
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(parmDef.RefMultiQICount))
                    {
                        var match = numRef.Match(parmDef.RefMultiQICount);
                        if (match.Success)
                        {
                            int idxRefMultiQICount = int.Parse(match.Groups["idx"].Value);

                            var numQueries = (int)parms[idxRefMultiQICount];

                            var qi = parms[iter];

                            for (int x = 0; x < numQueries; x++)
                            {
                                var piid = Bridge.DbgValFromString($"[[{FmtHelper.Hex(qi)}]+.{IntPtr.Size * (3 * x + 0)}]");
                                var pitf = Bridge.DbgValFromString($"[[{FmtHelper.Hex(qi)}]+.{IntPtr.Size * (3 * x + 1)}]");
                                var qhr = Bridge.DbgValFromString($"[[{FmtHelper.Hex(qi)}]+.{IntPtr.Size * (3 * x + 2)}]");

                                if ((int)qhr >= 0 && piid != UIntPtr.Zero)
                                {
                                    var iid = MemHelper.TryReadGuid(piid);

                                    OutputInterfaces.Add(
                                        new OutputInterface
                                        {
                                            iid = iid.Value,
                                            pv = pitf,
                                        }
                                    );
                                }
                            }
                        }
                    }

                    if (parmDef.IsCLSID == "1")
                    {
                        ProgId = new CoClassHint(MemHelper.TryReadGuid(parms[iter]));
                    }
                }
            }

            if (call.Print == "1")
            {
                var simpleArgs = parms
                    .Select(it => FmtHelper.Hex(it));

                var fullCall = (parent != null)
                    ? $"{parent.Name}::{call.Name}"
                    : $"{call.Name}";

                log.WriteLine($"# {fullCall}({string.Join(", ", simpleArgs)})");

                if (valueResolver != null)
                {
                    for (int x = 0; x < parms.Length; x++)
                    {
                        var parmDef = parmDefs[x];
                        var resp = valueResolver(new ValueResolverInput { Parm = parms[x], CType = parmDef.CType, });
                        if (resp != null)
                        {
                            log.WriteLine($"#  {parmDef.Name} = {resp}");
                        }
                    }
                }
            }
        }

        public class OutputInterface
        {
            public Guid iid;
            public UIntPtr pv;
        }
    }
}
