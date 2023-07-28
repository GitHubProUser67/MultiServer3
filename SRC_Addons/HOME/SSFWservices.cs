using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;

namespace PSMultiServer.SRC_Addons.HOME
{
    public class SSFWServices
    {
        private static string ssfwkey = "";

        private static string SSFWminibase = "[]";

        private static volatile bool _keepGoing = true;

        private static bool stopserver = false;

        private static bool ssfwcrosssave = false;

        public static bool ssfwstarted = false;

        // Create and start the HttpListener
        private static HttpListener listener = new HttpListener();

        private static Task? _mainLoop;
        public static void SSFWstart(string ssfwkeylocal, string SSFWminibaselocal, bool ssfwcrosssavelocal)
        {
            try
            {
                ssfwcrosssave = ssfwcrosssavelocal;

                ssfwkey = ssfwkeylocal;

                SSFWminibase = SSFWminibaselocal;

                ssfwstarted = true;

                if (SSFWminibase == "")
                {
                    SSFWminibase = "[]";
                }

                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/wwwssfwroot/"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/wwwssfwroot/");
                }

                if (!Directory.Exists(Directory.GetCurrentDirectory() + "/loginformNtemplates/SSFW_Accounts/"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/loginformNtemplates/SSFW_Accounts/");
                }

                if (ssfwkey == "")
                {
                    Console.WriteLine("No SSFW key so ssfw encryption is disabled.");
                }
                else if (ssfwkey.Length < 20)
                {
                    Console.WriteLine("SSFW key is less than 20 characters, so encryption is disabled.");

                    ssfwkey = "";
                }

                stopserver = false;
                _keepGoing = true;
                if (_mainLoop != null && !_mainLoop.IsCompleted) return; //Already started
                _mainLoop = loopserver();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server has throw an exception in Main : {ex}");
            }
        }
        public static void SSFWstop()
        {
            Console.WriteLine($"SSFW Server stopped - Block requests...");

            stopserver = true;
            listener.Stop();
            _keepGoing = false;

            ssfwstarted = false;

            return;
        }

        private async static Task loopserver()
        {
            listener.Prefixes.Add("http://*:10443/");
            listener.Start();

            Console.WriteLine($"SSFW Server started on *:10443 - Listening for requests...");

            while (true)
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

                            Console.WriteLine($"FATAL ERROR OCCURED in SSFW Listener - {ex} - Trying to listen for requests again...");

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
        }
        private static async Task ProcessRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (context.Request.Headers["User-Agent"].Contains("CellOS"))
                {
                    // Extract the HTTP method and the relative path
                    string httpMethod = request.HttpMethod;
                    string url = request.Url.LocalPath;

                    Console.WriteLine($"SSFW : Received {httpMethod} request for {url}");

                    // Split the URL into segments
                    string[] segments = url.Trim('/').Split('/');

                    // Combine the folder segments into a directory path
                    string directoryPath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwssfwroot/", string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                    // Process the request based on the HTTP method
                    string filePath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwssfwroot/", url.Substring(1));

                    switch (httpMethod)
                    {
                        case "PUT":

                            try
                            {
                                if (request.Headers["X-Home-Session-Id"] != null)
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        request.InputStream.CopyTo(ms);

                                        // Reset the memory stream position to the beginning
                                        ms.Position = 0;

                                        // Find the number of bytes in the stream
                                        int contentLength = (int)ms.Length;

                                        // Create a byte array
                                        byte[] buffer = new byte[contentLength];

                                        // Read the contents of the memory stream into the byte array
                                        ms.Read(buffer, 0, contentLength);

                                        try
                                        {
                                            if (!Directory.Exists(directoryPath))
                                            {
                                                Directory.CreateDirectory(directoryPath);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the PUT request and creating the directory : {ex}");

                                            // Return an internal server error response
                                            byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                    response.ContentLength64 = InternnalError.Length;
                                                    response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }

                                        try
                                        {
                                            if (request.ContentType == "image/jpeg")
                                            {
                                                using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.jpeg", FileMode.Create))
                                                {
                                                    if (ssfwkey != "")
                                                    {
                                                        byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                        byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), buffer));

                                                        fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }
                                                    else
                                                    {
                                                        fs.Write(buffer, 0, contentLength);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }

                                                    Console.WriteLine($"File {$"./wwwssfwroot{context.Request.Url.AbsolutePath}.jpeg"} has been uploaded to SSFW");
                                                }
                                            }
                                            if (request.ContentType == "application/json")
                                            {
                                                using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw", FileMode.Create))
                                                {
                                                    if (ssfwkey != "")
                                                    {
                                                        byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                        byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), buffer));

                                                        fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }
                                                    else
                                                    {
                                                        fs.Write(buffer, 0, contentLength);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }

                                                    Console.WriteLine($"File {$"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw"} has been uploaded to SSFW");
                                                }
                                            }
                                            else
                                            {
                                                using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.bin", FileMode.Create))
                                                {
                                                    if (ssfwkey != "")
                                                    {
                                                        byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                        byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), buffer));

                                                        fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }
                                                    else
                                                    {
                                                        fs.Write(buffer, 0, contentLength);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }

                                                    Console.WriteLine($"File {$"./wwwssfwroot{context.Request.Url.AbsolutePath}.bin"} has been uploaded to SSFW");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"SSFW Server : Server has throw an exception in ProcessRequest while processing the PUT request and creating the file : {ex}");

                                            // Return an internal server error response
                                            byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                    response.ContentLength64 = InternnalError.Length;
                                                    response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                // Return a success response
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.ContentLength64 = contentLength;
                                                response.OutputStream.Write(buffer, 0, contentLength);
                                                response.OutputStream.Close();
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

                                        ms.Dispose();
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"SSFW Server : Host has issued a PUT command without Home specific cookie. We forbid");

                                    // Return a not allowed response
                                    byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                    if (response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                                            response.ContentLength64 = notAllowed.Length;
                                            response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                            response.OutputStream.Close();
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
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the PUT request : {ex}");

                                // Return an internal server error response
                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                if (response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentLength64 = InternnalError.Length;
                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                        response.OutputStream.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client Disconnected early");
                                }
                            }

                            break;

                        case "POST":

                            try
                            {
                                if (context.Request.Url.AbsolutePath == "/bb88aea9-6bf8-4201-a6ff-5d1f8da0dd37/login/token/psn")
                                {
                                    if (request.Headers["X-HomeClientVersion"] != null && request.Headers["general-secret"] != null)
                                    {
                                        try
                                        {
                                            string resultString = "Default";

                                            string sessionid = "";

                                            int logoncount = 1;

                                            string homeClientVersion = request.Headers["X-HomeClientVersion"].Replace(".", "");

                                            using (MemoryStream ms = new MemoryStream())
                                            {
                                                request.InputStream.CopyTo(ms);

                                                // Reset the memory stream position to the beginning
                                                ms.Position = 0;

                                                // Find the number of bytes in the stream
                                                int contentLength = (int)ms.Length;

                                                // Create a byte array
                                                byte[] bufferwrite = new byte[contentLength];

                                                // Read the contents of the memory stream into the byte array
                                                ms.Read(bufferwrite, 0, contentLength);

                                                try
                                                {
                                                    // Extract the desired portion of the binary data
                                                    byte[] extractedData = new byte[0x63 - 0x54 + 1];

                                                    // Copy it
                                                    Array.Copy(bufferwrite, 0x54, extractedData, 0, extractedData.Length);

                                                    // Convert 0x00 bytes to 0x48 so FileSystem can support it
                                                    for (int i = 0; i < extractedData.Length; i++)
                                                    {
                                                        if (extractedData[i] == 0x00)
                                                        {
                                                            extractedData[i] = 0x48;
                                                        }
                                                    }

                                                    if (await Task.Run(() => Misc.FindbyteSequence(bufferwrite, new byte[] { 0x52, 0x50, 0x43, 0x4E })) && !ssfwcrosssave)
                                                    {
                                                        Console.WriteLine($"User {Encoding.ASCII.GetString(extractedData).Replace("H", "")} logged in and is on RPCN");

                                                        // Convert the modified data to a string
                                                        resultString = Encoding.ASCII.GetString(extractedData) + "RPCN" + homeClientVersion;

                                                        // Calculate the MD5 hash of the result
                                                        using (MD5 md5 = MD5.Create())
                                                        {
                                                            string salt = "";

                                                            if (request.Headers["x-signature"] != null)
                                                            {
                                                                salt = request.Headers["general-secret"] + request.Headers["x-signature"] + request.Headers["X-HomeClientVersion"];
                                                            }
                                                            else
                                                            {
                                                                salt = request.Headers["general-secret"] + request.Headers["X-HomeClientVersion"];
                                                            }

                                                            byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(resultString + salt));
                                                            string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                                                            // Trim the hash to a specific length
                                                            hash = hash.Substring(0, 10);

                                                            // Append the trimmed hash to the result
                                                            resultString += hash;

                                                            sessionid = ssfwgenerateguid(hash, resultString);

                                                            md5.Dispose();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine($"User {Encoding.ASCII.GetString(extractedData).Replace("H", "")} logged in and is on PSN");

                                                        // Convert the modified data to a string
                                                        resultString = Encoding.ASCII.GetString(extractedData) + homeClientVersion;

                                                        // Calculate the MD5 hash of the result
                                                        using (MD5 md5 = MD5.Create())
                                                        {
                                                            string salt = "";

                                                            if (request.Headers["x-signature"] != null)
                                                            {
                                                                salt = request.Headers["general-secret"] + request.Headers["x-signature"] + request.Headers["X-HomeClientVersion"];
                                                            }
                                                            else
                                                            {
                                                                salt = request.Headers["general-secret"] + request.Headers["X-HomeClientVersion"];
                                                            }

                                                            byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(resultString + salt));
                                                            string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                                                            // Trim the hash to a specific length
                                                            hash = hash.Substring(0, 14);

                                                            // Append the trimmed hash to the result
                                                            resultString += hash;

                                                            sessionid = ssfwgenerateguid(hash, resultString);

                                                            md5.Dispose();
                                                        }
                                                    }

                                                    string userprofilefile = Directory.GetCurrentDirectory() + $"/loginformNtemplates/SSFW_Accounts/{sessionid}.ssfw";

                                                    if (File.Exists(userprofilefile))
                                                    {
                                                        string tempcontent = "";

                                                        byte[] firstNineBytes = new byte[9];

                                                        using (FileStream fileStream = new FileStream(userprofilefile, FileMode.Open, FileAccess.Read))
                                                        {
                                                            fileStream.Read(firstNineBytes, 0, 9);
                                                            fileStream.Close();
                                                        }

                                                        if (ssfwkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                                                        {
                                                            byte[] src = File.ReadAllBytes(userprofilefile);
                                                            byte[] dst = new byte[src.Length - 9];

                                                            Array.Copy(src, 9, dst, 0, dst.Length);

                                                            tempcontent = Encoding.UTF8.GetString(CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                                                        CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey)));
                                                        }
                                                        else
                                                        {
                                                            tempcontent = File.ReadAllText(userprofilefile);
                                                        }

                                                        // Parsing JSON data to SSFWUserData object
                                                        SSFWUserData userData = JsonConvert.DeserializeObject<SSFWUserData>(tempcontent);

                                                        if (userData != null)
                                                        {
                                                            // Modifying the object if needed
                                                            userData.LogonCount += 1;

                                                            logoncount = userData.LogonCount;

                                                            Console.WriteLine($"SSFW Server : {Encoding.ASCII.GetString(extractedData)} - Session ID : {userData.Username}");
                                                            Console.WriteLine($"SSFW Server : {Encoding.ASCII.GetString(extractedData)} - LogonCount : {userData.LogonCount}");
                                                            Console.WriteLine($"SSFW Server : {Encoding.ASCII.GetString(extractedData)} - IGA : {userData.IGA}");

                                                            using (FileStream fs = new FileStream(userprofilefile, FileMode.Create))
                                                            {
                                                                byte[] dataforoutput = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userData));

                                                                if (ssfwkey != "")
                                                                {
                                                                    byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                                    byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), dataforoutput));

                                                                    fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                                    fs.Flush();
                                                                    fs.Dispose();
                                                                }
                                                                else
                                                                {
                                                                    fs.Write(dataforoutput, 0, dataforoutput.Length);
                                                                    fs.Flush();
                                                                    fs.Dispose();
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        string tempcontent = $"{{\"Username\":\"{sessionid}\",\"LogonCount\":{logoncount},\"IGA\":0}}";

                                                        // Parsing JSON data to SSFWUserData object
                                                        SSFWUserData userData = JsonConvert.DeserializeObject<SSFWUserData>(tempcontent);

                                                        if (userData != null)
                                                        {
                                                            Console.WriteLine($"SSFW Server : Account Created - {Encoding.ASCII.GetString(extractedData)} - Session ID : {userData.Username}");
                                                            Console.WriteLine($"SSFW Server : Account Created - {Encoding.ASCII.GetString(extractedData)} - LogonCount : {userData.LogonCount}");
                                                            Console.WriteLine($"SSFW Server : Account Created - {Encoding.ASCII.GetString(extractedData)} - IGA : {userData.IGA}");

                                                            using (FileStream fs = new FileStream(userprofilefile, FileMode.Create))
                                                            {
                                                                byte[] dataforoutput = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(userData));

                                                                if (ssfwkey != "")
                                                                {
                                                                    byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                                    byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), dataforoutput));

                                                                    fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                                    fs.Flush();
                                                                    fs.Dispose();
                                                                }
                                                                else
                                                                {
                                                                    fs.Write(dataforoutput, 0, dataforoutput.Length);
                                                                    fs.Flush();
                                                                    fs.Dispose();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the PSN Ticket : {ex}");

                                                    // Return an internal server error response
                                                    byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                            response.ContentLength64 = InternnalError.Length;
                                                            response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }

                                                try
                                                {
                                                    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/wwwssfwroot/LayoutService/cprod/person/" + resultString))
                                                    {
                                                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"/wwwssfwroot/LayoutService/cprod/person/" + resultString);
                                                    }

                                                    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/wwwssfwroot/RewardsService/cprod/rewards/" + resultString))
                                                    {
                                                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/wwwssfwroot/RewardsService/cprod/rewards/" + resultString);
                                                    }

                                                    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/wwwssfwroot/RewardsService/trunks-cprod/trunks/"))
                                                    {
                                                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/wwwssfwroot/RewardsService/trunks-cprod/trunks/");
                                                    }

                                                    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/wwwssfwroot/AvatarLayoutService/cprod/" + resultString))
                                                    {
                                                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/wwwssfwroot/AvatarLayoutService/cprod/" + resultString);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Directories creation/check : {ex}");

                                                    // Return an internal server error response
                                                    byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                            response.ContentLength64 = InternnalError.Length;
                                                            response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }

                                                try
                                                {
                                                    if (!File.Exists(Directory.GetCurrentDirectory() + "/wwwssfwroot/LayoutService/cprod/person/" + resultString + "/mylayout.ssfw"))
                                                    {
                                                        using (FileStream fs = new FileStream($"./wwwssfwroot/LayoutService/cprod/person/" + resultString + "/mylayout.ssfw", FileMode.Create))
                                                        {
                                                            byte[] dataforlayout = Encoding.UTF8.GetBytes("[{\"00000000-00000000-00000000-00000004\":{\"version\":3,\"wallpaper\":2,\"furniture\":[{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000010\",\"instanceId\":\"4874595585\",\"itemId\":0,\"positionX\":-4.287144660949707,\"positionY\":2.9999580383300781,\"positionZ\":-2.3795166015625,\"rotationX\":2.6903744583250955E-06,\"rotationY\":0.70767402648925781,\"rotationZ\":-2.1571504476014525E-06,\"rotationW\":0.70653915405273438,\"time\":1686384673},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000002\",\"instanceId\":\"4874595586\",\"itemId\":1,\"positionX\":-3.7360246181488037,\"positionY\":2.9999902248382568,\"positionZ\":-0.93418246507644653,\"rotationX\":1.5251726836140733E-05,\"rotationY\":0.92014747858047485,\"rotationZ\":-0.00032892703893594444,\"rotationW\":0.39157184958457947,\"time\":1686384699},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000002\",\"instanceId\":\"4874595587\",\"itemId\":2,\"positionX\":-4.2762022018432617,\"positionY\":2.9999568462371826,\"positionZ\":-4.1523990631103516,\"rotationX\":1.4554960570123399E-09,\"rotationY\":0.4747755229473114,\"rotationZ\":-1.4769816480963982E-08,\"rotationW\":0.88010692596435547,\"time\":1686384723},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000002\",\"instanceId\":\"4874595588\",\"itemId\":3,\"positionX\":-2.8646721839904785,\"positionY\":2.9999570846557617,\"positionZ\":-3.0560495853424072,\"rotationX\":0.00010053320875158533,\"rotationY\":-0.26336261630058289,\"rotationZ\":-3.8589099858654663E-05,\"rotationW\":0.96469688415527344,\"time\":1686384751},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000001\",\"instanceId\":\"4874595589\",\"itemId\":4,\"positionX\":3.9096813201904297,\"positionY\":2.9995136260986328,\"positionZ\":-4.2813630104064941,\"rotationX\":4.3287433072691783E-05,\"rotationY\":-0.53099715709686279,\"rotationZ\":-3.9187150832731277E-05,\"rotationW\":0.8473736047744751,\"time\":1686384774},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000004\",\"instanceId\":\"4874595590\",\"itemId\":5,\"positionX\":1.8418744802474976,\"positionY\":3.0001647472381592,\"positionZ\":-3.2746503353118896,\"rotationX\":-5.4990476201055571E-05,\"rotationY\":-0.53177982568740845,\"rotationZ\":-1.335094293608563E-05,\"rotationW\":0.84688264131546021,\"time\":1686384795},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000008\",\"instanceId\":\"4874595591\",\"itemId\":6,\"positionX\":3.4726400375366211,\"positionY\":3.0000433921813965,\"positionZ\":4.783566951751709,\"rotationX\":6.1347323935478926E-05,\"rotationY\":0.99999260902404785,\"rotationZ\":-1.7070769899873994E-05,\"rotationW\":0.0038405421655625105,\"time\":1686384822},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000008\",\"instanceId\":\"4874595592\",\"itemId\":7,\"positionX\":3.4952659606933594,\"positionY\":3.0000007152557373,\"positionZ\":0.2776024341583252,\"rotationX\":-1.2929040167364292E-05,\"rotationY\":-0.0061355167999863625,\"rotationZ\":-4.4378830352798104E-05,\"rotationW\":0.999981164932251,\"time\":1686384834},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000001\",\"instanceId\":\"4874595593\",\"itemId\":8,\"positionX\":1.3067165613174438,\"positionY\":2.9994897842407227,\"positionZ\":2.546649694442749,\"rotationX\":2.8451957405195571E-05,\"rotationY\":0.70562022924423218,\"rotationZ\":-8.0827621786738746E-06,\"rotationW\":0.70859026908874512,\"time\":1686384862},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000003\",\"instanceId\":\"4874595594\",\"itemId\":9,\"positionX\":3.4803681373596191,\"positionY\":2.9999568462371826,\"positionZ\":2.5385856628417969,\"rotationX\":3.1659130428352E-08,\"rotationY\":-0.70712763071060181,\"rotationZ\":8.1442820487609424E-08,\"rotationW\":0.70708584785461426,\"time\":1686384884},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000009\",\"instanceId\":\"4874595595\",\"itemId\":10,\"positionX\":-3.5043892860412598,\"positionY\":2.9999568462371826,\"positionZ\":-9.527653694152832,\"rotationX\":-1.7184934222314041E-06,\"rotationY\":0.00023035785125102848,\"rotationZ\":2.5227839728358958E-07,\"rotationW\":0.99999994039535522,\"time\":1686384912},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000009\",\"instanceId\":\"4874595596\",\"itemId\":11,\"positionX\":3.6248698234558105,\"positionY\":2.9999566078186035,\"positionZ\":-9.5347089767456055,\"rotationX\":-2.1324558474589139E-07,\"rotationY\":2.0361580027383752E-05,\"rotationZ\":-4.7822368287597783E-08,\"rotationW\":1,\"time\":1686384931},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000005\",\"instanceId\":\"4874595597\",\"itemId\":12,\"positionX\":-3.5068926811218262,\"positionY\":3.4883472919464111,\"positionZ\":-9.5313901901245117,\"rotationX\":-0.00091801158851012588,\"rotationY\":0.006055513396859169,\"rotationZ\":0.000585820700507611,\"rotationW\":0.99998104572296143,\"time\":1686384961,\"photo\":\"/Furniture/Modern2/lampOutputcube.dds\"},{\"flags\":0,\"furnitureObjectId\":\"00000000-00000000-00000002-00000005\",\"instanceId\":\"4874595598\",\"itemId\":13,\"positionX\":3.6171293258666992,\"positionY\":3.4891724586486816,\"positionZ\":-9.53490161895752,\"rotationX\":0.00042979296995326877,\"rotationY\":-0.0092521701008081436,\"rotationZ\":-0.00027207753737457097,\"rotationW\":0.99995702505111694,\"time\":1686385008,\"photo\":\"/Furniture/Modern2/lampOutputcube.dds\"}]}}]");

                                                            if (ssfwkey != "")
                                                            {
                                                                byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                                byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), dataforlayout));

                                                                fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                                fs.Flush();
                                                                fs.Dispose();
                                                            }
                                                            else
                                                            {
                                                                fs.Write(dataforlayout, 0, dataforlayout.Length);
                                                                fs.Flush();
                                                                fs.Dispose();
                                                            }
                                                        }
                                                    }

                                                    if (!File.Exists(Directory.GetCurrentDirectory() + "/wwwssfwroot/RewardsService/cprod/rewards/" + resultString + "/mini.ssfw"))
                                                    {
                                                        using (FileStream fs = new FileStream($"./wwwssfwroot/RewardsService/cprod/rewards/" + resultString + "/mini.ssfw", FileMode.Create))
                                                        {
                                                            byte[] dataforrewardservice = Encoding.UTF8.GetBytes(SSFWminibase);

                                                            if (ssfwkey != "")
                                                            {
                                                                byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                                byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), dataforrewardservice));

                                                                fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                                fs.Flush();
                                                                fs.Dispose();
                                                            }
                                                            else
                                                            {
                                                                fs.Write(dataforrewardservice, 0, dataforrewardservice.Length);
                                                                fs.Flush();
                                                                fs.Dispose();
                                                            }
                                                        }
                                                    }

                                                    if (!File.Exists(Directory.GetCurrentDirectory() + "/wwwssfwroot/RewardsService/trunks-cprod/trunks/" + resultString + ".ssfw"))
                                                    {
                                                        using (FileStream fs = new FileStream($"./wwwssfwroot/RewardsService/trunks-cprod/trunks/" + resultString + ".ssfw", FileMode.Create))
                                                        {
                                                            byte[] dataforrewardservice = Encoding.UTF8.GetBytes("{\"objects\":[]}");

                                                            if (ssfwkey != "")
                                                            {
                                                                byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                                byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), dataforrewardservice));

                                                                fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                                fs.Flush();
                                                                fs.Dispose();
                                                            }
                                                            else
                                                            {
                                                                fs.Write(dataforrewardservice, 0, dataforrewardservice.Length);
                                                                fs.Flush();
                                                                fs.Dispose();
                                                            }
                                                        }
                                                    }

                                                    if (!File.Exists(Directory.GetCurrentDirectory() + "/wwwssfwroot/AvatarLayoutService/cprod/" + resultString + "/list.ssfw"))
                                                    {
                                                        using (FileStream fs = new FileStream($"./wwwssfwroot/AvatarLayoutService/cprod/" + resultString + "/list.ssfw", FileMode.Create))
                                                        {
                                                            byte[] dataforavatarlayoutservice = Encoding.UTF8.GetBytes("[]");

                                                            if (ssfwkey != "")
                                                            {
                                                                byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                                byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), dataforavatarlayoutservice));

                                                                fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                                fs.Flush();
                                                                fs.Dispose();
                                                            }
                                                            else
                                                            {
                                                                fs.Write(dataforavatarlayoutservice, 0, dataforavatarlayoutservice.Length);
                                                                fs.Flush();
                                                                fs.Dispose();
                                                            }
                                                        }
                                                    }

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            // Set the response content type and encoding
                                                            context.Response.ContentType = "application/json";

                                                            // Set the HTTP response status code
                                                            context.Response.StatusCode = 201;

                                                            // Write the JSON response to the output stream
                                                            byte[] buffer = Encoding.UTF8.GetBytes("{ \"session\": [\r\n\t  {\r\n\t\t\"@id\": \"PUT_SESSIONID_HERE\",\r\n\t\t\"person\": {\r\n\t\t  \"@id\": \"PUT_PERSONID_HERE\",\r\n\t\t  \"logonCount\": PUT_LOGONNUMBER_HERE\r\n\t\t}\r\n\t  }\r\n   ]\r\n}"
                                                                .Replace("PUT_SESSIONID_HERE", sessionid).Replace("PUT_PERSONID_HERE", resultString).Replace("PUT_LOGONNUMBER_HERE", logoncount.ToString()));
                                                            context.Response.ContentLength64 = buffer.Length;
                                                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                                            response.OutputStream.Close();
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
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Files creation/check : {ex}");

                                                    // Return an internal server error response
                                                    byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                            response.ContentLength64 = InternnalError.Length;
                                                            response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }

                                                ms.Dispose();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and sending the response : {ex}");

                                            // Return an internal server error response
                                            byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                    response.ContentLength64 = InternnalError.Length;
                                                    response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("SSFW Server : Host requested a login, but it doesn't have any Home required cookies, so we forbid");

                                        // Return a not allowed response
                                        byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = notAllowed.Length;
                                                response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                                response.OutputStream.Close();
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
                                    }
                                }
                                else if (context.Request.Url.AbsolutePath.Contains("/AvatarLayoutService/cprod/") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        string input = context.Request.Url.AbsolutePath;

                                        // Define the regular expression pattern to match a number at the end of the string
                                        Regex regex = new Regex(@"\d+(?![\d/])$");

                                        // Check if the string ends with a number
                                        if (regex.IsMatch(input))
                                        {
                                            // Get the matched number as a string
                                            string numberString = regex.Match(input).Value;

                                            // Convert the matched number to an integer
                                            int number;

                                            // Check if the number is valid
                                            if (!int.TryParse(numberString, out number))
                                            {
                                                Console.WriteLine($"SSFW Server : has errored out in ProcessRequest while processing the number verification for /AvatarLayoutService/cprod/");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        response.ContentLength64 = InternnalError.Length;
                                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        response.OutputStream.Close();
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
                                            }

                                            using (MemoryStream ms = new MemoryStream())
                                            {
                                                request.InputStream.CopyTo(ms);

                                                // Reset the memory stream position to the beginning
                                                ms.Position = 0;

                                                // Find the number of bytes in the stream
                                                int contentLength = (int)ms.Length;

                                                // Create a byte array
                                                byte[] buffer = new byte[contentLength];

                                                // Read the contents of the memory stream into the byte array
                                                ms.Read(buffer, 0, contentLength);

                                                SSFWServices prog = new SSFWServices();

                                                prog.SSFWUpdateavatar(directoryPath + "/list.ssfw", number, false);

                                                try
                                                {
                                                    if (!Directory.Exists(directoryPath))
                                                    {
                                                        Directory.CreateDirectory(directoryPath);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Directories creation/check : {ex}");

                                                    // Return an internal server error response
                                                    byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                            response.ContentLength64 = InternnalError.Length;
                                                            response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }

                                                try
                                                {
                                                    using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw", FileMode.Create))
                                                    {
                                                        if (ssfwkey != "")
                                                        {
                                                            byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                            byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), buffer));

                                                            fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                            fs.Flush();
                                                            fs.Dispose();
                                                        }
                                                        else
                                                        {
                                                            fs.Write(buffer, 0, contentLength);
                                                            fs.Flush();
                                                            fs.Dispose();
                                                        }

                                                        Console.WriteLine($"File {$"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw"} has been uploaded to SSFW");
                                                    }

                                                    // Return a success response

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.StatusCode = (int)HttpStatusCode.OK;
                                                            response.ContentLength64 = contentLength;
                                                            response.OutputStream.Write(buffer, 0, contentLength);
                                                            response.OutputStream.Close();
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

                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Files creation/check : {ex}");

                                                    // Return an internal server error response
                                                    byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                            response.ContentLength64 = InternnalError.Length;
                                                            response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }

                                                ms.Dispose();
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("SSFW Server : AvatarLayoutService does not end with a number, so the request is invalidated.");

                                            byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                    response.ContentLength64 = notAllowed.Length;
                                                    response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
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
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (context.Request.Url.AbsolutePath.Contains("/RewardsService/cprod/rewards/") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            request.InputStream.CopyTo(ms);

                                            // Reset the memory stream position to the beginning
                                            ms.Position = 0;

                                            // Find the number of bytes in the stream
                                            int contentLength = (int)ms.Length;

                                            // Create a byte array
                                            byte[] buffer = new byte[contentLength];

                                            // Read the contents of the memory stream into the byte array
                                            ms.Read(buffer, 0, contentLength);

                                            try
                                            {
                                                if (!Directory.Exists(directoryPath))
                                                {
                                                    Directory.CreateDirectory(directoryPath);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Directories creation/check : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        response.ContentLength64 = InternnalError.Length;
                                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }

                                            try
                                            {
                                                using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw", FileMode.Create))
                                                {
                                                    if (ssfwkey != "")
                                                    {
                                                        byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                        byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), buffer));

                                                        fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }
                                                    else
                                                    {
                                                        fs.Write(buffer, 0, contentLength);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }

                                                    Console.WriteLine($"File {$"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw"} has been uploaded to SSFW");
                                                }

                                                SSFWServices prog = new SSFWServices();

                                                prog.SSFWupdatemini(filePath + "/mini.ssfw", Encoding.UTF8.GetString(buffer));

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        // Return a success response
                                                        response.ContentType = "application/json";
                                                        response.StatusCode = (int)HttpStatusCode.OK;
                                                        response.ContentLength64 = contentLength;
                                                        response.OutputStream.Write(buffer, 0, contentLength);
                                                        response.OutputStream.Close();
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
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Files creation/check : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        response.ContentLength64 = InternnalError.Length;
                                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }

                                            ms.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (context.Request.Url.AbsolutePath.Contains("/RewardsService/trunks-cprod/trunks/") && context.Request.Url.AbsolutePath.Contains("/setpartial") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            request.InputStream.CopyTo(ms);

                                            // Reset the memory stream position to the beginning
                                            ms.Position = 0;

                                            // Find the number of bytes in the stream
                                            int contentLength = (int)ms.Length;

                                            // Create a byte array
                                            byte[] buffer = new byte[contentLength];

                                            // Read the contents of the memory stream into the byte array
                                            ms.Read(buffer, 0, contentLength);

                                            try
                                            {
                                                if (!Directory.Exists(directoryPath))
                                                {
                                                    Directory.CreateDirectory(directoryPath);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Directories creation/check : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        response.ContentLength64 = InternnalError.Length;
                                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }

                                            try
                                            {
                                                using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw", FileMode.Create))
                                                {
                                                    if (ssfwkey != "")
                                                    {
                                                        byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                        byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), buffer));

                                                        fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }
                                                    else
                                                    {
                                                        fs.Write(buffer, 0, contentLength);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }

                                                    Console.WriteLine($"File {$"./wwwwssfwroot{context.Request.Url.AbsolutePath}.ssfw"} has been uploaded to SSFW");
                                                }

                                                SSFWServices prog = new SSFWServices();

                                                prog.SSFWtrunkserviceprocess(filePath.Replace("/setpartial", "") + ".ssfw", Encoding.UTF8.GetString(buffer));

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        // Return a success response
                                                        response.ContentType = "application/json";
                                                        response.StatusCode = (int)HttpStatusCode.OK;
                                                        response.ContentLength64 = contentLength;
                                                        response.OutputStream.Write(buffer, 0, contentLength);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Files creation/check : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        response.ContentLength64 = InternnalError.Length;
                                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }

                                            ms.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (context.Request.Url.AbsolutePath.Contains("/RewardsService/trunks-cprod/trunks/") && context.Request.Url.AbsolutePath.Contains("/set") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            request.InputStream.CopyTo(ms);

                                            // Reset the memory stream position to the beginning
                                            ms.Position = 0;

                                            // Find the number of bytes in the stream
                                            int contentLength = (int)ms.Length;

                                            // Create a byte array
                                            byte[] buffer = new byte[contentLength];

                                            // Read the contents of the memory stream into the byte array
                                            ms.Read(buffer, 0, contentLength);

                                            try
                                            {
                                                if (!Directory.Exists(directoryPath))
                                                {
                                                    Directory.CreateDirectory(directoryPath);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Directories creation/check : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        response.ContentLength64 = InternnalError.Length;
                                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }

                                            try
                                            {
                                                using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw", FileMode.Create))
                                                {
                                                    if (ssfwkey != "")
                                                    {
                                                        byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                                                        byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), buffer));

                                                        fs.Write(encryptedbuffer, 0, encryptedbuffer.Length);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }
                                                    else
                                                    {
                                                        fs.Write(buffer, 0, contentLength);
                                                        fs.Flush();
                                                        fs.Dispose();
                                                    }

                                                    Console.WriteLine($"File {$"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw"} has been uploaded to SSFW");
                                                }

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        // Return a success response
                                                        response.StatusCode = (int)HttpStatusCode.OK;
                                                        response.ContentLength64 = contentLength;
                                                        response.OutputStream.Write(buffer, 0, contentLength);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Files creation/check : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        response.ContentLength64 = InternnalError.Length;
                                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }

                                            ms.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (context.Request.Url.AbsolutePath.Contains("/LayoutService/cprod/person/") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            request.InputStream.CopyTo(ms);

                                            // Reset the memory stream position to the beginning
                                            ms.Position = 0;

                                            // Find the number of bytes in the stream
                                            int contentLength = (int)ms.Length;

                                            // Create a byte array
                                            byte[] buffer = new byte[contentLength];

                                            // Read the contents of the memory stream into the byte array
                                            ms.Read(buffer, 0, contentLength);

                                            try
                                            {
                                                if (!Directory.Exists(directoryPath))
                                                {
                                                    Directory.CreateDirectory(directoryPath);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the Directories creation/check : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        response.ContentLength64 = InternnalError.Length;
                                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }

                                            try
                                            {
                                                string inputurlfortrim = context.Request.Url.AbsolutePath;
                                                string[] words = inputurlfortrim.Split('/');

                                                if (words.Length > 0)
                                                {
                                                    inputurlfortrim = words[words.Length - 1];
                                                }

                                                if (inputurlfortrim != context.Request.Url.AbsolutePath)
                                                {
                                                    SSFWServices prog = new SSFWServices();

                                                    prog.SSFWfurniturelayout(directoryPath + "/mylayout.ssfw", Encoding.UTF8.GetString(buffer), inputurlfortrim);

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            // Return a success response
                                                            response.ContentType = "application/json";
                                                            response.StatusCode = (int)HttpStatusCode.OK;
                                                            response.ContentLength64 = contentLength;
                                                            response.OutputStream.Write(buffer, 0, contentLength);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("SSFW Server : LayoutService does not end with an UUID, so the request is invalidated.");

                                                    byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                            response.ContentLength64 = notAllowed.Length;
                                                            response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and processing the mylayout update : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }

                                            ms.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (context.Request.Url.AbsolutePath.EndsWith("/morelife") && request.Headers["x-signature"] != null)
                                {
                                    try
                                    {
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            request.InputStream.CopyTo(ms);

                                            // Reset the memory stream position to the beginning
                                            ms.Position = 0;

                                            // Find the number of bytes in the stream
                                            int contentLength = (int)ms.Length;

                                            // Create a byte array
                                            byte[] buffer = new byte[contentLength];

                                            // Read the contents of the memory stream into the byte array
                                            ms.Read(buffer, 0, contentLength);

                                            Console.WriteLine("SSFW Server : Host has sent a morelife ping");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.ContentType = "application/json";
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.ContentLength64 = contentLength;
                                                    response.OutputStream.Write(buffer, 0, contentLength);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }

                                            ms.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            request.InputStream.CopyTo(ms);

                                            // Reset the memory stream position to the beginning
                                            ms.Position = 0;

                                            // Find the number of bytes in the stream
                                            int contentLength = (int)ms.Length;

                                            // Create a byte array
                                            byte[] buffer = new byte[contentLength];

                                            // Read the contents of the memory stream into the byte array
                                            ms.Read(buffer, 0, contentLength);

                                            Console.WriteLine("SSFW Server : WARNING - Host requested a POST method I don't know about!! Report it to GITHUB with the request : " + Encoding.UTF8.GetString(buffer));

                                            try
                                            {
                                                if (!Directory.Exists(directoryPath))
                                                {
                                                    Directory.CreateDirectory(directoryPath);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and creating the directory : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (context.Response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        context.Response.ContentLength64 = InternnalError.Length;
                                                        context.Response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        context.Response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }

                                            try
                                            {
                                                if (request.ContentType == "image/jpeg")
                                                {
                                                    using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.jpeg", FileMode.Create))
                                                    {
                                                        fs.Write(buffer, 0, contentLength);
                                                        fs.Flush();
                                                        fs.Dispose();

                                                        Console.WriteLine($"File {$"./wwwssfwroot{context.Request.Url.AbsolutePath}.jpeg"} has been uploaded to SSFW");
                                                    }

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.ContentType = "image/jpeg";
                                                            response.StatusCode = (int)HttpStatusCode.OK;
                                                            response.ContentLength64 = contentLength;
                                                            response.OutputStream.Write(buffer, 0, contentLength);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }
                                                if (request.ContentType == "application/json")
                                                {
                                                    using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw", FileMode.Create))
                                                    {
                                                        fs.Write(buffer, 0, contentLength);
                                                        fs.Flush();
                                                        fs.Dispose();

                                                        Console.WriteLine($"File {$"./wwwssfwroot{context.Request.Url.AbsolutePath}.ssfw"} has been uploaded to SSFW");
                                                    }

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.ContentType = "application/json";
                                                            response.StatusCode = (int)HttpStatusCode.OK;
                                                            response.ContentLength64 = contentLength;
                                                            response.OutputStream.Write(buffer, 0, contentLength);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }
                                                else
                                                {
                                                    using (FileStream fs = new FileStream($"./wwwssfwroot{context.Request.Url.AbsolutePath}.bin", FileMode.Create))
                                                    {
                                                        fs.Write(buffer, 0, contentLength);
                                                        fs.Flush();
                                                        fs.Dispose();

                                                        Console.WriteLine($"File {$"./wwwssfwroot{context.Request.Url.AbsolutePath}.bin"} has been uploaded to SSFW");
                                                    }

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.ContentType = "application/octet-stream";
                                                            response.StatusCode = (int)HttpStatusCode.OK;
                                                            response.ContentLength64 = contentLength;
                                                            response.OutputStream.Write(buffer, 0, contentLength);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and creating the file/http response : {ex}");

                                                // Return an internal server error response
                                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                                if (context.Response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                        context.Response.ContentLength64 = InternnalError.Length;
                                                        context.Response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                        context.Response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }

                                            ms.Dispose();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("SSFW Server : Host has issued a POST command without Home specific cookie. we forbid");

                                    // Return a not allowed response
                                    byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                    if (response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                                            response.ContentLength64 = notAllowed.Length;
                                            response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                            response.OutputStream.Close();
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the POST request : {ex}");

                                // Return an internal server error response
                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                if (response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentLength64 = InternnalError.Length;
                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                        response.OutputStream.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client Disconnected early");
                                }
                            }

                            break;

                        case "GET":

                            try
                            {
                                if (context.Request.Url.AbsolutePath.Contains("/LayoutService/cprod/person/") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        string inputurlfortrim = context.Request.Url.AbsolutePath;
                                        string[] words = inputurlfortrim.Split('/');

                                        if (words.Length > 0)
                                        {
                                            inputurlfortrim = words[words.Length - 1];
                                        }

                                        if (inputurlfortrim != context.Request.Url.AbsolutePath)
                                        {
                                            SSFWServices prog = new SSFWServices();

                                            string stringlayout = prog.SSFWgetfurniturelayout(directoryPath + "/mylayout.ssfw", inputurlfortrim);

                                            if (stringlayout != "")
                                            {
                                                Console.WriteLine($"SSFW Server : Returned furniture layout for " + inputurlfortrim);

                                                byte[] layout = Encoding.UTF8.GetBytes("[{\"PUT_SCENEID_HERE\":PUT_LAYOUT_HERE}]".Replace("PUT_SCENEID_HERE", inputurlfortrim).Replace("PUT_LAYOUT_HERE", stringlayout));

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        // Return a success response
                                                        response.ContentType = "application/json";
                                                        response.StatusCode = (int)HttpStatusCode.OK;
                                                        response.ContentLength64 = layout.Length;
                                                        response.OutputStream.Write(layout, 0, layout.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"SSFW Server : No furniture layout for " + inputurlfortrim);

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        // Return a not found response
                                                        byte[] notFoundResponse = Encoding.UTF8.GetBytes("File not found");
                                                        response.StatusCode = (int)HttpStatusCode.NotFound;
                                                        response.ContentLength64 = notFoundResponse.Length;
                                                        response.OutputStream.Write(notFoundResponse, 0, notFoundResponse.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("SSFW Server : LayoutService does not end with an UUID, so the request is invalidated.");

                                            byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                    response.ContentLength64 = notAllowed.Length;
                                                    response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the GET request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (context.Request.Url.AbsolutePath.Contains("/AdminObjectService/start") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        string sessionid = request.Headers["X-Home-Session-Id"];

                                        if (File.Exists(Directory.GetCurrentDirectory() + $"/loginformNtemplates/SSFW_Accounts/{sessionid}.ssfw"))
                                        {
                                            string tempcontent = "";

                                            byte[] firstNineBytes = new byte[9];

                                            using (FileStream fileStream = new FileStream(Directory.GetCurrentDirectory() + $"/loginformNtemplates/SSFW_Accounts/{sessionid}.ssfw", FileMode.Open, FileAccess.Read))
                                            {
                                                fileStream.Read(firstNineBytes, 0, 9);
                                                fileStream.Close();
                                            }

                                            if (ssfwkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                                            {
                                                byte[] src = File.ReadAllBytes(Directory.GetCurrentDirectory() + $"/loginformNtemplates/SSFW_Accounts/{sessionid}.ssfw");
                                                byte[] dst = new byte[src.Length - 9];

                                                Array.Copy(src, 9, dst, 0, dst.Length);

                                                tempcontent = Encoding.UTF8.GetString(CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                                            CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey)));
                                            }
                                            else
                                            {
                                                tempcontent = File.ReadAllText(Directory.GetCurrentDirectory() + $"/loginformNtemplates/SSFW_Accounts/{sessionid}.ssfw");
                                            }

                                            // Parsing JSON data to SSFWUserData object
                                            SSFWUserData userData = JsonConvert.DeserializeObject<SSFWUserData>(tempcontent);

                                            if (userData != null)
                                            {
                                                Console.WriteLine($"SSFW Server : IGA Request from : {sessionid} - IGA : {userData.IGA}");

                                                if (userData.IGA == 1)
                                                {
                                                    Console.WriteLine($"SSFW Server : Admin role confirmed");

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            // Return a success response
                                                            response.ContentType = "application/json";
                                                            response.StatusCode = (int)HttpStatusCode.OK;
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("SSFW Server : Host requested a IGA access, but no access allowed so we forbid");

                                                    byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                                    if (response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                            response.ContentLength64 = notAllowed.Length;
                                                            response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                                            response.OutputStream.Close();
                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Client Disconnected early");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("SSFW Server : Host requested a IGA access, but no access allowed so we forbid");

                                                byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                                if (response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                        response.ContentLength64 = notAllowed.Length;
                                                        response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                                        response.OutputStream.Close();
                                                    }
                                                    catch (Exception ex1)
                                                    {
                                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Client Disconnected early");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("SSFW Server : Host requested a IGA access, but no access allowed so we forbid");

                                            byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                    response.ContentLength64 = notAllowed.Length;
                                                    response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the GET /AdminObjectService/start request : {ex}");

                                        // Return an internal SSFW Server : error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (File.Exists(filePath + ".ssfw") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        byte[] firstNineBytes = new byte[9];

                                        using (FileStream fileStream = new FileStream(filePath + ".ssfw", FileMode.Open, FileAccess.Read))
                                        {
                                            fileStream.Read(firstNineBytes, 0, 9);
                                            fileStream.Close();
                                        }

                                        if (ssfwkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                                        {
                                            byte[] src = File.ReadAllBytes(filePath + ".ssfw");
                                            byte[] dst = new byte[src.Length - 9];

                                            Array.Copy(src, 9, dst, 0, dst.Length);

                                            byte[] fileBytes = CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                                        CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey));

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    if (request.Headers["Accept"] == "application/json")
                                                    {
                                                        response.ContentType = "application/json";
                                                    }

                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.ContentLength64 = fileBytes.Length;
                                                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                        else
                                        {
                                            // Read the file contents as bytes
                                            byte[] fileBytes = File.ReadAllBytes(filePath + ".ssfw");
                                            // Return the file contents as the response body

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    if (request.Headers["Accept"] == "application/json")
                                                    {
                                                        response.ContentType = "application/json";
                                                    }

                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.ContentLength64 = fileBytes.Length;
                                                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }

                                        Console.WriteLine($"SSFW Server : Returned file contents from {filePath + ".ssfw"}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server :  has throw an exception in ProcessRequest while processing the GET request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (File.Exists(filePath + ".bin") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        byte[] firstNineBytes = new byte[9];

                                        using (FileStream fileStream = new FileStream(filePath + ".bin", FileMode.Open, FileAccess.Read))
                                        {
                                            fileStream.Read(firstNineBytes, 0, 9);
                                            fileStream.Close();
                                        }

                                        if (ssfwkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                                        {
                                            byte[] src = File.ReadAllBytes(filePath + ".bin");
                                            byte[] dst = new byte[src.Length - 9];

                                            Array.Copy(src, 9, dst, 0, dst.Length);

                                            byte[] fileBytes = CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                                        CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey));

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    // Return the file contents as the response body
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.ContentLength64 = fileBytes.Length;
                                                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                                                    response.OutputStream.Close();

                                                    Console.WriteLine($"SSFW Server : Returned file contents from {filePath + ".bin"}");
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                        else
                                        {
                                            byte[] fileBytes = File.ReadAllBytes(filePath + ".bin");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    // Return the file contents as the response body
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.ContentLength64 = fileBytes.Length;
                                                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                                                    response.OutputStream.Close();

                                                    Console.WriteLine($"SSFW Server : Returned file contents from {filePath + ".bin"}");
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server :  has throw an exception in ProcessRequest while processing the GET request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (File.Exists(filePath + ".jpeg") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        byte[] firstNineBytes = new byte[9];

                                        using (FileStream fileStream = new FileStream(filePath + ".jpeg", FileMode.Open, FileAccess.Read))
                                        {
                                            fileStream.Read(firstNineBytes, 0, 9);
                                            fileStream.Close();
                                        }

                                        if (ssfwkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                                        {
                                            byte[] src = File.ReadAllBytes(filePath + ".jpeg");
                                            byte[] dst = new byte[src.Length - 9];

                                            Array.Copy(src, 9, dst, 0, dst.Length);

                                            byte[] fileBytes = CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                                        CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey));

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    // Return the file contents as the response body
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.ContentLength64 = fileBytes.Length;
                                                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                                                    response.OutputStream.Close();

                                                    Console.WriteLine($"SSFW Server : Returned file contents from {filePath + ".jpeg"}");
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                        else
                                        {
                                            // Read the file contents as bytes
                                            byte[] fileBytes = File.ReadAllBytes(filePath + ".jpeg");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    // Return the file contents as the response body
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.ContentLength64 = fileBytes.Length;
                                                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                                                    response.OutputStream.Close();

                                                    Console.WriteLine($"Returned file contents from {filePath + ".jpeg"}");
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the GET request and sending the response : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (request.Headers["X-Home-Session-Id"] != null)
                                {
                                    // Return nothing, it seems SSFW likes better response 404 with nothing inside.

                                    Console.WriteLine($"SSFW : File {filePath} not found");

                                    if (response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = 404;
                                            response.OutputStream.Close();
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"SSFW Server : Host has issued a GET command without Home specific cookie. We forbid");

                                    if (response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            // Return a not allowed response
                                            byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");
                                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                                            response.ContentLength64 = notAllowed.Length;
                                            response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                            response.OutputStream.Close();
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the GET request : {ex}");

                                // Return an internal server error response
                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                if (response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentLength64 = InternnalError.Length;
                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                        response.OutputStream.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client Disconnected early");
                                }
                            }

                            break;

                        case "DELETE":

                            try
                            {
                                if (context.Request.Url.AbsolutePath.Contains("/AvatarLayoutService/cprod/") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    string input = context.Request.Url.AbsolutePath;

                                    // Define the regular expression pattern to match a number at the end of the string
                                    Regex regex = new Regex(@"\d+(?![\d/])$");

                                    // Check if the string ends with a number
                                    if (regex.IsMatch(input))
                                    {
                                        // Get the matched number as a string
                                        string numberString = regex.Match(input).Value;

                                        // Convert the matched number to an integer
                                        int number;

                                        if (!int.TryParse(numberString, out number))
                                        {
                                            Console.WriteLine($"SSFW Server : has errored out in ProcessRequest while processing the number verification for /AvatarLayoutService/cprod/");

                                            // Return an internal server error response
                                            byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                    response.ContentLength64 = InternnalError.Length;
                                                    response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }

                                        SSFWServices prog = new SSFWServices();

                                        prog.SSFWUpdateavatar(directoryPath + "/list.ssfw", number, true);

                                        try
                                        {
                                            // Delete the binary file
                                            File.Delete(filePath + ".ssfw");

                                            Console.WriteLine($"Deleted file {filePath + ".ssfw"}");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    // Return a success response
                                                    byte[] deleteResponse = Encoding.UTF8.GetBytes("File deleted successfully");
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.ContentLength64 = deleteResponse.Length;
                                                    response.OutputStream.Write(deleteResponse, 0, deleteResponse.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the DELETE request and removing said file : {ex}");

                                            // Return an internal server error response
                                            byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                    response.ContentLength64 = InternnalError.Length;
                                                    response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("SSFW Server : AvatarLayoutService does not end with a number, so the request is invalidated.");

                                        byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = notAllowed.Length;
                                                response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (File.Exists(filePath + ".ssfw") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        // Delete the binary file
                                        File.Delete(filePath + ".ssfw");

                                        Console.WriteLine($"SSFW Server : Deleted file {filePath + ".ssfw"}");

                                        // Return a success response
                                        byte[] deleteResponse = Encoding.UTF8.GetBytes("File deleted successfully");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.ContentLength64 = deleteResponse.Length;
                                                response.OutputStream.Write(deleteResponse, 0, deleteResponse.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the DELETE request and removing said file : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (File.Exists(filePath + ".jpeg") && request.Headers["X-Home-Session-Id"] != null)
                                {
                                    try
                                    {
                                        // Delete the binary file
                                        File.Delete(filePath + ".jpeg");

                                        Console.WriteLine($"SSFW Server : Deleted file {filePath + ".jpeg"}");

                                        // Return a success response
                                        byte[] deleteResponse = Encoding.UTF8.GetBytes("File deleted successfully");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.ContentLength64 = deleteResponse.Length;
                                                response.OutputStream.Write(deleteResponse, 0, deleteResponse.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the DELETE request and removing said file : {ex}");

                                        // Return an internal server error response
                                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                                response.ContentLength64 = InternnalError.Length;
                                                response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Client Disconnected early");
                                        }
                                    }
                                }
                                else if (request.Headers["X-Home-Session-Id"] != null)
                                {
                                    // Return a not found response
                                    byte[] notFoundResponse = Encoding.UTF8.GetBytes("File not found");

                                    if (response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = (int)HttpStatusCode.NotFound;
                                            response.ContentLength64 = notFoundResponse.Length;
                                            response.OutputStream.Write(notFoundResponse, 0, notFoundResponse.Length);
                                            response.OutputStream.Close();

                                            Console.WriteLine($"File {filePath} not found");
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"SSFW Server : Host has issued a DELETE command without Home specific cookie. We forbid");

                                    // Return a not allowed response
                                    byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                    if (response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                                            response.ContentLength64 = notAllowed.Length;
                                            response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                            response.OutputStream.Close();
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the DELETE request : {ex}");

                                // Return an internal server error response
                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                if (response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentLength64 = InternnalError.Length;
                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                        response.OutputStream.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client Disconnected early");
                                }
                            }

                            break;

                        default:

                            try
                            {
                                if (request.Headers["X-Home-Session-Id"] != null)
                                {
                                    Console.WriteLine($"SSFW Server : WARNING - Host requested a method I don't know about!! Report it to GITHUB with the request : {httpMethod} request for {url} is not supported");

                                    // Return a method not allowed response for unsupported methods
                                    byte[] methodNotAllowedResponse = Encoding.UTF8.GetBytes("Method not allowed");

                                    if (response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                                            response.ContentLength64 = methodNotAllowedResponse.Length;
                                            response.OutputStream.Write(methodNotAllowedResponse, 0, methodNotAllowedResponse.Length);
                                            response.OutputStream.Close();
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"SSFW Server : Host has issued a UNKNOWN command without Home specific cookie. We forbid");

                                    // Return a not allowed response
                                    byte[] notAllowed = Encoding.UTF8.GetBytes("Not allowed.");

                                    if (response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                                            response.ContentLength64 = notAllowed.Length;
                                            response.OutputStream.Write(notAllowed, 0, notAllowed.Length);
                                            response.OutputStream.Close();
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest while processing the default request : {ex}");

                                // Return an internal server error response
                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                if (response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        response.ContentLength64 = InternnalError.Length;
                                        response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                        response.OutputStream.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client Disconnected early");
                                }
                            }

                            break;
                    }
                }
                else
                {
                    Console.WriteLine($"SSFW Server : {context.Request.Headers["User-Agent"]} - is not a PS3, we forbid!");

                    byte[] buffer = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                    if (response.OutputStream.CanWrite)
                    {
                        try
                        {
                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                            response.ContentLength64 = buffer.Length;
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                            response.OutputStream.Close();
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
                }

                // Close the response
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SSFW Server : has throw an exception in ProcessRequest : {ex}");

                GC.Collect();

                return;
            }

            GC.Collect();

            return;
        }
        private void SSFWtrunkserviceprocess(string filePath, string request)
        {
            try
            {
                string json = "";

                byte[] firstNineBytes = new byte[9];

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Read(firstNineBytes, 0, 9);
                    fileStream.Close();
                }

                if (ssfwkey != "" && Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 }))
                {
                    byte[] src = File.ReadAllBytes(filePath);
                    byte[] dst = new byte[src.Length - 9];

                    Array.Copy(src, 9, dst, 0, dst.Length);

                    json = Encoding.UTF8.GetString(CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey)));
                }
                else
                {
                    json = File.ReadAllText(filePath);
                }

                JObject mainFile = JObject.Parse(json);

                // Parse the request
                JObject requestObject = JObject.Parse(request);

                // Check if 'add' operation is requested
                if (requestObject.ContainsKey("add"))
                {
                    JArray addArray = (JArray)requestObject["add"]["objects"];
                    JArray mainArray = (JArray)mainFile["objects"];

                    // Add each new object to the main file
                    foreach (JObject addObject in addArray)
                    {
                        mainArray.Add(addObject);
                    }
                }

                // Check if 'update' operation is requested
                if (requestObject.ContainsKey("update"))
                {
                    JArray updateArray = (JArray)requestObject["update"]["objects"];
                    JArray mainArray = (JArray)mainFile["objects"];

                    // Update the existing objects in the main file
                    foreach (JObject updateObj in updateArray)
                    {
                        string objectId = updateObj["objectId"].ToString();

                        // Find the object in the main file and update its properties
                        JObject existingObj = (JObject)mainArray.FirstOrDefault(obj => obj["objectId"].ToString() == objectId);
                        if (existingObj != null)
                        {
                            existingObj.Merge(updateObj, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
                        }
                    }
                }

                // Check if 'delete' operation is requested
                if (requestObject.ContainsKey("delete"))
                {
                    JArray deleteArray = (JArray)requestObject["delete"]["objects"];
                    JArray mainArray = (JArray)mainFile["objects"];

                    // Remove the objects from the main file
                    foreach (JObject deleteObj in deleteArray)
                    {
                        string objectId = deleteObj["objectId"].ToString();

                        // Find and remove the object from the main file
                        JObject existingObj = (JObject)mainArray.FirstOrDefault(obj => obj["objectId"].ToString() == objectId);
                        if (existingObj != null)
                        {
                            existingObj.Remove();
                        }
                    }
                }

                // Save the modified main file back to the original file path
                string updatedJson = mainFile.ToString();

                if (ssfwkey != "")
                {
                    byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                    byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), Encoding.UTF8.GetBytes(updatedJson)));

                    File.WriteAllBytes(filePath, encryptedbuffer);
                }
                else
                {
                    File.WriteAllText(filePath, updatedJson);
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SSFW Server : has throw an exception in SSFWtrunkserviceprocess : {ex}");

                return;
            }
        }
        private void SSFWupdatemini(string filePath, string postData)
        {
            try
            {
                string json = "";

                byte[] firstNineBytes = new byte[9];

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Read(firstNineBytes, 0, 9);
                    fileStream.Close();
                }

                if (ssfwkey != "" && Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 }))
                {
                    byte[] src = File.ReadAllBytes(filePath);
                    byte[] dst = new byte[src.Length - 9];

                    Array.Copy(src, 9, dst, 0, dst.Length);

                    json = Encoding.UTF8.GetString(CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey)));
                }
                else
                {
                    json = File.ReadAllText(filePath);
                }

                // Parse the JSON string as a JArray
                JArray jsonArray;
                if (string.IsNullOrEmpty(json))
                {
                    jsonArray = new JArray();
                }
                else
                {
                    jsonArray = JArray.Parse(json);
                }

                // Extract the rewards object from the POST data
                JObject postDataObject = JObject.Parse(postData);
                JObject rewardsObject = (JObject)postDataObject["rewards"];

                // Iterate over each reward in the POST data
                foreach (var reward in rewardsObject)
                {
                    string rewardKey = reward.Key;

                    // Check if the reward exists in the JSON array
                    JToken existingReward = jsonArray.FirstOrDefault(r => r[rewardKey] != null);
                    if (existingReward != null)
                    {
                        // Update the value of the reward to 1
                        existingReward[rewardKey] = 1;
                    }
                    else
                    {
                        // Create a new reward object with the value 1
                        JObject newReward = new JObject();
                        newReward.Add(rewardKey, 1);

                        // Add the new reward to the JSON array
                        jsonArray.Add(newReward);
                    }
                }

                // Convert the updated JSON array back to a string
                string updatedJson = jsonArray.ToString();

                if (ssfwkey != "")
                {
                    byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                    byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), Encoding.UTF8.GetBytes(updatedJson)));

                    File.WriteAllBytes(filePath, encryptedbuffer);
                }
                else
                {
                    File.WriteAllText(filePath, updatedJson);
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SSFW Server : has throw an exception in SSFWupdatemini : {ex}");

                return;
            }
        }
        private void SSFWfurniturelayout(string filePath, string postData, string objectId)
        {
            try
            {
                string json = "";

                byte[] firstNineBytes = new byte[9];

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Read(firstNineBytes, 0, 9);
                    fileStream.Close();
                }

                if (ssfwkey != "" && Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 }))
                {
                    byte[] src = File.ReadAllBytes(filePath);
                    byte[] dst = new byte[src.Length - 9];

                    Array.Copy(src, 9, dst, 0, dst.Length);

                    json = Encoding.UTF8.GetString(CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey)));
                }
                else
                {
                    json = File.ReadAllText(filePath);
                }

                // Parse the JSON string as a JArray
                JArray jsonArray;
                if (string.IsNullOrEmpty(json))
                {
                    jsonArray = new JArray();
                }
                else
                {
                    jsonArray = JArray.Parse(json);
                }

                // Find the index of the existing object with the given objectId
                int existingIndex = -1;
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    JObject obj = jsonArray[i] as JObject;
                    if (obj != null && obj.Properties().Any(p => p.Name == objectId))
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    // Update the existing object with the new POST data
                    JObject existingObject = jsonArray[existingIndex] as JObject;
                    existingObject[objectId] = JObject.Parse(postData);
                }
                else
                {
                    // Create a new object with the objectId and POST data
                    JObject newObject = new JObject();
                    newObject.Add(objectId, JObject.Parse(postData));

                    // Add the new object to the JSON array
                    jsonArray.Add(newObject);
                }

                // Convert the updated JSON array back to a string
                string updatedJson = jsonArray.ToString(Newtonsoft.Json.Formatting.None);

                if (ssfwkey != "")
                {
                    byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                    byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), Encoding.UTF8.GetBytes(updatedJson)));

                    File.WriteAllBytes(filePath, encryptedbuffer);
                }
                else
                {
                    File.WriteAllText(filePath, updatedJson);
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SSFW Server : has throw an exception in SSFWfurniturelayout : {ex}");

                return;
            }
        }
        private string SSFWgetfurniturelayout(string filePath, string objectId)
        {
            try
            {
                string json = "";

                byte[] firstNineBytes = new byte[9];

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Read(firstNineBytes, 0, 9);
                    fileStream.Close();
                }

                if (ssfwkey != "" && Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 }))
                {
                    byte[] src = File.ReadAllBytes(filePath);
                    byte[] dst = new byte[src.Length - 9];

                    Array.Copy(src, 9, dst, 0, dst.Length);

                    json = Encoding.UTF8.GetString(CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey)));
                }
                else
                {
                    json = File.ReadAllText(filePath);
                }

                // Parse the JSON string as a JArray
                JArray jsonArray = JArray.Parse(json);

                // Find the object with the given objectId
                JObject existingObject = jsonArray
                    .FirstOrDefault(obj => obj is JObject && obj[objectId] != null) as JObject;

                if (existingObject != null)
                {
                    // Retrieve the value for the given objectId
                    JToken value = existingObject[objectId];
                    return value.ToString();
                }

                // If the objectId is not found, return null or an appropriate default value
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SSFW Server : has throw an exception in SSFWgetfurniturelayout : {ex}");

                return "";
            }
        }
        private void SSFWUpdateavatar(string filePath, int contentToUpdate, bool delete)
        {
            try
            {
                string json = "";

                byte[] firstNineBytes = new byte[9];

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Read(firstNineBytes, 0, 9);
                    fileStream.Close();
                }

                if (ssfwkey != "" && Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 }))
                {
                    byte[] src = File.ReadAllBytes(filePath);
                    byte[] dst = new byte[src.Length - 9];

                    Array.Copy(src, 9, dst, 0, dst.Length);

                    json = Encoding.UTF8.GetString(CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey)));
                }
                else
                {
                    json = File.ReadAllText(filePath);
                }

                // Parse the JSON content
                JArray jsonArray = JArray.Parse(json);

                if (delete)
                {
                    // Remove the element(s) with the specified number
                    jsonArray = new JArray(jsonArray.Where(item => item.Value<int>("") != contentToUpdate));
                }
                else
                {
                    // Create a new JSON object with the specified number
                    JObject jsonObject = new JObject();
                    jsonObject[""] = contentToUpdate;

                    if (!delete)
                    {
                        // Check if the element already exists
                        JToken existingItem = jsonArray.FirstOrDefault(item => item.Value<int>("") == contentToUpdate);

                        if (existingItem != null)
                        {
                            // Update the existing element
                            existingItem.Replace(jsonObject);
                        }
                        else
                        {
                            // Add the new element to the array
                            jsonArray.Add(jsonObject);
                        }
                    }
                    else
                    {
                        Console.WriteLine("SSFW Server : SSFWUpdateavatar - Invalid mode specified. Please use 'UPDATE' or 'DELETE'.");

                        return;
                    }
                }

                // Serialize the JSON array back to a string
                string updatedJson = jsonArray.ToString();

                if (ssfwkey != "")
                {
                    byte[] outfile = new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6C, 0x65, 0x64, 0x65, 0x73 };

                    byte[] encryptedbuffer = Misc.Combinebytearay(outfile, CRYPTOSPORIDIUM.TRIPLEDES.EncryptData(CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(ssfwkey), Encoding.UTF8.GetBytes(updatedJson)));

                    File.WriteAllBytes(filePath, encryptedbuffer);
                }
                else
                {
                    File.WriteAllText(filePath, updatedJson);
                }

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SSFW Server : has throw an exception in SSFWUpdateavatar : {ex}");

                return;
            }
        }

        private static string ssfwgenerateguid(string input1, string input2)
        {
            string md5hash = "";
            string sha512hash = "";

            using (MD5 md5 = MD5.Create())
            {
                string salt = "**H0mEIsG3reAT!!!!!!!!!!!!!!";

                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input1 + salt));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Dispose();
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                string salt = "C0MeBaCKHOm3*!*!*!*!*!*!*!*!";

                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(salt + input2));
                sha512hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                sha512.Dispose();
            }

            return (md5hash.Substring(1, 8) + "-" + sha512hash.Substring(2, 4) + "-" + md5hash.Substring(10, 4) + "-" + sha512hash.Substring(16, 4) + "-" + sha512hash.Substring(19, 12)).ToLower();
        }
    }
    public class SSFWUserData
    {
        public string Username { get; set; }
        public int LogonCount { get; set; }
        public int IGA { get; set; }
    }
}
