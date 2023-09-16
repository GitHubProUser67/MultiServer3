using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1.Pkcs;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Generators;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters;

namespace MultiServer.Addons.Org.BouncyCastle.Cms
{
	/// <summary>
	/// PKCS5 scheme-2 - password converted to bytes assuming ASCII.
	/// </summary>
	public class Pkcs5Scheme2PbeKey
		: CmsPbeKey
	{
		public Pkcs5Scheme2PbeKey(
			char[]	password,
			byte[]	salt,
			int		iterationCount)
			: base(password, salt, iterationCount)
		{
		}

		public Pkcs5Scheme2PbeKey(
			char[]				password,
			AlgorithmIdentifier keyDerivationAlgorithm)
			: base(password, keyDerivationAlgorithm)
		{
		}

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        public Pkcs5Scheme2PbeKey(ReadOnlySpan<char> password, ReadOnlySpan<byte> salt, int iterationCount)
            : base(password, salt, iterationCount)
        {
        }

        public Pkcs5Scheme2PbeKey(ReadOnlySpan<char> password, AlgorithmIdentifier keyDerivationAlgorithm)
            : base(password, keyDerivationAlgorithm)
        {
        }
#endif

        internal override KeyParameter GetEncoded(
			string algorithmOid)
		{
			Pkcs5S2ParametersGenerator gen = new Pkcs5S2ParametersGenerator();

			gen.Init(
				PbeParametersGenerator.Pkcs5PasswordToBytes(password),
				salt,
				iterationCount);

			return (KeyParameter) gen.GenerateDerivedParameters(
				algorithmOid,
				CmsEnvelopedHelper.GetKeySize(algorithmOid));
		}
	}
}
