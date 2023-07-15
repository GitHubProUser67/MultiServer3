using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Cryptography;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Models;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Models.ServerPlugins;
using PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.Medius.Models;
using System.Net;

namespace PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.Medius
{
    public class MAPS : BaseMediusComponent
    {
        static readonly IInternalLogger _logger = InternalLoggerFactory.GetInstance<MAPS>();

        protected override IInternalLogger Logger => _logger;

        public override int TCPPort => MediusClass.Settings.MAPSTCPPort;
        public override int UDPPort => MediusClass.Settings.MAPSUDPPort;


        public MAPS()
        {

        }

        protected override async Task ProcessMessage(BaseScertMessage message, IChannel clientChannel, ChannelData data)
        {
            // Get ScertClient data
            var scertClient = clientChannel.GetAttribute(Server.Pipeline.Constants.SCERT_CLIENT).Get();
            var enableEncryption = MediusClass.GetAppSettingsOrDefault(data.ApplicationId).EnableEncryption;
            scertClient.CipherService.EnableEncryption = enableEncryption;

            // 
            switch (message)
            {
                case RT_MSG_CLIENT_HELLO clientHello:
                    {
                        // send hello
                        Queue(new RT_MSG_SERVER_HELLO() { RsaPublicKey = enableEncryption ? MediusClass.Settings.DefaultKey.N : Org.BouncyCastle.Math.BigInteger.Zero }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CRYPTKEY_PUBLIC clientCryptKeyPublic:
                    {
                        // generate new client session key
                        scertClient.CipherService.GenerateCipher(CipherContext.RSA_AUTH, clientCryptKeyPublic.PublicKey.Reverse().ToArray());
                        scertClient.CipherService.GenerateCipher(CipherContext.RC_CLIENT_SESSION);

                        Queue(new RT_MSG_SERVER_CRYPTKEY_PEER() { SessionKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_TCP clientConnectTcp:
                    {
                        #region Check if AppId from Client matches Server
                        if (!MediusClass.Manager.IsAppIdSupported(clientConnectTcp.AppId))
                        {
                            Logger.Error($"Client {clientChannel.RemoteAddress} attempting to authenticate with incompatible app id {clientConnectTcp.AppId}");
                            await clientChannel.CloseAsync();
                            return;
                        }
                        #endregion

                        data.ApplicationId = clientConnectTcp.AppId;

                        Queue(new RT_MSG_SERVER_CONNECT_REQUIRE(), clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_REQUIRE clientConnectReadyRequire:
                    {
                        if (!scertClient.IsPS3Client)
                        {
                            Queue(new RT_MSG_SERVER_CRYPTKEY_GAME() { GameKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        }
                        Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                        {
                            PlayerId = 0,
                            ScertId = GenerateNewScertClientId(),
                            PlayerCount = 0x0001,
                            IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                        }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_TCP clientConnectReadyTcp:
                    {
                        Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                        Queue(new RT_MSG_SERVER_ECHO(), clientChannel);
                        break;
                    }
                case RT_MSG_SERVER_ECHO serverEchoReply:
                    {

                        break;
                    }
                case RT_MSG_CLIENT_ECHO clientEcho:
                    {
                        Queue(new RT_MSG_CLIENT_ECHO() { Value = clientEcho.Value }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_APP_TOSERVER clientAppToServer:
                    {

                        break;
                    }

                case RT_MSG_CLIENT_APP_TO_PLUGIN clientAppToPlugin:
                    {
                        ProcessMediusPluginMessage(clientAppToPlugin.Message, clientChannel, data);

                        //NetMessageProtocolInfo
                        //Queue(new RT_MSG_SERVER_PLUGIN_TO_APP() { Contents = Utils.FromString("000053100000002000006bd00000000") }, clientChannel);

                        break;
                    }

                case RT_MSG_SERVER_PLUGIN_TO_APP serverPluginToApp:
                    {

                        break;
                    }
                case RT_MSG_CLIENT_DISCONNECT _:
                case RT_MSG_CLIENT_DISCONNECT_WITH_REASON clientDisconnectWithReason:
                    {
                        data.State = ClientState.DISCONNECTED;
                        _ = clientChannel.CloseAsync();
                        break;
                    }
                default:
                    {
                        Logger.Warn($"UNHANDLED RT MESSAGE: {message}");

                        break;
                    }
            }
        }

        protected virtual void ProcessMediusPluginMessage(BaseMediusPluginMessage message, IChannel clientChannel, ChannelData data)
        {
            var scertClient = clientChannel.GetAttribute(Server.Pipeline.Constants.SCERT_CLIENT).Get();
            if (message == null)
            {
                Logger.Warn($"MessageType is Null!");
                return;
            }

            switch (message)
            {

                case NetMessageHello netMessageHello:
                    {
                        data.ClientObject = MediusClass.ProfileServer.ReserveClient(netMessageHello);

                        // Create client object
                        data.ClientObject.ApplicationId = data.ApplicationId;
                        data.ClientObject.MediusVersion = (int)scertClient.MediusVersion;
                        data.ClientObject.OnConnected();


                        var ProtoBytesReversed = ReverseBytesUInt(1958);
                        var BuildNumber = ReverseBytesUInt(10);
                        data.ClientObject.Queue(new NetMessageTypeProtocolInfo()
                        {
                            protocolInfo = ProtoBytesReversed, //1725
                            //protocolInfo = 1958,
                            buildNumber = 0
                        });

                        break;
                    }

                case NetMessageTypeProtocolInfo protocolInfo:
                    {
                        //Time
                        DateTime time = DateTime.Now;
                        var timeBS = time.Ticks >> 1;


                        //bool finBs = true >> 1;
                        //Content string bitshift
                        string newsBs = ShiftString("Test News");
                        string eulaBs = ShiftString("Test Eula");
                        // News/Eula Type bitshifted
                        var newsBS = Convert.ToInt32(NetMessageNewsEulaResponseContentType.News) >> 1;
                        var eulaBS = Convert.ToInt32(NetMessageNewsEulaResponseContentType.Eula) >> 1;

                        data.ClientObject.Queue(new NetMessageNewsEulaResponse()
                        {
                            m_finished = 1 >> 1,
                            m_content = newsBs,
                            m_type = (NetMessageNewsEulaResponseContentType)newsBS,
                            m_timestamp = timeBS
                        });

                        data.ClientObject.Queue(new NetMessageNewsEulaResponse()
                        {
                            m_finished = 1 >> 1,
                            m_content = eulaBs,
                            m_type = (NetMessageNewsEulaResponseContentType)eulaBS,
                            m_timestamp = timeBS
                        });
                        break;
                    }

                case NetMessageTypeKeepAlive keepAlive:
                    {
                        data.ClientObject.KeepAliveUntilNextConnection();
                        break;
                    }

                case NetMessageAccountLogoutRequest accountLogoutRequest:
                    {
                        /*
                        data.ClientObject.Queue(new NetMessageAccountLogoutResponse()
                        {
                            m_success = 0x0,
                        });
                        */
                        break;
                    }

                default:
                    {
                        Logger.Warn($"Unhandled Medius Plugin Message: {message}");
                        break;
                    }
            }
        }

        public static string ShiftString(string t)
        {
            return t.Substring(1, t.Length - 1) + t.Substring(0, 1);
        }
        public static uint ReverseBytesUInt(uint value)
        {
            return (uint)((value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24);
        }
        public static int ReverseBytesInt(int value)
        {
            return (int)((value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24);
        }

        #region ReverseBytes16
        /// <summary>
        /// Reverses UInt16 
        /// </summary>
        /// <param name="nValue"></param>
        /// <returns></returns>
        public static ushort ReverseBytes16(ushort nValue)
        {
            return (ushort)((ushort)((nValue >> 8)) | (nValue << 8));
        }
        #endregion

        public ClientObject ReserveClient(NetMessageHello request)
        {
            var client = new ClientObject();
            client.BeginSession();
            return client;
        }
    }
}
