using System.Text;

namespace QuazalServer.QNetZ
{
	public class QCheckSum
	{
		public byte SandboxAccessKeyCheckSum
		{
			get
			{
				if (string.IsNullOrEmpty(QuazalServerConfiguration.AccessKey))
					return 0;

				return (byte)Encoding.ASCII.GetBytes(QuazalServerConfiguration.AccessKey).Sum(b => b);
			}
		}

        // -------------------------------------------------------- static shit
        static QCheckSum? _instance;

        public static QCheckSum? Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
    }
}
