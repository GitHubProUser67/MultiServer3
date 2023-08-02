namespace PSMultiServer.CryptoSporidium.BAR
{
    internal class BEBinaryWriter : EndianAwareBinaryWriter
    {
        internal BEBinaryWriter(Stream output) : base(output)
        {
        }

        public override void Write(byte[] bytes)
        {
            m_bw.Write(Utils.EndianSwap(bytes));
        }

        public override void Write(uint val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Array.Reverse(bytes);
            m_bw.Write(bytes);
        }

        public override void Write(ushort val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Array.Reverse(bytes);
            m_bw.Write(bytes);
        }

        public override void Write(int val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Array.Reverse(bytes);
            m_bw.Write(bytes);
        }

        public override void Write(short val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Array.Reverse(bytes);
            m_bw.Write(bytes);
        }

        public override void Write(float val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Array.Reverse(bytes);
            m_bw.Write(bytes);
        }
    }
}
