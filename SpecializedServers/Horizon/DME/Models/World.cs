using CustomLogger;
using Horizon.RT.Common;
using Horizon.RT.Models;
using Horizon.DME.PluginArgs;
using System.Collections.Concurrent;
using System.Diagnostics;
using Horizon.PluginManager;
using Horizon.MEDIUS;

namespace Horizon.DME.Models
{
    public class World : IDisposable
    {
        public int MAX_CLIENTS_PER_WORLD = DmeClass.Settings.MaxClientsPerWorld;
        public int MAX_WORLDS = DmeClass.Settings.DmeServerMaxWorld;

        #region Id Management

        private static ConcurrentDictionary<uint, World> _idToWorld = new();
        private ConcurrentDictionary<int, bool> _pIdIsUsed = new();
        private static object _lock = new();

        private void RegisterWorld(uint WorldId)
        {
            if (_idToWorld.Count > MAX_WORLDS)
            {
                LoggerAccessor.LogError("[DMEWorld] - Max worlds reached or requested WorldId higher than allowed value in DME config!");
                return;
            }

            if (_idToWorld.Keys.Any())
            {
                uint PreviousWorldId = 0;

                foreach (uint FoundWorldId in _idToWorld.Keys.OrderBy(k => k))
                {
                    if (FoundWorldId - PreviousWorldId > 1)
                    {
                        WorldId = PreviousWorldId + 1; // Assigning the next available ID after the gap
                        break;
                    }

                    PreviousWorldId = FoundWorldId;
                }
            }

            this.WorldId = WorldId;

            _idToWorld.TryAdd(WorldId, this);
            LoggerAccessor.LogInfo($"[DMEWorld] - Registered world with id {WorldId}");
        }

        private void FreeWorld()
        {
            _idToWorld.TryRemove(WorldId, out _);
            LoggerAccessor.LogInfo($"[DMEWorld] - Unregistered world with id {WorldId}");
        }

        private bool TryRegisterNewClientIndex(out int index)
        {
            lock (_lock)
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

        public uint WorldId { get; protected set; } = 0;

        public int ApplicationId { get; protected set; } = 0;

        public int MaxPlayers { get; protected set; } = 0;

        public int SessionMaster { get; protected set; } = 0;

        public bool SelfDestructFlag { get; protected set; } = false;

        public bool ForceDestruct { get; protected set; } = false;

        public bool Destroy => ((WorldTimer.Elapsed.TotalSeconds > DmeClass.GetAppSettingsOrDefault(ApplicationId).GameTimeoutSeconds) || SelfDestructFlag) && Clients.IsEmpty;

        public bool Destroyed { get; protected set; } = false;

        public Stopwatch WorldTimer { get; protected set; } = Stopwatch.StartNew();

        public ConcurrentDictionary<int, ClientObject> Clients = new();

        public MPSClient? Manager { get; } = null;
        
        public World(MPSClient manager, int appId, int maxPlayers, uint WorldId)
        {
            if (maxPlayers > MAX_CLIENTS_PER_WORLD)
            {
                LoggerAccessor.LogError("[DMEWorld] - maxPlayers from request is higher than MaxClientsPerWorld allowed in DME config, world will not be created!");
                return;
            }

            Manager = manager;
            ApplicationId = appId;

            // populate collection of used player ids
            for (int i = 0; i < MAX_CLIENTS_PER_WORLD; ++i)
                _pIdIsUsed.TryAdd(i, false);

            RegisterWorld(WorldId);
            MaxPlayers = maxPlayers;
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

            Dispose();
        }

        public Task HandleIncomingMessages()
        {
            List<Task> tasks = new();

            // Process clients
            for (int i = 0; i < MAX_CLIENTS_PER_WORLD; ++i)
            {
                if (Clients.TryGetValue(i, out ClientObject? client))
                    tasks.Add(client.HandleIncomingMessages());
            }

            return Task.WhenAll(tasks);
        }

        public async Task HandleOutgoingMessages()
        {
            // Process clients
            for (int i = 0; i < MAX_CLIENTS_PER_WORLD; ++i)
            {
                if (Clients.TryGetValue(i, out ClientObject? client))
                {
                    if (client.Destroy || ForceDestruct || Destroyed)
                    {
                        await OnPlayerLeft(client);
                        Manager?.RemoveClient(client);
                        _ = client.Stop();
                        Clients.TryRemove(i, out _);
                    }
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

        public void BroadcastTcp(ClientObject source, byte[] Payload)
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

        public void BroadcastUdp(ClientObject source, byte[] Payload)
        {
            RT_MSG_CLIENT_APP_SINGLE msg = new()
            {
                TargetOrSource = (short)source.DmeId,
                Payload = Payload
            };

            foreach (var client in Clients)
            {
                if (client.Value == source || !client.Value.IsAuthenticated || !client.Value.IsConnected || !client.Value.HasRecvFlag(RT_RECV_FLAG.RECV_BROADCAST))
                //if (!client.Value.IsAuthenticated || !client.Value.IsConnected || !client.Value.HasRecvFlag(RT_RECV_FLAG.RECV_BROADCAST))
                    continue;

                client.Value.EnqueueUdp(msg);
            }
        }

        public void SendTcpAppList(ClientObject source, List<int> targetDmeIds, byte[] Payload)
        {
            foreach (int targetId in targetDmeIds)
            {
                if (Clients.TryGetValue(targetId, out ClientObject? client))
                {
                    if (client == null || !client.IsAuthenticated || !client.IsConnected || !client.HasRecvFlag(RT_RECV_FLAG.RECV_LIST))
                        continue;

                    client.EnqueueTcp(new RT_MSG_CLIENT_APP_LIST()
                    {
                        SourceIn = (short)source.DmeId,
                        Payload = Payload
                    });
                }
            }
        }

        public void SendUdpAppList(ClientObject source, List<int> targetDmeIds, byte[] Payload)
        {
            foreach (int targetId in targetDmeIds)
            {
                if (Clients.TryGetValue(targetId, out ClientObject? client))
                {
                    if (client == null || !client.IsAuthenticated || !client.IsConnected || !client.HasRecvFlag(RT_RECV_FLAG.RECV_LIST))
                        continue;

                    client.EnqueueUdp(new RT_MSG_CLIENT_APP_LIST()
                    {
                        SourceIn = (short)source.DmeId,
                        Payload = Payload
                    });
                }
            }
        }

        public void SendTcpAppSingle(ClientObject source, short targetDmeId, byte[] Payload)
        {
            ClientObject? target = Clients.FirstOrDefault(x => x.Value.DmeId == targetDmeId).Value;

            if (target != null && target.IsAuthenticated && target.IsConnected && target.HasRecvFlag(RT_RECV_FLAG.RECV_SINGLE))
            {
                target.EnqueueTcp(new RT_MSG_CLIENT_APP_SINGLE()
                {
                    TargetOrSource = (short)source.DmeId,
                    Payload = Payload
                });
            }
        }

        public void SendUdpAppSingle(ClientObject source, short targetDmeId, byte[] Payload)
        {
            ClientObject? target = Clients.FirstOrDefault(x => x.Value.DmeId == targetDmeId).Value;

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

        public async Task OnPlayerJoined(ClientObject player)
        {
            player.HasJoined = true;

            // Plugin
            await DmeClass.Plugins.OnEvent(PluginEvent.DME_PLAYER_ON_JOINED, new OnPlayerArgs()
            {
                Player = player,
                Game = this
            });

            // Tell other clients
            foreach (var client in Clients)
            {
                if (!client.Value.HasJoined || client.Value == player || !client.Value.HasRecvFlag(RT_RECV_FLAG.RECV_NOTIFICATION))
                    continue;

                client.Value.EnqueueTcp(new RT_MSG_SERVER_CONNECT_NOTIFY()
                {
                    PlayerIndex = (short)player.DmeId,
                    ScertId = (short)player.ScertId,
                    IP = player.RemoteUdpEndpoint?.Address ?? MediusClass.SERVER_IP
                });
            }

            // Tell server
            Manager?.Enqueue(new MediusServerConnectNotification()
            {
                MediusWorldUID = WorldId,
                PlayerSessionKey = player.SessionKey ?? string.Empty,
                ConnectEventType = MGCL_EVENT_TYPE.MGCL_EVENT_CLIENT_CONNECT
            });
        }

        public async Task OnPlayerLeft(ClientObject player)
        {
            player.HasJoined = false;

            // Plugin
            await DmeClass.Plugins.OnEvent(PluginEvent.DME_PLAYER_ON_LEFT, new OnPlayerArgs()
            {
                Player = player,
                Game = this
            });

            if (player.MediusVersion == 109)
            {
                //Migrate session master
                if (player.DmeId == SessionMaster)
                {
                    SessionMaster++;
                    LoggerAccessor.LogWarn($"[DMEWorld] - Session master migrated to client {SessionMaster}");
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
                    IP = player.RemoteUdpEndpoint?.Address ?? MediusClass.SERVER_IP
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
            ClientObject newClient;

            await Task.Delay(100);

            // find existing client and reuse
            var existingClient = Clients.FirstOrDefault(x => x.Value.SessionKey == request.ConnectInfo.SessionKey);
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
                if (!Clients.TryAdd(newClientIndex, newClient = new ClientObject(request.ConnectInfo.SessionKey, this, newClientIndex)))
                {
                    UnregisterClientIndex(newClientIndex);
                    return new MediusServerJoinGameResponse()
                    {
                        MessageID = request.MessageID,
                        Confirmation = MGCL_ERROR_CODE.MGCL_UNSUCCESSFUL
                    };
                }
                else
                    newClient.OnDestroyed += (client) => { UnregisterClientIndex(client.DmeId); };
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

            if (DmeClass.GetAppSettingsOrDefault(ApplicationId).EnableDmeEncryption)
                return new MediusServerJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    DmeClientIndex = newClient.DmeId,
                    AccessKey = request.ConnectInfo.AccessKey,
                    Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                    pubKey = request.ConnectInfo.ServerKey
                };
            else
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
