using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Math;
using MultiServer.Addons.Org.BouncyCastle.Math.EC;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters
{
    public class ECGost3410Parameters
        : ECNamedDomainParameters
    {
        private readonly DerObjectIdentifier _publicKeyParamSet;
        private readonly DerObjectIdentifier _digestParamSet;
        private readonly DerObjectIdentifier _encryptionParamSet;

        public DerObjectIdentifier PublicKeyParamSet
        {
            get { return _publicKeyParamSet; }
        }

        public DerObjectIdentifier DigestParamSet
        {
            get { return _digestParamSet; }
        }

        public DerObjectIdentifier EncryptionParamSet
        {
            get { return _encryptionParamSet; }
        }

        public ECGost3410Parameters(
            ECNamedDomainParameters dp,
            DerObjectIdentifier publicKeyParamSet,
            DerObjectIdentifier digestParamSet,
            DerObjectIdentifier encryptionParamSet)
            : base(dp.Name, dp.Curve, dp.G, dp.N, dp.H, dp.GetSeed())
        {
            this._publicKeyParamSet = publicKeyParamSet;
            this._digestParamSet = digestParamSet;
            this._encryptionParamSet = encryptionParamSet;
        }

        public ECGost3410Parameters(ECDomainParameters dp, DerObjectIdentifier publicKeyParamSet,
            DerObjectIdentifier digestParamSet,
            DerObjectIdentifier encryptionParamSet)
            : base(publicKeyParamSet, dp.Curve, dp.G, dp.N, dp.H, dp.GetSeed())
        {
            this._publicKeyParamSet = publicKeyParamSet;
            this._digestParamSet = digestParamSet;
            this._encryptionParamSet = encryptionParamSet;
        }
    }
}
