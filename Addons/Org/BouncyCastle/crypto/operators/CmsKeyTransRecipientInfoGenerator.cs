using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.Cms;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.Cms;
using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.X509;

namespace MultiServer.Addons.Org.BouncyCastle.Operators
{
    /// <deprecated>Use KeyTransRecipientInfoGenerator</deprecated>
    public class CmsKeyTransRecipientInfoGenerator
        : KeyTransRecipientInfoGenerator
    {
        public CmsKeyTransRecipientInfoGenerator(X509Certificate recipCert, IKeyWrapper keyWrapper)
            : base(new Asn1.Cms.IssuerAndSerialNumber(recipCert.CertificateStructure), keyWrapper)
        {
        }

        public CmsKeyTransRecipientInfoGenerator(IssuerAndSerialNumber issuerAndSerial, IKeyWrapper keyWrapper)
            : base(issuerAndSerial, keyWrapper)
        {
        }

        public CmsKeyTransRecipientInfoGenerator(byte[] subjectKeyID, IKeyWrapper keyWrapper) : base(subjectKeyID, keyWrapper)
        {
        }
    }
}
