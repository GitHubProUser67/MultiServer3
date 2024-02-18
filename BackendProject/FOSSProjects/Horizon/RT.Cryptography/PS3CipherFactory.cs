using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace BackendProject.Horizon.RT.Cryptography
{
    public class PS3CipherFactory : ICipherFactory
    {
        private static Random RNG = new();

        public ICipher CreateNew(CipherContext context)
        {
            if (context == CipherContext.RSA_AUTH)
                return CreateAsym();

            return CreateSym(context);
        }

        public ICipher CreateNew(CipherContext context, byte[] publicKey)
        {
            if (context == CipherContext.RSA_AUTH)
                return CreateAsymFromPublicKey(publicKey);

            return CreateSymFromPublicKey(context, publicKey);
        }

        public ICipher CreateNew(RSA.RsaKeyPair rsaKeyPair)
        {
            return rsaKeyPair.ToPS3();
        }

        private ICipher CreateSym(CipherContext context)
        {
            // generate random series of bytes
            var b = new byte[0x40];
            RNG.NextBytes(b);

            return new RC.PS3_RCQ(b, context);
        }

        private ICipher CreateSymFromPublicKey(CipherContext context, byte[] publicKey)
        {
            return new RC.PS3_RCQ(publicKey, context);
        }

        private ICipher CreateAsym()
        {
            // generate key
            RsaKeyPairGenerator rsa = new RsaKeyPairGenerator();
            BigInteger e = new BigInteger("17");

            var param = new RsaKeyGenerationParameters(
                e,
                new SecureRandom(),
                512,
                5
                );
            rsa.Init(param);
            var keypair = rsa.GenerateKeyPair();

            // pull modulus and private exp
            var n = (BigInteger)keypair.Public.GetType().GetProperty("Modulus").GetValue(keypair.Public);
            var d = (BigInteger)keypair.Private.GetType().GetProperty("Exponent").GetValue(keypair.Private);

            // 
            return new RSA.PS3_RSA(n, e, d);
        }

        private ICipher CreateAsymFromPublicKey(byte[] publicKey)
        {
            BigInteger e = new BigInteger("17");
            return new RSA.PS3_RSA(new BigInteger(1, publicKey), e, e);
        }
    }
}