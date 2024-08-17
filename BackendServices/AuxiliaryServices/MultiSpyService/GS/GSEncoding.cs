using System;

namespace MultiSpyService.GS
{
    public class GSEncoding
    {
        public static byte[] GenerateValidationKey()
        {
            long num = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).Ticks / 10000L;
            byte[] array = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                do
                {
                    num = num * 214013L + 2531011L & 127L;
                }
                while (num < 33L || num >= 127L);
                array[i] = (byte)num;
            }
            return array;
        }

        public static byte[] Decode(byte[] key, byte[] validate, byte[] data, long size)
        {
            if (key == null || validate == null || data == null || size < 0L)
            {
                return null;
            }
            return DecodeInternal(key, validate, data, size, null);
        }

        private static byte[] DecodeInternal(byte[] key, byte[] validate, byte[] data, long size, GSEncodingData encoding)
        {
            byte[] array = new byte[261];
            byte[] array2;
            if (encoding == null)
            {
                array2 = array;
            }
            else
            {
                array2 = encoding.EncodingKey;
            }
            if (encoding == null || encoding.Start == 0L)
            {
                data = EncodeInternal(ref array2, ref key, validate, ref data, ref size, ref encoding);
                if (data == null)
                {
                    return null;
                }
            }
            if (encoding == null)
            {
                ProcessUnXOR(ref array2, ref data, size);
                return data;
            }
            if (encoding.Start != 0L)
            {
                byte[] array3 = new byte[size - encoding.Offset];
                Array.ConstrainedCopy(data, (int)encoding.Offset, array3, 0, (int)(size - encoding.Offset));
                long num = ProcessUnXOR(ref array2, ref array3, size - encoding.Offset);
                Array.ConstrainedCopy(array3, 0, data, (int)encoding.Offset, (int)(size - encoding.Offset));
                encoding.Offset += num;
                byte[] array4 = new byte[size - encoding.Start];
                Array.ConstrainedCopy(data, (int)encoding.Start, array4, 0, (int)(size - encoding.Start));
                return array4;
            }
            return null;
        }

        public static byte[] Encode(byte[] key, byte[] validate, byte[] data, long size)
        {
            byte[] array = new byte[size + 23L];
            byte[] array2 = new byte[23];
            if (key == null || validate == null || data == null || size < 0L)
            {
                return null;
            }
            int num = key.Length;
            int num2 = validate.Length;
            int num3 = new Random().Next();
            for (int i = 0; i < array2.Length; i++)
            {
                num3 = num3 * 214013 + 2531011;
                array2[i] = (byte)((num3 ^ key[i % num] ^ validate[i % num2]) % 256);
            }
            array2[0] = 235;
            array2[1] = 0;
            array2[2] = 0;
            array2[8] = 228;
            for (long num4 = size - 1L; num4 >= 0L; num4 -= 1L)
            {
                checked
                {
                    array[(int)(IntPtr)unchecked(array2.Length + num4)] = data[(int)(IntPtr)num4];
                }
            }
            Array.Copy(array2, array, array2.Length);
            size += array2.Length;
            long u = size;
            byte[] array3 = EncodeInternal(key, validate, array, u, null);
            byte[] array4 = new byte[array3.Length + array2.Length];
            Array.Copy(array2, 0, array4, 0, array2.Length);
            Array.Copy(array3, 0, array4, array2.Length, array3.Length);
            return array4;
        }

        private static byte[] EncodeInternal(byte[] key, byte[] validate, byte[] data, long size, GSEncodingData encoding)
        {
            byte[] array = new byte[261];
            byte[] array2;
            if (encoding == null)
            {
                array2 = array;
            }
            else
            {
                array2 = encoding.EncodingKey;
            }
            if (encoding == null || encoding.Start == 0L)
            {
                data = EncodeInternal(ref array2, ref key, validate, ref data, ref size, ref encoding);
                if (data == null)
                {
                    return null;
                }
            }
            if (encoding == null)
            {
                ProcessXOR(ref array2, ref data, size);
                return data;
            }
            if (encoding.Start != 0L)
            {
                byte[] array3 = new byte[size - encoding.Offset];
                Array.ConstrainedCopy(data, (int)encoding.Offset, array3, 0, (int)(size - encoding.Offset));
                long num = ProcessXOR(ref array2, ref array3, size - encoding.Offset);
                Array.ConstrainedCopy(array3, 0, data, (int)encoding.Offset, (int)(size - encoding.Offset));
                encoding.Offset += num;
                byte[] array4 = new byte[size - encoding.Start];
                Array.ConstrainedCopy(data, (int)encoding.Start, array4, 0, (int)(size - encoding.Start));
                return array4;
            }
            return null;
        }

        private static byte[] EncodeInternal(ref byte[] encodingKey, ref byte[] key, byte[] validate, ref byte[] data, ref long size, ref GSEncodingData encoding)
        {
            long num = (data[0] ^ 236) + 2;
            byte[] destinationArray = new byte[8];
            if (size < 1L)
            {
                return null;
            }
            if (size < num)
            {
                return null;
            }
            long num2 = data[(int)checked((IntPtr)unchecked(num - 1L))] ^ 234;
            if (size < num + num2)
            {
                return null;
            }
            Array.Copy(validate, destinationArray, 8);
            byte[] array = new byte[size - num];
            Array.ConstrainedCopy(data, (int)num, array, 0, (int)(size - num));
            EncodeInternal(ref encodingKey, ref key, ref destinationArray, array, num2);
            Array.ConstrainedCopy(array, 0, data, (int)num, (int)(size - num));
            num += num2;
            if (encoding == null)
            {
                byte[] array2 = new byte[size - num];
                Array.ConstrainedCopy(data, (int)num, array2, 0, (int)(size - num));
                data = array2;
                size -= num;
            }
            else
            {
                encoding.Offset = num;
                encoding.Start = num;
            }
            return data;
        }

        private static void EncodeInternal(ref byte[] encodingKey, ref byte[] key, ref byte[] destinationArray, byte[] unk, long unk1)
        {
            long num = key.Length;
            for (long num2 = 0L; num2 <= unk1 - 1L; num2 += 1L)
            {
                checked
                {
                    destinationArray[(int)(IntPtr)(unchecked(key[(int)checked((IntPtr)(num2 % num))] * (ulong)num2) & 7UL)] = (byte)(destinationArray[(int)(IntPtr)(unchecked(key[(int)checked((IntPtr)(num2 % num))] * (ulong)num2) & 7UL)] ^ destinationArray[(int)(IntPtr)(num2 & 7L)] ^ unk[(int)(IntPtr)num2]);
                }
            }
            long num3 = 8L;
            EncodeInternal(ref encodingKey, ref destinationArray, ref num3);
        }

        private static void EncodeInternal(ref byte[] encodingKey, ref byte[] destinationArray, ref long unk)
        {
            long num = 0L;
            long num2 = 0L;
            if (unk < 1L)
            {
                return;
            }
            for (long num3 = 0L; num3 <= 255L; num3 += 1L)
            {
                encodingKey[(int)checked((IntPtr)num3)] = (byte)num3;
            }
            for (long num3 = 255L; num3 >= 0L; num3 -= 1L)
            {
                byte b = (byte)EncodeInternal(encodingKey, num3, destinationArray, unk, ref num, ref num2);
                checked
                {
                    byte b2 = encodingKey[(int)(IntPtr)num3];
                    encodingKey[(int)(IntPtr)num3] = encodingKey[b];
                    encodingKey[b] = b2;
                }
            }
            encodingKey[256] = encodingKey[1];
            encodingKey[257] = encodingKey[3];
            encodingKey[258] = encodingKey[5];
            encodingKey[259] = encodingKey[7];
            encodingKey[260] = encodingKey[(int)checked((IntPtr)(num & 255L))];
        }

        private static long EncodeInternal(byte[] encodingKey, long unk, byte[] destinationArray, long unk1, ref long unk2, ref long unk3)
        {
            long num = 0L;
            long num2 = 1L;
            if (unk == 0L)
            {
                return 0L;
            }
            if (unk > 1L)
            {
                do
                {
                    num2 = (num2 << 1) + 1L;
                }
                while (num2 < unk);
            }
            long num3;
            do
            {
                unk2 = encodingKey[(int)checked((IntPtr)(unk2 & 255L))] + destinationArray[(int)checked((IntPtr)unk3)];
                unk3 += 1L;
                if (unk3 >= unk1)
                {
                    unk3 = 0L;
                    unk2 += unk1;
                }
                num += 1L;
                if (num > 11L)
                {
                    num3 = unk2 & num2 % unk;
                }
                else
                {
                    num3 = unk2 & num2;
                }
            }
            while (num3 > unk);
            return num3;
        }

        private static long ProcessUnXOR(ref byte[] var0002, ref byte[] var0003, long var0005)
        {
            for (long num = 0L; num < var0005; num += 1L)
            {
                checked
                {
                    var0003[(int)(IntPtr)num] = UnXOR(ref var0002, var0003[(int)(IntPtr)num]);
                }
            }
            return var0005;
        }

        private static long ProcessXOR(ref byte[] var0002, ref byte[] var0003, long var0005)
        {
            for (long num = 0L; num < var0005; num += 1L)
            {
                checked
                {
                    var0003[(int)(IntPtr)num] = XOR(ref var0002, var0003[(int)(IntPtr)num]);
                }
            }
            return var0005;
        }

        private static byte UnXOR(ref byte[] data, byte var0003)
        {
            int num = data[256];
            int num2 = data[257];
            int num3 = data[num];
            data[256] = (byte)((num + 1) % 256);
            data[257] = (byte)((num2 + num3) % 256);
            num = data[260];
            num2 = data[257];
            num2 = data[num2];
            num3 = data[num];
            data[num] = (byte)num2;
            num = data[259];
            num2 = data[257];
            num = data[num];
            data[num2] = (byte)num;
            num = data[256];
            num2 = data[259];
            num = data[num];
            data[num2] = (byte)num;
            num = data[256];
            data[num] = (byte)num3;
            num2 = data[258];
            num = data[num3];
            num3 = data[259];
            num2 = (num2 + num) % 256;
            data[258] = (byte)num2;
            num = num2;
            num3 = data[num3];
            num2 = data[257];
            num2 = data[num2];
            num = data[num];
            num3 = (num3 + num2) % 256;
            num2 = data[260];
            num2 = data[num2];
            num3 = (num3 + num2) % 256;
            num2 = data[num3];
            num3 = data[256];
            num3 = data[num3];
            num = (num + num3) % 256;
            num3 = data[num2];
            num2 = data[num];
            data[260] = var0003;
            num3 = (num3 ^ num2 ^ var0003) % 256;
            data[259] = (byte)num3;
            return (byte)num3;
        }

        private static byte XOR(ref byte[] data, byte var0003)
        {
            int num = data[256];
            int num2 = data[257];
            int num3 = data[num];
            data[256] = (byte)((num + 1) % 256);
            data[257] = (byte)((num2 + num3) % 256);
            num = data[260];
            num2 = data[257];
            num2 = data[num2];
            num3 = data[num];
            data[num] = (byte)num2;
            num = data[259];
            num2 = data[257];
            num = data[num];
            data[num2] = (byte)num;
            num = data[256];
            num2 = data[259];
            num = data[num];
            data[num2] = (byte)num;
            num = data[256];
            data[num] = (byte)num3;
            num2 = data[258];
            num = data[num3];
            num3 = data[259];
            num2 = (num2 + num) % 256;
            data[258] = (byte)num2;
            num = num2;
            num3 = data[num3];
            num2 = data[257];
            num2 = data[num2];
            num = data[num];
            num3 = (num3 + num2) % 256;
            num2 = data[260];
            num2 = data[num2];
            num3 = (num3 + num2) % 256;
            num2 = data[num3];
            num3 = data[256];
            num3 = data[num3];
            num = (num + num3) % 256;
            num3 = data[num2];
            num2 = data[num];
            num3 = (num3 ^ num2 ^ var0003) % 256;
            data[260] = (byte)num3;
            data[259] = var0003;
            return (byte)num3;
        }
    }
}
