using System.Net;
using HttpMultipartParser;
using System.Text;

namespace PSMultiServer.Addons.HOME
{
    public class CustomHomeContextProcess
    {
        public static async Task ProcessHomeTHQRequest(HttpListenerContext context, string userAgent)
        {
            string url = context.Request.Url.LocalPath;

            // Split the URL into segments
            string[] segments = url.Trim('/').Split('/');

            // Combine the folder segments into a directory path
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/", string.Join("/", segments.Take(segments.Length - 1).ToArray()));

            if (context.Request.Headers["Host"] != null)
            {
                if (context.Request.Headers["Host"] == "sonyhome.thqsandbox.com") // THQ Home servers
                {
                    if (context.Request.ContentType.StartsWith("multipart/form-data"))
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

                                Console.WriteLine($"HTTP Server : {userAgent} issued a HOME THQ request : index.php");

                                try
                                {
                                    if (!Directory.Exists(directoryPath + $"/HOME_THQ/{Encoding.UTF8.GetString(id)}"))
                                    {
                                        Directory.CreateDirectory(directoryPath + $"/HOME_THQ/{Encoding.UTF8.GetString(id)}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"HTTP Server : thrown an exception in ProcessRequest while processing the HOME THQ index.php POST request and creating the directory : {ex}");

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
                                        context.Response.KeepAlive = true;
                                        context.Response.ContentEncoding = Encoding.UTF8;
                                        context.Response.Headers.Add("Connection", "Keep-Alive");
                                        context.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                                        context.Response.ContentLength64 = responsetooutput.Length;
                                        context.Response.OutputStream.Write(responsetooutput, 0, responsetooutput.Length);
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

                                copyStream.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"HTTP Server : thrown an exception in ProcessRequest while processing the OHS POST request and creating the file/http response : {ex}");

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
                    }
                    else
                    {
                        Console.WriteLine($"HTTP Server : {userAgent} tried to POST /index.php, but it's not correct so we forbid.");

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
                    Console.WriteLine($"HTTP Server : {userAgent} tried to POST /index.php, but it's not correct so we forbid.");

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
                Console.WriteLine($"HTTP Server : {userAgent} tried to POST /index.php, but it's not correct so we forbid.");

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

            return;
        }
        public static async Task ProcessOHSRequest(HttpListenerContext context, string userAgent)
        {
            // Process the request based on the HTTP method
            string filePath = Path.Combine(Directory.GetCurrentDirectory() + "/wwwroot/", context.Request.Url.LocalPath.Substring(1));

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
                        Console.WriteLine($"HTTP Server : thrown an exception in ProcessRequest while processing the OHS POST request and creating the directory : {ex}");

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

                        Console.WriteLine($"HTTP Server : {userAgent} issued a OHS request : Version - {data.GetParameterValue("version")}");

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
                        Console.WriteLine($"HTTP Server : thrown an exception in ProcessRequest while processing the OHS POST request and creating the file/http response : {ex}");

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
                    Console.WriteLine($"HTTP Server : {userAgent} tried to POST data to our OHS, but it's not correct so we forbid.");

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
                Console.WriteLine($"HTTP Server : {userAgent} tried to POST data to our OHS, but it's not correct so we forbid.");

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

        public static void PrepareHomeFolders()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_THQ/Server_Template/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_THQ/Server_Template/");
            }

            if (!File.Exists(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_THQ/Server_Template/get_template.xml"))
            {
                File.WriteAllText(Directory.GetCurrentDirectory() + "/loginformNtemplates/HOME_THQ/Server_Template/get_template.xml", "<xml></xml>");
            }

            return;
        }
    }
}
