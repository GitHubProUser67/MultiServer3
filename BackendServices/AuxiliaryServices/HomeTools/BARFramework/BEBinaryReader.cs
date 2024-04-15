using EndianTools;

namespace HomeTools.BARFramework
{
    internal class BEBinaryReader : EndianAwareBinaryReader
    {
        internal BEBinaryReader(Stream input) : base(input)
        {
        }

        public override byte[] ReadBytes(int length)
        {
            //.NET8 m_br.BaseStream will have length 0 sometimes.like:https://github.com/dotnet/wcf/issues/5205
            if (m_br.BaseStream.Length == 0)
                return Array.Empty<byte>();

            byte[] dataIn = m_br.ReadBytes(length);
            return EndianUtils.EndianSwap(dataIn);
        }

        public override short ReadInt16()
        {
            //.NET8 m_br.BaseStream will have length 0 sometimes.like:https://github.com/dotnet/wcf/issues/5205
            if (m_br.BaseStream.Length == 0)
                return 0;

            int num = 2;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToInt16(array, 0);
        }

        public override int ReadInt32()
        {
            //.NET8 m_br.BaseStream will have length 0 sometimes.like:https://github.com/dotnet/wcf/issues/5205
            if (m_br.BaseStream.Length == 0)
                return 0;

            int num = 4;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToInt32(array, 0);
        }

        public override float ReadSingle()
        {
            //.NET8 m_br.BaseStream will have length 0 sometimes.like:https://github.com/dotnet/wcf/issues/5205
            if (m_br.BaseStream.Length == 0)
                return 0;

            int num = 4;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToSingle(array, 0);
        }

        public override ushort ReadUInt16()
        {
            //.NET8 m_br.BaseStream will have length 0 sometimes.like:https://github.com/dotnet/wcf/issues/5205
            if (m_br.BaseStream.Length == 0)
                return 0;

            int num = 2;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToUInt16(array, 0);
        }

        public override uint ReadUInt32()
        {
            //.NET8 m_br.BaseStream will have length 0 sometimes.like:https://github.com/dotnet/wcf/issues/5205
            if (m_br.BaseStream.Length == 0)
                return 0;

            int num = 4;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToUInt32(array, 0);
        }

        public override long ReadInt64()
        {
            //.NET8 m_br.BaseStream will have length 0 sometimes.like:https://github.com/dotnet/wcf/issues/5205
            if (m_br.BaseStream.Length == 0)
                return 0;

            int num = 8;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToInt64(array, 0);
        }

        public override ulong ReadUInt64()
        {
            //.NET8 m_br.BaseStream will have length 0 sometimes.like:https://github.com/dotnet/wcf/issues/5205
            if (m_br.BaseStream.Length == 0)
                return 0;

            int num = 8;
            byte[] array = new byte[num];
            m_br.Read(array, 0, num);
            Array.Reverse(array);
            return BitConverter.ToUInt64(array, 0);
        }
    }
}