using MultiServer.Addons.Org.BouncyCastle.Utilities;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Bike
{
    public sealed class BikePublicKeyParameters
        : BikeKeyParameters
    {
        internal readonly byte[] m_publicKey;

        public BikePublicKeyParameters(BikeParameters param, byte[] publicKey)
            : base(false, param)
        {
            m_publicKey = Arrays.Clone(publicKey);
        }

        public byte[] GetEncoded() => Arrays.Clone(m_publicKey);
    }
}
