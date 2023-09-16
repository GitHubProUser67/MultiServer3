using System;
using System.IO;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters;
using MultiServer.Addons.Org.BouncyCastle.Security;
using MultiServer.Addons.Org.BouncyCastle.Utilities.Date;
using MultiServer.Addons.Org.BouncyCastle.Utilities.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Cms
{
	public class CmsAuthenticatedGenerator
		: CmsEnvelopedGenerator
	{
		public CmsAuthenticatedGenerator()
		{
		}

        /// <summary>Constructor allowing specific source of randomness</summary>
        /// <param name="random">Instance of <c>SecureRandom</c> to use.</param>
        public CmsAuthenticatedGenerator(SecureRandom random)
			: base(random)
		{
		}
	}
}
