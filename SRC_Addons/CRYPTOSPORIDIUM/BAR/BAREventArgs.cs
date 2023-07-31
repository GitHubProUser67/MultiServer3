namespace PSMultiServer.Addons.CRYPTOSPORIDIUM.BAR
{
	public class BAREventArgs : EventArgs
	{
		public string FileName
		{
			get
			{
				return m_fileName;
			}
		}

		public BAREventArgs(string filename)
		{
			m_fileName = filename;
		}

		private readonly string m_fileName;
	}
}
