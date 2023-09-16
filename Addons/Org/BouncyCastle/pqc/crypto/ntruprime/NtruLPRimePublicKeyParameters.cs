using MultiServer.Addons.Org.BouncyCastle.Utilities;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.NtruPrime
{
    public sealed class NtruLPRimePublicKeyParameters
        : NtruLPRimeKeyParameters
    {
        internal byte[] pubKey;

        public NtruLPRimePublicKeyParameters(NtruLPRimeParameters primeParameters, byte[] pubKey)
            : base(false, primeParameters)
        {
            this.pubKey = Arrays.Clone(pubKey);
        }

        public byte[] GetPublicKey()
        {
            return Arrays.Clone(pubKey);
        }

        public byte[] GetEncoded()
        {
            return GetPublicKey();
        }
    }
}
