using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;

namespace MultiServer.Addons.Org.BouncyCastle.Asn1.Smime
{
    public class SmimeCapabilitiesAttribute
        : AttributeX509
    {
        public SmimeCapabilitiesAttribute(
            SmimeCapabilityVector capabilities)
            : base(SmimeAttributes.SmimeCapabilities,
                    new DerSet(new DerSequence(capabilities.ToAsn1EncodableVector())))
        {
        }
    }
}
