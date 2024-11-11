using CustomLogger;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MitmDNS
{
    public class MitmDNSUDPProcessor
    {
        private bool _exit = false;
        private UdpClient listener = null;
        private CancellationTokenSource _cts = null!;
        private ConcurrentDictionary<string, uint> _clientCache = new ConcurrentDictionary<string, uint>();
        private ConcurrentDictionary<string, DateTime> _lastRequestTime = new ConcurrentDictionary<string, DateTime>();
        private HashSet<string> _bannedClients = new HashSet<string>();

        private bool IsIPBanned(string ipAddress, int clientport)
        {
            string key = $"{ipAddress}:{clientport}";

            // Check if the client is in the banned list
            if (_bannedClients.Contains(key))
                return false; // Return false as the client is in the banned list

            // Get the current time
            DateTime now = DateTime.UtcNow;

            // Check the time since the last request
            if (_lastRequestTime.TryGetValue(key, out DateTime lastRequestTime))
            {
                if ((now - lastRequestTime).TotalMilliseconds >= MitmDNSServerConfiguration.MinimumDeltaTimeMs)
                {
                    // Add the client to the banned list
                    _bannedClients.Add(key);
                    return true;
                }
            }

            // Update the last request time
            _lastRequestTime[key] = now;

            // Attempt to update the request count with overflow protection
            _clientCache.AddOrUpdate(key, 1, (k, currentCount) =>
            {
                // If the count exceeds the maximum counter value, reset it
                if (currentCount > uint.MaxValue)
                    return 1;
                else
                    return currentCount + 1; // Otherwise, increment the counter
            });

            // Check if the request count has exceeded the maximum allowed requests
            if (_clientCache[key] >= MitmDNSServerConfiguration.MaximumNumberOfRequests)
            {
                // Add the client to the banned list
                _bannedClients.Add(key);
                return true;
            }

            return false;
        }

        public void Start(CancellationToken cancellationToken)
        {
            _exit = false;

            if (NetworkLibrary.TCP_IP.TCP_UDPUtils.IsUDPPortAvailable(53))
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                new Thread(() => HandleClient(53)).Start();
            }
            else
                LoggerAccessor.LogError("[DNS_UDP] - UDP Port 53 is occupied, UDP server failed to start!");
        }

        public void Stop()
        {
            _cts?.Cancel();
            _exit = true;
            listener?.Dispose();
        }

        public void HandleClient(int listenPort)
        {
            Task serverDNS = Task.Run(() =>
            {
                object _sync = new();
                Task<UdpReceiveResult> CurrentRecvTask = null;
                listener = new UdpClient(listenPort);

                LoggerAccessor.LogInfo($"[DNS_UDP] Server initiated on port: {listenPort}...");

                while (!_cts.Token.IsCancellationRequested)
                {
                    lock (_sync)
                    {
                        if (_exit)
                            break;
                    }

                    try
                    {
                        // use non-blocking recieve
                        if (CurrentRecvTask != null)
                        {
                            if (CurrentRecvTask.IsCompleted)
                            {
                                UdpReceiveResult result = CurrentRecvTask.Result;
                                string clientip = result.RemoteEndPoint.Address.ToString();
                                CurrentRecvTask = null;

                                if (!string.IsNullOrEmpty(clientip) && !IsIPBanned(clientip, result.RemoteEndPoint.Port))
                                {
                                    byte[] ResultBuffer = DNSResolver.ProcRequest(result.Buffer);
                                    if (ResultBuffer != null)
                                        _ = listener.SendAsync(ResultBuffer, ResultBuffer.Length, result.RemoteEndPoint);
                                }
                            }
                            else if (CurrentRecvTask.IsCanceled || CurrentRecvTask.IsFaulted)
                                CurrentRecvTask = null;
                        }

                        if (CurrentRecvTask == null)
                        {
                            CurrentRecvTask = listener.ReceiveAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException is SocketException socketException &&
                            socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted)
                            LoggerAccessor.LogError($"[DNS_UDP] - HandleClient thrown an exception : {ex}");
                        CurrentRecvTask = null;
                    }

                    Thread.Sleep(1);
                }

                LoggerAccessor.LogWarn($"[DNS_UDP] Server on port: {listenPort} was stopped...");

                CurrentRecvTask = null;
            }, _cts.Token);
        }
    }
}
