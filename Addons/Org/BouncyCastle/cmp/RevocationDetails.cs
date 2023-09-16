using MultiServer.Addons.Org.BouncyCastle.Asn1.Cmp;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.Math;

namespace MultiServer.Addons.Org.BouncyCastle.Cmp
{
    public struct RevocationDetails
    {
        private readonly RevDetails m_revDetails;

        public RevocationDetails(RevDetails revDetails)
        {
            m_revDetails = revDetails;
        }

        public X509Name Subject => m_revDetails.CertDetails.Subject;

        public X509Name Issuer => m_revDetails.CertDetails.Issuer;

        public BigInteger SerialNumber => m_revDetails.CertDetails.SerialNumber.Value;

        // TODO[api] Rename to 'ToAsn1Structure'
        public RevDetails ToASN1Structure() => m_revDetails;
    }
}
