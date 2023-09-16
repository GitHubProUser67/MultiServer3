using System;
using System.Runtime.Serialization;

namespace MultiServer.Addons.Org.BouncyCastle.Security.Certificates
{
    [Serializable]
    public class CertificateExpiredException
		: CertificateException
	{
		public CertificateExpiredException()
			: base()
		{
		}

		public CertificateExpiredException(string message)
			: base(message)
		{
		}

		public CertificateExpiredException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected CertificateExpiredException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
