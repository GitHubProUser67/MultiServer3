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
                //Currently all placeholder master blaze ports for testing!
                //Mass Effect 2 PS3
                case "MassEffect2-ps3":
                    secure = false;
                    port = 33152;
                    break;
                case "MassEffect3-ps3":
                    secure = false;
                    port = 33152;
                    break;
                //PS3 Crysis 3 Open beta
                case "C3 Open Beta":
                    secure = false;
                    port = 33152;
                    break;
                //Battlefield 3
                case "bf3 client":
                    secure = false;
                    port = 33152;
                    break;
                //Battlefield 4
                case "warsaw client":
                    secure = false;
                    port = 33152;
                    break;
                // NFS: Rivals
                case "test":
                    secure = false;
                    port = 33152;
                    break;
                //Army of Two: The Devil's Cartel
                case "AO4 Client":
                    secure = false;
                    port = 33152;
                    break;
                //NFS Most Wanted
                case "NFS Hot Pursuit":
                    secure = false;
                    port = 33152;
                    break;
                //PS3 Dead Space 2
                case "/PS3/DEADSPACE-2011":
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

            string ip = MultiSocksServerConfiguration.UsePublicIPAddress ? InternetProtocolUtils.GetPublicIPAddress() : InternetProtocolUtils.GetLocalIPAddress().ToString();

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
