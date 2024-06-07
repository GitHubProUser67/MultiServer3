using System;

namespace HashLib.Crypto.BuildIn
{
    internal class SHA256CryptoServiceProvider : HashCryptoBuildIn
    {
        public SHA256CryptoServiceProvider()
#if NET6_0_OR_GREATER
            : base(System.Security.Cryptography.SHA256.Create(), 64)
#else
            : base(new System.Security.Cryptography.SHA256CryptoServiceProvider(), 64)
#endif
        {
        }
    }
}
