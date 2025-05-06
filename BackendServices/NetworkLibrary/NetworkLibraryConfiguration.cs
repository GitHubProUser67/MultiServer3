using CustomLogger;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace NetworkLibrary
{
    public static class NetworkLibraryConfiguration
    {
        public static bool EnableServerIpAutoNegotiation { get; set; } = true;
        public static bool UsePublicIp { get; set; } = true;
        public static bool EnableSNMPReports { get; set; } = false;
        public static string FallbackServerIp { get; set; } = "0.0.0.0";
        public static string SNMPTrapHost { get; set; } = IPAddress.Loopback.ToString();
        public static string SNMPEnterpriseOid { get; set; } = "12345";
        public static HashAlgorithmName SNMPHashAlgorithm { get; set; } = HashAlgorithmName.MD5;
        public static string SNMPUserName { get; set; } = "usr-md5-aes";
        public static string SNMPAuthPassword { get; set; } = "authkey1";
        public static string SNMPPrivatePassword { get; set; } = "privkey1";

        /// <summary>
        /// Tries to load the specified configuration file.
        /// Throws an exception if it fails to find the file.
        /// </summary>
        /// <param name="configPath"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public static void RefreshVariables(string configPath)
        {
            // Make sure the file exists
            if (!File.Exists(configPath))
            {
                LoggerAccessor.LogWarn("Could not find the NetworkLibrary.json file, writing and using server's default.");

                Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

                // Write the JsonObject to a file
                var configObject = new
                {
                    enable_server_ip_auto_negotiation = EnableServerIpAutoNegotiation,
                    use_public_ip = UsePublicIp,
                    fallback_server_ip = FallbackServerIp,
                    snmp = new
                    {
                        enable = EnableSNMPReports,
                        trap_host = SNMPTrapHost,
                        enterprise_oid = SNMPEnterpriseOid,
                        hash_algorithm = SNMPHashAlgorithm.Name,
                        username = SNMPUserName,
                        auth_password = SNMPAuthPassword,
                        private_password = SNMPPrivatePassword,
                    },
                };

                File.WriteAllText(configPath, JsonSerializer.Serialize(configObject, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

                return;
            }

            try
            {
                // Parse the JSON configuration
                using (var doc = JsonDocument.Parse(File.ReadAllText(configPath)))
                {
                    JsonElement config = doc.RootElement;

                    EnableServerIpAutoNegotiation = GetValueOrDefault(config, "enable_server_ip_auto_negotiation", EnableServerIpAutoNegotiation);
                    UsePublicIp = GetValueOrDefault(config, "use_public_ip", UsePublicIp);
                    string tempVerificationIp = GetValueOrDefault(config, "fallback_server_ip", FallbackServerIp);
                    if (IPAddress.TryParse(tempVerificationIp, out _))
                        FallbackServerIp = tempVerificationIp;
                    if (config.TryGetProperty("snmp", out JsonElement snmpElement))
                    {
                        EnableSNMPReports = snmpElement.GetProperty("enable").GetBoolean();
                        SNMPTrapHost = snmpElement.GetProperty("trap_host").GetString();
                        SNMPEnterpriseOid = snmpElement.GetProperty("enterprise_oid").GetString();
                        SNMPHashAlgorithm = new HashAlgorithmName(snmpElement.GetProperty("hash_algorithm").GetString());
                        SNMPUserName = snmpElement.GetProperty("username").GetString();
                        SNMPAuthPassword = snmpElement.GetProperty("auth_password").GetString();
                        SNMPPrivatePassword = snmpElement.GetProperty("private_password").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn($"NetworkLibrary.json file is malformed (exception: {ex}), using server's default.");
            }
        }

        // Helper method to get a value or default value if not present
        private static T GetValueOrDefault<T>(JsonElement config, string propertyName, T defaultValue)
        {
            try
            {
                if (config.TryGetProperty(propertyName, out JsonElement value))
                {
                    T extractedValue = JsonSerializer.Deserialize<T>(value.GetRawText());
                    if (extractedValue == null)
                        return defaultValue;
                    return extractedValue;
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Program] - GetValueOrDefault thrown an exception: {ex}");
            }

            return defaultValue;
        }
    }
}
