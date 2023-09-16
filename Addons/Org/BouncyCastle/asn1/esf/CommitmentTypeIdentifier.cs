using System;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.Pkcs;

namespace MultiServer.Addons.Org.BouncyCastle.Asn1.Esf
{
    public abstract class CommitmentTypeIdentifier
    {
        public static readonly DerObjectIdentifier ProofOfOrigin = PkcsObjectIdentifiers.IdCtiEtsProofOfOrigin;
        public static readonly DerObjectIdentifier ProofOfReceipt = PkcsObjectIdentifiers.IdCtiEtsProofOfReceipt;
        public static readonly DerObjectIdentifier ProofOfDelivery = PkcsObjectIdentifiers.IdCtiEtsProofOfDelivery;
        public static readonly DerObjectIdentifier ProofOfSender = PkcsObjectIdentifiers.IdCtiEtsProofOfSender;
        public static readonly DerObjectIdentifier ProofOfApproval = PkcsObjectIdentifiers.IdCtiEtsProofOfApproval;
        public static readonly DerObjectIdentifier ProofOfCreation = PkcsObjectIdentifiers.IdCtiEtsProofOfCreation;
    }
}
