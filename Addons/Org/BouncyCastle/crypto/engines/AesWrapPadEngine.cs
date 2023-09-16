using System;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Engines
{
    public class AesWrapPadEngine
        : Rfc5649WrapEngine
    {
        public AesWrapPadEngine()
            : base(AesUtilities.CreateEngine())
        {
        }
    }
}
