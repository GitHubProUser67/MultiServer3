using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace MultiServer.CryptoSporidium.BAR
{

    public class BARArchive
    {
        private Stream GetDataWriterStream(string fileName)
        {
            if (m_outputStream != null)
                return m_outputStream;
            return File.Open(fileName, FileMode.Create);
        }

        private Stream GetDataReadStream()
        {
            if (m_sourceStream != null)
                return m_sourceStream;
            return File.OpenRead(m_sourceFile);
        }

        public Header GetHeader()
        {
            return m_header;
        }

        public event BAREventDelegate OnLoadComplete;

        public event BARMessageDelegate OnMessage;

        public event BARLoadProgressEventDelegate OnLoadProgress;

        public event BARLoadHeaderEventDelegate OnLoadHeader;

        public event BAREventDelegate OnExtractComplete;

        public event BAREventDelegate OnRecompressComplete;

        public BARArchive()
        {
            m_toc = new TOC(this);
            m_deletedFileSection = new Dictionary<HashedFileName, TOCEntry>();
            m_sourceFile = string.Empty;
            m_endian = EndianType.LittleEndian;
            m_allowWhitespaceInFilenames = true;
        }

        public BARArchive(string sourceFilePath, string resourceRoot) : this()
        {
            m_sourceFile = sourceFilePath;
            m_resourceRoot = resourceRoot;
        }

        public bool Dirty
        {
            get
            {
                return m_dirty;
            }
            set
            {
                m_dirty = value;
            }
        }

        public bool AllowWhitespaceInFilenames
        {
            get
            {
                return m_allowWhitespaceInFilenames;
            }
            set
            {
                m_allowWhitespaceInFilenames = value;
            }
        }

        public string FileName
        {
            get
            {
                return m_sourceFile;
            }
            set
            {
                m_sourceFile = value;
            }
        }

        public Header BARHeader
        {
            get
            {
                return m_header;
            }
        }

        public TOC TableOfContents
        {
            get
            {
                return m_toc;
            }
        }

        public string ResourceRoot
        {
            get
            {
                return m_resourceRoot;
            }
            set
            {
                if (value.EndsWith("\\") || value.EndsWith("/"))
                {
                    m_resourceRoot = Path.GetDirectoryName(value);
                    return;
                }
                m_resourceRoot = value;
            }
        }

        public EndianType Endian
        {
            get
            {
                return m_endian;
            }
            set
            {
                m_endian = value;
            }
        }

        public CompressionMethod DefaultCompression
        {
            get
            {
                return m_defaultCompression;
            }
            set
            {
                m_defaultCompression = value;
            }
        }

        private static bool ShouldCompress(string filePath, BARAddFileOptions options)
        {
            bool result = true;
            if (options == BARAddFileOptions.ForceCompress)
                result = true;
            else if (options == BARAddFileOptions.ForceUncompress)
                result = false;
            else if (options == BARAddFileOptions.Default)
            {
                string a = Path.GetExtension(filePath).ToLower();
                if (a == ".mp3" || a == ".mp4" || a == ".bar")
                    result = false;
            }
            return result;
        }

        private string GetInBARPath(string filePath)
        {
            string text = filePath.Replace(Path.DirectorySeparatorChar, '/');
            return text.Substring(m_resourceRoot.Length + 1);
        }

        private void ProgressMessage(string msgString, params object[] msg)
        {
            string outputMsg = string.Format(msgString, msg);
            SendOrPostCallback d = delegate (object sender)
            {
                if (OnLoadProgress != null)
                    OnLoadProgress(this, new BARLoadProgressEventArgs(m_sourceFile, outputMsg));
            };
            if (m_asyncOp != null)
                m_asyncOp.Post(d, this);
        }

        private void GeneralMessage(string msgString, params object[] msg)
        {
            string outputMsg = string.Format(msgString, msg);
            SendOrPostCallback d = delegate (object sender)
            {
                if (OnMessage != null)
                    OnMessage(this, new BARMessageEventArgs(m_sourceFile, outputMsg));
            };
            if (m_asyncOp != null)
                m_asyncOp.Post(d, this);
        }

        private void RunDeleteOperation(HashedFileName fileName, BARFileOperationFlags flags)
        {
            TOCEntry value = m_toc[fileName];
            if ((flags & BARFileOperationFlags.Delete) == BARFileOperationFlags.Delete && !m_deletedFileSection.ContainsKey(fileName))
                m_deletedFileSection[fileName] = value;
            m_toc.Remove(fileName);
            Dirty = true;
            m_toc.ResortOffsets();
        }

        private bool LoadHeaderandTOC()
        {
            bool ok = false;
            Stream dataReadStream = GetDataReadStream();
            try
            {
                m_endian = GetEndianness(dataReadStream);
                if (ReadHeader(dataReadStream))
                {
                    uint num = 20U;
                    bool flag = (ushort)(m_header.Flags & ArchiveFlags.Bar_Flag_ZTOC) == 1;
                    if (flag)
                    {
                        EndianAwareBinaryReader endianAwareBinaryReader = EndianAwareBinaryReader.Create(dataReadStream, m_endian);
                        uint num2 = endianAwareBinaryReader.ReadUInt32();
                        EndianAwareBinaryReader endianAwareBinaryReader2 = EndianAwareBinaryReader.Create(dataReadStream, EndianType.LittleEndian);
                        byte[] inData = endianAwareBinaryReader2.ReadBytes((int)num2);
                        CompressionMethod method = CompressionMethod.ZLib;
                        byte[] buffer = CompressionFactory.Decompress(inData, method, m_header.Flags);
                        MemoryStream inStream = new MemoryStream(buffer);
                        ReadTOC(inStream, m_endian);
                        m_toc.CompressedSize = num2;
                        uint num3 = (uint)Utils.GetFourByteAligned((long)(ulong)num2) - num2;
                        uint num4 = num2 + 4U + num3;
                        num += num4;
                    }
                    else
                    {
                        ReadTOC(dataReadStream, m_endian);
                        num += m_toc.Size;
                    }
                    ProgressMessage("Loading file data", new object[0]);
                    foreach (object obj in m_toc)
                    {
                        TOCEntry tocentry = (TOCEntry)obj;
                        dataReadStream.Seek((long)(ulong)(num + tocentry.DataOffset), SeekOrigin.Begin);
                        EndianAwareBinaryReader endianAwareBinaryReader3 = EndianAwareBinaryReader.Create(dataReadStream, EndianType.LittleEndian);
                        byte[] array;
                        if (tocentry.CompressedSize <= 4194304UL)
                            array = endianAwareBinaryReader3.ReadBytes((int)tocentry.CompressedSize);
                        else
                        {
                            array = new byte[tocentry.CompressedSize];
                            byte[] array2;
                            for (long num5 = 0L; num5 < (long)(ulong)tocentry.CompressedSize; num5 += array2.LongLength)
                            {
                                int length = (int)Math.Min(4194304L, (long)(tocentry.CompressedSize - (ulong)num5));
                                array2 = endianAwareBinaryReader3.ReadBytes(length);
                                Array.Copy(array2, 0L, array, num5, array2.Length);
                            }
                        }
                        tocentry.RawData = array;
                    }

                    ok = true;
                }
            }
            finally
            {
                dataReadStream.Close();
            }

            if (ok)
                return true;
            else
                return false;
        }

        private void LoadBAR()
        {
            if (LoadHeaderandTOC())
            {
                ProgressMessage("Analysing file contents", new object[0]);
                AnalyseFileTypes();
            }
        }

        private EndianType GetEndianness(Stream inStream)
        {
            EndianType result = EndianType.LittleEndian;
            int num = inStream.ReadByte();
            if (num == 173)
                result = EndianType.BigEndian;
            inStream.Seek(0L, SeekOrigin.Begin);
            return result;
        }

        private void AnalyseFileTypes()
        {
            foreach (object obj in m_toc)
            {
                TOCEntry tocentry = (TOCEntry)obj;
                byte[] data = tocentry.GetData(m_header.Flags);
                MemoryStream memoryStream = new MemoryStream(data);
                tocentry.FileType = FileTypeAnalyser.Instance.Analyse(memoryStream);
                memoryStream.Close();
            }
        }

        private void LoadInternal()
        {
            LoadBAR();
            bool flag = false;
            string text = Path.ChangeExtension(m_sourceFile, ".map");
            if (!File.Exists(text))
            {
                text = m_sourceFile + ".map";
                if (File.Exists(text))
                    flag = true;
            }
            else
                flag = true;
            if (flag)
                LoadMap(text);
        }

        private int WriteDataAlignedSection(byte[] data, Stream outStream)
        {
            EndianAwareBinaryWriter endianAwareBinaryWriter = EndianAwareBinaryWriter.Create(outStream, EndianType.LittleEndian);
            endianAwareBinaryWriter.Write(data);

            long currentPosition = outStream.Position;
            long alignedPosition = currentPosition + 3 & ~3L; // Align the position to the nearest multiple of four

            if (alignedPosition > currentPosition)
            {
                int paddingBytes = (int)(alignedPosition - currentPosition);
                byte[] padding = new byte[paddingBytes];
                endianAwareBinaryWriter.Write(padding);
            }

            return data.Length + (int)(alignedPosition - currentPosition);
        }

        private void WriteMapAs(string mapFileName)
        {
            using (TextWriter textWriter = File.CreateText(mapFileName))
            {
                string value = Path.Combine(Path.GetDirectoryName(mapFileName), Path.GetFileNameWithoutExtension(mapFileName));
                textWriter.WriteLine(value);
                textWriter.WriteLine("\n== Header ==");
                textWriter.WriteLine("size: {0}", 20U);
                uint num = 0U;
                textWriter.WriteLine("{0:X8} Magic: {1:X4}", num, m_header.Magic);
                num += 4U;
                textWriter.WriteLine("{0:X8} Version: {1:X8}", num, m_header.Version);
                num += 4U;
                textWriter.WriteLine("{0:X8} Priority: {1:X8}", num, m_header.Priority);
                num += 4U;
                textWriter.WriteLine("{0:X8} User: {1}", num, m_header.UserData);
                num += 4U;
                textWriter.WriteLine("{0:X8} Number of files: {1}", num, m_header.NumFiles);
                num += 4U;
                textWriter.WriteLine("\n== Table of Contents ==");
                if ((ushort)(GetHeader().Flags & ArchiveFlags.Bar_Flag_ZTOC) == 1)
                    textWriter.WriteLine("size: {0} (compressed)", m_toc.CompressedSize);
                else
                    textWriter.WriteLine("size: {0}", m_toc.Size);
                foreach (object obj in m_toc)
                {
                    TOCEntry tocentry = (TOCEntry)obj;
                    if ((ushort)(GetHeader().Flags & ArchiveFlags.Bar_Flag_ZTOC) == 1)
                    {
                        textWriter.WriteLine("{0:X8} {1}[{2:X8}] size={3}", new object[]
                        {
                            0,
                            tocentry.Path,
                            (uint)tocentry.FileName.Value,
                            tocentry.Size
                        });
                    }
                    else
                    {
                        textWriter.WriteLine("{0:X8} {1}[{2:X8}] size={3}", new object[]
                        {
                            num,
                            tocentry.Path,
                            (uint)tocentry.FileName.Value,
                            tocentry.Size
                        });
                        num += 16U;
                    }
                }
                textWriter.WriteLine("\n== File Data ==");
                long num2 = 0L;
                long num3 = 0L;
                foreach (TOCEntry tocentry2 in m_toc.SortByDataSectionOffset())
                {
                    float num4 = tocentry2.Size != 0U ? (1f - tocentry2.CompressedSize / tocentry2.Size) * 100f : 0f;
                    textWriter.WriteLine("{0:X8} {1:X8} {2}({3}%)", new object[]
                    {
                        num,
                        tocentry2.CompressedSize,
                        tocentry2.Path,
                        num4
                    });
                    num += tocentry2.CompressedSize + tocentry2.CompressedSize % 4U;
                    num2 += (long)(ulong)tocentry2.Size;
                    num3 += (long)(ulong)(tocentry2.CompressedSize + tocentry2.CompressedSize % 4U);
                }
                textWriter.WriteLine("\n== Stats ==");
                float num5 = num2 != 0L ? (1f - num3 / (float)num2) * 100f : 0f;
                textWriter.WriteLine("Total input data size = {0}", num2);
                textWriter.WriteLine("Total output data size = {0}", num3);
                textWriter.WriteLine("Compression = {0}%", num5);
            }
        }

        private void WriteMap(string BARFileName)
        {
            string mapFileName;
            if (KeepExtension)
                mapFileName = BARFileName + ".map";
            else
                mapFileName = Path.ChangeExtension(BARFileName, ".bar") + ".map";
            WriteMapAs(mapFileName);
        }

        private bool ReadHeader(Stream inStream)
        {
            EndianAwareBinaryReader endianAwareBinaryReader = EndianAwareBinaryReader.Create(inStream, m_endian);
            m_header.Magic = endianAwareBinaryReader.ReadUInt32();
            if (m_header.Magic != 2918127585U)
            {
                ServerConfiguration.LogWarn("File is not a BAR File or the header is corrupt", m_sourceFile);
                return false;
            }
            uint num = endianAwareBinaryReader.ReadUInt32();
            ushort flags = (ushort)(65535U & num);
            ushort version = (ushort)(65535U & num >> 16);
            m_header.Flags = (ArchiveFlags)flags;
            m_header.Version = version;
            if (m_header.Version <= 256)
            {
                ServerConfiguration.LogInfo("BAR Version 1 Detected", m_sourceFile);
                m_header.Priority = endianAwareBinaryReader.ReadInt32();
                m_header.UserData = endianAwareBinaryReader.ReadInt32();
                m_header.NumFiles = endianAwareBinaryReader.ReadUInt32();
            }
            if (m_header.Version == 512) // SHARC.
            {
                ServerConfiguration.LogInfo("BAR Version 2 Detected - Not Supported yet", m_sourceFile);
                
                // Header is encrypted using AES 128 or AES 256, can't use it just yet.

                /*m_header.IV = endianAwareBinaryReader.ReadBytes(16);
                m_header.Priority = endianAwareBinaryReader.ReadInt32();
                m_header.User = endianAwareBinaryReader.ReadInt32();
                m_header.NumFiles = endianAwareBinaryReader.ReadUInt32();
                m_header.Key = endianAwareBinaryReader.ReadBytes(32);*/

                return false;
            }
            else if (m_header.Version > 512)
            {
                ServerConfiguration.LogWarn("Invalid BAR File Version", m_sourceFile);
                return false;
            }
            SendOrPostCallback d = delegate (object sender)
            {
                if (OnLoadHeader != null)
                    OnLoadHeader(this, new BARLoadHeaderEventArgs((int)m_header.NumFiles, m_sourceFile));
            };
            if (m_asyncOp != null)
                m_asyncOp.Post(d, this);

            return true;
        }

        private void ReadTOC(Stream inStream, EndianType endian)
        {
            ProgressMessage("Loading Table of Contents", new object[0]);
            EndianAwareBinaryReader endianAwareBinaryReader = EndianAwareBinaryReader.Create(inStream, endian);
            m_toc.Clear();
            int num = 0;
            while (num < (long)(ulong)m_header.NumFiles)
            {
                int fileName = endianAwareBinaryReader.ReadInt32();
                uint num2 = endianAwareBinaryReader.ReadUInt32();
                uint offset = num2 & 4294967292U;
                uint num3 = num2 & 3U;
                CompressionMethod compression = (CompressionMethod)num3;
                uint size = endianAwareBinaryReader.ReadUInt32();
                uint compressedSize = endianAwareBinaryReader.ReadUInt32();
                TOCEntry tocentry = new TOCEntry(fileName, size, compressedSize, offset);
                tocentry.Compression = compression;
                tocentry.Index = num;
                m_toc.Add(tocentry);
                ProgressMessage("Loaded file {0}", new object[]
                {
                    ((int)tocentry.FileName).ToString("X")
                });
                num++;
            }
        }

        private void WriteHeader(EndianAwareBinaryWriter writer)
        {
            writer.Write(m_header.Magic);
            uint num = m_header.Version;
            num = num << 16 | (uint)m_header.Flags;
            writer.Write(num);
            writer.Write(m_header.Priority);
            writer.Write(m_header.UserData);
            writer.Write(m_header.NumFiles);
        }

        private TOCEntry GetTocEntryFromFilepath(string filePath, uint dataLength)
        {
            string text = GetInBARPath(filePath).Trim();
            if (!m_allowWhitespaceInFilenames && text.Contains(" "))
                throw new ArgumentException(string.Format("File paths cannot contain whitespace: {0}", text));
            AfsHash afsHash = new AfsHash(text);
            bool flag = false;
            int num;
            if (Path.GetExtension(filePath) == string.Empty && int.TryParse(Path.GetFileName(filePath), out num))
                flag = true;
            TOCEntry tocentry;
            if (flag)
            {
                int val = int.Parse(Path.GetFileName(filePath));
                tocentry = new TOCEntry((HashedFileName)val, dataLength);
                tocentry.Path = string.Empty;
            }
            else
            {
                tocentry = new TOCEntry((HashedFileName)afsHash.Value, dataLength);
                tocentry.Path = text;
            }
            return tocentry;
        }

        private void CompressAndAddFile(bool compress, string filePath, Stream inStream, TOCEntry tocEntry)
        {
            CompressionMethod compressionMethod = compress ? DefaultCompression : CompressionMethod.Uncompressed;
            byte[] array = new byte[(int)inStream.Length];
            byte[] array2 = array;
            tocEntry.Size = (uint)inStream.Length;
            if (inStream.Length == 0L)
                compressionMethod = CompressionMethod.Uncompressed;
            else
            {
                inStream.Read(array, 0, (int)inStream.Length);
                inStream.Close();
                array2 = array;
                if (compress)
                {
                    array2 = CompressionFactory.Compress(array, compressionMethod, m_header.Flags);
                    if (array2.Length >= array.Length)
                    {
                        compressionMethod = CompressionMethod.Uncompressed;
                        array2 = array;
                    }
                }
            }
            tocEntry.Compression = compressionMethod;
            tocEntry.CompressedSize = (uint)array2.Length;
            MemoryStream memoryStream = new MemoryStream(array);
            tocEntry.FileType = FileTypeAnalyser.Instance.Analyse(memoryStream);
            memoryStream.Close();
            int count = (int)m_toc.Count;
            tocEntry.Index = count;
            tocEntry.RawData = array2;
        }

        public void VerifyFileIsInDirectory(DirectoryInfo directory, string filePath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
            bool flag = false;
            DirectoryInfo directoryInfo2 = directoryInfo;
            string b = directory.FullName.ToLower().TrimEnd(new char[]
            {
                '/',
                Path.DirectorySeparatorChar
            });
            while (directoryInfo2 != null)
            {
                if (directoryInfo2.FullName.ToLower() == b)
                {
                    flag = true;
                    break;
                }
                directoryInfo2 = directoryInfo2.Parent;
            }
            if (!flag)
                throw new FileNotFoundException(string.Format("{0} is not under the build root {1}.", filePath, directory.FullName));
        }

        public static BARArchive CreateEmpty(string resourceRoot)
        {
            return new BARArchive
            {
                ResourceRoot = resourceRoot
            };
        }

        public void BeginUpdate()
        {
            m_asyncOp = AsyncOperationManager.CreateOperation(null);
        }

        public void EndUpdate()
        {
            m_asyncOp = null;
        }

        public void AddFile(string filePath)
        {
            BARAddFileOptions options = BARAddFileOptions.Default;
            AddFile(filePath, options);
        }

        public void AddFile(string filePath, BARAddFileOptions options)
        {
            VerifyFileIsInDirectory(new DirectoryInfo(m_resourceRoot), filePath);
            if (ContainsFile(filePath))
            {
                ReplaceFile(filePath, options);
                return;
            }
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                try
                {
                    AddFile(filePath, fileStream, options);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fileStream.Close();
                }
            }
        }

        public bool ContainsFile(string filePath)
        {
            string inBARPath = GetInBARPath(filePath);
            AfsHash afsHash = new AfsHash(inBARPath);
            HashedFileName filename = new HashedFileName(afsHash.Value);
            TOCEntry tocentry = m_toc[filename];
            return tocentry != null;
        }

        public void ReplaceFile(string filePath, BARAddFileOptions options)
        {
            string inBARPath = GetInBARPath(filePath);
            AfsHash afsHash = new AfsHash(inBARPath);
            HashedFileName filename = (HashedFileName)afsHash.Value;
            TOCEntry tocEntry = m_toc[filename];
            FileStream inStream = File.OpenRead(filePath);
            bool compress = ShouldCompress(filePath, options);
            CompressAndAddFile(compress, filePath, inStream, tocEntry);
            Dirty = true;
            m_toc.ResortOffsets();
            GeneralMessage("Replaced file {0}", new object[]
            {
                filePath
            });
        }

        public void AddFile(string filePath, Stream inStream, BARAddFileOptions options)
        {
            TOCEntry tocEntryFromFilepath = GetTocEntryFromFilepath(filePath, (uint)inStream.Length);
            bool compress = ShouldCompress(filePath, options);
            CompressAndAddFile(compress, filePath, inStream, tocEntryFromFilepath);
            m_toc.Add(tocEntryFromFilepath);
            TOCEntry lastEntry = m_toc.GetLastEntry();
            if (lastEntry == null)
                tocEntryFromFilepath.DataOffset = 0U;
            else
                tocEntryFromFilepath.DataOffset = lastEntry.DataOffset + lastEntry.AlignedSize;
            GeneralMessage("Added file {0}", new object[]
            {
                filePath
            });
            Dirty = true;
        }

        public void DeleteFile(HashedFileName fileName)
        {
            RunDeleteOperation(fileName, BARFileOperationFlags.Delete);
        }

        public void ObliterateFile(HashedFileName fileName)
        {
            RunDeleteOperation(fileName, BARFileOperationFlags.Obliterate);
        }

        public void UndeleteFile(HashedFileName fileName)
        {
            if (m_deletedFileSection.ContainsKey(fileName))
            {
                TOCEntry newEntry = m_deletedFileSection[fileName];
                m_toc.Add(newEntry);
                Dirty = true;
                return;
            }
            throw new Exception("Undelete failed - cannot find specified file");
        }

        public void Load()
        {
            LoadInternal();
        }

        public void FastLoad()
        {
            LoadHeaderandTOC();
        }

        public void LoadAsync()
        {
            BeginUpdate();
            Path.ChangeExtension(m_sourceFile, ".map");
            LoadBARDelegate loadBARDelegate = new LoadBARDelegate(LoadInternal);
            try
            {
                loadBARDelegate.BeginInvoke(new AsyncCallback(LoadCompleted), loadBARDelegate);
            }
            catch (BARLoadException)
            {
                throw;
            }
        }

        public void LoadCompleted(IAsyncResult ar)
        {
            LoadBARDelegate loadBARDelegate = (LoadBARDelegate)ar.AsyncState;
            try
            {
                loadBARDelegate.EndInvoke(ar);
                SendOrPostCallback d = delegate (object state)
                {
                    if (OnLoadComplete != null)
                        OnLoadComplete(this, new BAREventArgs(m_sourceFile));
                };
                if (m_asyncOp != null)
                    m_asyncOp.PostOperationCompleted(d, this);
            }
            catch (BARLoadException)
            {
                throw;
            }
            finally
            {
                EndUpdate();
            }
        }

        public void Save()
        {
            SaveAs(FileName);
        }

        public void SaveAs(string fileName)
        {
            RunCrypto();
            Stream dataWriterStream = GetDataWriterStream(fileName);
            m_header.NumFiles = m_toc.Count;
            if (Dirty)
            {
                m_toc.ResortOffsets();
            }
            EndianAwareBinaryWriter endianAwareBinaryWriter = EndianAwareBinaryWriter.Create(dataWriterStream, m_endian);
            EndianAwareBinaryWriter endianAwareBinaryWriter2 = EndianAwareBinaryWriter.Create(dataWriterStream, EndianType.LittleEndian);
            byte[] array = m_toc.GetBytes();
            if (m_endian == EndianType.BigEndian)
                array = Utils.EndianSwap(array);
            bool flag = (ushort)(m_header.Flags & ArchiveFlags.Bar_Flag_ZTOC) == 1;
            if (flag)
            {
                CompressionMethod method = CompressionMethod.ZLib;
                byte[] array2 = CompressionFactory.Compress(array, method, m_header.Flags);
                uint num = (uint)Utils.GetFourByteAligned(array2.Length);
                if (num + 4U < m_toc.Size)
                {
                    WriteHeader(endianAwareBinaryWriter);
                    endianAwareBinaryWriter.Write(num);
                    byte[] array3 = new byte[num];
                    array2.CopyTo(array3, 0);
                    endianAwareBinaryWriter2.Write(array3);
                }
                else
                {
                    Header header = m_header;
                    header.Flags &= ~ArchiveFlags.Bar_Flag_ZTOC;
                    WriteHeader(endianAwareBinaryWriter);
                    endianAwareBinaryWriter2.Write(array);
                }
            }
            else
            {
                WriteHeader(endianAwareBinaryWriter);
                endianAwareBinaryWriter2.Write(array);
            }
            long position = dataWriterStream.Position;
            foreach (TOCEntry tocentry in m_toc.SortByDataSectionOffset())
            {
                byte[] rawData = tocentry.RawData;
                WriteDataAlignedSection(rawData, dataWriterStream);
            }
            if (m_outputStreamCopy != null)
            {
                dataWriterStream.Position = 0L;
                dataWriterStream.CopyTo(m_outputStreamCopy);
                dataWriterStream.Position = m_outputStreamCopy.Position = 0L;
            }
            endianAwareBinaryWriter2.Close();
            endianAwareBinaryWriter.Close();
            WriteMap(fileName);
            GeneralMessage("BAR Archive complete", new object[0]);
            EndUpdate();
            Dirty = false;
            FileName = fileName;
        }

        public void LoadMap(string MAPFile)
        {
            ProgressMessage("Loading map file {0}", new object[]
            {
                MAPFile
            });
            FileAttributes attributes = File.GetAttributes(MAPFile);
            FileAttributes fileAttributes = attributes;
            fileAttributes &= ~FileAttributes.ReadOnly;
            File.SetAttributes(MAPFile, fileAttributes);
            TextReader textReader = File.OpenText(MAPFile);
            string text = textReader.ReadToEnd();
            textReader.Close();
            File.SetAttributes(MAPFile, attributes);
            string[] array = text.Split(new char[]
            {
                '\n'
            });
            Hashtable hashtable = new Hashtable();
            Regex regex = new Regex("([\\w\\d]+)(\\s+)(Number of files:\\s+)(?<filecount>[\\d]+)");
            Regex regex2 = new Regex("([\\w\\d]+)\\s+(?<path>[:\\w\\d\\s-_$/\\.+]+)\\[(?<hash>[\\w\\d]+)\\]");
            foreach (string input in array)
            {
                Match match = regex.Match(input);
                if (match.Success)
                    int.Parse(match.Groups["filecount"].Value);
                else
                {
                    match = regex2.Match(input);
                    if (match.Success)
                    {
                        string value = match.Groups["path"].Value;
                        HashedFileName hashedFileName = (HashedFileName)int.Parse(match.Groups["hash"].Value, NumberStyles.HexNumber);
                        hashtable[hashedFileName] = value;
                    }
                }
            }
            foreach (object obj in hashtable.Keys)
            {
                HashedFileName hashedFileName2 = (HashedFileName)obj;
                TOCEntry tocentry = m_toc[hashedFileName2];
                if (tocentry != null)
                {
                    tocentry.Path = (string)hashtable[hashedFileName2];
                    string a = Path.GetExtension(tocentry.Path).ToLower();
                    if (a == ".bar")
                        tocentry.FileType = HomeFileType.BarArchive;
                    else if (a == ".png")
                        tocentry.FileType = HomeFileType.Texture;
                }
            }
            textReader.Close();
        }

        public byte[] GetFileData(string FileName)
        {
            AfsHash afsHash = new AfsHash(FileName);
            HashedFileName fileName = new HashedFileName(afsHash.Value);
            return GetFileData(fileName);
        }

        public byte[] GetFileData(HashedFileName FileName)
        {
            TOCEntry tocentry = m_toc[FileName];
            return tocentry.GetData(m_header.Flags);
        }

        public void UpdateFile(HashedFileName FileName, byte[] inData)
        {
            TOCEntry tocentry = m_toc[FileName];
            byte[] array = inData;
            byte[] array2 = CompressionFactory.Compress(inData, tocentry.Compression, m_header.Flags);
            if (array2 != null)
            {
                if (array2.Length < inData.Length)
                    array = array2;
                else
                    tocentry.Compression = CompressionMethod.Uncompressed;
            }
            tocentry.Size = (uint)inData.Length;
            tocentry.CompressedSize = (uint)array.Length;
            tocentry.RawData = array;
            Dirty = true;
            m_toc.ResortOffsets();
            GeneralMessage("Updated file {0}", new object[]
            {
                tocentry.Path
            });
        }

        public void Recompress(HashedFileName[] fileNames, CompressionMethod newMethod, ArchiveFlags newFlags)
        {
            foreach (HashedFileName hashedFileName in fileNames)
            {
                TOCEntry tocentry = m_toc[hashedFileName];
                if (newMethod == tocentry.Compression && BARHeader.Flags == newFlags)
                    ProgressMessage("Skipped " + hashedFileName, new object[0]);
                else if (newMethod != CompressionMethod.Uncompressed && !ShouldCompress(tocentry.Path, BARAddFileOptions.Default))
                    ProgressMessage("Skipped " + hashedFileName, new object[0]);
                else
                {
                    byte[] array = CompressionFactory.Decompress(tocentry, tocentry.Compression, m_header.Flags);
                    tocentry.RawData = array;
                    byte[] array2 = CompressionFactory.Compress(tocentry, newMethod, newFlags);
                    if (newMethod != CompressionMethod.Uncompressed && array2.Length >= array.Length && newMethod != CompressionMethod.Encrypted)
                    {
                        tocentry.Compression = CompressionMethod.Uncompressed;
                        tocentry.CompressedSize = (uint)array.Length;
                        tocentry.RawData = array;
                        Dirty = true;
                        m_toc.ResortOffsets();
                        ProgressMessage(string.Format("File {0} not recompressed because the size of the recompressed data is larger than the size of the uncompressed data", hashedFileName), new object[0]);
                    }
                    else
                    {
                        tocentry.Compression = newMethod;
                        tocentry.CompressedSize = (uint)array2.Length;
                        tocentry.RawData = array2;
                        Dirty = true;
                        m_toc.ResortOffsets();
                        ProgressMessage("Recompressed " + hashedFileName, new object[0]);
                    }
                }
            }
            BARHeader.Flags = newFlags;
        }

        public void RecompressAsync(HashedFileName[] fileNames, CompressionMethod newMethod, ArchiveFlags newFlags)
        {
            BeginUpdate();
            BarRecompressDelegate barRecompressDelegate = new BarRecompressDelegate(Recompress);
            try
            {
                barRecompressDelegate.BeginInvoke(fileNames, newMethod, newFlags, new AsyncCallback(RecompressCompleted), barRecompressDelegate);
            }
            catch (Exception ex)
            {
                throw new BARRecompressException(ex.Message, m_sourceFile);
            }
        }

        public void RecompressCompleted(IAsyncResult ar)
        {
            BarRecompressDelegate barRecompressDelegate = (BarRecompressDelegate)ar.AsyncState;
            try
            {
                barRecompressDelegate.EndInvoke(ar);
                SendOrPostCallback d = delegate (object state)
                {
                    if (OnRecompressComplete != null)
                        OnRecompressComplete(this, new BAREventArgs(m_sourceFile));
                };
                if (m_asyncOp != null)
                    m_asyncOp.PostOperationCompleted(d, this);
            }
            catch
            {
                throw;
            }
            finally
            {
                EndUpdate();
            }
        }

        public byte[] GetRawFileData(string FileName)
        {
            string inBARPath = GetInBARPath(FileName);
            AfsHash afsHash = new AfsHash(inBARPath);
            HashedFileName fileName = new HashedFileName(afsHash.Value);
            return GetRawFileData(fileName);
        }

        public byte[] GetRawFileData(HashedFileName FileName)
        {
            TOCEntry tocentry = m_toc[FileName];
            if (tocentry != null)
                return tocentry.RawData;
            return null;
        }

        public void ExtractToFile(HashedFileName FileName, string outDir)
        {
            TOCEntry tocentry = m_toc[FileName];
            string path = string.Empty;
            if (tocentry.Path == null || tocentry.Path == string.Empty)
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, FileName);
            else
            {
                string arg = tocentry.Path.Replace('/', Path.DirectorySeparatorChar);
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, arg);
            }
            string directoryName = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directoryName);
            FileStream fileStream = File.Open(path, FileMode.Create);
            byte[] data = tocentry.GetData(m_header.Flags);
            fileStream.Write(data, 0, data.Length);
            fileStream.Close();
            ProgressMessage("Extracted file {0}", new object[]
            {
                Path.GetFileName(path)
            });
        }

        public void ExtractAll(string outDir)
        {
            foreach (object obj in m_toc)
            {
                TOCEntry tocentry = (TOCEntry)obj;
                ExtractToFile(tocentry.FileName, outDir);
            }
        }

        public void ExtractAllAsync(string outDir)
        {
            BeginUpdate();
            BarExtractAllDelegate barExtractAllDelegate = new BarExtractAllDelegate(ExtractAll);
            barExtractAllDelegate.BeginInvoke(outDir, new AsyncCallback(ExtractCompleted), barExtractAllDelegate);
        }

        public void ExtractCompleted(IAsyncResult ar)
        {
            BarExtractAllDelegate barExtractAllDelegate = (BarExtractAllDelegate)ar.AsyncState;
            try
            {
                barExtractAllDelegate.EndInvoke(ar);
                SendOrPostCallback d = delegate (object state)
                {
                    if (OnExtractComplete != null)
                        OnExtractComplete(this, new BAREventArgs(m_sourceFile));
                };
                if (m_asyncOp != null)
                    m_asyncOp.PostOperationCompleted(d, this);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                EndUpdate();
            }
        }

        public Hashtable GetFileTree()
        {
            Hashtable hashtable = new Hashtable();
            foreach (object obj in m_toc)
            {
                TOCEntry tocentry = (TOCEntry)obj;
                string key = Path.GetDirectoryName(tocentry.Path).Replace(Path.DirectorySeparatorChar, '/');
                string fileName = Path.GetFileName(tocentry.Path);
                List<string> list;
                if (hashtable.ContainsKey(key))
                    list = (List<string>)hashtable[key];
                else
                {
                    list = new List<string>();
                    hashtable[key] = list;
                }
                list.Add(fileName);
            }
            return hashtable;
        }

        public void CreateManifest()
        {
            Hashtable fileTree = GetFileTree();
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.Encoding = Encoding.UTF8;
            xmlWriterSettings.OmitXmlDeclaration = true;
            StringBuilder stringBuilder = new StringBuilder();
            XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("manifest");
            foreach (object obj in fileTree.Keys)
            {
                string text = (string)obj;
                xmlWriter.WriteStartElement("dir");
                xmlWriter.WriteAttributeString("name", text);
                List<string> list = (List<string>)fileTree[text];
                foreach (string value in list)
                {
                    xmlWriter.WriteStartElement("file");
                    xmlWriter.WriteAttributeString("name", value);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            MemoryStream inStream = new MemoryStream(bytes);
            string filePath = m_resourceRoot + "\\__$manifest$__";
            AddFile(filePath, inStream, BARAddFileOptions.ForceCompress);
        }

        private void RunCrypto()
        {
            if (CryptoImplementation == null)
                return;
            m_toc.ResortOffsets();
            CryptoImplementation.DoWork();
        }

        public CompressionBase CryptoImplementation
        {
            get
            {
                return cryptoImplentation;
            }
            set
            {
                cryptoImplentation = value;
                if (cryptoImplentation != null)
                    cryptoImplentation.BarReference = this;
                CompressionFactory.CryptoImplementation = value;
            }
        }

        public bool KeepExtension
        {
            get
            {
                return m_keepExtension;
            }
            set
            {
                m_keepExtension = value;
            }
        }

        public const uint HEADERSIZE = 20U;

        public const uint TOCENTRYSIZE = 16U;

        internal const long TE_DATA_CHUNKSIZE = 4194304L;

        protected string m_sourceFile = string.Empty;

        private string m_resourceRoot = string.Empty;

        private readonly Header m_header = new Header();

        private readonly TOC m_toc;

        private readonly Dictionary<HashedFileName, TOCEntry> m_deletedFileSection;

        private AsyncOperation m_asyncOp;

        private bool m_dirty;

        private bool m_allowWhitespaceInFilenames;

        private CompressionMethod m_defaultCompression = CompressionMethod.EdgeZLib;

        private EndianType m_endian;

        private bool m_keepExtension;

        public bool IsEditMode;

        protected Stream m_outputStream;

        protected Stream m_outputStreamCopy;

        protected Stream m_sourceStream;

        private CompressionBase cryptoImplentation;

        private delegate void LoadBARDelegate();

        public delegate void BAREventDelegate(object sender, BAREventArgs args);

        public delegate void BARMessageDelegate(object sender, BARMessageEventArgs args);

        public delegate void BARLoadProgressEventDelegate(object sender, BARLoadProgressEventArgs args);

        public delegate void BARLoadHeaderEventDelegate(object sender, BARLoadHeaderEventArgs args);

        private delegate bool BARFileMatchDelegate(TOCEntry te);

        private delegate void BarExtractAllDelegate(string outDir);

        private delegate void BarRecompressDelegate(HashedFileName[] fileNames, CompressionMethod newMethod, ArchiveFlags newFlags);
    }
}
