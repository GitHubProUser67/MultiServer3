using System;
using System.Collections.Generic;
using System.IO;

using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>Generic signature packet.</remarks>
    public class SignaturePacket
        : ContainedPacket
    {
		private int						version;
        private int						signatureType;
        private long					creationTime;
        private long					keyId;
        private PublicKeyAlgorithmTag	keyAlgorithm;
        private HashAlgorithmTag		hashAlgorithm;
        private MPInteger[]				signature;
        private byte[]					fingerprint;
        private SignatureSubpacket[]	hashedData;
        private SignatureSubpacket[]	unhashedData;
		private byte[]					signatureEncoding;

		internal SignaturePacket(BcpgInputStream bcpgIn)
        {
            version = bcpgIn.ReadByte();

			if (version == 3 || version == 2)
            {
//                int l =
                bcpgIn.ReadByte();

				signatureType = bcpgIn.ReadByte();
                creationTime = (((long)bcpgIn.ReadByte() << 24) | ((long)bcpgIn.ReadByte() << 16)
                    | ((long)bcpgIn.ReadByte() << 8) | (uint)bcpgIn.ReadByte()) * 1000L;

				keyId |= (long)bcpgIn.ReadByte() << 56;
                keyId |= (long)bcpgIn.ReadByte() << 48;
                keyId |= (long)bcpgIn.ReadByte() << 40;
                keyId |= (long)bcpgIn.ReadByte() << 32;
                keyId |= (long)bcpgIn.ReadByte() << 24;
                keyId |= (long)bcpgIn.ReadByte() << 16;
                keyId |= (long)bcpgIn.ReadByte() << 8;
                keyId |= (uint)bcpgIn.ReadByte();

				keyAlgorithm = (PublicKeyAlgorithmTag) bcpgIn.ReadByte();
                hashAlgorithm = (HashAlgorithmTag) bcpgIn.ReadByte();
            }
            else if (version == 4)
            {
                signatureType = bcpgIn.ReadByte();
                keyAlgorithm = (PublicKeyAlgorithmTag) bcpgIn.ReadByte();
                hashAlgorithm = (HashAlgorithmTag) bcpgIn.ReadByte();

				int hashedLength = (bcpgIn.ReadByte() << 8) | bcpgIn.ReadByte();
                byte[] hashed = new byte[hashedLength];

				bcpgIn.ReadFully(hashed);

				//
                // read the signature sub packet data.
                //
                SignatureSubpacketsParser sIn = new SignatureSubpacketsParser(
                    new MemoryStream(hashed, false));

                var v = new List<SignatureSubpacket>();

				SignatureSubpacket sub;
				while ((sub = sIn.ReadPacket()) != null)
                {
                    v.Add(sub);
                }

                hashedData = v.ToArray();

				foreach (var p in hashedData)
                {
                    if (p is IssuerKeyId issuerKeyId)
                    {
                        keyId = issuerKeyId.KeyId;
                    }
                    else if (p is SignatureCreationTime sigCreationTime)
                    {
                        creationTime = DateTimeUtilities.DateTimeToUnixMs(sigCreationTime.GetTime());
                    }
                }

				int unhashedLength = (bcpgIn.ReadByte() << 8) | bcpgIn.ReadByte();
                byte[] unhashed = new byte[unhashedLength];

				bcpgIn.ReadFully(unhashed);

				sIn = new SignatureSubpacketsParser(new MemoryStream(unhashed, false));

				v.Clear();

				while ((sub = sIn.ReadPacket()) != null)
                {
                    v.Add(sub);
                }

                unhashedData = v.ToArray();

				foreach (var p in unhashedData)
                {
                    if (p is IssuerKeyId issuerKeyId)
                    {
                        keyId = issuerKeyId.KeyId;
                    }
                }
            }
            else
            {
                Streams.Drain(bcpgIn);

                throw new UnsupportedPacketVersionException("unsupported version: " + version);
            }

			fingerprint = new byte[2];
            bcpgIn.ReadFully(fingerprint);

			switch (keyAlgorithm)
            {
            case PublicKeyAlgorithmTag.RsaGeneral:
            case PublicKeyAlgorithmTag.RsaSign:
                MPInteger v = new MPInteger(bcpgIn);
				signature = new MPInteger[1]{ v };
                break;
			case PublicKeyAlgorithmTag.Dsa:
                MPInteger r = new MPInteger(bcpgIn);
                MPInteger s = new MPInteger(bcpgIn);
				signature = new MPInteger[2]{ r, s };
                break;
            case PublicKeyAlgorithmTag.ElGamalEncrypt: // yep, this really does happen sometimes.
            case PublicKeyAlgorithmTag.ElGamalGeneral:
                MPInteger p = new MPInteger(bcpgIn);
                MPInteger g = new MPInteger(bcpgIn);
                MPInteger y = new MPInteger(bcpgIn);
				signature = new MPInteger[3]{ p, g, y };
                break;
            case PublicKeyAlgorithmTag.ECDsa:
            case PublicKeyAlgorithmTag.EdDsa_Legacy:
                MPInteger ecR = new MPInteger(bcpgIn);
                MPInteger ecS = new MPInteger(bcpgIn);
                signature = new MPInteger[2]{ ecR, ecS };
                break;
            default:
				if (keyAlgorithm < PublicKeyAlgorithmTag.Experimental_1 || keyAlgorithm > PublicKeyAlgorithmTag.Experimental_11)
                    throw new IOException("unknown signature key algorithm: " + keyAlgorithm);

                signature = null;
                signatureEncoding = Streams.ReadAll(bcpgIn);
				break;
            }
        }

		/**
        * Generate a version 4 signature packet.
        *
        * @param signatureType
        * @param keyAlgorithm
        * @param hashAlgorithm
        * @param hashedData
        * @param unhashedData
        * @param fingerprint
        * @param signature
        */
        public SignaturePacket(
            int						signatureType,
            long					keyId,
            PublicKeyAlgorithmTag	keyAlgorithm,
            HashAlgorithmTag		hashAlgorithm,
            SignatureSubpacket[]	hashedData,
            SignatureSubpacket[]	unhashedData,
            byte[]					fingerprint,
            MPInteger[]				signature)
            : this(4, signatureType, keyId, keyAlgorithm, hashAlgorithm, hashedData, unhashedData, fingerprint, signature)
        {
        }

		/**
        * Generate a version 2/3 signature packet.
        *
        * @param signatureType
        * @param keyAlgorithm
        * @param hashAlgorithm
        * @param fingerprint
        * @param signature
        */
        public SignaturePacket(
            int						version,
            int						signatureType,
            long					keyId,
            PublicKeyAlgorithmTag	keyAlgorithm,
            HashAlgorithmTag		hashAlgorithm,
            long					creationTime,
            byte[]					fingerprint,
            MPInteger[]				signature)
            : this(version, signatureType, keyId, keyAlgorithm, hashAlgorithm, null, null, fingerprint, signature)
        {
			this.creationTime = creationTime;
        }

		public SignaturePacket(
            int						version,
            int						signatureType,
            long					keyId,
            PublicKeyAlgorithmTag	keyAlgorithm,
            HashAlgorithmTag		hashAlgorithm,
            SignatureSubpacket[]	hashedData,
            SignatureSubpacket[]	unhashedData,
            byte[]					fingerprint,
            MPInteger[]				signature)
        {
            this.version = version;
            this.signatureType = signatureType;
            this.keyId = keyId;
            this.keyAlgorithm = keyAlgorithm;
            this.hashAlgorithm = hashAlgorithm;
            this.hashedData = hashedData;
            this.unhashedData = unhashedData;
            this.fingerprint = fingerprint;
            this.signature = signature;

			if (hashedData != null)
			{
				SetCreationTime();
			}
		}

		public int Version => version;

		public int SignatureType => signatureType;

        /**
        * return the keyId
        * @return the keyId that created the signature.
        */
        public long KeyId => keyId;

        /**
         * Return the signatures fingerprint.
         * @return fingerprint (digest prefix) of the signature
         */
        public byte[] GetFingerprint()
        {
            return Arrays.Clone(fingerprint);
        }

		/**
        * return the signature trailer that must be included with the data
        * to reconstruct the signature
        *
        * @return byte[]
        */
        public byte[] GetSignatureTrailer()
        {
			if (version == 3)
            {
                long time = creationTime / 1000L;

                byte[] trailer = new byte[5];
				trailer[0] = (byte)signatureType;
                Pack.UInt32_To_BE((uint)time, trailer, 1);
                return trailer;
            }

            MemoryStream sOut = new MemoryStream();

			sOut.WriteByte((byte)Version);
            sOut.WriteByte((byte)SignatureType);
            sOut.WriteByte((byte)KeyAlgorithm);
            sOut.WriteByte((byte)HashAlgorithm);

            // Mark position an reserve two bytes for length
            long lengthPosition = sOut.Position;
            sOut.WriteByte(0x00);
            sOut.WriteByte(0x00);

            SignatureSubpacket[] hashed = GetHashedSubPackets();
			for (int i = 0; i != hashed.Length; i++)
            {
                hashed[i].Encode(sOut);
            }

            ushort dataLength = Convert.ToUInt16(sOut.Position - lengthPosition - 2);
            uint hDataLength = Convert.ToUInt32(sOut.Position);

			sOut.WriteByte((byte)Version);
            sOut.WriteByte(0xff);
            sOut.WriteByte((byte)(hDataLength >> 24));
            sOut.WriteByte((byte)(hDataLength >> 16));
            sOut.WriteByte((byte)(hDataLength >>  8));
            sOut.WriteByte((byte)(hDataLength      ));

            // Reset position and fill in length
            sOut.Position = lengthPosition;
            sOut.WriteByte((byte)(dataLength >> 8));
            sOut.WriteByte((byte)(dataLength     ));

            return sOut.ToArray();
        }

		public PublicKeyAlgorithmTag KeyAlgorithm => keyAlgorithm;

        public HashAlgorithmTag HashAlgorithm => hashAlgorithm;

        /**
		* return the signature as a set of integers - note this is normalised to be the
        * ASN.1 encoding of what appears in the signature packet.
        */
        public MPInteger[] GetSignature() => signature;

		/**
		 * Return the byte encoding of the signature section.
		 * @return uninterpreted signature bytes.
		 */
		public byte[] GetSignatureBytes()
		{
			if (signatureEncoding != null)
				return (byte[])signatureEncoding.Clone();

			MemoryStream bOut = new MemoryStream();

            using (var pOut = new BcpgOutputStream(bOut))
            {
                foreach (MPInteger sigObj in signature)
                {
                    try
                    {
                        pOut.WriteObject(sigObj);
                    }
                    catch (IOException e)
                    {
                        throw new Exception("internal error: " + e);
                    }
                }
            }

            return bOut.ToArray();
		}

		public SignatureSubpacket[] GetHashedSubPackets() => hashedData;

		public SignatureSubpacket[] GetUnhashedSubPackets() => unhashedData;

		/// <summary>Return the creation time in milliseconds since 1 Jan., 1970 UTC.</summary>
        public long CreationTime => creationTime;

		public override void Encode(BcpgOutputStream bcpgOut)
        {
            MemoryStream bOut = new MemoryStream();
            using (var pOut = new BcpgOutputStream(bOut))
            {
                pOut.WriteByte((byte)version);

                if (version == 3 || version == 2)
                {
                    byte nextBlockLength = 5;
                    pOut.Write(nextBlockLength, (byte)signatureType);
                    pOut.WriteInt((int)(creationTime / 1000L));
                    pOut.WriteLong(keyId);
                    pOut.Write((byte)keyAlgorithm, (byte)hashAlgorithm);
                }
                else if (version == 4)
                {
                    pOut.Write((byte)signatureType, (byte)keyAlgorithm, (byte)hashAlgorithm);
                    EncodeLengthAndData(pOut, GetEncodedSubpackets(hashedData));
                    EncodeLengthAndData(pOut, GetEncodedSubpackets(unhashedData));
                }
                else
                {
                    throw new IOException("unknown version: " + version);
                }

                pOut.Write(fingerprint);

                if (signature != null)
                {
                    pOut.WriteObjects(signature);
                }
                else
                {
                    pOut.Write(signatureEncoding);
                }
            }

			bcpgOut.WritePacket(PacketTag.Signature, bOut.ToArray());
        }

		private static void EncodeLengthAndData(
			BcpgOutputStream	pOut,
			byte[]				data)
		{
			pOut.WriteShort((short) data.Length);
			pOut.Write(data);
		}

		private static byte[] GetEncodedSubpackets(
			SignatureSubpacket[] ps)
		{
			MemoryStream sOut = new MemoryStream();

			foreach (SignatureSubpacket p in ps)
			{
				p.Encode(sOut);
			}

			return sOut.ToArray();
		}

		private void SetCreationTime()
		{
			foreach (SignatureSubpacket p in hashedData)
			{
				if (p is SignatureCreationTime signatureCreationTime)
				{
                    creationTime = DateTimeUtilities.DateTimeToUnixMs(signatureCreationTime.GetTime());
					break;
				}
			}
		}

        public static SignaturePacket FromByteArray(byte[] data)
        {
            BcpgInputStream input = BcpgInputStream.Wrap(new MemoryStream(data));

            return new SignaturePacket(input);
        }
    }
}
