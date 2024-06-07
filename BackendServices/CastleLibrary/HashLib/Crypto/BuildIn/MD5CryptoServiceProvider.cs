using System;

namespace HashLib.Crypto.BuildIn
{
    internal class MD5CryptoServiceProvider : HashCryptoBuildIn, IHasHMACBuildIn
    {
        public MD5CryptoServiceProvider()
#if NET6_0_OR_GREATER
            : base(System.Security.Cryptography.MD5.Create(), 64)
#else
            : base(new System.Security.Cryptography.MD5CryptoServiceProvider(), 64)
#endif
        {
        }

        public virtual System.Security.Cryptography.HMAC GetBuildHMAC()
        {
            return new System.Security.Cryptography.HMACMD5();
        }
    }
}
