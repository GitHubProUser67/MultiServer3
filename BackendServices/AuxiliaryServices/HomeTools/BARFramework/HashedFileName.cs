namespace HomeTools.BARFramework
{
    public struct HashedFileName
    {
        public HashedFileName(int val)
        {
            m_value = val;
        }

        public int Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        public static explicit operator int(HashedFileName fileName)
        {
            return fileName.Value;
        }

        public static explicit operator HashedFileName(int val)
        {
            return new HashedFileName(val);
        }

        public override string ToString()
        {
            return m_value.ToString();
        }

        private int m_value;
    }
}