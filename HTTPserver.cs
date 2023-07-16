using PSMultiServer.SRC_Addons.HOME;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace PSMultiServer
{
    public class HTTPserver
    {
        private static volatile bool _keepGoing = true;

        private static bool stopserver = false;

        public static bool httpstarted = false;

        private static string httpkey = "";

        private static string phpver = "";

        private static Dictionary<string, List<WebSocket>> rooms = new Dictionary<string, List<WebSocket>>();

        // Create and start the HttpListener
        private static HttpListener listener = new HttpListener();

        private static Task? _mainLoop;

        public static void HTTPstart(string phpverlocal, string _HTTPKEY, int port)
        {
            try
            {
                httpstarted = true;

                httpkey = _HTTPKEY;

                phpver = phpverlocal;

                GENERALHomeClass.PrepareHomeFoldersNFiles();

                if (httpkey == "")
                {
                    Console.WriteLine("No HTTP key so http encryption is disabled.");
                }
                else if (httpkey.Length < 20)
                {
                    Console.WriteLine("HTTP key is less than 20 characters, so encryption is disabled.");

                    httpkey = "";
                }

                stopserver = false;

                _keepGoing = true;
                if (_mainLoop != null && !_mainLoop.IsCompleted) return; //Already started
                _mainLoop = loopserver(port);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP Server has throw an exception in Main : {ex}");
            }
        }
        public static void HTTPstop()
        {
            stopserver = true;

            listener.Stop();
            _keepGoing = false;

            httpstarted = false;

            return;
        }

        private async static Task loopserver(int port)
        {
            listener.Prefixes.Add($"http://*:{port}/");

            Console.WriteLine($"HTTP Server started on *:{port} - Listening for requests...");

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
                        _keepGoing = false;

                        if (stopserver)
                        {
                            RemoveAllPrefixes(listener);
                            stopserver = false;
                        }
                        else
                        {
                            _keepGoing = false;

                            Console.WriteLine($"HTTP Server : FATAL ERROR OCCURED - {ex} - Trying to listen for requests again...");

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
                bool specialpage = false;

                string userAgent = context.Request.Headers["User-Agent"];

                if (userAgent != null && userAgent.Contains("CellOS"))
                {
                    Console.WriteLine($"HTTP Server : Received request from : {userAgent} : {context.Request.Url.AbsolutePath}");

                    if (context.Request.IsWebSocketRequest)
                    {
                        Task.Run(() => ProcessWebSocketRequest(context, userAgent));
                    }
                    else
                    {
                        if (context.Request.Url.AbsolutePath == "/" || context.Request.Url.AbsolutePath == @"\")
                        {
                            specialpage = true;

                            Console.WriteLine($"HTTP Server : {userAgent} - Requested server root, we forbid!");

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
                        else if (context.Request.Url.AbsolutePath == "/index.php")
                        {
                            specialpage = true;

                            if (context.Request.Headers["Host"] != null)
                            {
                                if (context.Request.Headers["Host"] == "sonyhome.thqsandbox.com") // THQ Home servers
                                {
                                    Task.Run(() => THQservices.ProcessHomeTHQRequest(context, userAgent));
                                }
                                else
                                {
                                    Console.WriteLine($"HTTP Server : {userAgent} tried to access a sub-server via /index.php server, but it's not correct so we forbid.");

                                    // Return a not allowed response
                                    byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                    if (context.Response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                            context.Response.ContentLength64 = notAllowed.Length;
                                            context.Response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                            context.Response.OutputStream.Close();
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

                                    context.Response.Close();
                                    GC.Collect();
                                }
                            }
                            else
                            {
                                Console.WriteLine($"HTTP Server : {userAgent} tried to access /index.php without a host, so we forbid.");

                                // Return a not allowed response
                                byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                if (context.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                        context.Response.ContentLength64 = notAllowed.Length;
                                        context.Response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                        context.Response.OutputStream.Close();
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

                                context.Response.Close();
                                GC.Collect();
                            }
                        }
                        else if (context.Request.Url.AbsolutePath.Contains("/ohs"))
                        {
                            specialpage = true;

                            Task.Run(() => OHSservices.ProcessRequest(context, userAgent));
                        }
                        else if (context.Request.Headers["Host"] != null && (context.Request.Headers["Host"] == "away.veemee.com" || context.Request.Headers["Host"] == "home.veemee.com") && context.Request.Url.AbsolutePath.Contains(".php"))
                        {
                            specialpage = true;

                            Task.Run(() => VEEMEEservices.ProcessRequest(context, userAgent));
                        }

                        if (!specialpage)
                        {
                            string page = Directory.GetCurrentDirectory() + "/wwwroot" + context.Request.Url.LocalPath;

                            if (File.Exists(page))
                            {
                                ContextProcess wwwroot = new ContextProcess();

                                Task.Run(() => wwwroot.Processwwwroot(context, page, phpver, httpkey, userAgent));
                            }
                            else
                            {
                                Console.WriteLine($"HTTP Server : {userAgent} - Requested a file that doesn't exist : {page}");

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
                        }
                    }
                }
                else
                {
                    string clientDNS = "";

                    Uri uri = new Uri("http://" + context.Request.UserHostName.ToString());

                    string clientIP = uri.Host.ToString();

                    IPHostEntry entry = Dns.GetHostEntry(clientIP);

                    if (entry != null)
                    {
                        clientDNS = entry.HostName;
                    }
                    else
                    {
                        clientDNS = clientIP;
                    }

                    if (userAgent == null)
                    {
                        userAgent = $"{clientDNS}";
                    }
                    else
                    {
                        userAgent = userAgent + $" - {clientDNS}";
                    }

                    Console.WriteLine($"HTTP Server : Received request from : {userAgent} : {context.Request.Url.AbsolutePath}");

                    if (context.Request.IsWebSocketRequest)
                    {
                        Task.Run(() => ProcessWebSocketRequest(context, userAgent));
                    }
                    else
                    {
                        if (context.Request.Url.AbsolutePath == "/" || context.Request.Url.AbsolutePath == @"\")
                        {
                            specialpage = true;

                            Console.WriteLine($"HTTP Server : {userAgent} - Requested server root, we forbid!");

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

                        if (!specialpage)
                        {
                            string page = Directory.GetCurrentDirectory() + "/wwwroot" + context.Request.Url.LocalPath;

                            if (File.Exists(page))
                            {
                                ContextProcess wwwroot = new ContextProcess();

                                Task.Run(() => wwwroot.Processwwwroot(context, page, phpver, httpkey, userAgent));
                            }
                            else
                            {
                                Console.WriteLine($"HTTP Server : {userAgent} - Requested a file that doesn't exist : {page}");

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
                        }
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP Server : an error occured in ProcessRequest - {ex}");

                context.Response.Close();
                GC.Collect();

                return;
            }
        }

        private static async Task ProcessWebSocketRequest(HttpListenerContext context, string userAgent)
        {
            try
            {
                HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);

                WebSocket webSocket = webSocketContext.WebSocket;
                // Perform any initialization or handling specific to your WebSocket server

                // Get the client IP address
                string clientIp = context.Request.RemoteEndPoint.Address.ToString();

                // Check if the room for the client's IP address exists
                if (!rooms.ContainsKey(clientIp))
                {
                    rooms[clientIp] = new List<WebSocket>();
                }

                // Add the new WebSocket to the client's room
                rooms[clientIp].Add(webSocket);

                byte[] receiveBuffer = new byte[1024];

                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count);
                        // Handle the received message

                        // Broadcast the message to all connected clients in the room
                        await BroadcastMessageAsync(clientIp, receivedMessage);
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        // Handle the WebSocket connection close request

                        // Remove the WebSocket from the client's room
                        rooms[clientIp].Remove(webSocket);

                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                }

                // Clean up the room if it's empty
                if (rooms[clientIp].Count == 0)
                {
                    rooms.Remove(clientIp);
                }

                Console.WriteLine($"HTTP Server: {userAgent} closed the websocket connection.");

                context.Response.Close();
                GC.Collect();

                return;
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine($"HTTP Server: {userAgent} tried to access a chat channel that doesn't exist anymore! Closing.");

                context.Response.Close();
                GC.Collect();

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP Server: An error occurred in ProcessWebSocketRequest - {ex}");

                context.Response.Close();
                GC.Collect();

                return;
            }
        }

        private static async Task BroadcastMessageAsync(string clientIp, string message)
        {
            try
            {
                if (rooms.ContainsKey(clientIp))
                {
                    List<WebSocket> roomClients = rooms[clientIp];

                    for (int i = roomClients.Count - 1; i >= 0; i--)
                    {
                        WebSocket clientWebSocket = roomClients[i];

                        try
                        {
                            if (clientWebSocket.State == WebSocketState.Open)
                            {
                                await clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            else if (clientWebSocket.State == WebSocketState.CloseReceived || clientWebSocket.State == WebSocketState.CloseSent)
                            {
                                roomClients.RemoveAt(i);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error broadcasting message to client - {ex}");
                            roomClients.RemoveAt(i);
                        }
                    }

                    // Clean up the room if it's empty
                    if (roomClients.Count == 0)
                    {
                        rooms.Remove(clientIp);
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP Server: An error occurred in BroadcastMessageAsync - {ex}");

                return;
            }
        }

        public static async Task WriteFile(HttpListenerContext ctx, string path)
        {
            var response = ctx.Response;
            using (FileStream fs = File.OpenRead(path))
            {
                string filename = Path.GetFileName(path);

                response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                byte[] buffer = new byte[64 * 1024];
                int read;
                using (BinaryWriter bw = new BinaryWriter(response.OutputStream))
                {
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, read);
                        bw.Flush();
                    }

                    bw.Close();
                    bw.Dispose();
                }

                Stream ros = ctx.Response.OutputStream;

                if (ros.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = fs.Length;
                        response.SendChunked = false;
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.StatusDescription = "OK";
                        response.Close();
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
            }

            return;
        }

        public static byte[] ReadLine(BinaryReader reader)
        {
            using (MemoryStream lineStream = new MemoryStream())
            {
                byte prevByte = 0;
                byte currentByte;

                while ((currentByte = reader.ReadByte()) != -1)
                {
                    if (prevByte == 13 && currentByte == 10)
                    {
                        lineStream.SetLength(lineStream.Length - 1); // Remove the trailing \r\n
                        return lineStream.ToArray();
                    }

                    lineStream.WriteByte(currentByte);
                    prevByte = currentByte;
                }

                if (lineStream.Length > 0)
                    return lineStream.ToArray();

                return null;
            }
        }

        public static bool ByteArrayStartsWith(byte[] byteArray, byte[] prefix)
        {
            if (byteArray.Length < prefix.Length)
                return false;

            for (int i = 0; i < prefix.Length; i++)
            {
                if (byteArray[i] != prefix[i])
                    return false;
            }

            return true;
        }

        private static bool RemoveAllPrefixes(HttpListener listener)
        {
            // Get the prefixes that the Web server is listening to.
            HttpListenerPrefixCollection prefixes = listener.Prefixes;
            try
            {
                prefixes.Clear();
            }
            // If the operation failed, return false.
            catch
            {
                return false;
            }

            return true;
        }
    }
}
