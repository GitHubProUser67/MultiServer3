namespace BackendProject.BARTools.BAR
{
    public class TOCEntry
    {
        public HomeFileType FileType
        {
            get
            {
                return m_fileType;
            }
            set
            {
                m_fileType = value;
            }
        }

        public string Path
        {
            get
            {
                return m_path;
            }
            set
            {
                m_path = value.ToLower();
            }
        }

        public int Index
        {
            get
            {
                return m_tocIndex;
            }
            set
            {
                m_tocIndex = value;
            }
        }

        public uint CompressedSize
        {
            get
            {
                return m_compressedSize;
            }
            set
            {
                m_compressedSize = value;
            }
        }

        public uint DataOffset
        {
            get
            {
                return m_dataOffset;
            }
            set
            {
                m_dataOffset = value;
            }
        }

        public HashedFileName FileName
        {
            get
            {
                return m_fileNameHash;
            }
            set
            {
                m_fileNameHash = value;
            }
        }

        public uint Size
        {
            get
            {
                return m_size;
            }
            set
            {
                m_size = value;
            }
        }

        public uint AlignedSize
        {
            get
            {
                return (uint)Utils.GetFourByteAligned((long)(ulong)CompressedSize);
            }
        }

        public CompressionMethod Compression
        {
            get
            {
                return m_compressedMethod;
            }
            set
            {
                m_compressedMethod = value;
            }
        }

        public TOCEntry()
        {
            Random random = new();
            m_size = 0U;
            m_fileNameHash = (HashedFileName)0;
            m_compressedSize = 0U;
            m_compressedMethod = CompressionMethod.ZLib;
            m_dataOffset = uint.MaxValue;
            m_tocIndex = -1;
            m_fileType = HomeFileType.Unknown;
            m_path = string.Empty;
            m_data = Array.Empty<byte>();
            m_iv = new byte[8];
            random.NextBytes(m_iv);
        }

        public TOCEntry(HashedFileName fileName, uint size) : this()
        {
            Random random = new();
            m_size = size;
            m_fileNameHash = fileName;
            m_data = Array.Empty<byte>();
            m_iv = new byte[8];
            random.NextBytes(m_iv);
        }

        public TOCEntry(int fileName, uint size, uint compressedSize, uint offset) : this()
        {
            Random random = new();
            m_size = size;
            m_fileNameHash = (HashedFileName)fileName;
            m_dataOffset = offset;
            m_compressedSize = compressedSize;
            m_data = Array.Empty<byte>();
            m_iv = new byte[8];
            random.NextBytes(m_iv);
        }

        public TOCEntry(int fileName, uint size, uint compressedSize, uint offset, byte[] IV) : this()
        {
            m_size = size;
            m_fileNameHash = (HashedFileName)fileName;
            m_dataOffset = offset;
            m_compressedSize = compressedSize;
            m_data = Array.Empty<byte>();
            m_iv = IV;
        }

        internal byte[] GetRawDataWithEndianSwap()
        {
            return Utils.EndianSwap(m_data);
        }

        public override int GetHashCode()
        {
            return (int)m_fileNameHash;
        }

        public byte[] GetData(ArchiveFlags flags)
        {
            byte[]? array2 = null;
            try
            {
                try
                {
                    byte[]? array = CompressionFactory.Decompress(this, Compression, flags);

                    if (Compression != CompressionMethod.Encrypted && array != null && array.Length > (long)(ulong)m_size)
                    {
                        array2 = new byte[m_size];
                        Array.Copy(array, array2, (long)(ulong)m_size);
                    }
                    else
                        array2 = array;
                }
                catch (Exception)
                {
                    // Coredata.sharc file was not done right, so some entries are pure non-sense
                    // (EdgeZlib while plaintext...)

                    array2 = RawData;
                }
            }
            catch (Exception)
            {
                array2 = null;
            }
            return array2 ?? Array.Empty<byte>();
        }

        public byte[] RawData
        {
            get
            {
                return m_data;
            }
            set
            {
                m_data = value;
            }
        }

        public byte[] IV
        {
            get
            {
                return m_iv;
            }
            set
            {
                m_iv = value;
            }
        }

        public override string ToString()
        {
            return m_fileNameHash.ToString();
        }

        private HashedFileName m_fileNameHash;

        private uint m_size;

        private uint m_compressedSize;

        private CompressionMethod m_compressedMethod;

        private int m_tocIndex;

        private string m_path;

        private HomeFileType m_fileType;

        private uint m_dataOffset;

        private byte[] m_data;

        private byte[] m_iv;
    }
}