using System.Collections.Concurrent;
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
using CyberBackendLibrary.DataTypes;
using CustomLogger;
using Org.BouncyCastle.Asn1;

namespace MultiSocks.Tls;

/// <summary>
/// Based on the following article: https://github.com/Aim4kill/Bug_OldProtoSSL
/// Credits: https://github.com/valters-tomsons/arcadia - https://github.com/a-blondel/mohh2-wii-server
/// </summary>
public class ProtoSSLUtils
{
    private readonly ConcurrentDictionary<string, (AsymmetricKeyParameter, Certificate)> _certCache = new();
    private static readonly Dictionary<string, DerObjectIdentifier> RDN_NAME_TO_BC_STYLE = new()
    {
        { "US", X509Name.C },
        { "California", X509Name.ST },
        { "Redwood City", X509Name.L },
        { "Electronic Arts, Inc.", X509Name.O },
        { "Online Technology Group", X509Name.OU }
    };
    private const string SHA1CipherAlgorithm = "SHA1WITHRSA";
    private const string MD5CipherAlgorithm = "MD5WITHRSA";
    private const string IssuerDN = "CN=OTG3 Certificate Authority, C=US, ST=California, L=Redwood City, O=\"Electronic Arts, Inc.\", OU=Online Technology Group, emailAddress=dirtysock-contact@ea.com";
    private static readonly ReadOnlyMemory<byte> SHA1CipherSignature = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x05 };
    private static readonly ReadOnlyMemory<byte> MD5CipherSignature = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x04 };

    public (AsymmetricKeyParameter, Certificate) GetVulnerableFeslEaCert()
    {
        string cacheKey = "fesl.ea.com";
        string SubjectDN = $"C=US, ST=California, O=\"Electronic Arts, Inc.\", OU=Online Technology Group, CN={cacheKey}, emailAddress=fesl@ea.com";

        if (_certCache.TryGetValue(cacheKey, out (AsymmetricKeyParameter, Certificate) cacheHit))
            // !TODO: New connections will break after running 10 years without a restart 
            return cacheHit;

        (AsymmetricKeyParameter, Certificate) creds = GenerateVulnerableCert(IssuerDN, SubjectDN);
        _certCache.TryAdd(cacheKey, creds);
        return creds;
    }

    public (AsymmetricKeyParameter, Certificate) GetVulnerableCustomEaCert(string CN, string email)
    {
        string SubjectDN = $"C=US, ST=California, O=\"Electronic Arts, Inc.\", OU=Online Technology Group, CN={CN}, emailAddress={email}";

        if (_certCache.TryGetValue(CN, out (AsymmetricKeyParameter, Certificate) cacheHit))
            // !TODO: New connections will break after running 10 years without a restart 
            return cacheHit;

        (AsymmetricKeyParameter, Certificate) creds = GenerateVulnerableCert(IssuerDN, SubjectDN);
        _certCache.TryAdd(CN, creds);
        return creds;
    }

    public (AsymmetricKeyParameter, Certificate) GetVulnerableLegacyCustomEaCert(string CN, string email, bool WeakChainSignedRSAKey)
    {
        if (_certCache.TryGetValue(CN, out (AsymmetricKeyParameter, Certificate) cacheHit))
            // !TODO: New connections will break after running 10 years without a restart 
            return cacheHit;

        (AsymmetricKeyParameter, Certificate) creds = GenerateVulnerableLegacyCert("OTG3 Certificate Authority", "dirtysock-contact@ea.com",
            CN, email, WeakChainSignedRSAKey);
        _certCache.TryAdd(CN, creds);
        return creds;
    }

    /// <summary>
    /// Generates a vulnerable certificate for vulnerable ProtoSSL versions.
    /// </summary>
    private (AsymmetricKeyParameter, Certificate) GenerateVulnerableCert(string issuer, string subject)
    {
        BcTlsCrypto crypto = new(new SecureRandom());
        RsaKeyPairGenerator rsaKeyPairGen = new();
        rsaKeyPairGen.Init(new KeyGenerationParameters(crypto.SecureRandom, 1024));

        AsymmetricCipherKeyPair caKeyPair = rsaKeyPairGen.GenerateKeyPair();
        AsymmetricCipherKeyPair cKeyPair = rsaKeyPairGen.GenerateKeyPair();

        Pkcs12Store store = new Pkcs12StoreBuilder().Build();
        X509CertificateEntry certEntry = new(PatchCertificateSignaturePattern(GenerateCertificate(SHA1CipherAlgorithm, subject,
            cKeyPair, caKeyPair.Private, GenerateCertificate(SHA1CipherAlgorithm, issuer, caKeyPair, caKeyPair.Private))));

        string certDomain = subject.Split("CN=")[1].Split(",")[0];

        LoggerAccessor.LogDebug("[ProtoSSL] - Certificate generated for: {domain}", certDomain);

        store.SetCertificateEntry(certDomain, certEntry);
        store.SetKeyEntry(certDomain, new AsymmetricKeyEntry(cKeyPair.Private), new[] { certEntry });

        return (cKeyPair.Private, new Certificate(new TlsCertificate[] { new BcTlsCertificate(crypto, certEntry.Certificate.GetEncoded()) }));
    }

    /// <summary>
    /// Generates a vulnerable certificate for vulnerable ProtoSSL versions.
    /// </summary>
    private (AsymmetricKeyParameter, Certificate) GenerateVulnerableLegacyCert(string RootCN, string Rootemail, string ChainCN, string Chainemail, bool WeakChainSignedRSAKey)
    {
        BcTlsCrypto crypto = new(new SecureRandom());
        SecureRandom random = crypto.SecureRandom;
        RsaKeyPairGenerator rsaKeyPairGen = new();
        rsaKeyPairGen.Init(new KeyGenerationParameters(random, 1024));
        RsaKeyPairGenerator rsaKeyPairGen0 = new();
        rsaKeyPairGen0.Init(new KeyGenerationParameters(random, WeakChainSignedRSAKey ? 512 : 1024));

        AsymmetricCipherKeyPair caKeyPair = rsaKeyPairGen.GenerateKeyPair();
        AsymmetricCipherKeyPair cKeyPair = rsaKeyPairGen0.GenerateKeyPair();

        Pkcs12Store store = new Pkcs12StoreBuilder().Build();
        X509CertificateEntry certEntry = new(PatchCertificateSignaturePattern(GenerateLegacyCertificate(MD5CipherAlgorithm, ChainCN, Chainemail,
            cKeyPair, caKeyPair.Private, GenerateLegacyCertificate(MD5CipherAlgorithm, RootCN, Rootemail, caKeyPair, caKeyPair.Private)), true));

        LoggerAccessor.LogDebug("[ProtoSSL] - Legacy Certificate generated for: {domain}", ChainCN);

        store.SetCertificateEntry(ChainCN, certEntry);
        store.SetKeyEntry(ChainCN, new AsymmetricKeyEntry(cKeyPair.Private), new[] { certEntry });

        return (cKeyPair.Private, new Certificate(new TlsCertificate[] { new BcTlsCertificate(crypto, certEntry.Certificate.GetEncoded()) }));
    }

    private static X509Certificate GenerateCertificate(string CipherAlgorithm, string subjectName, AsymmetricCipherKeyPair subjectKeyPair, AsymmetricKeyParameter issuerPrivKey, X509Certificate? issuerCert = null)
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

    private static X509Certificate GenerateLegacyCertificate(string CipherAlgorithm, string CN, string email, AsymmetricCipherKeyPair subjectKeyPair, AsymmetricKeyParameter issuerPrivKey, X509Certificate? issuerCert = null)
    {
        X509V3CertificateGenerator certGen = new();
        certGen.SetSerialNumber(BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), new SecureRandom()));
        certGen.SetIssuerDN(issuerCert == null ? BuildX509Name(CN, email) : issuerCert.SubjectDN);
        certGen.SetNotBefore(new DateTime(2008, 01, 01));
        certGen.SetNotAfter(new DateTime(2011, 08, 11)); // No choice, ProtoSSL cannot handle years after 2050 (client bug).
        certGen.SetSubjectDN(BuildX509Name(CN, email));
        certGen.SetPublicKey(subjectKeyPair.Public);
        return certGen.Generate(new Asn1SignatureFactory(CipherAlgorithm, issuerPrivKey));
    }

    private static X509Certificate PatchCertificateSignaturePattern(X509Certificate cCertificate, bool MD5Mode = false)
    {
        byte[] certDer = DotNetUtilities.ToX509Certificate(cCertificate).GetRawCertData();

        // There must be two signatures in the DER encoded certificate
        int signature1Offset = DataTypesUtils.FindBytePattern(certDer, MD5Mode ? MD5CipherSignature.Span : SHA1CipherSignature.Span);
        int signature2Offset = DataTypesUtils.FindBytePattern(certDer, MD5Mode ? MD5CipherSignature.Span : SHA1CipherSignature.Span, signature1Offset + (MD5Mode ? MD5CipherSignature.Length : SHA1CipherSignature.Length));

        if (signature1Offset == -1 || signature2Offset == -1)
            throw new Exception("[ProtoSSL] - Failed to find valid signature for patching!");

        // Patch the second signature to TLS_NULL_WITH_NULL_NULL
        certDer[signature2Offset + 8] = 0x01;

        using MemoryStream derStream = new(certDer);
        return new X509CertificateParser().ReadCertificate(derStream);
    }

    private static X509Name BuildX509Name(string CN, string email)
    {
        // build name attributes
        List<DerObjectIdentifier> nameOids = new();
        List<string> nameValues = new();

        foreach (KeyValuePair<string, DerObjectIdentifier> props in RDN_NAME_TO_BC_STYLE)
        {
            nameOids.Add(props.Value);
            nameValues.Add(props.Key);
        }

        nameOids.Add(X509Name.CN);
        nameValues.Add(CN);
        nameOids.Add(X509Name.EmailAddress);
        nameValues.Add(email);

        return new X509Name(nameOids, nameValues);
    }
}