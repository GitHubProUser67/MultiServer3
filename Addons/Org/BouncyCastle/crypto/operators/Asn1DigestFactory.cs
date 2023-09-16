using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Operators
{
    public class Asn1DigestFactory
        : IDigestFactory
    {
        public static Asn1DigestFactory Get(DerObjectIdentifier oid) =>
            new Asn1DigestFactory(DigestUtilities.GetDigest(oid), oid);

        public static Asn1DigestFactory Get(string mechanism) => Get(DigestUtilities.GetObjectIdentifier(mechanism));

        private readonly IDigest m_digest;
        private readonly DerObjectIdentifier m_oid;

        public Asn1DigestFactory(IDigest digest, DerObjectIdentifier oid)
        {
            m_digest = digest;
            m_oid = oid;
        }

        public virtual object AlgorithmDetails => new AlgorithmIdentifier(m_oid);

        public virtual int DigestLength => m_digest.GetDigestSize();

        public virtual IStreamCalculator<IBlockResult> CreateCalculator() => new DefaultDigestCalculator(m_digest);
    }
}
