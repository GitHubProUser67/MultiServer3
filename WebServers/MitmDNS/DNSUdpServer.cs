using CustomLogger;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace MitmDNS
{
    public class DNSUdpServer
    {
        public static bool IsStarted = false;

        private Thread thread;
        private volatile bool threadActive;

        private UdpClient listener;

        private readonly int AwaiterTimeoutInMS;
        private readonly int MaxConcurrentListeners;

        public DNSUdpServer(int MaxConcurrentListeners = 10, int awaiterTimeoutInMS = 500)
        {
            this.MaxConcurrentListeners = MaxConcurrentListeners;
            AwaiterTimeoutInMS = awaiterTimeoutInMS;

            Start();
        }

        public void Start()
        {
            if (thread != null)
            {
                LoggerAccessor.LogWarn("[DNS_UDP] - Server already active.");
                return;
            }
            thread = new Thread(Listen);
            thread.Start();
            IsStarted = true;
        }

        public void Stop()
        {
            // stop thread and listener
            threadActive = false;
            if (listener != null) listener.Dispose();

            // wait for thread to finish
            if (thread != null)
            {
                thread.Join();
                thread = null;
            }

            // finish closing listener
            if (listener != null)
                listener = null;

            IsStarted = false;
        }

        private void Listen()
        {
            threadActive = true;

            object _sync = new();

            // start listener
            try
            {
                listener = new UdpClient(53);

                LoggerAccessor.LogInfo($"[DNS_UDP] - Server started on port 53...");

                List<Task> TcpClientTasks = new();
                for (int i = 0; i < MaxConcurrentListeners; i++)
                    TcpClientTasks.Add(listener.ReceiveAsync().ContinueWith((t) => ReceiveClientRequestTask(t)));

                // wait for requests
                while (threadActive)
                {
                    try
                    {
                        lock (_sync)
                        {
                            if (!threadActive)
                                break;
                        }

                        while (TcpClientTasks.Count < MaxConcurrentListeners) //Maximum number of concurrent listeners
                            TcpClientTasks.Add(listener.ReceiveAsync().ContinueWith((t) => ReceiveClientRequestTask(t)));

                        int RemoveAtIndex = Task.WaitAny(TcpClientTasks.ToArray(), AwaiterTimeoutInMS); //Synchronously Waits up to 500ms for any Task completion
                        if (RemoveAtIndex != -1) //Remove the completed task from the list
                            TcpClientTasks.RemoveAt(RemoveAtIndex);
                    }
                    catch (Exception e)
                    {
                        LoggerAccessor.LogError("[DNS_UDP] - An Exception Occured in the listener loop: " + e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("[DNS_UDP] - An Exception Occured while starting the udp client: " + e.Message);
                threadActive = false;
            }
        }

        #region Protected Functions
        protected virtual Task ReceiveClientRequestTask(Task t)
        {
            if (t is not null and Task<UdpReceiveResult>)
            {
                UdpReceiveResult? result = null;

                try
                {
                    result = (t as Task<UdpReceiveResult>)?.Result;
                }
                catch (AggregateException ex)
                {
                    ex.Handle(innerEx =>
                    {
                        if (innerEx is TaskCanceledException)
                            return true; // Indicate that the exception was handled

                        LoggerAccessor.LogError($"[DNS_UDP] - UdpReceiveResult Task thrown an AggregateException: {ex} (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");

                        return false;
                    });
                }

                _ = Task.Run(() => ProcessMessagesFromClient(result));
            }

            return Task.CompletedTask;
        }

        protected virtual void ProcessMessagesFromClient(UdpReceiveResult? result)
        {
            if (!result.HasValue)
                return;
#if DEBUG
            LoggerAccessor.LogInfo($"[DNS_UDP] - Connection received on port 53 (Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
#endif
            UdpReceiveResult resultVal = result.Value;
            byte[] ResultBuffer = DNSResolver.ProcRequest(resultVal.Buffer);
            if (ResultBuffer != null)
            {
                try
                {
                    _ = listener.SendAsync(ResultBuffer, ResultBuffer.Length, resultVal.RemoteEndPoint);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode != 995 && ex.SocketErrorCode != SocketError.ConnectionReset && ex.SocketErrorCode != SocketError.ConnectionAborted && ex.SocketErrorCode != SocketError.ConnectionRefused)
                        LoggerAccessor.LogError($"[DNS_UDP] - ProcessMessagesFromClient - Socket thrown an exception : {ex}");
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[DNS_UDP] - ProcessMessagesFromClient thrown an exception : {ex}");
                }
            }
        }
        #endregion
    }
}
