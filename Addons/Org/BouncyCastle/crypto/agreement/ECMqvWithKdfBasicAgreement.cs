using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Agreement.Kdf;
using MultiServer.Addons.Org.BouncyCastle.Math;
using MultiServer.Addons.Org.BouncyCastle.Security;
using MultiServer.Addons.Org.BouncyCastle.Utilities;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Agreement
{
    // TODO[api] sealed
    public class ECMqvWithKdfBasicAgreement
		: ECMqvBasicAgreement
	{
		private readonly string m_algorithm;
		private readonly IDerivationFunction m_kdf;

		public ECMqvWithKdfBasicAgreement(string algorithm, IDerivationFunction kdf)
		{
            m_algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            m_kdf = kdf ?? throw new ArgumentNullException(nameof(kdf));
		}

		public override BigInteger CalculateAgreement(ICipherParameters pubKey)
		{
            BigInteger result = base.CalculateAgreement(pubKey);

            return BasicAgreementWithKdf.CalculateAgreementWithKdf(m_algorithm, m_kdf, GetFieldSize(), result);
		}
	}
}
