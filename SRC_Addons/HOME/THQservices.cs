using HttpMultipartParser;
using System.Net;
using System.Text;

namespace PSMultiServer.SRC_Addons.HOME
{
    public class THQservices
    {
        public static async Task ProcessHomeTHQRequest(HttpListenerContext context, string userAgent)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // Extract the HTTP method and the relative path
                string httpMethod = request.HttpMethod;
                string url = request.Url.LocalPath;

                Console.WriteLine($"THQ Server : Received {httpMethod} request for {url}");

                // Split the URL into segments
                string[] segments = url.Trim('/').Split('/');

                // Combine the folder segments into a directory path
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/", string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                // Process the request based on the HTTP method
                string filePath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/", url.Substring(1));

                switch (httpMethod)
                {
                    case "POST":

                        if (request.Url.AbsolutePath == "/index.php" && context.Request.ContentType.StartsWith("multipart/form-data"))
                        {
                            try
                            {
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

                                    byte[] ticketData = ExtractUFCTicketData(copyStream, boundary);

                                    byte[] functooutput = Encoding.UTF8.GetBytes(data.GetParameterValue("func"));

                                    byte[] id = Encoding.UTF8.GetBytes(data.GetParameterValue("id"));

                                    byte[] responsetooutput = File.ReadAllBytes(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_THQ/Server_Template/get_template.xml");

                                    Console.WriteLine($"THQ Server : {userAgent} issued a HOME THQ request : index.php");

                                    try
                                    {
                                        if (!Directory.Exists(directoryPath + $"/HOME_THQ/{Encoding.UTF8.GetString(id)}"))
                                        {
                                            Directory.CreateDirectory(directoryPath + $"/HOME_THQ/{Encoding.UTF8.GetString(id)}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"THQ Server : thrown an exception in ProcessRequest while processing the HOME THQ index.php POST request and creating the directory : {ex}");

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
                                    }

                                    using (FileStream fs = new FileStream($"./wwwroot/HOME_THQ/{Encoding.UTF8.GetString(id)}/ticket.bin", FileMode.Create))
                                    {
                                        fs.Write(ticketData, 0, ticketData.Length);
                                        fs.Flush();
                                        fs.Dispose();

                                        Console.WriteLine($"File {$"./wwwroot/HOME_THQ/{Encoding.UTF8.GetString(id)}/ticket.bin"} has been uploaded to HTTP");
                                    }

                                    if (context.Response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            response.KeepAlive = true;
                                            response.ContentEncoding = Encoding.UTF8;
                                            response.Headers.Add("Connection", "Keep-Alive");
                                            response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                            response.StatusCode = (int)HttpStatusCode.OK;
                                            response.ContentLength64 = responsetooutput.Length;
                                            response.OutputStream.Write(responsetooutput, 0, responsetooutput.Length);
                                            response.OutputStream.Close();
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");

                                            response.Close();
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");

                                        response.Close();
                                    }

                                    copyStream.Dispose();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"THQ Server : thrown an exception in ProcessRequest while processing the OHS POST request and creating the file/http response : {ex}");

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

                                    Console.WriteLine($"THQ Server : Method {filePath} - {httpMethod} not found");
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
                        }

                        break;

                    default:

                        try
                        {
                            Console.WriteLine($"THQ Server : WARNING - Host requested a method I don't know about!! Report it to GITHUB with the request : {httpMethod} request for {url} is not supported");

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

                            response.Close();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"THQ Server : has throw an exception in ProcessRequest while processing the default request : {ex}");

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
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"THQ Server : an error occured in ProcessRequest - {ex}");

                context.Response.Close();
            }

            GC.Collect();

            return;
        }

        private static byte[] ExtractUFCTicketData(Stream inputStream, string boundary)
        {
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("--" + boundary);
            byte[] endBoundaryBytes = Encoding.ASCII.GetBytes("--" + boundary + "--");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryReader reader = new BinaryReader(inputStream))
                {
                    using (BinaryWriter writer = new BinaryWriter(memoryStream))
                    {
                        bool isTicketData = false;
                        bool isReadingData = false;

                        byte[] lineBytes;
                        while ((lineBytes = HTTPserver.ReadLine(reader)) != null)
                        {
                            if (HTTPserver.ByteArrayStartsWith(lineBytes, boundaryBytes))
                            {
                                if (isReadingData)
                                    break;

                                isTicketData = false;
                                isReadingData = false;
                            }
                            else if (HTTPserver.ByteArrayStartsWith(lineBytes, endBoundaryBytes))
                            {
                                break;
                            }
                            else if (isTicketData && isReadingData)
                            {
                                writer.Write(lineBytes);
                            }
                            else if (HTTPserver.ByteArrayStartsWith(lineBytes, Encoding.ASCII.GetBytes("Content-Disposition: form-data; name=\"ticket\"; filename=\"ticket.bin\"")))
                            {
                                isTicketData = true;
                            }
                            else if (HTTPserver.ByteArrayStartsWith(lineBytes, Encoding.ASCII.GetBytes("Content-Type: application/octet-stream")))
                            {
                                isReadingData = true;
                            }
                        }

                        return memoryStream.ToArray();
                    }
                }
            }
        }
    }
}
