namespace PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM.BAR
{
	public class BARLoadHeaderEventArgs : BAREventArgs
	{
		public BARLoadHeaderEventArgs(int fileCount, string fileName) : base(fileName)
		{
			m_fileCount = fileCount;
		}

		public int FileCount
		{
			get
			{
				return m_fileCount;
			}
		}

		private readonly int m_fileCount;
	}
}
