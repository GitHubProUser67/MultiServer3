using HttpMultipartParser;
using DotNetty.Common.Internal.Logging;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PSMultiServer.SRC_Addons.MEDIUS.SVO.GAMES
{
    public class Ps_Home
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Ps_Home>();
        public static async Task Home_SVO(HttpListenerContext context, string userAgent)
        {
            using (var response = context.Response)
            {
                try
                {
                    switch (context.Request.Url.AbsolutePath)
                    {
                        #region HOME
                        case "/HUBPS3_SVML/unity/start.jsp":

                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {

                                        response.Headers.Set("X-SVOMac", serverMac);
                                        string region = context.Request.Url.Query.Substring(8);

                                        byte[] uriStore = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                            "<SVML>\r\n" +
                                           $"    <BROWSER_INIT name=\"init\" />\r\n\t" +
                                            "    \r\n    <SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n" +
                                            "    \r\n    <DATA dataType=\"URI\" name=\"SvfsUpload\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/fileservices/UploadFileServlet\"/>\r\n" +
                                            "    \r\n    <DATA dataType=\"URI\" name=\"SvfsDownload\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/fileservices/Download.jsp\"/>\r\n" +
                                            "    <DATA dataType=\"URI\" name=\"SvfsDeleteSubmit\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/fileservices/Delete.jsp\"/>\r\n    \r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpArcadeMachines\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=arcade\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpBowling\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=bowling\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpCharacterCreation\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=characterCustomisation\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpChess\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=chess\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpConversations\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=conversations\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpDoors\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=relocation\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpDraughts\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=draughts\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpEmotes\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=emotes\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpFirstTimeUser\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=firstTimeUsing\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpGameLaunchingCreate\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=gamelaunchingCreate\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpGameLaunchingJoin\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=gamelaunchingJoin\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpGamesRoom\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=gamespace\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpHomeApartment\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=homespace\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpPool\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=pool\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpSafetyInHome\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=stayingsafe\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"HelpSeats\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=seats\"/>\r\n\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"CommunityNews\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=news\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"CommunityLatestUpdate\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=latestUpdate\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"CommunityHandyLinks\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=handyLinks\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"CommunityMotd\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=messageoftheday\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"CommunityUsagePolicy\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=eula\"/>\r\n\r\n    " +
                                           $"    <DATA dataType=\"URI\" name=\"GriefReportStart\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/griefreporting/GriefReportWelcome.jsp?region={region}\"/>\r\n" +
                                            "    <DATA dataType=\"URI\" name=\"ViralProvisioningStart\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/viralprovisioning/HomeInviteWelcome.jsp\"/>\r\n" +
                                            "    <DATA dataType=\"URI\" name=\"UserActivityLogUploadServlet\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/tracking/StatTrackingServlet\"/>\r\n    \r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"CommunityNewsSummary\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=news\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"CommunityNewsDetailed\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=latestUpdate\"/>\r\n" +
                                           $"    <DATA dataType=\"URI\" name=\"CommunityBetaTrialRoadmap\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/home/help/HelpGeneric.jsp?region={region}&amppageName=handyLinks\"/>\r\n    \r\n" +
                                            "    <DATA dataType=\"DATA\" name=\"gameFinishURL\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/game/Game_Finish_Submit.jsp\" />\r\n\t\r\n" +
                                            "    <DATA dataType=\"DATA\" name=\"TicketLoginURI\" value=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/account/SP_Login_Submit.jsp\" />\r\n\t\r\n\t" +
                                            "<!--\r\n    <DATA dataType=\"DATA\" name=\"TicketLoginURI\" value=\"https://homeps3.svo.online.scee.com:10061/HUBPS3_SVML/account/SP_Login_Submit.jsp\" />\r\n       -->" + // TODO, make it optional to http it.
                                           $"     \r\n    \r\n\t<REDIRECT href=\"eulaCheck.jsp?region={region}&amp\" name=\"redirect\"/>\r\n" +
                                            "      \r\n" +
                                            "</SVML>");

                                        Stream ros = response.OutputStream;

                                        if (ros.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.StatusDescription = "OK";
                                                response.ContentLength64 = uriStore.Length;
                                                ros.Write(uriStore, 0, uriStore.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        ros.Dispose();
                                    }
                                    break;
                            }
                            break;

                        case "/HUBPS3_SVML/unity/eulaCheck.jsp":

                            if (context.Request.Url.AbsolutePath.Contains("autologin=0"))
                            {
                                switch (context.Request.HttpMethod)
                                {
                                    case "GET":

                                        response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");

                                        string clientMac = context.Request.Headers.Get("X-SVOMac");

                                        string serverMac = SVO.CalcuateSVOMac(clientMac);

                                        if (serverMac == null)
                                        {
                                            Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                            // Return an internal server error response
                                            byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                    response.ContentLength64 = Refused.Length;
                                                    response.OutputStream.Write(Refused, 0, Refused.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            context.Response.Close();

                                            GC.Collect();

                                            return;
                                        }
                                        else
                                        {
                                            string region = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("region");
                                            response.Headers.Set("X-SVOMac", serverMac);

                                            byte[] eulaCheck = Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?> \r\n" +
                                                "<SVML>\r\n  " +
                                                "<SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n  " +
                                                $"<EULA name=\"eula\" mode=\"check\" href=\"unityNpLogin.jsp?region={region}\" eulahref=\"eulaDisplay.jsp?region={region}\" linkOption=\"NORMAL\" />\r\n" +
                                                "</SVML>");

                                            Stream ros = response.OutputStream;

                                            if (ros.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.StatusDescription = "OK";
                                                    response.ContentLength64 = eulaCheck.Length;
                                                    ros.Write(eulaCheck, 0, eulaCheck.Length);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            ros.Dispose();
                                        }

                                        break;
                                }
                            }
                            else if (context.Request.Url.AbsolutePath.Contains("autologin=1"))
                            {
                                switch (context.Request.HttpMethod)
                                {
                                    case "GET":

                                        response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");

                                        string clientMac = context.Request.Headers.Get("X-SVOMac");

                                        string serverMac = SVO.CalcuateSVOMac(clientMac);

                                        if (serverMac == null)
                                        {
                                            Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                            // Return an internal server error response
                                            byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                    response.ContentLength64 = Refused.Length;
                                                    response.OutputStream.Write(Refused, 0, Refused.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            context.Response.Close();

                                            GC.Collect();

                                            return;
                                        }
                                        else
                                        {

                                            response.Headers.Set("X-SVOMac", serverMac);

                                            string region = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("region");

                                            byte[] unityNpLogin = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?> \r\n" +
                                                "<SVML>\r\n" +
                                                "        <EULA name=\"eula\" mode=\"save\" />\r\n" +
                                                $"        <UNITY name=\"login\" type=\"command\" success_href=\"../announcement/Medius_Announcement_Read.jsp?region={region}\" success_linkoption=\"NORMAL\"/>\r\n" +
                                                $" \r\n\t\t<HOMEACTION name=\"FrontEndAction\">" +
                                                $"\r\n\t\t\t<OnEnterPage event=\"FrontEndEvent\" param1=\"SigningIntoSvo\" param2=\"\" />" +
                                                $"\r\n\t\t</HOMEACTION>\r\n" +
                                                $"  \r\n" +
                                                $"        <SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n" +
                                                $"</SVML>");

                                            Stream ros = response.OutputStream;

                                            if (ros.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.StatusDescription = "OK";
                                                    response.ContentLength64 = unityNpLogin.Length;
                                                    ros.Write(unityNpLogin, 0, unityNpLogin.Length);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            ros.Dispose();
                                        }

                                        break;
                                }
                            }
                            else
                            {
                                switch (context.Request.HttpMethod)
                                {
                                    case "GET":

                                        response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");

                                        string clientMac = context.Request.Headers.Get("X-SVOMac");

                                        string serverMac = SVO.CalcuateSVOMac(clientMac);

                                        if (serverMac == null)
                                        {
                                            Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                            // Return an internal server error response
                                            byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                    response.ContentLength64 = Refused.Length;
                                                    response.OutputStream.Write(Refused, 0, Refused.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            context.Response.Close();

                                            GC.Collect();

                                            return;
                                        }
                                        else
                                        {

                                            response.Headers.Set("X-SVOMac", serverMac);

                                            string region = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("region");

                                            byte[] unityNpLogin = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?> \r\n" +
                                                "<SVML>\r\n" +
                                                "        <EULA name=\"eula\" mode=\"save\" />\r\n" +
                                                $"        <UNITY name=\"login\" type=\"command\" success_href=\"../announcement/Medius_Announcement_Read.jsp?region={region}\" success_linkoption=\"NORMAL\"/>\r\n" +
                                                $" \r\n\t\t<HOMEACTION name=\"FrontEndAction\">" +
                                                $"\r\n\t\t\t<OnEnterPage event=\"FrontEndEvent\" param1=\"SigningIntoSvo\" param2=\"\" />" +
                                                $"\r\n\t\t</HOMEACTION>\r\n" +
                                                $"  \r\n" +
                                                $"        <SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n" +
                                                $"</SVML>");

                                            Stream ros = response.OutputStream;

                                            if (ros.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.StatusDescription = "OK";
                                                    response.ContentLength64 = unityNpLogin.Length;
                                                    ros.Write(unityNpLogin, 0, unityNpLogin.Length);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            ros.Dispose();
                                        }

                                        break;
                                }
                            }

                            break;

                        case "/HUBPS3_SVML/unity/eulaDisplay.jsp":

                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");


                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {
                                        response.Headers.Set("X-SVOMac", serverMac);

                                        string region = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("region");

                                        byte[] eulaDisplay = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?> \r\n" +
                                            "<SVML>\r\n  <SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n  " +
                                            //$"<EULA name=\"eula\" mode=\"browser\" href=\"unityNpLogin.jsp?region={region}\" eulahref=\"eulaDisplay.jsp?region={region}\" linkOption=\"NORMAL\" />\r\n" +
                                            $"<EULA name=\"eula\" mode=\"save\" href=\"unityNpLogin.jsp?region={region}\" eulahref=\"eulaDisplay.jsp?region={region}\" linkOption=\"NORMAL\" />\r\n" + // Not correct but gets around a RPCS3 issue.
                                            "</SVML>");

                                        Stream ros = response.OutputStream;

                                        if (ros.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.StatusDescription = "OK";
                                                response.ContentLength64 = eulaDisplay.Length;
                                                ros.Write(eulaDisplay, 0, eulaDisplay.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        ros.Dispose();
                                    }


                                    break;
                            }
                            break;

                        case "/HUBPS3_SVML/unity/unityNpLogin.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {

                                        response.Headers.Set("X-SVOMac", serverMac);
                                        string region = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("region");


                                        byte[] unityNpLogin = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?> \r\n" +
                                            "<SVML>\r\n" +
                                            "        <EULA name=\"eula\" mode=\"save\" />\r\n" +
                                            $"        <UNITY name=\"login\" type=\"command\" success_href=\"../announcement/Medius_Announcement_Read.jsp?region={region}\" success_linkoption=\"NORMAL\"/>\r\n" +
                                            $" \r\n\t\t<HOMEACTION name=\"FrontEndAction\">" +
                                            $"\r\n\t\t\t<OnEnterPage event=\"FrontEndEvent\" param1=\"SigningIntoSvo\" param2=\"\" />" +
                                            $"\r\n\t\t</HOMEACTION>\r\n" +
                                            $"  \r\n" +
                                            $"        <SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n" +
                                            $"</SVML>");

                                        Stream ros = response.OutputStream;

                                        if (ros.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.StatusDescription = "OK";
                                                response.ContentLength64 = unityNpLogin.Length;
                                                ros.Write(unityNpLogin, 0, unityNpLogin.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        ros.Dispose();
                                    }

                                    break;
                            }

                            break;

                        case "/HUBPS3_SVML/account/SP_Login_Submit.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "POST":

                                    response.Headers.Set("Content-Type", "text/xml");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {
                                        int appId = Convert.ToInt32(HttpUtility.ParseQueryString(context.Request.Url.Query).Get("applicationID"));

                                        if (!context.Request.HasEntityBody)
                                        {
                                            Logger.Warn($"SVO server : {userAgent} Requested a SVO Home SP Submit without any data.");

                                            // Return an internal server error response
                                            byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = 500;
                                                    response.ContentLength64 = Refused.Length;
                                                    response.OutputStream.Write(Refused, 0, Refused.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            context.Response.Close();

                                            return;
                                        }

                                        // Get the data from the HTTP stream
                                        Stream body = context.Request.InputStream;
                                        Encoding encoding = context.Request.ContentEncoding;
                                        StreamReader reader = new StreamReader(body, encoding);

                                        // Convert the data to a string and display it on the console.
                                        string s = reader.ReadToEnd();

                                        byte[] bytes = Encoding.ASCII.GetBytes(s);
                                        int AcctNameLen = Convert.ToInt32(bytes.GetValue(81));

                                        string acctName = s.Substring(82, 32);

                                        string acctNameREX = Regex.Replace(acctName, @"[^a-zA-Z0-9]+", string.Empty);

                                        Logger.Info($"Logging user {acctNameREX} into SVO...\n");

                                        response.Headers.Set("X-SVOMac", serverMac);

                                        string sig = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("sig");

                                        int accountId = -1;

                                        string langId = "0";

                                        SvoClass.Database.GetAccountByName(acctNameREX, appId).ContinueWith((r) =>
                                        {
                                            //Found in database so keep.
                                            string langId = context.Request.Url.Query.Substring(94, context.Request.Url.Query.Length - 94);
                                            string accountName = r.Result.AccountName;
                                            accountId = r.Result.AccountId;
                                        });

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

                                        Stream ros = response.OutputStream;

                                        if (ros.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.StatusDescription = "OK";
                                                response.ContentLength64 = sp_Login.Length;
                                                ros.Write(sp_Login, 0, sp_Login.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        ros.Dispose();
                                    }

                                    break;
                            }

                            break;

                        case "/HUBPS3_SVML/announcement/Medius_Announcement_Read.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {

                                        response.Headers.Set("X-SVOMac", serverMac);

                                        byte[] Medius_Announcement_Read = Encoding.UTF8.GetBytes(SvoClass.HOMEmessageoftheday);

                                        Stream ros = response.OutputStream;

                                        if (ros.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.StatusDescription = "OK";
                                                response.ContentLength64 = Medius_Announcement_Read.Length;
                                                ros.Write(Medius_Announcement_Read, 0, Medius_Announcement_Read.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        ros.Dispose();
                                    }

                                    break;
                            }

                            break;

                        case "/HUBPS3_SVML/home/homeEnterWorld.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {
                                        response.Headers.Set("X-SVOMac", serverMac);

                                        byte[] homeEnterWorld = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?> \r\n<SVML>\r\n\r\n" +
                                            "    <HUB type=\"AutoChangeMode\" textColor=\"#FF7381BA\" highlightTextColor=\"#FF7381BA\" x=\"20\" y=\"200\" width=\"200\" height=\"40\"\r\n" +
                                            "      align=\"center\" border=\"true\" href=\"EnterLobby\" extra=\"Home Square\" skipOn=\"6\"></HUB>\r\n\r\n\t" +
                                            "   <REDIRECT name=\"toBlankPage\" href=\"homeInWorld.jsp\" linkOption=\"NORMAL\"/>\r\n\r\n" +
                                            "</SVML>");

                                        Stream ros = response.OutputStream;

                                        if (ros.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.StatusDescription = "OK";
                                                response.ContentLength64 = homeEnterWorld.Length;
                                                ros.Write(homeEnterWorld, 0, homeEnterWorld.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        ros.Dispose();
                                    }

                                    break;
                            }

                            break;

                        case "/HUBPS3_SVML/home/homeInWorld.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {
                                        response.Headers.Set("X-SVOMac", serverMac);

                                        byte[] homeEnterWorld = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?> \r\n" +
                                            "<SVML>\r\n\r\n</SVML>");

                                        Stream ros = response.OutputStream;

                                        if (ros.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.StatusDescription = "OK";
                                                response.ContentLength64 = homeEnterWorld.Length;
                                                ros.Write(homeEnterWorld, 0, homeEnterWorld.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        ros.Dispose();
                                    }

                                    break;
                            }

                            break;

                        case "/HUBPS3_SVML/home/fileservices/Download.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "text/xml");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    response.Headers.Set("X-SVOMac", serverMac);

                                    string fileNameBeginsWith = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("filename");

                                    bool fileExists = File.Exists($"./wwwsvoroot/HUBPS3_SVML/fileservices/{fileNameBeginsWith}");

                                    byte[] xmlMessage;

                                    if (fileExists == true)
                                    {
                                        string fileId = "1";

                                        string encodedFileName = SecurityElement.Escape(fileNameBeginsWith);

                                        xmlMessage = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                            $"<XML>\r\n\r\n<XMLSVOFILETRANSFER direction=\"download\" filename=\"{encodedFileName}\" errorCode=\"None\" src=\"http://homeps3.svo.online.scee.com:10060/HUBPS3_SVML/fileservices/DownloadFileServlet?fileID={fileId}&amp;fileNameBeginsWith={encodedFileName}\"/>\r\n</XML>");
                                    }
                                    else
                                    {
                                        string encodedFileName = SecurityElement.Escape(fileNameBeginsWith);

                                        xmlMessage = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                            $"<XML>\r\n\r\n<XMLSVOFILETRANSFER direction=\"download\" filename=\"{encodedFileName}\" errorCode=\"FileDoesNotExist\" src=\"\"/>\r\n</XML>");
                                    }

                                    Stream ros = response.OutputStream;

                                    if (ros.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = (int)HttpStatusCode.OK;
                                            response.StatusDescription = "OK";
                                            response.ContentLength64 = xmlMessage.Length;
                                            ros.Write(xmlMessage, 0, xmlMessage.Length);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                        }
                                    }
                                    else
                                    {
                                        Logger.Warn("Client Disconnected early");
                                    }

                                    ros.Dispose();

                                    break;
                            }

                            break;


                        case "/HUBPS3_SVML/fileservices/DownloadFileServlet":

                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    string fileNameBeginsWith = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("fileNameBeginsWith");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {
                                        response.Headers.Set("X-SVOMac", serverMac);

                                        if (fileNameBeginsWith.Contains(".jpg"))
                                        {
                                            response.Headers.Set("Content-Type", "image/jpeg;charset=UTF-8");
                                            response.AppendHeader("Content-Disposition", $"attachment; filename={fileNameBeginsWith}");
                                            response.AppendHeader("Accept-Ranges", "bytes");
                                            response.AppendHeader("Cache-Control", "private");
                                            response.AppendHeader("ETag", $"{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 12)}");
                                        }
                                        else if (fileNameBeginsWith.Contains(".xml") || fileNameBeginsWith.Contains("Profile"))
                                        {
                                            response.Headers.Set("Content-Type", "text/xml;charset=UTF-8");
                                            response.AppendHeader("Content-Disposition", $"attachment; filename={fileNameBeginsWith}");
                                            response.AppendHeader("Accept-Ranges", "bytes");
                                            response.AppendHeader("Cache-Control", "private");
                                            response.AppendHeader("ETag", $"{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 12)}");
                                        }

                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            StreamWriter writer = new StreamWriter(ms);

                                            using (FileStream fs = new FileStream($"./wwwsvoroot/HUBPS3_SVML/fileservices/{fileNameBeginsWith}", FileMode.Open))
                                            {
                                                int fileLen = Convert.ToInt32(fs.Length);

                                                response.AppendHeader("Content-Length", fileLen.ToString());

                                                // Create a byte array.
                                                byte[] strArr = new byte[fileLen];
                                                fs.Read(strArr, 0, fileLen);
                                                fs.Flush();


                                                //You have to rewind the MemoryStream before copying
                                                ms.Write(strArr, 0, fileLen);

                                                Stream ros = response.OutputStream;

                                                if (ros.CanWrite)
                                                {
                                                    try
                                                    {
                                                        response.StatusCode = (int)HttpStatusCode.OK;
                                                        response.StatusDescription = "OK";
                                                        ros.Write(strArr, 0, fileLen);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                    }
                                                }
                                                else
                                                {
                                                    Logger.Warn("Client Disconnected early");
                                                }

                                                ros.Dispose();
                                            }
                                        }
                                    }

                                    break;
                            }

                            break;

                        //Uploads Player's UserTrackingLog, we can handle this in different way but we just upload it for now
                        case "/HUBPS3_SVML/tracking/StatTrackingServlet":
                            switch (context.Request.HttpMethod)
                            {
                                case "POST":

                                    response.Headers.Set("Content-Type", "text/xml");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {
                                        response.Headers.Set("X-SVOMac", serverMac);

                                        byte[] xmlMessage;
                                        string fileNameBeginsWith = "UserTrackingLog.xml";

                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            StreamWriter writer = new StreamWriter(ms);


                                            // Find number of bytes in stream.
                                            int strLen = Convert.ToInt32(context.Request.ContentLength64);
                                            // Create a byte array.
                                            byte[] strArr = new byte[strLen];

                                            context.Request.InputStream.Read(strArr, 0, strLen);

                                            //You have to rewind the MemoryStream before copying
                                            ms.Read(strArr, 0, strLen);

                                            //We can do whatever we want with the POST information for the UseTrackingLog from any player it seems? 
                                            //Lets just write to file!

                                            using (FileStream fs = new FileStream($"./wwwsvoroot/HUBPS3_SVML/tracking/{fileNameBeginsWith}", FileMode.OpenOrCreate))
                                            {
                                                fs.Write(strArr, 0, strLen);
                                                fs.Flush();
                                                fs.Dispose();
                                            }
                                        }

                                        xmlMessage = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
                                        $"<XML>\r\n\r\n<XMLSVOFILETRANSFER direction=\"upload\" filename=\"{fileNameBeginsWith}\"/>\r\n</XML>");

                                        Stream ros = response.OutputStream;

                                        if (ros.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.StatusDescription = "OK";
                                                response.ContentLength64 = xmlMessage.Length;
                                                ros.Write(xmlMessage, 0, xmlMessage.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        ros.Dispose();
                                    }
                                    break;
                            }
                            break;

                        case "/HUBPS3_SVML/fileservices/UploadFileServlet":
                            switch (context.Request.HttpMethod)
                            {
                                case "POST":

                                    response.Headers.Set("Content-Type", "text/xml");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {
                                        response.Headers.Set("X-SVOMac", serverMac);

                                        string toUpload = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("fileNameBeginsWith");

                                        byte[] xmlMessage;

                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            context.Request.InputStream.CopyTo(ms);

                                            // Reset the memory stream position to the beginning
                                            ms.Position = 0;

                                            // Find the number of bytes in the stream
                                            int contentLength = (int)ms.Length;

                                            // Create a byte array
                                            byte[] buffer = new byte[contentLength];

                                            // Read the contents of the memory stream into the byte array
                                            ms.Read(buffer, 0, contentLength);

                                            using (FileStream fs = new FileStream($"./wwwsvoroot/HUBPS3_SVML/fileservices/{toUpload}", FileMode.OpenOrCreate))
                                            {
                                                fs.Write(buffer, 0, contentLength);
                                                fs.Flush();
                                                fs.Dispose();
                                            }
                                        }

                                        xmlMessage = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
                                        $"<XML>\r\n\r\n" +
                                        $"<XMLSVOFILETRANSFER direction=\"upload\" filename=\"{toUpload}\"/>\r\n" +
                                        $"</XML>");

                                        Stream ros = response.OutputStream;

                                        if (ros.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.OK;
                                                response.StatusDescription = "OK";
                                                response.ContentLength64 = xmlMessage.Length;
                                                ros.Write(xmlMessage, 0, xmlMessage.Length);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        ros.Dispose();
                                    }
                                    break;
                            }
                            break;

                        case "/HUBPS3_SVML/home/help/HelpGeneric.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "text/svml; charset=UTF-8");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    if (serverMac == null)
                                    {
                                        Logger.Warn($"SVO server : {userAgent} Requested a SVO file without a SVOMAC, so we forbid.");

                                        // Return an internal server error response
                                        byte[] Refused = Encoding.UTF8.GetBytes(PreMadeWebPages.rootrefused);

                                        if (response.OutputStream.CanWrite)
                                        {
                                            try
                                            {
                                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                                response.ContentLength64 = Refused.Length;
                                                response.OutputStream.Write(Refused, 0, Refused.Length);
                                                response.OutputStream.Close();
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("Client Disconnected early");
                                        }

                                        context.Response.Close();

                                        GC.Collect();

                                        return;
                                    }
                                    else
                                    {

                                        response.Headers.Set("X-SVOMac", serverMac);

                                        string region = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("region");

                                        string pageName = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("pageName");

                                        byte[] xmlMessage;

                                        if (pageName == "news")
                                        {
                                            xmlMessage = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                                "<SVML>\r\n  <RECTANGLE name=\"background\" x=\"0\" y=\"0\" width=\"1280\" height=\"720\" fillColor=\"#99000000\"/>\r\n" +
                                                "  <IMAGE name=\"icon\" x=\"32\" y=\"32\" width=\"96\" height=\"96\" src=\"file:///HOST_SVML/images/xmb_Firsttimeusinghome.dds\"/>\r\n" +
                                                "  <TEXT name=\"title\" x=\"160\" y=\"52\" width=\"1056\" height=\"56\" fontSize=\"56\" align=\"left\" textColor=\"#ffffffff\" selectable=\"false\">News</TEXT>\r\n" +
                                               $"  <TEXTAREA\r\n\t\tclass=\"BG_NONE,SCROLL_ARROWS\" name=\"message\"\r\n\t\tx=\"64\" y=\"160\" width=\"1152\" height=\"452\" scrollBarWidth=\"32\"\r\n\t\tfontSize=\"52\" lineSpacing=\"56\" linesVisible=\"8\"\r\n\t\treadonly=\"\" blinkCursor=\"false\"\r\n\t\ttextColor=\"#BBFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n\t\tleftPadValue=\"0\" topPadValue=\"0\"\r\n\t\tdefaultTextEntry=\"1\" defaultTextScroll=\"1\"\r\n\t\tselectable=\"false\" selected=\"false\">News!</TEXTAREA>\r\n\r\n" +
                                                "  <TEXT name=\"legend\" class=\"localise\" x=\"48\" y=\"640\" fontSize=\"48\" align=\"left\" textColor=\"#ffffffff\" selectable=\"false\">[CIRCLE]Back</TEXT>\r\n" +
                                                "  <TEXT name=\"legend\" class=\"localise\" x=\"1232\" y=\"640\" fontSize=\"48\" align=\"right\" textColor=\"#ffffffff\" selectable=\"false\">SCROLL_UP_DOWN</TEXT>\r\n" +
                                                "  <SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n" +
                                                "  <QUICKLINK name=\"returnToPSP\" button=\"SV_PAD_BACK\" linkOption=\"\" href=\"file:///HOST_SVML/ReturnToPSP.svml\"/>\r\n" +
                                                "</SVML>");

                                            Stream ros = response.OutputStream;

                                            if (ros.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.StatusDescription = "OK";
                                                    response.ContentLength64 = xmlMessage.Length;
                                                    ros.Write(xmlMessage, 0, xmlMessage.Length);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            ros.Dispose();
                                        }
                                        else if (pageName == "latestUpdate")
                                        {
                                            xmlMessage = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                                "<SVML>\r\n  <RECTANGLE name=\"background\" x=\"0\" y=\"0\" width=\"1280\" height=\"720\" fillColor=\"#99000000\"/>\r\n" +
                                                "  <IMAGE name=\"icon\" x=\"32\" y=\"32\" width=\"96\" height=\"96\" src=\"file:///HOST_SVML/images/xmb_Firsttimeusinghome.dds\"/>\r\n" +
                                                "  <TEXT name=\"title\" x=\"160\" y=\"52\" width=\"1056\" height=\"56\" fontSize=\"56\" align=\"left\" textColor=\"#ffffffff\" selectable=\"false\">Latest Update</TEXT>\r\n" +
                                               $"  <TEXTAREA\r\n\t\tclass=\"BG_NONE,SCROLL_ARROWS\" name=\"message\"\r\n\t\tx=\"64\" y=\"160\" width=\"1152\" height=\"452\" scrollBarWidth=\"32\"\r\n\t\tfontSize=\"52\" lineSpacing=\"56\" linesVisible=\"8\"\r\n\t\treadonly=\"\" blinkCursor=\"false\"\r\n\t\ttextColor=\"#BBFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n\t\tleftPadValue=\"0\" topPadValue=\"0\"\r\n\t\tdefaultTextEntry=\"1\" defaultTextScroll=\"1\"\r\n\t\tselectable=\"false\" selected=\"false\">Last Update!</TEXTAREA>\r\n\r\n" +
                                                "  <TEXT name=\"legend\" class=\"localise\" x=\"48\" y=\"640\" fontSize=\"48\" align=\"left\" textColor=\"#ffffffff\" selectable=\"false\">[CIRCLE]Back</TEXT>\r\n" +
                                                "  <TEXT name=\"legend\" class=\"localise\" x=\"1232\" y=\"640\" fontSize=\"48\" align=\"right\" textColor=\"#ffffffff\" selectable=\"false\">SCROLL_UP_DOWN</TEXT>\r\n" +
                                                "  <SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n" +
                                                "  <QUICKLINK name=\"returnToPSP\" button=\"SV_PAD_BACK\" linkOption=\"\" href=\"file:///HOST_SVML/ReturnToPSP.svml\"/>\r\n" +
                                                "</SVML>");

                                            Stream ros = response.OutputStream;

                                            if (ros.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.StatusDescription = "OK";
                                                    response.ContentLength64 = xmlMessage.Length;
                                                    ros.Write(xmlMessage, 0, xmlMessage.Length);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            ros.Dispose();
                                        }
                                        else if (pageName == "messageoftheday")
                                        {
                                            xmlMessage = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                                "<SVML>\r\n  <RECTANGLE name=\"background\" x=\"0\" y=\"0\" width=\"1280\" height=\"720\" fillColor=\"#99000000\"/>\r\n" +
                                                "  <IMAGE name=\"icon\" x=\"32\" y=\"32\" width=\"96\" height=\"96\" src=\"file:///HOST_SVML/images/xmb_Firsttimeusinghome.dds\"/>\r\n" +
                                                "  <TEXT name=\"title\" x=\"160\" y=\"52\" width=\"1056\" height=\"56\" fontSize=\"56\" align=\"left\" textColor=\"#ffffffff\" selectable=\"false\">Message of the Day</TEXT>\r\n" +
                                               $"  <TEXTAREA\r\n\t\tclass=\"BG_NONE,SCROLL_ARROWS\" name=\"message\"\r\n\t\tx=\"64\" y=\"160\" width=\"1152\" height=\"452\" scrollBarWidth=\"32\"\r\n\t\tfontSize=\"52\" lineSpacing=\"56\" linesVisible=\"8\"\r\n\t\treadonly=\"\" blinkCursor=\"false\"\r\n\t\ttextColor=\"#BBFFFFFF\" highlightTextColor=\"#FFFFFFFF\"\r\n\t\tleftPadValue=\"0\" topPadValue=\"0\"\r\n\t\tdefaultTextEntry=\"1\" defaultTextScroll=\"1\"\r\n\t\tselectable=\"false\" selected=\"false\">Message of the Day!</TEXTAREA>\r\n\r\n" +
                                                "  <TEXT name=\"legend\" class=\"localise\" x=\"48\" y=\"640\" fontSize=\"48\" align=\"left\" textColor=\"#ffffffff\" selectable=\"false\">[CIRCLE]Back</TEXT>\r\n" +
                                                "  <TEXT name=\"legend\" class=\"localise\" x=\"1232\" y=\"640\" fontSize=\"48\" align=\"right\" textColor=\"#ffffffff\" selectable=\"false\">SCROLL_UP_DOWN</TEXT>\r\n" +
                                                "  <SET name=\"nohistory\" neverBackOnto=\"true\"/>\r\n" +
                                                "  <QUICKLINK name=\"returnToPSP\" button=\"SV_PAD_BACK\" linkOption=\"\" href=\"file:///HOST_SVML/ReturnToPSP.svml\"/>\r\n" +
                                                "</SVML>");

                                            Stream ros = response.OutputStream;

                                            if (ros.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = (int)HttpStatusCode.OK;
                                                    response.StatusDescription = "OK";
                                                    response.ContentLength64 = xmlMessage.Length;
                                                    ros.Write(xmlMessage, 0, xmlMessage.Length);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            ros.Dispose();
                                        }
                                    }

                                    break;
                            }

                            break;

                        case "/HUBPS3_SVML/home/griefreporting/GriefReportWelcome.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "text/svml");

                                    string clientMac = context.Request.Headers.Get("X-SVOMac");

                                    string serverMac = SVO.CalcuateSVOMac(clientMac);

                                    response.Headers.Set("X-SVOMac", serverMac);

                                    byte[] xmlMessage = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<SVML>\r\n" +
                                        "    <RECTANGLE class=\"CHIP_FACE\" name=\"backPanel\" x=\"292\" y=\"140\" width=\"708\" height=\"440\"/>\r\n" +
                                        "    <RECTANGLE class=\"CHIP_RECESS\" name=\"backPanel\" x=\"300\" y=\"148\" width=\"692\" height=\"384\" fillColor=\"#FFFFFFFF\"/>\r\n\r\n" +
                                        "    <TEXT name=\"text\" x=\"640\" y=\"171\" width=\"636\" height=\"26\" fontSize=\"26\" align=\"center\" textColor=\"#cc000000\">Grief Report</TEXT>\r\n\r\n" +
                                        "    <TEXTAREA class=\"TEXTAREA1\" name=\"message\" x=\"308\" y=\"204\" width=\"664\" height=\"320\"\r\n\t\tfontSize=\"22\" lineSpacing=\"22\" linesVisible=\"14\"\r\n\t\t readonly=\"true\"selectable=\"false\" blinkCursor=\"false\"\r\n\t\ttextColor=\"#CC000000\" highlightTextColor=\"#FF000000\"\r\n\t\tleftPadValue=\"8\" topPadValue=\"8\" \r\n defaultTextEntry=\"1\" defaultTextScroll=\"1\">" +
                                        "    Please provide evidence of the person you would like to submit for the grief report!\r\n\r\n" +
                                        "    <TEXT name=\"legend\" x=\"984\" y=\"548\" width=\"652\" height=\"18\" fontSize=\"18\" align=\"right\" textColor=\"#CCFFFFFF\">[CROSS] Continue</TEXT>\r\n" +
                                        "    <QUICKLINK name=\"refresh\" button=\"SV_PAD_X\" linkOption=\"NORMAL\" href=\"../home/homeInWorld.jsp\"/>\r\n" +
                                        "</SVML>\r\n" +
                                        "<SVML>\r\n" +
                                        "</SVML>");

                                    Stream ros = response.OutputStream;

                                    if (ros.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = (int)HttpStatusCode.OK;
                                            response.StatusDescription = "OK";
                                            response.ContentLength64 = xmlMessage.Length;
                                            ros.Write(xmlMessage, 0, xmlMessage.Length);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                        }
                                    }
                                    else
                                    {
                                        Logger.Warn("Client Disconnected early");
                                    }

                                    ros.Dispose();

                                    break;
                            }

                            break;

                        case "/dataloaderweb/queue":
                            {
                                if (context.Request.ContentType.StartsWith("multipart/form-data"))
                                {
                                    switch (context.Request.HttpMethod)
                                    {
                                        case "POST":

                                            response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                            response.Headers.Set("Content-Language", "");

                                            string boundary = Misc.ExtractBoundary(context.Request.ContentType);

                                            var data = MultipartFormDataParser.Parse(context.Request.InputStream, boundary);

                                            byte[] datatooutput = Encoding.UTF8.GetBytes(data.GetParameterValue("body"));

                                            string nameoffile = Misc.CreateSessionID(context);

                                            using (FileStream fs = new FileStream($"./wwwsvoroot/dataloaderweb/queue/{nameoffile}.xml", FileMode.OpenOrCreate))
                                            {
                                                fs.Write(datatooutput, 0, datatooutput.Length);
                                                fs.Flush();
                                                fs.Dispose();
                                            }

                                            if (response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    response.StatusCode = 200;
                                                    response.SendChunked = true;
                                                    response.OutputStream.Write(datatooutput, 0, datatooutput.Length);
                                                    response.OutputStream.Close();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                                }
                                            }
                                            else
                                            {
                                                Logger.Warn("Client Disconnected early");
                                            }

                                            break;
                                    }

                                    break;
                                }

                                break;
                            }

                        default:

                            Logger.Warn($"SVO server : {userAgent} Requested a PsHome SVO Method that doesn't exist.");

                            // Return an internal server error response
                            byte[] FileNotFound = Encoding.UTF8.GetBytes(PreMadeWebPages.filenotfound);

                            if (response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    response.StatusCode = 200;
                                    response.ContentLength64 = FileNotFound.Length;
                                    response.OutputStream.Write(FileNotFound, 0, FileNotFound.Length);
                                    response.OutputStream.Close();
                                }
                                catch (Exception ex)
                                {
                                    Logger.Warn($"Client Disconnected early and thrown an exception {ex}");
                                }
                            }
                            else
                            {
                                Logger.Warn("Client Disconnected early");
                            }

                            break;

                            #endregion
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"SVO Server : an error occured in Ps_Home Request type - {ex}");
                }

                context.Response.Close();

                GC.Collect();

                return;
            }
        }

        public static void PrepareSVOFolders()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/wwwsvoroot/HUBPS3_SVML/tracking/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/wwwsvoroot/HUBPS3_SVML/tracking/");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/wwwsvoroot/HUBPS3_SVML/fileservices/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/wwwsvoroot/HUBPS3_SVML/fileservices/");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/wwwsvoroot/dataloaderweb/queue/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/wwwsvoroot/dataloaderweb/queue/");
            }

            return;
        }
    }
}
