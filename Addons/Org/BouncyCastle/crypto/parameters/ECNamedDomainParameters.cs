using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X9;
using MultiServer.Addons.Org.BouncyCastle.Math;
using MultiServer.Addons.Org.BouncyCastle.Math.EC;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters
{
    public class ECNamedDomainParameters
        : ECDomainParameters
    {
        private readonly DerObjectIdentifier name;

        public DerObjectIdentifier Name
        {
            get { return name; }
        }

        public ECNamedDomainParameters(DerObjectIdentifier name, ECDomainParameters dp)
            : this(name, dp.Curve, dp.G, dp.N, dp.H, dp.GetSeed())
        {
        }

        public ECNamedDomainParameters(DerObjectIdentifier name, X9ECParameters x9)
            : base(x9)
        {
            this.name = name;
        }

        public ECNamedDomainParameters(DerObjectIdentifier name, ECCurve curve, ECPoint g, BigInteger n)
            : base(curve, g, n)
        {
            this.name = name;
        }

        public ECNamedDomainParameters(DerObjectIdentifier name, ECCurve curve, ECPoint g, BigInteger n, BigInteger h)
            : base(curve, g, n, h)
        {
            this.name = name;
        }

        public ECNamedDomainParameters(DerObjectIdentifier name, ECCurve curve, ECPoint g, BigInteger n, BigInteger h, byte[] seed)
            : base(curve, g, n, h, seed)
        {
            this.name = name;
        }
    }
}
