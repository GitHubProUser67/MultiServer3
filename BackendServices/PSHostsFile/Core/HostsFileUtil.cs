using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PSHostsFile.Core
{
    public class HostsFileUtil
    {
        static public bool IsLineAHostFilesEntry(string line)
        {
            if (line.Trim().Length == 0)
                return false;
            else if (line.TrimStart().StartsWith("#"))
                return false;
            else
                return true;
        }

        static public HostsFileEntry GetHostsFileEntry(string line)
        {
            var result = TryGetHostsFileEntry(line);

            return result == null ? throw new InvalidDataException() : result;
        }

        static public HostsFileEntry? TryGetHostsFileEntry(string line)
        {
            var match = RegexHostsEntry.Match(line);

            if (!match.Success)
                return null;

            return new HostsFileEntry(match.Groups["name"].Value, match.Groups["address"].Value);
        }

        static public Encoding GetEncoding(string file)
        {
            using StreamReader? reader = new(file);
            reader.Peek();
            return reader.CurrentEncoding;
        }

        static Regex RegexHostsEntry = new(@"^\s*(?<address>\S+)\s+(?<name>\S+)\s*($|#)", RegexOptions.Compiled);
    }
}
