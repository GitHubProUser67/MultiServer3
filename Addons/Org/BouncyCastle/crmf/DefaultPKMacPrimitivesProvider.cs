using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Crmf
{
    public class DefaultPKMacPrimitivesProvider
        : IPKMacPrimitivesProvider
    {
        public IDigest CreateDigest(AlgorithmIdentifier digestAlg)
        {
            return DigestUtilities.GetDigest(digestAlg.Algorithm);
        }

        public IMac CreateMac(AlgorithmIdentifier macAlg)
        {
            return MacUtilities.GetMac(macAlg.Algorithm);
        }
    }
}
