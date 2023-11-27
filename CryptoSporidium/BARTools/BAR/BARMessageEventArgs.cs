namespace CryptoSporidium.BARTools.BAR
{
    public class BARMessageEventArgs : BAREventArgs
    {
        public string Message
        {
            get
            {
                return m_msg;
            }
        }

        public BARMessageEventArgs(string filename, string msg) : base(filename)
        {
            m_msg = msg;
        }

        private readonly string m_msg;
    }
}