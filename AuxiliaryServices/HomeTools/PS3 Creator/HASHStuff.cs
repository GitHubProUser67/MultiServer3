using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace HomeTools.PS3_Creator
{
        abstract class Hash {
            public bool compareBytes(byte[] value1, int offset1, byte[] value2, int offset2, int len)
            {
                for (int i = 0; i < len; i++)
                {
                    if (value1[i + offset1] != value2[i + offset2])
                    {
                        return false;
                    }
                }
                return true;
            }

            public virtual void setHashLen(int len) { }

            public virtual void doInit(byte[] key) { }

            public virtual void doUpdate(byte[] i, int inOffset, int len) { }

            public virtual bool doFinal(byte[] expectedhash, int hashOffset, bool hashDebug) { return false;  }
            public virtual bool doFinalButGetHash(byte[] generatedHash) { return false; }
        }

        class HMAC : Hash {

            private int hashLen;
            private HMACSHA1 mac;
            private byte[] result;

            public override void setHashLen(int len) {
                if (len == 0x10 || len == 0x14) {
                    hashLen = len;
                    // mac.HashSize = len; needed oO?!?!?!
                } else {
                    throw new Exception("Hash len must be 0x10 or 0x14");
                }
            }

            public override void doInit(byte[] key) {
                try {
                    mac = new HMACSHA1(key);
                } catch (Exception ex) {
                    throw ex;
                }
            }

            public override void doUpdate(byte[] i, int inOffset, int len) {
                result = mac.ComputeHash(i, inOffset, len);
            }

            public override bool doFinal(byte[] expectedhash, int hashOffset, bool hashDebug) {
                return (hashDebug || compareBytes(result, 0, expectedhash, hashOffset, hashLen));
            }

            public override bool doFinalButGetHash(byte[] generatedHash)
            {
                ConversionUtils.arraycopy(result, 0, generatedHash, 0, result.Length);
                return true;
            }
        }

        class CMAC : Hash {

            private int hashLen;
            private CMac mac;
            private byte[] result;

            public CMAC() => hashLen = 16;

            public override void setHashLen(int len) => hashLen = len == 16 ? len : throw new Exception("Hash len must be 0x10");

            public override void doInit(byte[] key)
            {
                try
                {
                    mac = new CMac(new Org.BouncyCastle.Crypto.Engines.AesEngine());
                    mac.Init(new KeyParameter(key));

                    result = new byte[hashLen];
                }
                catch
                {
                }
            }

            public override void doUpdate(byte[] i, int inOffset, int len)
            {
                mac.BlockUpdate(i, inOffset, len);
            }

            public override bool doFinal(byte[] expectedhash, int hashOffset, bool hashDebug)
            {
                mac.DoFinal(result, 0);
                return hashDebug || compareBytes(result, 0, expectedhash, hashOffset, hashLen);
            }

            public override bool doFinalButGetHash(byte[] generatedHash)
            {
                mac.DoFinal(result, 0);
                ConversionUtils.arraycopy(result, 0, generatedHash, 0L, result.Length);
                return true;
            }
        }

        abstract class HashGenerator
        {
            public bool compareBytes(byte[] value1, int offset1, byte[] value2, int offset2, int len)
            {
                for (int i = 0; i < len; i++)
                {
                    if (value1[i + offset1] != value2[i + offset2])
                    {
                        return false;
                    }
                }
                return true;
            }

            public virtual void setHashLen(int len) { }

            public virtual void doInit(byte[] key) { }

            public virtual void doUpdate(byte[] i, int inOffset, int len) { }

            public virtual bool doFinal(byte[] generateHash) { return false; }
        }

        class CMACGenerator : HashGenerator
        {
            private int hashLen;
            private CMac mac;
            private byte[] result;

            public CMACGenerator() => hashLen = 16;

            public override void setHashLen(int len) => hashLen = len == 16 ? len : throw new Exception("Hash len must be 0x10");

            public override void doInit(byte[] key)
            {
                try
                {
                    mac = new CMac(new Org.BouncyCastle.Crypto.Engines.AesEngine());
                    mac.Init(new KeyParameter(key));

                    result = new byte[hashLen];
                }
                catch
                {
                }
            }

            public override void doUpdate(byte[] i, int inOffset, int len)
            {
                mac.BlockUpdate(i, inOffset, len);
            }

            public override bool doFinal(byte[] generatedHash)
            {
                mac.DoFinal(result, 0);
                ConversionUtils.arraycopy(result, 0, generatedHash, 0L, result.Length);
                return true;
            }
        }

        class HMACGenerator : HashGenerator
        {

            private int hashLen;
            private HMACSHA1 mac;
            private byte[] result;

            public override void setHashLen(int len)
            {
                if (len == 0x10 || len == 0x14)
                {
                    hashLen = len;
                    // mac.HashSize = len; needed oO?!?!?!
                }
                else
                {
                    throw new Exception("Hash len must be 0x10 or 0x14");
                }
            }

            public override void doInit(byte[] key)
            {
                try
                {
                    mac = new HMACSHA1(key);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public override void doUpdate(byte[] i, int inOffset, int len)
            {
                result = mac.ComputeHash(i, inOffset, len);
            }

            public override bool doFinal(byte[] generatedHash)
            {
                ConversionUtils.arraycopy(result, 0, generatedHash, 0, result.Length);
                return true;
            }
        }

}
