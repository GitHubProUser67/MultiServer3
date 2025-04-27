using CustomLogger;
using Horizon.RT.Common;
using Horizon.RT.Models;
using Horizon.DME.PluginArgs;
using System.Collections.Concurrent;
using System.Diagnostics;
using Horizon.PluginManager;
using NetworkLibrary.Extension;

namespace Horizon.DME.Models
{
    public class World : IDisposable
    {
        public int MAX_CLIENTS_PER_WORLD = DmeClass.Settings.MaxClientsPerWorld;
        public int MAX_WORLDS = DmeClass.Settings.DmeServerMaxWorld;

        #region Id Management

        private static ConcurrentDictionary<int, World> _idToWorld = new();
        private ConcurrentDictionary<int, bool> _pIdIsUsed = new();

        private object _ClientIndexlock = new();

        private void RegisterWorld(int GameChannelWorldId, int WorldId)
        {
            if (_idToWorld.Count > MAX_WORLDS)
            {
                LoggerAccessor.LogError("[DMEWorld] - Max worlds reached or requested WorldId higher than allowed value in DME config!");
                return;
            }

            this.GameChannelWorldId = GameChannelWorldId;
            this.WorldId = WorldId;

            if (_idToWorld.TryAdd(WorldId, this))
                LoggerAccessor.LogInfo($"[DMEWorld] - Registered world with id {WorldId}");
            else
            {
                this.WorldId = -1;
                LoggerAccessor.LogError($"[DMEWorld] - Failed to register world with id {WorldId}");
            }
        }

        private void FreeWorld()
        {
            if (_idToWorld.TryRemove(WorldId, out _))
                LoggerAccessor.LogInfo($"[DMEWorld] - Unregistered world with id {WorldId}");
            else
                LoggerAccessor.LogError($"[DMEWorld] - Failed to unregister world with id {WorldId}");
        }

        public World? GetWorldById(int GameChannelWorldId, int DmeWorldId)
        {
            return _idToWorld.Values.FirstOrDefault(world => world.GameChannelWorldId == GameChannelWorldId && world.WorldId == DmeWorldId);
        }

        private bool TryRegisterNewClientIndex(out int index)
        {
            lock (_ClientIndexlock)
            {
                for (index = 0; index < _pIdIsUsed.Count; ++index)
                {
                    if (_pIdIsUsed.TryGetValue(index, out bool isUsed) && !isUsed)
                    {
                        _pIdIsUsed[index] = true;
                        return true;
                    }
                }
            }

            return false;
        }

        public void UnregisterClientIndex(int index)
        {
            _pIdIsUsed[index] = false;
        }

        #endregion

        #region TokenManagement

        public Dictionary<ushort, List<int>> clientTokens = new();

        #endregion

        public int WorldId { get; protected set; } = -1;

        public int GameChannelWorldId { get; protected set; } = -1;

        public int ApplicationId { get; protected set; } = 0;

        public int MaxPlayers { get; protected set; } = 0;

        public int SessionMaster { get; protected set; } = 0;

        public bool SelfDestructFlag { get; protected set; } = false;

        public bool ForceDestruct { get; protected set; } = false;

        public bool Destroy => ((WorldTimer.Elapsed.TotalSeconds > DmeClass.GetAppSettingsOrDefault(ApplicationId).GameTimeoutSeconds) || SelfDestructFlag) && Clients.IsEmpty;

        public bool Destroyed { get; protected set; } = false;

        public DateTime? UtcLastJoined { get; protected set; }

        public Stopwatch WorldTimer { get; protected set; } = Stopwatch.StartNew();

        public ConcurrentDictionary<int, DMEObject> Clients = new();

        private ConcurrentQueue<MediusServerJoinGameRequest> _requestQueue = new();

        private readonly SemaphoreSlim _requestQueueSemaphore = new(1, 1);

        public MPSClient? Manager { get; } = null;
        
        public World(MPSClient manager, int appId, int maxPlayers, int GameChannelWorldId, int WorldId)
        {
            MaxPlayers = (DmeClass.Settings.MaxClientsOverride != -1) ? DmeClass.Settings.MaxClientsOverride : maxPlayers;

            if (MaxPlayers > MAX_CLIENTS_PER_WORLD)
            {
                LoggerAccessor.LogError($"[DMEWorld] - maxPlayers from {((DmeClass.Settings.MaxClientsOverride != -1) ? "dme config override parameter" : "request")} is higher than MaxClientsPerWorld allowed in DME config, world will not be created!");
                return;
            }

            Manager = manager;
            ApplicationId = appId;

            // populate collection of used player ids
            for (int i = 0; i < MAX_CLIENTS_PER_WORLD; ++i)
                _pIdIsUsed.TryAdd(i, false);

            RegisterWorld(GameChannelWorldId, WorldId);
        }

        public void Dispose()
        {
            FreeWorld();
            Destroyed = true;
        }

        public async Task Stop()
        {
            // Stop all clients
            await Task.WhenAll(Clients.Select(x => x.Value.Stop()));

            _requestQueue.Clear();

            Dispose();
        }

        public Task EnqueueJoinGame(MediusServerJoinGameRequest request)
        {
            _requestQueue.Enqueue(request);
            return Task.CompletedTask;
        }

        public async Task HandleIncomingJoinGame()
        {
            if (!_requestQueueSemaphore.Wait(0))
                return;

            try
            {
                while (_requestQueue.TryDequeue(out MediusServerJoinGameRequest? request))
                {
                    Manager?.Enqueue(await OnJoinGameRequest(request).ConfigureAwait(false));
                }
            }
            finally
            {
                _requestQueueSemaphore.Release();
            }
        }

        public Task HandleIncomingMessages()
        {
            List<Task> tasks = new();

            // Process clients
            for (int i = 0; i < MAX_CLIENTS_PER_WORLD; ++i)
            {
                if (Clients.TryGetValue(i, out DMEObject? client))
                    tasks.Add(client.HandleIncomingMessages());
            }

            return Task.WhenAll(tasks);
        }

        public async Task HandleOutgoingMessages()
        {
            // Process clients
            for (int i = 0; i < MAX_CLIENTS_PER_WORLD; ++i)
            {
                if (Clients.TryGetValue(i, out DMEObject? client))
                {
                    if (client.Destroy || ForceDestruct || Destroyed)
                    {
                        await OnPlayerLeft(client);
                        Manager?.RemoveClient(client);
                        Clients.TryRemove(i, out _);
                        _ = client.Stop();
                    }
                    else if (client.Timedout)
                        client.ForceDisconnect();
                    else if (client.IsAggTime)
                        client.HandleOutgoingMessages();
                }
            }

            // Remove
            if (Destroy)
            {
                if (!Destroyed)
                {
                    LoggerAccessor.LogInfo($"[DMEWorld] - {this} destroyed.");
                    await Stop();
                }

                Manager?.RemoveWorld(this);
            }
        }

        #region Send

        public void BroadcastTcp(DMEObject source, byte[] Payload)
        {
            RT_MSG_CLIENT_APP_SINGLE msg = new()
            {
                TargetOrSource = (short)source.DmeId,
                Payload = Payload
            };

            foreach (var client in Clients)
            {
                if (client.Value == source || !client.Value.IsAuthenticated || !client.Value.IsConnected || !client.Value.HasRecvFlag(RT_RECV_FLAG.RECV_BROADCAST))
                    continue;

                client.Value.EnqueueTcp(msg);
            }
        }

        public void BroadcastTcpScertMessage(BaseScertMessage msg)
        {
            foreach (var client in Clients)
            {
                if (!client.Value.IsAuthenticated || !client.Value.IsConnected || !client.Value.HasRecvFlag(RT_RECV_FLAG.RECV_BROADCAST))
                    continue;

                client.Value.EnqueueTcp(msg);
            }
        }

        public void BroadcastUdp(DMEObject source, byte[] Payload)
        {
            RT_MSG_CLIENT_APP_SINGLE msg = new()
            {
                TargetOrSource = (short)source.DmeId,
                Payload = Payload
            };

            foreach (var client in Clients)
            {
                if (client.Value == source || !client.Value.IsAuthenticated || !client.Value.IsConnected || !client.Value.HasRecvFlag(RT_RECV_FLAG.RECV_BROADCAST))
                    continue;

                client.Value.EnqueueUdp(msg);
            }
        }

        public void SendTcpAppList(DMEObject source, List<int> targetDmeIds, byte[] Payload)
        {
            foreach (int targetId in targetDmeIds)
            {
                if (Clients.TryGetValue(targetId, out DMEObject? client))
                {
                    if (client == null || !client.IsAuthenticated || !client.IsConnected || !client.HasRecvFlag(RT_RECV_FLAG.RECV_LIST))
                        continue;

                    client.EnqueueTcp(new RT_MSG_CLIENT_APP_SINGLE()
                    {
                        TargetOrSource = (short)source.DmeId,
                        Payload = Payload
                    });
                }
            }
        }

        public void SendUdpAppList(DMEObject source, List<int> targetDmeIds, byte[] Payload)
        {
            foreach (int targetId in targetDmeIds)
            {
                if (Clients.TryGetValue(targetId, out DMEObject? client))
                {
                    if (client == null || !client.IsAuthenticated || !client.IsConnected || !client.HasRecvFlag(RT_RECV_FLAG.RECV_LIST))
                        continue;

                    client.EnqueueUdp(new RT_MSG_CLIENT_APP_SINGLE()
                    {
                        TargetOrSource = (short)source.DmeId,
                        Payload = Payload
                    });
                }
            }
        }

        public void SendTcpAppSingle(DMEObject source, short targetDmeId, byte[] Payload)
        {
            DMEObject? target = Clients.FirstOrDefault(x => x.Value.DmeId == targetDmeId).Value;

            if (target != null && target.IsAuthenticated && target.IsConnected && target.HasRecvFlag(RT_RECV_FLAG.RECV_SINGLE))
            {
                target.EnqueueTcp(new RT_MSG_CLIENT_APP_SINGLE()
                {
                    TargetOrSource = (short)source.DmeId,
                    Payload = Payload
                });
            }
        }

        public void SendUdpAppSingle(DMEObject source, short targetDmeId, byte[] Payload)
        {
            DMEObject? target = Clients.FirstOrDefault(x => x.Value.DmeId == targetDmeId).Value;

            if (target != null && target.IsAuthenticated && target.IsConnected && target.HasRecvFlag(RT_RECV_FLAG.RECV_SINGLE))
            {
                target.EnqueueUdp(new RT_MSG_CLIENT_APP_SINGLE()
                {
                    TargetOrSource = (short)source.DmeId,
                    Payload = Payload
                });
            }
        }

        #endregion

        #region Message Handlers

        public void OnEndGameRequest(MediusServerEndGameRequest request)
        {
            SelfDestructFlag = true;
            ForceDestruct = request.BrutalFlag;
        }

        public Task OnPlayerJoined(DMEObject player)
        {
            if (player.RemoteUdpEndpoint == null)
            {
                LoggerAccessor.LogError($"[World] - OnPlayerJoined - player {player.IP} on ApplicationId {player.ApplicationId} has no UdpEndpoint!");
                return Task.CompletedTask;
            }

            player.HasJoined = true;
            UtcLastJoined = DateTimeUtils.GetHighPrecisionUtcTime();

            // Plugin
            DmeClass.Plugins.OnEvent(PluginEvent.DME_PLAYER_ON_JOINED, new OnPlayerArgs()
            {
                Player = player,
                Game = this
            }).Wait();

            // Tell other clients
            foreach (var client in Clients)
            {
                if (!client.Value.HasJoined || client.Value == player || !client.Value.HasRecvFlag(RT_RECV_FLAG.RECV_NOTIFICATION))
                    continue;

                client.Value.EnqueueTcp(new RT_MSG_SERVER_CONNECT_NOTIFY()
                {
                    PlayerIndex = (short)player.DmeId,
                    ScertId = (short)player.ScertId,
                    IP = player.RemoteUdpEndpoint.Address
                });
            }

            _ = Task.Run(() => {
                ConcurrentBag<(RT_TOKEN_MESSAGE_TYPE, ushort, ushort)> tokenList = new();

                Parallel.ForEach(clientTokens.Keys, (ushort token) =>
                {
                    if (clientTokens.TryGetValue(token, out List<int>? value) && value != null && value.Count > 0)
                        tokenList.Add((RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_OWNED, token, (ushort)value[0]));
                });

                if (!tokenList.IsEmpty) // We need to actualize client with every owned tokens.
                    player.EnqueueTcp(new RT_MSG_SERVER_TOKEN_MESSAGE()
                    {
                        TokenList = tokenList.ToList()
                    });
            });

            // Tell server
            Manager?.Enqueue(new MediusServerConnectNotification()
            {
                MediusWorldUID = WorldId,
                PlayerSessionKey = player.SessionKey ?? string.Empty,
                ConnectEventType = MGCL_EVENT_TYPE.MGCL_EVENT_CLIENT_CONNECT
            });

            return Task.CompletedTask;
        }

        public async Task OnPlayerLeft(DMEObject player)
        {
            if (player.RemoteUdpEndpoint == null)
            {
                LoggerAccessor.LogError($"[World] - OnPlayerLeft - player {player.IP} on ApplicationId {player.ApplicationId} has no UdpEndpoint!");
                return;
            }

            player.HasJoined = false;

            // Plugin
            await DmeClass.Plugins.OnEvent(PluginEvent.DME_PLAYER_ON_LEFT, new OnPlayerArgs()
            {
                Player = player,
                Game = this
            });

            if (player.MediusVersion >= 109)
            {
                // Migrate session master
                if (player.DmeId == SessionMaster && Clients.Any())
                {
                    DMEObject? preferredHost = Clients.ToArray()
                        .Select(client => client.Value)
                        .Where(client => client != player)
                        .OrderBy(client => client.DmeId)
                        .FirstOrDefault();

                    if (preferredHost != null)
                    {
                        SessionMaster = preferredHost.DmeId;
                        LoggerAccessor.LogWarn($"[DMEWorld] - Session master migrated to client {SessionMaster}");
                    }
                }
            }

            // Tell other clients
            foreach (var client in Clients)
            {
                if (!client.Value.HasJoined || client.Value == player || !client.Value.HasRecvFlag(RT_RECV_FLAG.RECV_NOTIFICATION))
                    continue;

                client.Value.EnqueueTcp(new RT_MSG_SERVER_DISCONNECT_NOTIFY()
                {
                    PlayerIndex = (short)player.DmeId,
                    ScertId = (short)player.ScertId,
                    IP = player.RemoteUdpEndpoint.Address
                });
            }

            // Tell server
            Manager?.Enqueue(new MediusServerConnectNotification()
            {
                MediusWorldUID = WorldId,
                PlayerSessionKey = player.SessionKey ?? string.Empty,
                ConnectEventType = MGCL_EVENT_TYPE.MGCL_EVENT_CLIENT_DISCONNECT
            });
        }
#pragma warning disable
        public async Task<MediusServerJoinGameResponse> OnJoinGameRequest(MediusServerJoinGameRequest request)
        {
            DMEObject newClient;

            await Task.Delay(100);

            // find existing client and reuse
            KeyValuePair<int, DMEObject> existingClient = Clients.FirstOrDefault(x => x.Value.SessionKey == request.ConnectInfo.SessionKey);
            if (existingClient.Value != null)
            {
                // found existing
                return new MediusServerJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    DmeClientIndex = existingClient.Value.DmeId,
                    AccessKey = request.ConnectInfo.AccessKey,
                    Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                    pubKey = request.ConnectInfo.ServerKey
                };
            }

            // If world is full then fail
            if (Clients.Count >= MAX_CLIENTS_PER_WORLD)
            {
                LoggerAccessor.LogWarn($"[DMEWorld] - Player attempted to join world {this} but world is full!");
                return new MediusServerJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    Confirmation = MGCL_ERROR_CODE.MGCL_UNSUCCESSFUL
                };
            }

            if (TryRegisterNewClientIndex(out int newClientIndex))
            {
                if (!Clients.TryAdd(newClientIndex, newClient = new DMEObject(request.ConnectInfo.SessionKey, this, newClientIndex)))
                {
                    UnregisterClientIndex(newClientIndex);
                    return new MediusServerJoinGameResponse()
                    {
                        MessageID = request.MessageID,
                        Confirmation = MGCL_ERROR_CODE.MGCL_UNSUCCESSFUL
                    };
                }
                else
                {
                    newClient.ApplicationId = this.ApplicationId;
                    newClient.OnDestroyed = (client) => {
                        UnregisterClientIndex(client.DmeId);
                        LoggerAccessor.LogWarn($"[DMEWorld] - Player:{client} left world {this}, {client.DmeId} Freed.");
                    };
                }
            }
            else
            {
                LoggerAccessor.LogWarn($"Player attempted to join world {this} but unable to add player!");
                return new MediusServerJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    Confirmation = MGCL_ERROR_CODE.MGCL_UNSUCCESSFUL
                };
            }

            // Add client to manager
            Manager.AddClient(newClient);

            /*if (DmeClass.GetAppSettingsOrDefault(ApplicationId).EnableDmeEncryption)
                return new MediusServerJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    DmeClientIndex = newClient.DmeId,
                    AccessKey = request.ConnectInfo.AccessKey,
                    Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                    pubKey = request.ConnectInfo.ServerKey
                };
            else*/
                return new MediusServerJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    DmeClientIndex = newClient.DmeId,
                    AccessKey = request.ConnectInfo.AccessKey,
                    Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS
                };
        }
#pragma warning restore
        #endregion
    }
}
