using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Math;
using MultiServer.Addons.Org.BouncyCastle.Math.EC;

namespace MultiServer.Addons.Org.BouncyCastle.Bcpg
{
    public sealed class EdDsaPublicBcpgKey
        : ECPublicBcpgKey
    {
        internal EdDsaPublicBcpgKey(BcpgInputStream bcpgIn)
            : base(bcpgIn)
        {
        }

        public EdDsaPublicBcpgKey(DerObjectIdentifier oid, ECPoint point)
            : base(oid, point)
        {
        }

        public EdDsaPublicBcpgKey(DerObjectIdentifier oid, BigInteger encodedPoint)
            : base(oid, encodedPoint)
        {
        }
    }
}
