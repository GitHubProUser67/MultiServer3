using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PSMultiServer.PoodleHTTP
{
    /// <summary>
    /// Utilities for manipulating TLS/SSL Certificates and Keys.
    /// </summary>
    public class HTTPSCertificateGenerator
    {
        private static DateTimeOffset currentdatetime = DateTimeOffset.Now;

        // Based on ideas from:
        // https://github.com/wheever/ProxHTTPSProxyMII/blob/master/CertTool.py#L58
        // https://github.com/rwatjen/AzureIoTDPSCertificates/blob/master/src/DPSCertificateTool/CertificateUtil.cs#L46

        public const string DefaultCASubject = "C=SU, O=PoodleHTTP, OU=This is not really secure connection, CN=MultiServer Certificate Authority";

        /// <summary>
        /// Create a self-signed SSL certificate and private key, and save them to CER files.
        /// </summary>
        /// <param name="directory">Path of the certificates.</param>
        /// <param name="certSubject">Certificate subject.</param>
        /// <param name="certHashAlgorithm">Certificate hash algorithm.</param>
        public static void MakeSelfSignedCert(string directory, string certSubject, HashAlgorithmName certHashAlgorithm)
        {
            Directory.CreateDirectory(directory);

            // PEM file headers.
            const string CRT_HEADER = "-----BEGIN CERTIFICATE-----\n";
            const string CRT_FOOTER = "\n-----END CERTIFICATE-----";

            const string KEY_HEADER = "-----BEGIN RSA PRIVATE KEY-----\n";
            const string KEY_FOOTER = "\n-----END RSA PRIVATE KEY-----";

            // Append unique ID of certificate in its CN if it's default.
            // This prevents "sec_error_bad_signature" error in Firefox.
            if (certSubject == DefaultCASubject) certSubject += " [" + new Random().NextInt64(100, 999) + "]";

            // Set up a certificate creation request.
            using RSA rsa = RSA.Create();
            CertificateRequest certRequest = new(certSubject, rsa, certHashAlgorithm, RSASignaturePadding.Pkcs1);

            // Configure the certificate as CA.
            certRequest.CertificateExtensions.Add(
                   new X509BasicConstraintsExtension(true, true, 12, true));

            // Configure the certificate for Digital Signature and Key Encipherment.
            certRequest.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.KeyCertSign,
                    true));

            // Issue & self-sign the certificate.
            X509Certificate2 certificate;
            byte[] certSerialNumber = new byte[16];
            new Random().NextBytes(certSerialNumber);

            X500DistinguishedName certName = new(certSubject);
            RsaPkcs1SignatureGenerator customSignatureGenerator = new(rsa);
            certificate = certRequest.Create(
                certName,
                customSignatureGenerator,
                currentdatetime,
                currentdatetime.AddDays(21900),
                certSerialNumber);

            // Export the private key.
            string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(directory + "/RootCA.key", KEY_HEADER + privateKey + KEY_FOOTER);

            // Export the certificate.
            byte[] exportData = certificate.Export(X509ContentType.Cert);
            string crt = Convert.ToBase64String(exportData, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(directory + "/RootCA.cer", CRT_HEADER + crt + CRT_FOOTER);

            byte[] certData = certificate.Export(X509ContentType.Pfx);
            File.WriteAllBytes(directory + "/RootCA.pfx", certData);

            X509Certificate2 RootCertificate = new X509Certificate2(X509Certificate2.CreateFromPemFile(directory + "/RootCA.cer", directory + "/RootCA.key").Export(X509ContentType.Pkcs12));

            X509Certificate2 ComCertificate = MakeChainSignedCert("CN=" + "*.com", RootCertificate);

            exportData = ComCertificate.Export(X509ContentType.Cert);
            crt = Convert.ToBase64String(exportData, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(directory + "/com.cer", CRT_HEADER + crt + CRT_FOOTER);

            X509Certificate2 NetCertificate = MakeChainSignedCert("CN=" + "*.net", RootCertificate);

            exportData = NetCertificate.Export(X509ContentType.Cert);
            crt = Convert.ToBase64String(exportData, Base64FormattingOptions.InsertLineBreaks);
            File.WriteAllText(directory + "/net.cer", CRT_HEADER + crt + CRT_FOOTER);
        }

        /// <summary>
        /// Issue a chain-signed SSL certificate with private key.
        /// </summary>
        /// <param name="certSubject">Certificate subject (domain name).</param>
        /// <param name="issuerCertificate">Authority's certificate used to sign this certificate.</param>
        /// <param name="certHashAlgorithm">Certificate hash algorithm.</param>
        /// <returns>Signed chain of SSL Certificates.</returns>
        public static X509Certificate2 MakeChainSignedCert(string certSubject, X509Certificate2 issuerCertificate)
        {
            // If not, initialize private key generator & set up a certificate creation request.
            using RSA rsa = RSA.Create();

            // Generate an unique serial number.
            byte[] certSerialNumber = new byte[16];
            new Random().NextBytes(certSerialNumber);

            // Issue & sign the certificate.
            X509Certificate2 certificate;
            // strange, RsaPkcs1SignatureGenerator gives a "sec_error_bad_signature", so use .NET signature generator & SHA256 in some cases.
            // set up a certificate creation request.
            CertificateRequest certRequestSha256 = new(certSubject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            certificate = certRequestSha256.Create(
                issuerCertificate,
                currentdatetime,
                currentdatetime.AddDays(21900),
                certSerialNumber
            );

            // Export the issued certificate with private key.
            X509Certificate2 certificateWithKey = new(certificate.CopyWithPrivateKey(rsa).Export(X509ContentType.Pkcs12));

            return certificateWithKey;
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
            _realRsaGenerator = X509SignatureGenerator.CreateForRSA(rsa, RSASignaturePadding.Pkcs1);
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
                return HexToByteArray(MD5id);
            if (hashAlgorithm == HashAlgorithmName.SHA1)
                return HexToByteArray(SHA1id);
            if (hashAlgorithm == HashAlgorithmName.SHA256)
                return HexToByteArray(SHA256id);
            if (hashAlgorithm == HashAlgorithmName.SHA384)
                return HexToByteArray(SHA384id);
            if (hashAlgorithm == HashAlgorithmName.SHA512)
                return HexToByteArray(SHA512id);

            throw new ArgumentOutOfRangeException(nameof(hashAlgorithm), "'" + hashAlgorithm + "' is not a supported algorithm at this moment.");
        }

        /// <summary>
        /// Convert a hex-formatted string to byte array.
        /// </summary>
        /// <param name="hex">A string looking like "300D06092A864886F70D0101050500".</param>
        /// <returns>A byte array.</returns>
        public static byte[] HexToByteArray(string hex)
        {
            //copypasted from:
            //https://social.msdn.microsoft.com/Forums/en-US/851492fa-9ddb-42d7-8d9a-13d5e12fdc70/convert-from-a-hex-string-to-a-byte-array-in-c?forum=aspgettingstarted
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// Sign specified <paramref name="data"/> using specified <paramref name="hashAlgorithm"/>.
        /// </summary>
        /// <returns>X.509 signature for specified data.</returns>
        public override byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm) =>
            _realRsaGenerator.SignData(data, hashAlgorithm);
    }
}
