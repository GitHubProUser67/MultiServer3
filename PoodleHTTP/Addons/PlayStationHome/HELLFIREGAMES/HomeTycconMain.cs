using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using PSMultiServer.PoodleHTTP.Addons.PlayStationHome.SSFW;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.HELLFIREGAMES
{
    public class HomeTycconMain
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

            string GoldCoins = "";

            string Options = "";

            string Workers = "";

            string TotalCollected = "";

            string Wallet = "";

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
                    case "VersionCheck":
                        responsetooutput = Encoding.UTF8.GetBytes("<Response><URL>http://game2.hellfiregames.com/HomeTycoon</URL></Response>");
                        break;
                    case "QueryMotd":
                        responsetooutput = Encoding.UTF8.GetBytes($"<Response><Motd>MOTD</Motd></Response>");
                        break;
                    case "RequestNPTicket":
                        responsetooutput = await HomeTycoonProcessor.RequestNPTicket(boundary, copyStream);
                        break;
                    case "RequestTownInstance":
                        responsetooutput = await HomeTycoonProcessor.RequestTownInstance(UserID, DisplayName, phpSessionId);
                        break;
                    case "QueryServerGlobals":
                        responsetooutput = await HomeTycoonProcessor.QueryServerGlobals(UserID);
                        break;
                    case "QueryPrices":
                        responsetooutput = await HomeTycoonProcessor.QueryPrices();
                        break;
                    case "QueryBoosters":
                        responsetooutput = await HomeTycoonProcessor.QueryBoosters();
                        break;
                    case "QueryHoldbacks":
                        responsetooutput = await HomeTycoonProcessor.QueryHoldbacks();
                        break;
                    case "QueryRewards":
                        responsetooutput = await HomeTycoonProcessor.QueryRewards(UserID);
                        break;
                    case "QueryGifts":
                        responsetooutput = await HomeTycoonProcessor.QueryGifts(UserID);
                        break;
                    case "RequestTown":
                        responsetooutput = await HomeTycoonProcessor.RequestTown(UserID, InstanceID);
                        break;
                    case "RequestUser":
                        responsetooutput = await HomeTycoonProcessor.RequestUser(UserID);
                        break;
                    case "RequestVisitingUser":
                        responsetooutput = await HomeTycoonProcessor.RequestUser(UserID);
                        break;
                    case "RequestUserTowns":
                        responsetooutput = Encoding.UTF8.GetBytes($"<Response><MyTown></MyTown></Response>");
                        break;
                    case "UpdateTownTime":
                        responsetooutput = Encoding.UTF8.GetBytes($"<Response></Response>");
                        break;
                    case "UpdateUser":
                        responsetooutput = await HomeTycoonProcessor.UpdateUser(UserID, data);
                        break;
                    case "CreateBuilding":
                        responsetooutput = await HomeTycoonProcessor.CreateBuilding(UserID, data);
                        break;
                    case "RemoveBuilding":
                        responsetooutput = await HomeTycoonProcessor.RemoveBuilding(UserID, data);
                        break;
                    default:
                        responsetooutput = Encoding.UTF8.GetBytes($"<Response></Response>");
                        break;
                }

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
        }

        public static async Task PostCards(HttpListenerRequest request, HttpListenerResponse response)
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
        }
    }
}
