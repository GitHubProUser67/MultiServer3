namespace PSMultiServer.CryptoSporidium.BAR
{
    public class BARLoadException : BARException
    {
        public BARLoadException(string message, string filename) : base(message, filename)
        {
        }
    }
}
