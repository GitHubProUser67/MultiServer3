using System;
using System.Numerics;
using System.Security.Cryptography;

namespace HomeTools.PS3_Creator
{
    public class ToolsImpl
    {
        public static int ENCRYPT_MODE = 1;
        public static int DECRYPT_MODE = 2;

        public static void fail(String a)
        {
            CustomLogger.LoggerAccessor.LogError($"[PS3 Creator] - ToolsImpl - fail:{a}");
        }

        public static void aesecbDecrypt(byte[] key, byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
            CipherMode mode = CipherMode.ECB; // "ECB";
            PaddingMode padding = PaddingMode.None; // "NoPadding";
            
            int opMode = DECRYPT_MODE;
            crypto(key, mode, padding, null, opMode, i, inOffset, len, o, outOffset);
        }

        public static void aesecbEncrypt(byte[] key, byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
            CipherMode mode = CipherMode.ECB; //String mode = "ECB";
            PaddingMode padding = PaddingMode.None; // "NoPadding";
            int opMode = ENCRYPT_MODE;
            crypto(key, mode, padding, null, opMode, i, inOffset, len, o, outOffset);
        }

        public static void aescbcDecrypt(byte[] key, byte[] iv, byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
            CipherMode mode = CipherMode.CBC; //String mode = "CBC";
            PaddingMode padding = PaddingMode.None; // "NoPadding";
            int opMode = DECRYPT_MODE;
            crypto(key, mode, padding, iv, opMode, i, inOffset, len, o, outOffset);
        }

        private static void calculateSubkey(byte[] key, byte[] K1, byte[] K2)
        {
            byte[] zero = new byte[0x10];
            byte[] L = new byte[0x10];
            ToolsImpl.aesecbEncrypt(key, zero, 0, L, 0, zero.Length);
            BigInteger aux = new BigInteger(ConversionUtils.reverseByteWithSizeFIX(L));

            if ((L[0] & 0x80) != 0)
            {
                //Case MSB is set
                aux = (aux << 1) ^ (new BigInteger(0x87));
            }
            else
            {
                aux = aux << 1;
            }
            byte[] aux1 = ConversionUtils.reverseByteWithSizeFIX(aux.ToByteArray());
            if (aux1.Length >= 0x10)
            {
                ConversionUtils.arraycopy(aux1, aux1.Length - 0x10, K1, 0, 0x10);
            }
            else
            {
                ConversionUtils.arraycopy(zero, 0, K1, 0, zero.Length);
                ConversionUtils.arraycopy(aux1, 0, K1, 0x10 - aux1.Length, aux1.Length);
            }
            aux = new BigInteger(ConversionUtils.reverseByteWithSizeFIX(K1));

            if ((K1[0] & 0x80) != 0)
            {
                aux = (aux << 1) ^ (new BigInteger(0x87));
            }
            else
            {
                aux = aux << 1;
            }
            aux1 = ConversionUtils.reverseByteWithSizeFIX(aux.ToByteArray());
            if (aux1.Length >= 0x10)
            {
                ConversionUtils.arraycopy(aux1, aux1.Length - 0x10, K2, 0, 0x10);
            }
            else
            {
                ConversionUtils.arraycopy(zero, 0, K2, 0, zero.Length);
                ConversionUtils.arraycopy(aux1, 0, K2, 0x10 - aux1.Length, aux1.Length);
            }
        }

        private static void crypto(byte[] key, CipherMode mode, PaddingMode padding, byte[] iv, int opMode, byte[] i, int inOffset, int len, byte[] o, int outOffset)
        {
            try
            {
                Aes cipher = Aes.Create();
                cipher.Padding = padding;
                cipher.Mode = mode;
                cipher.KeySize = 0x80;
                cipher.BlockSize = 0x80;
                cipher.Key = key;
                if (iv != null)
                    cipher.IV = iv;

                byte[] aux = null;
                if (opMode == DECRYPT_MODE)
                    aux = cipher.CreateDecryptor().TransformFinalBlock(i, inOffset, len);
                else if (opMode == ENCRYPT_MODE)
                    aux = cipher.CreateEncryptor().TransformFinalBlock(i, inOffset, len);
                else
                    fail("NOT SUPPORTED OPMODE");
                ConversionUtils.arraycopy(aux, 0, o, outOffset, len);
            }
            catch (Exception ex)
            {
                fail(ex.Message);
            }
        }

        public static byte[] CMAC128(byte[] key, byte[] i, int inOffset, int len)
        {
            byte[] K1 = new byte[0x10];
            byte[] K2 = new byte[0x10];
            calculateSubkey(key, K1, K2);
            byte[] input = new byte[0x10];
            byte[] previous = new byte[0x10];
            int currentOffset = inOffset;
            int remaining = len;
            while (remaining > 0x10)
            {
                ConversionUtils.arraycopy(i, currentOffset, input, 0, 0x10);
                XOR(input, input, previous);

                aesecbEncrypt(key, input, 0, previous, 0, input.Length);
                currentOffset += 0x10;
                remaining -= 0x10;
            }
            input = new byte[0x10]; //Memset 0
            ConversionUtils.arraycopy(i, currentOffset, input, 0, remaining);
            if (remaining == 0x10)
            {
                XOR(input, input, previous);
                XOR(input, input, K1);
            }
            else
            {
                input[remaining] = (byte)0x80;
                XOR(input, input, previous);
                XOR(input, input, K2);
            }
            aesecbEncrypt(key, input, 0, previous, 0, input.Length);
            return previous;

        }

        public static void XOR(byte[] output, byte[] inputA, byte[] inputB)
        {
            for (int i = 0; i < inputA.Length; i++)
            {
                output[i] = (byte)(inputA[i] ^ inputB[i]);
            }
        }
    }
}
