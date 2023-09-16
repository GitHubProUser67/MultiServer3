using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Utilities;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Lms
{
    public abstract class LmsKeyParameters
        : AsymmetricKeyParameter, IEncodable
    {
        internal LmsKeyParameters(bool isPrivateKey)
            : base(isPrivateKey)
        {
        }

        public abstract byte[] GetEncoded();
    }
}
