using CustomLogger;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;

namespace CryptoSporidium
{
    public class SSLUtils
    {
        // PEM file headers.
        public const string CRT_HEADER = "-----BEGIN CERTIFICATE-----\n";
        public const string CRT_FOOTER = "\n-----END CERTIFICATE-----";
        public const string PRIVATE_RSA_KEY_HEADER = "-----BEGIN RSA PRIVATE KEY-----\n";
        public const string PRIVATE_RSA_KEY_FOOTER = "\n-----END RSA PRIVATE KEY-----";
        public const string PUBLIC_RSA_KEY_HEADER = "-----BEGIN RSA PUBLIC KEY-----\n";
        public const string PUBLIC_RSA_KEY_FOOTER = "\n-----END RSA PUBLIC KEY-----";
        public const string SCERT_ROOT_CA = "-----BEGIN CERTIFICATE-----\r\n" +
            "MIIDujCCAqKgAwIBAgIUAQAAAAAAAAAAAAAAAAAAAAAAAAAwDQYJKoZIhvcNAQEF\r\n" +
            "BQAwgZYxCzAJBgNVBAYTAlVTMQswCQYDVQQIEwJDQTESMBAGA1UEBxMJU2FuIERp\r\n" +
            "ZWdvMTEwLwYDVQQKEyhTT05ZIENvbXB1dGVyIEVudGVydGFpbm1lbnQgQW1lcmlj\r\n" +
            "YSBJbmMuMRQwEgYDVQQLEwtTQ0VSVCBHcm91cDEdMBsGA1UEAxMUU0NFUlQgUm9v\r\n" +
            "dCBBdXRob3JpdHkwHhcNMDUwMTAxMTIwMDAwWhcNMzQxMjMxMjM1OTU5WjCBljEL\r\n" +
            "MAkGA1UEBhMCVVMxCzAJBgNVBAgTAkNBMRIwEAYDVQQHEwlTYW4gRGllZ28xMTAv\r\n" +
            "BgNVBAoTKFNPTlkgQ29tcHV0ZXIgRW50ZXJ0YWlubWVudCBBbWVyaWNhIEluYy4x\r\n" +
            "FDASBgNVBAsTC1NDRVJUIEdyb3VwMR0wGwYDVQQDExRTQ0VSVCBSb290IEF1dGhv\r\n" +
            "cml0eTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALKc0nZ71kfvzujJ\r\n" +
            "UeegRB6ZIK64dwY4HXm6b8q0i7YQWeCPT5uOPoQUC2gLtyUxn2vwhNWv4Zf/Dn/R\r\n" +
            "RSz7AbqD2KlcbwryIbQMBx6lOnG80baC0pCRFWCpxYRgrndxXYeotSceGD4t0+xc\r\n" +
            "T7qaR4X+z8s2M3zfpLtXiCQ6L9Fqzy+ZV3mEokkO26nG5LicnwSiPGO7yrkHFZGn\r\n" +
            "x30oLv7rhTh18iUUgd0Wzp7t+39OkcUo2mJnG9yMEtphA0JW0/LFZTsuIGWy4H6P\r\n" +
            "Ll9fvX/+ZZzx2+cTeGym6y7bIxorRAr246payX2v0ZmNyzz/SD4XC/ui0QNzuL0r\r\n" +
            "joG48o0CAwEAATANBgkqhkiG9w0BAQUFAAOCAQEAOgopH+NM7SJTvVDS90pS0vKY\r\n" +
            "AgMNi0o5wp8VKAvqFusZULN/dz+gXSwfwydzCJteZfTw8Xs+iYIRgGqqAuTaV1xT\r\n" +
            "TGDaHYgCw1EUF4chNKtQ8PjFb7nOFJOGd25Aedr4W09g2sTAtz7htsED0703odZM\r\n" +
            "FTk9g6Q1wrP/ZWRIYKHSXYMSwssmMpgMeb7v0z7tgqizbppnu9BC/BcaST2yliLO\r\n" +
            "rrlrBCNoL34oHSz9Wd9JexamZ4JQkiU8ViIyRfaey9/qGZRdVv/sJkxFv4zqGk/t\r\n" +
            "iRmeyTsNzoS+i+Fk3qbajbJcAd3TesHckyWcODi46+jf4VZxqIZLNXRqvJmanw==\r\n" +
            "-----END CERTIFICATE-----\n";
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
        public const string HOME_PRIVATE_KEY = "-----BEGIN RSA PRIVATE KEY-----\r\n" +
            "MIICXQIBAAKBgQCzeKs2pwCMRPYkTKcUd+cuLFk5bGHBy1DWZED/uTtfERwqjJEi\r\n" +
            "cKVwJxTlayFWnqigZthNgFW/RJUMIZsK10lc1j/jM4lf7vlUjrTwGoOSTP2dIFO7\r\n" +
            "cLVNMTDuq82bfbwnPX8SJAW8MpL9KJOxhX1aiMaUekaN5x56yOMQr1lwFwIDAQAB\r\n" +
            "AoGBAKpWq1ox42k+wsftIN9idj7yxLSl05rV2CHEAZU1P86ZNLyFsfKYK81oqoKc\r\n" +
            "zYWjDLVBJ6dXWQsykqxy8O63Kt6lH42TCezj/iQN+hV2rzhQQgMJ8K3goqsPgIlt\r\n" +
            "hHqvYKZxBUGym9s+v/B2QOaz8Bvbew4ge2/L/xzX4vpaR6ABAkEA3vUROwJoVFOj\r\n" +
            "hQOccOJZ93rGlNrGWkpM4UE5LsaanALEWb8b1rfugsQ457RoGtXOk4+lFAhbVq/2\r\n" +
            "yDtet9QIFwJBAM4RwiGnD6Xct504gsiJCFTgPaye8gDwvmdp3XNsAABnnioJp0yM\r\n" +
            "aTJ7aXp6IyVm00RQKEVegQbZ1Menh5Ie2AECQADwX0Y0WGQihgnFXh9LlL1qEvQF\r\n" +
            "h9hRf8ljEO6Vf4kwqcsG9wMMe0CpuuOe6uFSDTCp5jQTZO8UhqGJPnjft7kCQHg+\r\n" +
            "wIsmkuj0DGi/qwEdhTER0KtD7G9EC7cIfWJ2qOGTlSVukKMIY/JDNV90mcGfaLQ6\r\n" +
            "GeWwqZW30oPWbDOFsAECQQC076s5komcJC1YipfTYGdpyNS5tAGeLJfE7IlpVrwt\r\n" +
            "n9rAHnFapGDVIRpkhIWWOmFzUttc+zUglqERusjqAAYj\r\n" +
            "-----END RSA PRIVATE KEY-----\r\n";

        public static string[] DnsList = {
        "www.outso-srv1.com",
        "sonyhome.thqsandbox.com",
        "juggernaut-games.com",
        "away.veemee.com",
        "home.veemee.com",
        "homeps3.svo.online.scee.com",
        "pshome.ndreams.net",
        "stats.outso-srv1.com",
        "s3.amazonaws.com",
        "game2.hellfiregames.com",
        "youtube.com",
        "api.pottermore.com",
        "cprod.homerewards.online.scee.com",
        "api.stathat.com",
        "hubps3.online.scee.com",
        "homeps3-content.online.scee.com",
        "cprod.homeidentity.online.scee.com",
        "homeps3.online.scee.com",
        "scee-home.playstation.net",
        "scea-home.playstation.net",
        "update-prod.pfs.online.scee.com",
        "cprod.homeserverservices.online.scee.com",
        "wipeout2048.online.scee.com",
        "mmgproject0001.com",
        "massmedia.com",
        "alpha.lootgear.com",
        "prd.destinations.scea.com",
        "root.pshomecasino.com",
        "homeec.scej-nbs.jp",
        "homeecqa.scej-nbs.jp",
        "test.playstationhome.jp",
        "playstationhome.jp",
        "hdc.cprod.homeps3.online.scee.com",
        "download-prod.online.scea.com",
        "us.ads.playstation.net",
        "ww-prod-sec.destinations.scea.com",
        "ll-100.ea.com",
        "services.heavyh2o.net",
        "starhawk-prod2.svo.online.scea.com",
        "secure.cprod.homeps3.online.scee.com",
        "destinationhome.live" };

        public static X509Certificate2 CreateRootCertificateAuthority(string FileName)
        {
            byte[] certSerialNumber = new byte[16];
            new Random().NextBytes(certSerialNumber);

            // Generate a new RSA key pair
            using (RSA rsa = RSA.Create(1024))
            {
                rsa.ImportFromPem(HOME_PRIVATE_KEY.ToArray());

                // Create a certificate request with the RSA key pair
                CertificateRequest request = new($"CN=MultiServer Certificate Authority [" + new Random().NextInt64(100, 999) + "], OU=Scientists Department, O=MultiServer Corp, L=New York, S=Northeastern United, C=United States", rsa, HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);

                // Configure the certificate as CA.
                request.CertificateExtensions.Add(
                   new X509BasicConstraintsExtension(true, true, 12, true));

                // Configure the certificate for Digital Signature and Key Encipherment.
                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.KeyCertSign,
                        true));

                // Set the validity period of the certificate
                DateTimeOffset notBefore = new(new DateTime(1980, 1, 1), TimeSpan.Zero);
                DateTimeOffset notAfter = new(new DateTime(7980, 1, 1), TimeSpan.Zero);

                RsaPkcs1SignatureGenerator customSignatureGenerator = new(rsa);

                // Create a self-signed certificate from the certificate request
                X509Certificate2 certificate = request.Create(
                    request.SubjectName,
                    customSignatureGenerator,
                    notBefore,
                    notAfter,
                    certSerialNumber);

                X509Certificate2 SelfSignedCertificate = certificate.CopyWithPrivateKey(rsa);

                // Export the private key.
                string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);
                File.WriteAllText(Path.GetDirectoryName(FileName) + $"/{Path.GetFileNameWithoutExtension(FileName)}_rootca_privkey.pem", PRIVATE_RSA_KEY_HEADER + privateKey + PRIVATE_RSA_KEY_FOOTER);

                // Export the certificate.
                byte[] exportData = SelfSignedCertificate.Export(X509ContentType.Cert);
                string crt = Convert.ToBase64String(exportData, Base64FormattingOptions.InsertLineBreaks);
                File.WriteAllText(Path.GetDirectoryName(FileName) + $"/{Path.GetFileNameWithoutExtension(FileName)}_rootca.pem", CRT_HEADER + crt + CRT_FOOTER);
                File.WriteAllText(Path.GetDirectoryName(FileName) + $"/{Path.GetFileNameWithoutExtension(FileName)}_rootca.cer", CRT_HEADER + crt + CRT_FOOTER); // For Windows MMC.

                rsa.Clear();

                return SelfSignedCertificate;
            }
        }

        public static string CreateChainSignedCert(X509Certificate2 issuerCertificate, string FileName)
        {
            byte[] certSerialNumber = new byte[16];
            new Random().NextBytes(certSerialNumber);

            // Generate a new RSA key pair
            using (RSA rsa = RSA.Create(1024))
            {
                rsa.ImportFromPem(HOME_PRIVATE_KEY.ToArray());

                // Create a certificate request with the RSA key pair
                CertificateRequest request = new($"CN=*.net [" + new Random().NextInt64(100, 999) + "], OU=Scientists Department, O=MultiServer Corp, L=New York, S=Northeastern United, C=United States", rsa, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);

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
                sanBuilder.AddDnsName("*.net");
                sanBuilder.AddDnsName("*.com");
                sanBuilder.AddDnsName("*.fr");
                sanBuilder.AddDnsName("*.it");
                sanBuilder.AddDnsName("*.en");
                sanBuilder.AddDnsName("*.online");
                if (DnsList != null) // Yep... some clients do not allow wildcard domains.
                {
                    foreach (string str in DnsList)
                    {
                        sanBuilder.AddDnsName(str);
                    }
                }
                sanBuilder.AddIpAddress(IPAddress.Parse("0.0.0.0"));
                sanBuilder.AddEmailAddress("MultiServer@gmail.com");
                request.CertificateExtensions.Add(sanBuilder.Build());

                // Set the validity period of the certificate
                DateTimeOffset notBefore = new(new DateTime(1980, 1, 1), TimeSpan.Zero);
                DateTimeOffset notAfter = new(new DateTime(7980, 1, 1), TimeSpan.Zero);

                // Create a self-signed certificate from the certificate request
                X509Certificate2 certificate = request.Create(
                    issuerCertificate,
                    notBefore,
                    notAfter,
                    certSerialNumber);

                X509Certificate2 SelfSignedCertificate = certificate.CopyWithPrivateKey(rsa);

                string certPassword = "qwerty"; // Set a password to protect the private key
                File.WriteAllBytes(FileName, SelfSignedCertificate.Export(X509ContentType.Pfx, certPassword));

                // Export the private key.
                string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);
                File.WriteAllText(Path.GetDirectoryName(FileName) + $"/{Path.GetFileNameWithoutExtension(FileName)}_privkey.pem", PRIVATE_RSA_KEY_HEADER + privateKey + PRIVATE_RSA_KEY_FOOTER);

                // Export the public key.
                string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey(), Base64FormattingOptions.InsertLineBreaks);
                File.WriteAllText(Path.GetDirectoryName(FileName) + $"/{Path.GetFileNameWithoutExtension(FileName)}_pubkey.pem", PUBLIC_RSA_KEY_HEADER + publicKey + PUBLIC_RSA_KEY_FOOTER);

                Org.BouncyCastle.X509.X509Certificate x509cert = ImportCertFromPfx(FileName, certPassword);

                StringBuilder CertPem = new();
                PemWriter CSRPemWriter = new(new StringWriter(CertPem));
                CSRPemWriter.WriteObject(x509cert);
                CSRPemWriter.Writer.Flush();
                File.WriteAllText(Path.GetDirectoryName(FileName) + $"/{Path.GetFileNameWithoutExtension(FileName)}.pem", CertPem.ToString());

                rsa.Clear();

                return CertPem.ToString();
            }
        }

        public static string CreateSelfSignedCert(string FileName)
        {
            // Generate a new RSA key pair
            using (RSA rsa = RSA.Create(1024))
            {
                rsa.ImportFromPem(HOME_PRIVATE_KEY.ToArray());

                // Create a certificate request with the RSA key pair
                CertificateRequest request = new($"CN=*.net [" + new Random().NextInt64(100, 999) + "], OU=Scientists Department, O=MultiServer Corp, L=New York, S=Northeastern United, C=United States", rsa, HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);

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
                var sanBuilder = new SubjectAlternativeNameBuilder();
                sanBuilder.AddDnsName("*.net");
                sanBuilder.AddDnsName("*.com");
                sanBuilder.AddDnsName("*.fr");
                sanBuilder.AddDnsName("*.it");
                sanBuilder.AddDnsName("*.en");
                sanBuilder.AddDnsName("*.online");
                if (DnsList != null) // Yep... some clients do not allow wildcard domains.
                {
                    foreach (string str in DnsList)
                    {
                        sanBuilder.AddDnsName(str);
                    }
                }
                sanBuilder.AddIpAddress(IPAddress.Parse("0.0.0.0"));
                sanBuilder.AddEmailAddress("MultiServer@gmail.com");
                request.CertificateExtensions.Add(sanBuilder.Build());

                // Set the validity period of the certificate
                DateTimeOffset notBefore = new(new DateTime(1980, 1, 1), TimeSpan.Zero);
                DateTimeOffset notAfter = new(new DateTime(7980, 1, 1), TimeSpan.Zero);

                RsaPkcs1SignatureGenerator customSignatureGenerator = new(rsa);

                // Create a self-signed certificate from the certificate request
                X509Certificate2 certificate = request.Create(
                    request.SubjectName,
                    customSignatureGenerator,
                    notBefore,
                    notAfter,
                    new byte[] { 0x00, 0x01, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00 }); // This byte array specifies SSLv2 compatibility

                X509Certificate2 SelfSignedCertificate = certificate.CopyWithPrivateKey(rsa);

                string certPassword = "qwerty"; // Set a password to protect the private key
                File.WriteAllBytes(FileName, SelfSignedCertificate.Export(X509ContentType.Pfx, certPassword));

                // Export the private key.
                string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);
                File.WriteAllText(Path.GetDirectoryName(FileName) + $"/{Path.GetFileNameWithoutExtension(FileName)}_key.pem", PRIVATE_RSA_KEY_HEADER + privateKey + PRIVATE_RSA_KEY_FOOTER);

                // Export the public key.
                string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey(), Base64FormattingOptions.InsertLineBreaks);
                File.WriteAllText(Path.GetDirectoryName(FileName) + $"/{Path.GetFileNameWithoutExtension(FileName)}_pubkey.pem", PUBLIC_RSA_KEY_HEADER + publicKey + PUBLIC_RSA_KEY_FOOTER);

                Org.BouncyCastle.X509.X509Certificate x509cert = ImportCertFromPfx(FileName, certPassword);

                StringBuilder CertPem = new();
                PemWriter CSRPemWriter = new(new StringWriter(CertPem));
                CSRPemWriter.WriteObject(x509cert);
                CSRPemWriter.Writer.Flush();
                File.WriteAllText(Path.GetDirectoryName(FileName) + $"/{Path.GetFileNameWithoutExtension(FileName)}.pem", CertPem.ToString());

                rsa.Clear();

                return CertPem.ToString();
            }
        }

        public static Org.BouncyCastle.X509.X509Certificate ImportCertFromPfx(string path, string password)
        {
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.Load(File.OpenRead(path), password.ToCharArray());
            string alias = string.Empty;
            foreach (string str in store.Aliases)
            {
                if (store.IsKeyEntry(str))
                    alias = str;
            }
            X509CertificateEntry certEntry = store.GetCertificate(alias);
            return certEntry.Certificate;
        }

        public static void CreateHomeCertificatesFile(string rootcaSubject, string selfsignedSubject, string FileName)
        {
            File.WriteAllText(FileName, rootcaSubject + selfsignedSubject + SCERT_ROOT_CA + ENTRUST_NET_CA); // For PSHome clients.
        }

        public static void InitCerts(string certpath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(certpath));

            if (!File.Exists(Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_rootca.pem") || !File.Exists(certpath) || !File.Exists(Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_chaincert.pfx"))
            {
                bool changed = false;
                X509Certificate2? rootca = null;

                if (!File.Exists(Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_rootca.pem"))
                {
                    rootca = CreateRootCertificateAuthority(certpath);
                    changed = true;
                }
                else
                {
                    using (RSA rsa = RSA.Create())
                    {
                        rsa.ImportFromPem(File.ReadAllText(Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_rootca_privkey.pem").ToArray());
                        rootca = X509Certificate2.CreateFromPem(File.ReadAllText(Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_rootca.pem").ToArray()).CopyWithPrivateKey(rsa);
                        rsa.Clear();
                    }
                }

                Org.BouncyCastle.X509.X509Certificate x509cert = pemToX509Certificate(File.ReadAllText(Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_rootca.pem"));

                StringBuilder CertPem = new();
                PemWriter CSRPemWriter = new(new StringWriter(CertPem));
                CSRPemWriter.WriteObject(x509cert);
                CSRPemWriter.Writer.Flush();

                if (!File.Exists(certpath) || changed) // If rootca changed, create chain signed cert again.
                    CreateChainSignedCert(rootca, certpath);

                if (!File.Exists(Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_selfsigned.pfx")) // Not use rootca so no need to change it if rootca changed.
                    CreateSelfSignedCert(Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_selfsigned.pfx");

                CreateHomeCertificatesFile(CertPem.ToString(), File.ReadAllText(Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_selfsigned.pem"),
                    Path.GetDirectoryName(certpath) + "/CERTIFICATES.TXT");
            }
        }

        private static Org.BouncyCastle.X509.X509Certificate pemToX509Certificate(string signature)
        {
            byte[] buffer = GetBytesFromPEM("CERTIFICATE", signature);
            X509CertificateParser parser = new();
            Org.BouncyCastle.X509.X509Certificate cert = parser.ReadCertificate(buffer);
            return cert;
        }

        private static byte[] GetBytesFromPEM(string type, string pem)
        {
            string header = string.Format("-----BEGIN {0}-----", type);
            string footer = string.Format("-----END {0}-----", type);
            int start = pem.IndexOf(header) + header.Length;
            int end = pem.IndexOf(footer, start);
            string base64 = pem.Substring(start, (end - start));
            return Convert.FromBase64String(base64);
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

            LoggerAccessor.LogError(nameof(hashAlgorithm), "'" + hashAlgorithm + "' is not a supported algorithm at this moment.");

            return null;
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
