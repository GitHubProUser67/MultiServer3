using System;
using System.IO;

using MultiServer.Addons.Org.BouncyCastle.Asn1;
using MultiServer.Addons.Org.BouncyCastle.Asn1.X509;
using MultiServer.Addons.Org.BouncyCastle.Crypto.IO;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Utilities;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Operators
{
    public class Asn1CipherBuilderWithKey : ICipherBuilderWithKey
    {
        private readonly KeyParameter encKey;
        private AlgorithmIdentifier algorithmIdentifier;

        public Asn1CipherBuilderWithKey(DerObjectIdentifier encryptionOID, int keySize, SecureRandom random)
        {
            random = CryptoServicesRegistrar.GetSecureRandom(random);

            CipherKeyGenerator keyGen = CipherKeyGeneratorFactory.CreateKeyGenerator(encryptionOID, random);

            encKey = keyGen.GenerateKeyParameter();
            algorithmIdentifier = AlgorithmIdentifierFactory.GenerateEncryptionAlgID(encryptionOID, encKey.KeyLength * 8, random);
        }

        public object AlgorithmDetails
        {
            get { return algorithmIdentifier; }
        }

        public int GetMaxOutputSize(int inputLen)
        {
            throw new NotImplementedException();
        }

        public ICipher BuildCipher(Stream stream)
        {
            object cipher = CipherFactory.CreateContentCipher(true, encKey, algorithmIdentifier);

            //
            // BufferedBlockCipher
            // IStreamCipher
            //

            if (cipher is IStreamCipher)
            {
                cipher = new BufferedStreamCipher((IStreamCipher)cipher);
            }

            if (stream == null)
            {
                stream = new MemoryStream();
            }

            return new BufferedCipherWrapper((IBufferedCipher)cipher, stream);
        }

        public ICipherParameters Key
        {
            get { return encKey; }
        }
    }

    public class BufferedCipherWrapper : ICipher
    {
        private readonly IBufferedCipher bufferedCipher;
        private readonly CipherStream stream;

        public BufferedCipherWrapper(IBufferedCipher bufferedCipher, Stream source)
        {
            this.bufferedCipher = bufferedCipher;
            stream = new CipherStream(source, bufferedCipher, bufferedCipher);
        }

        public int GetMaxOutputSize(int inputLen)
        {
            return bufferedCipher.GetOutputSize(inputLen);
        }

        public int GetUpdateOutputSize(int inputLen)
        {
            return bufferedCipher.GetUpdateOutputSize(inputLen);
        }

        public Stream Stream
        {
            get { return stream; }
        }
    }
}
