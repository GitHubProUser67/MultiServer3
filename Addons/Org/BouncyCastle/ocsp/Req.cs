using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1.Ocsp;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.X509;

namespace MultiServer.Addons.Org.BouncyCastle.Ocsp
{
	public class Req
		: X509ExtensionBase
	{
		private Request req;

		public Req(
			Request req)
		{
			this.req = req;
		}

		public CertificateID GetCertID()
		{
			return new CertificateID(req.ReqCert);
		}

		public X509Extensions SingleRequestExtensions
		{
			get { return req.SingleRequestExtensions; }
		}

		protected override X509Extensions GetX509Extensions()
		{
			return SingleRequestExtensions;
		}
	}
}
