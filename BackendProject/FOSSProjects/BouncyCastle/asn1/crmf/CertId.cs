using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Crmf
{
    public class CertId
        : Asn1Encodable
    {
        public static CertId GetInstance(object obj)
        {
            if (obj == null)
                return null;
            if (obj is CertId certID)
                return certID;
            return new CertId(Asn1Sequence.GetInstance(obj));
        }

        public static CertId GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            return new CertId(Asn1Sequence.GetInstance(obj, isExplicit));
        }

        private readonly GeneralName m_issuer;
        private readonly DerInteger m_serialNumber;

        private CertId(Asn1Sequence seq)
        {
            m_issuer = GeneralName.GetInstance(seq[0]);
            m_serialNumber = DerInteger.GetInstance(seq[1]);
        }

        public virtual GeneralName Issuer => m_issuer;

        public virtual DerInteger SerialNumber => m_serialNumber;

        /**
         * <pre>
         * CertId ::= SEQUENCE {
         *                 issuer           GeneralName,
         *                 serialNumber     INTEGER }
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object() => new DerSequence(m_issuer, m_serialNumber);
    }
}