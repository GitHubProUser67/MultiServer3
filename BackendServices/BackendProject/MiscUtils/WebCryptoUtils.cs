using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BackendProject.MiscUtils
{
    public static class WebCryptoUtils
    {
        public static readonly byte[] AuthIV = new byte[] { 0x30, 0x57, 0xB5, 0x1F, 0x32, 0xD4, 0xAD, 0xBF, 0xAA, 0xAA, 0x21, 0x41, 0x6C, 0xDC, 0x5D, 0xF5 };

        public static string? Encrypt(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = CryptoUtils.AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverterUtils() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = CryptoUtils.AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverterUtils() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            return result;
        }

        public static byte[]? EncryptToByteArray(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string? result = null;

            if (xmlbody)
                result = CryptoUtils.AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverterUtils() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = CryptoUtils.AESCTR256EncryptDecrypt.InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverterUtils() }
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
            return Encoding.UTF8.GetString(CryptoUtils.AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV));
        }

        public static string? Decrypt(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return Encoding.UTF8.GetString(CryptoUtils.AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV));
        }

        public static byte[]? DecryptToByteArray(string StringToDecrypt, string AccessKey, byte[] IV)
        {
            return CryptoUtils.AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }

        public static byte[]? DecryptToByteArray(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return CryptoUtils.AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }
    }
}
