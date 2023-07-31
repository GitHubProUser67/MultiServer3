using System.Net;
using System.Text;
using System.Security.Cryptography;

namespace PSMultiServer.Addons.Medius.SVO
{
    public class SVO_OTG
    {
        private static volatile bool _keepGoing = true;

        private static bool stopserver = false;

        public static bool svostarted = false;

        // Create and start the HttpListener
        private static HttpListener listener = new HttpListener();

        private static Task? _mainLoop;

        public static void SVOstart()
        {
            try
            {
                svostarted = true;

                GAMES.Ps_Home.PrepareSVOFolders();

                stopserver = false;
                _keepGoing = true;
                if (_mainLoop != null && !_mainLoop.IsCompleted) return; //Already started
                _mainLoop = loopserver();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SVO Server has throw an exception in Main : {ex}");
            }
        }
        public static void SVOstop()
        {
            stopserver = true;
            listener.Stop();
            _keepGoing = false;

            svostarted = false;

            return;
        }

        private async static Task loopserver()
        {
            listener.Prefixes.Add("http://*:10060/");

            Console.WriteLine("SVO Server started on *:10060 - Listening for requests...");

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

                            Console.WriteLine($"FATAL ERROR OCCURED in SVO Listener - {ex} - Trying to listen for requests again...");

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

                Console.WriteLine($"SVO Server : Received request from : {userAgent} : {context.Request.Url.AbsolutePath}");

                if (context.Request.Url.AbsolutePath == "/" || context.Request.Url.AbsolutePath == @"\")
                {
                    Console.WriteLine($"SVO Server : {userAgent} - Requested server root, we forbid!");

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
                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Client Disconnected early");
                    }

                    ros.Dispose();

                    context.Response.Close();

                    GC.Collect();
                }
                else if (context.Request.Url.AbsolutePath.Contains("HUBPS3_SVML") || context.Request.Url.AbsolutePath == "/dataloaderweb/queue")
                {
                    GAMES.Ps_Home.Home_SVO(context, userAgent);
                }
                else
                {
                    Console.WriteLine($"SVO Server : {userAgent} - Requested a SVO method that doesn't exist.");

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
                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Client Disconnected early");
                    }

                    ros.Dispose();

                    context.Response.Close();

                    GC.Collect();
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SVO Server : an error occured in ProcessRequest - {ex}");

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
