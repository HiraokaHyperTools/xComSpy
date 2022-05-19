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
        /// <summary>
        /// `Microsoft.Jet.OLEDB.4.0`
        /// </summary>
        public string ProgId { get; }

        /// <summary>
        /// `{dee35070-506b-11cf-b1aa-00aa00b8de95}`
        /// </summary>
        public Guid CLSID { get; }

        /// <summary>
        /// `Microsoft.Jet.OLEDB.4.0`
        /// </summary>
        public string Hint { get; }

        public string VersionIndependentProgID { get; }

        public CoClassHint(Guid? clsid)
        {
            CLSID = clsid ?? Guid.Empty;

            Hint = Registry.GetValue($@"HKEY_CLASSES_ROOT\CLSID\{CLSID:B}", "", null) as string;

            if (string.IsNullOrEmpty(Hint))
            {
                Hint = Registry.GetValue($@"HKEY_CLASSES_ROOT\CLSID\{CLSID:B}\InprocServer32", "", null) as string;
            }

            ProgId = Registry.GetValue($@"HKEY_CLASSES_ROOT\CLSID\{CLSID:B}\ProgID", "", null) as string;

            VersionIndependentProgID = Registry.GetValue($@"HKEY_CLASSES_ROOT\CLSID\{CLSID:B}\VersionIndependentProgID", "", null) as string;
        }

        /// <summary>
        /// Text formatter
        /// </summary>
        /// <example>
        /// - `"Scriptlet.Constructor" {06290bd1-48aa-11d2-8432-006008c3fbfc} (Constructor that allows hosts better control creating scriptlets)`
        /// - `"ADODB.Connection.6.0" {00000514-0000-0010-8000-00aa006d2ea4} (ADODB.Connection)`
        /// - `"JRO.JetEngine.2.6" {de88c160-ff2c-11d1-bb6f-00c04fae22da} (JetEngine Class)`
        /// - `"Microsoft.Jet.OLEDB.4.0" {dee35070-506b-11cf-b1aa-00aa00b8de95} (Microsoft.Jet.OLEDB.4.0)`
        /// </example>
        /// <returns></returns>
        public override string ToString() => $"\"{ProgId}\" {CLSID:B} ({Hint})";
    }
}
