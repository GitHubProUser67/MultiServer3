using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System.Reflection;

namespace BackendProject.FOSSProjects.VulnerableSSLv3Server.Crypto;

/// <summary>
/// Original code from: https://github.com/zivillian/ism7mqtt
/// </summary>
public class Rc4TlsCrypto : BcTlsCrypto
{
    private readonly bool _writeSslKeyLog = false;

    public Rc4TlsCrypto(bool writeSslKeyLog)
    {
        _writeSslKeyLog = writeSslKeyLog;
    }

    public override TlsCipher CreateCipher(TlsCryptoParameters cryptoParams, int encryptionAlgorithm, int macAlgorithm)
    {
        if (_writeSslKeyLog)
        {
            byte[] secret = ReflectMasterSecretFromBCTls(cryptoParams.SecurityParameters.MasterSecret) ?? throw new Exception("Failed to reflect master secret");
            string clientRandom = Convert.ToHexString(cryptoParams.SecurityParameters.ClientRandom);
            string masterSecret = Convert.ToHexString(secret);

            using StreamWriter sw = File.AppendText("sslkeylog.log");
            sw.WriteLine("CLIENT_RANDOM " + clientRandom + " " + masterSecret);
        }

        return encryptionAlgorithm switch
        {
            EncryptionAlgorithm.RC4_128 => CreateCipher_RC4(cryptoParams, 16, macAlgorithm),
            EncryptionAlgorithm.RC4_40 => CreateCipher_RC4(cryptoParams, 5, macAlgorithm),
            _ => base.CreateCipher(cryptoParams, encryptionAlgorithm, macAlgorithm),
        };
    }

    public override bool HasEncryptionAlgorithm(int encryptionAlgorithm)
    {
        return encryptionAlgorithm switch
        {
            EncryptionAlgorithm.RC4_128 or EncryptionAlgorithm.RC4_40 => true,
            _ => base.HasEncryptionAlgorithm(encryptionAlgorithm),
        };
    }

    public byte[]? ReflectMasterSecretFromBCTls(TlsSecret secret)
    {
        // We need to use reflection to access the master secret from BC
        // because using Extract() destroys the key for subsequent calls
        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        return (byte[]?)typeof(BcTlsSecret).GetField("m_data", bindingFlags)?.GetValue(secret);
    }

    private TlsRc4Cipher CreateCipher_RC4(TlsCryptoParameters cryptoParams, int cipherKeySize, int macAlgorithm)
    {
        return new TlsRc4Cipher(cryptoParams, cipherKeySize, CreateMac(cryptoParams, macAlgorithm),
            CreateMac(cryptoParams, macAlgorithm));
    }
}