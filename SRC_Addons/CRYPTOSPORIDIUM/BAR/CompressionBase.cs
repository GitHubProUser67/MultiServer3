namespace PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM.BAR
{
	public abstract class CompressionBase
	{
		public abstract byte[] Compress(byte[] inData);

		public abstract byte[] Decompress(byte[] inData);

		public virtual byte[] Decrypt(TOCEntry toc_entry)
		{
			return null;
		}

		public virtual byte[] Compress(TOCEntry toc_entry)
		{
			return null;
		}

		public virtual void DoWork()
		{
		}

		public abstract CompressionMethod Method { get; }

		public BARArchive BarReference;
	}
}
