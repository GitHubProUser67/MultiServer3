using System;

using MultiServer.Addons.Org.BouncyCastle.Utilities;
using MultiServer.Addons.Org.BouncyCastle.X509.Store;

namespace MultiServer.Addons.Org.BouncyCastle.Cms
{
    // TODO[api] sealed
    public class SignerID
        : X509CertStoreSelector, IEquatable<SignerID>
    {
        public virtual bool Equals(SignerID other)
        {
            return other == null ? false
                :  other == this ? true
                :  MatchesSubjectKeyIdentifier(other)
                && MatchesSerialNumber(other)
                && MatchesIssuer(other);
        }

        public override bool Equals(object obj) => Equals(obj as SignerID);

        public override int GetHashCode()
        {
            return GetHashCodeOfSubjectKeyIdentifier()
                ^  Objects.GetHashCode(SerialNumber)
                ^  Objects.GetHashCode(Issuer);
        }
    }
}
