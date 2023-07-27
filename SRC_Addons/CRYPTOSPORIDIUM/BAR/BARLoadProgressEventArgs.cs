namespace PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM.BAR
{
	public class BARLoadProgressEventArgs : BARMessageEventArgs
	{
		public BARLoadProgressEventArgs(string filename, string msg) : base(filename, msg)
		{
		}
	}
}
