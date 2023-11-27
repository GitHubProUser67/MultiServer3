namespace CryptoSporidium.BARTools.BAR
{
    public class BARLoadProgressEventArgs : BARMessageEventArgs
    {
        public BARLoadProgressEventArgs(string filename, string msg) : base(filename, msg)
        {
        }
    }
}