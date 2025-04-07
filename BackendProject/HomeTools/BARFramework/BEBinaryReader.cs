namespace BackendProject.HomeTools.BARFramework
{
    internal class BEBinaryReader : EndianAwareBinaryReader
    {
        internal BEBinaryReader(Stream input) : base(input)
        {
        }

        public override byte[] ReadBytes(int length)
        {
            byte[] dataIn = m_br.ReadBytes(length);
            return Utils.EndianSwap(dataIn);
        }

        public override short ReadInt16()
        {
            int num = 2;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToInt16(array, 0);
        }

        public override int ReadInt32()
        {
            int num = 4;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToInt32(array, 0);
        }

        public override float ReadSingle()
        {
            int num = 4;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToSingle(array, 0);
        }

        public override ushort ReadUInt16()
        {
            int num = 2;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToUInt16(array, 0);
        }

        public override uint ReadUInt32()
        {
            int num = 4;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToUInt32(array, 0);
        }

        public override long ReadInt64()
        {
            int num = 8;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToInt64(array, 0);
        }

        public override ulong ReadUInt64()
        {
            int num = 8;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToUInt64(array, 0);
        }
    }
}