using System;

namespace HomeTools.SDAT
{
    internal class AppLoaderReverse
    {
        private Decryptor dec;
        private HashGenerator hash;

        public bool DoAll(
          int hashFlag,
          int version,
          int cryptoFlag,
          byte[] i,
          int inOffset,
          byte[] o,
          int outOffset,
          int len,
          byte[] key,
          byte[] iv,
          byte[] hash,
          byte[] generatedHash,
          int hashOffset)
        {
            DoInit(hashFlag, version, cryptoFlag, key, iv, hash);
            DoUpdate(i, inOffset, o, outOffset, len);
            return DoFinal(generatedHash);
        }

        public void DoInit(int hashFlag, int version, int cryptoFlag, byte[] key, byte[] iv, byte[] hashKey)
        {
            byte[] numArray1 = new byte[key.Length];
            byte[] numArray2 = new byte[iv.Length];
            byte[] numArray3 = new byte[hashKey.Length];
            GetCryptoKeys(cryptoFlag, version, numArray1, numArray2, key, iv);
            GetHashKeys(hashFlag, version, numArray3, hashKey);
            SetDecryptor(cryptoFlag);
            SetHash(hashFlag);
            dec.DoInit(numArray1, numArray2);
            hash.DoInit(numArray3);
        }

        public void DoUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
            dec.DoUpdate(i, inOffset, o, outOffset, len);
            hash.DoUpdate(o, outOffset, len);
        }

        public bool DoFinal(byte[] generatedHash) => hash.DoFinal(generatedHash);

        private static void GetCryptoKeys(
          int cryptoFlag,
          int version,
          byte[] calculatedKey,
          byte[] calculatedIV,
          byte[] key,
          byte[] iv)
        {
            switch ((uint)(cryptoFlag & -268435456))
            {
                case 0:
                    ConversionUtils.Arraycopy(key, 0, calculatedKey, 0L, calculatedKey.Length);
                    ConversionUtils.Arraycopy(iv, 0, calculatedIV, 0L, calculatedIV.Length);
                    break;
                case 268435456:
                    if (version == 4)
                        CryptUtils.AescbcDecrypt(EDATKeys.EDATKEY1, EDATKeys.EDATIV, key, 0, calculatedKey, 0, calculatedKey.Length);
                    else
                        CryptUtils.AescbcDecrypt(EDATKeys.EDATKEY0, EDATKeys.EDATIV, key, 0, calculatedKey, 0, calculatedKey.Length);
                    ConversionUtils.Arraycopy(iv, 0, calculatedIV, 0L, calculatedIV.Length);
                    break;
                case 536870912:
                    if (version == 4)
                        ConversionUtils.Arraycopy(EDATKeys.EDATKEY1, 0, calculatedKey, 0L, calculatedKey.Length);
                    else
                        ConversionUtils.Arraycopy(EDATKeys.EDATKEY0, 0, calculatedKey, 0L, calculatedKey.Length);
                    ConversionUtils.Arraycopy(EDATKeys.EDATIV, 0, calculatedIV, 0L, calculatedIV.Length);
                    break;
                default:
                    throw new Exception("Crypto mode is not valid: Undefined keys calculator");
            }
        }

        private static void GetHashKeys(int hashFlag, int version, byte[] calculatedHash, byte[] hash)
        {
            switch ((uint)(hashFlag & -268435456))
            {
                case 0:
                    ConversionUtils.Arraycopy(hash, 0, calculatedHash, 0L, calculatedHash.Length);
                    break;
                case 268435456:
                    if (version == 4)
                        CryptUtils.AescbcDecrypt(EDATKeys.EDATKEY1, EDATKeys.EDATIV, hash, 0, calculatedHash, 0, calculatedHash.Length);
                    else
                        CryptUtils.AescbcDecrypt(EDATKeys.EDATKEY0, EDATKeys.EDATIV, hash, 0, calculatedHash, 0, calculatedHash.Length);
                    break;
                case 536870912:
                    if (version == 4)
                        ConversionUtils.Arraycopy(EDATKeys.EDATHASH1, 0, calculatedHash, 0L, calculatedHash.Length);
                    else
                        ConversionUtils.Arraycopy(EDATKeys.EDATHASH0, 0, calculatedHash, 0L, calculatedHash.Length);
                    break;
                default:
                    throw new Exception("Hash mode is not valid: Undefined keys calculator");
            }
        }

        private void SetDecryptor(int cryptoFlag)
        {
            switch (cryptoFlag & byte.MaxValue)
            {
                case 1:
                    dec = new NoCrypt();
                    break;
                case 2:
                    dec = new AESCBC128Encrypt();
                    break;
                default:
                    throw new Exception("Crypto mode is not valid: Undefined decryptor");
            }
        }

        private void SetHash(int hashFlag)
        {
            switch (hashFlag & byte.MaxValue)
            {
                case 1:
                    hash = new HMACGenerator();
                    hash.SetHashLen(20);
                    break;
                case 2:
                    hash = new CMACGenerator();
                    hash.SetHashLen(16);
                    break;
                case 4:
                    hash = new HMACGenerator();
                    hash.SetHashLen(16);
                    break;
                default:
                    throw new Exception("Hash mode is not valid: Undefined hash algorithm");
            }
        }
    }
}