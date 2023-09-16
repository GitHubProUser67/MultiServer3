using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.Ocsp;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.X509;

namespace MultiServer.Addons.Org.BouncyCastle.Ocsp
{
	public class RespData
		: X509ExtensionBase
	{
		internal readonly ResponseData data;

		public RespData(
			ResponseData data)
		{
			this.data = data;
		}

		public int Version
		{
            get { return data.Version.IntValueExact + 1; }
		}

		public RespID GetResponderId()
		{
			return new RespID(data.ResponderID);
		}

		public DateTime ProducedAt
		{
			get { return data.ProducedAt.ToDateTime(); }
		}

        public SingleResp[] GetResponses()
        {
            return data.Responses.MapElements(element => new SingleResp(SingleResponse.GetInstance(element)));
        }

        public X509Extensions ResponseExtensions
		{
			get { return data.ResponseExtensions; }
		}

		protected override X509Extensions GetX509Extensions()
		{
			return ResponseExtensions;
		}
	}
}
