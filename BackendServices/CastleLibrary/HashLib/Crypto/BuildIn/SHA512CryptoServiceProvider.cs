using System;

namespace HashLib.Crypto.BuildIn
{
    internal class SHA512CryptoServiceProvider : HashCryptoBuildIn
    {
        public SHA512CryptoServiceProvider()
#if NET6_0_OR_GREATER
            : base(System.Security.Cryptography.SHA512.Create(), 128)
#else
            : base(new System.Security.Cryptography.SHA512CryptoServiceProvider(), 128)
#endif
        {
        }
    }
}
