using System;

namespace HashLib.Crypto.BuildIn
{
    internal class SHA1Managed : HashCryptoBuildIn, IHasHMACBuildIn
    {
        public SHA1Managed()
#if NET6_0_OR_GREATER
            : base(System.Security.Cryptography.SHA1.Create(), 64)
#else
            : base(new System.Security.Cryptography.SHA1Managed(), 64)
#endif
        {
        }

        public virtual System.Security.Cryptography.HMAC GetBuildHMAC()
        {
#if NET6_0_OR_GREATER
            return new System.Security.Cryptography.HMACSHA1(new byte[0]);
#else
            return new System.Security.Cryptography.HMACSHA1(new byte[0], true);
#endif
        }
    }
}
