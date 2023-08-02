namespace PSMultiServer.CryptoSporidium.BAR
{
    public class StreamBar : BARArchive
    {
        public Stream InputStream
        {
            get
            {
                return m_sourceStream;
            }
        }

        public Stream OutputStream
        {
            get
            {
                return m_outputStreamCopy;
            }
        }

        public StreamBar(string sourceFile, string resourceRoot, Stream source)
        {
            m_sourceStream = source;
            m_outputStream = new MemoryStream();
            m_outputStreamCopy = new MemoryStream();
            ResourceRoot = resourceRoot;
            m_sourceFile = sourceFile;
        }

        public StreamBar(string sourceFile, string resourceRoot)
        {
            m_sourceStream = new MemoryStream();
            m_outputStream = new MemoryStream();
            m_outputStreamCopy = new MemoryStream();
            ResourceRoot = resourceRoot;
            m_sourceFile = sourceFile;
        }

        public StreamBar(string resourceRoot)
        {
            m_sourceStream = new MemoryStream();
            m_outputStream = new MemoryStream();
            m_outputStreamCopy = new MemoryStream();
            ResourceRoot = resourceRoot;
        }

        public StreamBar(Stream source)
        {
            m_sourceStream = source;
            m_outputStream = new MemoryStream();
            m_outputStreamCopy = new MemoryStream();
        }
    }
}
