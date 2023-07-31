namespace PSMultiServer.Addons.CRYPTOSPORIDIUM.UnBAR
{
    internal class AppLoader
    {
        private Decryptor dec;
        private Hash hash;
        private bool hashDebug;

        public bool doAll(
          int hashFlag,
          int cryptoFlag,
          byte[] i,
          int inOffset,
          byte[] o,
          int outOffset,
          int len,
          byte[] key,
          byte[] iv,
          byte[] hash,
          byte[] expectedHash,
          int hashOffset)
        {
            this.doInit(hashFlag, cryptoFlag, key, iv, hash);
            this.doUpdate(i, inOffset, o, outOffset, len);
            return this.doFinal(expectedHash, hashOffset);
        }

        public void doInit(int hashFlag, int cryptoFlag, byte[] key, byte[] iv, byte[] hashKey)
        {
            byte[] numArray1 = new byte[key.Length];
            byte[] numArray2 = new byte[iv.Length];
            byte[] numArray3 = new byte[hashKey.Length];
            this.getCryptoKeys(cryptoFlag, numArray1, numArray2, key, iv);
            this.getHashKeys(hashFlag, numArray3, hashKey);
            this.setDecryptor(cryptoFlag);
            this.setHash(hashFlag);
            this.dec.doInit(numArray1, numArray2);
            this.hash.doInit(numArray3);
        }

        public void doUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
            this.hash.doUpdate(i, inOffset, len);
            this.dec.doUpdate(i, inOffset, o, outOffset, len);
        }

        public bool doFinal(byte[] expectedhash, int hashOffset) => this.hash.doFinal(expectedhash, hashOffset, this.hashDebug);

        public bool doFinalButGetHash(byte[] generatedHash) => this.hash.doFinalButGetHash(generatedHash);

        private void getCryptoKeys(
          int cryptoFlag,
          byte[] calculatedKey,
          byte[] calculatedIV,
          byte[] key,
          byte[] iv)
        {
            switch ((uint)(cryptoFlag & -268435456))
            {
                case 0:
                    ConversionUtils.arraycopy(key, 0, calculatedKey, 0L, calculatedKey.Length);
                    ConversionUtils.arraycopy(iv, 0, calculatedIV, 0L, calculatedIV.Length);
                    break;
                case 268435456:
                    ToolsImpl.aescbcDecrypt(EDATKeys.EDATKEY, EDATKeys.EDATIV, key, 0, calculatedKey, 0, calculatedKey.Length);
                    ConversionUtils.arraycopy(iv, 0, calculatedIV, 0L, calculatedIV.Length);
                    break;
                case 536870912:
                    ConversionUtils.arraycopy(EDATKeys.EDATKEY, 0, calculatedKey, 0L, calculatedKey.Length);
                    ConversionUtils.arraycopy(EDATKeys.EDATIV, 0, calculatedIV, 0L, calculatedIV.Length);
                    break;
                default:
                    throw new Exception("Crypto mode is not valid: Undefined keys calculator");
            }
        }

        private void getHashKeys(int hashFlag, byte[] calculatedHash, byte[] hash)
        {
            switch ((uint)(hashFlag & -268435456))
            {
                case 0:
                    ConversionUtils.arraycopy(hash, 0, calculatedHash, 0L, calculatedHash.Length);
                    break;
                case 268435456:
                    ToolsImpl.aescbcDecrypt(EDATKeys.EDATKEY, EDATKeys.EDATIV, hash, 0, calculatedHash, 0, calculatedHash.Length);
                    break;
                case 536870912:
                    ConversionUtils.arraycopy(EDATKeys.EDATHASH, 0, calculatedHash, 0L, calculatedHash.Length);
                    break;
                default:
                    throw new Exception("Hash mode is not valid: Undefined keys calculator");
            }
        }

        private void setDecryptor(int cryptoFlag)
        {
            switch (cryptoFlag & (int)byte.MaxValue)
            {
                case 1:
                    this.dec = (Decryptor)new NoCrypt();
                    break;
                case 2:
                    this.dec = (Decryptor)new AESCBC128Decrypt();
                    break;
                default:
                    throw new Exception("Crypto mode is not valid: Undefined decryptor");
            }
        }

        private void setHash(int hashFlag)
        {
            switch (hashFlag & (int)byte.MaxValue)
            {
                case 1:
                    this.hash = (Hash)new HMAC();
                    this.hash.setHashLen(20);
                    break;
                case 2:
                    this.hash = (Hash)new CMAC();
                    this.hash.setHashLen(16);
                    break;
                case 4:
                    this.hash = (Hash)new HMAC();
                    this.hash.setHashLen(16);
                    break;
                default:
                    throw new Exception("Hash mode is not valid: Undefined hash algorithm");
            }
            if ((hashFlag & 251658240) == 0)
                return;
            this.hashDebug = true;
        }
    }
}
