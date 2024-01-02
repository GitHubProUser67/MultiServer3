using System.Text;

namespace BackendProject.HomeTools.ChannelID
{
    public class SceneKey
    {
        private byte[] m_A = new byte[4];
        private byte[] m_B = new byte[2];
        private byte[] m_C = new byte[2];
        private byte[] m_D = new byte[2];
        private byte[] m_E = new byte[6];

        public SceneKey(string idString) => ConvertFromGUID(new Guid(idString));

        public SceneKey(byte[] bytes)
        {
            Array.Copy(bytes, 0, m_A, 0, 4);
            Array.Copy(bytes, 4, m_B, 0, 2);
            Array.Copy(bytes, 6, m_C, 0, 2);
            Array.Copy(bytes, 8, m_D, 0, 2);
            Array.Copy(bytes, 10, m_E, 0, 6);
        }

        public SceneKey(Guid guid) => ConvertFromGUID(guid);

        public static SceneKey New() => new(Guid.NewGuid());

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0:x2}{1:x2}{2:x2}{3:x2}-", m_A[0], m_A[1], m_A[2], m_A[3]);
            stringBuilder.AppendFormat("{0:x2}{1:x2}-", m_B[0], m_B[1]);
            stringBuilder.AppendFormat("{0:x2}{1:x2}-", m_C[0], m_C[1]);
            stringBuilder.AppendFormat("{0:x2}{1:x2}-", m_D[0], m_D[1]);
            stringBuilder.AppendFormat("{0:x2}{1:x2}{2:x2}{3:x2}{4:x2}{5:x2}", m_E[0], m_E[1], m_E[2], m_E[3], m_E[4], m_E[5]);
            return stringBuilder.ToString();
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[16];
            m_A.CopyTo(bytes, 0);
            m_B.CopyTo(bytes, 4);
            m_C.CopyTo(bytes, 6);
            m_D.CopyTo(bytes, 8);
            m_E.CopyTo(bytes, 10);
            return bytes;
        }

        private byte[] ReverseBytes(byte[] sourceBytes)
        {
            byte[] numArray = new byte[sourceBytes.Length];
            for (int index = 0; index < sourceBytes.Length; ++index)
                numArray[index] = sourceBytes[sourceBytes.Length - index - 1];
            return numArray;
        }

        private void ConvertFromGUID(Guid sourceGUID)
        {
            byte[] byteArray = sourceGUID.ToByteArray();
            Array.Copy(byteArray, 0, m_A, 0, 4);
            Array.Copy(byteArray, 4, m_B, 0, 2);
            Array.Copy(byteArray, 6, m_C, 0, 2);
            Array.Copy(byteArray, 8, m_D, 0, 2);
            Array.Copy(byteArray, 10, m_E, 0, 6);
            m_A = ReverseBytes(m_A);
            m_B = ReverseBytes(m_B);
            m_C = ReverseBytes(m_C);
        }
    }
}