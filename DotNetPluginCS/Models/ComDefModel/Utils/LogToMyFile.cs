using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xComSpy.Models.ComDefModel.Utils
{
    public class LogToMyFile
    {
        private static Lazy<string> filePath = new Lazy<string>(
            () =>
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"xComSpy_{DateTime.Now:yyyyMMddHHmmss}.txt"
                );
            }
        );

        internal static TextWriter Get()
        {
            var writer = new StreamWriter(
                File.Open(
                    filePath.Value,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite
                )
            );
            writer.AutoFlush = true;
            return writer;
        }
    }
}
