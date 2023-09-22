using HttpMultipartParser;
using System.Net;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.HELLFIREGAMES
{
    public class HOMETYCOONMain
    {
        public static async Task Main(HttpListenerRequest request, HttpListenerResponse response)
        {
            string phpSessionId = "";

            try
            {
                // Get the query string parameters
                phpSessionId = request.QueryString["PHPSESSID"];
            }
            catch (Exception)
            {
                phpSessionId = "hi";
            }

            string Command = "";

            string UserID = "";

            string DisplayName = "";

            string InstanceID = "";

            string Region = "";

            string boundary = Extensions.ExtractBoundary(request.ContentType);

            // Get the input stream from the context
            Stream inputStream = request.InputStream;

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

                Command = data.GetParameterValue("Command");

                UserID = data.GetParameterValue("UserID");

                try
                {
                    DisplayName = data.GetParameterValue("DisplayName");
                }
                catch (Exception)
                {

                }

                try
                {
                    InstanceID = data.GetParameterValue("InstanceID");
                }
                catch (Exception)
                {

                }

                try
                {
                    Region = data.GetParameterValue("Region");
                }
                catch (Exception)
                {

                }

                byte[] responsetooutput = Encoding.UTF8.GetBytes("<Response></Response>");

                switch (Command)
                {
                    case "QueryMotd":
                        responsetooutput = Encoding.UTF8.GetBytes($"<Response><Motd>MOTD</Motd></Response>");
                        break;
                    case "RequestNPTicket":
                        responsetooutput = HOMETYCOONProcessor.RequestNPTicket(boundary, copyStream);
                        break;
                    case "RequestTownInstance":
                        responsetooutput = HOMETYCOONProcessor.RequestTownInstance(UserID, DisplayName, phpSessionId);
                        break;
                    case "QueryServerGlobals":
                        responsetooutput = HOMETYCOONProcessor.QueryServerGlobals(UserID);
                        break;
                    case "QueryPrices":
                        responsetooutput = HOMETYCOONProcessor.QueryPrices();
                        break;
                    case "QueryBoosters":
                        responsetooutput = HOMETYCOONProcessor.QueryBoosters();
                        break;
                    case "QueryHoldbacks":
                        responsetooutput = HOMETYCOONProcessor.QueryHoldbacks();
                        break;
                    case "QueryRewards":
                        responsetooutput = HOMETYCOONProcessor.QueryRewards(UserID);
                        break;
                    case "QueryGifts":
                        responsetooutput = await HOMETYCOONProcessor.QueryGifts(UserID);
                        break;
                    case "RequestTown":
                        responsetooutput = await HOMETYCOONProcessor.RequestTown(UserID, InstanceID);
                        break;
                    case "RequestUser":
                        responsetooutput = await HOMETYCOONProcessor.RequestUser(UserID);
                        break;
                    case "RequestVisitingUser":
                        responsetooutput = await HOMETYCOONProcessor.RequestUser(UserID);
                        break;
                    case "RequestUserTowns":
                        responsetooutput = Encoding.UTF8.GetBytes($"<Response><MyTown></MyTown></Response>");
                        break;
                    case "UpdateTownTime":
                        responsetooutput = Encoding.UTF8.GetBytes($"<Response></Response>");
                        break;
                    case "UpdateUser":
                        responsetooutput = await HOMETYCOONProcessor.UpdateUser(UserID, data);
                        break;
                    case "CreateBuilding":
                        responsetooutput = await HOMETYCOONProcessor.CreateBuilding(UserID, data);
                        break;
                    case "RemoveBuilding":
                        responsetooutput = await HOMETYCOONProcessor.RemoveBuilding(UserID, data);
                        break;
                    default:
                        responsetooutput = Encoding.UTF8.GetBytes($"<Response></Response>");
                        break;
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = responsetooutput.Length;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.OutputStream.Write(responsetooutput, 0, responsetooutput.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                }

                copyStream.Dispose();
            }
        }

        public static Task PostCards(HttpListenerRequest request, HttpListenerResponse response)
        {
            string phpSessionId = "";

            try
            {
                // Get the query string parameters
                phpSessionId = request.QueryString["PHPSESSID"];
            }
            catch (Exception)
            {
                phpSessionId = "hi";
            }

            // Now you have the value of the PHPSESSID parameter
            Console.WriteLine($"PHPSESSID value: {phpSessionId}");

            string Command = "";

            string UserID = "";

            string Version = "";

            string boundary = Extensions.ExtractBoundary(request.ContentType);

            // Get the input stream from the context
            Stream inputStream = request.InputStream;

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

                Command = data.GetParameterValue("Command");

                UserID = data.GetParameterValue("UserID");

                Version = data.GetParameterValue("Version");

                byte[] responsetooutput = Encoding.UTF8.GetBytes("<XML></XML>");

                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = responsetooutput.Length;
                        response.OutputStream.Write(responsetooutput, 0, responsetooutput.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                }

                copyStream.Dispose();
            }

            return Task.CompletedTask;
        }
    }
}
