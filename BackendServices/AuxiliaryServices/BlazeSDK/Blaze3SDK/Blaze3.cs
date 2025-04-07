using BlazeCommon;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Tdf;

namespace Blaze3SDK
{
    public static class Blaze3
    {
        static TdfFactory factory;
        static TdfEncoder encoder;
        static TdfDecoder decoder;

        static Blaze3()
        {
            factory = new TdfFactory();
            encoder = factory.CreateEncoder(true);
            decoder = factory.CreateDecoder(true);
        }

        public static BlazeServer CreateBlazeServer(string name, IPEndPoint endPoint, X509Certificate? certificate = null, bool forceSsl = true, ConnectionDelegate? onNewConnection = null, ConnectionDelegate? onDisconnected = null, ConnectionOnRequestDelegate? onRequest = null, ConnectionOnErrorDelegate? onError = null)
        {
            BlazeServerConfiguration blaze3Settings = new BlazeServerConfiguration(name, endPoint, encoder, decoder)
            {
                Certificate = certificate,
                ForceSsl = forceSsl,
                ComponentNotFoundErrorCode = (int)Blaze3RpcError.ERR_COMPONENT_NOT_FOUND,
                CommandNotFoundErrorCode = (int)Blaze3RpcError.ERR_COMMAND_NOT_FOUND,
                ErrSystemErrorCode = (int)Blaze3RpcError.ERR_SYSTEM,
                OnNewConnection = onNewConnection,
                OnDisconnected = onDisconnected,
                OnRequest = onRequest,
                OnError = onError
            };

            return new BlazeServer(blaze3Settings);
        }

        public static BlazeClientConnection CreateBlazeClientConnection(ProtoFireConnection connection)
        {
            BlazeClientConfiguration config = new BlazeClientConfiguration(encoder, decoder);
            return new BlazeClientConnection(connection, config);
        }
    }
}
