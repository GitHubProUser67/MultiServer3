using System.Collections.Concurrent;
using BackendProject;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace SRVEmu.Arcadia.EA;

/// <summary>
/// Based on the following article: https://github.com/Aim4kill/Bug_OldProtoSSL
/// </summary>
public class ProtoSSL
{
    private readonly ConcurrentDictionary<string, (AsymmetricKeyParameter, Certificate)> _certCache = new();
    private const string Sha1CipherAlgorithm = "SHA1WITHRSA";
    private static readonly ReadOnlyMemory<byte> Sha1CipherSignature = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x05 };

    public (AsymmetricKeyParameter, Certificate) GetFeslEaCert()
    {
        const string IssuerDN = "CN=OTG3 Certificate Authority, C=US, ST=California, L=Redwood City, O=\"Electronic Arts, Inc.\", OU=Online Technology Group, emailAddress=dirtysock-contact@ea.com";
        const string SubjectDN = "C=US, ST=California, O=\"Electronic Arts, Inc.\", OU=Online Technology Group, CN=fesl.ea.com, emailAddress=fesl@ea.com";
        const string cacheKey = "fesl.ea.com";

        if (_certCache.TryGetValue(cacheKey, out var cacheHit))
            // !TODO: New connections will break after running 10 years without a restart 
            return cacheHit;

        (AsymmetricKeyParameter, Certificate) creds = GenerateVulnerableCert(IssuerDN, SubjectDN);
        _certCache.TryAdd(cacheKey, creds);
        return creds;
    }

    /// <summary>
    /// Generates a certificate for vulnerable ProtoSSL versions.
    /// </summary>
    private (AsymmetricKeyParameter, Certificate) GenerateVulnerableCert(string issuer, string subject)
    {
        BcTlsCrypto crypto = new(new SecureRandom());
        RsaKeyPairGenerator rsaKeyPairGen = new();
        rsaKeyPairGen.Init(new KeyGenerationParameters(crypto.SecureRandom, 1024));

        AsymmetricCipherKeyPair caKeyPair = rsaKeyPairGen.GenerateKeyPair();

        AsymmetricCipherKeyPair cKeyPair = rsaKeyPairGen.GenerateKeyPair();

        Pkcs12Store store = new Pkcs12StoreBuilder().Build();
        X509CertificateEntry certEntry = new(PatchCertificateSignaturePattern(GenerateCertificate(subject,
            cKeyPair, caKeyPair.Private, GenerateCertificate(issuer, caKeyPair, caKeyPair.Private))));

        string certDomain = subject.Split("CN=")[1].Split(",")[0];

        CustomLogger.LoggerAccessor.LogDebug("[Arcadia] - ProtoSSL-GenerateVulnerableCert Certificate generated for: {domain}", certDomain);

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
        return certGen.Generate(new Asn1SignatureFactory(Sha1CipherAlgorithm, issuerPrivKey));
    }

    private static X509Certificate PatchCertificateSignaturePattern(X509Certificate cCertificate)
    {
        byte[] certDer = DotNetUtilities.ToX509Certificate(cCertificate).GetRawCertData();

        // There must be two signatures in the DER encoded certificate
        int signature1Offset = MiscUtils.FindBytePattern(certDer, Sha1CipherSignature.Span);
        int signature2Offset = MiscUtils.FindBytePattern(certDer, Sha1CipherSignature.Span, signature1Offset + Sha1CipherSignature.Length);

        if (signature1Offset == -1 || signature2Offset == -1)
            throw new Exception("Failed to find valid signature for patching!");

        // Patch the second signature to TLS_NULL_WITH_NULL_NULL
        certDer[signature2Offset + 8] = 0x01;

        using MemoryStream derStream = new(certDer);
        return new X509CertificateParser().ReadCertificate(derStream);
    }
}