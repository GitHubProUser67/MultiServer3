using DotNetty.Transport.Channels;
using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.RT.Cryptography;
using MultiServer.Addons.Horizon.RT.Models;
using MultiServer.Addons.Horizon.LIBRARY.Common;
using MultiServer.Addons.Horizon.MEDIUS.Config;
using MultiServer.Addons.Horizon.MEDIUS.Medius.Models;
using MultiServer.Addons.Horizon.MEDIUS.PluginArgs;
using MultiServer.Addons.Horizon.LIBRARY.Pipeline.Attribute;
using System.Net;
using MultiServer.PluginManager;

namespace MultiServer.Addons.Horizon.MEDIUS.Medius
{
    public class MPS : BaseMediusComponent
    {
        public override int TCPPort => MediusClass.Settings.MPSPort;
        public override int UDPPort => 00000;

        DateTime lastSend = Utils.GetHighPrecisionUtcTime();

        IChannel channel = null;
        ChannelData channelData = null;
        ClientObject clientObject = null;

        public MPS()
        {

        }

        protected override Task OnConnected(IChannel clientChannel)
        {
            // Get ScertClient data
            if (!clientChannel.HasAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT))
                clientChannel.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Set(new ScertClientAttribute());
            var scertClient = clientChannel.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            scertClient.RsaAuthKey = MediusClass.Settings.MPSKey;
            scertClient.CipherService.GenerateCipher(MediusClass.Settings.MPSKey);


            return base.OnConnected(clientChannel);
        }

        protected override async Task ProcessMessage(BaseScertMessage message, IChannel clientChannel, ChannelData data)
        {
            channelData = data;
            channel = clientChannel;

            // Get ScertClient data
            var scertClient = clientChannel.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            scertClient.CipherService.EnableEncryption = MediusClass.GetAppSettingsOrDefault(data.ApplicationId).EnableEncryption;
            var enableEncryption = MediusClass.GetAppSettingsOrDefault(data.ApplicationId).EnableEncryption;

            // 
            switch (message)
            {
                case RT_MSG_CLIENT_HELLO clientHello:
                    {
                        if (data.State > ClientState.HELLO)
                            throw new Exception($"Unexpected RT_MSG_CLIENT_HELLO from {clientChannel.RemoteAddress}: {clientHello}");

                        data.State = ClientState.HELLO;
                        Queue(new RT_MSG_SERVER_HELLO() { RsaPublicKey = enableEncryption ? MediusClass.Settings.DefaultKey.N : Org.BouncyCastle.Math.BigInteger.Zero }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CRYPTKEY_PUBLIC clientCryptKeyPublic:
                    {
                        if (data.State > ClientState.HANDSHAKE)
                            throw new Exception($"Unexpected RT_MSG_CLIENT_CRYPTKEY_PUBLIC from {clientChannel.RemoteAddress}: {clientCryptKeyPublic}");

                        /*
                        // Ensure key is correct
                        if (!clientCryptKeyPublic.PublicKey.Reverse().SequenceEqual(MediusStarter.Settings.MPSKey.N.ToByteArrayUnsigned()))
                        {
                            ServerConfiguration.LogError($"Client {clientChannel.RemoteAddress} attempting to authenticate with invalid key {Encoding.Default.GetString(clientCryptKeyPublic.PublicKey)}");
                            data.State = ClientState.DISCONNECTED;
                            await clientChannel.CloseAsync();
                            break;
                        }
                        */

                        data.State = ClientState.CONNECT_1;

                        // generate new client session key
                        scertClient.CipherService.GenerateCipher(CipherContext.RSA_AUTH, clientCryptKeyPublic.PublicKey.Reverse().ToArray());
                        scertClient.CipherService.GenerateCipher(CipherContext.RC_CLIENT_SESSION);

                        Queue(new RT_MSG_SERVER_CRYPTKEY_PEER() { SessionKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_TCP clientConnectTcp:
                    {
                        if (data.State > ClientState.CONNECT_1)
                            throw new Exception($"Unexpected RT_MSG_CLIENT_CONNECT_TCP from {clientChannel.RemoteAddress}: {clientConnectTcp}");
                        /*
                        if (clientConnectTcp.AccessToken == null)
                            throw new Exception($"AccessToken in RT_MSG_CLIENT_CONNECT_TCP from {clientChannel.RemoteAddress}: {clientConnectTcp} is null");
                        */

                        data.ApplicationId = clientConnectTcp.AppId;
                        scertClient.ApplicationID = clientConnectTcp.AppId;

                        List<int> pre108ServerConnect = new List<int>() { 10683, 10114, 10164, 10190, 10124, 10284, 10330, 10334, 10414, 10442, 10540, 10680 };
                        List<int> pre108NoServerConnect = new List<int>() { 10010, 10031, 10274 };

                        if (clientConnectTcp.AccessToken != null)
                        {
                            data.ClientObject = MediusClass.Manager.GetClientByAccessToken(clientConnectTcp.AccessToken, data.ApplicationId);
                            if (data.ClientObject != null)
                            {
                                clientObject = data.ClientObject;
                                ServerConfiguration.LogInfo("MPS Client Connected!");
                                data.ClientObject.OnConnected();
                            }
                        }

                        data.State = ClientState.AUTHENTICATED;
                        Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                        {
                            PlayerId = 0,
                            ScertId = GenerateNewScertClientId(),
                            PlayerCount = 0x0001,
                            IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                        }, clientChannel);

                        #region 108 SERVER_CONNECT_COMPLETE
                        // Depending on game, complete connection or not.
                        if (scertClient.MediusVersion == 108 || !pre108NoServerConnect.Contains(scertClient.ApplicationID) && pre108ServerConnect.Contains(scertClient.ApplicationID))
                            Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                        #endregion

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
                        if (data.State != ClientState.AUTHENTICATED)
                            throw new Exception($"Unexpected RT_MSG_CLIENT_APP_TOSERVER from {clientChannel.RemoteAddress}: {clientAppToServer}");

                        await ProcessMediusMessage(clientAppToServer.Message, clientChannel, data);
                        break;
                    }
                case RT_MSG_CLIENT_DISCONNECT _:
                    {
                        //ServerConfiguration.LogInfo($"Client id = {data.ClientObject.AccountId} disconnected by request with no specific reason\n");
                        break;
                    }
                case RT_MSG_CLIENT_DISCONNECT_WITH_REASON clientDisconnectWithReason:
                    {
                        data.State = ClientState.DISCONNECTED;
                        await clientChannel.CloseAsync();
                        break;
                    }
                default:
                    {
                        ServerConfiguration.LogWarn($"UNHANDLED RT MESSAGE: {message}");
                        break;
                    }
            }

            return;
        }

        protected virtual async Task ProcessMediusMessage(BaseMediusMessage message, IChannel clientChannel, ChannelData data)
        {
            if (message == null)
                return;

            switch (message)
            {
                #region MediusServerSetAttributesRequest
                // This is a bit of a hack to get our custom dme client to authenticate
                // Our client skips MAS and just connects directly to MPS with this message
                case MediusServerSetAttributesRequest dmeSetAttributesRequest:
                    {
                        // Create DME object
                        var dme = new DMEObject(dmeSetAttributesRequest);
                        dme.ApplicationId = data.ApplicationId;
                        dme.BeginSession();
                        MediusClass.Manager.AddDmeClient(dme);

                        data.ClientObject = dme;

                        data.ClientObject.OnConnected();

                        Queue(new RT_MSG_SERVER_APP()
                        {
                            Message = new MediusServerSetAttributesResponse()
                            {
                                MessageID = dmeSetAttributesRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS
                            }
                        }, clientChannel);

                        break;
                    }
                #endregion

                #region MediusServerCreateGameWithAttributesResponse
                case MediusServerCreateGameWithAttributesResponse createGameWithAttrResponse:
                    {
                        int gameId = int.Parse(createGameWithAttrResponse.MessageID.Value.Split('-')[0]);
                        int accountId = int.Parse(createGameWithAttrResponse.MessageID.Value.Split('-')[1]);
                        string msgId = createGameWithAttrResponse.MessageID.Value.Split('-')[2];
                        var game = MediusClass.Manager.GetGameByGameId(gameId);
                        var rClient = MediusClass.Manager.GetClientByAccountId(accountId, data.ClientObject.ApplicationId);

                        if (data.ApplicationId == 22920 || data.ApplicationId == 21834)
                        {
                            //Send Matchmaking create game
                            if (!createGameWithAttrResponse.IsSuccess)
                            {
                                rClient?.Queue(new MediusMatchCreateGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusFail
                                });

                                await game.EndGame();
                            }
                            else
                            {
                                game.DMEWorldId = createGameWithAttrResponse.WorldID;
                                await game.GameCreated();

                                rClient?.Queue(new MediusMatchCreateGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    MediusWorldID = game.Id,
                                    SystemSpecificStatusCode = 0,
                                    RequestData = rClient.requestData,
                                    ApplicationDataSize = rClient.appDataSize,
                                    ApplicationData = rClient.appData,
                                });

                                // Send to plugins
                                await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_CREATED, new OnPlayerGameArgs() { Player = rClient, Game = game });
                            }
                        }
                        else
                        {
                            if (!createGameWithAttrResponse.IsSuccess)
                            {
                                rClient?.Queue(new MediusCreateGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusFail
                                });

                                await game.EndGame();
                            }
                            else
                            {
                                game.DMEWorldId = createGameWithAttrResponse.WorldID;
                                await game.GameCreated();
                                rClient?.Queue(new MediusCreateGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    MediusWorldID = game.Id
                                });

                                // Send to plugins
                                await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_CREATED, new OnPlayerGameArgs() { Player = rClient, Game = game });
                            }
                        }

                        break;
                    }
                #endregion

                #region MediusServerJoinGameResponse
                case MediusServerJoinGameResponse joinGameResponse:
                    {
                        int gameId = int.Parse(joinGameResponse.MessageID.Value.Split('-')[0]);
                        int accountId = int.Parse(joinGameResponse.MessageID.Value.Split('-')[1]);
                        string msgId = joinGameResponse.MessageID.Value.Split('-')[2];
                        var game = MediusClass.Manager.GetGameByGameId(gameId);
                        var rClient = MediusClass.Manager.GetClientByAccountId(accountId, data.ClientObject.ApplicationId);

                        if (!joinGameResponse.IsSuccess)
                        {
                            rClient?.Queue(new MediusJoinGameResponse()
                            {
                                MessageID = new MessageId(msgId),
                                StatusCode = MediusCallbackStatus.MediusFail
                            });
                        }
                        else
                        {
                            if (gameId != 0)
                            {
                                #region P2P
                                if (game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                    game.netAddressList.AddressList[0].AddressType == NetAddressType.NetAddressTypeSignalAddress)
                                {
                                    // Join game P2P
                                    await rClient?.JoinGameP2P(game);

                                    rClient?.Queue(new MediusJoinGameResponse()
                                    {
                                        MessageID = new MessageId(msgId),
                                        StatusCode = MediusCallbackStatus.MediusSuccess,
                                        GameHostType = game.GameHostType,
                                        ConnectInfo = new NetConnectionInfo()
                                        {
                                            AccessKey = joinGameResponse.AccessKey,
                                            SessionKey = rClient.SessionKey,
                                            WorldID = game.WorldID,
                                            ServerKey = joinGameResponse.pubKey,
                                            AddressList = new NetAddressList()
                                            {
                                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                                {
                                                        new NetAddress() { Address = game.netAddressList.AddressList[0].Address, Port = game.netAddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeSignalAddress},
                                                        new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                                }
                                            },
                                            Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                        }
                                    });
                                }
                                else if (game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                    game.netAddressList.AddressList[0].AddressType == NetAddressType.NetAddressTypeExternal &&
                                    game.netAddressList.AddressList[1].AddressType == NetAddressType.NetAddressTypeInternal
                                    )
                                {
                                    // Join game P2P
                                    await rClient?.JoinGameP2P(game);

                                    rClient?.Queue(new MediusJoinGameResponse()
                                    {
                                        MessageID = new MessageId(msgId),
                                        StatusCode = MediusCallbackStatus.MediusSuccess,
                                        GameHostType = game.GameHostType,
                                        ConnectInfo = new NetConnectionInfo()
                                        {
                                            AccessKey = joinGameResponse.AccessKey,
                                            SessionKey = rClient.SessionKey,
                                            WorldID = game.WorldID,
                                            ServerKey = joinGameResponse.pubKey,
                                            AddressList = new NetAddressList()
                                            {
                                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                                {
                                                        new NetAddress() { Address = game.netAddressList.AddressList[0].Address, Port = game.netAddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                        new NetAddress() { Address = game.netAddressList.AddressList[1].Address, Port = game.netAddressList.AddressList[1].Port, AddressType = NetAddressType.NetAddressTypeInternal},
                                                }
                                            },
                                            Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                        }
                                    });
                                }

                                else if (game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                    game.netAddressList.AddressList[0].AddressType == NetAddressType.NetAddressTypeBinaryExternalVport &&
                                    game.netAddressList.AddressList[1].AddressType == NetAddressType.NetAddressTypeBinaryInternalVport
                                    )
                                {
                                    // Join game P2P
                                    await rClient?.JoinGameP2P(game);

                                    rClient?.Queue(new MediusJoinGameResponse()
                                    {
                                        MessageID = new MessageId(msgId),
                                        StatusCode = MediusCallbackStatus.MediusSuccess,
                                        GameHostType = game.GameHostType,
                                        ConnectInfo = new NetConnectionInfo()
                                        {
                                            AccessKey = rClient.Token,
                                            SessionKey = rClient.SessionKey,
                                            WorldID = game.WorldID,
                                            ServerKey = joinGameResponse.pubKey,
                                            AddressList = new NetAddressList()
                                            {
                                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                                {
                                                    new NetAddress() {
                                                        AddressType = NetAddressType.NetAddressTypeBinaryExternalVport,
                                                        IPBinaryBitOne = game.netAddressList.AddressList[0].IPBinaryBitOne,
                                                        IPBinaryBitTwo = game.netAddressList.AddressList[0].IPBinaryBitTwo,
                                                        IPBinaryBitThree = game.netAddressList.AddressList[0].IPBinaryBitThree,
                                                        IPBinaryBitFour = game.netAddressList.AddressList[0].IPBinaryBitFour,
                                                        BinaryPort = game.netAddressList.AddressList[0].BinaryPort},
                                                    new NetAddress() {
                                                        AddressType = NetAddressType.NetAddressTypeBinaryInternalVport,
                                                        IPBinaryBitOne = game.netAddressList.AddressList[1].IPBinaryBitOne,
                                                        IPBinaryBitTwo = game.netAddressList.AddressList[1].IPBinaryBitTwo,
                                                        IPBinaryBitThree = game.netAddressList.AddressList[1].IPBinaryBitThree,
                                                        IPBinaryBitFour = game.netAddressList.AddressList[1].IPBinaryBitFour,
                                                        BinaryPort = game.netAddressList.AddressList[1].BinaryPort},
                                                }
                                            },
                                            Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                        }
                                    });
                                }

                                #endregion
                                else if (rClient.MediusVersion == 108)
                                {
                                    // Join game DME
                                    await rClient?.JoinGame(game, joinGameResponse.DmeClientIndex);

                                    rClient?.Queue(new MediusJoinGameResponse()
                                    {
                                        MessageID = new MessageId(msgId),
                                        StatusCode = MediusCallbackStatus.MediusSuccess,
                                        GameHostType = game.GameHostType,
                                        ConnectInfo = new NetConnectionInfo()
                                        {
                                            AccessKey = joinGameResponse.AccessKey,
                                            SessionKey = rClient.SessionKey,
                                            WorldID = game.DMEWorldId,
                                            ServerKey = joinGameResponse.pubKey,
                                            AddressList = new NetAddressList()
                                            {
                                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                                {
                                                    new NetAddress() { Address = (data.ClientObject as DMEObject).IP.MapToIPv4().ToString(), Port = (data.ClientObject as DMEObject).Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                    new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                                }
                                            },
                                            Type = NetConnectionType.NetConnectionTypeClientServerTCP
                                        }
                                    });
                                }
                                else
                                {
                                    // Join game DME
                                    await rClient?.JoinGame(game, joinGameResponse.DmeClientIndex);

                                    rClient?.Queue(new MediusJoinGameResponse()
                                    {
                                        MessageID = new MessageId(msgId),
                                        StatusCode = MediusCallbackStatus.MediusSuccess,
                                        GameHostType = game.GameHostType,
                                        ConnectInfo = new NetConnectionInfo()
                                        {
                                            AccessKey = joinGameResponse.AccessKey,
                                            SessionKey = rClient.SessionKey,
                                            WorldID = game.DMEWorldId,
                                            ServerKey = joinGameResponse.pubKey,
                                            AddressList = new NetAddressList()
                                            {
                                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                            {
                                                        new NetAddress() { Address = (data.ClientObject as DMEObject).IP.MapToIPv4().ToString(), Port = (data.ClientObject as DMEObject).Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                        new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                            }
                                            },
                                            Type = NetConnectionType.NetConnectionTypeClientServerTCPAuxUDP
                                        }
                                    });
                                }

                                /// For Legacy Medius v1.50 clients that DO NOT 
                                /// send a ServerConnectNotificationConnect when creating a game
                                if (data.ClientObject.MediusVersion < 109 && data.ClientObject.ApplicationId != 10394)
                                    await game.OnMediusJoinGameResponse(rClient.SessionKey);
                            }

                            /*

                            if (game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                game.netAddressList.AddressList[0].AddressType == NetAddressType.NetAddressTypeSignalAddress)
                            {

                                // Join game P2P
                                await rClient?.JoinGameP2P(game);

                                // 
                                rClient?.Queue(new MediusJoinGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    GameHostType = game.GameHostType,
                                    ConnectInfo = new NetConnectionInfo()
                                    {
                                        AccessKey = joinGameResponse.AccessKey,
                                        SessionKey = rClient.SessionKey,
                                        WorldID = game.WorldID,
                                        ServerKey = joinGameResponse.pubKey,
                                        AddressList = new NetAddressList()
                                        {
                                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                            {
                                                new NetAddress() { Address = game.netAddressList.AddressList[0].Address, Port = game.netAddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeSignalAddress},
                                                new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                            }
                                        },
                                        Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                    }
                                });
                            }
                            else if(game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                game.netAddressList.AddressList[0].AddressType == NetAddressType.NetAddressTypeExternal &&
                                game.netAddressList.AddressList[1].AddressType == NetAddressType.NetAddressTypeInternal
                                )
                            {
                                // Join game P2P
                                await rClient?.JoinGameP2P(game);

                                // 
                                rClient?.Queue(new MediusJoinGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    GameHostType = game.GameHostType,
                                    ConnectInfo = new NetConnectionInfo()
                                    {
                                        AccessKey = joinGameResponse.AccessKey,
                                        SessionKey = rClient.SessionKey,
                                        WorldID = game.WorldID,
                                        ServerKey = joinGameResponse.pubKey,
                                        AddressList = new NetAddressList()
                                        {
                                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                            {
                                                new NetAddress() { Address = game.netAddressList.AddressList[0].Address, Port = game.netAddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                new NetAddress() { Address = game.netAddressList.AddressList[1].Address, Port = game.netAddressList.AddressList[1].Port, AddressType = NetAddressType.NetAddressTypeInternal},
                                            }
                                        },
                                        Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                    }
                                });
                            } 
                            */

                            /*
                            if(rClient.WorldId == null)
                            {
                                // 
                                rClient?.Queue(new MediusJoinGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    GameHostType = game.GameHostType,
                                    ConnectInfo = new NetConnectionInfo()
                                    {
                                        AccessKey = joinGameResponse.AccessKey,
                                        SessionKey = rClient.SessionKey,
                                        WorldID = game.DMEWorldId,
                                        ServerKey = joinGameResponse.pubKey,
                                        AddressList = new NetAddressList()
                                        {
                                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                            {
                                                new NetAddress() { Address = game.netAddressList.AddressList[1].Address, Port = game.netAddressList.AddressList[1].Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                            }
                                        },
                                        Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                    }
                                });
                            } else {
                                // 
                                rClient?.Queue(new MediusJoinGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    GameHostType = game.GameHostType,
                                    ConnectInfo = new NetConnectionInfo()
                                    {
                                        AccessKey = joinGameResponse.AccessKey,
                                        SessionKey = rClient.SessionKey,
                                        WorldID = game.DMEWorldId,
                                        ServerKey = joinGameResponse.pubKey,
                                        AddressList = new NetAddressList()
                                        {
                                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                            {
                                                new NetAddress() { Address = (data.ClientObject as DMEObject).IP.MapToIPv4().ToString(), Port = (uint)(data.ClientObject as DMEObject).Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                            }
                                        },
                                        Type = NetConnectionType.NetConnectionTypeClientServerTCPAuxUDP
                                    }
                                });
                            }
                            */
                        }
                        break;
                    }
                #endregion

                #region MediusServerCreateGameOnSelfRequest
                case MediusServerCreateGameOnSelfRequest serverCreateGameOnSelfRequest:
                    {
                        // Create DME object on Player
                        var dme = new DMEObject(serverCreateGameOnSelfRequest);
                        dme.BeginSession();
                        MediusClass.Manager.AddDmeClient(dme);

                        data.ClientObject.OnConnected();

                        // validate name
                        if (!MediusClass.PassTextFilter(data.ApplicationId, TextFilterContext.GAME_NAME, Convert.ToString(serverCreateGameOnSelfRequest.GameName)))
                        {
                            data.ClientObject.Queue(new MediusServerCreateGameOnMeResponse()
                            {
                                MessageID = serverCreateGameOnSelfRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG
                            });
                            return;
                        }

                        // Send to plugins
                        await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_CREATE_GAME, new OnPlayerRequestArgs() { Player = data.ClientObject, Request = serverCreateGameOnSelfRequest });

                        await MediusClass.Manager.CreateGameP2P(data.ClientObject, serverCreateGameOnSelfRequest, clientChannel, dme);
                        break;
                    }
                #endregion

                #region MediusServerCreateGameOnSelfRequest0
                case MediusServerCreateGameOnSelfRequest0 serverCreateGameOnSelfRequest0:
                    {
                        // Create DME object
                        var dme = new DMEObject(serverCreateGameOnSelfRequest0);

                        dme.BeginSession();
                        MediusClass.Manager.AddDmeClient(dme);

                        data.ClientObject.OnConnected();

                        // validate name
                        if (!MediusClass.PassTextFilter(data.ApplicationId, TextFilterContext.GAME_NAME, Convert.ToString(serverCreateGameOnSelfRequest0.GameName)))
                        {
                            data.ClientObject.Queue(new MediusServerCreateGameOnMeResponse()
                            {
                                MessageID = serverCreateGameOnSelfRequest0.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG
                            });
                            return;
                        }

                        // Send to plugins
                        await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_CREATE_GAME, new OnPlayerRequestArgs() { Player = data.ClientObject, Request = serverCreateGameOnSelfRequest0 });

                        await MediusClass.Manager.CreateGameP2P(data.ClientObject, serverCreateGameOnSelfRequest0, clientChannel, dme);
                        break;
                    }
                #endregion

                #region MediusServerCreateGameOnMeRequest   
                case MediusServerCreateGameOnMeRequest serverCreateGameOnMeRequest:
                    {
                        // Create DME object
                        var dme = new DMEObject(serverCreateGameOnMeRequest);

                        dme.BeginSession();
                        MediusClass.Manager.AddDmeClient(dme);

                        data.ClientObject.OnConnected();

                        // validate name
                        if (!MediusClass.PassTextFilter(data.ApplicationId, TextFilterContext.GAME_NAME, Convert.ToString(serverCreateGameOnMeRequest.GameName)))
                        {
                            data.ClientObject.Queue(new MediusServerCreateGameOnMeResponse()
                            {
                                MessageID = serverCreateGameOnMeRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG,
                            });
                            return;
                        }

                        // Send to plugins
                        await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_CREATE_GAME, new OnPlayerRequestArgs() { Player = data.ClientObject, Request = serverCreateGameOnMeRequest });

                        await MediusClass.Manager.CreateGameP2P(data.ClientObject, serverCreateGameOnMeRequest, clientChannel, dme);

                        break;
                    }

                #endregion

                #region MediusServerMoveGameWorldOnMeRequest
                case MediusServerMoveGameWorldOnMeRequest serverMoveGameWorldOnMeRequest:
                    {
                        //Fetch Current Game, and Update it with the new one
                        var game = MediusClass.Manager.GetGameByGameId(serverMoveGameWorldOnMeRequest.CurrentMediusWorldID);
                        if (game.WorldID != serverMoveGameWorldOnMeRequest.CurrentMediusWorldID)
                        {
                            data.ClientObject.Queue(new MediusServerMoveGameWorldOnMeResponse()
                            {
                                MessageID = serverMoveGameWorldOnMeRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_UNSUCCESSFUL,
                            });
                        }
                        else
                        {
                            game.WorldID = serverMoveGameWorldOnMeRequest.NewGameWorldID;
                            game.netAddressList.AddressList[0] = serverMoveGameWorldOnMeRequest.AddressList.AddressList[0];
                            game.netAddressList.AddressList[1] = serverMoveGameWorldOnMeRequest.AddressList.AddressList[1];

                            ServerConfiguration.LogInfo("MediusServerMoveGameWorldOnMeRequest Successful");
                            data.ClientObject.Queue(new MediusServerMoveGameWorldOnMeResponse()
                            {
                                MessageID = serverMoveGameWorldOnMeRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                                MediusWorldID = serverMoveGameWorldOnMeRequest.NewGameWorldID
                            });
                        }
                        break;
                    }
                #endregion

                #region MediusServerWorldReportOnMe

                case MediusServerWorldReportOnMe serverWorldReportOnMe:
                    {
                        if (data.ClientObject.CurrentGame != null)
                            await data.ClientObject.CurrentGame.OnWorldReportOnMe(serverWorldReportOnMe);
                        break;
                    }

                #endregion

                #region MediusServerEndGameOnMeRequest
                /// <summary>
                /// This structure uses the game world ID as MediusWorldID. This should not be confused with the net World ID on this host.
                /// </summary>
                case MediusServerEndGameOnMeRequest serverEndGameOnMeRequest:
                    {
                        data.ClientObject.Queue(new MediusServerEndGameOnMeResponse()
                        {
                            MessageID = serverEndGameOnMeRequest.MessageID,
                            Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                        });
                        break;
                    }
                #endregion

                #region MediusServerReport
                case MediusServerReport serverReport:
                    {

                        (data.ClientObject as DMEObject)?.OnServerReport(serverReport);
                        //data.ClientObject.OnConnected();
                        //data.ClientObject.KeepAliveUntilNextConnection();
                        //ServerConfiguration.LogInfo($"ServerReport SessionKey {serverReport.SessionKey} MaxWorlds {serverReport.MaxWorlds} MaxPlayersPerWorld {serverReport.MaxPlayersPerWorld} TotalWorlds {serverReport.ActiveWorldCount} TotalPlayers {serverReport.TotalActivePlayers} Alert {serverReport.AlertLevel} ConnIndex {data.ClientObject.DmeId} WorldID {data.ClientObject.WorldId}");
                        break;
                    }
                #endregion

                #region MediusServerConnectNotification
                case MediusServerConnectNotification connectNotification:
                    {
                        ServerConfiguration.LogInfo("MediusServerConnectNotification Received");
                        await MediusClass.Manager.GetGameByDmeWorldId((data.ClientObject as DMEObject).SessionKey, (int)connectNotification.MediusWorldUID)?.OnMediusServerConnectNotification(connectNotification);

                        //MediusServerConnectNotification -  sent Notify msg to MUM
                        //DmeServerGetConnectKeys
                        /*
                        if (data.ClientObject.SessionKey == null)
                        {
                            Logger.Warn($"ServerConnectNotificationHandler - DmeServerGetConnectKeys error = {data.ClientObject.SessionKey} is null");
                            return;
                        } else {
                        }
                        */
                        break;
                    }
                #endregion

                #region MediusServerDisconnectPlayerRequest UNIMPLEMENTED
                case MediusServerDisconnectPlayerRequest serverDisconnectPlayerRequest:
                    {

                        break;
                    }
                #endregion

                #region MediusServerEndGameRequest
                case MediusServerEndGameRequest endGameRequest:
                    {
                        var game = MediusClass.Manager.GetGameByGameId(endGameRequest.WorldID);

                        if (game != null && endGameRequest.BrutalFlag == true)
                        {
                            await game.EndGame();

                            data.ClientObject.Queue(new MediusServerEndGameResponse()
                            {
                                MessageID = endGameRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS
                            });
                        }
                        else if (game != null && endGameRequest.BrutalFlag == false)
                        {
                            data.ClientObject.Queue(new MediusServerEndGameResponse()
                            {
                                MessageID = endGameRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                            });
                        }
                        else
                        {
                            data.ClientObject.Queue(new MediusServerEndGameResponse()
                            {
                                MessageID = endGameRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG
                            });
                        }
                        break;
                    }
                #endregion

                #region MediusServerSessionEndRequest
                case MediusServerSessionEndRequest sessionEndRequest:
                    {
                        ServerConfiguration.LogInfo("ServerSessionEndRequest Received");

                        //DmeServerGetConnectKeys
                        if (data.ClientObject.SessionKey == null)
                        {
                            ServerConfiguration.LogWarn($"MediusServerSessionEndRequestHandler: DmeServerGetConnectKeys error {data.ClientObject.SessionKey} is null");
                            data?.ClientObject.Queue(new MediusServerSessionEndResponse()
                            {
                                MessageID = sessionEndRequest.MessageID,
                                ErrorCode = MGCL_ERROR_CODE.MGCL_SESSIONEND_FAILED
                            });
                        }
                        else
                        {
                            data?.ClientObject.KeepAliveUntilNextConnection();

                            //Success
                            ServerConfiguration.LogInfo("Server Session End Success");
                            data?.ClientObject.Queue(new MediusServerSessionEndResponse()
                            {
                                MessageID = sessionEndRequest.MessageID,
                                ErrorCode = MGCL_ERROR_CODE.MGCL_SUCCESS
                            });
                        }

                        break;
                    }
                #endregion

                default:
                    {
                        ServerConfiguration.LogWarn($"Unhandled Medius Message: {message}");
                        break;
                    }
            }
        }

        public DMEObject GetFreeDme(int appId)
        {
            try
            {
                return _scertHandler.Group
                    .Select(x => _channelDatas[x.Id.AsLongText()]?.ClientObject)
                    .Where(x => x is DMEObject && x != null && (x.ApplicationId == appId || x.ApplicationId == 0))
                    .MinByAlt(x => (x as DMEObject).CurrentWorlds) as DMEObject;
            }
            catch (Exception e)
            {
                ServerConfiguration.LogError("No DME Game Server assigned to this AppId\n", e);
            }

            return null;
        }

        public void SendServerCreateGameWithAttributesRequest(string messageId, int gameId, int gameAttributes, int clientAppId, int gameMaxPlayers)
        {
            Queue(new RT_MSG_SERVER_APP()
            {
                Message = new MediusServerCreateGameWithAttributesRequest()
                {
                    MessageID = new MessageId($"{messageId}"),
                    MediusWorldUID = (uint)gameId,
                    Attributes = (MediusWorldAttributesType)gameAttributes,
                    ApplicationID = clientAppId,
                    MaxClients = gameMaxPlayers
                }
            }, channel);
        }

        //Actual DME flow unimplemented
        public DMEObject ReserveDMEObject(MediusServerSessionBeginRequest request)
        {
            var dme = new DMEObject(request);
            dme.BeginSession();
            MediusClass.Manager.AddDmeClient(dme);
            return dme;
        }

        public DMEObject ReserveDMEObject(MediusServerSessionBeginRequest1 request)
        {
            var dme = new DMEObject(request);
            dme.BeginSession();
            MediusClass.Manager.AddDmeClient(dme);
            return dme;
        }

        public DMEObject ReserveDMEObject(MediusServerSessionBeginRequest2 request)
        {
            var dme = new DMEObject(request);
            dme.BeginSession();
            MediusClass.Manager.AddDmeClient(dme);
            return dme;
        }

        public ClientObject ReserveClient(MediusServerSessionBeginRequest mgclSessionBeginRequest)
        {
            var client = new ClientObject();
            client.BeginSession();
            return client;
        }
    }
}
