using System.IO;

using MultiServer.Addons.Org.BouncyCastle.Asn1.Pkcs;
using MultiServer.Addons.Org.BouncyCastle.Utilities.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Cms
{
    public class CmsTypedStream
	{
		private readonly string	m_oid;
		private readonly Stream	m_in;

		public CmsTypedStream(Stream inStream)
			: this(PkcsObjectIdentifiers.Data.Id, inStream)
		{
		}

		public CmsTypedStream(string oid, Stream inStream)
			: this(oid, inStream, Streams.DefaultBufferSize)
		{
		}

		public CmsTypedStream(string oid, Stream inStream, int bufSize)
		{
			m_oid = oid;
            m_in = new BufferedFilterStream(inStream, bufSize);
        }

		public string ContentType => m_oid;

		public Stream ContentStream => m_in;

		public void Drain()
		{
			using (m_in) Streams.Drain(m_in);
		}
	}
}
