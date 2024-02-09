using System.Text.RegularExpressions;

namespace BackendProject.HomeTools.AFS
{
    public class AFSRegexProcessor
    {
        public static List<MappedList> ScanForSceneListPaths(string sourceFile)
        {
            return ReturnMappedListFromFile(new List<RegexPatterns>()
              {
                    new() {
                        type = ".scene",
                        pattern = "(?<=\\b(?<=config=\"))[^\"]*.scene"
                    }
              }, sourceFile);
        }

        public static List<MappedList> ScanForString(string sourceFileContent)
        {
            List<RegexPatterns> regexPatternsList = new();

            Parallel.ForEach(Regex.Matches(sourceFileContent, "(?<=\\b(?<=source=\"|src=\"|href=\"|file=\"|filename=\"|efx_filename=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*\\.[^\\\"]*").Cast<Match>(), match => {
                string extension = Path.GetExtension(match.Value);
                string pattern = $"(?<=\\b(?<=source=\"|src=\"|href=\"|file=\"|filename=\"|efx_filename=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*{extension}";
                lock (regexPatternsList)
                {
                    // Check if any existing pattern matches pattern
                    if (!regexPatternsList.Any(p => p.type == extension && p.pattern == pattern))
                    {
                        regexPatternsList.Add(new RegexPatterns
                        {
                            type = extension,
                            pattern = pattern
                        });
                    }
                }
            });

            Parallel.ForEach(Regex.Matches(sourceFileContent, "([\\w-\\s]+\\\\)+[\\w-\\s]+\\.[\\w]+").Cast<Match>(), match => {
                string extension = Path.GetExtension(match.Value);
                string pattern = $"([\\w-\\s]+\\\\)+[\\w-\\s]+\\{extension}";
                lock (regexPatternsList)
                {
                    // Check if any existing pattern matches pattern2
                    if (!regexPatternsList.Any(p => p.type == extension && p.pattern == pattern))
                    {
                        regexPatternsList.Add(new RegexPatterns
                        {
                            type = extension,
                            pattern = pattern
                        });
                    }
                }
            });

            return ReturnMappedList(regexPatternsList, sourceFileContent);
        }

        public static List<MappedList> ReturnMappedListFromFile(List<RegexPatterns> regexPatternsList, string sourceFile)
        {
            string input = string.Empty;
            List<MappedList> mappedListList = new();
            using (StreamReader streamReader = File.OpenText(sourceFile))
            {
                input = streamReader.ReadToEnd();
                streamReader.Close();
            }
            // Process 2 patherns at a time, removing the limit is not tolerable as CPU usage goes way too high.
            Parallel.ForEach(regexPatternsList, new ParallelOptions { MaxDegreeOfParallelism = 2 }, regexPatterns =>
            {
                if (!string.IsNullOrEmpty(regexPatterns.pattern))
                {
                    Parallel.ForEach(Regex.Matches(input, regexPatterns.pattern), match => {
                        lock (mappedListList)
                        {
                            if (!mappedListList.Contains(new MappedList()
                            {
                                type = regexPatterns.type,
                                file = match.Value
                            }))
                                mappedListList.Add(new MappedList()
                                {
                                    type = regexPatterns.type,
                                    file = match.Value
                                });
                        }
                    });
                }
            });
            return mappedListList;
        }

        public static List<MappedList> ReturnMappedList(List<RegexPatterns> regexPatternsList, string sourceFileContent)
        {
            List<MappedList> mappedListList = new();
            // Process 2 patherns at a time, removing the limit is not tolerable as CPU usage goes way too high.
            Parallel.ForEach(regexPatternsList, new ParallelOptions { MaxDegreeOfParallelism = 2 }, regexPatterns =>
            {
                if (!string.IsNullOrEmpty(regexPatterns.pattern))
                {
                    Parallel.ForEach(Regex.Matches(sourceFileContent, regexPatterns.pattern), match => {
                        lock (mappedListList)
                        {
                            if (!mappedListList.Contains(new MappedList()
                            {
                                type = regexPatterns.type,
                                file = match.Value
                            }))
                                mappedListList.Add(new MappedList()
                                {
                                    type = regexPatterns.type,
                                    file = match.Value
                                });
                        }
                    });
                }
            });
            return mappedListList;
        }
    }

    public class MappedList
    {
        public string? type;
        public string? file;
    }

    public class RegexPatterns
    {
        public string? type;
        public string? pattern;
    }
}
