using CustomLogger;
using System;
using System.Numerics;
using System.Security.Cryptography;

namespace HomeTools.SDAT
{
    public class CryptUtils
    {
        public static int ENCRYPT_MODE = 1;
        public static int DECRYPT_MODE = 2;

        public static void Fail(string a)
        {
            LoggerAccessor.LogError($"[CryptUtils] - Failed with error {a}");
        }

        private static void calculateSubkey(byte[] key, byte[] K1, byte[] K2)
        {
            byte[] numArray1 = new byte[16];
            byte[] numArray2 = new byte[16];
            aesecbEncrypt(key, numArray1, 0, numArray2, 0, numArray1.Length);
            BigInteger bigInteger1 = new(ConversionUtils.reverseByteWithSizeFIX(numArray2));
            BigInteger bigInteger2 = (numArray2[0] & 128) == 0 ? bigInteger1 << 1 : bigInteger1 << 1 ^ new BigInteger(135);
            byte[] src1 = ConversionUtils.reverseByteWithSizeFIX(bigInteger2.ToByteArray());
            if (src1.Length >= 16)
                ConversionUtils.arraycopy(src1, src1.Length - 16, K1, 0L, 16);
            else
            {
                ConversionUtils.arraycopy(numArray1, 0, K1, 0L, numArray1.Length);
                ConversionUtils.arraycopy(src1, 0, K1, 16 - src1.Length, src1.Length);
            }
            bigInteger2 = new BigInteger(ConversionUtils.reverseByteWithSizeFIX(K1));
            byte[] src2 = ConversionUtils.reverseByteWithSizeFIX(((K1[0] & 128) == 0 ? bigInteger2 << 1 : bigInteger2 << 1 ^ new BigInteger(135)).ToByteArray());
            if (src2.Length >= 16)
                ConversionUtils.arraycopy(src2, src2.Length - 16, K2, 0L, 16);
            else
            {
                ConversionUtils.arraycopy(numArray1, 0, K2, 0L, numArray1.Length);
                ConversionUtils.arraycopy(src2, 0, K2, 16 - src2.Length, src2.Length);
            }
        }

        public static void aesecbDecrypt(
          byte[] key,
          byte[] i,
          int inOffset,
          byte[] o,
          int outOffset,
          int len)
        {
            CipherMode mode = CipherMode.ECB;
            PaddingMode padding = PaddingMode.None;
            int decryptMode = DECRYPT_MODE;
            crypto(key, mode, padding, null, decryptMode, i, inOffset, len, o, outOffset);
        }

        public static void aesecbEncrypt(
          byte[] key,
          byte[] i,
          int inOffset,
          byte[] o,
          int outOffset,
          int len)
        {
            CipherMode mode = CipherMode.ECB;
            PaddingMode padding = PaddingMode.None;
            int encryptMode = ENCRYPT_MODE;
            crypto(key, mode, padding, null, encryptMode, i, inOffset, len, o, outOffset);
        }

        public static void aescbcDecrypt(
          byte[] key,
          byte[] iv,
          byte[] i,
          int inOffset,
          byte[] o,
          int outOffset,
          int len)
        {
            CipherMode mode = CipherMode.CBC;
            PaddingMode padding = PaddingMode.None;
            int decryptMode = DECRYPT_MODE;
            crypto(key, mode, padding, iv, decryptMode, i, inOffset, len, o, outOffset);
        }

        private static void crypto(
          byte[] key,
          CipherMode mode,
          PaddingMode padding,
          byte[] iv,
          int opMode,
          byte[] i,
          int inOffset,
          int len,
          byte[] o,
          int outOffset)
        {
            try
            {
                Aes rijndaelManaged = Aes.Create();
                rijndaelManaged.Padding = padding;
                rijndaelManaged.Mode = mode;
                rijndaelManaged.KeySize = 128;
                rijndaelManaged.BlockSize = 128;
                rijndaelManaged.Key = key;
                if (iv != null)
                    rijndaelManaged.IV = iv;
                byte[] src = null;
                if (opMode == DECRYPT_MODE)
                    src = rijndaelManaged.CreateDecryptor().TransformFinalBlock(i, inOffset, len);
                else if (opMode == ENCRYPT_MODE)
                    src = rijndaelManaged.CreateEncryptor().TransformFinalBlock(i, inOffset, len);
                else
                    Fail("NOT SUPPORTED OPMODE");
                ConversionUtils.arraycopy(src, 0, o, outOffset, len);
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        public static byte[] CMAC128(byte[] key, byte[] i, int inOffset, int len)
        {
            byte[] numArray1 = new byte[16];
            byte[] numArray2 = new byte[16];
            calculateSubkey(key, numArray1, numArray2);
            byte[] numArray3 = new byte[16];
            byte[] numArray4 = new byte[16];
            int srcPos = inOffset;
            int length;
            for (length = len; length > 16; length -= 16)
            {
                ConversionUtils.arraycopy(i, srcPos, numArray3, 0L, 16);
                XOR(numArray3, numArray3, numArray4);
                aesecbEncrypt(key, numArray3, 0, numArray4, 0, numArray3.Length);
                srcPos += 16;
            }
            byte[] numArray5 = new byte[16];
            ConversionUtils.arraycopy(i, srcPos, numArray5, 0L, length);
            if (length == 16)
            {
                XOR(numArray5, numArray5, numArray4);
                XOR(numArray5, numArray5, numArray1);
            }
            else
            {
                numArray5[length] = 128;
                XOR(numArray5, numArray5, numArray4);
                XOR(numArray5, numArray5, numArray2);
            }
            aesecbEncrypt(key, numArray5, 0, numArray4, 0, numArray5.Length);
            return numArray4;
        }

        public static void XOR(byte[] output, byte[] inputA, byte[] inputB)
        {
            for (int index = 0; index < inputA.Length; ++index)
                output[index] = (byte)(inputA[index] ^ (uint)inputB[index]);
        }
    }
}
