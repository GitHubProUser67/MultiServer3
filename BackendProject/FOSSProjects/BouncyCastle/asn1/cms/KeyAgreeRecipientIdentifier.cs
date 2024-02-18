using System;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.Cms
{
    public class KeyAgreeRecipientIdentifier
		: Asn1Encodable, IAsn1Choice
	{
        public static KeyAgreeRecipientIdentifier GetInstance(object obj)
        {
			if (obj == null)
				return null;

			if (obj is KeyAgreeRecipientIdentifier keyAgreeRecipientIdentifier)
				return keyAgreeRecipientIdentifier;

			if (obj is IssuerAndSerialNumber issuerAndSerialNumber)
				return new KeyAgreeRecipientIdentifier(issuerAndSerialNumber);

            if (obj is Asn1Sequence sequence)
                return new KeyAgreeRecipientIdentifier(IssuerAndSerialNumber.GetInstance(sequence));

            if (obj is Asn1TaggedObject taggedObject)
            {
                if (taggedObject.HasContextTag(0))
                    return new KeyAgreeRecipientIdentifier(RecipientKeyIdentifier.GetInstance(taggedObject, false));
            }

			throw new ArgumentException("Invalid KeyAgreeRecipientIdentifier: " + Platform.GetTypeName(obj), nameof(obj));
        }

        public static KeyAgreeRecipientIdentifier GetInstance(Asn1TaggedObject obj, bool isExplicit)
		{
            return Asn1Utilities.GetInstanceFromChoice(obj, isExplicit, GetInstance);
		}

		private readonly IssuerAndSerialNumber issuerSerial;
		private readonly RecipientKeyIdentifier rKeyID;

		public KeyAgreeRecipientIdentifier(
			IssuerAndSerialNumber issuerSerial)
		{
			this.issuerSerial = issuerSerial;
		}

		public KeyAgreeRecipientIdentifier(
			RecipientKeyIdentifier rKeyID)
		{
			this.rKeyID = rKeyID;
		}

		public IssuerAndSerialNumber IssuerAndSerialNumber
		{
			get { return issuerSerial; }
		}

		public RecipientKeyIdentifier RKeyID
		{
			get { return rKeyID; }
		}

		/** 
		 * Produce an object suitable for an Asn1OutputStream.
		 * <pre>
		 * KeyAgreeRecipientIdentifier ::= CHOICE {
		 *     issuerAndSerialNumber IssuerAndSerialNumber,
		 *     rKeyId [0] IMPLICIT RecipientKeyIdentifier
		 * }
		 * </pre>
		 */
		public override Asn1Object ToAsn1Object()
		{
			if (issuerSerial != null)
			{
				return issuerSerial.ToAsn1Object();
			}

			return new DerTaggedObject(false, 0, rKeyID);
		}
	}
}