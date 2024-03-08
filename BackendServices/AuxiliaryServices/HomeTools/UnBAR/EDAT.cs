using System.Numerics;
using HomeTools.Crypto;
using BackendProject.MiscUtils;

namespace HomeTools.UnBAR
{
    internal class EDAT
    {
        public static int STATUS_ERROR_OUPUTFILE_IO = -101;
        public static int STATUS_ERROR_INPUTFILE_IO = -100;
        public static int STATUS_ERROR_HASHTITLEIDNAME = -1;
        public static int STATUS_ERROR_HASHDEVKLIC = -2;
        public static int STATUS_ERROR_MISSINGKEY = -3;
        public static int STATUS_ERROR_HEADERCHECK = -4;
        public static int STATUS_ERROR_DECRYPTING = -5;
        public static int STATUS_ERROR_INCORRECT_FLAGS = -6;
        public static int STATUS_ERROR_INCORRECT_VERSION = -7;
        public static int STATUS_ERROR_NOT_A_SDAT = -8;
        public static int STATUS_OK = 0;
        public static long FLAG_COMPRESSED = 1;
        public static long FLAG_0x02 = 2;
        public static long FLAG_KEYENCRYPTED = 8;
        public static long FLAG_0x10 = 16;
        public static long FLAG_0x20 = 32;
        public static long FLAG_SDAT = 16777216;
        public static long FLAG_DEBUG = 2147483648;
        private static int HEADER_MAX_BLOCKSIZE = 15360;

        public int encryptFile(string inFile, string outFile, string? sdatnpdcopyfile)
        {
            if (string.IsNullOrEmpty(inFile))
                return STATUS_ERROR_INPUTFILE_IO;
            if (string.IsNullOrEmpty(outFile))
                return STATUS_ERROR_OUPUTFILE_IO;
            FileStream fileStream = File.Open(inFile, (FileMode)3);
            NPD[] npdPtr = new NPD[1];
            FileStream o1 = File.Open(outFile, (FileMode)2);
            if (string.IsNullOrEmpty(sdatnpdcopyfile))
            {
                byte[] numArray1 = writeValidNPD(npdPtr, fileStream, "");
                o1.Write(numArray1, 0, numArray1.Length);
            }
            else
            {
                byte[] numArray1 = writeValidNPD(npdPtr, fileStream, sdatnpdcopyfile);
                o1.Write(numArray1, 0, numArray1.Length);
            }
            byte[] numArray2 = new byte[4]
            {
                 1,
                 0,
                 0,
                 60
            };
            o1.Write(numArray2, 0, 4);
            numArray2[0] = 0;
            numArray2[1] = 0;
            numArray2[2] = 64;
            numArray2[3] = 0;
            o1.Write(numArray2, 0, 4);
            long length1 = fileStream.Length;
            byte[] bytes = BitConverter.GetBytes(length1);
            byte[] numArray3 = new byte[8];
            for (int index = 0; index < 8; ++index)
                numArray3[index] = 0;
            for (int index = 0; index < bytes.Length; ++index)
                numArray3[7 - index] = bytes[index];
            o1.Write(numArray3, 0, 8);
            numArray2[0] = 0;
            while (o1.Length < 256L)
                o1.Write(numArray2, 0, 1);
            EDATData data = new();
            data.flags = 16777276L;
            data.blockSize = 16384L;
            data.fileLen = new BigInteger(length1);
            byte[] key1 = getKey(npdPtr[0], data);
            int hashFlag1 = 268435458;
            encryptData(fileStream, o1, npdPtr[0], data, key1);
            o1.Seek(144L, 0);
            AppLoader appLoader = new();
            appLoader.doInit(hashFlag1, 0, 1, new byte[16], new byte[16], key1);
            int num1 = (data.getFlags() & FLAG_SDAT) != 0L ? 32 : 16;
            int num2 = (int)((data.getFileLen() + (BigInteger)data.getBlockSize() - (BigInteger)1) / (BigInteger)data.getBlockSize());
            long num3 = 0;
            int num4 = 256;
            int num5 = num2;
            int len;
            for (long index = num1 * num5; index > 0L; index -= len)
            {
                len = HEADER_MAX_BLOCKSIZE > index ? (int)index : HEADER_MAX_BLOCKSIZE;
                o1.Seek(num4 + num3, 0);
                byte[] i = new byte[len];
                byte[] o2 = new byte[len];
                o1.Read(i, 0, i.Length);
                appLoader.doUpdate(i, 0, o2, 0, len);
                num3 += len;
            }
            byte[] generatedHash1 = new byte[16];
            appLoader.doFinalButGetHash(generatedHash1);
            o1.Seek(144L, 0);
            o1.Write(generatedHash1, 0, generatedHash1.Length);
            o1.Seek(0L, 0);
            byte[] numArray4 = new byte[160];
            byte[] numArray5 = new byte[160];
            o1.Read(numArray4, 0, numArray4.Length);
            AppLoaderReverse appLoaderReverse = new();
            byte[] numArray6 = new byte[16];
            int hashFlag2 = hashFlag1;
            byte[] i1 = numArray4;
            byte[] o3 = numArray5;
            int length2 = numArray4.Length;
            byte[] key2 = new byte[16];
            byte[] iv = new byte[16];
            byte[] hash = key1;
            byte[] generatedHash2 = numArray6;
            appLoaderReverse.doAll(hashFlag2, 0, 1, i1, 0, o3, 0, length2, key2, iv, hash, generatedHash2, 0);
            o1.Seek(160L, 0);
            o1.Write(numArray6, 0, numArray6.Length);
            while (o1.Length < 256L)
                o1.Write(numArray2, 0, 1);
            o1.Close();
            fileStream.Close();
            return STATUS_OK;
        }

        private byte[] createNPDHash2(byte[] klicensee)
        {
            byte[] output = new byte[16];
            ToolsImpl.XOR(output, klicensee, EDATKeys.SDATKEY);
            return output;
        }

        private byte[] writeValidNPD(NPD[] npdPtr, FileStream fin, string sdatFile) // The sdatFile argument allows to copy the NPD of a SDAT to an other, very handy.
        {
            byte[] numArray = new byte[128];
            numArray[0] = 78;
            numArray[1] = 80;
            numArray[2] = 68;
            numArray[3] = 0;
            numArray[4] = 0;
            numArray[5] = 0;
            numArray[6] = 0;
            numArray[7] = 3;
            numArray[8] = 0;
            numArray[9] = 0;
            numArray[10] = 0;
            numArray[11] = 0;
            numArray[12] = 0;
            numArray[13] = 0;
            numArray[14] = 0;
            numArray[15] = 0;
            for (int index = 0; index < 48; ++index)
                numArray[16 + index] = 0;
            byte[] src1;
            byte[] src2;
            byte[] klicensee1;
            if (!string.IsNullOrEmpty(sdatFile))
            {
                src1 = new byte[16];
                byte[] klicensee2 = new byte[16];
                src2 = new byte[16];
                FileStream fileStream = File.Open(sdatFile, (FileMode)3);
                fileStream.Seek(64L, 0);
                fileStream.Read(src1, 0, src1.Length);
                fileStream.Read(src2, 0, src2.Length);
                fileStream.Read(klicensee2, 0, klicensee2.Length);
                klicensee1 = createNPDHash2(klicensee2);
                fileStream.Close();
            }
            else
            {
                src1 = ConversionUtils.getByteArray("B207996ECD0BB1BD5038D8938401885F");
                src2 = ConversionUtils.getByteArray("D313D4EA5E6416C7ACEFB0CDF50F9280");
                klicensee1 = ConversionUtils.getByteArray("CEF7F4089F5451EA1BCFF509A4A71E2E");
            }
            ConversionUtils.arraycopy(src1, 0, numArray, 64L, 16);
            ConversionUtils.arraycopy(src2, 0, numArray, 80L, 16);
            ConversionUtils.arraycopy(createNPDHash2(klicensee1), 0, numArray, 96L, 16);
            for (int index = 0; index < 16; ++index)
                numArray[112 + index] = 0;
            npdPtr[0] = NPD.createNPD(numArray);
            return numArray;
        }

        public int decryptFile(string inFile, string outFile)
        {
            FileStream fileStream = File.Open(inFile, (FileMode)3);
            NPD[] npdPtr = new NPD[1];
            int num1 = validateNPD(npdPtr, fileStream);
            if (num1 < 0)
            {
                fileStream.Close();
                return num1;
            }
            NPD npd = npdPtr[0];
            EDATData edatData = getEDATData(fileStream);
            byte[] key = getKey(npd, edatData);
            if (key == null)
            {
                fileStream.Close();
                return STATUS_ERROR_MISSINGKEY;
            }
            int num2 = checkHeader(key, edatData, npd, fileStream);
            if (num2 < 0)
            {
                fileStream.Close();
                return num2;
            }
            int version = 0;
            if (npd.getVersion() == 1L)
                version = 1;
            else if (npd.getVersion() == 2L)
                version = 2;
            else if (npd.getVersion() == 3L)
                version = 3;
            else if (npd.getVersion() == 4L)
                version = 4;
            FileStream o = File.Open(outFile, (FileMode)2);
            int num3 = decryptData(fileStream, version, o, npd, edatData, key);
            if (num3 < 0)
            {
                fileStream.Close();
                return num3;
            }
            fileStream.Close();
            o.Close();
            return STATUS_OK;
        }

        private int checkHeader(byte[] rifKey, EDATData data, NPD npd, FileStream i)
        {
            i.Seek(0L, 0);
            byte[] numArray = new byte[160];
            byte[] o1 = new byte[160];
            byte[] expectedHash = new byte[16];
            int version = 0;
            switch (npd.getVersion())
            {
                case 0L:
                    if ((data.getFlags() & 2147483646L) != 0L)
                        return STATUS_ERROR_INCORRECT_FLAGS;
                    break;
                case 1L:
                    version = 1;
                    if ((data.getFlags() & 2147483646L) != 0L)
                        return STATUS_ERROR_INCORRECT_FLAGS;
                    break;
                case 2L:
                    version = 2;
                    if ((data.getFlags() & 2130706400L) != 0L)
                        return STATUS_ERROR_INCORRECT_FLAGS;
                    break;
                case 3L:
                    version = 3;
                    if ((data.getFlags() & 2130706368L) != 0L)
                        return STATUS_ERROR_INCORRECT_FLAGS;
                    break;
                case 4L:
                    version = 4;
                    if ((data.getFlags() & 2130706368L) != 0L)
                        return STATUS_ERROR_INCORRECT_FLAGS;
                    break;
                default:
                    return STATUS_ERROR_INCORRECT_VERSION;
            }
            i.Read(numArray, 0, numArray.Length);
            i.Read(expectedHash, 0, expectedHash.Length);
            AppLoader appLoader1 = new();
            int hashFlag = (data.getFlags() & FLAG_KEYENCRYPTED) == 0L ? 2 : 268435458;
            if ((data.getFlags() & FLAG_DEBUG) != 0L)
                hashFlag |= 16777216;
            if (!appLoader1.doAll(hashFlag, version, 1, numArray, 0, o1, 0, numArray.Length, new byte[16], new byte[16], rifKey, expectedHash, 0))
                return STATUS_ERROR_HEADERCHECK;
            if ((data.getFlags() & FLAG_0x20) == 0L)
            {
                AppLoader appLoader2 = new();
                appLoader2.doInit(hashFlag, version, 1, new byte[16], new byte[16], rifKey);
                int num1 = (data.getFlags() & FLAG_COMPRESSED) != 0L ? 32 : 16;
                int num2 = (int)((data.getFileLen() + (BigInteger)data.getBlockSize() - (BigInteger)11) / (BigInteger)data.getBlockSize());
                int num3 = 0;
                int num4 = 256;
                int num5 = num2;
                int len;
                for (long index = num1 * num5; index > 0L; index -= len)
                {
                    len = HEADER_MAX_BLOCKSIZE > index ? (int)index : HEADER_MAX_BLOCKSIZE;
                    i.Seek(num4 + num3, 0);
                    byte[] i1 = new byte[len];
                    byte[] o2 = new byte[len];
                    i.Read(i1, 0, i1.Length);
                    appLoader2.doUpdate(i1, 0, o2, 0, len);
                    num3 += len;
                }
                if (!appLoader2.doFinal(numArray, 144))
                    return STATUS_ERROR_HEADERCHECK;
            }
            return STATUS_OK;
        }

        private byte[] decryptMetadataSection(byte[] metadata) => new byte[16]
        {
          (byte) ( metadata[12] ^ (uint) metadata[8] ^  metadata[16]),
          (byte) ( metadata[13] ^ (uint) metadata[9] ^  metadata[17]),
          (byte) ( metadata[14] ^ (uint) metadata[10] ^  metadata[18]),
          (byte) ( metadata[15] ^ (uint) metadata[11] ^  metadata[19]),
          (byte) ( metadata[4] ^ (uint) metadata[8] ^  metadata[20]),
          (byte) ( metadata[5] ^ (uint) metadata[9] ^  metadata[21]),
          (byte) ( metadata[6] ^ (uint) metadata[10] ^  metadata[22]),
          (byte) ( metadata[7] ^ (uint) metadata[11] ^  metadata[23]),
          (byte) ( metadata[12] ^ (uint) metadata[0] ^  metadata[24]),
          (byte) ( metadata[13] ^ (uint) metadata[1] ^  metadata[25]),
          (byte) ( metadata[14] ^ (uint) metadata[2] ^  metadata[26]),
          (byte) ( metadata[15] ^ (uint) metadata[3] ^  metadata[27]),
          (byte) ( metadata[4] ^ (uint) metadata[0] ^  metadata[28]),
          (byte) ( metadata[5] ^ (uint) metadata[1] ^  metadata[29]),
          (byte) ( metadata[6] ^ (uint) metadata[2] ^  metadata[30]),
          (byte) ( metadata[7] ^ (uint) metadata[3] ^  metadata[31])
        };

        private EDATData getEDATData(FileStream i)
        {
            i.Seek(128L, 0);
            byte[] data = new byte[16];
            i.Read(data, 0, data.Length);
            return EDATData.createEDATData(data);
        }

        private bool compareBytes(byte[] value1, int offset1, byte[] value2, int offset2, int len)
        {
            bool flag = true;
            for (int index = 0; index < len; ++index)
            {
                if (value1[index + offset1] != value2[index + offset2])
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        private int validateNPD(NPD[] npdPtr, FileStream i)
        {
            i.Seek(0L, 0);
            byte[] npd = new byte[128];
            i.Read(npd, 0, npd.Length);
            byte[] buffer = new byte[4];
            i.Read(buffer, 0, buffer.Length);
            if ((ConversionUtils.be32(buffer, 0) & FLAG_SDAT) == 0L)
                return STATUS_ERROR_NOT_A_SDAT;
            npdPtr[0] = NPD.createNPD(npd);
            return STATUS_OK;
        }

        private byte[] getKey(NPD npd, EDATData data)
        {
            byte[] output = new byte[16];
            ToolsImpl.XOR(output, npd.getDevHash(), EDATKeys.SDATKEY);
            return output;
        }

        private int encryptData(FileStream ii, FileStream o, NPD npd, EDATData data, byte[] rifkey)
        {
            int num1 = (int)((data.getFileLen() + (BigInteger)data.getBlockSize() - (BigInteger)1) / (BigInteger)data.getBlockSize());
            int num2 = (data.getFlags() & FLAG_COMPRESSED) != 0L || (data.getFlags() & FLAG_0x20) != 0L ? 32 : 16;
            int num3 = 256;
            for (int blk = 0; blk < num1; ++blk)
            {
                if (blk > 4043309056L / data.getBlockSize())
                    blk = blk + 1 - 1;
                long num4 = blk * data.getBlockSize();
                ii.Seek(num4, 0);
                int num5 = (int)data.getBlockSize();
                if (blk == num1 - 1)
                {
                    num5 = (int)(data.getFileLen() % new BigInteger(data.getBlockSize()));
                    if (num5 == 0)
                        num5 = (int)data.getBlockSize();
                }
                int num6 = num5;
                int length1 = num5 + 15 & -16;
                byte[] numArray1 = new byte[length1];
                byte[] numArray2 = new byte[length1];
                int num7 = num6;
                while (num7 > 0)
                    num7 -= ii.Read(numArray2, num6 - num7, num7);
                for (int index = num6; index < length1; ++index)
                    numArray2[index] = 0;
                byte[] numArray3 = new byte[16];
                byte[] numArray4 = new byte[16];
                byte[] blockKey = calculateBlockKey(blk, npd);
                ToolsImpl.aesecbEncrypt(rifkey, blockKey, 0, numArray3, 0, blockKey.Length);
                if ((data.getFlags() & FLAG_0x10) != 0L)
                    ToolsImpl.aesecbEncrypt(rifkey, numArray3, 0, numArray4, 0, numArray3.Length);
                else
                    ConversionUtils.arraycopy(numArray3, 0, numArray4, 0L, numArray3.Length);
                int num8 = 268435458;
                int num9 = 268435457;
                AppLoaderReverse appLoaderReverse = new();
                byte[] digest = npd.getDigest();
                byte[] numArray5 = new byte[20];
                int hashFlag = num9;
                int cryptoFlag = num8;
                byte[] i = numArray2;
                byte[] o1 = numArray1;
                int length2 = numArray2.Length;
                byte[] key = numArray3;
                byte[] iv = digest;
                byte[] hash = numArray4;
                byte[] generatedHash = numArray5;
                appLoaderReverse.doAll(hashFlag, 3, cryptoFlag, i, 0, o1, 0, length2, key, iv, hash, generatedHash, 0);
                o.Seek(num3 + blk * (num2 + data.getBlockSize()), 0);
                byte[] byteArray = ConversionUtils.getByteArray("555555555555555555555555");
                byte[] output = new byte[16];
                byte[] numArray6 = new byte[16];
                ConversionUtils.arraycopy(numArray5, 16, numArray6, 0L, 4);
                ConversionUtils.arraycopy(byteArray, 0, numArray6, 4L, byteArray.Length);
                ToolsImpl.XOR(output, numArray6, numArray5);
                o.Write(output, 0, output.Length);
                o.Write(numArray6, 0, numArray6.Length);
                o.Write(numArray1, 0, numArray1.Length);
            }
            byte[] byteArray1 = ConversionUtils.getByteArray("534441544120332E332E302E57000000");
            o.Write(byteArray1, 0, byteArray1.Length);
            return STATUS_OK;
        }

        private int decryptData(FileStream ii, int version, FileStream o, NPD npd, EDATData data, byte[] rifkey)
        {
            int num1 = (int)((data.getFileLen() + (BigInteger)data.getBlockSize() - (BigInteger)1) / (BigInteger)data.getBlockSize());
            int num2 = (data.getFlags() & FLAG_COMPRESSED) != 0L || (data.getFlags() & FLAG_0x20) != 0L ? 32 : 16;
            int num3 = 256;
            for (int blk = 0; blk < num1; ++blk)
            {
                ii.Seek(num3 + blk * num2, 0);
                byte[] dest = new byte[20];
                long num4;
                int num5;
                if ((data.getFlags() & FLAG_COMPRESSED) != 0L)
                {
                    byte[] numArray = new byte[32];
                    ii.Read(numArray, 0, numArray.Length);
                    byte[] buffer = decryptMetadataSection(numArray);
                    num4 = (int)ConversionUtils.be64(buffer, 0);
                    num5 = (int)ConversionUtils.be32(buffer, 8);
                    ConversionUtils.be32(buffer, 12);
                    ConversionUtils.arraycopy(numArray, 0, dest, 0L, 16);
                }
                else if ((data.getFlags() & FLAG_0x20) != 0L)
                {
                    byte[] src = new byte[32];
                    ii.Seek(num3 + blk * (num2 + data.getBlockSize()), 0);
                    ii.Read(src, 0, src.Length);
                    for (int index = 0; index < 16; ++index)
                        dest[index] = (byte)(src[index] ^ (uint)src[index + 16]);
                    ConversionUtils.arraycopy(src, 16, dest, 16L, 4);
                    num4 = num3 + blk * (num2 + data.getBlockSize()) + num2;
                    num5 = (int)data.getBlockSize();
                    if (blk == num1 - 1)
                    {
                        int num6 = (int)(data.getFileLen() % new BigInteger(data.getBlockSize()));
                        num5 = num6 > 0 ? num6 : num5;
                    }
                }
                else
                {
                    ii.Read(dest, 0, dest.Length);
                    num4 = num3 + blk * data.getBlockSize() + num1 * num2;
                    num5 = (int)data.getBlockSize();
                    if (blk == num1 - 1)
                        num5 = (int)(data.getFileLen() % new BigInteger(data.getBlockSize()));
                }
                int num7 = num5;
                int length1 = num5 + 15 & -16;
                ii.Seek(num4, 0);
                byte[] numArray1 = new byte[length1];
                byte[] numArray2 = new byte[length1];
                ii.Read(numArray1, 0, numArray1.Length);
                byte[] numArray3 = new byte[16];
                byte[] numArray4 = new byte[16];
                byte[] blockKey = calculateBlockKey(blk, npd);
                ToolsImpl.aesecbEncrypt(rifkey, blockKey, 0, numArray3, 0, blockKey.Length);
                if ((data.getFlags() & FLAG_0x10) != 0L)
                    ToolsImpl.aesecbEncrypt(rifkey, numArray3, 0, numArray4, 0, numArray3.Length);
                else
                    ConversionUtils.arraycopy(numArray3, 0, numArray4, 0L, numArray3.Length);
                int num8 = (data.getFlags() & FLAG_0x02) == 0L ? 2 : 1;
                int num9 = (data.getFlags() & FLAG_0x10) != 0L ? (data.getFlags() & FLAG_0x20) != 0L ? 1 : 4 : 2;
                if ((data.getFlags() & FLAG_KEYENCRYPTED) != 0L)
                {
                    num8 |= 268435456;
                    num9 |= 268435456;
                }
                if ((data.getFlags() & FLAG_DEBUG) != 0L)
                {
                    num8 |= 16777216;
                    num9 |= 16777216;
                }
                AppLoader appLoader = new();
                byte[] numArray5 = npd.getVersion() <= 1L ? new byte[16] : npd.getDigest();
                if (npd.getVersion() > 1L && VariousUtils.AreArraysIdentical(numArray5, new byte[16]))
                    // Unsure about this, but a SDATv2 or higher with empty IV is not documented.
                    // So we assume that we need to disable the cryptography when it's the case.
                    num8 = 1;
                int hashFlag = num9;
                int cryptoFlag = num8;
                byte[] i = numArray1;
                byte[] o1 = numArray2;
                int length2 = numArray1.Length;
                byte[] key = numArray3;
                byte[] iv = numArray5;
                byte[] hash = numArray4;
                byte[] expectedHash = dest;
                appLoader.doAll(hashFlag, version, cryptoFlag, i, 0, o1, 0, length2, key, iv, hash, expectedHash, 0);
                if ((data.getFlags() & FLAG_COMPRESSED) == 0L)
                    o.Write(numArray2, 0, num7);
            }
            return STATUS_OK;
        }

        private byte[] calculateBlockKey(int blk, NPD npd)
        {
            byte[] src = npd.getVersion() <= 1L ? new byte[16] : npd.getDevHash();
            byte[] blockKey = new byte[16];
            byte[] dest = blockKey;
            ConversionUtils.arraycopy(src, 0, dest, 0L, 12);
            blockKey[12] = (byte)(blk >> 24 & byte.MaxValue);
            blockKey[13] = (byte)(blk >> 16 & byte.MaxValue);
            blockKey[14] = (byte)(blk >> 8 & byte.MaxValue);
            blockKey[15] = (byte)(blk & byte.MaxValue);
            return blockKey;
        }
    }
}