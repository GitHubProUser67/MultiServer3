namespace PSMultiServer.CryptoSporidium.BAR
{
    public class Header
    {
        public uint Magic
        {
            get
            {
                return m_magic;
            }
            set
            {
                m_magic = value;
            }
        }

        public ushort Version
        {
            get
            {
                return m_version;
            }
            set
            {
                m_version = value;
            }
        }

        public int Priority
        {
            get
            {
                return m_priority;
            }
            set
            {
                m_priority = value;
            }
        }

        public int Id
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        public uint NumFiles
        {
            get
            {
                return m_numFiles;
            }
            set
            {
                m_numFiles = value;
            }
        }

        public ArchiveFlags Flags
        {
            get
            {
                return m_flags;
            }
            set
            {
                m_flags = value;
            }
        }

        internal Header()
        {
            m_magic = 2918127585U;
            m_version = 256;
            m_id = (int)DateTime.Now.ToFileTime();
            m_priority = 0;
            m_numFiles = 0U;
        }

        public const uint BIG_MAGIC = 2918127585U;

        public const uint LITTLE_MAGIC = 3776442285U;

        public const ushort AFS_VERSION = 256;

        private uint m_magic;

        private ushort m_version;

        private int m_priority;

        private int m_id;

        private uint m_numFiles;

        private ArchiveFlags m_flags;
    }
}
