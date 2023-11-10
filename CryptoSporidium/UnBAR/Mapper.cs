using CustomLogger;
using System.Text.RegularExpressions;

namespace CryptoSporidium.UnBAR
{
    public class Mapper
    {
        public Task MapperStart(string foldertomap, string? helperfolder, string prefix, string bruteforce)
        {
            MapperPrepareFiles(foldertomap);

            try
            {
                if (bruteforce == "on" && !string.IsNullOrEmpty(helperfolder))
                    CopyFiles(helperfolder, foldertomap);

                IEnumerable<string> strings = Directory.EnumerateFiles(foldertomap, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".mdl")
                || s.ToLower().EndsWith(".efx") || s.ToLower().EndsWith(".xml") || s.ToLower().EndsWith(".scene") || s.ToLower().EndsWith(".map")
                || s.ToLower().EndsWith(".lua") || s.ToLower().EndsWith(".luac") || s.ToLower().EndsWith(".unknown"));
                List<MappedList> mappedListList = new List<MappedList>();
                foreach (string sourceFile in strings)
                {
                    mappedListList.AddRange(ScanForString(sourceFile));
                }
                foreach (MappedList mappedList in mappedListList)
                {
                    string text = Regex.Replace(mappedList.file, "file:(\\/+)resource_root\\/build\\/", "", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "file:", "", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "///", "", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "/", "\\", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "reward//ConfederateGeneral_M_Reward.dds", "reward/ConfederateGeneral_M_Reward.dds", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "NATGParticles", "ATGParticles", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "xenvironments", "environments", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "Tenvironments", "environments", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "netinit.svml", "host_svml\\netinit.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "home2.svml", "host_svml\\home2.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "home.svml", "host_svml\\home.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "agecheck.svml", "host_svml\\agecheck.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "ageratingfailed.svml", "host_svml\\ageratingfailed.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "GAMERETURNHOME2.svml", "host_svml\\GAMERETURNHOME2.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "HUBLOGIN.svml", "host_svml\\HUBLOGIN.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "LOGOUT.svml", "host_svml\\LOGOUT.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "nekoonline.svml", "host_svml\\nekoonline.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "onliness.svml", "host_svml\\onliness.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "onlinetext.svml", "host_svml\\onlinetext.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "placeholder.svml", "host_svml\\placeholder.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "popup.svml", "host_svml\\popup.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "unityinit.svml", "host_svml\\unityinit.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "unitymuis.svml", "host_svml\\unitymuis.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "unitytersm.svml", "host_svml\\unitytersm.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "update.svml", "host_svml\\update.svml", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, "nekomuis.svml", "host_svml\\nekomuis.svml", RegexOptions.IgnoreCase);

                    string str = ComputeAFSHash(prefix + text).ToString("X8");

                    foreach (FileInfo file in new DirectoryInfo(foldertomap).GetFiles(str + ".*"))
                    {
                        if (File.Exists(Path.Combine(foldertomap, file.Name)))
                        {
                            new FileInfo(Path.Combine(foldertomap, text).ToUpper()).Directory.Create();
                            if (!File.Exists(Path.Combine(foldertomap, text.ToUpper())))
                            {
                                File.Move(Path.Combine(foldertomap, file.Name), Path.Combine(foldertomap, text.ToUpper()));
#if DEBUG
                                LoggerAccessor.LogInfo($"[Mapper] - Mapped {file.Name} -> {text.ToUpper()}");
#endif
                            }
                        }
                    }

                    if (text.ToLower().EndsWith(".atmos"))
                    {
                        string cdatafromatmos = text.Remove(text.Length - 6) + ".cdata";

                        string str2 = ComputeAFSHash(cdatafromatmos).ToString("X8");

                        foreach (FileInfo file in new DirectoryInfo(foldertomap).GetFiles(str2 + ".*"))
                        {
                            if (File.Exists(Path.Combine(foldertomap, file.Name)))
                            {
                                new FileInfo(Path.Combine(foldertomap, cdatafromatmos).ToUpper()).Directory.Create();
                                if (!File.Exists(Path.Combine(foldertomap, cdatafromatmos.ToUpper())))
                                {
                                    File.Move(Path.Combine(foldertomap, file.Name), Path.Combine(foldertomap, cdatafromatmos.ToUpper()));
#if DEBUG
                                    LoggerAccessor.LogInfo($"[Mapper] - Mapped {file.Name} -> {cdatafromatmos.ToUpper()}");
#endif
                                }
                            }
                        }
                    }
                }

                if (File.Exists(foldertomap + "/4E545585.dds") && !File.Exists(foldertomap + "/PLACEHOLDER_N.DDS"))
                    File.Move(foldertomap + "/4E545585.dds", foldertomap + "/PLACEHOLDER_N.DDS");

                if (File.Exists(foldertomap + "/4EE3523A.dds") && !File.Exists(foldertomap + "/PLACEHOLDER_S.DDS"))
                    File.Move(foldertomap + "/4EE3523A.dds", foldertomap + "/PLACEHOLDER_S.DDS");

                if (File.Exists(foldertomap + "/696E72D6.dds") && !File.Exists(foldertomap + "/HATBUBBLE.DDS"))
                    File.Move(foldertomap + "/696E72D6.dds", foldertomap + "/HATBUBBLE.DDS");

                if (File.Exists(foldertomap + "/D3A7AF9F.xml") && !File.Exists(foldertomap + "/__$manifest$__"))
                    File.Move(foldertomap + "/D3A7AF9F.xml", foldertomap + "/__$manifest$__");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Mapper] - An Error happened in MapperStart - {ex}");
            }

            return Task.CompletedTask;
        }

        private void MapperPrepareFiles(string foldertomap)
        {
            try
            {
                foreach (string myfile in Directory.GetFiles(foldertomap))
                {
                    string newFileName = myfile.Replace("0X", "");

                    // Get the file size
                    long fileSize = new FileInfo(myfile).Length;

                    if (fileSize <= 0)
                    {
                        if (!File.Exists(newFileName + ".CORRUPTED"))
                            File.Move(myfile, newFileName + ".CORRUPTED");
                    }
                    else
                    {
                        if (!Path.HasExtension(myfile))
                        {
                            if (File.ReadLines(myfile).First().Contains("DDS |"))
                            {
                                if (!File.Exists(newFileName + ".dds"))
                                    File.Move(myfile, newFileName + ".dds");
                            }
                            else if (File.ReadLines(myfile).First().Contains("LuaQ"))
                            {
                                if (!File.Exists(newFileName + ".luac"))
                                    File.Move(myfile, newFileName + ".luac");
                            }
                            else if (File.ReadLines(myfile).First().Contains("CHNK"))
                            {
                                if (!File.Exists(newFileName + ".effect"))
                                    File.Move(myfile, newFileName + ".effect");
                            }
                            else if (File.ReadLines(myfile).First().Contains("HM") || File.ReadLines(myfile).First().Contains("MR04"))
                            {
                                if (!File.Exists(newFileName + ".mdl"))
                                    File.Move(myfile, newFileName + ".mdl");
                            }
                            else if (File.ReadLines(myfile).First().Contains("‰PNG") || File.ReadAllText(myfile).Contains("Photoshop ICC profile") || File.ReadAllText(myfile).Contains("IHDR"))
                            {
                                if (!File.Exists(newFileName + ".png"))
                                    File.Move(myfile, newFileName + ".png");
                            }
                            else if (File.ReadLines(myfile).First().Contains("WW") || File.ReadAllText(myfile).Contains("Havok-5.0.0-r1"))
                            {
                                if (!File.Exists(newFileName + ".hkx"))
                                    File.Move(myfile, newFileName + ".hkx");
                            }
                            else if (File.ReadAllText(myfile).Contains("AC11"))
                            {
                                if (!File.Exists(newFileName + ".ani"))
                                    File.Move(myfile, newFileName + ".ani");
                            }
                            else if (File.ReadAllText(myfile).Contains("LoadLibrary") || File.ReadAllText(myfile).Contains("function"))
                            {
                                if (!File.Exists(newFileName + ".lua"))
                                    File.Move(myfile, newFileName + ".lua");
                            }
                            else if (File.ReadAllText(myfile).Contains("SK08"))
                            {
                                if (!File.Exists(newFileName + ".skn"))
                                    File.Move(myfile, newFileName + ".skn");
                            }
                            else if (File.ReadAllText(myfile).Contains("klBS"))
                            {
                                if (!File.Exists(newFileName + ".bnk"))
                                    File.Move(myfile, newFileName + ".bnk");
                            }
                            else if (File.ReadAllText(myfile).Contains("gap:game") || File.ReadAllText(myfile).Contains("</gameObject>"))
                            {
                                if (!File.Exists(newFileName + ".scene"))
                                    File.Move(myfile, newFileName + ".scene");
                            }
                            else if (File.ReadAllText(myfile).Contains("LAME3.") || File.ReadAllText(myfile).Contains("SfMarkers"))
                            {
                                if (!File.Exists(newFileName + ".mp3"))
                                    File.Move(myfile, newFileName + ".mp3");
                            }
                            else if (File.ReadAllText(myfile).Contains("DSIG"))
                            {
                                if (!File.Exists(newFileName + ".ttf"))
                                    File.Move(myfile, newFileName + ".ttf");
                            }
                            else if (File.ReadAllText(myfile).Contains("</") || File.ReadAllText(myfile).Contains("/>"))
                            {
                                if (!File.Exists(newFileName + ".xml"))
                                    File.Move(myfile, newFileName + ".xml");
                            }
                            else
                            {
                                if (!File.Exists(newFileName + ".unknown"))
                                    File.Move(myfile, newFileName + ".unknown");
                            }
                        }
                        else
                        {
                            if (!File.Exists(newFileName))
                                File.Move(myfile, newFileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Mapper] - An Error happened in MapperPrepareFiles - {ex}");
            }
        }

        private static void CopyFiles(string sourceDir, string targetDir)
        {
            // Check if the source directory exists
            if (!Directory.Exists(sourceDir))
                return;

            // Create the target directory if it doesn't exist
            Directory.CreateDirectory(targetDir);

            // Get all files in the source directory and its subdirectories
            string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                try
                {
                    string targetPath = Path.Combine(targetDir, Path.GetRelativePath(sourceDir, file));

                    // Create the directory structure in the target directory if it doesn't exist
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    // Copy the file to the target directory
                    File.Copy(file, targetPath, true); // Use true to overwrite existing files
                }
                catch (Exception)
                {
                    // Not Important.
                }
            }
        }

        private int ComputeAFSHash(string text)
        {
            int hash = 0;
            foreach (char ch in text.ToLower().Replace(Path.DirectorySeparatorChar, '/'))
                hash = hash * 37 + Convert.ToInt32(ch);
            return hash;
        }

        internal static List<MappedList> ScanForString(string sourceFile)
        {
            List<RegexPatterns> regexPatternsList = new List<RegexPatterns>()
              {
                    new RegexPatterns()
                    {
                        type = ".mdl",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.mdl"
                    },
                    new RegexPatterns
                    {
                        type = ".dds",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.dds"
                    },
                    new RegexPatterns
                    {
                        type = ".dds",
                        pattern = "(?<=\\b(?<=href=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.dds"
                    },
                    new RegexPatterns
                    {
                        type = ".dds",
                        pattern = "(?<=\\b(?<=src=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.dds"
                    },
                    new RegexPatterns
                    {
                        type = ".dds",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.DDS"
                    },
                    new RegexPatterns
                    {
                        type = ".dds",
                        pattern = "(?<=\\b(?<=href=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.DDS"
                    },
                    new RegexPatterns
                    {
                        type = ".dds",
                        pattern = "(?<=\\b(?<=src=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.DDS"
                    },
                    new RegexPatterns
                    {
                        type = ".dds",
                        pattern = "([\\w-\\s]+\\\\)+[\\w-\\s]+\\.dds"
                    },
                    new RegexPatterns
                    {
                        type = ".dds",
                        pattern = "([\\w-\\s]+\\\\)+[\\w-\\s]+\\.DDS"
                    },
                    new RegexPatterns
                    {
                        type = "..dds",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*..dds"
                    },
                    new RegexPatterns
                    {
                        type = "..dds",
                        pattern = "(?<=\\b(?<=href=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*..dds"
                    },
                    new RegexPatterns
                    {
                        type = "..dds",
                        pattern = "(?<=\\b(?<=src=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*..dds"
                    },
                    new RegexPatterns
                    {
                        type = "..dds",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*..DDS"
                    },
                    new RegexPatterns
                    {
                        type = "..dds",
                        pattern = "(?<=\\b(?<=href=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*..DDS"
                    },
                    new RegexPatterns
                    {
                        type = "..dds",
                        pattern = "(?<=\\b(?<=src=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*..DDS"
                    },
                    new RegexPatterns
                    {
                        type = "..dds",
                        pattern = "([\\w-\\s]+\\\\)+[\\w-\\s]+\\..dds"
                    },
                    new RegexPatterns
                    {
                        type = "..dds",
                        pattern = "([\\w-\\s]+\\\\)+[\\w-\\s]+\\..DDS"
                    },
                    new RegexPatterns
                    {
                        type = "passphrase",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*passphrase"
                    },
                    new RegexPatterns
                    {
                        type = "passphrase_EU",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*passphrase_EU"
                    },
                    new RegexPatterns
                    {
                        type = ".repertoire_circuit",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.repertoire_circuit"
                    },
                    new RegexPatterns
                    {
                        type = ".lua",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.lua"
                    },
                    new RegexPatterns
                    {
                        type = ".lua",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.LUA"
                    },
                    new RegexPatterns
                    {
                        type = ".lua",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.Lua"
                    },
                    new RegexPatterns
                    {
                        type = ".luac",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.luac"
                    },
                    new RegexPatterns
                    {
                        type = ".json",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.json"
                    },
                    new RegexPatterns
                    {
                        type = ".luac",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.LUAC"
                    },
                    new RegexPatterns
                    {
                        type = ".xml",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.xml"
                    },
                    new RegexPatterns
                    {
                        type = ".xml",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.XML"
                    },
                    new RegexPatterns
                    {
                        type = ".efx",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|efx_filename=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.efx"
                    },
                    new RegexPatterns
                    {
                        type = ".efx",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.effect"
                    },
                    new RegexPatterns
                    {
                        type = ".bnk",
                        pattern = "(?<=\\b(?<=source=\"|filename=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.bnk"
                    },
                    new RegexPatterns
                    {
                        type = ".ttf",
                        pattern = "(?<=\\b(?<=source=\"|filename=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.ttf"
                    },
                    new RegexPatterns
                    {
                        type = ".bank",
                        pattern = "(?<=\\b(?<=source=\"|filename=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.bank"
                    },
                    new RegexPatterns
                    {
                        type = ".hkx",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.hkx"
                    },
                    new RegexPatterns
                    {
                        type = ".probe",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.probe"
                    },
                    new RegexPatterns
                    {
                        type = ".ocean",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.ocean"
                    },
                    new RegexPatterns
                    {
                        type = ".skn",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.skn"
                    },
                    new RegexPatterns
                    {
                        type = ".ani",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.ani"
                    },
                    new RegexPatterns
                    {
                        type = ".mp3",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.mp3"
                    },
                    new RegexPatterns
                    {
                        type = ".atmos",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.atmos"
                    },
                    new RegexPatterns
                    {
                        type = ".png",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.png"
                    },
                    new RegexPatterns
                    {
                        type = ".cer",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.cer"
                    },
                    new RegexPatterns
                    {
                        type = ".der",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.der"
                    },
                    new RegexPatterns
                    {
                        type = ".bin",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.bin"
                    },
                    new RegexPatterns
                    {
                        type = ".raw",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.raw"
                    },
                    new RegexPatterns
                    {
                        type = ".ini",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.ini"
                    },
                    new RegexPatterns
                    {
                        type = ".enemy",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.enemy"
                    },
                    new RegexPatterns
                    {
                        type = ".ui-setup",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.ui-setup"
                    },
                    new RegexPatterns
                    {
                        type = ".cam-def",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.cam-def"
                    },
                    new RegexPatterns
                    {
                        type = ".level-setup",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.level-setup"
                    },
                    new RegexPatterns
                    {
                        type = ".node-def",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.node-def"
                    },
                    new RegexPatterns
                    {
                        type = ".spline-def",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.spline-def"
                    },
                    new RegexPatterns
                    {
                        type = ".psd",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.psd"
                    },
                    new RegexPatterns
                    {
                        type = ".tmx",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.tmx"
                    },
                    new RegexPatterns
                    {
                        type = ".atgi",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.atgi"
                    },
                    new RegexPatterns
                    {
                        type = ".fpo",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.fpo"
                    },
                    new RegexPatterns
                    {
                        type = ".bank",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.bank"
                    },
                    new RegexPatterns
                    {
                        type = ".bnk",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.bnk"
                    },
                    new RegexPatterns
                    {
                        type = ".agf",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.agf"
                    },
                    new RegexPatterns
                    {
                        type = ".avtr",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.avtr"
                    },
                    new RegexPatterns
                    {
                        type = ".vpo",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.vpo"
                    },
                    new RegexPatterns
                    {
                        type = ".vxd",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.vxd"
                    },
                    new RegexPatterns
                    {
                        type = ".jpg",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.jpg"
                    },
                    new RegexPatterns
                    {
                        type = ".mp4",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.mp4"
                    },
                    new RegexPatterns
                    {
                        type = ".sdat",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.sdat"
                    },
                    new RegexPatterns
                    {
                        type = ".dat",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.dat"
                    },
                    new RegexPatterns
                    {
                        type = ".svml",
                        pattern = "(?<=\\b(?<=href=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.svml"
                    },
                    new RegexPatterns
                    {
                        type = ".svml",
                        pattern = "(?<=\\b(?<=src=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.svml"
                    },
                    new RegexPatterns
                    {
                        type = ".sql",
                        pattern = "(?<=\\b(?<=src=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.sql"
                    },
                    new RegexPatterns
                    {
                        type = ".svml",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.svml"
                    },
                    new RegexPatterns
                    {
                        type = ".fp",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.fp"
                    },
                    new RegexPatterns
                    {
                        type = ".sql",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.sql"
                    },
                    new RegexPatterns
                    {
                        type = ".BAR",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.BAR"
                    },
                    new RegexPatterns
                    {
                        type = ".vp",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.vp"
                    },
                    new RegexPatterns
                    {
                        type = ".dat",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.dat"
                    },
                    new RegexPatterns
                    {
                        type = ".sdat",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.sdat"
                    },
                    new RegexPatterns
                    {
                        type = ".bar",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.bar"
                    },
                    new RegexPatterns
                    {
                        type = ".odc",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.odc"
                    },
                    new RegexPatterns
                    {
                        type = ".scene",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.scene"
                    },
                    new RegexPatterns
                    {
                        type = ".scene",
                        pattern = "(?<=\\b(?<=config=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.scene"
                    },
                    new RegexPatterns
                    {
                        type = ".sharc",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.sharc"
                    },
                    new RegexPatterns
                    {
                        type = ".SHARC",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.SHARC"
                    },
                    new RegexPatterns
                    {
                        type = ".map",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.map"
                    },
                    new RegexPatterns
                    {
                        type = ".gui-setup",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.gui-setup"
                    },
                    new RegexPatterns
                    {
                        type = ".oel",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.oel"
                    },
                    new RegexPatterns
                    {
                        type = ".wav",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.wav"
                    },
                    new RegexPatterns
                    {
                        type = ".gui-setup",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.gui-setup"
                    },
                    new RegexPatterns
                    {
                        type = ".sdc",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.sdc"
                    },
                    new RegexPatterns
                    {
                        type = ".sdc",
                        pattern = "(?<=\\b(?<=desc=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.sdc"
                    },
                    new RegexPatterns
                    {
                        type = ".oxml",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.oxml"
                    },
                    new RegexPatterns
                    {
                        type = ".ttf",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.ttf"
                    },
                    new RegexPatterns
                    {
                        type = ".ttf",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.TTF"
                    },
                    new RegexPatterns
                    {
                        type = ".tga",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.tga"
                    },
                    new RegexPatterns
                    {
                        type = ".rig",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.rig"
                    },
                    new RegexPatterns
                    {
                        type = ".jsp",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.jsp"
                    },
                    new RegexPatterns
                    {
                        type = ".fnt",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.fnt"
                    },
                    new RegexPatterns
                    {
                        type = ".shpack",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.shpack"
                    },
                    new RegexPatterns
                    {
                        type = ".php",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.php"
                    },
                    new RegexPatterns
                    {
                        type = ".html",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.html"
                    },
                    new RegexPatterns
                    {
                        type = ".htm",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.htm"
                    },
                    new RegexPatterns
                    {
                        type = ".txt",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.txt"
                    },
                    new RegexPatterns
                    {
                        type = ".md",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.md"
                    },
                    new RegexPatterns
                    {
                        type = ".cs",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.cs"
                    },
                    new RegexPatterns
                    {
                        type = ".exe",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.exe"
                    },
                    new RegexPatterns
                    {
                        type = ".elf",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.elf"
                    },
                    new RegexPatterns
                    {
                        type = ".pdb",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.pdb"
                    },
                    new RegexPatterns
                    {
                        type = ".csproj",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.csproj"
                    },
                    new RegexPatterns
                    {
                        type = ".ico",
                        pattern = "(?<=\\b(?<=source=\"|file=\"|texture\\s=\\s\"|spriteTexture\\s=\\s\"))[^\"]*.ico"
                    }
              };
            string input = string.Empty;
            List<MappedList> mappedListList = new List<MappedList>();
            using (StreamReader streamReader = File.OpenText(sourceFile))
            {
                input = streamReader.ReadToEnd();
                streamReader.Close();
            }
            foreach (RegexPatterns regexPatterns in regexPatternsList)
            {
                if (regexPatterns.pattern != null)
                {
                    foreach (Match match in Regex.Matches(input, regexPatterns.pattern))
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
                }
            }
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
