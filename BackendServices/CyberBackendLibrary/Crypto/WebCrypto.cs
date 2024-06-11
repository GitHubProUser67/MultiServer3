using CyberBackendLibrary.HTTP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using CastleLibrary.Utils.AES;

namespace CyberBackendLibrary.Crypto
{
    public static class WebCrypto
    {
        public static readonly byte[] AuthIV = new byte[] { 0x30, 0x57, 0xB5, 0x1F, 0x32, 0xD4, 0xAD, 0xBF, 0xAA, 0xAA, 0x21, 0x41, 0x6C, 0xDC, 0x5D, 0xF5 };

        private static readonly float[] ChecksumFloatBox = new float[] {
            58008202f,
            283586167f,
            691630987f,
            0.0121423f,
            24496404f,
            100693.8f,
            11111.00f,
            33743066f,
            11161060f,
            22.609399f,
            8.0000000f,
            656079619f,
            0.0001639f,
            254526166f,
            738776787f,
            455288633f,
            0.0000000f,
            float.MinValue,
            float.MaxValue,
            9.9999999f,
            -66916347f,
            8.001010f,
            24885909f,
            0.0335716f,
            345748357f,
            44845496f,
            217480376f,
            13121294f,
            158692601f,
            32197227f,
            564358.0f,
            0.000000f,
            25984833f,
            float.MaxValue,
            0.000000f,
            306495034f,
            0.0000000f,
            -0.0000000f,
            0.0000003f,
            -685.0607f,
            236381221f,
            0.0000000f,
            48180597f,
            45155118f,
            1976.2049f,
            -19991440f,
            -14114456f,
            0.000004f,
            0.0000000f,
            45522309f,
            2.000000f,
            154946698f,
            -1.0000009f,
            63222279f,
            -65.03404f,
            3.0000000f,
            -1.299990f,
            0.000000f,
            85.476051f,
            48973531f,
            37662912f,
            -0.000000f,
            -19991.000f,
            307838413f,
            904143232f,
            -107104265f,
            213395580f,
            0.000000f,
            0.0000000f,
            18.33077f,
            0.0000000f,
            57067016f,
            14917448f,
            432.62677f,
            99999999f,
            0.000887f,
            -389197792f,
            0.0000000f,
            52872964f,
            0.8713466f,
            363465965f,
            999999990f,
            -0.000000f,
            49137600f,
            73203757f,
            0.000000f,
            99984.100f,
            -530440842f,
            0.0000000f,
            1.1807239f,
            2584606.7f,
            0.000000f,
            0.000000f,
            0.0000000f,
            3383709.5f,
            3.278632f,
            0.000000f,
            0.000000f,
            455318494f,
            -91443337f,
            float.MaxValue,
            778152399f,
            0.0000000f,
            234353870f,
            0.0009164f,
            1119.0000f,
            150841736f,
            9.0880000f,
            155608066f,
            1.5884156f,
            113359872f,
            0.000000f,
            float.MaxValue,
            -0.0000000f,
            99956886f,
            99.000000f,
            111.00000f,
            337012321f,
            12136144f,
            4.783637f,
            106917.5f,
            6.6644000f,
            806013785f,
            16321673f,
            14066396f,
            35364254f,
            38071700f,
            -411691540f,
            0.025415f,
            31909135f,
            13653414f,
            9.1100000f,
            574320387f,
            -28599966f,
            2.0403958f,
            736205017f,
            10.646077f,
            0.0004768f,
            12589274f,
            92538261f,
            0.6502635f,
            -281029210f,
            0.0000000f,
            0.000000f,
            27956324f,
            129829580f,
            0.0000000f,
            81660997f,
            534194995f,
            192919466f,
            367647210f,
            0.55555555f,
            -620646330f,
            520537602f,
            164146190f,
            87026353f,
            810672770f,
            1.0000000f,
            0.0007157f,
            3.000000f,
            10973.481f,
            789490777f,
            0.000000f,
            0.0000122f,
            0.0000027f,
            -0.007104f,
            0.000000f,
            0.0000000f,
            704.64929f,
            13869565f,
            -8.000000f,
            42714203f,
            18517149f,
            0.000000f,
            -44077661f,
            0.000000f,
            56232092f,
            13171912f,
            321896641f,
            79.715774f,
            116500676f,
            33671630f,
            113833607f,
            0.000027f,
            -0.000490f,
            4926.680f,
            249628960f,
            24789044f,
            779834022f,
            -114004686f,
            11813414f,
            39849447f,
            331297423f,
            0.000000f,
            298826107f,
            4.4202899f,
            -0.4316006f,
            685650963f,
            0.000000f,
            114157306f,
            -387315200f,
            0.0000000f,
            288964480f,
            144.59900f,
            0.099100f,
            05614100f,
            0.0000001f,
            20511712f,
            -58915550f,
            19.84004f,
            0.0000000f,
            9.000000f,
            0.000000f,
            771425661f,
            0.000000f,
            1.0000000f,
            1317.196f,
            0.0000014f,
            15941265f,
            49330645f,
            218309042f,
            0.000000f,
            55.40400f,
            36322385f,
            0.0030822f,
            float.MaxValue,
            0.000000f,
            948228673f,
            -0.000000f,
            49617375f,
            28899893f,
            799385305f,
            38902177f,
            525543053f,
            0.006318f,
            32261.566f,
            121926.03f,
            749418696f,
            147206035f,
            92048209f,
            268444598f,
            20088921f,
            float.MinValue,
            20665186f,
            0.0000000f,
            26009441f,
            141566736f,
            0.0000966f,
            -0.0000000f,
            222.0000f,
            -99178045f,
            27448502f,
            0.000000f,
            -9984210f,
            0.0000000f,
            34751204f
        };

        private static readonly byte[] ChecksumByteBox = new byte[] { 244, 255, 193, 198, 73,
            25, 27, 127, 192, 90, 35, 239, 122, 141, 223, 225, 111, 127, 53, 226, 240, 185,
            90, 26, 57, 224, 100, 115, 35, 196, 114, 197, 142, 108, 119, 62, 60, 55, 16, 167,
            73, 53, 169, 40, 105, 223, 222, 166, 178, 14, 131, 5, 9, 148, 181, 37, 215, 123,
            75, 255, 32, 134, 166, 29, 115, 164, 102, 232, 166, 204, 45, 202, 57, 75, 21, 119,
            255, 197, 117, 203, 227, 3, 51, 167, 232, 34, 125, 150, 10, 90, 112, 252, 33, 76,
            41, 216, 1, 65, 153, 8, 186, 201, 230, 193, 150, 240, 75, 181, 160, 15, 69, 214,
            176, 36, 71, 251, 216, 105, 35, 95, 220, 233, 168, 63, 51, 116, 54, 152, 208,
            228, 207, 248, 167, 183, 184, 16, 102, 84, 156, 99, 82, 204, 43, 74, 72, 247,
            134, 63, 26, 91, 190, 240, 10, 245, 244, 23, 100, 143, 35, 130, 51, 81, 57, 120,
            175, 192, 179, 240, 193, 60, 233, 177, 128, 67, 103, 48, 98, 101, 134, 114, 92,
            114, 155, 187, 63, 120, 7, 246, 12, 9, 6, 131, 57, 217, 8, 49, 253, 166, 38, 0,
            242, 163, 128, 236, 2, 210, 86, 54, 63, 124, 252, 5, 175, 108, 65, 216, 106, 240,
            186, 72, 63, 38, 93, 220, 44, 18, 193, 255, 179, 67, 231, 39, 254, 235, 174, 24,
            112, 136, 67, 56, 237, 179, 166, 68, 167, 147, 197, 51, 58, 248, 240, 215, 57, 
            64, 72, 113 };

        public static string? Encrypt(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            return result;
        }

        public static byte[]? EncryptToByteArray(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            if (string.IsNullOrEmpty(result))
                return null;
            else
                return Encoding.UTF8.GetBytes(result);
        }

        public static string? EncryptNoPreserve(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            return result;
        }

        public static byte[]? EncryptNoPreserveToByteArray(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            if (string.IsNullOrEmpty(result))
                return null;
            else
                return Encoding.UTF8.GetBytes(result);
        }

        public static string? Decrypt(string StringToDecrypt, string AccessKey, byte[] IV)
        {
            return Encoding.UTF8.GetString(AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV) ?? Array.Empty<byte>());
        }

        public static string? Decrypt(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return Encoding.UTF8.GetString(AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV) ?? Array.Empty<byte>());
        }

        public static byte[]? DecryptToByteArray(string StringToDecrypt, string AccessKey, byte[] IV)
        {
            return AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }

        public static byte[]? DecryptToByteArray(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }

        public static string? ProcessSecureCheckum(byte[] inputData, long initialValue)
        {
            byte currentByte = 0;
            int DataIndex = 0;
            float tempFloat = (float)(initialValue & 0xFFFFFF | 0x3F000000);
            byte[] CheckSumBytes = new byte[inputData.Length];

            Ionic.Crc.CRC32 crc = new();

            do
            {
                currentByte = (byte)(inputData[DataIndex] ^ BitConverter.SingleToInt32Bits(!BitConverter.IsLittleEndian ? EndianTools.EndianUtils.EndianSwap(tempFloat) : tempFloat));
                CheckSumBytes[DataIndex] = currentByte;
                tempFloat = (ChecksumFloatBox[currentByte]) / 
                            (float)(((uint)(ChecksumByteBox[(uint)tempFloat & 0xFF]) << 0x10) |
                                    ((uint)(ChecksumByteBox[(uint)tempFloat >> 8 & 0xFF]) << 8) |
                                    (uint)(ChecksumByteBox[(uint)tempFloat >> 0x10 & 0xFF]) | 0x3F000000);
                DataIndex++;
            } while (DataIndex != inputData.Length);

            crc.SlurpBlock(CheckSumBytes, 0, CheckSumBytes.Length);

            return $"{crc.Crc32Result:X4}";
        }
    }
}
