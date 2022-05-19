using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetPlugin.Models.ComDefModel.Utils
{
    public class CoClassHint
    {
        public string ProgId { get; }
        public string Hint { get; }
        public Guid CLSID { get; }

        public CoClassHint(Guid? clsid)
        {
            CLSID = clsid ?? Guid.Empty;
            ProgId = CLSID.ToString("B");

            Hint = Registry.GetValue($@"HKEY_CLASSES_ROOT\CLSID\{ProgId}", "", null) as string;

            var name = Registry.GetValue($@"HKEY_CLASSES_ROOT\CLSID\{ProgId}\ProgID", "", null) as string;
            ProgId = name ?? ProgId;
        }

        public override string ToString() => $"\"{ProgId}\" {CLSID:B} ({Hint})";
    }
}
