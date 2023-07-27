namespace PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM.BAR
{
	public abstract class EndianAwareBinaryReader
	{
		public EndianAwareBinaryReader(Stream input)
		{
			m_br = new BinaryReader(input);
		}

		public static EndianAwareBinaryReader Create(Stream input, EndianType endian)
		{
			if (endian == EndianType.LittleEndian)
			{
				return new LEBinaryReader(input);
			}
			return new BEBinaryReader(input);
		}

		public abstract byte[] ReadBytes(int length);

		public abstract uint ReadUInt32();

		public abstract ushort ReadUInt16();

		public abstract int ReadInt32();

		public abstract short ReadInt16();

		public abstract float ReadSingle();

		// Token: 0x0400003D RID: 61
		protected BinaryReader m_br;
	}
}
