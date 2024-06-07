using System;

namespace HashLib.Crypto.BuildIn
{
    internal class SHA256Managed : HashCryptoBuildIn, IHasHMACBuildIn
    {
        public SHA256Managed()
#if NET6_0_OR_GREATER
            : base(System.Security.Cryptography.SHA256.Create(), 64)
#else
            : base(new System.Security.Cryptography.SHA256Managed(), 64)
#endif
        {
        }

        public virtual System.Security.Cryptography.HMAC GetBuildHMAC()
        {
            return new System.Security.Cryptography.HMACSHA256();
        }
    }
}
