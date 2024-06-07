using System;

namespace HashLib.Crypto.BuildIn
{
    internal class SHA1CryptoServiceProvider : HashCryptoBuildIn, IHasHMACBuildIn
    {
        public SHA1CryptoServiceProvider()
#if NET6_0_OR_GREATER
            : base(System.Security.Cryptography.SHA1.Create(), 64)
#else
            : base(new System.Security.Cryptography.SHA1CryptoServiceProvider(), 64)
#endif
        {
        }

        public virtual System.Security.Cryptography.HMAC GetBuildHMAC()
        {
#if NET6_0_OR_GREATER
            return new System.Security.Cryptography.HMACSHA1(new byte[0]);
#else
            return new System.Security.Cryptography.HMACSHA1(new byte[0], false);
#endif
        }
    }
}
