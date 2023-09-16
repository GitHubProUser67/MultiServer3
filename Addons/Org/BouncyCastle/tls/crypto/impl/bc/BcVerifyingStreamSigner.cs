using System;
using System.IO;

using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Crypto.IO;
using MultiServer.Addons.Org.BouncyCastle.Utilities.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    internal sealed class BcVerifyingStreamSigner
        : TlsStreamSigner
    {
        private readonly ISigner m_signer;
        private readonly ISigner m_verifier;
        private readonly TeeOutputStream m_output;

        internal BcVerifyingStreamSigner(ISigner signer, ISigner verifier)
        {
            Stream outputSigner = new SignerSink(signer);
            Stream outputVerifier = new SignerSink(verifier);

            this.m_signer = signer;
            this.m_verifier = verifier;
            this.m_output = new TeeOutputStream(outputSigner, outputVerifier);
        }

        public Stream Stream
        {
            get { return m_output; }
        }

        public byte[] GetSignature()
        {
            try
            {
                byte[] signature = m_signer.GenerateSignature();
                if (m_verifier.VerifySignature(signature))
                    return signature;
            }
            catch (CryptoException e)
            {
                throw new TlsFatalAlert(AlertDescription.internal_error, e);
            }

            throw new TlsFatalAlert(AlertDescription.internal_error);
        }
    }
}
