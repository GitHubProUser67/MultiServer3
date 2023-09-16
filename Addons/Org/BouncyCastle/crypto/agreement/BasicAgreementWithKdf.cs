using System.Security.Cryptography;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Agreement.Kdf;
using MultiServer.Addons.Org.BouncyCastle.Math;
using MultiServer.Addons.Org.BouncyCastle.Security;
using MultiServer.Addons.Org.BouncyCastle.Utilities;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Agreement
{
    internal static class BasicAgreementWithKdf
    {
        internal static BigInteger CalculateAgreementWithKdf(string algorithm, IDerivationFunction kdf, int fieldSize,
            BigInteger result)
        {
            // Note that the ec.KeyAgreement class in JCE only uses kdf in one
            // of the engineGenerateSecret methods.

            int keySize = GeneratorUtilities.GetDefaultKeySize(algorithm);

            DHKdfParameters dhKdfParams = new DHKdfParameters(
                new DerObjectIdentifier(algorithm),
                keySize,
                BigIntegers.AsUnsignedByteArray(fieldSize, result));

            kdf.Init(dhKdfParams);

            byte[] keyBytes = new byte[keySize / 8];
            kdf.GenerateBytes(keyBytes, 0, keyBytes.Length);

            return new BigInteger(1, keyBytes);
        }
    }
}
