using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters;

namespace MultiServer.Addons.Org.BouncyCastle.Cms
{
	internal interface CmsSecureReadable
	{
		AlgorithmIdentifier Algorithm { get; }
		object CryptoObject { get; }
		CmsReadable GetReadable(KeyParameter key);
	}
}
