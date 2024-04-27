using CustomLogger;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using CyberBackendLibrary.DataTypes;
using System;
using System.IO;
using System.Linq;

namespace CyberBackendLibrary.SSL
{
    public class SSLUtils
    {
        // PEM file headers.
        public const string CRT_HEADER = "-----BEGIN CERTIFICATE-----\n";
        public const string CRT_FOOTER = "\n-----END CERTIFICATE-----\n";
        public const string PRIVATE_RSA_KEY_HEADER = "-----BEGIN RSA PRIVATE KEY-----\n";
        public const string PRIVATE_RSA_KEY_FOOTER = "\n-----END RSA PRIVATE KEY-----";
        public const string PUBLIC_RSA_KEY_HEADER = "-----BEGIN RSA PUBLIC KEY-----\n";
        public const string PUBLIC_RSA_KEY_FOOTER = "\n-----END RSA PUBLIC KEY-----";
        public const string ENTRUST_NET_CA = "-----BEGIN CERTIFICATE-----\r\n" +
            "MIIEKjCCAxKgAwIBAgIEOGPe+DANBgkqhkiG9w0BAQUFADCBtDEUMBIGA1UEChML\r\n" +
            "RW50cnVzdC5uZXQxQDA+BgNVBAsUN3d3dy5lbnRydXN0Lm5ldC9DUFNfMjA0OCBp\r\n" +
            "bmNvcnAuIGJ5IHJlZi4gKGxpbWl0cyBsaWFiLikxJTAjBgNVBAsTHChjKSAxOTk5\r\n" +
            "IEVudHJ1c3QubmV0IExpbWl0ZWQxMzAxBgNVBAMTKkVudHJ1c3QubmV0IENlcnRp\r\n" +
            "ZmljYXRpb24gQXV0aG9yaXR5ICgyMDQ4KTAeFw05OTEyMjQxNzUwNTFaFw0yOTA3\r\n" +
            "MjQxNDE1MTJaMIG0MRQwEgYDVQQKEwtFbnRydXN0Lm5ldDFAMD4GA1UECxQ3d3d3\r\n" +
            "LmVudHJ1c3QubmV0L0NQU18yMDQ4IGluY29ycC4gYnkgcmVmLiAobGltaXRzIGxp\r\n" +
            "YWIuKTElMCMGA1UECxMcKGMpIDE5OTkgRW50cnVzdC5uZXQgTGltaXRlZDEzMDEG\r\n" +
            "A1UEAxMqRW50cnVzdC5uZXQgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkgKDIwNDgp\r\n" +
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArU1LqRKGsuqjIAcVFmQq\r\n" +
            "K0vRvwtKTY7tgHalZ7d4QMBzQshowNtTK91euHaYNZOLGp18EzoOH1u3Hs/lJBQe\r\n" +
            "sYGpjX24zGtLA/ECDNyrpUAkAH90lKGdCCmziAv1h3edVc3kw37XamSrhRSGlVuX\r\n" +
            "MlBvPci6Zgzj/L24ScF2iUkZ/cCovYmjZy/Gn7xxGWC4LeksyZB2ZnuU4q941mVT\r\n" +
            "XTzWnLLPKQP5L6RQstRIzgUyVYr9smRMDuSYB3Xbf9+5CFVghTAp+XtIpGmG4zU/\r\n" +
            "HoZdenoVve8AjhUiVBcAkCaTvA5JaJG/+EfTnZVCwQ5N328mz8MYIWJmQ3DW1cAH\r\n" +
            "4QIDAQABo0IwQDAOBgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH/BAUwAwEB/zAdBgNV\r\n" +
            "HQ4EFgQUVeSB0RGAvtiJuQijMfmhJAkWuXAwDQYJKoZIhvcNAQEFBQADggEBADub\r\n" +
            "j1abMOdTmXx6eadNl9cZlZD7Bh/KM3xGY4+WZiT6QBshJ8rmcnPyT/4xmf3IDExo\r\n" +
            "U8aAghOY+rat2l098c5u9hURlIIM7j+VrxGrD9cv3h8Dj1csHsm7mhpElesYT6Yf\r\n" +
            "zX1XEC+bBAlahLVu2B064dae0Wx5XnkcFMXj0EyTO2U87d89vqbllRrDtRnDvV5b\r\n" +
            "u/8j72gZyxKTJ1wDLW8w0B62GqzeWvfRqqgnpv55gcR5mTNXuhKwqeBCbJPKVt7+\r\n" +
            "bYQLCIt+jerXmCHG8+c8eS9enNFMFY3h7CI3zJpDC5fcgJCNs2ebb0gIFVbPv/Er\r\n" +
            "fF6adulZkMV8gzURZVE=\r\n" +
            "-----END CERTIFICATE-----\n";

        /// <summary>
        /// Creates a Root CA Cert for chain signed usage.
        /// <para>Creation d'un certificat Root pour usage sur une chaine de certificats.</para>
        /// </summary>
        /// <param name="directoryPath">The output RootCA filename.</param>
        /// <returns>A X509Certificate2.</returns>
        public static X509Certificate2 CreateRootCertificateAuthority(string directoryPath, string CN = "MultiServer Certificate Authority", string OU = "Scientists Department", string O = "MultiServer Corp", string L = "New York", string S = "Northeastern United", string C = "US")
        {
            byte[] certSerialNumber = new byte[16];
            new Random().NextBytes(certSerialNumber);

            // Generate a new RSA key pair
            using RSA rsa = RSA.Create();

            // Create a certificate request with the RSA key pair
            CertificateRequest request = new($"CN={CN}, OU={OU}, O=\"{O}\", L={L}, S={S}, C={C}", rsa, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            // Configure the certificate as CA.
            request.CertificateExtensions.Add(
               new X509BasicConstraintsExtension(true, true, 12, true));

            // Configure the certificate for Digital Signature and Key Encipherment.
            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.KeyCertSign,
                    true));

            X509Certificate2 RootCACertificate = request.Create(
                request.SubjectName,
                new RsaPkcs1SignatureGenerator(rsa),
                new(new DateTime(1980, 1, 1), TimeSpan.Zero),
                new(new DateTime(7980, 1, 1), TimeSpan.Zero),
                certSerialNumber).CopyWithPrivateKey(rsa);

            string PemRootCACertificate = CRT_HEADER + Convert.ToBase64String(RootCACertificate.RawData, Base64FormattingOptions.InsertLineBreaks) + CRT_FOOTER;

            // Export the private key.
            File.WriteAllText(directoryPath + "/MultiServer_rootca_privkey.pem",
                PRIVATE_RSA_KEY_HEADER + Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks) + PRIVATE_RSA_KEY_FOOTER);

            // Export the certificate.
            File.WriteAllText(directoryPath + "/MultiServer_rootca.pem", PemRootCACertificate);

            // Export the certificate in PFX format.
            File.WriteAllBytes(directoryPath + "/MultiServer_rootca.pfx", RootCACertificate.Export(X509ContentType.Pfx, ""));

            rsa.Clear();

            CreateCertificatesTextFile(PemRootCACertificate, directoryPath + "/CERTIFICATES.TXT");

            return RootCACertificate;
        }

        /// <summary>
        /// Creates a Chain CA Cert for chain signed usage.
        /// <para>Creation d'un certificat sur une chaine de certificats issue d'un RootCA.</para>
        /// </summary>
        /// <param name="RootCACertificate">The initial RootCA.</param>
        /// <param name="PFXCertificatePath">The output ChainCA file path.</param>
        /// <returns>A string.</returns>
        public static void CreateChainSignedCert(X509Certificate2 RootCACertificate, string PFXCertificatePath, string certPassword, string[]? DnsList, string CN = "MultiServerCorp.online", string OU = "Scientists Department", string O = "MultiServer Corp", string L = "New York", string S = "Northeastern United", string C = "US", bool wildcard = true)
        {
            RSA? RootCAPrivateKey = RootCACertificate.GetRSAPrivateKey();

            if (RootCAPrivateKey == null)
            {
                LoggerAccessor.LogError("[SSLUtils] - Root Certificate doesn't have a private key, Chain Signed Certificated will not be generated.");
                return;
            }

            byte[] certSerialNumber = new byte[16];
            new Random().NextBytes(certSerialNumber);

            // Generate a new RSA key pair
            using RSA rsa = RSA.Create();

            // Create a certificate request with the RSA key pair
            CertificateRequest request = new($"CN={CN} [{GetRandomInt64(100, 999)}], OU={OU}, O=\"{O}\", L={L}, S={S}, C={C}", rsa, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            // Set additional properties of the certificate
            request.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(false, false, 0, true));

            // Enhanced key usages
            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection {
                            new Oid("1.3.6.1.5.5.7.3.2"), // TLS Client auth
                            new Oid("1.3.6.1.5.5.7.3.1"), // TLS Server auth
                            new Oid("1.3.6.1.5.5.7.3.4"), // Non-TLS Client auth
                            new Oid("1.3.6.1.5.5.7.3.5")  // Non-TLS Server auth
                    },
                    true));

            // Add a Subject Alternative Name (SAN) extension with a wildcard DNS entry
            SubjectAlternativeNameBuilder sanBuilder = new();
            if (DnsList != null) // Some clients do not allow wildcard domains, so we use SAN attributes as a fallback.
            {
                foreach (string str in DnsList)
                {
                    sanBuilder.AddDnsName(str);
                }
            }
            if (wildcard)
            {
                sanBuilder.AddDnsName("*.net");
                sanBuilder.AddDnsName("*.com");
                sanBuilder.AddDnsName("*.fr");
                sanBuilder.AddDnsName("*.it");
                sanBuilder.AddDnsName("*.en");
                sanBuilder.AddDnsName("*.de");
                sanBuilder.AddDnsName("*.ru");
                sanBuilder.AddDnsName("*.online");
            }
            IPAddress PublicServerIP = IPAddress.Parse(TCP_IP.IPUtils.GetPublicIPAddress());
            IPAddress LocalServerIP = TCP_IP.IPUtils.GetLocalIPAddress();
            sanBuilder.AddDnsName(PublicServerIP.ToString());
            sanBuilder.AddIpAddress(PublicServerIP);
            if (PublicServerIP != LocalServerIP)
            {
                sanBuilder.AddDnsName(LocalServerIP.ToString());
                sanBuilder.AddIpAddress(LocalServerIP);
            }
            sanBuilder.AddEmailAddress("MultiServer@gmail.com");
            request.CertificateExtensions.Add(sanBuilder.Build());

            X509Certificate2 ChainSignedCert = request.Create(
                RootCACertificate.IssuerName,
                new RsaPkcs1SignatureGenerator(RootCAPrivateKey),
                new(new DateTime(1980, 1, 1), TimeSpan.Zero),
                new(new DateTime(7980, 1, 1), TimeSpan.Zero),
                certSerialNumber).CopyWithPrivateKey(rsa);

            // Export the private key.
            File.WriteAllText(Path.GetDirectoryName(PFXCertificatePath) + $"/{Path.GetFileNameWithoutExtension(PFXCertificatePath)}_privkey.pem",
                PRIVATE_RSA_KEY_HEADER + Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks) + PRIVATE_RSA_KEY_FOOTER);

            // Export the public key.
            File.WriteAllText(Path.GetDirectoryName(PFXCertificatePath) + $"/{Path.GetFileNameWithoutExtension(PFXCertificatePath)}_pubkey.pem",
                PUBLIC_RSA_KEY_HEADER + Convert.ToBase64String(rsa.ExportRSAPublicKey(), Base64FormattingOptions.InsertLineBreaks) + PUBLIC_RSA_KEY_FOOTER);

            // Export the certificate.
            File.WriteAllText(Path.GetDirectoryName(PFXCertificatePath) + $"/{Path.GetFileNameWithoutExtension(PFXCertificatePath)}.pem",
                CRT_HEADER + Convert.ToBase64String(ChainSignedCert.RawData, Base64FormattingOptions.InsertLineBreaks) + CRT_FOOTER);

            // Export the certificate in PFX format.
            File.WriteAllBytes(PFXCertificatePath, ChainSignedCert.Export(X509ContentType.Pfx, certPassword));

            rsa.Clear();
        }

        /// <summary>
        /// Initiate the certificate generation routine.
        /// <para>Initialise la génération de certificats.</para>
        /// </summary>
        /// <param name="certpath">Output cert path.</param>
        /// <param name="certPassword">Password of the certificate file.</param>
        /// <param name="DnsList">DNS domains to include in the certificate.</param>
        /// <returns>Nothing.</returns>
        public static void InitCerts(string certpath, string certPassword, string[]? DnsList)
        {
            string directoryPath = Path.GetDirectoryName(certpath) ?? Directory.GetCurrentDirectory() + "/static/SSL";

            Directory.CreateDirectory(directoryPath);

            X509Certificate2? RootCACertificate = null;

            if (!File.Exists(directoryPath + "/MultiServer_rootca.pem") || !File.Exists(directoryPath + "/MultiServer_rootca_privkey.pem"))
                RootCACertificate = CreateRootCertificateAuthority(directoryPath);
            else
                RootCACertificate = X509Certificate2.CreateFromPem(File.ReadAllText(directoryPath + "/MultiServer_rootca.pem").ToArray(), File.ReadAllText(directoryPath + "/MultiServer_rootca_privkey.pem").ToArray());

            CreateChainSignedCert(RootCACertificate, certpath, certPassword, DnsList);
        }

        private static long GetRandomInt64(long minValue, long maxValue)
        {
            Random random = new();
            return (long)(((random.Next() << 32) | random.Next()) * (double)(maxValue - minValue) / 0xFFFFFFFFFFFFFFFF) + minValue;
        }

        /// <summary>
        /// Creates a specific CERTIFICATES.TXT file.
        /// <para>Génération d'un fichier CERTIFICATES.TXT.</para>
        /// </summary>
        /// <param name="rootcaSubject">The root CA.</param>
        /// <param name="selfsignedSubject">The self signed CA.</param>
        /// <param name="FileName">The output file.</param>
        /// <returns>Nothing.</returns>
        public static void CreateCertificatesTextFile(string rootcaSubject, string FileName)
        {
            File.WriteAllText(FileName, rootcaSubject + ENTRUST_NET_CA);
        }
    }

    /// <summary>
	/// RSA-MD5, RSA-SHA1, RSA-SHA256, RSA-SHA512 signature generator for X509 certificates.
	/// </summary>
	sealed class RsaPkcs1SignatureGenerator : X509SignatureGenerator
    {
        // Workaround for SHA1 and MD5 ban in .NET 4.7.2 and .NET Core.
        // Ideas used from:
        // https://stackoverflow.com/a/59989889/7600726
        // https://github.com/dotnet/corefx/pull/18344/files/c74f630f38b6f29142c8dc73623fdcb4f7905f87#r112066147
        // https://github.com/dotnet/corefx/blob/5fe5f9aae7b2987adc7082f90712b265bee5eefc/src/System.Security.Cryptography.X509Certificates/tests/CertificateCreation/PrivateKeyAssociationTests.cs#L531-L553
        // https://github.com/dotnet/runtime/blob/89f3a9ef41383bb409b69d1a0f0db910f3ed9a34/src/libraries/System.Security.Cryptography/tests/X509Certificates/CertificateCreation/X509Sha1SignatureGenerators.cs#LL31C38-L31C38

        private readonly X509SignatureGenerator _realRsaGenerator;

        internal RsaPkcs1SignatureGenerator(RSA rsa)
        {
            _realRsaGenerator = CreateForRSA(rsa, RSASignaturePadding.Pkcs1);
        }

        protected override PublicKey BuildPublicKey() => _realRsaGenerator.PublicKey;

        /// <summary>
        /// Callback for .NET signing functions.
        /// </summary>
        /// <param name="hashAlgorithm">Hashing algorithm name.</param>
        /// <returns>Hashing algorithm ID in some correct format.</returns>
        public override byte[] GetSignatureAlgorithmIdentifier(HashAlgorithmName hashAlgorithm)
        {
            /*
			 * https://bugzilla.mozilla.org/show_bug.cgi?id=1064636#c28
				300d06092a864886f70d0101020500  :md2WithRSAEncryption           1
				300b06092a864886f70d01010b      :sha256WithRSAEncryption        2
				300b06092a864886f70d010105      :sha1WithRSAEncryption          1
				300d06092a864886f70d01010c0500  :sha384WithRSAEncryption        20
				300a06082a8648ce3d040303        :ecdsa-with-SHA384              20
				300a06082a8648ce3d040302        :ecdsa-with-SHA256              97
				300d06092a864886f70d0101040500  :md5WithRSAEncryption           6512
				300d06092a864886f70d01010d0500  :sha512WithRSAEncryption        7715
				300d06092a864886f70d01010b0500  :sha256WithRSAEncryption        483338
				300d06092a864886f70d0101050500  :sha1WithRSAEncryption          4498605
			 */

            const string MD5id = "300D06092A864886F70D0101040500";
            const string SHA1id = "300D06092A864886F70D0101050500";
            const string SHA256id = "300D06092A864886F70D01010B0500";
            const string SHA384id = "300D06092A864886F70D01010C0500"; //?
            const string SHA512id = "300D06092A864886F70D01010D0500";

            if (hashAlgorithm == HashAlgorithmName.MD5)
                return DataTypesUtils.HexStringToByteArray(MD5id);
            if (hashAlgorithm == HashAlgorithmName.SHA1)
                return DataTypesUtils.HexStringToByteArray(SHA1id);
            if (hashAlgorithm == HashAlgorithmName.SHA256)
                return DataTypesUtils.HexStringToByteArray(SHA256id);
            if (hashAlgorithm == HashAlgorithmName.SHA384)
                return DataTypesUtils.HexStringToByteArray(SHA384id);
            if (hashAlgorithm == HashAlgorithmName.SHA512)
                return DataTypesUtils.HexStringToByteArray(SHA512id);

            LoggerAccessor.LogError(nameof(hashAlgorithm), "'" + hashAlgorithm + "' is not a supported algorithm at this moment.");

            return Array.Empty<byte>();
        }

        /// <summary>
        /// Sign specified <paramref name="data"/> using specified <paramref name="hashAlgorithm"/>.
        /// </summary>
        /// <returns>X.509 signature for specified data.</returns>
        public override byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm) =>
            _realRsaGenerator.SignData(data, hashAlgorithm);
    }
}
