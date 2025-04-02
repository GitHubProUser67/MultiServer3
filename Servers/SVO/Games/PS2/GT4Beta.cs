﻿using CustomLogger;
using System.Net;
using System.Text;

namespace Server.SVO_CLI.SVOGames
{
    public class GT4Beta
    {


        public static void GT4_XML_SVO(HttpListenerContext context)
        {
            using (var resp = context.Response)
            {
                try
                {
                    switch (context.Request.Url.AbsolutePath)
                    {
                        case "/GT4_XML/index.jsp":
                            {
                                switch (context.Request.HttpMethod)
                                {
                                    case "GET":
                                        HttpListenerRequest req = context.Request;
                                        resp.Headers.Set("Content-Type", "text/xml");

                                        byte[] uriStore = Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                            "<XML>\r\n\t" +
                                            "<URL_List>\r\n\t\t  <SET IPAddress=\"127.0.0.1\" />\r\n\t\t" +
                                            "\r\n\t\t" +
                                            "<Login loginURL=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/account/Account_Login.jsp\"/>\r\n\t\t" +
                                            "<Logout loginURL=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/account/Account_Logout.jsp\"/>\r\n\t\t" +
                                            "<Create_Game gameArcadeCreateURL=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/game/Game_Arcade_Create_Submit.jsp\"/>\r\n\t\t" +
                                            "\r\n\t\t  <DATA dataType=\"URI\" name=\"homeURI\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/home\" />\r\n\t\t" +
                                            "<DATA dataType=\"URI\" name=\"StaticHomeURI\" value=\"static://staticHome\" />\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"playerProfileURI\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/playerProfile\" />\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"entryURI\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/account/login\" />\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"loginEncryptedURI\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/loginEncrypted\" />\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"rankInfoURI\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/account/rankInfo\" />\r\n\t\t" +
                                            "<DATA dataType=\"URI\" name=\"loginURL\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/account/Account_Login.jsp\" />  \r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"playerStatsURI\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/account/stats\" />\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"playerProfileURI\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/account/profile\" />\r\n\t\t" +
                                            "<DATA dataType=\"URI\" name=\"LogoutURL\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/account/Account_Logout.jsp\"/>\r\n\t\t\r\n\t\t" +
                                            "<DATA dataType=\"URI\" name=\"gameTimeTrialCreateURL\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/game/Game_TimeTrial_Create_Submit.jsp\"/>\r\n\t\t" +
                                            "<DATA dataType=\"URI\" name=\"gameArcadeCreateURL\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/game/Game_Arcade_Create_Submit.jsp\"/>\r\n\t\t" +
                                            "<DATA dataType=\"URI\" name=\"gameFinishURL\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/game/Game_Finish_Submit.jsp\"/>\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"loginE3URL\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/account/loginE3.jsp\" />\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"GetTrialRankingUrl\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/ranking/TrialRanking.jsp\" />\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"statsLocationURL\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/stats/statsLocation.jsp\" />\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"statsTrialURL\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/stats/statsTrial.jsp\" />\r\n\t\t" +
                                            "<DATA dataType=\"DATA\" name=\"statsTrialPositionURL\" value=\"http://gt4-pubeta.svo.pdonline.scea.com:10060/GT4_XML/stats/statsTrialPosition.jsp\" />\r\n\t\t" +
                                            "<BROWSER_INIT name=\"init\" />\r\n\t" +
                                            "</URL_List>\r\n" +
                                            "</XML>");

                                        resp.ContentLength64 = uriStore.Length;
                                        resp.OutputStream.Write(uriStore);
                                        break;
                                }

                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[SVO] - GT4Beta_SVO thrown an assertion - {ex}");
                    resp.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
        }
    }

}