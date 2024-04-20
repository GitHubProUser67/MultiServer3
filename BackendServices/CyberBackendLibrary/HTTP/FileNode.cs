using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CyberBackendLibrary.HTTP
{
    public class FileNode
    {
        public string? Name { get; set; }
        public string? Link { get; set; }
        public string? Image { get; set; }
        public string? Descriptor { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public long? Size { get; set; }
        public DateTime CreationDate { get; set; }
        public List<FileNode>? Childrens { get; set; }
    }

    public class FileStructure
    {
        public FileNode? Root { get; set; }
    }

    public class FileStructureToJson
    {
        public static string GetFileStructureAsJson(string rootDirectory, string httpdirectoryrequest)
        {
            try
            {
                if (Directory.Exists(rootDirectory))
                    return JsonConvert.SerializeObject(new FileStructure() { Root = CreateFileNode(rootDirectory, httpdirectoryrequest) },
                     Formatting.Indented, new JsonSerializerSettings()
                     {
                         NullValueHandling = NullValueHandling.Ignore
                     });
            }
            catch
            {
                throw;
            }

            return "[]";
        }

        private static bool IsHidden(FileSystemInfo fileSystemInfo)
        {
            return (fileSystemInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        }

        private static FileNode? CreateFileNode(string directoryPath, string httpdirectoryrequest)
        {
            try
            {
                DirectoryInfo directoryInfo = new(directoryPath);
                FileNode fileNode = new()
                {
                    Name = directoryInfo.Name,
                    Type = "Directory",
                    CreationDate = directoryInfo.CreationTimeUtc,
                    Childrens = new List<FileNode>()
                };

                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    if (!IsHidden(file))
                    {
                        string mimetype = HTTPProcessor.GetMimeType(Path.GetExtension(file.FullName));

                        switch (mimetype)
                        {
                            case "video/mp4":
                                string? ImgLink = null;
                                string? DescriptorText = null;

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
                    if (!IsHidden(subdirectory))
                        fileNode.Childrens.Add(new FileNode
                        {
                            Link = httpdirectoryrequest + $"/{subdirectory.Name}/",
                            Name = subdirectory.Name,
                            Type = "Directory",
                            CreationDate = subdirectory.CreationTimeUtc
                        });
                }

                return fileNode;
            }
            catch
            {
                throw;
            }
        }
    }
}
