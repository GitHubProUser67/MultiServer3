using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace BackendProject.FOSSProjects.VulnerableSSLv3Server;

/// <summary>
/// Based on the following article: https://github.com/Aim4kill/Bug_OldProtoSSL
/// </summary>
public class CertGenerator
{
    private const string CipherAlgorithm = "SHA1WITHRSA";

    public CertGenerator()
    {

    }

    /// <summary>
    /// Generates a certificate for vulnerable ProtoSSL versions.
    /// </summary>
    public (AsymmetricKeyParameter, Certificate) GenerateProtoSSLVulnerableCert(string issuer, string subject)
    {
        BcTlsCrypto crypto = new(new SecureRandom());
        RsaKeyPairGenerator rsaKeyPairGen = new();
        rsaKeyPairGen.Init(new KeyGenerationParameters(crypto.SecureRandom, 1024));

        AsymmetricCipherKeyPair caKeyPair = rsaKeyPairGen.GenerateKeyPair();
        AsymmetricCipherKeyPair cKeyPair = rsaKeyPairGen.GenerateKeyPair();

        Pkcs12Store store = new Pkcs12StoreBuilder().Build();
        X509CertificateEntry certEntry = new(PatchCertificateSignaturePattern(GenerateCertificate(subject, cKeyPair, caKeyPair.Private, GenerateCertificate(issuer, caKeyPair, caKeyPair.Private))));

        string certDomain = subject.Split("CN=")[1].Split(",")[0];

        CustomLogger.LoggerAccessor.LogDebug("[VulnerableSSLv3Server:CertGenerator] - GenerateVulnerableCert - Certificate generated for: {domain}", certDomain);

        store.SetCertificateEntry(certDomain, certEntry);
        store.SetKeyEntry(certDomain, new AsymmetricKeyEntry(cKeyPair.Private), new[] { certEntry });

        return (cKeyPair.Private, new Certificate(new TlsCertificate[] { new BcTlsCertificate(crypto, certEntry.Certificate.GetEncoded()) }));
    }

    /// <summary>
    /// Generates a certificate for old SSLv3 clients (with a valid cert signature too), you need a rsa private key for this one.
    /// </summary>
    public (AsymmetricKeyParameter, Certificate) GenerateVulnerableCert(string issuer, string subject, string rsaprivatekeycontent)
    {
        BcTlsCrypto crypto = new(new SecureRandom());
        RsaKeyPairGenerator rsaKeyPairGen = new();
        rsaKeyPairGen.Init(new KeyGenerationParameters(crypto.SecureRandom, 1024));

        AsymmetricCipherKeyPair caKeyPair = ReadPrivateKeyFromString(rsaprivatekeycontent);
        AsymmetricCipherKeyPair cKeyPair = rsaKeyPairGen.GenerateKeyPair();

        Pkcs12Store store = new Pkcs12StoreBuilder().Build();
        X509CertificateEntry certEntry = new(GenerateCertificate(subject, cKeyPair, caKeyPair.Private, GenerateCertificate(issuer, caKeyPair, caKeyPair.Private)));

        string certDomain = subject.Split("CN=")[1].Split(",")[0];

        CustomLogger.LoggerAccessor.LogDebug("[VulnerableSSLv3Server:CertGenerator] - GenerateVulnerableCertWithoutHax - Certificate generated for: {domain}", certDomain);

        store.SetCertificateEntry(certDomain, certEntry);
        store.SetKeyEntry(certDomain, new AsymmetricKeyEntry(cKeyPair.Private), new[] { certEntry });

        return (cKeyPair.Private, new Certificate(new TlsCertificate[] { new BcTlsCertificate(crypto, certEntry.Certificate.GetEncoded()) }));
    }

    private static X509Certificate GenerateCertificate(string subjectName, AsymmetricCipherKeyPair subjectKeyPair, AsymmetricKeyParameter issuerPrivKey, X509Certificate? issuerCert = null)
    {
        X509V3CertificateGenerator certGen = new();
        certGen.SetSerialNumber(BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), new SecureRandom()));
        certGen.SetIssuerDN(issuerCert == null ? new X509Name(subjectName) : issuerCert.SubjectDN);
        certGen.SetNotBefore(DateTime.UtcNow.Date);
        certGen.SetNotAfter(DateTime.UtcNow.Date.AddYears(10));
        certGen.SetSubjectDN(new X509Name(subjectName));
        certGen.SetPublicKey(subjectKeyPair.Public);
        return certGen.Generate(new Asn1SignatureFactory(CipherAlgorithm, issuerPrivKey));
    }

    private static X509Certificate PatchCertificateSignaturePattern(X509Certificate cCertificate)
    {
        byte[] certDer = DotNetUtilities.ToX509Certificate(cCertificate).GetRawCertData();

        // Pattern to find the SHA-1 signature in the DER encoded certificate
        byte[] signaturePattern = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x05 };

        // There must be two signatures in the DER encoded certificate
        int signature1Offset = MiscUtils.FindBytePattern(certDer, signaturePattern);
        int signature2Offset = MiscUtils.FindBytePattern(certDer, signaturePattern, signature1Offset + 1);

        if (signature1Offset == -1 || signature2Offset == -1)
            throw new Exception("[VulnerableSSLv3Server:CertGenerator] - Failed to find valid signature for patching!");

        // Patch the second signature to TLS_NULL_WITH_NULL_NULL
        int byteOffset = signature2Offset + 8;
        certDer[byteOffset] = 0x01;

        using MemoryStream derStream = new(certDer);
        return new X509CertificateParser().ReadCertificate(derStream);
    }

    private static AsymmetricCipherKeyPair ReadPrivateKeyFromString(string privateKeyData)
    {
        AsymmetricCipherKeyPair keyPair;

        using (StringReader reader = new(privateKeyData))
            keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

        return keyPair;
    }
}