using System.Collections.Generic;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Crypto;

namespace MultiServer.Addons.Org.BouncyCastle.Pkcs
{
    public class AsymmetricKeyEntry
        : Pkcs12Entry
    {
        private readonly AsymmetricKeyParameter key;

		public AsymmetricKeyEntry(AsymmetricKeyParameter key)
			: base(new Dictionary<DerObjectIdentifier, Asn1Encodable>())
        {
            this.key = key;
        }

        public AsymmetricKeyEntry(AsymmetricKeyParameter key,
			IDictionary<DerObjectIdentifier, Asn1Encodable> attributes)
			: base(attributes)
        {
            this.key = key;
        }

		public AsymmetricKeyParameter Key
        {
            get { return this.key; }
        }

		public override bool Equals(object obj)
		{
			AsymmetricKeyEntry other = obj as AsymmetricKeyEntry;

			if (other == null)
				return false;

			return key.Equals(other.key);
		}

		public override int GetHashCode()
		{
			return ~key.GetHashCode();
		}
	}
}
