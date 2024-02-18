using System;
using System.Collections.Generic;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Operators.Utilities;

namespace Org.BouncyCastle.Cms
{
    /**
     * general class for generating a pkcs7-signature message.
     * <p>
     * A simple example of usage.
     *
     * <pre>
     *      IX509Store certs...
     *      IX509Store crls...
     *      CmsSignedDataGenerator gen = new CmsSignedDataGenerator();
     *
     *      gen.AddSigner(privKey, cert, CmsSignedGenerator.DigestSha1);
     *      gen.AddCertificates(certs);
     *      gen.AddCrls(crls);
     *
     *      CmsSignedData data = gen.Generate(content);
     * </pre>
	 * </p>
     */
    public class CmsSignedDataGenerator
        : CmsSignedGenerator
    {
		private readonly IList<SignerInf> signerInfs = new List<SignerInf>();

		private class SignerInf
        {
            private readonly CmsSignedGenerator outer;

			private readonly ISignatureFactory			sigCalc;
			private readonly SignerIdentifier			signerIdentifier;
			private readonly string						digestOid;
			private readonly string						encOid;
			private readonly CmsAttributeTableGenerator	sAttr;
			private readonly CmsAttributeTableGenerator	unsAttr;
			private readonly Asn1.Cms.AttributeTable	baseSignedTable;

			internal SignerInf(
                CmsSignedGenerator			outer,
	            AsymmetricKeyParameter		key,
				SecureRandom                random,
	            SignerIdentifier			signerIdentifier,
	            string						digestOid,
	            string						encOid,
	            CmsAttributeTableGenerator	sAttr,
	            CmsAttributeTableGenerator	unsAttr,
	            Asn1.Cms.AttributeTable		baseSignedTable)
	        {
                string digestName = CmsSignedHelper.GetDigestAlgName(digestOid);

                string signatureName = digestName + "with" + CmsSignedHelper.GetEncryptionAlgName(encOid);

                this.outer = outer;
                this.sigCalc = new Asn1SignatureFactory(signatureName, key, random);
                this.signerIdentifier = signerIdentifier;
                this.digestOid = digestOid;
                this.encOid = encOid;
	            this.sAttr = sAttr;
	            this.unsAttr = unsAttr;
	            this.baseSignedTable = baseSignedTable;
            }

            internal SignerInf(
                CmsSignedGenerator outer,
                ISignatureFactory sigCalc,
                SignerIdentifier signerIdentifier,
                CmsAttributeTableGenerator sAttr,
                CmsAttributeTableGenerator unsAttr,
                Asn1.Cms.AttributeTable baseSignedTable)
            {
                this.outer = outer;
                this.sigCalc = sigCalc;
                this.signerIdentifier = signerIdentifier;
                this.digestOid = DefaultDigestAlgorithmFinder.Instance
					.Find((AlgorithmIdentifier)sigCalc.AlgorithmDetails).Algorithm.Id;
                this.encOid = ((AlgorithmIdentifier)sigCalc.AlgorithmDetails).Algorithm.Id;
                this.sAttr = sAttr;
                this.unsAttr = unsAttr;
                this.baseSignedTable = baseSignedTable;
            }

            internal AlgorithmIdentifier DigestAlgorithmID
			{
				get { return new AlgorithmIdentifier(new DerObjectIdentifier(digestOid), DerNull.Instance); }
			}

			internal CmsAttributeTableGenerator SignedAttributes
            {
				get { return sAttr; }
            }

            internal CmsAttributeTableGenerator UnsignedAttributes
            {
				get { return unsAttr; }
            }

			internal SignerInfo ToSignerInfo(DerObjectIdentifier contentType, CmsProcessable content)
            {
                AlgorithmIdentifier digAlgId = DigestAlgorithmID;
				string digestName = CmsSignedHelper.GetDigestAlgName(digestOid);

				string signatureName = digestName + "with" + CmsSignedHelper.GetEncryptionAlgName(encOid);

				if (!outer.m_digests.TryGetValue(digestOid, out var hash))
                {
                    IDigest dig = CmsSignedHelper.GetDigestInstance(digestName);
                    if (content != null)
                    {
                        content.Write(new DigestSink(dig));
                    }
                    hash = DigestUtilities.DoFinal(dig);
                    outer.m_digests.Add(digestOid, (byte[])hash.Clone());
                }

				Asn1Set signedAttr = null;

				IStreamCalculator<IBlockResult> calculator = sigCalc.CreateCalculator();
				using (Stream sigStr = calculator.Stream)
                {
					if (sAttr != null)
					{
						var parameters = outer.GetBaseParameters(contentType, digAlgId, hash);

                        //Asn1.Cms.AttributeTable signed = sAttr.GetAttributes(Collections.unmodifiableMap(parameters));
                        Asn1.Cms.AttributeTable signed = sAttr.GetAttributes(parameters);

						if (contentType == null) //counter signature
						{
							if (signed != null && signed[CmsAttributes.ContentType] != null)
							{
								signed = signed.Remove(CmsAttributes.ContentType);
							}
						}

						// TODO Validate proposed signed attributes

						signedAttr = outer.GetAttributeSet(signed);

						// sig must be composed from the DER encoding.
						signedAttr.EncodeTo(sigStr, Asn1Encodable.Der);
					}
					else if (content != null)
					{
						// TODO Use raw signature of the hash value instead
						content.Write(sigStr);
					}
				}

                byte[] sigBytes = calculator.GetResult().Collect();

				Asn1Set unsignedAttr = null;
				if (unsAttr != null)
				{
					var baseParameters = outer.GetBaseParameters(contentType, digAlgId, hash);
					baseParameters[CmsAttributeTableParameter.Signature] = sigBytes.Clone();

//					Asn1.Cms.AttributeTable unsigned = unsAttr.GetAttributes(Collections.unmodifiableMap(baseParameters));
					Asn1.Cms.AttributeTable unsigned = unsAttr.GetAttributes(baseParameters);

					// TODO Validate proposed unsigned attributes

					unsignedAttr = outer.GetAttributeSet(unsigned);
				}

				// TODO[RSAPSS] Need the ability to specify non-default parameters
				Asn1Encodable sigX509Parameters = SignerUtilities.GetDefaultX509Parameters(signatureName);
				AlgorithmIdentifier encAlgId = CmsSignedHelper.GetEncAlgorithmIdentifier(
					new DerObjectIdentifier(encOid), sigX509Parameters);
				
                return new SignerInfo(signerIdentifier, digAlgId,
                    signedAttr, encAlgId, new DerOctetString(sigBytes), unsignedAttr);
            }
        }

		public CmsSignedDataGenerator()
        {
        }

		/// <summary>Constructor allowing specific source of randomness</summary>
		/// <param name="random">Instance of <c>SecureRandom</c> to use.</param>
		public CmsSignedDataGenerator(SecureRandom random)
			: base(random)
		{
		}

		/**
        * add a signer - no attributes other than the default ones will be
        * provided here.
		*
		* @param key signing key to use
		* @param cert certificate containing corresponding public key
		* @param digestOID digest algorithm OID
        */
        public void AddSigner(
            AsymmetricKeyParameter	privateKey,
            X509Certificate			cert,
            string					digestOID)
        {
        	AddSigner(privateKey, cert, CmsSignedHelper.GetEncOid(privateKey, digestOID), digestOID);
		}

		/**
		 * add a signer, specifying the digest encryption algorithm to use - no attributes other than the default ones will be
		 * provided here.
		 *
		 * @param key signing key to use
		 * @param cert certificate containing corresponding public key
		 * @param encryptionOID digest encryption algorithm OID
		 * @param digestOID digest algorithm OID
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			X509Certificate			cert,
			string					encryptionOID,
			string					digestOID)
		{
			doAddSigner(privateKey, GetSignerIdentifier(cert), encryptionOID, digestOID,
				new DefaultSignedAttributeTableGenerator(), null, null);
		}

	    /**
	     * add a signer - no attributes other than the default ones will be
	     * provided here.
	     */
	    public void AddSigner(
            AsymmetricKeyParameter	privateKey,
	        byte[]					subjectKeyID,
            string					digestOID)
	    {
			AddSigner(privateKey, subjectKeyID, CmsSignedHelper.GetEncOid(privateKey, digestOID), digestOID);
	    }

		/**
		 * add a signer, specifying the digest encryption algorithm to use - no attributes other than the default ones will be
		 * provided here.
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			byte[]					subjectKeyID,
			string					encryptionOID,
			string					digestOID)
		{
			doAddSigner(privateKey, GetSignerIdentifier(subjectKeyID), encryptionOID, digestOID,
				new DefaultSignedAttributeTableGenerator(), null, null);
		}

        /**
        * add a signer with extra signed/unsigned attributes.
		*
		* @param key signing key to use
		* @param cert certificate containing corresponding public key
		* @param digestOID digest algorithm OID
		* @param signedAttr table of attributes to be included in signature
		* @param unsignedAttr table of attributes to be included as unsigned
        */
        public void AddSigner(
            AsymmetricKeyParameter	privateKey,
            X509Certificate			cert,
            string					digestOID,
            Asn1.Cms.AttributeTable	signedAttr,
            Asn1.Cms.AttributeTable	unsignedAttr)
        {
			AddSigner(privateKey, cert, CmsSignedHelper.GetEncOid(privateKey, digestOID), digestOID,
				signedAttr, unsignedAttr);
		}

		/**
		 * add a signer, specifying the digest encryption algorithm, with extra signed/unsigned attributes.
		 *
		 * @param key signing key to use
		 * @param cert certificate containing corresponding public key
		 * @param encryptionOID digest encryption algorithm OID
		 * @param digestOID digest algorithm OID
		 * @param signedAttr table of attributes to be included in signature
		 * @param unsignedAttr table of attributes to be included as unsigned
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			X509Certificate			cert,
			string					encryptionOID,
			string					digestOID,
			Asn1.Cms.AttributeTable	signedAttr,
			Asn1.Cms.AttributeTable	unsignedAttr)
		{
			doAddSigner(privateKey, GetSignerIdentifier(cert), encryptionOID, digestOID,
				new DefaultSignedAttributeTableGenerator(signedAttr),
				new SimpleAttributeTableGenerator(unsignedAttr),
				signedAttr);
		}

	    /**
	     * add a signer with extra signed/unsigned attributes.
		 *
		 * @param key signing key to use
		 * @param subjectKeyID subjectKeyID of corresponding public key
		 * @param digestOID digest algorithm OID
		 * @param signedAttr table of attributes to be included in signature
		 * @param unsignedAttr table of attributes to be included as unsigned
	     */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			byte[]					subjectKeyID,
			string					digestOID,
			Asn1.Cms.AttributeTable	signedAttr,
			Asn1.Cms.AttributeTable	unsignedAttr)
		{
			AddSigner(privateKey, subjectKeyID, CmsSignedHelper.GetEncOid(privateKey, digestOID), digestOID,
				signedAttr, unsignedAttr); 
		}

		/**
		 * add a signer, specifying the digest encryption algorithm, with extra signed/unsigned attributes.
		 *
		 * @param key signing key to use
		 * @param subjectKeyID subjectKeyID of corresponding public key
		 * @param encryptionOID digest encryption algorithm OID
		 * @param digestOID digest algorithm OID
		 * @param signedAttr table of attributes to be included in signature
		 * @param unsignedAttr table of attributes to be included as unsigned
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			byte[]					subjectKeyID,
			string					encryptionOID,
			string					digestOID,
			Asn1.Cms.AttributeTable	signedAttr,
			Asn1.Cms.AttributeTable	unsignedAttr)
		{
			doAddSigner(privateKey, GetSignerIdentifier(subjectKeyID), encryptionOID, digestOID,
				new DefaultSignedAttributeTableGenerator(signedAttr),
				new SimpleAttributeTableGenerator(unsignedAttr),
				signedAttr);
		}

		/**
		 * add a signer with extra signed/unsigned attributes based on generators.
		 */
		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			X509Certificate				cert,
			string						digestOID,
			CmsAttributeTableGenerator	signedAttrGen,
			CmsAttributeTableGenerator	unsignedAttrGen)
		{
			AddSigner(privateKey, cert, CmsSignedHelper.GetEncOid(privateKey, digestOID), digestOID,
				signedAttrGen, unsignedAttrGen);
		}

		/**
		 * add a signer, specifying the digest encryption algorithm, with extra signed/unsigned attributes based on generators.
		 */
		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			X509Certificate				cert,
			string						encryptionOID,
			string						digestOID,
			CmsAttributeTableGenerator	signedAttrGen,
			CmsAttributeTableGenerator	unsignedAttrGen)
		{
			doAddSigner(privateKey, GetSignerIdentifier(cert), encryptionOID, digestOID, signedAttrGen,
				unsignedAttrGen, null);
		}

	    /**
	     * add a signer with extra signed/unsigned attributes based on generators.
	     */
	    public void AddSigner(
			AsymmetricKeyParameter		privateKey,
	        byte[]						subjectKeyID,
	        string						digestOID,
	        CmsAttributeTableGenerator	signedAttrGen,
	        CmsAttributeTableGenerator	unsignedAttrGen)
	    {
			AddSigner(privateKey, subjectKeyID, CmsSignedHelper.GetEncOid(privateKey, digestOID), digestOID,
				signedAttrGen, unsignedAttrGen);
	    }

		/**
		 * add a signer, including digest encryption algorithm, with extra signed/unsigned attributes based on generators.
		 */
		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			byte[]						subjectKeyID,
			string						encryptionOID,
			string						digestOID,
			CmsAttributeTableGenerator	signedAttrGen,
			CmsAttributeTableGenerator	unsignedAttrGen)
		{
			doAddSigner(privateKey, GetSignerIdentifier(subjectKeyID), encryptionOID, digestOID,
				signedAttrGen, unsignedAttrGen, null);
		}

        public void AddSignerInfoGenerator(SignerInfoGenerator signerInfoGenerator)
        {
            signerInfs.Add(new SignerInf(this, signerInfoGenerator.contentSigner, signerInfoGenerator.sigId,
                            signerInfoGenerator.signedGen, signerInfoGenerator.unsignedGen, null));
        }

        private void doAddSigner(
			AsymmetricKeyParameter		privateKey,
			SignerIdentifier            signerIdentifier,
			string                      encryptionOID,
			string                      digestOID,
			CmsAttributeTableGenerator  signedAttrGen,
			CmsAttributeTableGenerator  unsignedAttrGen,
			Asn1.Cms.AttributeTable		baseSignedTable)
		{
			signerInfs.Add(new SignerInf(this, privateKey, m_random, signerIdentifier, digestOID, encryptionOID,
				signedAttrGen, unsignedAttrGen, baseSignedTable));
		}

		/**
        * generate a signed object that for a CMS Signed Data object
        */
        public CmsSignedData Generate(
            CmsProcessable content)
        {
            return Generate(content, false);
        }

        /**
        * generate a signed object that for a CMS Signed Data
        * object  - if encapsulate is true a copy
        * of the message will be included in the signature. The content type
        * is set according to the OID represented by the string signedContentType.
        */
        public CmsSignedData Generate(
            string			signedContentType,
			// FIXME Avoid accessing more than once to support CmsProcessableInputStream
            CmsProcessable	content,
            bool			encapsulate)
        {
            Asn1EncodableVector digestAlgs = new Asn1EncodableVector();
            Asn1EncodableVector signerInfos = new Asn1EncodableVector();

			m_digests.Clear(); // clear the current preserved digest state

			//
            // add the precalculated SignerInfo objects.
            //
            foreach (SignerInformation signer in _signers)
            {
                CmsUtilities.AddDigestAlgs(digestAlgs, signer, DefaultDigestAlgorithmFinder.Instance);
                // TODO Verify the content type and calculated digest match the precalculated SignerInfo
                signerInfos.Add(signer.ToSignerInfo());
            }

			//
            // add the SignerInfo objects
            //
            bool isCounterSignature = (signedContentType == null);

            DerObjectIdentifier contentTypeOid = isCounterSignature
                ?   null
				:	new DerObjectIdentifier(signedContentType);

            foreach (SignerInf signer in signerInfs)
            {
				try
                {
					digestAlgs.Add(signer.DigestAlgorithmID);
                    signerInfos.Add(signer.ToSignerInfo(contentTypeOid, content));
				}
                catch (IOException e)
                {
                    throw new CmsException("encoding error.", e);
                }
                catch (InvalidKeyException e)
                {
                    throw new CmsException("key inappropriate for signature.", e);
                }
                catch (SignatureException e)
                {
                    throw new CmsException("error creating signature.", e);
                }
                catch (CertificateEncodingException e)
                {
                    throw new CmsException("error creating sid.", e);
                }
            }

			Asn1Set certificates = null;

			if (_certs.Count != 0)
			{
				certificates = UseDerForCerts
                    ?   CmsUtilities.CreateDerSetFromList(_certs)
                    :   CmsUtilities.CreateBerSetFromList(_certs);
			}

			Asn1Set certrevlist = null;

			if (_crls.Count != 0)
			{
                certrevlist = UseDerForCrls
                    ?   CmsUtilities.CreateDerSetFromList(_crls)
                    :   CmsUtilities.CreateBerSetFromList(_crls);
            }

			Asn1OctetString octs = null;
			if (encapsulate)
            {
                MemoryStream bOut = new MemoryStream();
				if (content != null)
				{
	                try
	                {
	                    content.Write(bOut);
	                }
	                catch (IOException e)
	                {
	                    throw new CmsException("encapsulation error.", e);
	                }
				}
				octs = new BerOctetString(bOut.ToArray());
            }

            ContentInfo encInfo = new ContentInfo(contentTypeOid, octs);

            SignedData sd = new SignedData(
                DerSet.FromVector(digestAlgs),
                encInfo,
                certificates,
                certrevlist,
                DerSet.FromVector(signerInfos));

            ContentInfo contentInfo = new ContentInfo(CmsObjectIdentifiers.SignedData, sd);

            return new CmsSignedData(content, contentInfo);
        }

        /**
        * generate a signed object that for a CMS Signed Data
        * object - if encapsulate is true a copy
        * of the message will be included in the signature with the
        * default content type "data".
        */
        public CmsSignedData Generate(
            CmsProcessable	content,
            bool			encapsulate)
        {
            return this.Generate(Data, content, encapsulate);
        }

		/**
		* generate a set of one or more SignerInformation objects representing counter signatures on
		* the passed in SignerInformation object.
		*
		* @param signer the signer to be countersigned
		* @param sigProvider the provider to be used for counter signing.
		* @return a store containing the signers.
		*/
		public SignerInformationStore GenerateCounterSigners(
			SignerInformation signer)
		{
			return this.Generate(null, new CmsProcessableByteArray(signer.GetSignature()), false).GetSignerInfos();
		}
	}
}