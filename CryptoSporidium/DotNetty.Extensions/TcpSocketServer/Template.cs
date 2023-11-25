using System.Security.Cryptography.X509Certificates;
using CustomLogger;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;

namespace DotNetty.Extensions
{
    public class Template
    {
        public static bool TCPStarted = false;

        public static void TCPServerStart(int port, bool tls = false) // Only here for future work so I remember lol.
        {
            var server = new TcpSocketServer(port);

            server.OnPipeline(pipeline =>
            {
                //心跳
                pipeline.AddLast(new IdleStateHandler(60, 0, 0));

                //编码解码器
                //pipeline.AddLast(new LengthFieldPrepender(2));
                //pipeline.AddLast(new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                //tls证书

                if (tls)
                {
                    var cert = new X509Certificate2("ThePlaceWherePFXis", "password");
                    pipeline.AddLast(TlsHandler.Server(cert));
                }
            });

            server.OnStart(() =>
            {
                LoggerAccessor.LogInfo($"[TCP] - Server started on port {port}");
            });

            server.OnConnectionConnect(conn =>
            {
                LoggerAccessor.LogInfo("[TCP] - Connect from " + conn.Id);
                LoggerAccessor.LogInfo("[TCP] - Count " + server.GetConnectionCount());
            });

            server.OnConnectionReceive((conn, bytes) =>
            {
                LoggerAccessor.LogInfo("[TCP] - Received " + bytes);
                _ = conn.SendAsync(bytes);
            });

            server.OnConnectionException((conn, ex) =>
            {
                LoggerAccessor.LogError("[TCP] - Thrown an exception : " + ex);
            });

            server.OnConnectionClose(conn =>
            {
                LoggerAccessor.LogInfo("[TCP] - Close from " + conn.Id);
                LoggerAccessor.LogInfo("[TCP] - Count " + server.GetConnectionCount());
            });

            server.OnStop(ex =>
            {
                LoggerAccessor.LogError($"[TCP] - Server was stopped! : " + ex);
                //restart
                //server.StartAsync();
            });

            _ = server.StartAsync();

            TCPStarted = true;
        }
    }
}
