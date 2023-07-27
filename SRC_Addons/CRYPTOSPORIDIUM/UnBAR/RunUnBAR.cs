using PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM.BAR;

namespace PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM.UnBAR
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
            Console.WriteLine("Loading BAR/dat: {0}", (object)filePath);
            try
            {
                archive = new BARArchive(filePath, outDir);
                archive.Load();
                foreach (TOCEntry tableOfContent in archive.TableOfContents)
                {
                    MemoryStream memoryStream = new MemoryStream(tableOfContent.GetData(archive.GetHeader().Flags));
                    try
                    {
                        string registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse((Stream)memoryStream));
                        ExtractToFile(tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), registeredExtension);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine((object)ex);
                        string fileType = ".unknown";
                        ExtractToFile(tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), fileType);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine((object)ex);
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
                path = string.Format("{0}{1}{2:X8}{3}", (object)outDir, (object)Path.DirectorySeparatorChar, (object)FileName.Value, (object)fileType);
            }
            else
            {
                string str = tableOfContent.Path.Replace('/', Path.DirectorySeparatorChar);
                path = string.Format("{0}{1}{2}", (object)outDir, (object)Path.DirectorySeparatorChar, (object)str);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            FileStream fileStream = File.Open(path, (FileMode)2);
            byte[] data = tableOfContent.GetData(archive.GetHeader().Flags);
            ((Stream)fileStream).Write(data, 0, data.Length);
            ((Stream)fileStream).Close();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Extracted file {0}", new object[1]
            {
                (object) Path.GetFileName(path)
            });
            Console.ResetColor();
        }
    }
}
