using System.IO;

using MultiServer.Addons.Org.BouncyCastle.Crypto.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Operators
{
    public sealed class DefaultDigestCalculator
        : IStreamCalculator<IBlockResult>
    {
        private readonly DigestSink m_digestSink;

        public DefaultDigestCalculator(IDigest digest)
        {
            m_digestSink = new DigestSink(digest);
        }

        public Stream Stream => m_digestSink;

        public IBlockResult GetResult() => new DefaultDigestResult(m_digestSink.Digest);
    }
}
