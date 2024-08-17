using CustomLogger;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Horizon.RT.Common;
using Horizon.RT.Cryptography;
using Horizon.RT.Models;
using Horizon.LIBRARY.Pipeline.Tcp;
using Horizon.LIBRARY.Common;
using Horizon.DME.Models;
using System.Collections.Concurrent;
using System.Net;
using DotNetty.Handlers.Timeout;
using Horizon.DME.PluginArgs;
using Horizon.PluginManager;
using System.Collections.Generic;
using System.Linq;
using EndianTools;
using CyberBackendLibrary.Extension;

namespace Horizon.DME
{
    public class TcpServer
    {
        public static Random RNG = new();

        public bool IsRunning => _boundChannel != null && _boundChannel.Active;

        public int Port => DmeClass.Settings.TCPPort;

        protected IEventLoopGroup? _bossGroup = null;
        protected IEventLoopGroup? _workerGroup = null;
        protected IChannel? _boundChannel = null;
        protected ScertServerHandler? _scertHandler = null;
        private ushort _clientCounter = 0;

        protected internal class ChannelData
        {
            public int ApplicationId { get; set; } = 0;
            public bool Ignore { get; set; } = false;
            public ClientObject? ClientObject { get; set; } = null;
            public ConcurrentQueue<BaseScertMessage> RecvQueue { get; } = new();
            public ConcurrentQueue<BaseScertMessage> SendQueue { get; } = new();
            public DateTime TimeConnected { get; set; } = Utils.GetHighPrecisionUtcTime();


            /// <summary>
            /// Timesout client if they authenticated after a given number of seconds.
            /// </summary>
            public bool ShouldDestroy => ClientObject == null && (Utils.GetHighPrecisionUtcTime() - TimeConnected).TotalSeconds > DmeClass.GetAppSettingsOrDefault(ApplicationId).ClientTimeoutSeconds;
        }

        protected ConcurrentQueue<IChannel> _forceDisconnectQueue = new();
        protected ConcurrentDictionary<string, ChannelData> _channelDatas = new();
        protected ConcurrentDictionary<uint, ClientObject> _scertIdToClient = new();

        /// <summary>
        /// Start the Dme Tcp Server.
        /// </summary>
        public virtual async void Start()
        {
            _bossGroup = new MultithreadEventLoopGroup(1);
            _workerGroup = new MultithreadEventLoopGroup();
            _scertHandler = new ScertServerHandler();

            // Add client on connect
            _scertHandler.OnChannelActive += (channel) =>
            {
                string key = channel.Id.AsLongText();
                _channelDatas.TryAdd(key, new ChannelData());
            };

            // Remove client on disconnect
            _scertHandler.OnChannelInactive += (channel) =>
            {
                string key = channel.Id.AsLongText();
                if (_channelDatas.TryRemove(key, out var data))
                {
                    if (data.ClientObject != null)
                    {
                        data.ClientObject.OnTcpDisconnected();
                        _scertIdToClient.TryRemove(data.ClientObject.ScertId, out _);
                    }
                }
            };

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += async (channel, message) =>
            {
                string key = channel.Id.AsLongText();
                if (_channelDatas.TryGetValue(key, out var data))
                {
                    if (!data.Ignore && (data.ClientObject == null || !data.ClientObject.IsDestroyed))
                    {

                        OnTcpMsg pluginArgs = new()
                        {
                            Player = data.ClientObject,
                            Packet = message
                        };

                        // Plugin
                        await DmeClass.Plugins.OnEvent(PluginEvent.DME_GAME_ON_RECV_TCP, pluginArgs);

                        data.RecvQueue.Enqueue(message);

                        if (message is RT_MSG_SERVER_ECHO serverEcho)
                            data.ClientObject?.OnRecvServerEcho(serverEcho);
                        else if (message is RT_MSG_CLIENT_ECHO clientEcho)
                            data.ClientObject?.OnRecvClientEcho(clientEcho);
                    }
                }

                // Log if id is set
                if (message.CanLog())
                    LoggerAccessor.LogDebug($"TCP {data?.ClientObject},{channel}: {message}");
            };

            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(_bossGroup, _workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .Handler(new LoggingHandler(LogLevel.INFO))
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    pipeline.AddLast(new WriteTimeoutHandler(30));
                    pipeline.AddLast(new ScertEncoder());
                    pipeline.AddLast(new ScertIEnumerableEncoder());
                    pipeline.AddLast(new ScertTcpFrameDecoder(DotNetty.Buffers.ByteOrder.LittleEndian, 2048, 1, 2, 0, 0, false));
                    pipeline.AddLast(new ScertDecoder());
                    pipeline.AddLast(new ScertMultiAppDecoder());
                    pipeline.AddLast(_scertHandler);
                }));

            _boundChannel = await bootstrap.BindAsync(Port);
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public virtual async Task Stop()
        {
            try
            {
                if (_boundChannel != null)
                {
                    await _boundChannel.CloseAsync();
                    _boundChannel = null;
                }
            }
            finally
            {
                if (_bossGroup != null && _workerGroup != null)
                    await Task.WhenAll(
                        _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                        _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        /// <summary>
        /// Process incoming messages.
        /// </summary>
        public async Task HandleIncomingMessages()
        {
            if (_scertHandler == null || _scertHandler.Group == null)
                return;

#if NET8_0_OR_GREATER
            /* ToArray() is necessary, else, weird issues happens in NET8.0+ (https://github.com/dotnet/runtime/issues/105576) */
            await Task.WhenAll(_scertHandler.Group.ToArray().Select(c => HandleIncomingMessages(c)));
#else
            await Task.WhenAll(_scertHandler.Group.Select(c => HandleIncomingMessages(c)));
#endif
        }

        /// <summary>
        /// Process outgoing messages.
        /// </summary>
        public async Task HandleOutgoingMessages()
        {
            if (_scertHandler == null || _scertHandler.Group == null)
                return;

#if NET8_0_OR_GREATER
            /* ToArray() is necessary, else, weird issues happens in NET8.0+ (https://github.com/dotnet/runtime/issues/105576) */
            await Task.WhenAll(_scertHandler.Group.ToArray().Select(c => HandleOutgoingMessages(c)));
#else
            await Task.WhenAll(_scertHandler.Group.Select(c => HandleOutgoingMessages(c)));
#endif

            // Disconnect and remove timedout unauthenticated channels
            while (_forceDisconnectQueue.TryDequeue(out var channel))
            {
                // Send disconnect message
                _ = ForceDisconnectClient(channel);

                // Remove
                _channelDatas.TryRemove(channel.Id.AsLongText(), out var d);
                LoggerAccessor.LogWarn($"REMOVING CHANNEL {channel},{d},{d?.ClientObject}");

                // close after 5 seconds
                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    try
                    {
                        await channel.CloseAsync();
                    }
                    catch (Exception) { }
                });
            }
        }

        private async Task HandleIncomingMessages(IChannel clientChannel)
        {
            if (clientChannel == null)
                return;

            string key = clientChannel.Id.AsLongText();

            try
            {
                if (_channelDatas.TryGetValue(key, out var data))
                {
                    // Process all messages in queue
                    while (data.RecvQueue.TryDequeue(out var message))
                    {
                        try
                        {
                            if (!await PassMessageToPlugins(clientChannel, data, message, true))
                                await ProcessMessage(message, clientChannel, data);
                        }
                        catch (Exception e)
                        {
                            LoggerAccessor.LogError(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);
            }
        }

        private async Task HandleOutgoingMessages(IChannel clientChannel)
        {
            if (clientChannel == null)
                return;

            List<BaseScertMessage> responses = new();
            string key = clientChannel.Id.AsLongText();

            try
            {
                if (_channelDatas.TryGetValue(key, out var data))
                {
                    // Destroy
                    if (data.ShouldDestroy)
                    {
                        _forceDisconnectQueue.Enqueue(clientChannel);
                        return;
                    }

                    // Disconnect on destroy
                    if (data.ClientObject != null && data.ClientObject.IsDestroyed)
                    {
                        data.Ignore = true;
                        return;
                    }

                    // Send if writeable
                    if (clientChannel.IsWritable)
                    {
                        // Add send queue to responses
                        while (data.SendQueue.TryDequeue(out var message))
                            if (!await PassMessageToPlugins(clientChannel, data, message, false))
                                responses.Add(message);

                        if (data.ClientObject != null)
                        {
                            // Echo
                            if (data.ClientObject.MediusVersion > 108 && (Utils.GetHighPrecisionUtcTime() - data.ClientObject.UtcLastServerEchoSent).TotalSeconds > DmeClass.GetAppSettingsOrDefault(data.ClientObject.ApplicationId).ServerEchoIntervalSeconds)
                            {
                                var message = new RT_MSG_SERVER_ECHO();
                                if (!await PassMessageToPlugins(clientChannel, data, message, false))
                                    responses.Add(message);
                                data.ClientObject.UtcLastServerEchoSent = Utils.GetHighPrecisionUtcTime();
                            }

                            // Add client object's send queue to responses
                            // But only if not in a world
                            if (data.ClientObject.DmeWorld == null || data.ClientObject.DmeWorld.Destroyed)
                                while (data.ClientObject.TcpSendMessageQueue.TryDequeue(out var message))
                                    if (!await PassMessageToPlugins(clientChannel, data, message, false))
                                        responses.Add(message);
                        }

                        if (responses.Count > 0)
                            _ = clientChannel.WriteAndFlushAsync(responses);
                    }
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);
            }
        }

        #region Message Processing

        protected async Task ProcessMessage(BaseScertMessage message, IChannel clientChannel, ChannelData data)
        {
            // Get ScertClient data
            var scertClient = clientChannel.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            bool enableEncryption = false /*DmeClass.GetAppSettingsOrDefault(data.ApplicationId).EnableDmeEncryption*/;
            if (scertClient.CipherService != null)
                scertClient.CipherService.EnableEncryption = enableEncryption;

            switch (message)
            {
                case RT_MSG_CLIENT_HELLO clientHello:
                    {
                        // send hello
                        Queue(new RT_MSG_SERVER_HELLO() { RsaPublicKey = enableEncryption ? DmeClass.Settings.DefaultKey.N : Org.BouncyCastle.Math.BigInteger.Zero }, clientChannel);
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
                case RT_MSG_CLIENT_CONNECT_TCP_AUX_UDP clientConnectTcpAuxUdp:
                    {
                        /*
                        if (clientConnectTcpAuxUdp.Key == null)
                        {
                            scertClient.CipherService.GenerateCipher(CipherContext.RC_CLIENT_SESSION);

                            if (scertClient.CipherService.HasKey(CipherContext.RC_CLIENT_SESSION))
                            {
                                Queue(new RT_MSG_SERVER_CRYPTKEY_GAME() { GameKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                            }
                        }
                        */

                        data.ApplicationId = clientConnectTcpAuxUdp.AppId;
                        data.ClientObject = DmeClass.GetMPSClientByAccessToken(clientConnectTcpAuxUdp.AccessToken);

                        if (data.ClientObject == null)
                        {
                            LoggerAccessor.LogWarn("Access Token for client not found, fallback to Sessionkey!");
                            data.ClientObject = DmeClass.GetMPSClientBySessionKey(clientConnectTcpAuxUdp.SessionKey);
                            if (data.ClientObject != null)
                            {
                                LoggerAccessor.LogWarn("CLIENTOBJECT FALLBACK FOUND!!");
                                //var clients = Program.GetClients(clientConnectTcpAuxUdp.AppId);
                                //Logger.Warn($"Clients Count for AppId {clients.Count()}");
                                /*
                                foreach (var client in clients)
                                {
                                    if (client.Token == clientConnectTcp.AccessToken)
                                    {

                                        LoggerAccessor.LogWarn("CLIENTOBJECT FALLBACK FOUND!!");
                                        data.ClientObject = client;
                                    }
                                }
                                */
                            }
                            else
                            {
                                LoggerAccessor.LogWarn("AccessToken and SessionKey null! FALLBACK WITH NEW CLIENTOBJECT!");
                                //var clients = Program.GetClientsByAppId(clientConnectTcpAuxUdp.AppId);
                                //data.ClientObject = clients.Where(x => x.Token == clientConnectTcpAuxUdp.AccessToken).FirstOrDefault();  
                                ClientObject clientObject = new(clientConnectTcpAuxUdp.SessionKey ?? string.Empty)
                                {
                                    ApplicationId = clientConnectTcpAuxUdp.AppId
                                };
                                data.ClientObject = clientObject;
                            }
                        }

                        /*
                        if (data.ClientObject == null || data.ClientObject.DmeWorld == null || data.ClientObject.DmeWorld.WorldId != clientConnectTcpAuxUdp.ARG1)
                            throw new Exception($"Client connected with invalid world id!");
                        */
                        data.ClientObject.ApplicationId = clientConnectTcpAuxUdp.AppId;
                        data.ClientObject.OnTcpConnected(clientChannel);
                        data.ClientObject.ScertId = GenerateNewScertClientId();
                        data.ClientObject.MediusVersion = scertClient.MediusVersion;
                        if (!_scertIdToClient.TryAdd(data.ClientObject.ScertId, data.ClientObject))
                        {
                            LoggerAccessor.LogError($"Duplicate scert client id");
                            break;
                        }

                        // start udp server
                        data.ClientObject.BeginUdp();

                        if (scertClient.MediusVersion > 108 || scertClient.IsPS3Client)
                            Queue(new RT_MSG_SERVER_CONNECT_REQUIRE() { MaxPacketSize = Constants.MEDIUS_MESSAGE_MAXLEN, MaxUdpPacketSize = Constants.MEDIUS_UDP_MESSAGE_MAXLEN }, clientChannel);
                        else if (data.ClientObject.DmeWorld != null)
                        {
                            Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                            {
                                PlayerId = (ushort)data.ClientObject.DmeId,
                                ScertId = data.ClientObject.ScertId,
                                PlayerCount = (ushort)data.ClientObject.DmeWorld.Clients.Count,
                                IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                            }, clientChannel);
                            Queue(new RT_MSG_SERVER_INFO_AUX_UDP()
                            {
                                Ip = DmeClass.SERVER_IP,
                                Port = (ushort)data.ClientObject.UdpPort
                            }, clientChannel);
                        }
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_TCP clientConnectTcp:
                    {
                        data.ApplicationId = clientConnectTcp.AppId;

                        data.ClientObject = DmeClass.GetMPSClientByAccessToken(clientConnectTcp.AccessToken ?? string.Empty);

                        if (data.ClientObject == null)
                        {
                            LoggerAccessor.LogWarn("Access Token for client not found, fallback to Sessionkey!");
                            data.ClientObject = DmeClass.GetMPSClientBySessionKey(clientConnectTcp.SessionKey ?? string.Empty);
                            if (data.ClientObject != null)
                                LoggerAccessor.LogWarn("CLIENTOBJECT FALLBACK FOUND!!");
                            else
                            {
                                LoggerAccessor.LogWarn("AccessToken and SessionKey null! FALLBACK WITH NEW CLIENTOBJECT!");
                                //var clients = DmeClass.GetClientsByAppId(clientConnectTcpAuxUdp.AppId);
                                //data.ClientObject = clients.Where(x => x.Token == clientConnectTcpAuxUdp.AccessToken).FirstOrDefault();  
                                ClientObject clientObject = new(clientConnectTcp.SessionKey ?? string.Empty)
                                {
                                    ApplicationId = clientConnectTcp.AppId
                                };
                                data.ClientObject = clientObject;
                            }
                        }

                        if (enableEncryption == true && scertClient.CipherService != null && scertClient.CipherService.HasKey(CipherContext.RC_CLIENT_SESSION) && scertClient.RsaAuthKey != null)
                        {
                            //Queue(new RT_MSG_SERVER_CRYPTKEY_GAME() { GameKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        }

                        data.ClientObject.OnTcpConnected(clientChannel);
                        data.ClientObject.ScertId = GenerateNewScertClientId();
                        data.ClientObject.MediusVersion = scertClient.MediusVersion;
                        if (!_scertIdToClient.TryAdd(data.ClientObject.ScertId, data.ClientObject))
                        {
                            LoggerAccessor.LogWarn($"Duplicate scert client id");
                            break;
                        }

                        if (data.ClientObject.DmeWorld != null)
                            Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                            {
                                PlayerId = (ushort)data.ClientObject.DmeId,
                                ScertId = data.ClientObject.ScertId,
                                PlayerCount = (ushort)data.ClientObject.DmeWorld.Clients.Count,
                                IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                            }, clientChannel);

                        //pre108Complete

                        if (data.ClientObject.DmeWorld != null && (scertClient.MediusVersion == 108 || scertClient.ApplicationID == 10683 || scertClient.ApplicationID == 10684))
                            Queue(new RT_MSG_SERVER_CONNECT_COMPLETE()
                            {
                                ClientCountAtConnect = (ushort)data.ClientObject.DmeWorld.Clients.Count
                            }, clientChannel);

                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_REQUIRE clientConnectReadyRequire:
                    {
                        if (data.ClientObject != null && data.ClientObject.DmeWorld != null)
                            Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                            {
                                PlayerId = (ushort)data.ClientObject.DmeId,
                                ScertId = data.ClientObject.ScertId,
                                PlayerCount = (ushort)data.ClientObject.DmeWorld.Clients.Count,
                                IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                            }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_TCP clientConnectReadyTcp:
                    {
                        if (data.ClientObject != null && data.ClientObject.DmeWorld != null)
                        {
                            // Update recv flag
                            data.ClientObject.RecvFlag = clientConnectReadyTcp.RecvFlag;

                            Queue(new RT_MSG_SERVER_STARTUP_INFO_NOTIFY()
                            {
                                GameHostType = (byte)MGCL_GAME_HOST_TYPE.MGCLGameHostClientServerAuxUDP,
                                Timebase = (uint)data.ClientObject.DmeWorld.WorldTimer.ElapsedMilliseconds
                            }, clientChannel);
                            Queue(new RT_MSG_SERVER_INFO_AUX_UDP()
                            {
                                Ip = DmeClass.SERVER_IP,
                                Port = (ushort)data.ClientObject.UdpPort
                            }, clientChannel);
                        }
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_AUX_UDP connectReadyAuxUdp:
                    {
                        data.ClientObject?.OnConnectionCompleted();

                        if (data.ClientObject != null && data.ClientObject.DmeWorld != null)
                            Queue(new RT_MSG_SERVER_CONNECT_COMPLETE()
                            {
                                ClientCountAtConnect = (ushort)data.ClientObject.DmeWorld.Clients.Count
                            }, clientChannel);

                        //MSPR doesn't need this packet sent
                        if ((scertClient.MediusVersion > 108 && (data.ApplicationId == 24000 || data.ApplicationId == 24180)) || data.ApplicationId == 10683 || data.ApplicationId == 10684)
                        {
                            Queue(new RT_MSG_SERVER_APP()
                            {
                                Message = new TypeServerVersion()
                                {
                                    Version = "2.10.0009"
                                }
                            }, clientChannel);
                        }

                        data.ClientObject?.DmeWorld?.OnPlayerJoined(data.ClientObject);
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
                case RT_MSG_CLIENT_SET_RECV_FLAG setRecvFlag:
                    {
                        if (data.ClientObject != null)
                            data.ClientObject.RecvFlag = setRecvFlag.Flag;
                        break;
                    }
                case RT_MSG_CLIENT_SET_AGG_TIME setAggTime:
                    {
                        LoggerAccessor.LogInfo($"rt_msg_server_process_client_set_agg_time_msg: new agg time = {setAggTime.AggTime}");
                        List<int> preClientObject = new() { 10952, 10954, 10130 };

                        if (data.ClientObject != null && preClientObject.Contains(scertClient.ApplicationID))
                            data.ClientObject.AggTimeMs = setAggTime.AggTime; //Else we don't set AggTime here YET, the client object isn't created! for Pre-108 clients
                        break;
                    }
                case RT_MSG_CLIENT_FLUSH_ALL flushAll:
                    {

                        return;
                    }

                case RT_MSG_CLIENT_TIMEBASE_QUERY timebaseQuery:
                    {
                        if (data.ClientObject != null && data.ClientObject.DmeWorld != null)
                        {
                            RT_MSG_SERVER_TIMEBASE_QUERY_NOTIFY timebaseQueryNotifyMessage = new()
                            {
                                ClientTime = timebaseQuery.Timestamp,
                                ServerTime = (uint)data.ClientObject.DmeWorld.WorldTimer.ElapsedMilliseconds
                            };

                            //if (data.ClientObject?.Udp != null && data.ClientObject.RemoteUdpEndpoint != null)
                            //{
                            //    await data.ClientObject.Udp.SendImmediate(timebaseQueryNotifyMessage);
                            //}
                            //else
                            //{
                            //    await clientChannel.WriteAndFlushAsync(timebaseQueryNotifyMessage);
                            //}

                            await clientChannel.WriteAndFlushAsync(timebaseQueryNotifyMessage);
                            //await clientChannel.WriteAndFlushAsync(new RT_MSG_SERVER_TIMEBASE_QUERY_NOTIFY()
                            //{
                            //    ClientTime = timebaseQuery.Timestamp,
                            //    ServerTime = (uint)data.ClientObject.DmeWorld.WorldTimer.ElapsedMilliseconds
                            //});
                            //Queue(new RT_MSG_SERVER_TIMEBASE_QUERY_NOTIFY()
                            //{
                            //    ClientTime = timebaseQuery.Timestamp,
                            //    ServerTime = (uint)data.ClientObject.DmeWorld.WorldTimer.ElapsedMilliseconds
                            //}, clientChannel);
                        }
                        break;
                    }
                case RT_MSG_CLIENT_TOKEN_MESSAGE tokenMessage:
                    {
                        await ProcessRTTHostTokenMessage(tokenMessage, clientChannel, data);
                        break;
                    }
                case RT_MSG_CLIENT_APP_BROADCAST clientAppBroadcast:
                    {
                        data.ClientObject?.DmeWorld?.BroadcastTcp(data.ClientObject, clientAppBroadcast.Payload);
                        break;
                    }
                case RT_MSG_CLIENT_APP_LIST clientAppList:
                    {
                        data.ClientObject?.DmeWorld?.SendTcpAppList(data.ClientObject, clientAppList.Targets, clientAppList.Payload ?? Array.Empty<byte>());
                        break;
                    }
                case RT_MSG_CLIENT_APP_SINGLE clientAppSingle:
                    {
                        if (data.ClientObject != null)
                        {
                            bool InvalidRequest = false;

                            if (data.ClientObject.ApplicationId == 20371 || data.ClientObject.ApplicationId == 20374)
                            {
                                byte[] HubMessagePayload = clientAppSingle.Payload;
                                int HubPathernOffset = DataUtils.FindBytePattern(HubMessagePayload, new byte[] { 0x64, 0x00, 0x00 });

                                if (HubPathernOffset != -1) // Hub command.
                                {
                                    switch (BitConverter.IsLittleEndian ? EndianUtils.ReverseInt(BitConverter.ToInt32(HubMessagePayload, HubPathernOffset + 4)) : BitConverter.ToInt32(HubMessagePayload, HubPathernOffset + 4))
                                    {
                                        case -85: // IGA
                                            InvalidRequest = true;
                                            string SupplementalMessage = "Unknown";

                                            switch (HubMessagePayload[HubPathernOffset + 3]) // TODO, add all the other codes.
                                            {
                                                case 0x0B:
                                                    SupplementalMessage = "Kick";
                                                    break;
                                            }

                                            LoggerAccessor.LogError($"[DME] - TcpServer - HOME ANTI-CHEAT - DETECTED MALICIOUS USAGE (Reason: UNAUTHORISED IGA COMMAND - {SupplementalMessage}) - DmeId:{data.ClientObject.DmeId}");
                                            break;
                                    }
                                }
                            }

                            if (!InvalidRequest)
                                data.ClientObject.DmeWorld?.SendTcpAppSingle(data.ClientObject, clientAppSingle.TargetOrSource, clientAppSingle.Payload ?? Array.Empty<byte>());
                        }
                        break;
                    }
                case RT_MSG_CLIENT_APP_TOSERVER clientAppToServer:
                    {
                        if (clientAppToServer.Message != null)
                            await ProcessMediusMessage(clientAppToServer.Message, clientChannel, data);
                        break;
                    }

                case RT_MSG_CLIENT_DISCONNECT _:
                case RT_MSG_CLIENT_DISCONNECT_WITH_REASON _:
                    {
                        _ = clientChannel.CloseAsync();
                        break;
                    }
                default:
                    {
                        LoggerAccessor.LogWarn($"UNHANDLED RT MESSAGE: {message}");

                        break;
                    }
            }

            return;
        }

        protected virtual Task ProcessMediusMessage(BaseMediusMessage message, IChannel clientChannel, ChannelData data)
        {
            if (message == null)
                return Task.CompletedTask;

            switch (message)
            {
                case TypePing ping:
                    {
                        LoggerAccessor.LogInfo($"PingPacketHandler: client {data.ClientObject} received \n");
                        if (ping.RequestEcho == true)
                        {
                            byte[] value = new byte[0xA];
                            Queue(new RT_MSG_CLIENT_ECHO()
                            {
                                Value = value
                            }, clientChannel);
                            break;
                        }

                        /*
                        Queue(new RT_MSG_SERVER_APP()
                        {
                            Message = new TypePing()
                            {
                                TimeOfSend = Utils.GetUnixTime(),
                                PingInstance = ping.PingInstance,
                                RequestEcho = ping.RequestEcho
                            }
                        });
                        */

                        data.ClientObject?.EnqueueTcp(new RT_MSG_SERVER_APP() { 
                            Message = new TypePing()
                            {
                                TimeOfSend = Utils.GetUnixTime(),
                                PingInstance = ping.PingInstance,
                                RequestEcho = ping.RequestEcho
                            }
                        });

                        break;
                    }
            }

            return Task.CompletedTask;
        }

        protected virtual Task ProcessRTTHostTokenMessage(RT_MSG_CLIENT_TOKEN_MESSAGE clientTokenMsg, IChannel clientChannel, ChannelData data)
        {
            LoggerAccessor.LogInfo($"rt_msg_server_process_client_token_msg: msg type {clientTokenMsg.RT_TOKEN_MESSAGE_TYPE}, client {data.ClientObject?.ScertId}, target token = {clientTokenMsg.targetToken}");

            bool isTokenValid = rt_token_is_valid(clientTokenMsg.targetToken);

            if (!isTokenValid)
                LoggerAccessor.LogInfo($"rt_msg_server_process_client_token_msg: bad target token {clientTokenMsg.targetToken}");
            else
            {
                switch (clientTokenMsg.RT_TOKEN_MESSAGE_TYPE)
                {
                    case RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_CLIENT_REQUEST:
                        {
                            if (data.ClientObject != null && data.ClientObject.DmeWorld != null)
                            {
                                if (!data.ClientObject.DmeWorld.clientTokens.ContainsKey(clientTokenMsg.targetToken))
                                {
                                    data.ClientObject.DmeWorld.clientTokens.TryAdd(clientTokenMsg.targetToken, new List<int> { data.ClientObject.DmeId });

                                    data.ClientObject.DmeWorld.BroadcastTcpScertMessage(new RT_MSG_SERVER_TOKEN_MESSAGE() // We need to broadcast the signal that this token is owned.
                                    {
                                        tokenMsgType = RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_OWNED,
                                        TokenID = clientTokenMsg.targetToken,
                                        TokenHost = (ushort)data.ClientObject.DmeWorld.clientTokens[clientTokenMsg.targetToken][0],
                                    });
                                }
                                else
                                {
                                    lock (data.ClientObject.DmeWorld.clientTokens[clientTokenMsg.targetToken])
                                        data.ClientObject.DmeWorld.clientTokens[clientTokenMsg.targetToken].Add(data.ClientObject.DmeId);

                                    Queue(new RT_MSG_SERVER_TOKEN_MESSAGE() // This message should not be broadcasted, Home doesn't like it.
                                    {
                                        tokenMsgType = RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_GRANTED,
                                        TokenID = clientTokenMsg.targetToken
                                    }, clientChannel);
                                }
                            }
                            else
                            {
                                LoggerAccessor.LogWarn($"[DME] - TcpServer - ProcessRTTHostTokenMessage: Client {data.ClientObject?.IP} requested a token request without being in a DmeWorld!");

                                Queue(new RT_MSG_SERVER_FORCED_DISCONNECT()
                                {
                                    Reason = SERVER_FORCE_DISCONNECT_REASON.SERVER_FORCED_DISCONNECT_ERROR
                                }, clientChannel);
                            }

                            break;
                        }

                    case RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_CLIENT_RELEASE:
                        {
                            if (data.ClientObject != null && data.ClientObject.DmeWorld != null)
                            {
                                if (data.ClientObject.DmeWorld.clientTokens.TryGetValue(clientTokenMsg.targetToken, out List<int>? value))
                                {
                                    if (value.Contains(data.ClientObject.DmeId))
                                    {
                                        if (value.IndexOf(data.ClientObject.DmeId) == 0)
                                        {
                                            data.ClientObject.DmeWorld.clientTokens.TryRemove(clientTokenMsg.targetToken, out _);

                                            data.ClientObject.DmeWorld.BroadcastTcpScertMessage(new RT_MSG_SERVER_TOKEN_MESSAGE()
                                            {
                                                tokenMsgType = RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_FREED,
                                                TokenID = clientTokenMsg.targetToken,
                                            });
                                        }
                                        else
                                        {
                                            lock (value)
                                                value.Remove(data.ClientObject.DmeId);

                                            Queue(new RT_MSG_SERVER_TOKEN_MESSAGE()
                                            {
                                                tokenMsgType = RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_RELEASED,
                                                TokenID = clientTokenMsg.targetToken,
                                            }, clientChannel);
                                        }
                                    }
                                    else
                                        Queue(new RT_MSG_SERVER_TOKEN_MESSAGE()
                                        {
                                            tokenMsgType = RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_RELEASED,
                                            TokenID = clientTokenMsg.targetToken,
                                        }, clientChannel);
                                }
                                else
                                    Queue(new RT_MSG_SERVER_TOKEN_MESSAGE()
                                    {
                                        tokenMsgType = RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_OWNER_REMOVED,
                                    }, clientChannel);
                            }
                            else
                            {
                                LoggerAccessor.LogWarn($"[DME] - TcpServer - ProcessRTTHostTokenMessage: Client {data.ClientObject?.IP} requested a token release without being in a DmeWorld!");

                                Queue(new RT_MSG_SERVER_FORCED_DISCONNECT()
                                {
                                    Reason = SERVER_FORCE_DISCONNECT_REASON.SERVER_FORCED_DISCONNECT_ERROR
                                }, clientChannel);
                            }

                            break;
                        }

                    default:
                        {
                            LoggerAccessor.LogWarn($"UNHANDLED RT TOKEN MESSAGE: {clientTokenMsg.RT_TOKEN_MESSAGE_TYPE}");
                            break;
                        }
                }
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Channel

        /// <summary>
        /// Closes the client channel.
        /// </summary>
        protected async Task ForceDisconnectClient(IChannel channel)
        {
            try
            {
                // send force disconnect message
                await channel.WriteAndFlushAsync(new RT_MSG_SERVER_FORCED_DISCONNECT()
                {
                    Reason = SERVER_FORCE_DISCONNECT_REASON.SERVER_FORCED_DISCONNECT_ERROR
                });

                // close channel
                await channel.CloseAsync();
            }
            catch
            {
                // Silence exception since the client probably just closed the socket before we could write to it
            }
        }

        #endregion

        #region Queue

        public void Queue(BaseScertMessage message, params IChannel[] clientChannels)
        {
            Queue(message, (IEnumerable<IChannel>)clientChannels);
        }

        public void Queue(BaseScertMessage message, IEnumerable<IChannel> clientChannels)
        {
            foreach (var clientChannel in clientChannels)
                if (clientChannel != null)
                    if (_channelDatas.TryGetValue(clientChannel.Id.AsLongText(), out var data))
                        data.SendQueue.Enqueue(message);
        }

        public void Queue(IEnumerable<BaseScertMessage> messages, params IChannel[] clientChannels)
        {
            Queue(messages, (IEnumerable<IChannel>)clientChannels);
        }

        public void Queue(IEnumerable<BaseScertMessage> messages, IEnumerable<IChannel> clientChannels)
        {
            foreach (var clientChannel in clientChannels)
                if (clientChannel != null)
                    if (_channelDatas.TryGetValue(clientChannel.Id.AsLongText(), out var data))
                        foreach (var message in messages)
                            data.SendQueue.Enqueue(message);
        }

        #endregion

        #region Plugins

        protected async Task<bool> PassMessageToPlugins(IChannel clientChannel, ChannelData data, BaseScertMessage message, bool isIncoming)
        {
            var onMsg = new OnMessageArgs(isIncoming)
            {
                Player = data.ClientObject,
                Channel = clientChannel,
                Message = message
            };

            // Send to plugins
            await DmeClass.Plugins.OnMessageEvent(message.Id, onMsg);
            if (onMsg.Ignore)
                return true;

            // Send medius message to plugins
            if (message is RT_MSG_CLIENT_APP_TOSERVER clientApp)
            {
                OnMediusMessageArgs onMediusMsg = new(isIncoming)
                {
                    Player = data.ClientObject,
                    Channel = clientChannel,
                    Message = clientApp.Message
                };
                if (clientApp.Message != null)
                    await DmeClass.Plugins.OnMediusMessageEvent(clientApp.Message.PacketClass, clientApp.Message.PacketType, onMediusMsg);
                if (onMediusMsg.Ignore)
                    return true;
            }
            else if (message is RT_MSG_SERVER_APP serverApp)
            {
                OnMediusMessageArgs onMediusMsg = new(isIncoming)
                {
                    Player = data.ClientObject,
                    Channel = clientChannel,
                    Message = serverApp.Message
                };
                if (serverApp.Message != null)
                    await DmeClass.Plugins.OnMediusMessageEvent(serverApp.Message.PacketClass, serverApp.Message.PacketType, onMediusMsg);
                if (onMediusMsg.Ignore)
                    return true;
            }

            return false;
        }

        #endregion

        public ClientObject? GetClientByScertId(ushort scertId)
        {
            if (_scertIdToClient.TryGetValue(scertId, out var result))
                return result;

            return null;
        }

        protected ushort GenerateNewScertClientId()
        {
            return _clientCounter++;
        }

        public bool rt_token_is_valid(ushort TokenId)
        {
            return TokenId <= 65534;
        }
    }
}
