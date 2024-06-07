using System;

namespace HashLib.Crypto.BuildIn
{
    internal class SHA384Managed : HashCryptoBuildIn, IHasHMACBuildIn
    {
        public SHA384Managed()
#if NET6_0_OR_GREATER
            : base(System.Security.Cryptography.SHA384.Create(), 128)
#else
            : base(new System.Security.Cryptography.SHA384Managed(), 128)
#endif
        {
        }

        public virtual System.Security.Cryptography.HMAC GetBuildHMAC()
        {
            return new System.Security.Cryptography.HMACSHA384();
        }
    }
}
