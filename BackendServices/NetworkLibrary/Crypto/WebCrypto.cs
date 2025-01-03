using NetworkLibrary.HTTP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkLibrary.Crypto
{
    public static class WebCrypto
    {
        public static readonly byte[] AuthIV = new byte[] { 0x30, 0x57, 0xB5, 0x1F, 0x32, 0xD4, 0xAD, 0xBF, 0xAA, 0xAA, 0x21, 0x41, 0x6C, 0xDC, 0x5D, 0xF5 };

        public static async Task<string> GenerateRandomBase64KeyAsync()
        {
            const string url = "https://www.digitalsanctuary.com/aes-key-generator-free";
            const string startText = "AES-256 Key:";
            const string endText = "You ";
            string content;

            try
            {
#if NET7_0_OR_GREATER
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    // Fetch the webpage content using HttpClient
                    content = await client.GetStringAsync(url).ConfigureAwait(false);
                }
#else
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    // Fetch the webpage content using WebClient
                    content = client.DownloadString(url);
                }
#endif

                // Locate the target text and extract the key
                int startIndex = content.IndexOf(startText);

                if (startIndex != -1)
                {
                    startIndex += startText.Length; // Move past the marker text
                    int endIndex = content.IndexOf(endText, startIndex);

                    if (endIndex != -1)
                    {
                        // Match the key inside <strong>...</strong> tags
                        Match match = new Regex(@"<strong>(.*?)<\/strong>")
                            .Match(content.Substring(startIndex, endIndex - startIndex).Trim());

                        if (match.Success)
                            return match.Groups[1].Value.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[WebCrypto] - GenerateRandomBase64KeyAsync - an exception was thrown while fetching the key:{ex}");
            }

            return null;
        }

        public static string EncryptCBC(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string result = null;

            if (xmlbody)
                result = InitiateCBCEncryptBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = InitiateCBCEncryptBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            return result;
        }

        public static byte[] EncryptToByteArrayCBC(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string result = null;

            if (xmlbody)
                result = InitiateCBCEncryptBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = InitiateCBCEncryptBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
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

        public static string EncryptNoPreserveCBC(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string result = null;

            if (xmlbody)
                result = InitiateCBCEncryptBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = InitiateCBCEncryptBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            return result;
        }

        public static byte[] EncryptNoPreserveToByteArrayCBC(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string result = null;

            if (xmlbody)
                result = InitiateCBCEncryptBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = InitiateCBCEncryptBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
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

        public static string DecryptCBC(string StringToDecrypt, string AccessKey, byte[] IV)
        {
            return Encoding.UTF8.GetString(InitiateCBCDecryptBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV) ?? Array.Empty<byte>());
        }

        public static string DecryptCBC(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return Encoding.UTF8.GetString(InitiateCBCDecryptBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV) ?? Array.Empty<byte>());
        }

        public static byte[] DecryptToByteArrayCBC(string StringToDecrypt, string AccessKey, byte[] IV)
        {
            return InitiateCBCDecryptBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }

        public static byte[] DecryptToByteArrayCBC(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return InitiateCBCDecryptBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }

        public static string EncryptCTR(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string result = null;

            if (xmlbody)
                result = InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            return result;
        }

        public static byte[] EncryptToByteArrayCTR(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string result = null;

            if (xmlbody)
                result = InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects | PreserveReferencesHandling.Arrays,
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
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

        public static string EncryptNoPreserveCTR(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string result = null;

            if (xmlbody)
                result = InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                }), Convert.FromBase64String(AccessKey), IV);

            if (!string.IsNullOrEmpty(result) && xmlsecuretags)
                result = "<Secure>" + result + "</Secure>";

            return result;
        }

        public static byte[] EncryptNoPreserveToByteArrayCTR(object ObjectToEncrypt, string AccessKey, byte[] IV, bool xmlsecuretags = false, bool xmlbody = false)
        {
            string result = null;

            if (xmlbody)
                result = InitiateCTRBufferTobase64String(JsonConvert.DeserializeXmlNode(new JObject(new JProperty("ServerResult", JToken.Parse(JsonConvert.SerializeObject(ObjectToEncrypt, new JsonSerializerSettings
                {
                    Converters = { new JsonIPConverter() }
                })))).ToString(), "Root")?.OuterXml ?? "<Root></Root>", Convert.FromBase64String(AccessKey), IV);
            else
                result = InitiateCTRBufferTobase64String(JsonConvert.SerializeObject(ObjectToEncrypt, Formatting.Indented, new JsonSerializerSettings
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

        public static string DecryptCTR(string StringToDecrypt, string AccessKey, byte[] IV)
        {
            return Encoding.UTF8.GetString(InitiateCTRBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV) ?? Array.Empty<byte>());
        }

        public static string DecryptCTR(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return Encoding.UTF8.GetString(InitiateCTRBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV) ?? Array.Empty<byte>());
        }

        public static byte[] DecryptToByteArrayCTR(string StringToDecrypt, string AccessKey, byte[] IV)
        {
            return InitiateCTRBuffer(Convert.FromBase64String(StringToDecrypt.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }

        public static byte[] DecryptToByteArrayCTR(byte[] ByteArrayToDecrypt, string AccessKey, byte[] IV)
        {
            return InitiateCTRBuffer(Convert.FromBase64String(Encoding.UTF8.GetString(ByteArrayToDecrypt).Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(AccessKey), IV);
        }

        private static byte[] InitiateCBCDecryptBuffer(byte[] FileBytes, byte[] KeyBytes, byte[] m_iv)
        {
            if (KeyBytes.Length >= 16 && m_iv.Length == 16)
            {
                // Create the cipher
                IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CBC/OAEPWITHSHA224ANDMGF1PADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return ciphertextBytes;
            }
            else
                CustomLogger.LoggerAccessor.LogError("[WebCrypto] - InitiateCBCDecryptBuffer - Invalid KeyBytes or IV!");

            return null;
        }

        private static string InitiateCBCDecryptBufferTobase64String(string FileString, byte[] KeyBytes, byte[] m_iv)
        {
            if (KeyBytes.Length >= 16 && m_iv.Length == 16)
                return Convert.ToBase64String(InitiateCBCDecryptBuffer(Encoding.UTF8.GetBytes(FileString), KeyBytes, m_iv));
            else
                CustomLogger.LoggerAccessor.LogError("[WebCrypto] - InitiateCBCDecryptBufferTobase64String - Invalid KeyBytes or IV!");

            return null;
        }

        private static byte[] InitiateCBCEncryptBuffer(byte[] FileBytes, byte[] KeyBytes, byte[] m_iv)
        {
            if (KeyBytes.Length >= 16 && m_iv.Length == 16)
            {
                // Create the cipher
                IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CBC/OAEPWITHSHA224ANDMGF1PADDING");

                cipher.Init(true, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return ciphertextBytes;
            }
            else
                CustomLogger.LoggerAccessor.LogError("[WebCrypto] - InitiateCBCEncryptBuffer - Invalid KeyBytes or IV!");

            return null;
        }

        private static string InitiateCBCEncryptBufferTobase64String(string FileString, byte[] KeyBytes, byte[] m_iv)
        {
            if (KeyBytes.Length >= 16 && m_iv.Length == 16)
                return Convert.ToBase64String(InitiateCBCEncryptBuffer(Encoding.UTF8.GetBytes(FileString), KeyBytes, m_iv));
            else
                CustomLogger.LoggerAccessor.LogError("[WebCrypto] - InitiateCBCEncryptBufferTobase64String - Invalid KeyBytes or IV!");

            return null;
        }

        private static byte[] InitiateCTRBuffer(byte[] FileBytes, byte[] KeyBytes, byte[] m_iv)
        {
            if (KeyBytes.Length >= 16 && m_iv.Length == 16)
            {
                // Create the cipher
                IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/OAEPWITHSHA224ANDMGF1PADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return ciphertextBytes;
            }
            else
                CustomLogger.LoggerAccessor.LogError("[WebCrypto] - InitiateCTRBuffer - Invalid KeyBytes or IV!");

            return null;
        }

        private static string InitiateCTRBufferTobase64String(string FileString, byte[] KeyBytes, byte[] m_iv)
        {
            if (KeyBytes.Length >= 16 && m_iv.Length == 16)
                return Convert.ToBase64String(InitiateCTRBuffer(Encoding.UTF8.GetBytes(FileString), KeyBytes, m_iv));
            else
                CustomLogger.LoggerAccessor.LogError("[WebCrypto] - InitiateCTRBufferTobase64String - Invalid KeyBytes or IV!");

            return null;
        }
    }
}
