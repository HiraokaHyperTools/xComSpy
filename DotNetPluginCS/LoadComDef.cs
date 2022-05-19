using DotNetPlugin.Models.ComDefModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DotNetPlugin
{
    public class LoadComDef
    {
        internal static ComDef comDef = new ComDef();
        internal static List<ComDef> comDefs = new List<ComDef>();

        private static readonly XmlSerializer xs = new XmlSerializer(typeof(ComDef));

        private readonly IEnumerable<FuncDef> funcList;
        private readonly IEnumerable<InterfaceDef> ifsList;

        public LoadComDef()
        {
            var xmlFiles = new string[] {
                @"H:\Proj\DotNetPluginCS\DotNetPluginCS\ComDef.xml",
                Path.Combine(Path.GetDirectoryName(new Uri(typeof(DotNetPluginCS).Assembly.Location).LocalPath), "ComDef.xml"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ComDef.xml"),
            };

            foreach (var xmlFile in xmlFiles
                .Where(it => File.Exists(it))
                .Take(1)
            )
            {
                comDef = (ComDef)xs.Deserialize(
                    new MemoryStream(
                        File.ReadAllBytes(xmlFile)
                    )
                );
            }

            comDefs.Clear();

            var xmlDirs = new string[] {
                @"H:\Proj\DotNetPluginCS\DotNetPluginCS\bin\x86\Debug\ComDefs",
                Path.Combine(Path.GetDirectoryName(new Uri(typeof(DotNetPluginCS).Assembly.Location).LocalPath), "ComDefs"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ComDefs"),
            };
            foreach (var xmlDir in xmlDirs
                .Where(it => Directory.Exists(it))
                .Take(1)
            )
            {
                foreach (var xmlFile in Directory.GetFiles(xmlDir, "*.xml"))
                {
                    comDefs.Add(
                        (ComDef)xs.Deserialize(
                            new MemoryStream(
                                File.ReadAllBytes(xmlFile)
                            )
                        )
                    );
                }
            }

            funcList = new FuncDef[0]
                .Concat(comDef.Func ?? new FuncDef[0])
                .Concat(
                    comDefs.
                        SelectMany(
                            one => one.Func ?? new FuncDef[0]
                        )
                )
                .ToArray();

            ifsList = new InterfaceDef[0]
                .Concat(comDef.Interface ?? new InterfaceDef[0])
                .Concat(
                    comDefs.
                        SelectMany(
                            one => one.Interface ?? new InterfaceDef[0]
                        )
                )
                .ToArray();

            Console.WriteLine($"# Loaded {funcList.Count():#,##0} funcs, {ifsList.Count():#,##0} ifs.");
        }

        internal IEnumerable<InterfaceDef> FindAndJoinInterfaces(Guid iid)
        {
            var list = new List<InterfaceDef>();
            var hit = ifsList.FirstOrDefault(intf => intf.GetIID() == iid);
            while (hit != null)
            {
                list.Insert(0, hit);

                hit = ifsList.FirstOrDefault(intf => intf.Name == hit.Inherit);
            }
            return list.ToArray();
        }

        internal IEnumerable<FuncDef> GetFuncs()
        {
            return funcList;
        }
    }
}
