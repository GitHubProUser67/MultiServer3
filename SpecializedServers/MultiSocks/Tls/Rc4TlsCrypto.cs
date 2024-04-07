using BackendProject.MiscUtils;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;

namespace MultiSocks.Tls;

/// <summary>
/// Original code from: https://github.com/zivillian/ism7mqtt
/// </summary>
public class Rc4TlsCrypto : BcTlsCrypto
{
    private readonly bool _writeSslKeyLog = false;

    public Rc4TlsCrypto(bool WriteKeyLog)
    {
        _writeSslKeyLog = WriteKeyLog;
    }

    public override TlsCipher CreateCipher(TlsCryptoParameters cryptoParams, int encryptionAlgorithm, int macAlgorithm)
    {
        if (_writeSslKeyLog)
        {
            var secret = VariousUtils.ReflectMasterSecretFromBCTls(cryptoParams.SecurityParameters.MasterSecret) ?? throw new Exception("[MultiSocks Rc4TlsCrypto] - Failed to reflect master secret");
            var clientRandom = Convert.ToHexString(cryptoParams.SecurityParameters.ClientRandom);
            var masterSecret = Convert.ToHexString(secret);

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

    private TlsRc4Cipher CreateCipher_RC4(TlsCryptoParameters cryptoParams, int cipherKeySize, int macAlgorithm)
    {
        return new TlsRc4Cipher(cryptoParams, cipherKeySize, CreateMac(cryptoParams, macAlgorithm),
            CreateMac(cryptoParams, macAlgorithm));
    }
}