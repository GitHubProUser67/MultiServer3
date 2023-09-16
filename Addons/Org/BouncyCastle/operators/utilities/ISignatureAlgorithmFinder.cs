using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;

namespace MultiServer.Addons.Org.BouncyCastle.Operators.Utilities
{
    public interface ISignatureAlgorithmFinder
    {
        AlgorithmIdentifier Find(string signatureName);
    }
}
