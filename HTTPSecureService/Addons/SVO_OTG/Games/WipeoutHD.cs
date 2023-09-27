using System.Net;
using System.Text;
using System.Web;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.SVO_OTG.Games
{
    public class WipeoutHD
    {
        public static async void WipeoutHD_OTG(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            if (request.Url == null)
            {
                response.SetBegin((int)HttpStatusCode.Forbidden);
                response.SetBody();
                return;
            }

            switch (HTTPSClass.RemoveQueryString(request.Url))
            {
                #region Wipeout
                case "/wox_ws/rest/account/TicketLogin":

                    switch (request.Method)
                    {
                        case "POST":

                            string signature = "";

                            string signatureClass = "";

                            string userContext = "";

                            string languageId = "";

                            string timeZone = "";

                            string psnname = "";

                            int accountId = -1;

                            string url = request.Url.ToString();

                            string[] urlParts = url.Split('?');
                            string basePath = urlParts[0];
                            string queryString = urlParts.Length > 1 ? urlParts[1] : string.Empty;

                            string[] parameters = queryString.Split('&');

                            foreach (string parameter in parameters)
                            {
                                string[] parts = parameter.Split('=');

                                string key = Uri.UnescapeDataString(parts[0]);
                                string value = Uri.UnescapeDataString(parts[1]);

                                if (key == "signature")
                                    signature = value;
                                else if (key == "signatureClass")
                                    signatureClass = value;
                                else if (key == "userContext")
                                    userContext = value;
                                else if (key == "languageId")
                                    languageId = value;
                                else if (key == "timeZone")
                                    timeZone = value;
                            }

                            // Create a byte array
                            byte[] buffer = request.BodyBytes;

                            // Extract the desired portion of the binary data
                            byte[] extractedData = new byte[0x63 - 0x54 + 1];

                            // Copy it
                            Array.Copy(buffer, 0x54, extractedData, 0, extractedData.Length);

                            // Convert 0x00 bytes to 0x20 so we pad as space.
                            for (int i = 0; i < extractedData.Length; i++)
                            {
                                if (extractedData[i] == 0x00)
                                    extractedData[i] = 0x20;
                            }

                            // Convert the modified data to a string
                            psnname = Encoding.ASCII.GetString(extractedData).Replace(" ", "");

                            if (Misc.FindbyteSequence(buffer, new byte[] { 0x52, 0x50, 0x43, 0x4E }))
                                ServerConfiguration.LogInfo($"[OTG - HTTPS - WipeoutHD] : User {psnname} logged in and is on RPCN");
                            else
                                ServerConfiguration.LogInfo($"[OTG - HTTPS - WipeoutHD] : User {psnname} logged in and is on PSN");

                            try
                            {
                                await ServerConfiguration.Database.GetAccountByName(psnname, 23360).ContinueWith((r) =>
                                {
                                    //Found in database so keep.
                                    string langId = SVOHTTPSClass.ExtractLanguageId(url).ToString();
                                    string accountName = r.Result.AccountName;
                                    accountId = r.Result.AccountId;
                                });
                            }
                            catch (Exception)
                            {
                                string langId = SVOHTTPSClass.ExtractLanguageId(url).ToString();
                                accountId = 0;
                            }

                            response.SetHeader("Set-Cookie", $"id=ddb4fac6-f908-33e5-80f9-febd2e2ef58f; Path=/");
                            response.SetHeader("Set-Cookie", $"name={psnname}; Path=/");
                            response.SetHeader("Set-Cookie", $"authKey=2b8e1723-9e40-41e6-a740-05ddefacfe94; Path=/");
                            response.SetHeader("Set-Cookie", $"timeZone=GMT; Path=/");
                            response.SetHeader("Set-Cookie", $"signature=ghpE-ws_dBmIY-WNbkCQb1NnamA; Path=/");
                            response.SetHeader("Content-Language", "");

                            response.SetContentType("application/xml;charset=UTF-8");

                            response.SetBegin((int)HttpStatusCode.OK);
                            response.SetBody("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                "<SP_Login>\r\n" +
                                "   <status> \r\n" +
                                "        <id>1</id> \r\n" +
                                "        <message>Success</message> \r\n" +
                                "   </status> \r\n" +
                                $"   <accountID>{accountId}</accountID>\r\n\t" +
                                $"   <languageID>{languageId}</languageID>\r\n" +
                                $"   <userContext>{userContext}</userContext> \r\n" +
                                "</SP_Login>");
                            break;
                    }
                    break;

                default:
                    response.SetBegin((int)HttpStatusCode.Forbidden);
                    response.SetBody();
                    break;
                    #endregion
            }
        }
    }
}
