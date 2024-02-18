using System;

namespace Org.BouncyCastle.Asn1.Cms
{
    public class ContentInfo
        : Asn1Encodable
    {
        public static ContentInfo GetInstance(object obj)
        {
            if (obj == null)
                return null;
            if (obj is ContentInfo contentInfo)
                return contentInfo;
            return new ContentInfo(Asn1Sequence.GetInstance(obj));
        }

        public static ContentInfo GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            return new ContentInfo(Asn1Sequence.GetInstance(obj, isExplicit));
        }

        private readonly DerObjectIdentifier	contentType;
        private readonly Asn1Encodable			content;

        private ContentInfo(
            Asn1Sequence seq)
        {
            if (seq.Count < 1 || seq.Count > 2)
                throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

            contentType = (DerObjectIdentifier) seq[0];

            if (seq.Count > 1)
            {
                Asn1TaggedObject tagged = Asn1TaggedObject.GetInstance(seq[1], Asn1Tags.ContextSpecific);
                if (!tagged.IsExplicit() || tagged.TagNo != 0)
                    throw new ArgumentException("Bad tag for 'content'", "seq");

                content = tagged.GetExplicitBaseObject();
            }
        }

        public ContentInfo(
            DerObjectIdentifier	contentType,
            Asn1Encodable		content)
        {
            this.contentType = contentType;
            this.content = content;
        }

        public DerObjectIdentifier ContentType
        {
            get { return contentType; }
        }

        public Asn1Encodable Content
        {
            get { return content; }
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * ContentInfo ::= Sequence {
         *          contentType ContentType,
         *          content
         *          [0] EXPLICIT ANY DEFINED BY contentType OPTIONAL }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(contentType);

            if (content != null)
            {
                v.Add(new BerTaggedObject(0, content));
            }

            return new BerSequence(v);
        }
    }
}