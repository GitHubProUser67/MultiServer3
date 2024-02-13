using BackendProject.MiscUtils;
using CustomLogger;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SVO.Games
{
    public class SingStar
    {
        public static async Task Singstar_SVO(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                if (request.Url == null)
                {
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }

                string method = request.HttpMethod;

                using (response)
                {
                    switch (request.Url.AbsolutePath)
                    {
                        #region SINGSTAR
                        case "/SINGSTARPS3_SVML/start.jsp":

                            switch (method)
                            {
                                case "GET":

                                    string? clientMac = request.Headers.Get("X-SVOMac");

                                    string? serverMac = SVOProcessor.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                        return;
                                    }
                                    else
                                    {
                                        response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");
                                        response.Headers.Set("X-SVOMac", serverMac);

                                        byte[]? uriStore = null;

                                        if (SVOServerConfiguration.SVOHTTPSBypass)
                                            uriStore = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                            "<SVML>\n" +
                                            $"    <SET name=\"IP\" IPAddress=\"{VariousUtils.GetPublicIPAddress()}\" />    \r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"createGameURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Create.jsp?gameMode=%d\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"gamePostBinaryStatsURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_PostBinaryStats_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"finishGameURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Finish_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"SetUniversePasswordURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/account/SP_SetPassword_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"loginEncryptedURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/account/Account_Encrypted_Login_Submit.jsp\" />    \r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"TicketLoginURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/account/SP_Login_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"SetBuddyListURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/buddy/Buddy_SetList_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"SetIgnoreListURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/account/SP_UpdateIgnoreList_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"drmSignatureURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/commerce/Commerce_BufferedSignature.jsp\"/>\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"spUpdateTicketURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/account/SP_UpdateTicket.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"createGamePlayerURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Create_Player_Submit.jsp?SVOGameID=%d&amp;playerSide=%d\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"createGameSubmitURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Create_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"gameBinaryStatsPostURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_BinaryStatsPost_Submit.jsp\"/>\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"gameFinishURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Finish_Submit.jsp\"/>\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"finishGameURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Finish_Game_Submit.jsp\"/>\r\n" +
                                            $"    <BROWSER_INIT name=\"init\" />\r\n" +
                                            $"     \r\n    \r\n\t<REDIRECT href=\"unityNpLogin.jsp\" name=\"redirect\"/>\r\n" +
                                            "</SVML>");
                                        else
                                            uriStore = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                            "<SVML>\n" +
                                            $"    <SET name=\"IP\" IPAddress=\"{VariousUtils.GetPublicIPAddress()}\" />    \r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"createGameURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Create.jsp?gameMode=%d\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"gamePostBinaryStatsURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_PostBinaryStats_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"finishGameURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Finish_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"SetUniversePasswordURI\" value=\"https://singstar.svo.online.com:10061/SINGSTARPS3_SVML/account/SP_SetPassword_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"loginEncryptedURI\" value=\"https://singstar.svo.online.com:10061/SINGSTARPS3_SVML/account/Account_Encrypted_Login_Submit.jsp\" />    \r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"TicketLoginURI\" value=\"https://singstar.svo.online.com:10061/SINGSTARPS3_SVML/account/SP_Login_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"SetBuddyListURI\" value=\"https://singstar.svo.online.com:10061/SINGSTARPS3_SVML/buddy/Buddy_SetList_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"SetIgnoreListURI\" value=\"https://singstar.svo.online.com:10061/SINGSTARPS3_SVML/account/SP_UpdateIgnoreList_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"drmSignatureURI\" value=\"https://singstar.svo.online.com:10061/SINGSTARPS3_SVML/commerce/Commerce_BufferedSignature.jsp\"/>\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"spUpdateTicketURI\" value=\"https://singstar.svo.online.com:10061/SINGSTARPS3_SVML/account/SP_UpdateTicket.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"createGamePlayerURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Create_Player_Submit.jsp?SVOGameID=%d&amp;playerSide=%d\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"createGameSubmitURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Create_Submit.jsp\" />\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"gameBinaryStatsPostURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_BinaryStatsPost_Submit.jsp\"/>\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"gameFinishURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Game_Finish_Submit.jsp\"/>\r\n" +
                                            $"    <DATA dataType=\"DATA\" name=\"finishGameURI\" value=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/game/Finish_Game_Submit.jsp\"/>\r\n" +
                                            $"    <BROWSER_INIT name=\"init\" />\r\n" +
                                            $"     \r\n    \r\n\t<REDIRECT href=\"unityNpLogin.jsp\" name=\"redirect\"/>\r\n" +
                                            "</SVML>");

                                        response.StatusCode = (int)HttpStatusCode.OK;

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.ContentLength64 = uriStore.Length;
                                                response.OutputStream.Write(uriStore, 0, uriStore.Length);
                                            }
                                            catch (Exception)
                                            {
                                                // Not Important;
                                            }
                                        }
                                    }
                                    break;
                            }
                            break;

                        case "/SINGSTARPS3_SVML/unityNpLogin.jsp":
                            switch (request.HttpMethod)
                            {
                                case "GET":

                                    string? clientMac = request.Headers.Get("X-SVOMac");

                                    string? serverMac = SVOProcessor.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                        return;
                                    }
                                    else
                                    {
                                        response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");
                                        response.Headers.Set("X-SVOMac", serverMac);

                                        byte[] unityNpLogin = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?> \r\n" +
                                            "<SVML>\r\n" +
                                            $"        <UNITY name=\"login\" type=\"command\" success_href=\"/SINGSTARPS3_SVML/singstar/home.jsp\" success_linkoption=\"NORMAL\"/>\r\n" +
                                            $"        <SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n" +
                                            $"</SVML>");

                                        response.StatusCode = (int)HttpStatusCode.OK;

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.ContentLength64 = unityNpLogin.Length;
                                                response.OutputStream.Write(unityNpLogin, 0, unityNpLogin.Length);
                                            }
                                            catch (Exception)
                                            {
                                                // Not Important;
                                            }
                                        }
                                    }

                                    break;
                            }

                            break;

                        case "/SINGSTARPS3_SVML/account/SP_Login_Submit.jsp":
                            switch (request.HttpMethod)
                            {
                                case "POST":

                                    string? clientMac = request.Headers.Get("X-SVOMac");

                                    string? serverMac = SVOProcessor.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                        return;
                                    }
                                    else
                                    {
                                        int appId = Convert.ToInt32(HttpUtility.ParseQueryString(request.Url.Query).Get("applicationID"));

                                        if (!request.HasEntityBody)
                                        {
                                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                                            return;
                                        }

                                        response.Headers.Set("Content-Type", "text/xml");

                                        string s = string.Empty;

                                        // Get the data from the HTTP stream
                                        using (StreamReader reader = new(request.InputStream, request.ContentEncoding))
                                        {
                                            // Convert the data to a string and display it on the console.
                                            s = reader.ReadToEnd();
                                            reader.Close();
                                        }

                                        byte[] bytes = Encoding.ASCII.GetBytes(s);
                                        int AcctNameLen = Convert.ToInt32(bytes.GetValue(81));

                                        string acctName = s.Substring(82, 32);

                                        string acctNameREX = Regex.Replace(acctName, @"[^a-zA-Z0-9]+", string.Empty);

                                        LoggerAccessor.LogInfo($"Logging user {acctNameREX} into SVO...\n");

                                        response.Headers.Set("X-SVOMac", serverMac);

                                        string? sig = HttpUtility.ParseQueryString(request.Url.Query).Get("sig");

                                        int accountId = -1;

                                        string langId = "0";

                                        try
                                        {
                                            await SVOServerConfiguration.Database.GetAccountByName(acctNameREX, appId).ContinueWith((r) =>
                                            {
                                                //Found in database so keep.
                                                langId = request.Url.Query.Substring(94, request.Url.Query.Length - 94);
                                                string? accountName = r.Result.AccountName;
                                                accountId = r.Result.AccountId;
                                            });
                                        }
                                        catch (Exception)
                                        {
                                            langId = request.Url.Query.Substring(94, request.Url.Query.Length - 94);
                                            accountId = 0;
                                        }

                                        response.AddHeader("Set-Cookie", $"LangID={langId}; Path=/");
                                        response.AppendHeader("Set-Cookie", $"AcctID={accountId}; Path=/");
                                        response.AppendHeader("Set-Cookie", $"NPCountry=us; Path=/");
                                        response.AppendHeader("Set-Cookie", $"ClanID=-1; Path=/");
                                        response.AppendHeader("Set-Cookie", $"AuthKeyTime=03-31-2023 16:03:41; Path=/");
                                        response.AppendHeader("Set-Cookie", $"NPLang=1; Path=/");
                                        response.AppendHeader("Set-Cookie", $"ModerateMode=false; Path=/");
                                        response.AppendHeader("Set-Cookie", $"TimeZone=PST; Path=/");
                                        response.AppendHeader("Set-Cookie", $"ClanID=-1; Path=/");
                                        response.AppendHeader("Set-Cookie", $"NPContentRating=201326592; Path=/");
                                        response.AppendHeader("Set-Cookie", $"AuthKey=nRqnf97f~UaSANLErurJIzq9GXGWqWCADdA3TfqUIVXXisJyMnHsQ34kA&C^0R#&~JULZ7xUOY*rXW85slhQF&P&Eq$7kSB&VBtf`V8rb^BC`53jGCgIT; Path=/");
                                        response.AppendHeader("Set-Cookie", $"AcctName={acctNameREX}; Path=/");
                                        response.AppendHeader("Set-Cookie", $"OwnerID=-255; Path=/");
                                        response.AppendHeader("Set-Cookie", $"Sig={sig}==; Path=/");

                                        byte[] sp_Login = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
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

                                        response.StatusCode = (int)HttpStatusCode.OK;

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.ContentLength64 = sp_Login.Length;
                                                response.OutputStream.Write(sp_Login, 0, sp_Login.Length);
                                            }
                                            catch (Exception)
                                            {
                                                // Not Important;
                                            }
                                        }
                                    }

                                    break;
                            }

                            break;

                        case "/SINGSTARPS3_SVML/singstar/home.jsp":
                            switch (request.HttpMethod)
                            {
                                case "GET":

                                    string? clientMac = request.Headers.Get("X-SVOMac");

                                    string? serverMac = SVOProcessor.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                        return;
                                    }
                                    else
                                    {
                                        response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");
                                        response.Headers.Set("X-SVOMac", serverMac);

                                        byte[] homeEnterWorld = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n\r\n" +
                                            "<SVML>\r\n" +
                                            " <SS:SCREEN name=\"page\">\r\n" +
                                            "   <SS:TEXT name=\"title\" x=\"0\" y=\"40\">My SingStar Online</SS:TEXT>\r\n\r\n" +
                                            "   <SS:TEXT name=\"subTitle\" x=\"0\" y=\"60\">sélectionner le serveur</SS:TEXT>\r\n\r\n" +
                                            "   <SINGBUTTON name=\"offline\" linkOption=\"NORMAL\" href=\"offline/store/viewSongs.svml\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n" +
                                            "     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"0\" y=\"70\" width=\"250\" height=\"100\" label=\"Shop Static\"> </SINGBUTTON>\r\n\r\n" +
                                            "   <SINGBUTTON name=\"unity\" linkOption=\"NORMAL\" href=\"netInit.svml\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n" +
                                            "     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"300\" y=\"70\" width=\"250\" height=\"100\" label=\"unity\"></SINGBUTTON>\r\n\r\n" +
                                            "   <SINGBUTTON name=\"stable\" linkOption=\"NORMAL\" href=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/start.jsp\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"600\" y=\"70\" width=\"250\" height=\"100\" label=\"stable\"></SINGBUTTON>\r\n\r\n" +
                                            "   <SINGBUTTON name=\"hotbox\" linkOption=\"NORMAL\" href=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/singstar/home.jsp\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n" +
                                            "     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"900\" y=\"70\" width=\"250\" height=\"100\" label=\"hotbox\"></SINGBUTTON>\r\n\r\n" +
                                            "   <SINGBUTTON name=\"nchingpc\" linkOption=\"NORMAL\" href=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/singstar/home.jsp\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n" +
                                            "     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"0\" y=\"300\" width=\"200\" height=\"100\" label=\"nchingpc\"></SINGBUTTON>\r\n\r\n" +
                                            "   <SINGBUTTON name=\"vpatelpc\"linkOption=\"NORMAL\" href=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/singstar/home.jsp\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n" +
                                            "     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"250\" y=\"300\" width=\"200\" height=\"100\" label=\"vpatelpc\"></SINGBUTTON>\r\n\r\n" +
                                            "   <SINGBUTTON class=\"BUTTON1\" name=\"Test\" linkOption=\"NORMAL\" href=\"other.svml\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n" +
                                            "     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"500\" y=\"300\" width=\"200\" height=\"100\" label=\"offline\"></SINGBUTTON>\r\n\r\n" +
                                            "  <SINGBUTTON name=\"data\" linkOption=\"NORMAL\" href=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/singstar/test/testData.jsp\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n" +
                                            "     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"750\" y=\"300\" width=\"200\" height=\"100\" label=\"data\"></SINGBUTTON>\r\n\r\n" +
                                            "  <SINGBUTTON name=\"jpg\" linkOption=\"NORMAL\" href=\"http://singstar.svo.online.com:10060/SINGSTARPS3_SVML/singstar/test/testJpg.jsp\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n" +
                                            "     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"1000\" y=\"300\" width=\"200\" height=\"100\" label=\"jpg\"></SINGBUTTON>\r\n\r\n" +
                                            "  <SINGBUTTON name=\"commerce\" linkOption=\"NORMAL\" href=\"commerce/started.svml\"\r\n" +
                                            "     fillColor=\"#FF027ABB\" highlightFillColor=\"#FF13DEEA\"\r\n" +
                                            "     textColor=\"#FFFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n" +
                                            "     x=\"500\" y=\"600\" width=\"200\" height=\"100\" label=\"commerce\"></SINGBUTTON>\r\n\r\n" +
                                            " </SS:SCREEN>\r\n\r\n" +
                                            " <ACTIONLINK name=\"link\" button=\"SV_ACTION_BACK\" href=\"hide.svml\"/>\r\n</SVML>");

                                        response.StatusCode = (int)HttpStatusCode.OK;

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.ContentLength64 = homeEnterWorld.Length;
                                                response.OutputStream.Write(homeEnterWorld, 0, homeEnterWorld.Length);
                                            }
                                            catch (Exception)
                                            {
                                                // Not Important;
                                            }
                                        }
                                    }

                                    break;
                            }

                            break;

                        default:
                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                            break;

                            #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SVO] - Singstar_SVO thrown an assertion - {ex}");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
