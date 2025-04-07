using Blaze3SDK;
using Blaze3SDK.Blaze.Redirector;
using Blaze3SDK.Components;
using BlazeCommon;
using CustomLogger;
using MultiSocks.Utils;
using NetworkLibrary.Extension;

namespace MultiSocks.Blaze.Components.Redirector
{
    internal class RedirectorComponent : RedirectorComponentBase.Server
    {
        public override Task<ServerInstanceInfo> GetServerInstanceAsync(ServerInstanceRequest request, BlazeRpcContext context)
        {
#if DEBUG
            LoggerAccessor.LogInfo($"[Blaze] - Redirector: Connection Id    : {context.Connection.ID}");
            LoggerAccessor.LogInfo($"[Blaze] - Redirector: Client Name  : {request.mClientName}");
            LoggerAccessor.LogInfo($"[Blaze] - Redirector: Client Type      : {request.mClientType}");
            LoggerAccessor.LogInfo($"[Blaze] - Redirector: Client SkuId  : {request.mClientSkuId}");
            LoggerAccessor.LogInfo($"[Blaze] - Redirector: Client Environment  : {request.mEnvironment}");
#endif
            bool secure = MultiSocksServerConfiguration.EnableBlazeEncryption;
            ushort port;

            switch (request.mClientName)
            {
                case "MassEffect3-ps3":
                    secure = false;
                    port = 33152;
                    break;
                default:
                    throw new BlazeRpcException(Blaze3RpcError.REDIRECTOR_UNKNOWN_SERVICE_NAME, new ServerInstanceError()
                    {
                        mMessages = new List<string>() {
                        "Unknown game requested! Please report to GITHUB."
                    }
                    });
            }

            string ip = MultiSocksServerConfiguration.UsePublicIPAddress ? InternetProtocolUtils.GetPublicIPAddress() : InternetProtocolUtils.GetLocalIPAddresses().First().ToString();

            return Task.FromResult(new ServerInstanceInfo()
            {
                //this is an union type, so we specify only one of the values
                mAddress = new ServerAddress()
                {
                    IpAddress = new IpAddress()
                    {
                        mHostname = ip,
                        mIp = InternetProtocolUtils.GetIPAddressAsUInt(ip),
                        mPort = port
                    },
                },

                mSecure = secure,
                mDefaultDnsAddress = 0
            });
        }
    }
}
