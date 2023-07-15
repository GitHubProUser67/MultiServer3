using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HttpMultipartParser;

namespace PSMultiServer.SRC_Addons.HOME
{
    public class VEEMEEservices
    {
        public static async Task ProcessRequest(HttpListenerContext context, string userAgent)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // Extract the HTTP method and the relative path
                string httpMethod = request.HttpMethod;
                string url = request.Url.LocalPath;

                Console.WriteLine($"VEEMEE Server : Received {httpMethod} request for {url}");

                // Split the URL into segments
                string[] segments = url.Trim('/').Split('/');

                // Combine the folder segments into a directory path
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/", string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                // Process the request based on the HTTP method
                string filePath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/", url.Substring(1));

                switch (httpMethod)
                {
                    case "GET":

                        try
                        {
                            if (request.Url.AbsolutePath.Replace("//", "/") == "/stats/getconfig.php")
                            {
                                Task.Run(() => GetConfig(context, true, userAgent));
                            }
                            else
                            {
                                // Return a not found response
                                byte[] notFoundResponse = Encoding.UTF8.GetBytes("Method not found");

                                if (response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        response.ContentType = "text/plain; charset=utf-8";
                                        response.ContentLength64 = notFoundResponse.Length;
                                        response.KeepAlive = true;
                                        response.StatusCode = 405;
                                        response.OutputStream.Write(notFoundResponse, 0, notFoundResponse.Length);
                                        response.OutputStream.Close();

                                        Console.WriteLine($"VEEMEE Server : Method {filePath} - {httpMethod} not found");
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

                                response.Close();

                                GC.Collect();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"VEEMEE Server : has throw an exception in ProcessRequest while processing GET request : {ex}");

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

                            response.Close();

                            GC.Collect();
                        }

                        break;

                    case "POST":

                        try
                        {
                            if (request.Url.AbsolutePath.Replace("//", "/") == "/commerce/get_count.php")
                            {
                                Task.Run(() => get_count(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/commerce/get_ownership.php")
                            {
                                Task.Run(() => get_ownership(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/data/parkChallenges.php")
                            {
                                Task.Run(() => ParkChallenges(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/data/parkTasks.php")
                            {
                                Task.Run(() => ParkTasks(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/slot-management/getobjectslot.php")
                            {
                                Task.Run(() => GetObjectSlot(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/slot-management/remove.php")
                            {
                                Task.Run(() => Remove(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/slot-management/heartbeat.php")
                            {
                                Task.Run(() => Heartbeat(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/stats/getconfig.php")
                            {
                                Task.Run(() => GetConfig(context, false, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/stats/crash.php")
                            {
                                Task.Run(() => crash(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/stats_tracking/usage.php")
                            {
                                Task.Run(() => Usage(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/storage/readconfig.php")
                            {
                                Task.Run(() => ReadConfig(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/storage/readtable.php")
                            {
                                Task.Run(() => ReadTable(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath.Replace("//", "/") == "/storage/writetable.php")
                            {
                                Task.Run(() => WriteTable(context, userAgent));
                            }
                            else
                            {
                                // Return a not found response
                                byte[] notFoundResponse = Encoding.UTF8.GetBytes("Method not found");

                                if (response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        response.ContentType = "text/plain; charset=utf-8";
                                        response.ContentLength64 = notFoundResponse.Length;
                                        response.KeepAlive = true;
                                        response.StatusCode = 405;
                                        response.OutputStream.Write(notFoundResponse, 0, notFoundResponse.Length);
                                        response.OutputStream.Close();

                                        Console.WriteLine($"VEEMEE Server : Method {filePath} - {httpMethod} not found");
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

                                response.Close();

                                GC.Collect();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"VEEMEE Server : has throw an exception in ProcessRequest while processing POST request : {ex}");

                            // Return an internal server error response
                            byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                            if (response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    response.ContentType = "text/plain; charset=utf-8";
                                    response.ContentLength64 = InternnalError.Length;
                                    response.KeepAlive = true;
                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
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

                            response.Close();

                            GC.Collect();
                        }

                        break;

                    default:

                        try
                        {
                            Console.WriteLine($"VEEMEE Server : WARNING - Host requested a method I don't know about!! Report it to GITHUB with the request : {httpMethod} request for {url} is not supported");

                            // Return a method not allowed response for unsupported methods
                            byte[] methodNotAllowedResponse = Encoding.UTF8.GetBytes("Method not allowed");

                            if (response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    response.ContentType = "text/plain; charset=utf-8";
                                    response.ContentLength64 = methodNotAllowedResponse.Length;
                                    response.KeepAlive = true;
                                    response.StatusCode = 405;
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

                            response.Close();

                            GC.Collect();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"VEEMEE Server : has throw an exception in ProcessRequest while processing the default request : {ex}");

                            // Return an internal server error response
                            byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                            if (response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    response.ContentType = "text/plain; charset=utf-8";
                                    response.ContentLength64 = InternnalError.Length;
                                    response.KeepAlive = true;
                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
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

                            response.Close();

                            GC.Collect();
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : an error occured in ProcessRequest - {ex}");

                context.Response.Close();

                GC.Collect();
            }

            return;
        }

        private static async Task get_count(HttpListenerContext context, string userAgent)
        {
            try
            {
                VEEMEELoginCounter counter = new VEEMEELoginCounter();

                byte[] clientresponse = sign("{ \"count\": PUT_NUMBER_HERE }".Replace("PUT_NUMBER_HERE", counter.GetLoginCount("Voodooperson05").ToString()));

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in get_count while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        private static async Task get_ownership(HttpListenerContext context, string userAgent)
        {
            try
            {
                byte[] clientresponse = sign("{ \"owner\": \"Voodooperson05\" }");

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in get_ownership while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        private static async Task ParkChallenges(HttpListenerContext context, string userAgent)
        {
            try
            {
                byte[] clientresponse = sign(File.ReadAllText(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_VEEMEE/Acorn_Medow/challenges.json"));

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in ParkChallenges while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        private static async Task ParkTasks(HttpListenerContext context, string userAgent)
        {
            try
            {
                byte[] clientresponse = sign(File.ReadAllText(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_VEEMEE/Acorn_Medow/tasks.json"));

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in ParkTasks while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }
        private static async Task GetObjectSlot(HttpListenerContext context, string userAgent)
        {
            try
            {
                string slot_name = "";
                string session_key = "";
                string scene_id = "";
                string region = "";
                int max_slot = 0;
                string object_id = "";
                string psn_id = "";
                string instance_id = "";
                string hex = "";
                string __salt = "";

                string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                // Get the input stream from the context
                Stream inputStream = context.Request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    slot_name = data.GetParameterValue("slot_name");

                    session_key = data.GetParameterValue("session_key");

                    scene_id = data.GetParameterValue("scene_id");

                    region = data.GetParameterValue("region");

                    max_slot = int.Parse(data.GetParameterValue("max_slot"));

                    object_id = data.GetParameterValue("object_id");

                    psn_id = data.GetParameterValue("psn_id");

                    instance_id = data.GetParameterValue("instance_id");

                    hex = data.GetParameterValue("hex");

                    __salt = data.GetParameterValue("__salt");

                    copyStream.Dispose();
                }

                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"loginformNtemplates/HOME_VEEMEE/Acorn_Medow/Object_Instances/{instance_id}/{slot_name}/");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                for (int i = 1; i <= max_slot; i++)
                {
                    if (!File.Exists(directoryPath + $"{i}.xml"))
                    {
                        File.WriteAllText(directoryPath + $"{i}.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<xml>\r\n  <slot>unnocupied</slot>\r\n  <expiration>01/01/1970 00:00:00</expiration>\r\n</xml>");
                    }
                }

                byte[] clientresponse = sign("{\"slot\":PUT_NUMBER_HERE}".Replace("PUT_NUMBER_HERE", UpdateSlot(directoryPath, psn_id, 0, false)));

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in GetObjectSlot while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        private static async Task Remove(HttpListenerContext context, string userAgent)
        {
            try
            {
                int slot_num = 0;
                string slot_name = "";
                string session_key = "";
                string scene_id = "";
                string region = "";
                string object_id = "";
                string psn_id = "";
                string instance_id = "";
                string hex = "";
                string __salt = "";

                string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                // Get the input stream from the context
                Stream inputStream = context.Request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    try
                    {
                        slot_num = int.Parse(data.GetParameterValue("slot_num")); // issues
                    }
                    catch (Exception ex) // User not clicked on anything, remove all
                    {
                        slot_num = 0;
                    }

                    slot_name = data.GetParameterValue("slot_name");

                    session_key = data.GetParameterValue("session_key");

                    scene_id = data.GetParameterValue("scene_id");

                    region = data.GetParameterValue("region");

                    object_id = data.GetParameterValue("object_id");

                    psn_id = data.GetParameterValue("psn_id");

                    instance_id = data.GetParameterValue("instance_id");

                    hex = data.GetParameterValue("hex");

                    __salt = data.GetParameterValue("__salt");

                    copyStream.Dispose();
                }

                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"loginformNtemplates/HOME_VEEMEE/Acorn_Medow/Object_Instances/{instance_id}/{slot_name}/");

                if (!Directory.Exists(directoryPath))
                {
                    byte[] clientresponse = sign("{\"success\":false}");

                    if (context.Response.OutputStream.CanWrite)
                    {
                        try
                        {
                            context.Response.ContentType = "text/plain; charset=utf-8";
                            context.Response.ContentLength64 = clientresponse.Length;
                            context.Response.KeepAlive = true;
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                            context.Response.OutputStream.Close();

                            Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                    context.Response.Close();
                }
                else
                {
                    byte[] clientresponse = sign("{\"success\":PUT_BOOL_HERE}".Replace("PUT_BOOL_HERE", UpdateSlot(directoryPath, psn_id, slot_num, true)));

                    if (context.Response.OutputStream.CanWrite)
                    {
                        try
                        {
                            context.Response.ContentType = "text/plain; charset=utf-8";
                            context.Response.ContentLength64 = clientresponse.Length;
                            context.Response.KeepAlive = true;
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                            context.Response.OutputStream.Close();

                            Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in Remove while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        private static async Task Heartbeat(HttpListenerContext context, string userAgent)
        {
            try
            {
                string session_key = "";
                string scene_id = "";
                string region = "";
                string slot_name = "";
                string object_id = "";
                string psn_id = "";
                string instance_id = "";
                string hex = "";
                string __salt = "";

                string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                // Get the input stream from the context
                Stream inputStream = context.Request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    session_key = data.GetParameterValue("session_key");

                    scene_id = data.GetParameterValue("scene_id");

                    region = data.GetParameterValue("territory");

                    slot_name = data.GetParameterValue("slot_name");

                    region = data.GetParameterValue("territory");

                    object_id = data.GetParameterValue("object_id");

                    psn_id = data.GetParameterValue("psn_id");

                    instance_id = data.GetParameterValue("instance_id");

                    hex = data.GetParameterValue("hex");

                    __salt = data.GetParameterValue("__salt");

                    copyStream.Dispose();
                }

                byte[] clientresponse = sign("{ \"heartbeat\": true }");

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in Heartbeat while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        private static async Task GetConfig(HttpListenerContext context, bool get, string userAgent)
        {
            try
            {
                /*if (get)
                {
                    string id = "";
                    string region = "";
                    string territory = "";

                    string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                    // Get the input stream from the context
                    Stream inputStream = context.Request.InputStream;

                    // Create a memory stream to copy the content
                    using (MemoryStream copyStream = new MemoryStream())
                    {
                        // Copy the input stream to the memory stream
                        inputStream.CopyTo(copyStream);

                        // Reset the position of the copy stream to the beginning
                        copyStream.Position = 0;

                        var data = MultipartFormDataParser.Parse(copyStream, boundary);

                        // Reset the position of the copy stream to the beginning
                        copyStream.Position = 0;

                        id = data.GetParameterValue("id");

                        region = data.GetParameterValue("region");

                        territory = data.GetParameterValue("territory");

                        copyStream.Dispose();
                    }
                }*/

                if (!get)
                {
                    string id = "";

                    string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                    // Get the input stream from the context
                    Stream inputStream = context.Request.InputStream;

                    // Create a memory stream to copy the content
                    using (MemoryStream copyStream = new MemoryStream())
                    {
                        // Copy the input stream to the memory stream
                        inputStream.CopyTo(copyStream);

                        // Reset the position of the copy stream to the beginning
                        copyStream.Position = 0;

                        var data = MultipartFormDataParser.Parse(copyStream, boundary);

                        id = data.GetParameterValue("id");

                        copyStream.Dispose();
                    }

                    Console.WriteLine($"VEEMEE Server : Getconfig values : id|{id}");
                }

                byte[] clientresponse = sign(File.ReadAllText(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_VEEMEE/Acorn_Medow/stats_config.json"));

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in GetConfig while processing GET/POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        private static async Task crash(HttpListenerContext context, string userAgent)
        {
            try
            {
                string corehook = "";
                string territory = "";
                string region = "";
                string psnid = "";
                string scene = "";
                string sceneid = "";
                string scenetime = "";
                string sceneowner = "";
                string owner = "";
                string owned = "";
                string crash = "";
                string numplayers = "";
                string numpeople = "";
                string objectid = "";
                string objectname = "";

                string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                // Get the input stream from the context
                Stream inputStream = context.Request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    corehook = data.GetParameterValue("corehook");

                    territory = data.GetParameterValue("territory");

                    region = data.GetParameterValue("region");

                    psnid = data.GetParameterValue("psnid");

                    scene = data.GetParameterValue("scene");

                    sceneid = data.GetParameterValue("sceneid");

                    scenetime = data.GetParameterValue("scenetime");

                    sceneowner = data.GetParameterValue("sceneowner");

                    owner = data.GetParameterValue("owner");

                    owned = data.GetParameterValue("owned");

                    crash = data.GetParameterValue("crash");

                    numplayers = data.GetParameterValue("numplayers");

                    numpeople = data.GetParameterValue("numpeople");

                    objectid = data.GetParameterValue("objectid");

                    objectname = data.GetParameterValue("objectname");

                    copyStream.Dispose();
                }

                Console.WriteLine($"VEEMEE Server : A Crash Happen - issued by {userAgent} - Details : corehook|{corehook} - territory|{territory} - region|{region} - psnid|{psnid}" +
                    $" - scene|{scene} - sceneid|{sceneid} - scenetime|{scenetime} - sceneowner|{sceneowner} - owner|{owner} - owned|{owned} - crash|{crash} - numplayers|{numplayers} - numpeople|{numpeople} - objectid|{objectid} - objectname|{objectname}");

                byte[] clientresponse = sign(File.ReadAllText(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_VEEMEE/Acorn_Medow/stats_config.json"));

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in crash while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        private static async Task Usage(HttpListenerContext context, string userAgent)
        {
            try
            {
                string usage = "";

                string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                // Get the input stream from the context
                Stream inputStream = context.Request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    usage = data.GetParameterValue("usage");

                    copyStream.Dispose();
                }

                byte[] clientresponse = sign(usage);

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in Usage while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }
        private static async Task ReadConfig(HttpListenerContext context, string userAgent)
        {
            try
            {
                string config = "";
                string product = "";

                string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                // Get the input stream from the context
                Stream inputStream = context.Request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    config = data.GetParameterValue("config");
                    product = data.GetParameterValue("product");

                    copyStream.Dispose();
                }

                Console.WriteLine($"VEEMEE Server : Readconfig values : config|{config} - product|{product}");

                string configValue = "{}"; // Default response when config field doesn't exist

                if (!string.IsNullOrEmpty(config) && !string.IsNullOrEmpty(product))
                {
                    string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_VEEMEE/Acorn_Medow/config.json");

                    if (File.Exists(jsonFilePath))
                    {
                        dynamic jsondata = JsonConvert.DeserializeObject(File.ReadAllText(jsonFilePath));

                        if (jsondata.ContainsKey(product) && jsondata[product].ContainsKey(config))
                        {
                            configValue = jsondata[product][config].ToString();
                        }
                    }
                }

                byte[] clientresponse = sign(configValue);

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in ReadConfig while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }
        private static async Task ReadTable(HttpListenerContext context, string userAgent)
        {
            try
            {
                string psnid = "";
                string product = "";
                string hex = "";
                string __salt = "";

                string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                // Get the input stream from the context
                Stream inputStream = context.Request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    psnid = data.GetParameterValue("psnid");

                    product = data.GetParameterValue("product");

                    hex = data.GetParameterValue("hex");

                    __salt = data.GetParameterValue("__salt");

                    copyStream.Dispose();
                }

                byte[] clientresponse = VEEMEEProfileManager.ReadProfile(psnid, product, hex, __salt);

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in ReadTable while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }
        private static async Task WriteTable(HttpListenerContext context, string userAgent)
        {
            try
            {
                string psnid = "";

                string profile = "";

                string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                // Get the input stream from the context
                Stream inputStream = context.Request.InputStream;

                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new MemoryStream())
                {
                    // Copy the input stream to the memory stream
                    inputStream.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    psnid = data.GetParameterValue("psnid");

                    profile = data.GetParameterValue("profile");

                    copyStream.Dispose();
                }

                byte[] clientresponse = VEEMEEProfileManager.WriteProfile(psnid, profile);

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"VEEMEE Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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

                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in WriteTable while processing POST request : {ex}");

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

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        public static string Sha1Hash(string data)
        {
            try
            {
                string salt = "veemeeHTTPRequ9R3UMWDAT8F3*#@&$^";

                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] hashBytes = sha1.ComputeHash(Misc.ConcatenateByteArrays(Encoding.UTF8.GetBytes(salt), Encoding.UTF8.GetBytes(data)));

                    sha1.Dispose();

                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in Sha1Hash : {ex}");

                return "ERROR in Sha1Hash";
            }
        }

        public static byte[] sign(string jsonData)
        {
            try
            {
                string formattedJson = JToken.Parse(jsonData.Replace("\n", "")).ToString(Formatting.None);

                string hash = Sha1Hash(formattedJson).ToUpper();

                JToken token = JToken.Parse(formattedJson);

                if (token.Type == JTokenType.Object)
                {
                    JObject obj = (JObject)token;
                    obj["hash"] = hash;
                    formattedJson = obj.ToString(Formatting.None);
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray array = (JArray)token;
                    JObject obj = new JObject();
                    obj["hash"] = hash;
                    array.Add(obj);
                    formattedJson = array.ToString(Formatting.None);
                }

                return Encoding.UTF8.GetBytes(formattedJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server : has throw an exception in sign : {ex}");

                return Encoding.UTF8.GetBytes("ERROR in sign");
            }
        }

        private static string UpdateSlot(string directoryPath, string psn_id, int slot_num, bool removemode)
        {
            try
            {
                bool found = false;

                if (slot_num != 0 && removemode)
                {
                    if (File.Exists(directoryPath + $"{slot_num}.xml"))
                    {
                        XDocument slotDocument = XDocument.Load(directoryPath + $"{slot_num}.xml");
                        XElement slotElement = slotDocument.Root;

                        string slotValue = slotElement.Element("slot")?.Value;
                        string expirationDate = slotElement.Element("expiration")?.Value;

                        if (slotValue == psn_id)
                        {
                            slotElement.Element("slot").Value = "unnocupied";
                            slotElement.Element("expiration").Value = "01/01/1970 00:00:00";
                            slotDocument.Save(directoryPath + $"{slot_num}.xml");

                            return "true";
                        }
                    }
                }

                if (slot_num == 0)
                {
                    string[] slotFiles;

                    slotFiles = Directory.GetFiles(directoryPath, "*.xml");
                    Array.Sort(slotFiles, new SlotFileComparer()); // Sort the array using the custom comparer

                    foreach (string slotFile in slotFiles)
                    {
                        if (File.Exists(slotFile) && !removemode)
                        {
                            XDocument slotDocument = XDocument.Load(slotFile);
                            XElement slotElement = slotDocument.Root;

                            string slotValue = slotElement.Element("slot")?.Value;
                            string expirationDate = slotElement.Element("expiration")?.Value;

                            DateTime expirationDatefromxml = DateTime.Parse(expirationDate);

                            if (slotValue == psn_id && DateTime.Now < expirationDatefromxml)
                            {
                                return Path.GetFileNameWithoutExtension(slotFile);
                            }

                            if (slotValue == "unnocupied" || (slotValue != "unnocupied" && expirationDatefromxml <= DateTime.Now))
                            {
                                slotElement.Element("slot").Value = psn_id;
                                slotElement.Element("expiration").Value = DateTime.Now.AddSeconds(45).ToString();
                                slotDocument.Save(slotFile);

                                return Path.GetFileNameWithoutExtension(slotFile);
                            }
                        }
                        else if (File.Exists(slotFile) && removemode)
                        {
                            XDocument slotDocument = XDocument.Load(slotFile);
                            XElement slotElement = slotDocument.Root;

                            string slotValue = slotElement.Element("slot")?.Value;
                            string expirationDate = slotElement.Element("expiration")?.Value;

                            if (slotValue == psn_id)
                            {
                                slotElement.Element("slot").Value = "unnocupied";
                                slotElement.Element("expiration").Value = "01/01/1970 00:00:00";
                                slotDocument.Save(slotFile);

                                found = true;
                            }
                        }
                    }
                }

                if (!found)
                {
                    return "false";
                }
                else
                {
                    return "true";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server: has thrown an exception in UpdateSlot: {ex}");

                return "false";
            }
        }
    }
    public class VEEMEELoginCounter
    {
        private Dictionary<string, int> loginCounts;

        public VEEMEELoginCounter()
        {
            loginCounts = new Dictionary<string, int>();
        }

        public void ProcessLogin(string username)
        {
            if (loginCounts.ContainsKey(username))
            {
                loginCounts[username]++;
            }
            else
            {
                loginCounts.Add(username, 1);
            }
        }

        public int GetLoginCount(string username)
        {
            if (loginCounts.ContainsKey(username))
            {
                return loginCounts[username];
            }

            return 0;
        }
    }
    
    public static class VEEMEEProfileManager
    {
        public static byte[] ReadProfile(string psnid, string product, string hex, string salt)
        {
            try
            {
                if (hex == null || salt == null)
                {
                    return Encoding.UTF8.GetBytes("No Access.");
                }

                string reader = "";

                if (File.Exists(Directory.GetCurrentDirectory() + $"/loginformNtemplates/HOME_VEEMEE/Acorn_Medow/User_Profiles/{psnid}.json"))
                {
                    reader = File.ReadAllText(Directory.GetCurrentDirectory() + $"/loginformNtemplates/HOME_VEEMEE/Acorn_Medow/User_Profiles/{psnid}.json");
                }
                else
                {
                    reader = File.ReadAllText(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_VEEMEE/Acorn_Medow/default_profile.json");
                }

                return VEEMEEservices.sign(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server: An exception occurred in VEEMEEProfileManager.ReadProfile: {ex}");

                return Encoding.UTF8.GetBytes("ERROR in ReadProfile");
            }
        }

        public static byte[] WriteProfile(string psnid, string profile)
        {
            try
            {
                File.WriteAllText(Directory.GetCurrentDirectory() + $"/loginformNtemplates/HOME_VEEMEE/Acorn_Medow/User_Profiles/{psnid}.json", profile);

                return VEEMEEservices.sign(profile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VEEMEE Server: An exception occurred in VEEMEEProfileManager.WriteProfile: {ex}");

                return Encoding.UTF8.GetBytes("ERROR in WriteProfile");
            }
        }
    }
}
