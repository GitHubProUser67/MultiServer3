using CustomLogger;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace CryptoSporidium
{
    public class MiscUtils
    {
        public static object? GetValueFromJToken(JToken jToken, string propertyName)
        {
            JToken? valueToken = jToken[propertyName];

            if (valueToken != null)
            {
                if (valueToken.Type == JTokenType.Object || valueToken.Type == JTokenType.Array)
                    return valueToken.ToObject<object>();
                else if (valueToken.Type == JTokenType.Integer)
                    return valueToken.ToObject<int>();
                else if (valueToken.Type == JTokenType.String)
                    return valueToken.ToObject<string>();
                else if (valueToken.Type == JTokenType.Boolean)
                    return valueToken.ToObject<bool>();
                else if (valueToken.Type == JTokenType.Float)
                    return valueToken.ToObject<float>();
            }

            return null;
        }

        public string? ExtractFirst16Characters(string input)
        {
            // Check if the input string is not null and has at least 16 characters
            if (input != null && input.Length >= 16)
                // Use Substring to get the first 16 characters
                return input.Substring(0, 16);
            return null;
        }

        public string GetNanoseconds()
        {
            // C# DateTime only provides up to ticks (100 nanoseconds) resolution
            long ticks = DateTime.Now.Ticks;
            long nanoseconds = (ticks % TimeSpan.TicksPerMillisecond) * 100;

            return nanoseconds.ToString("00000000"); // Pad with zeros to 8 digits
        }

        public string GetCurrentDateTime()
        {
            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = $"{currentDateTime:yyyy-MM-dd HH:mm:ss.fff}{GetNanoseconds()}";
            return formattedDateTime;
        }

        public string GenerateDynamicCacheGuid(string input)
        {
            string md5hash = "";

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(GetCurrentDateTime() + input));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Dispose();
            }

            return md5hash;
        }

        public byte[] endianSwap(byte[] input)
        {
            int length = input.Length;
            byte[] swapped = new byte[length];

            for (int i = 0; i < length; i++)
            {
                swapped[i] = input[length - i - 1];
            }

            return swapped;
        }

        public byte[] HexStringToByteArray(string hex)
        {
            int len = hex.Length;
            byte[] byteArray = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return byteArray;
        }

        public string ByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }

        public byte[] ReadBinaryFile(string filePath, int offset, int length)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(offset, SeekOrigin.Begin);

                byte[] data = new byte[length];
                fs.Read(data, 0, length);

                return data;
            }
        }

        public int FindDataPosInBinary(byte[] data1, byte[] data2)
        {
            for (int i = 0; i < data1.Length - data2.Length + 1; i++)
            {
                bool found = true;
                for (int j = 0; j < data2.Length; j++)
                {
                    if (data1[i + j] != data2[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    return i;
            }

            return -1; // Data2 not found in Data1
        }

        public byte[]? GetRequiredBlocks(byte[] byteArray, int blockSize)
        {
            if (blockSize <= 0)
            {
                LoggerAccessor.LogError("Block size must be greater than zero.");
                return null;
            }

            if (byteArray.Length == 0)
                return new byte[0]; // If the input array is empty, return an empty array.

            // Create a new byte array with the calculated length.
            byte[] result = new byte[blockSize];

            // Copy the required blocks from the input array to the result array.
            Array.Copy(byteArray, result, blockSize);

            return result;
        }

        public byte[] ReverseByteArray(byte[] input)
        {
            byte[] reversedArray = new byte[input.Length];
            int lastIndex = input.Length - 1;

            for (int i = 0; i < input.Length; i++)
            {
                reversedArray[i] = input[lastIndex - i];
            }

            return reversedArray;
        }

        public byte[] Combinebytearay(byte[] first, byte[]? second)
        {
            if (second == null)
                return first;

            byte[] bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public bool FindbyteSequence(byte[] byteArray, byte[] sequenceToFind)
        {
            try
            {
                for (int i = 0; i < byteArray.Length - sequenceToFind.Length + 1; i++)
                {
                    if (byteArray[i] == sequenceToFind[0])
                    {
                        bool found = true;
                        for (int j = 1; j < sequenceToFind.Length; j++)
                        {
                            if (byteArray[i + j] != sequenceToFind[j])
                            {
                                found = false;
                                break;
                            }
                        }

                        if (found)
                            return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server has throw an exception in FindRPCNSequence : {ex}");

                return false;
            }
        }

        public byte[]? CopyBytes(byte[] source, uint size)
        {
            if (source == null)
                return null;

            if (size > source.Length)
            {
                LoggerAccessor.LogError("Size exceeds the length of the source array.");
                return null;
            }

            byte[] result = new byte[size];
            Array.Copy(source, result, (int)size);
            return result;
        }

        public byte[]? GetNumberBytes(byte[]? byteArray, int number)
        {
            // Check if the index is within the bounds of the array
            if (byteArray == null || number < 0 || number >= byteArray.Length)
            {
                LoggerAccessor.LogError("Index is out of range or entry is null");
                return null;
            }

            // Create a new array to store the result bytes
            byte[] resultBytes = new byte[number];

            // Copy the bytes from the original array to the result array
            Array.Copy(byteArray, 0, resultBytes, 0, resultBytes.Length);

            return resultBytes;
        }

        public byte[] TrimArray(byte[] arr)
        {
            int i = arr.Length - 1;
            while (arr[i] == 0) i--;
            byte[] data = new byte[i + 1];
            Array.Copy(arr, data, i + 1);
            return data;
        }

        public byte[] TrimStart(byte[] byteArray, int index)
        {
            if (index >= byteArray.Length)
            {
                // If the index is greater than or equal to the length of the array,
                // return an empty byte array.
                return new byte[0];
            }
            else
            {
                // Create a new byte array starting from the specified index.
                byte[] trimmedArray = new byte[byteArray.Length - index];
                Array.Copy(byteArray, index, trimmedArray, 0, trimmedArray.Length);
                return trimmedArray;
            }
        }

        public byte[]? TrimBytes(byte[] source, uint size)
        {
            if (source == null)
                return null;

            if (size >= source.Length)
                return new byte[0]; // Return an empty array if size is greater than or equal to the source length.

            byte[] result = new byte[source.Length - size];
            Array.Copy(source, size, result, 0, result.Length);
            return result;
        }

        public string TrimString(byte[] str)
        {
            int i = str.Length - 1;
            while (str[i] == 0)
            {
                Array.Resize(ref str, i);
                i -= 1;
            }
            string res = Encoding.ASCII.GetString(str);
            //if (res.ToLower() == "www") return null; Some sites do not work without www
            /* else*/
            return res;
        }

        public byte[]? ConvertSha1StringToByteArray(string sha1String)
        {
            if (sha1String.Length % 2 != 0)
            {
                LoggerAccessor.LogError("Input string length must be even.");
                return null;
            }

            byte[] byteArray = new byte[sha1String.Length / 2];

            for (int i = 0; i < sha1String.Length; i += 2)
            {
                string hexByte = sha1String.Substring(i, 2);
                byteArray[i / 2] = Convert.ToByte(hexByte, 16);
            }

            return byteArray;
        }
    }
}
