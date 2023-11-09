namespace CryptoSporidium.BAR
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

        public int UserData
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

        public byte[] Key
        {
            get
            {
                return m_key;
            }
            set
            {
                m_key = value;
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
            UserData = (int)DateTime.Now.ToFileTime();
            m_priority = 0;
            m_numFiles = 0U;
            m_iv = null;
            m_key = null;
        }

        public const uint BIG_MAGIC = 2918127585U;

        public const uint LITTLE_MAGIC = 3776442285U;

        public const ushort AFS_VERSION = 256;

        private uint m_magic;

        private ushort m_version;

        private byte[] m_iv;

        private int m_priority;

        private int m_id;

        private uint m_numFiles;

        private byte[] m_key;

        private ArchiveFlags m_flags;
    }
}