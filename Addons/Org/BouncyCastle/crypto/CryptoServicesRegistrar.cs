using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto
{
    public static class CryptoServicesRegistrar
    {
        public static SecureRandom GetSecureRandom()
        {
            return new SecureRandom();
        }

        public static SecureRandom GetSecureRandom(SecureRandom secureRandom)
        {
            return secureRandom ?? GetSecureRandom();
        }
    }
}
