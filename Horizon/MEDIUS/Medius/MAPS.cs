using CustomLogger;
using DotNetty.Transport.Channels;
using BackendProject.Horizon.RT.Cryptography;
using BackendProject.Horizon.RT.Models;
using BackendProject.Horizon.RT.Models.ServerPlugins;
using Horizon.MEDIUS.Medius.Models;
using System.Net;

namespace Horizon.MEDIUS.Medius
{
    public class MAPS : BaseMediusComponent
    {
        public override int TCPPort => MediusClass.Settings.MAPSTCPPort;
        public override int UDPPort => MediusClass.Settings.MAPSUDPPort;

        public MAPS()
        {

        }

        protected override async Task ProcessMessage(BaseScertMessage message, IChannel clientChannel, ChannelData data)
        {
            // Get ScertClient data
            var scertClient = clientChannel.GetAttribute(BackendProject.Horizon.LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            bool enableEncryption = MediusClass.GetAppSettingsOrDefault(data.ApplicationId).EnableEncryption;
            if (scertClient.CipherService != null)
                scertClient.CipherService.EnableEncryption = enableEncryption;

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
                        if (clientCryptKeyPublic.PublicKey != null)
                        {
                            // generate new client session key
                            scertClient.CipherService?.GenerateCipher(CipherContext.RSA_AUTH, clientCryptKeyPublic.PublicKey.Reverse().ToArray());
                            scertClient.CipherService?.GenerateCipher(CipherContext.RC_CLIENT_SESSION);

                            Queue(new RT_MSG_SERVER_CRYPTKEY_PEER() { SessionKey = scertClient.CipherService?.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        }
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_TCP clientConnectTcp:
                    {
                        #region Check if AppId from Client matches Server
                        if (!MediusClass.Manager.IsAppIdSupported(clientConnectTcp.AppId))
                        {
                            LoggerAccessor.LogError($"Client {clientChannel.RemoteAddress} attempting to authenticate with incompatible app id {clientConnectTcp.AppId}");
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
                            Queue(new RT_MSG_SERVER_CRYPTKEY_GAME() { GameKey = scertClient.CipherService?.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
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
                        if (clientAppToPlugin.Message != null)
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
                        LoggerAccessor.LogWarn($"UNHANDLED RT MESSAGE: {message}");

                        break;
                    }
            }
        }

        protected virtual void ProcessMediusPluginMessage(BaseMediusPluginMessage message, IChannel clientChannel, ChannelData data)
        {
            var scertClient = clientChannel.GetAttribute(BackendProject.Horizon.LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            if (message == null)
            {
                LoggerAccessor.LogWarn($"MessageType is Null!");
                return;
            }

            switch (message)
            {

                case NetMessageHello netMessageHello:
                    {
                        data.ClientObject = MediusClass.ProfileServer.ReserveClient(netMessageHello);

                        // Create client object
                        data.ClientObject.ApplicationId = data.ApplicationId;
                        data.ClientObject.MediusVersion = scertClient.MediusVersion ?? 0;
                        data.ClientObject.OnConnected();

                        data.ClientObject.Queue(new NetMAPSHelloMessage()
                        {
                            m_success = false,
                            m_isOnline = false,
                            m_availableFactions = new byte[3] { 1, 2, 3 }
                        });

                        /*
                        var ProtoBytesReversed = ReverseBytesUInt(1725);
                        var BuildNumber = ReverseBytesUInt(0);
                        data.ClientObject.Queue(new NetMessageTypeProtocolInfo()
                        {
                            protocolInfo = ProtoBytesReversed, //1725 //1958
                            //protocolInfo = 1958,
                            buildNumber = BuildNumber
                        });
                        */

                        break;
                    }

                case NetMessageTypeProtocolInfo protocolInfo:
                    {
                        //Time
                        DateTime time = DateTime.Now;
                        long timeBS = time.Ticks >> 1;

                        //bool finBs = true >> 1;
                        //Content string bitshift
                        string newsBs = ShiftString("Test News");
                        string eulaBs = ShiftString("Test Eula");
                        // News/Eula Type bitshifted
                        int newsBS = 0;//Convert.ToInt32(NetMessageNewsEulaResponseContentType.News) >> 1;
                        int eulaBS = 1;//Convert.ToInt32(NetMessageNewsEulaResponseContentType.Eula) >> 1;

                        byte[] sequence = new byte[1];
                        byte[] type = new byte[1];

                        /*
                        data.ClientObject.Queue(new NetMessageNewsEulaRequest()
                        {
                            m_languageExtension = "",
                        });
                        */
                        /*
                        data.ClientObject.Queue(new NetMessageNewsEulaResponse()
                        {
                            m_finished = BitShift(sequence, 1).First(),
                            m_content = newsBs,
                            m_type = (NetMessageNewsEulaResponseContentType)BitShift(type, 1).First(),
                            m_timestamp = timeBS
                        });
                        data.ClientObject.Queue(new NetMessageNewsEulaResponse()
                        {
                            m_finished = 1,
                            m_content = eulaBs,
                            m_type = (NetMessageNewsEulaResponseContentType)eulaBS,
                            m_timestamp = timeBS
                        });
                        
                        */

                        break;
                    }

                    /*
                case NetMessageTypeKeepAlive keepAlive:
                    {
                        data.ClientObject.KeepAliveUntilNextConnection();
                        break;
                    }
                    */

                case NetMessageAccountLogoutRequest accountLogoutRequest:
                    {
                        bool success = true;
                        data.ClientObject?.Queue(new NetMessageAccountLogoutResponse()
                        {
                            m_success = success,
                        });

                        break;
                    }

                default:
                    {
                        LoggerAccessor.LogWarn($"Unhandled Medius Plugin Message: {message}");
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

        public byte[] BitShift(byte[] sequence, int length)
        {
            // Check if the length is valid
            if (length <= 0 || length >= 8)
            {
                LoggerAccessor.LogError("[MAPS] - Invalid shift length. The length must be between 1 and 7.");
                return Array.Empty<byte>();
            }

            // Perform the bitwise shift operation
            byte[] shiftedSequence = new byte[sequence.Length];
            for (int i = 0; i < sequence.Length; i++)
            {
                shiftedSequence[i] = (byte)(sequence[i] << length);
            }

            return shiftedSequence;
        }

        public ClientObject ReserveClient(NetMessageHello request)
        {
            ClientObject client = new();
            client.BeginSession();
            return client;
        }
    }
}
