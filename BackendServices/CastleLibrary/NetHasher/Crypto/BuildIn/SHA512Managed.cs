using System;

namespace HashLib.Crypto.BuildIn
{
    internal class SHA512Managed : HashCryptoBuildIn, IHasHMACBuildIn
    {
        public SHA512Managed()
#if NET6_0_OR_GREATER
            : base(System.Security.Cryptography.SHA512.Create(), 128)
#else
            : base(new System.Security.Cryptography.SHA512Managed(), 128)
#endif
        {
        }

        public virtual System.Security.Cryptography.HMAC GetBuildHMAC()
        {
            return new System.Security.Cryptography.HMACSHA512();
        }
    }
}
