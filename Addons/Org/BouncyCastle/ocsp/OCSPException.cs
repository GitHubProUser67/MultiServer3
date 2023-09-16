using System;
using System.Runtime.Serialization;

namespace MultiServer.Addons.Org.BouncyCastle.Ocsp
{
    [Serializable]
    public class OcspException
		: Exception
	{
		public OcspException()
			: base()
		{
		}

		public OcspException(string message)
			: base(message)
		{
		}

		public OcspException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected OcspException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
