namespace CryptoSporidium.BAR
{
    public class AfsHash
    {
        public AfsHash(string text)
        {
            m_source = text;
            ComputeHash(text);
        }

        public int Value
        {
            get
            {
                return m_hash;
            }
        }

        private void ComputeHash(string text)
        {
            int num = 0;
            string text2 = text.ToLower().Replace(Path.DirectorySeparatorChar, '/');
            foreach (char value in text2)
            {
                num *= 37;
                num += Convert.ToInt32(value);
            }
            m_hash = num;
        }

        private int m_hash;

        private string m_source;
    }
}