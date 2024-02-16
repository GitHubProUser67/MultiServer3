using CustomLogger;
using DotNetty.Transport.Channels;
using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.RT.Cryptography;
using BackendProject.Horizon.RT.Models;
using BackendProject.Horizon.LIBRARY.Common;
using Horizon.MEDIUS.Config;
using Horizon.MEDIUS.Medius.Models;
using Horizon.MEDIUS.PluginArgs;
using BackendProject.Horizon.LIBRARY.Pipeline.Attribute;
using System.Net;
using Horizon.PluginManager;
using BackendProject.Horizon.RT.Models.MGCL;
using Horizon.MUM;

namespace Horizon.MEDIUS.Medius
{
    public class MPS : BaseMediusComponent
    {
        public override int TCPPort => MediusClass.Settings.MPSPort;
        public override int UDPPort => 00000;

        DateTime lastSend = Utils.GetHighPrecisionUtcTime();

        IChannel? channel = null;
        ChannelData? channelData = null;
        ClientObject? clientObject = null;

        public MPS()
        {

        }

        protected override Task OnConnected(IChannel clientChannel)
        {
            // Get ScertClient data
            if (!clientChannel.HasAttribute(BackendProject.Horizon.LIBRARY.Pipeline.Constants.SCERT_CLIENT))
                clientChannel.GetAttribute(BackendProject.Horizon.LIBRARY.Pipeline.Constants.SCERT_CLIENT).Set(new ScertClientAttribute());
            var scertClient = clientChannel.GetAttribute(BackendProject.Horizon.LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            scertClient.RsaAuthKey = MediusClass.Settings.MPSKey;
            scertClient.CipherService?.GenerateCipher(MediusClass.Settings.MPSKey);

            return base.OnConnected(clientChannel);
        }

        protected override async Task ProcessMessage(BaseScertMessage message, IChannel clientChannel, ChannelData data)
        {
            channelData = data;
            channel = clientChannel;

            // Get ScertClient data
            var scertClient = clientChannel.GetAttribute(BackendProject.Horizon.LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            if (scertClient.CipherService != null)
                scertClient.CipherService.EnableEncryption = MediusClass.GetAppSettingsOrDefault(data.ApplicationId).EnableEncryption;
            bool enableEncryption = MediusClass.GetAppSettingsOrDefault(data.ApplicationId).EnableEncryption;

            switch (message)
            {
                case RT_MSG_CLIENT_HELLO clientHello:
                    {
                        if (data.State > ClientState.HELLO)
                        {
                            LoggerAccessor.LogError($"Unexpected RT_MSG_CLIENT_HELLO from {clientChannel.RemoteAddress}: {clientHello}");
                            break;
                        }

                        data.State = ClientState.HELLO;
                        Queue(new RT_MSG_SERVER_HELLO() { RsaPublicKey = enableEncryption ? MediusClass.Settings.DefaultKey.N : Org.BouncyCastle.Math.BigInteger.Zero }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CRYPTKEY_PUBLIC clientCryptKeyPublic:
                    {
                        if (data.State > ClientState.HANDSHAKE)
                        {
                            LoggerAccessor.LogError($"Unexpected RT_MSG_CLIENT_CRYPTKEY_PUBLIC from {clientChannel.RemoteAddress}: {clientCryptKeyPublic}");
                            break;
                        }

                        /*
                        // Ensure key is correct
                        if (!clientCryptKeyPublic.PublicKey.Reverse().SequenceEqual(MediusStarter.Settings.MPSKey.N.ToByteArrayUnsigned()))
                        {
                            LoggerAccessor.LogError($"Client {clientChannel.RemoteAddress} attempting to authenticate with invalid key {Encoding.Default.GetString(clientCryptKeyPublic.PublicKey)}");
                            data.State = ClientState.DISCONNECTED;
                            await clientChannel.CloseAsync();
                            break;
                        }
                        */

                        data.State = ClientState.CONNECT_1;

                        // generate new client session key
                        scertClient.CipherService?.GenerateCipher(CipherContext.RSA_AUTH, clientCryptKeyPublic.PublicKey.Reverse().ToArray());
                        scertClient.CipherService?.GenerateCipher(CipherContext.RC_CLIENT_SESSION);

                        Queue(new RT_MSG_SERVER_CRYPTKEY_PEER() { SessionKey = scertClient.CipherService?.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_TCP clientConnectTcp:
                    {
                        if (data.State > ClientState.CONNECT_1)
                        {
                            LoggerAccessor.LogError($"Unexpected RT_MSG_CLIENT_CONNECT_TCP from {clientChannel.RemoteAddress}: {clientConnectTcp}");
                            break;
                        }
                        /*
                        if (clientConnectTcp.AccessToken == null)
                            throw new Exception($"AccessToken in RT_MSG_CLIENT_CONNECT_TCP from {clientChannel.RemoteAddress}: {clientConnectTcp} is null");
                        */

                        data.ApplicationId = clientConnectTcp.AppId;
                        scertClient.ApplicationID = clientConnectTcp.AppId;

                        List<int> pre108ServerConnect = new List<int>() { 10114, 10164, 10190, 10124, 10130, 10164, 10284, 10330, 10334, 10414, 10421, 10442, 10538, 10540, 10550, 10582, 10584, 10680, 10683, 10684, 10724 };

                        if (clientConnectTcp.AccessToken != null)
                        {
                            data.ClientObject = MediusClass.Manager.GetClientByAccessToken(clientConnectTcp.AccessToken, data.ApplicationId);
                            if (data.ClientObject != null)
                            {
                                clientObject = data.ClientObject;
                                LoggerAccessor.LogInfo("MPS Client Connected!");
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
                        if (scertClient.MediusVersion == 108 && pre108ServerConnect.Contains(scertClient.ApplicationID))
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
                        {
                            LoggerAccessor.LogError($"Unexpected RT_MSG_CLIENT_APP_TOSERVER from {clientChannel.RemoteAddress}: {clientAppToServer}");
                            break;
                        }

                        await ProcessMediusMessage(clientAppToServer.Message, clientChannel, data);
                        break;
                    }
                case RT_MSG_CLIENT_DISCONNECT _:
                    {
                        //LoggerAccessor.LogInfo($"Client id = {data.ClientObject.AccountId} disconnected by request with no specific reason\n");
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
                        LoggerAccessor.LogWarn($"UNHANDLED RT MESSAGE: {message}");
                        break;
                    }
            }

            return;
        }

        protected virtual async Task ProcessMediusMessage(BaseMediusMessage? message, IChannel clientChannel, ChannelData data)
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
                        DMEObject dme = new(dmeSetAttributesRequest)
                        {
                            ApplicationId = data.ApplicationId
                        };
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
                        if (createGameWithAttrResponse.MessageID.Value.Contains("-") && data.ClientObject != null)
                        {
                            bool offseted = false;
                            int partyType = 0;
                            int gameOrPartyId = 0;
                            int accountId = 0;
                            string msgId = string.Empty;

                            string[] messageParts = createGameWithAttrResponse.MessageID.Value.Split('-');

                            if (messageParts.Length == 5) // This is an ugly hack, anonymous accounts can have a negative ID which messes up the traditional parser.
                            {
                                offseted = true;
                                gameOrPartyId = int.Parse(messageParts[0]);
                                accountId = -int.Parse(messageParts[2]);
                                msgId = messageParts[3];
                            }
                            else if (int.TryParse(messageParts[0], out gameOrPartyId) &&
                                int.TryParse(messageParts[1], out accountId))
                                msgId = messageParts[2];
                            else
                            {
                                LoggerAccessor.LogWarn("[MPS] - createGameWithAttrResponse received an invalid MessageID, ignoring request...");
                                break;
                            }

                            Game? game = MediusClass.Manager.GetGameByGameId(gameOrPartyId);
                            Party? party = MediusClass.Manager.GetPartyByPartyId(gameOrPartyId);
                            ClientObject? rClient = MediusClass.Manager.GetClientByAccountId(accountId, data.ClientObject.ApplicationId);
                            try
                            {
                                if (offseted)
                                    partyType = int.Parse(messageParts[4]);
                                else
                                    partyType = int.Parse(messageParts[3]);
                            }
                            catch (Exception)
                            {
                                // Not Important.
                            }

                            if (partyType == 1 && party != null)
                            {
                                party.DMEWorldId = createGameWithAttrResponse.WorldID;
                                await party.PartyCreated();
                                rClient?.Queue(new MediusPartyCreateResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    MediusWorldID = gameOrPartyId,
                                    StatusCode = MediusCallbackStatus.MediusSuccess
                                });
                            }
                            else if (data.ApplicationId == 22920 || data.ApplicationId == 21834)
                            {
                                //Send Matchmaking create game
                                if (!createGameWithAttrResponse.IsSuccess)
                                {
                                    rClient?.Queue(new MediusMatchCreateGameResponse()
                                    {
                                        MessageID = new MessageId(msgId),
                                        StatusCode = MediusCallbackStatus.MediusFail
                                    });

                                    if (game != null)
                                        await game.EndGame(data.ApplicationId);
                                }
                                else if (game != null)
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

                                    if (game != null)
                                        await game.EndGame(data.ApplicationId);
                                }
                                else if (game != null)
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
                        }
                        else
                        {
                            data.ClientObject?.Queue(new MediusCreateGameResponse()
                            {
                                MessageID = new MessageId(createGameWithAttrResponse.MessageID.Value),
                                StatusCode = MediusCallbackStatus.MediusSuccess,
                                MediusWorldID = createGameWithAttrResponse.WorldID
                            });

                            // Send to plugins
                            await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_CREATED, new OnPlayerGameArgs() { Player = data.ClientObject, Game = data.ClientObject?.CurrentGame });
                        }

                        break;
                    }
                #endregion

                #region MediusServerJoinGameResponse
                case MediusServerJoinGameResponse joinGameResponse:
                    {
                        if (data.ClientObject != null)
                        {
                            bool offseted = false;
                            int partyType = 0;
                            int gameOrPartyId = 0;
                            int accountId = 0;
                            string msgId = string.Empty;

                            string[] messageParts = joinGameResponse.MessageID.Value.Split('-');

                            if (messageParts.Length == 5) // This is an ugly hack, anonymous accounts can have a negative ID which messes up the traditional parser.
                            {
                                offseted = true;
                                gameOrPartyId = int.Parse(messageParts[0]);
                                accountId = -int.Parse(messageParts[2]);
                                msgId = messageParts[3];
                            }
                            else if (int.TryParse(messageParts[0], out gameOrPartyId) &&
                                int.TryParse(messageParts[1], out accountId))
                                msgId = messageParts[2];
                            else
                            {
                                LoggerAccessor.LogWarn("[MPS] - joinGameResponse received an invalid MessageID, ignoring request...");
                                break;
                            }

                            Game? game = MediusClass.Manager.GetGameByGameId(gameOrPartyId);
                            Party? party = MediusClass.Manager.GetPartyByPartyId(gameOrPartyId);
                            ClientObject? rClient = MediusClass.Manager.GetClientByAccountId(accountId, data.ClientObject.ApplicationId);
                            try
                            {
                                if (offseted)
                                    partyType = int.Parse(messageParts[4]);
                                else
                                    partyType = int.Parse(messageParts[3]);
                            }
                            catch (Exception)
                            {
                                // Not Important.
                            }

                            IPHostEntry host = Dns.GetHostEntry(MediusClass.Settings.NATIp ?? "natservice.pdonline.scea.com");

                            if (partyType == 1 && party != null)
                            {
                                rClient?.Queue(new MediusPartyJoinByIndexResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    PartyHostType = party.PartyHostType,
                                    ConnectionInfo = new NetConnectionInfo()
                                    {
                                        AccessKey = rClient.Token,
                                        SessionKey = rClient.SessionKey,
                                        WorldID = MediusClass.Manager.GetOrCreateDefaultLobbyChannel(rClient.ApplicationId).Id,
                                        ServerKey = joinGameResponse.pubKey,
                                        AddressList = new NetAddressList()
                                        {
                                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                                    {
                                                    new NetAddress() { Address = ((DMEObject)data.ClientObject).IP.MapToIPv4().ToString(), Port = ((DMEObject)data.ClientObject).Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                    new NetAddress() { Address = host.AddressList.First().ToString(), Port = MediusClass.Settings.NATPort, AddressType = NetAddressType.NetAddressTypeNATService },
                                                    }
                                        },
                                        Type = NetConnectionType.NetConnectionTypeClientServerTCPAuxUDP
                                    },
                                    partyIndex = party.Id,
                                    maxPlayers = party.MaxPlayers
                                });
                            }
                            else if (!joinGameResponse.IsSuccess)
                            {
                                rClient?.Queue(new MediusJoinGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusFail
                                });
                            }
                            else
                            {
                                if (gameOrPartyId != 0 && game != null)
                                {
                                    bool P2PLog = false;

                                    #region P2P
                                    if (game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                        game.netAddressList?.AddressList[0].AddressType == NetAddressType.NetAddressTypeSignalAddress && rClient != null)
                                    {
                                        P2PLog = true;

                                        // Join game P2P
                                        await rClient.JoinGameP2P(game);

                                        rClient.Queue(new MediusJoinGameResponse()
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
                                                        new() { Address = game.netAddressList.AddressList[0].Address, Port = game.netAddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeSignalAddress},
                                                        new() { AddressType = NetAddressType.NetAddressNone},
                                                    }
                                                },
                                                Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                            }
                                        });
                                    }
                                    else if (game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                        game.netAddressList?.AddressList[0].AddressType == NetAddressType.NetAddressTypeExternal &&
                                        game.netAddressList?.AddressList[1].AddressType == NetAddressType.NetAddressTypeInternal && rClient != null
                                        )
                                    {
                                        P2PLog = true;

                                        // Join game P2P
                                        await rClient.JoinGameP2P(game);

                                        rClient.Queue(new MediusJoinGameResponse()
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
                                                        new() { Address = game.netAddressList.AddressList[0].Address, Port = game.netAddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                        new() { Address = game.netAddressList.AddressList[1].Address, Port = game.netAddressList.AddressList[1].Port, AddressType = NetAddressType.NetAddressTypeInternal},
                                                    }
                                                },
                                                Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                            }
                                        });
                                    }

                                    else if (game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                        game.netAddressList?.AddressList[0].AddressType == NetAddressType.NetAddressTypeBinaryExternalVport &&
                                        game.netAddressList?.AddressList[1].AddressType == NetAddressType.NetAddressTypeBinaryInternalVport && rClient != null
                                        )
                                    {
                                        P2PLog = true;

                                        // Join game P2P
                                        await rClient.JoinGameP2P(game);

                                        rClient.Queue(new MediusJoinGameResponse()
                                        {
                                            MessageID = new MessageId(msgId),
                                            StatusCode = MediusCallbackStatus.MediusSuccess,
                                            GameHostType = game.GameHostType,
                                            ConnectInfo = new NetConnectionInfo()
                                            {
                                                AccessKey = rClient.Token,
                                                SessionKey = rClient.SessionKey,
                                                WorldID = game.WorldID,
                                                ServerKey = game.pubKey,
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
                                    else if (rClient != null && rClient.MediusVersion == 108)
                                    {
                                        // Join game DME
                                        await rClient.JoinGame(game, joinGameResponse.DmeClientIndex);

                                        rClient.Queue(new MediusJoinGameResponse()
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
                                                    new NetAddress() { Address = ((DMEObject)data.ClientObject).IP.MapToIPv4().ToString(), Port = ((DMEObject)data.ClientObject).Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                    new NetAddress() { Address = host.AddressList.First().ToString(), Port = MediusClass.Settings.NATPort, AddressType = NetAddressType.NetAddressTypeNATService }
                                                    }
                                                },
                                                Type = NetConnectionType.NetConnectionTypeClientServerTCP
                                            }
                                        });
                                    }
                                    else if (rClient != null)
                                    {
                                        // Join game DME
                                        await rClient.JoinGame(game, joinGameResponse.DmeClientIndex);

                                        rClient.Queue(new MediusJoinGameResponse()
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
                                                        new NetAddress() { Address = ((DMEObject)data.ClientObject).IP.MapToIPv4().ToString(), Port = ((DMEObject)data.ClientObject).Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                        new NetAddress() { Address = host.AddressList.First().ToString(), Port = MediusClass.Settings.NATPort, AddressType = NetAddressType.NetAddressTypeNATService }
                                                }
                                                },
                                                Type = NetConnectionType.NetConnectionTypeClientServerTCPAuxUDP
                                            }
                                        });
                                    }

                                    /// For Legacy Medius v1.50 clients that DO NOT 
                                    /// send a ServerConnectNotificationConnect when creating a game
                                    if (data.ClientObject.MediusVersion < 109 && data.ClientObject.ApplicationId != 10394)
                                        await game.OnMediusJoinGameResponse(rClient?.SessionKey, P2PLog ? game.WorldID.ToString() : game.DMEWorldId.ToString());
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

                        data.ClientObject?.OnConnected();

                        // validate name
                        if (!MediusClass.PassTextFilter(data.ApplicationId, TextFilterContext.GAME_NAME, Convert.ToString(serverCreateGameOnSelfRequest.GameName)))
                        {
                            data.ClientObject?.Queue(new MediusServerCreateGameOnMeResponse()
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

                        data.ClientObject?.OnConnected();

                        // validate name
                        if (!MediusClass.PassTextFilter(data.ApplicationId, TextFilterContext.GAME_NAME, Convert.ToString(serverCreateGameOnSelfRequest0.GameName)))
                        {
                            data.ClientObject?.Queue(new MediusServerCreateGameOnMeResponse()
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

                        data.ClientObject?.OnConnected();

                        // validate name
                        if (!MediusClass.PassTextFilter(data.ApplicationId, TextFilterContext.GAME_NAME, Convert.ToString(serverCreateGameOnMeRequest.GameName)))
                        {
                            data.ClientObject?.Queue(new MediusServerCreateGameOnMeResponse()
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
                        Game? game = MediusClass.Manager.GetGameByGameId(serverMoveGameWorldOnMeRequest.CurrentMediusWorldID);
                        if (game?.WorldID != serverMoveGameWorldOnMeRequest.CurrentMediusWorldID)
                        {
                            data.ClientObject?.Queue(new MediusServerMoveGameWorldOnMeResponse()
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

                            LoggerAccessor.LogInfo("MediusServerMoveGameWorldOnMeRequest Successful");
                            data.ClientObject?.Queue(new MediusServerMoveGameWorldOnMeResponse()
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
                        if (data.ClientObject?.CurrentGame != null)
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
                        if (data.ClientObject != null)
                        {
                            if (data.ClientObject.CurrentGame != null)
                                await data.ClientObject.LeaveGame(data.ClientObject.CurrentGame);

                            data.ClientObject.Queue(new MediusServerEndGameOnMeResponse()
                            {
                                MessageID = serverEndGameOnMeRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                            });
                        }

                        break;
                    }
                #endregion

                #region MediusServerReport
                case MediusServerReport serverReport:
                    {

                        (data.ClientObject as DMEObject)?.OnServerReport(serverReport);
                        data.ClientObject?.OnConnected();
                        //data.ClientObject.OnConnected();
                        //data.ClientObject.KeepAliveUntilNextConnection();
                        //LoggerAccessor.LogInfo($"ServerReport SessionKey {serverReport.SessionKey} MaxWorlds {serverReport.MaxWorlds} MaxPlayersPerWorld {serverReport.MaxPlayersPerWorld} TotalWorlds {serverReport.ActiveWorldCount} TotalPlayers {serverReport.TotalActivePlayers} Alert {serverReport.AlertLevel} ConnIndex {data.ClientObject.DmeId} WorldID {data.ClientObject.WorldId}");
                        break;
                    }
                #endregion

                #region MediusServerConnectNotification
                case MediusServerConnectNotification connectNotification:
                    {
                        LoggerAccessor.LogInfo("MediusServerConnectNotification Received");
                        //LoggerAccessor.LogWarn($"sessionkey {(data.ClientObject as DMEObject).SessionKey} worldid {(int)connectNotification.MediusWorldUID}");
                        if (data.ClientObject != null && MediusClass.Manager.GetGameByDmeWorldId(((DMEObject)data.ClientObject).SessionKey ?? string.Empty, (int)connectNotification.MediusWorldUID) != null)
                        {
                            Game? conn = MediusClass.Manager.GetGameByDmeWorldId(((DMEObject)data.ClientObject).SessionKey ?? string.Empty, (int)connectNotification.MediusWorldUID);
                            if (conn != null)
                                await conn.OnMediusServerConnectNotification(connectNotification, conn.DMEWorldId.ToString());
                        }
                        else if (data.ClientObject != null)
                        {
                            Party? conn = MediusClass.Manager.GetPartyByDmeWorldId(((DMEObject)data.ClientObject).SessionKey ?? string.Empty, (int)connectNotification.MediusWorldUID);
                            if (conn != null)
                                await conn.OnMediusServerConnectNotification(connectNotification);
                        }

                        //MediusServerConnectNotification -  sent Notify msg to MUM
                        //DmeServerGetConnectKeys
                        /*
                        if (data.ClientObject.SessionKey == null)
                        {
                            LoggerAccessor.LogWarn($"ServerConnectNotificationHandler - DmeServerGetConnectKeys error = {data.ClientObject.SessionKey} is null");
                            return;
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

                        if (game != null && endGameRequest.BrutalFlag == true && data.ClientObject != null)
                        {
                            await game.EndGame(data.ClientObject.ApplicationId);

                            data.ClientObject.Queue(new MediusServerEndGameResponse()
                            {
                                MessageID = endGameRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS
                            });
                        }
                        else if (game != null && endGameRequest.BrutalFlag == false)
                        {
                            data.ClientObject?.Queue(new MediusServerEndGameResponse()
                            {
                                MessageID = endGameRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                            });
                        }
                        else
                        {
                            data.ClientObject?.Queue(new MediusServerEndGameResponse()
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
                        LoggerAccessor.LogInfo("ServerSessionEndRequest Received");

                        //DmeServerGetConnectKeys
                        if (data.ClientObject?.SessionKey == null)
                        {
                            LoggerAccessor.LogWarn($"MediusServerSessionEndRequestHandler: DmeServerGetConnectKeys error {data.ClientObject?.SessionKey} is null");
                            data?.ClientObject?.Queue(new MediusServerSessionEndResponse()
                            {
                                MessageID = sessionEndRequest.MessageID,
                                ErrorCode = MGCL_ERROR_CODE.MGCL_SESSIONEND_FAILED
                            });
                        }
                        else
                        {
                            data?.ClientObject.KeepAliveUntilNextConnection();

                            //Success
                            LoggerAccessor.LogInfo("Server Session End Success");
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
                        LoggerAccessor.LogWarn($"Unhandled Medius Message: {message}");
                        break;
                    }
            }
        }

        public DMEObject? GetFreeDme(int appId)
        {
            try
            {
                return _scertHandler?.Group?
                    .Select(x => _channelDatas[x.Id.AsLongText()]?.ClientObject)
                    .Where(x => x is DMEObject && x != null && (x.ApplicationId == appId || x.ApplicationId == 0))
                    .MediusMinBy(x => (x as DMEObject).CurrentWorlds) as DMEObject;
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("No DME Game Server assigned to this AppId\n", e);
            }

            return null;
        }

        public void SendServerCreateGameWithAttributesRequestP2P(string msgId, int acctId, int gameId, bool partyType, Game game, ClientObject client)
        {
            //{gameId}-{acctId}-{messageId}-{partyType}
            Queue(new RT_MSG_SERVER_APP()
            {
                Message = new MediusServerCreateGameWithAttributesRequest2()
                {
                    MessageID = new MessageId($"{msgId}"),
                    MediusWorldID = (uint)gameId,
                    Attributes = game.Attributes,
                    ApplicationID = client.ApplicationId,
                    MaxClients = game.MaxPlayers,
                    ConnectInfo = new NetConnectionInfo()
                    {
                        AccessKey = client.Token,
                        SessionKey = client.SessionKey,
                        WorldID = game.WorldID,
                        ServerKey = new RSA_KEY(),
                        AddressList = new NetAddressList()
                        {
                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                              {
                                  new() { BinaryAddress = BitConverter.ToUInt32(client.IP.GetAddressBytes()), BinaryPort = 0, AddressType = NetAddressType.NetAddressTypeBinaryExternal }, //Address = game.netAddressList.AddressList[0].Address, Port = game.netAddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeBinaryExternal},
                                  new() { AddressType = NetAddressType.NetAddressNone},
                              }
                        },
                        Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                    }
                }
            }, channel);
        }

        public void SendServerCreateGameWithAttributesRequest(string msgId, int acctId, int gameId, bool partyType, int gameAttributes, int clientAppId, int gameMaxPlayers)
        {
            //{gameId}-{acctId}-{messageId}-{partyType}
            Queue(new RT_MSG_SERVER_APP()
            {
                Message = new MediusServerCreateGameWithAttributesRequest()
                {
                    MessageID = new MessageId($"{gameId}-{acctId}-{msgId}-{1}"),
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
            DMEObject dme = new(request);
            dme.BeginSession();
            MediusClass.Manager.AddDmeClient(dme);
            return dme;
        }

        public DMEObject ReserveDMEObject(MediusServerSessionBeginRequest1 request)
        {
            DMEObject dme = new(request);
            dme.BeginSession();
            MediusClass.Manager.AddDmeClient(dme);
            return dme;
        }

        public DMEObject ReserveDMEObject(MediusServerSessionBeginRequest2 request)
        {
            DMEObject dme = new(request);
            dme.BeginSession();
            MediusClass.Manager.AddDmeClient(dme);
            return dme;
        }

        public ClientObject ReserveClient(MediusServerSessionBeginRequest mgclSessionBeginRequest, string? MachineId)
        {
            ClientObject client = new(MachineId);
            client.BeginSession();
            return client;
        }
    }
}
