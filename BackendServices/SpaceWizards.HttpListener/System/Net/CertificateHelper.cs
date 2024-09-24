using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Net;

namespace SpaceWizards.HttpListener
{
    public class CertificateHelper
    {
        private static readonly string[] tlds = {
            ".com", ".org", ".net", ".int", ".edu", ".gov", ".mil", // Generic TLDs
            ".info", ".biz", ".mobi", ".name", ".pro", ".aero", ".coop", // Generic TLDs continued
            ".asia", ".cat", ".jobs", ".museum", ".tel", ".travel", ".tel", // Sponsored TLDs
            ".travel", ".int", ".online",
            ".ac", ".ad", ".ae", ".af", ".ag", ".ai", ".al", ".am", ".an", // Country Code TLDs (A-Z)
            ".ao", ".aq", ".ar", ".as", ".at", ".au", ".aw", ".ax", ".az",
            ".ba", ".bb", ".bd", ".be", ".bf", ".bg", ".bh", ".bi", ".bj",
            ".bm", ".bn", ".bo", ".br", ".bs", ".bt", ".bv", ".bw", ".by",
            ".bz", ".ca", ".cc", ".cd", ".cf", ".cg", ".ch", ".ci", ".ck",
            ".cl", ".cm", ".cn", ".co", ".cr", ".cs", ".cu", ".cv", ".cx",
            ".cy", ".cz", ".dd", ".de", ".dj", ".dk", ".dm", ".do", ".dz",
            ".ec", ".ee", ".eg", ".eh", ".er", ".es", ".et", ".eu", ".fi",
            ".fj", ".fk", ".fm", ".fo", ".fr", ".ga", ".gb", ".gd", ".ge",
            ".gf", ".gg", ".gh", ".gi", ".gl", ".gm", ".gn", ".gp", ".gq",
            ".gr", ".gs", ".gt", ".gu", ".gw", ".gy", ".hk", ".hm", ".hn",
            ".hr", ".ht", ".hu", ".id", ".ie", ".il", ".im", ".in", ".io",
            ".iq", ".ir", ".is",".it", ".je", ".jm", ".jo", ".jp", ".ke",
            ".kg", ".kh", ".ki", ".km", ".kn", ".kp", ".kr", ".kw", ".ky",
            ".kz", ".la", ".lb", ".lc", ".li", ".lk", ".lr", ".ls", ".lt",
            ".lu", ".lv", ".ly", ".ma", ".mc", ".md", ".me", ".mg", ".mh",
            ".mk", ".ml", ".mm", ".mn", ".mo", ".mp", ".mq", ".mr", ".ms",
            ".mt", ".mu", ".mv", ".mw", ".mx", ".my", ".mz", ".na", ".nc",
            ".ne", ".nf", ".ng", ".ni", ".nl", ".no", ".np",  ".nr", ".nu",
            ".nz", ".om", ".pa", ".pe", ".pf", ".pg", ".ph", ".pk", ".pl",
            ".pm", ".pn", ".pr", ".ps", ".pt", ".pw", ".py", ".qa", ".re",
            ".ro", ".rs", ".ru", ".rw", ".sa", ".sb", ".sc", ".sd", ".se",
            ".sg", ".sh", ".si", ".sj", ".sk", ".sl", ".sm", ".sn", ".so",
            ".sr", ".ss", ".st", ".su",  ".sv", ".sx", ".sy", ".sz", ".tc",
            ".td", ".tf", ".tg", ".th", ".tj", ".tk", ".tl", ".tm", ".tn",
            ".to", ".tp", ".tr", ".tt", ".tv", ".tw", ".tz",  ".ua", ".ug",
            ".uk", ".us", ".uy", ".uz",  ".va", ".vc", ".ve", ".vg", ".vi",
            ".vn", ".vu", ".wf", ".ws", ".ye", ".yt", ".za", ".zm", ".zw",
            ".arpa", ".aero", ".coop", ".museum", ".asia", ".cat", ".jobs", // Infrastructure TLD
            ".mobi",
            ".example", ".localhost", ".test" // Reserved TLDs
        };

        private static ConcurrentDictionary<string, X509Certificate2> FakeCertificates = new ConcurrentDictionary<string, X509Certificate2>();

        /// <summary>
        /// Issue a chain-signed SSL certificate with private key.
        /// </summary>
        /// <param name="certSubject">Certificate subject (domain name).</param>
        /// <param name="issuerCertificate">Authority's certificate used to sign this certificate.</param>
        /// <param name="serverIp">IP Address of the remote server.</param>
        /// <param name="certHashAlgorithm">Certificate hash algorithm.</param>
        /// <param name="certVaildBeforeNow">Minimum Certificate validity Date.</param>
        /// <param name="certVaildAfterNow">Maximum Certificate validity Date.</param>
        /// <param name="wildcard">(optional) Enables wildcard SAN attributes.</param>
        /// <returns>Signed chain of SSL Certificates.</returns>
        public static X509Certificate MakeChainSignedCert(string certSubject, X509Certificate2 issuerCertificate, HashAlgorithmName certHashAlgorithm,
            IPAddress serverIp, DateTimeOffset certVaildBeforeNow, DateTimeOffset certVaildAfterNow, bool wildcard = false)
        {
            // Look if it is already issued.
            // Why: https://support.mozilla.org/en-US/kb/Certificate-contains-the-same-serial-number-as-another-certificate
            if (FakeCertificates.ContainsKey(certSubject))
            {
                X509Certificate2 CachedCertificate = FakeCertificates[certSubject];
                //check that it hasn't expired
                if (CachedCertificate.NotAfter > DateTime.Now && CachedCertificate.NotBefore < DateTime.Now)
                { return CachedCertificate; }
                else
                { FakeCertificates.Remove(certSubject, out _); }
            }

            using RSA issuerPrivKey = issuerCertificate.GetRSAPrivateKey() ?? throw new Exception("Issuer Certificate doesn't have a private key, Chain Signed Certificate will not be generated.");

            // If not found, initialize private key generator & set up a certificate creation request.
            using RSA rsa = RSA.Create();

            // Generate an unique serial number.
            byte[] certSerialNumber = new byte[16];
            new Random().NextBytes(certSerialNumber);

            // set up a certificate creation request.
            CertificateRequest certRequestAny = new CertificateRequest($"CN={certSubject} [{GetRandomInt64(100, 999)}], OU=SpaceStation14 Department," +
                $" O=\"SpaceWizards Corp\", L=New York, S=Northeastern United, C=US", rsa, certHashAlgorithm, RSASignaturePadding.Pkcs1);

            // set up a optional SAN builder.
            SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();

            sanBuilder.AddDnsName(certSubject); // Some legacy clients will not recognize the cert serial-number.
            sanBuilder.AddEmailAddress("SpaceWizards@gmail.com");
            sanBuilder.AddIpAddress(serverIp);

            if (wildcard)
            {
                tlds.Select(tld => "*" + tld)
                .ToList()
                .ForEach(sanBuilder.AddDnsName);
            }

            certRequestAny.CertificateExtensions.Add(sanBuilder.Build());

            // Export the issued certificate with private key.
            X509Certificate2 certificateWithKey = new X509Certificate2(certRequestAny.Create(
                issuerCertificate.IssuerName,
                new RsaPkcs1SignatureGenerator(issuerPrivKey),
                certVaildBeforeNow,
                certVaildAfterNow,
                certSerialNumber).CopyWithPrivateKey(rsa).Export(X509ContentType.Pfx));

            // Save the certificate and return it.
            FakeCertificates.TryAdd(certSubject, certificateWithKey);
            return certificateWithKey;
        }

        /// <summary>
        /// Checks if the X509Certificate is of Certificate Authority type.
        /// </summary>
        /// <param name="certificate">The certificate to check on.</param>
        /// <returns>A bool.</returns>
        public static bool IsCertificateAuthority(X509Certificate certificate)
        {
            // Compare the Issuer and Subject properties of the certificate
            return certificate.Issuer == certificate.Subject;
        }


        /// <summary>
        /// Get a random int64 number.
        /// </summary>
        /// <param name="minValue">The min value.</param>
        /// <param name="maxValue">The max value.</param>
        /// <returns>A long.</returns>
        private static long GetRandomInt64(long minValue, long maxValue)
        {
#if NET6_0_OR_GREATER
            return new Random().NextInt64(minValue, maxValue);
#else
            Random random = new Random();
            return (long)(((random.Next() << 32) | random.Next()) * (double)(maxValue - minValue) / 0xFFFFFFFFFFFFFFFF) + minValue;
#endif
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
