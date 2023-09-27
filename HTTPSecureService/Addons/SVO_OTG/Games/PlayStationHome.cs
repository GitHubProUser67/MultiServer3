using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using MultiServer.HTTPService.Addons.SVO;
using NetCoreServer;
using System.Security.Policy;

namespace MultiServer.HTTPSecureService.Addons.SVO_OTG.Games
{
    public class PlayStationHome
    {
        public static async void Home_SVO(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            if (request.Url == null)
            {
                response.SetBegin((int)HttpStatusCode.Forbidden);
                response.SetBody();
                return;
            }

            switch (HTTPSClass.RemoveQueryString(request.Url))
            {
                #region HOME
                case "/HUBPS3_SVML/account/SP_Login_Submit.jsp":
                    switch (request.Method)
                    {
                        case "POST":

                            string clientMac = HTTPSClass.GetHeaderValue(Headers, "X-SVOMac");

                            string serverMac = SVOProcessor.CalcuateSVOMac(clientMac);

                            if (serverMac == null)
                            {
                                response.SetBegin((int)HttpStatusCode.Forbidden);
                                response.SetBody();
                                return;
                            }
                            else
                            {
                                int appId = Convert.ToInt32(HttpUtility.ParseQueryString(HTTPSClass.GetQueryFromUri(request.Url)).Get("applicationID"));

                                if (request.BodyLength <= 0)
                                {
                                    response.SetBegin((int)HttpStatusCode.Forbidden);
                                    response.SetBody();
                                    return;
                                }

                                response.SetContentType("text/xml");

                                // Convert the data to a string and display it on the console.
                                string s = request.Body;

                                byte[] bytes = Encoding.ASCII.GetBytes(s);
                                int AcctNameLen = Convert.ToInt32(bytes.GetValue(81));

                                string acctName = s.Substring(82, 32);

                                string acctNameREX = Regex.Replace(acctName, @"[^a-zA-Z0-9]+", string.Empty);

                                ServerConfiguration.LogInfo($"Logging user {acctNameREX} into SVO...\n");

                                response.SetHeader("X-SVOMac", serverMac);

                                string sig = HttpUtility.ParseQueryString(HTTPSClass.GetQueryFromUri(request.Url)).Get("sig");

                                int accountId = -1;

                                string langId = "0";

                                await ServerConfiguration.Database.GetAccountByName(acctNameREX, appId).ContinueWith((r) =>
                                {
                                    //Found in database so keep.
                                    string langId = SVOHTTPSClass.ExtractLanguageId(request.Url).ToString();
                                    string accountName = r.Result.AccountName;
                                    accountId = r.Result.AccountId;
                                });

                                response.SetHeader("Set-Cookie", $"LangID={langId}; Path=/");
                                response.SetHeader("Set-Cookie", $"AcctID={accountId}; Path=/");
                                response.SetHeader("Set-Cookie", $"NPCountry=us; Path=/");
                                response.SetHeader("Set-Cookie", $"ClanID=-1; Path=/");
                                response.SetHeader("Set-Cookie", $"AuthKeyTime=03-31-2023 16:03:41; Path=/");
                                response.SetHeader("Set-Cookie", $"NPLang=1; Path=/");
                                response.SetHeader("Set-Cookie", $"ModerateMode=false; Path=/");
                                response.SetHeader("Set-Cookie", $"TimeZone=PST; Path=/");
                                response.SetHeader("Set-Cookie", $"ClanID=-1; Path=/");
                                response.SetHeader("Set-Cookie", $"NPContentRating=201326592; Path=/");
                                response.SetHeader("Set-Cookie", $"AuthKey=nRqnf97f~UaSANLErurJIzq9GXGWqWCADdA3TfqUIVXXisJyMnHsQ34kA&C^0R#&~JULZ7xUOY*rXW85slhQF&P&Eq$7kSB&VBtf`V8rb^BC`53jGCgIT; Path=/");
                                response.SetHeader("Set-Cookie", $"AcctName={acctNameREX}; Path=/");
                                response.SetHeader("Set-Cookie", $"OwnerID=-255; Path=/");
                                response.SetHeader("Set-Cookie", $"Sig={sig}==; Path=/");

                                response.SetBegin((int)HttpStatusCode.OK);
                                response.SetBody("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                    "<XML action=\"http://scee-home.playstation.net/c.home/prod/live\">\r\n" +
                                    "    <SP_Login>\r\n" +
                                    "        <status>\r\n" +
                                    "            <id>20600</id>\r\n" +
                                    "            <message>ACCT_LOGIN_SUCCESS</message>\r\n" +
                                    "        </status>\r\n" +
                                    $"       <accountID>{accountId}</accountID>\r\n" +
                                    "        <userContext>0</userContext>\r\n" +
                                    "    </SP_Login>\r\n" +
                                    "</XML>");
                            }

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
