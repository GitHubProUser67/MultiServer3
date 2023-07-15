using HttpMultipartParser;
using System.Net;
using System.Text;

namespace PSMultiServer.SRC_Addons.HOME
{
    public class OHSservices
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

                Console.WriteLine($"OHS : Received {httpMethod} request for {url}");

                // Split the URL into segments
                string[] segments = url.Trim('/').Split('/');

                // Combine the folder segments into a directory path
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/", string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                // Process the request based on the HTTP method
                string filePath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/", url.Substring(1));

                switch (httpMethod)
                {
                    case "POST":

                        try
                        {
                            if (request.Url.AbsolutePath.Contains("/batch"))
                            {
                                if (context.Request.ContentType != null)
                                {
                                    if (context.Request.ContentType.StartsWith("multipart/form-data"))
                                    {
                                        try
                                        {
                                            if (!Directory.Exists(filePath))
                                            {
                                                Directory.CreateDirectory(filePath);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"OHS Server : thrown an exception in ProcessRequest while processing the OHS POST request and creating the directory : {ex}");

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

                                        try
                                        {
                                            var data = MultipartFormDataParser.Parse(context.Request.InputStream, Misc.ExtractBoundary(context.Request.ContentType));

                                            byte[] datatooutput = Encoding.UTF8.GetBytes(data.GetParameterValue("data"));

                                            // Convert the bytes to a string
                                            string resultfirst8Bytes = Encoding.UTF8.GetString(datatooutput.Take(8).ToArray());

                                            byte[] postresponsetooutput = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                                $"<ohs value=\"{Encoding.UTF8.GetString(datatooutput)}\" >\r\n" +
                                                "</ohs>");

                                            Console.WriteLine($"OHS Server : {userAgent} issued a OHS request : Version - {data.GetParameterValue("version")}");

                                            using (FileStream fs = new FileStream($"./wwwroot{context.Request.Url.AbsolutePath}{resultfirst8Bytes}.xml", FileMode.Create))
                                            {
                                                fs.Write(postresponsetooutput, 0, postresponsetooutput.Length);
                                                fs.Flush();
                                                fs.Dispose();

                                                Console.WriteLine($"File {$"./wwwroot{context.Request.Url.AbsolutePath}{resultfirst8Bytes}.xml"} has been uploaded to HTTP");
                                            }

                                            if (context.Response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    context.Response.KeepAlive = true;
                                                    context.Response.ContentEncoding = Encoding.UTF8;
                                                    context.Response.Headers.Add("Connection", "Keep-Alive");
                                                    context.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                                                    context.Response.ContentLength64 = postresponsetooutput.Length;
                                                    context.Response.OutputStream.Write(postresponsetooutput, 0, postresponsetooutput.Length);
                                                    context.Response.OutputStream.Close();
                                                }
                                                catch (Exception ex1)
                                                {
                                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");

                                                    context.Response.Close();
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Client Disconnected early");

                                                context.Response.Close();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"OHS Server : thrown an exception in ProcessRequest while processing the OHS POST request and creating the file/http response : {ex}");

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

                                        // OHS is a keep-alive type of packet, so we never close anything.

                                        //context.Response.Close();

                                        // TODO : do the OHS stuff.
                                    }
                                    else
                                    {
                                        Console.WriteLine($"OHS Server : {userAgent} tried to POST data to our OHS, but it's not correct so we forbid.");

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
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"OHS Server : {userAgent} tried to POST data to our OHS, but it's not correct so we forbid.");

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
                                }
                            }
                            else
                            {
                                // Return a not found response
                                byte[] notFoundResponse = Encoding.UTF8.GetBytes("Method not found");

                                if (response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        response.StatusCode = (int)HttpStatusCode.NotFound;
                                        response.ContentLength64 = notFoundResponse.Length;
                                        response.OutputStream.Write(notFoundResponse, 0, notFoundResponse.Length);
                                        response.OutputStream.Close();

                                        Console.WriteLine($"VEEMEE Method {filePath} - {httpMethod} not found");
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
                            Console.WriteLine($"OHS Server has throw an exception in ProcessRequest while processing POST request : {ex}");

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
                            Console.WriteLine($"OHS WARNING - Host requested a method I don't know about!! Report it to GITHUB with the request : {httpMethod} request for {url} is not supported");

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
                        catch (Exception ex)
                        {
                            Console.WriteLine($"OHS Server has throw an exception in ProcessRequest while processing the default request : {ex}");

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
            catch (Exception ex)
            {
                Console.WriteLine($"OHS Server : an error occured in ProcessRequest - {ex}");

                context.Response.Close();
            }

            GC.Collect();

            return;
        }
    }
}
