using Ultralight;
using Ultralight.Listeners;

namespace STOMPServer
{
    public class STOMPProcessor
    {
        public static bool IsStarted = false;

        public static StompServer? server = null;

        public static StompWebsocketListener? wsListener = null;

        public static Task StartSTOMP(string certpath) // The server doesn't work for now on Home, because it uses a geriatric sslv2 conn but at least client not loop
                                                       // on the STOMP server indefinitly.
        {
            try
            {
                wsListener = new StompWebsocketListener("wss://0.0.0.0:10086/", certpath);

                wsListener.OnConnect
                    += stompClient =>
                    {
                        CustomLogger.LoggerAccessor.LogWarn("[STOMP] - A new client has requested a connection.");
                        stompClient.OnMessage += msg =>
                        {
                            CustomLogger.LoggerAccessor.LogInfo("[STOMP] - msg received: {0} {1}", msg.Command, msg.Body);
                        };
                    };

                server = new StompServer(wsListener);
                server.Start();

                IsStarted = true;

                while (IsStarted)
                {
                    
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[STOMP] - Server thrown an exception: {ex}, restarting queue...");

                server?.Stop();
                server = null;
                wsListener = null;
                _ = Task.Run(() => StartSTOMP(certpath));
            }

            return Task.CompletedTask;
        }
    }
}
