using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace PSHostsFile.Core
{
    public class TransformOperation
    {
        public static void TransformFile(string hostsFile, params Func<IEnumerable<string>, IEnumerable<string>>[] transforms)
        {
            Encoding encoding = HostsFileUtil.GetEncoding(hostsFile);
            IEnumerable<string> contents = File.ReadAllLines(hostsFile);

            foreach (var transform in transforms)
            {
                contents = transform(contents);
            }

            File.WriteAllLines(hostsFile, contents.ToArray(), encoding);
        }
    }
}
