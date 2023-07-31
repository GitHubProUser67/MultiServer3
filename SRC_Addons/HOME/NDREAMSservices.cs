using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PSMultiServer.Addons.HOME
{
    public class NDREAMSservices
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

                Console.WriteLine($"NDREAMS Server : Received {httpMethod} request for {url}");

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

                                    Console.WriteLine($"NDREAMS Server : Method {filePath} - {httpMethod} not found");
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
                            Console.WriteLine($"NDREAMS Server : has throw an exception in ProcessRequest while processing GET request : {ex}");

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
                            if (request.Url.AbsolutePath == "/xi2/cont/battle_cont.php")
                            {
                                Task.Run(() => battle_cont(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath == "/xi2/cont/articles_cont.php")
                            {
                                Task.Run(() => articles_cont(context, userAgent));
                            }
                            else if (request.Url.AbsolutePath == "/xi2/cont/xi2_cont.php")
                            {
                                Task.Run(() => xi2_cont(context, userAgent));
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

                                        Console.WriteLine($"NDREAMS Server : Method {filePath} - {httpMethod} not found");
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
                            Console.WriteLine($"NDREAMS Server : has throw an exception in ProcessRequest while processing POST request : {ex}");

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
                            Console.WriteLine($"NDREAMS Server : WARNING - Host requested a method I don't know about!! Report it to GITHUB with the request : {httpMethod} request for {url} is not supported");

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
                            Console.WriteLine($"NDREAMS Server : has throw an exception in ProcessRequest while processing the default request : {ex}");

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
                Console.WriteLine($"NDREAMS Server : an error occured in ProcessRequest - {ex}");

                context.Response.Close();

                GC.Collect();
            }

            return;
        }

        private static async Task battle_cont(HttpListenerContext context, string userAgent)
        {
            try
            {
                string func = "";
                string name = "";
                string key = "";

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

                    func = data.GetParameterValue("func");

                    name = data.GetParameterValue("name");

                    key = data.GetParameterValue("key");

                    copyStream.Dispose();
                }

                byte[] clientresponse = Encoding.UTF8.GetBytes("<XML></XML>");

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/xml; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"NDREAMS Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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
                Console.WriteLine($"NDREAMS Server : has throw an exception in battle_cont while processing POST request : {ex}");

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
        private static async Task articles_cont(HttpListenerContext context, string userAgent)
        {
            try
            {
                string func = "";
                string name = "";
                string key = "";

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

                    func = data.GetParameterValue("func");

                    name = data.GetParameterValue("name");

                    key = data.GetParameterValue("key");

                    copyStream.Dispose();
                }

                byte[] clientresponse = Encoding.UTF8.GetBytes("<XML></XML>");

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/xml; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"NDREAMS Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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
                Console.WriteLine($"NDREAMS Server : has throw an exception in articles_cont while processing POST request : {ex}");

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
        private static async Task xi2_cont(HttpListenerContext context, string userAgent)
        {
            try
            {
                string territory = "";
                string func = "";
                string name = "";
                string region = "";
                string key = "";

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

                    territory = data.GetParameterValue("territory");

                    func = data.GetParameterValue("func");

                    name = data.GetParameterValue("name");

                    region = data.GetParameterValue("region");

                    key = data.GetParameterValue("key");

                    copyStream.Dispose();
                }

                byte[] clientresponse = Encoding.UTF8.GetBytes("<XML></XML>");

                if (context.Response.OutputStream.CanWrite)
                {
                    try
                    {
                        context.Response.ContentType = "text/xml; charset=utf-8";
                        context.Response.ContentLength64 = clientresponse.Length;
                        context.Response.KeepAlive = true;
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        context.Response.OutputStream.Close();

                        Console.WriteLine($"NDREAMS Server : Returned response for {userAgent} - {Encoding.UTF8.GetString(clientresponse)}");
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
                Console.WriteLine($"NDREAMS Server : has throw an exception in xi2_cont while processing POST request : {ex}");

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
    }
}
