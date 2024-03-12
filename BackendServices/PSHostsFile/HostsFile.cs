using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PSHostsFile.Core;

namespace PSHostsFile
{
    public class HostsFile
    {
        public static IEnumerable<HostsFileEntry> Get(string? filepath = null)
        {
            filepath = filepath ?? GetHostsPath();
            
            var lines = File.ReadAllLines(filepath);

            return lines
                .Where(l => HostsFileUtil.IsLineAHostFilesEntry(l))
                .Select(l => HostsFileUtil.GetHostsFileEntry(l));
        }

        public static void Set(string hostname, string address, string? filepath = null)
        {
            Set(new[] { new HostsFileEntry(hostname, address), }, filepath);
        }

        public static void Set(IEnumerable<HostsFileEntry>? entries, string? filepath = null)
        {
            if (entries == null)
                return;

            filepath ??= GetHostsPath();

            List<Func<IEnumerable<string>, IEnumerable<string>>> transforms = new();
            
            foreach (HostsFileEntry? entry in entries.Reverse())
            {
                string hostName = entry.Hostname;
                transforms.Add(Core.Remove.GetRemoveTransformForHost(hostName));
                transforms.Add(lines => GetSetHostTransform(lines.ToArray(), hostName, entry.Address));
            }

            TransformOperation.TransformFile(filepath, transforms.ToArray());
        }

        public static void Remove(string hostName)
        {
            new Remove().RemoveFromFile(hostName, GetHostsPath());
        }

        public static void Remove(Regex pattern)
        {
            new Remove().RemoveFromFile(pattern, GetHostsPath());
        }

        public static string GetHostsPath()
        {
            string? systemPath = Environment.GetEnvironmentVariable("SystemRoot");

            if (string.IsNullOrEmpty(systemPath))
                throw new FileNotFoundException("System Root not found.");

            string hostsPath = Path.Combine(systemPath, "system32\\drivers\\etc\\hosts");

            if (!File.Exists(hostsPath))
                throw new FileNotFoundException("Hosts file not found at expected location.");
            return hostsPath;
        }

        public static IEnumerable<string> GetSetHostTransform(IEnumerable<string> contents, string hostName, string address)
        {
            List<string> result = new();

            bool needsInsert = true;

            foreach (string line in contents)
            {
                if (!HostsFileUtil.IsLineAHostFilesEntry(line))
                {
                    result.Add(line);
                    continue;
                }
 
                if (needsInsert)
                {
                    result.Add(GetHostLine(hostName, address));
                    needsInsert = false;
                }
                result.Add(line);
            }

            if (needsInsert)
            {
                result.Add(GetHostLine(hostName, address));
                needsInsert = false;
            }

            return result;
        }

        public static string GetHostLine(string hostName, string address)
        {
            return address + "\t\t" + hostName;
        }
    }
}
