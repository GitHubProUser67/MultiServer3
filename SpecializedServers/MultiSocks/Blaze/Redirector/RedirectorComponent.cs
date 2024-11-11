using Blaze3SDK;
using Blaze3SDK.Blaze.Redirector;
using Blaze3SDK.Components;
using BlazeCommon;
using CustomLogger;
using MultiSocks.Utils;

namespace MultiSocks.Blaze.Redirector
{
    internal class RedirectorComponent : RedirectorComponentBase.Server
    {
        /// <summary>
        /// You only need to override the base method to handle new requests.
        /// If the request type or/and response type is NullStruct, you can change the request/response types in the Component Base.
        /// </summary>
        public override Task<ServerInstanceInfo> GetServerInstanceAsync(ServerInstanceRequest request, BlazeRpcContext context)
        {
            //if needded access underlying connection which issued the request and other stuff
            ProtoFireConnection connection = context.Connection;

            //manually displaying some data
            LoggerAccessor.LogInfo($"[Blaze] - Redirector: Connection Id    : {connection.ID}");
            LoggerAccessor.LogInfo($"[Blaze] - Redirector: Service Name     : {request.mName}");
            LoggerAccessor.LogInfo($"[Blaze] - Redirector: Client Type      : {request.mClientType}");
            LoggerAccessor.LogInfo($"[Blaze] - Redirector: Client Platform  : {request.mPlatform}");

            //if something is wrong with request data - thrown an BlazeRpcException with error code and error data, which will be sent to client (in debug environment disable breaking on this exception)
            if (request.mName == "someValueHere")
            {
                throw new BlazeRpcException(Blaze3RpcError.REDIRECTOR_UNKNOWN_SERVICE_NAME, new ServerInstanceError()
                {
                    mMessages = new List<string>() {
                        "Unknown service name"
                    }
                });
            }

            //Connection details
            bool secure = false; //insecure
            string ip = "127.0.0.1";
            ushort port = 33152;

            ServerInstanceInfo responseData = new()
            {
                //this is an union type, so we specify only one of the values
                mAddress = new ServerAddress()
                {
                    IpAddress = new IpAddress()
                    {
                        mHostname = ip,
                        mIp = NetworkUtils.GetIPAddressAsUInt(ip),
                        mPort = port
                    },
                },

                //optionally address remaps can be specified
                mAddressRemaps = new List<AddressRemapEntry>()
                {

                },

                //optionally server messages can be specified
                mMessages = new List<string>() {
                    "Hello, from Multiserver!"
                },

                //optionally name remaps can be specified
                mNameRemaps = new List<NameRemapEntry>()
                {

                },
                mSecure = secure,
                mDefaultDnsAddress = 0
            };

            //return the response
            return Task.FromResult(responseData);
        }
    }
}
