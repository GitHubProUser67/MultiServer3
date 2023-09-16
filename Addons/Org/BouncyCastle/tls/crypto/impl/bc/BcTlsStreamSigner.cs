using System;
using System.IO;

using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Crypto.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    internal sealed class BcTlsStreamSigner
        : TlsStreamSigner
    {
        private readonly SignerSink m_output;

        internal BcTlsStreamSigner(ISigner signer)
        {
            this.m_output = new SignerSink(signer);
        }

        public Stream Stream
        {
            get { return m_output; }
        }

        public byte[] GetSignature()
        {
            try
            {
                return m_output.Signer.GenerateSignature();
            }
            catch (CryptoException e)
            {
                throw new TlsFatalAlert(AlertDescription.internal_error, e);
            }
        }
    }
}
