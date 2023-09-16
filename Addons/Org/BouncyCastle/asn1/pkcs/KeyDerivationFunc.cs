using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;

namespace MultiServer.Addons.Org.BouncyCastle.Asn1.Pkcs
{
	public class KeyDerivationFunc
		: AlgorithmIdentifier
	{
		internal KeyDerivationFunc(Asn1Sequence seq)
			: base(seq)
		{
		}

		public KeyDerivationFunc(
			DerObjectIdentifier	id,
			Asn1Encodable		parameters)
			: base(id, parameters)
		{
		}
	}
}