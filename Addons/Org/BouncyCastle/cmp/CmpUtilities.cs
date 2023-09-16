using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.Operators.Utilities;

namespace MultiServer.Addons.Org.BouncyCastle.Cmp
{
    internal static class CmpUtilities
    {
        internal static byte[] CalculateCertHash(Asn1Encodable asn1Encodable, AlgorithmIdentifier signatureAlgorithm,
            IDigestAlgorithmFinder digestAlgorithmFinder)
        {
            var digestAlgorithm = digestAlgorithmFinder.Find(signatureAlgorithm)
                ?? throw new CmpException("cannot find digest algorithm from signature algorithm");

            return X509.X509Utilities.CalculateDigest(digestAlgorithm, asn1Encodable);
        }
    }
}
