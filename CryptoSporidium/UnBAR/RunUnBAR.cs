using PSMultiServer.CryptoSporidium.BAR;

namespace PSMultiServer.CryptoSporidium.UnBAR
{
    internal class RunUnBAR
    {
        internal static BARArchive archive;

        public static void Run(string inputfile, string outputpath, bool edat)
        {
            if (edat)
                RunDecrypt(inputfile, outputpath);
            else
                RunExtract(inputfile, outputpath);
        }

        private static void RunDecrypt(string filePath, string ourDir)
        {
            new EDAT().decryptFile(filePath, Path.Combine(ourDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"));
            RunExtract(Path.Combine(ourDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"), ourDir);
        }

        private static void RunExtract(string filePath, string outDir)
        {
            Console.WriteLine("Loading BAR/dat: {0}", filePath);
            try
            {
                archive = new BARArchive(filePath, outDir);
                archive.Load();
                foreach (TOCEntry tableOfContent in archive.TableOfContents)
                {
                    MemoryStream memoryStream = new MemoryStream(tableOfContent.GetData(archive.GetHeader().Flags));
                    try
                    {
                        string registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                        ExtractToFile(tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), registeredExtension);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        string fileType = ".unknown";
                        ExtractToFile(tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), fileType);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public static void ExtractToFile(HashedFileName FileName, string outDir, string fileType)
        {
            TOCEntry tableOfContent = archive.TableOfContents[FileName];
            string empty = string.Empty;
            string path;
            if (tableOfContent.Path == null || tableOfContent.Path == string.Empty)
            {
                path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, fileType);
            }
            else
            {
                string str = tableOfContent.Path.Replace('/', Path.DirectorySeparatorChar);
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, str);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            FileStream fileStream = File.Open(path, (FileMode)2);
            byte[] data = tableOfContent.GetData(archive.GetHeader().Flags);
            fileStream.Write(data, 0, data.Length);
            fileStream.Close();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Extracted file {0}", new object[1]
            {
                 Path.GetFileName(path)
            });
            Console.ResetColor();
        }
    }
}
