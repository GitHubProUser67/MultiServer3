using System;

namespace HashLib.Crypto.BuildIn
{
    internal class SHA384CryptoServiceProvider : HashCryptoBuildIn
    {
        public SHA384CryptoServiceProvider()
#if NET6_0_OR_GREATER
            : base(System.Security.Cryptography.SHA384.Create(), 128)
#else
            : base(new System.Security.Cryptography.SHA384CryptoServiceProvider(), 128)
#endif
        {
        }
    }
}
