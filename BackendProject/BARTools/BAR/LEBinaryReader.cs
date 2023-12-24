namespace BackendProject.BARTools.BAR
{
    internal class LEBinaryReader : EndianAwareBinaryReader
    {
        internal LEBinaryReader(Stream input) : base(input)
        {
        }

        public override byte[] ReadBytes(int length)
        {
            return m_br.ReadBytes(length);
        }

        public override short ReadInt16()
        {
            return m_br.ReadInt16();
        }

        public override int ReadInt32()
        {
            return m_br.ReadInt32();
        }

        public override float ReadSingle()
        {
            return m_br.ReadSingle();
        }

        public override ushort ReadUInt16()
        {
            return m_br.ReadUInt16();
        }

        public override uint ReadUInt32()
        {
            return m_br.ReadUInt32();
        }

        public override long ReadInt64()
        {
            return m_br.ReadInt64();
        }

        public override ulong ReadUInt64()
        {
            return m_br.ReadUInt64();
        }
    }
}