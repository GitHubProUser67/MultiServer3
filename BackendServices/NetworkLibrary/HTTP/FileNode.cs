using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using NetworkLibrary.Extension;
using System.Threading.Tasks;
using NetworkLibrary.Extension.Csharp;

namespace NetworkLibrary.HTTP
{
    public static class FileStructureToJson
    {
        private static int ProcessorCount = Environment.ProcessorCount;
        private static bool IsSingleProcessor => ProcessorCount == 1;

        private static int ProcessorCountLeft = IsSingleProcessor ? ProcessorCount : (int)Math.Floor(ProcessorCount / 2.0);
        private static int ProcessorCountRight = IsSingleProcessor ? ProcessorCount : ProcessorCount - ProcessorCountLeft;

        public static string GetFileStructureAsJson(string rootDirectory, string httpdirectoryrequest, Dictionary<string, string> mimeTypesDic)
        {
            FileNode node = null;

            try
            {
                if (Directory.Exists(rootDirectory))
                {
                    node = CreateFileNodeAsync(rootDirectory, httpdirectoryrequest, mimeTypesDic).Result;

                    return JsonConvert.SerializeObject(new FileStructure() { Root = node },
                     Formatting.Indented, new JsonSerializerSettings()
                     {
                         NullValueHandling = NullValueHandling.Ignore
                     });
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[FileStructureToJson] - GetFileStructureAsJson - Thrown an exception: {ex}");
            }
            finally
            {
                if (node != null)
                    node.Childrens.Clear();
            }

            return "{}";
        }

        private static async Task<FileNode> CreateFileNodeAsync(string directoryPath, string httpdirectoryrequest, Dictionary<string, string> mimeTypesDic)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                FileNode fileNode = new FileNode()
                {
                    Name = directoryInfo.Name,
                    Type = "Directory",
                    CreationDate = directoryInfo.CreationTimeUtc,
                    Childrens = new ConcurrentList<FileNode>()
                };

#if NETCOREAPP || NETSTANDARD1_0_OR_GREATER
                await Task.WhenAll(
                    Parallel.ForEachAsync(directoryInfo.GetFiles(), new ParallelOptions { MaxDegreeOfParallelism = ProcessorCountLeft }, async (file, cancellationToken) =>
                    {
                        if (!file.IsHidden())
                        {
                            string mimetype = HTTPProcessor.GetMimeType(Path.GetExtension(file.FullName), mimeTypesDic);

                            switch (mimetype)
                            {
                                case "video/mp4":
                                    string ImgLink = null;
                                    string DescriptorText = null;

                                    if (File.Exists(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.jpeg"))
                                        ImgLink = httpdirectoryrequest + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.jpeg";
                                    else if (File.Exists(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.jpg"))
                                        ImgLink = httpdirectoryrequest + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.jpg";
                                    else if (File.Exists(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.png"))
                                        ImgLink = httpdirectoryrequest + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.png";

                                    if (File.Exists(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_description.txt"))
                                        DescriptorText = File.ReadAllText(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_description.txt");

                                    fileNode.Childrens.Add(new FileNode
                                    {
                                        Link = httpdirectoryrequest + $"/{file.Name}",
                                        Image = ImgLink,
                                        Descriptor = DescriptorText,
                                        Name = file.Name,
                                        Type = mimetype,
                                        Size = file.Length,
                                        CreationDate = file.CreationTimeUtc
                                    });
                                    break;
                                case "text/plain":
                                case "text/xml":
                                case "application/xml":
                                case "application/json":
                                    fileNode.Childrens.Add(new FileNode
                                    {
                                        Link = httpdirectoryrequest + $"/{file.Name}",
                                        Content = File.ReadAllText(file.FullName),
                                        Name = file.Name,
                                        Type = mimetype,
                                        Size = file.Length,
                                        CreationDate = file.CreationTimeUtc
                                    });
                                    break;
                                default:
                                    fileNode.Childrens.Add(new FileNode
                                    {
                                        Link = httpdirectoryrequest + $"/{file.Name}",
                                        Name = file.Name,
                                        Type = mimetype,
                                        Size = file.Length,
                                        CreationDate = file.CreationTimeUtc
                                    });
                                    break;
                            }
                        }
                    }),
                    Parallel.ForEachAsync(directoryInfo.GetDirectories(), new ParallelOptions { MaxDegreeOfParallelism = ProcessorCountRight }, async (subdirectory, cancellationToken) =>
                    {
                        if (!subdirectory.IsHidden())
                            fileNode.Childrens.Add(new FileNode
                            {
                                Link = httpdirectoryrequest + $"/{subdirectory.Name}/",
                                Name = subdirectory.Name,
                                Type = "Directory",
                                CreationDate = subdirectory.CreationTimeUtc
                            });
                    })
                );
#else
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    if (!file.IsHidden())
                    {
                        string mimetype = HTTPProcessor.GetMimeType(Path.GetExtension(file.FullName), mimeTypesDic);

                        switch (mimetype)
                        {
                            case "video/mp4":
                                string ImgLink = null;
                                string DescriptorText = null;

                                if (File.Exists(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.jpeg"))
                                    ImgLink = httpdirectoryrequest + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.jpeg";
                                else if (File.Exists(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.jpg"))
                                    ImgLink = httpdirectoryrequest + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.jpg";
                                else if (File.Exists(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.png"))
                                    ImgLink = httpdirectoryrequest + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_pic.png";

                                if (File.Exists(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_description.txt"))
                                    DescriptorText = File.ReadAllText(Path.GetDirectoryName(file.FullName) + $"/{Path.GetFileNameWithoutExtension(file.FullName)}_description.txt");

                                fileNode.Childrens.Add(new FileNode
                                {
                                    Link = httpdirectoryrequest + $"/{file.Name}",
                                    Image = ImgLink,
                                    Descriptor = DescriptorText,
                                    Name = file.Name,
                                    Type = mimetype,
                                    Size = file.Length,
                                    CreationDate = file.CreationTimeUtc
                                });
                                break;
                            case "text/plain":
                            case "text/xml":
                            case "application/xml":
                            case "application/json":
                                fileNode.Childrens.Add(new FileNode
                                {
                                    Link = httpdirectoryrequest + $"/{file.Name}",
                                    Content = File.ReadAllText(file.FullName),
                                    Name = file.Name,
                                    Type = mimetype,
                                    Size = file.Length,
                                    CreationDate = file.CreationTimeUtc
                                });
                                break;
                            default:
                                fileNode.Childrens.Add(new FileNode
                                {
                                    Link = httpdirectoryrequest + $"/{file.Name}",
                                    Name = file.Name,
                                    Type = mimetype,
                                    Size = file.Length,
                                    CreationDate = file.CreationTimeUtc
                                });
                                break;
                        }
                    }
                }

                foreach (DirectoryInfo subdirectory in directoryInfo.GetDirectories())
                {
                    if (!subdirectory.IsHidden())
                        fileNode.Childrens.Add(new FileNode
                        {
                            Link = httpdirectoryrequest + $"/{subdirectory.Name}/",
                            Name = subdirectory.Name,
                            Type = "Directory",
                            CreationDate = subdirectory.CreationTimeUtc
                        });
                }
#endif

                return fileNode;
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[FileStructureToJson] - CreateFileNode - Thrown an exception: {ex}");
            }

            return null;
        }
    }

    public class FileNode
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public string Image { get; set; }
        public string Descriptor { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public long? Size { get; set; }
        public DateTime CreationDate { get; set; }
        public ConcurrentList<FileNode> Childrens { get; set; }
    }

    public class FileStructure
    {
        public FileNode Root { get; set; }
    }
}
