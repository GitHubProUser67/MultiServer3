using DotNetty.Common.Internal.Logging;
using PSMultiServer.Addons.CRYPTOSPORIDIUM.BAR;
using PSMultiServer.Addons.Medius.SVO.GAMES;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PSMultiServer.Addons.Medius.SVO
{
    /// <summary>
    /// SVO.
    /// </summary>
    public class SVO
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<SVO>();

        private static volatile bool _keepGoing = true;

        private static bool stopserver = false;

        public static bool svostarted = false;

        // Create and start the HttpListener
        private static HttpListener listener = new HttpListener();

        private static Task? _mainLoop;

        public SVO()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Logger.Info("An exception as occured in SVO : " + ex);

                return;
            }

            return;
        }

        /// <summary>
        /// Start the SVO TCP Server.
        /// </summary>
        public async Task Start()
        {
            try
            {
                svostarted = true;

                Ps_Home.PrepareSVOFolders();

                WipeoutHD.PrepareSVOFolders();

                stopserver = false;
                _keepGoing = true;
                if (_mainLoop != null && !_mainLoop.IsCompleted) return; //Already started
                _mainLoop = loopserver();
            }
            catch (Exception ex)
            {
                Logger.Info($"SVO Server has throw an exception in Main : {ex}");
            }
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public virtual async Task Stop()
        {
            try
            {
                Logger.Warn($"SVO Server stopped - Block requests...");

                stopserver = true;
                listener.Stop();
                _keepGoing = false;

                svostarted = false;
            }
            catch (Exception ex)
            {
                Logger.Error($"SVO Server : an error occured in Stop - {ex}");
            }

            return;
        }

        public Task Tick()
        {
            return Task.CompletedTask;
        }

        private async static Task loopserver()
        {
            string[] portNumbers = { "10060", "10061" };

            foreach (string portNumber in portNumbers)
            {
                string httpprefix = "http";

                if (portNumber == "10061")
                {
                    httpprefix = "https";
                }

                listener.Prefixes.Add($"{httpprefix}://*:{portNumber}/");
            }

            Logger.Info("SVO Server started on *:10060 - *:10061 - Listening for requests...");

            listener.Start();

            while (_keepGoing)
            {
                try
                {
                    //GetContextAsync() returns when a new request come in
                    var context = await listener.GetContextAsync();

                    lock (listener)
                    {
                        if (_keepGoing)
                        {
                            Task.Run(() => ProcessRequest(context));
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is HttpListenerException)
                    {
                        if (stopserver)
                        {
                            stopserver = false;
                            return;
                        }
                        else
                        {
                            _keepGoing = false;

                            Logger.Error($"FATAL ERROR OCCURED in SVO Listener - {ex} - Trying to listen for requests again...");

                            if (!listener.IsListening)
                            {
                                _keepGoing = true;
                                listener.Start();
                            }
                            else
                            {
                                _keepGoing = true;
                            }
                        }
                    }
                }
            }

            return;
        }

        public async static Task ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string userAgent = context.Request.Headers["User-Agent"];

                if (userAgent == null || userAgent == "") // Medius not always expose a userAgent
                {
                    userAgent = "Medius Client";
                }

                Logger.Info($"SVO Server : Received request from : {userAgent} : {context.Request.Url.AbsolutePath}");

                if (context.Request.Url.AbsolutePath == "/" || context.Request.Url.AbsolutePath == @"\")
                {
                    Logger.Warn($"SVO Server : {userAgent} - Requested server root, we forbid!");

                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                    byte[] buffer = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                    context.Response.ContentLength64 = buffer.Length;

                    Stream ros = context.Response.OutputStream;

                    if (ros.CanWrite)
                    {
                        try
                        {
                            ros.Write(buffer, 0, buffer.Length);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                        }
                    }
                    else
                    {
                        Logger.Warn("Client Disconnected early");
                    }

                    ros.Dispose();

                    context.Response.Close();

                    GC.Collect();
                }
                else if (context.Request.Url.AbsolutePath.Contains("/HUBPS3_SVML") || context.Request.Url.AbsolutePath == "/dataloaderweb/queue")
                {
                    Ps_Home.Home_SVO(context, userAgent);
                }
                else if (context.Request.Url.AbsolutePath.Contains("/wox_ws"))
                {
                    WipeoutHD.WipeoutHD_SVO(context, userAgent);
                }
                else if (context.Request.Url.AbsolutePath.Contains("/motorstorm3ps3_xml"))
                {
                    MotorstormApocalypse.MotorstormApocalypse_SVO(context, userAgent);
                }
                else
                {
                    Logger.Info($"SVO Server : {userAgent} - Requested a SVO method that doesn't exist.");

                    context.Response.StatusCode = 404;

                    byte[] buffer = Encoding.UTF8.GetBytes(PreMadeWebPages.filenotfound);
                    context.Response.ContentLength64 = buffer.Length;

                    Stream ros = context.Response.OutputStream;

                    if (ros.CanWrite)
                    {
                        try
                        {
                            ros.Write(buffer, 0, buffer.Length);
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                        }
                    }
                    else
                    {
                        Logger.Warn("Client Disconnected early");
                    }

                    ros.Dispose();

                    context.Response.Close();

                    GC.Collect();
                }

                return;
            }
            catch (Exception ex)
            {
                Logger.Error($"SVO Server : an error occured in ProcessRequest - {ex}");

                context.Response.Close();
                GC.Collect();

                return;
            }
        }
        public static string CalcuateSVOMac(string clientSVOMac)
        {
            if (string.IsNullOrEmpty(clientSVOMac))
                return null;


            if (clientSVOMac.Length != 32)
                return null;

            //SVO SpeaksId internal SCE-RT
            string speaksId = Misc.Base64Decode("c3A5Y2swMzQ4c2xkMDAwMDAwMDAwMDAwMDAwMDAw");

            //Get SVOMac from client and combine with speaksId together for new MD5, converting to a byte array for MD5 rehashing
            byte[] HashedSVOMac = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(clientSVOMac + speaksId));

            if (HashedSVOMac.Length != 16)
                return null;

            // Create the Cipher RSA_RC4_40_MD5 value by concatenating the encoded key and the MD5 hash
            string cipher = $"{BitConverter.ToString(HashedSVOMac).Replace("-", string.Empty).ToLower()}";

            return cipher;
        }
    }
}
