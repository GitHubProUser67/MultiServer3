﻿using CustomLogger;
using DotNetty.Common.Internal.Logging;
using Horizon.LIBRARY.Database.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using SVO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SVO.Games.PS3
{
    public class RFOM
    {

        public static async Task RFOM_SVO(HttpListenerContext context)
        {
            using (var response = context.Response)
            {
                try
                {
                    switch (context.Request.Url.AbsolutePath)
                    {
                        case "/RESISTANCE_SVML/index.jsp":
                            {
                                switch (context.Request.HttpMethod)
                                {
                                    case "GET":
                                        HttpListenerRequest req = context.Request;
                                        HttpListenerResponse resp = context.Response;
                                        resp.Headers.Set("Content-Type", "text/svml");

                                        string clientMac = req.Headers.Get("X-SVOMac");
                                        string serverMac = SVOProcessor.CalcuateSVOMac(clientMac);
                                        if (string.IsNullOrEmpty(serverMac))
                                        {
                                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                                            return;
                                        }
                                        else
                                        {
                                            if (!req.HasEntityBody)
                                            {
                                                response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                                                return;
                                            }

                                            resp.Headers.Set("X-SVOMac", serverMac);

                                            byte[] uriStore = null;
                                            if (SVOServerConfiguration.SVOHTTPSBypass)
                                            {
                                                uriStore = Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""UTF-8""?>   
                                            <SVML>  
                                                <SET name=""IP"" IPAddress=""127.0.0.1"" />       
                                                <DATA dataType=""URI"" name=""entryURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/Account_Login.jsp"" />  
                                                <DATA dataType=""URI"" name=""homeURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/home.jsp"" />  
                                                <DATA dataType=""URI"" name=""logoutURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/Logout.jsp"" />  
                                                <DATA dataType=""URI"" name=""syncprofileURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/syncprofile.jsp"" />  
                                                <DATA dataType=""DATA"" name=""loginEncryptedURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/account/Account_Encrypted_Login_Submit.jsp"" />  
                                                <DATA dataType=""URI"" name=""personalStatsURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/stats/personalStats.jsp"" />  
                                                <DATA dataType=""DATA"" name=""gameCreateURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/tempgame.jsp"" />  
                                                <DATA dataType=""DATA"" name=""createGameURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_Create.jsp?gameMode=%d"" />  
                                                <DATA dataType=""DATA"" name=""createGameSubmitURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_Create_Submit.jsp"" />  
                                                <DATA dataType=""DATA"" name=""finishGameURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_Finish_Submit.jsp"" />  
                                                <DATA dataType=""DATA"" name=""gamePostBinaryStatsURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_PostBinaryStats_Submit.jsp"" />  
                                                <DATA dataType=""DATA"" name=""createGamePlayerURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_Create_Player_Submit.jsp?SVOGameID=%d&amp;playerSide=%d"" />  
                                                <DATA dataType=""URI"" name=""mediusAccountLoginURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Account_Login.jsp"" />  
                                                <DATA dataType=""URI"" name=""mediusAccountCreateURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Account_Create.jsp"" />  
                                                <DATA dataType=""URI"" name=""mediusLobbyListURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Lobby_List.jsp"" />  
                                                <DATA dataType=""URI"" name=""mediusChatLobbyURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Chat_Lobby.jsp"" />    
                                                <DATA dataType=""URI"" name=""mediusChallengePopupURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Challenge_Popup.jsp"" />  
                                                <DATA dataType=""URI"" name=""mediusAcceptPopupURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Accept_Popup.jsp"" />  
                                                <DATA dataType=""DATA"" name=""tickerStrURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/ticker/TickerStr.jsp"" />  
                                                <DATA dataType=""URI"" name=""rankingsURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/ranks/rankings.jsp"" />  
                                                <DATA dataType=""URI"" name=""tourLaunchPopupURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/tourney/Tourney_AutoLaunch.jsp"" />   
                                                <DATA dataType=""DATA"" name=""tourDataURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/tourney/Tourney_CheckIn.jsp"" />  
                                                <DATA dataType=""DATA"" name=""teamTourneyMatchDataURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/teamtourney/TeamTourney_MatchData.jsp?teamTourID=%d"" />  
                                                <DATA dataType=""DATA"" name=""teamTourneyForfeitURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/teamtourney/TeamTourney_ForfeitTeam_Submit.jsp?teamTourTeamID=%d&amp;teamTourBracketID=%d&amp;teamTourID=%d"" />  
                                                <DATA dataType=""DATA"" name=""tourForfeitURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/tourney/Tourney_Forfeit_Submit.jsp"" />  
                                                <DATA dataType=""DATA"" name=""getLadderMatchDataURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/ladder/Ladder_GetMatchData.jsp?ladderMatchID=%d"" />  
                                                <DATA dataType=""DATA"" name=""getForfeitLadderMatchURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/ladder/Ladder_Forfeit_Submit.jsp?ladderMatchID=%d&amp;clanID=%d"" />  
                                                <DATA dataType=""DATA"" name=""downloadPatch"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/download/patchDownload.jsp"" />  
                                                <DATA dataType=""DATA"" name=""playerStatsURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/stats/Stats_GetPlayerStats.jsp?PlayerID=%d&amp;gameMode=%d"" />  
                                                <DATA dataType=""DATA"" name=""playerProfileURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/profile/Profile_GetPlayerProfile.jsp?PlayerID=%d"" />  
                                                <DATA dataType=""DATA"" name=""rankInfoURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/stats/Stats_CareerRankInfo.jsp?playerList=""  />   
                                                <DATA dataType=""URI"" name=""downloadVerificationURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/commerce/Commerce_VerifySubmit.jsp"" />  
                                                <DATA dataType=""DATA"" name=""purchaseListURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/commerce/Commerce_PurchaseList.jsp?categoryID=default"" />  
                                                <DATA dataType=""DATA"" name=""createVerifiedFileGameURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/commerce/Commerce_GameCreatorFileVerification.jsp?fileList=$1&amp;userPassword=$2&amp;spectatorPassword=$3"" />  
                                                <DATA dataType=""DATA"" name=""joinVerifiedFileGameURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/commerce/Commerce_GameJoinerFileVerification.jsp?fileList=$1&amp;userPassword=$2&amp;ticket=$3"" />  
                                                <DATA dataType=""DATA"" name=""spectateVerifiedFileGameURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/commerce/Commerce_GameSpectatorFileVerification.jsp?fileList=$1&amp;spectatorPassword=$2&amp;ticket=$3"" />  
                                                <DATA dataType=""DATA"" name=""TicketLoginURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/SP_Login_Submit.jsp"" />  
                                                <DATA dataType=""DATA"" name=""SetIgnoreListURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/SP_UpdateIgnoreList_Submit.jsp"" />  
                                                <DATA dataType=""DATA"" name=""SetUniversePasswordURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/SP_SetPassword_Submit.jsp"" />  
  
                                                <BROWSER_INIT name=""init"" />  
                                            </SVML>");
                                            }
                                            else
                                            {
                                                uriStore = Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""UTF-8""?>   
                                                <SVML>  
                                                    <SET name=""IP"" IPAddress=""127.0.0.1"" />       
                                                    <DATA dataType=""URI"" name=""entryURI"" value=""https://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/Account_Login.jsp"" />  
                                                    <DATA dataType=""URI"" name=""homeURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/home.jsp"" />  
                                                    <DATA dataType=""URI"" name=""logoutURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/Logout.jsp"" />  
                                                    <DATA dataType=""URI"" name=""syncprofileURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/syncprofile.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""loginEncryptedURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/account/Account_Encrypted_Login_Submit.jsp"" />  
                                                    <DATA dataType=""URI"" name=""personalStatsURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/stats/personalStats.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""gameCreateURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/tempgame.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""createGameURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_Create.jsp?gameMode=%d"" />  
                                                    <DATA dataType=""DATA"" name=""createGameSubmitURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_Create_Submit.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""finishGameURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_Finish_Submit.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""gamePostBinaryStatsURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_PostBinaryStats_Submit.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""createGamePlayerURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/game/Game_Create_Player_Submit.jsp?SVOGameID=%d&amp;playerSide=%d"" />  
                                                    <DATA dataType=""URI"" name=""mediusAccountLoginURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Account_Login.jsp"" />  
                                                    <DATA dataType=""URI"" name=""mediusAccountCreateURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Account_Create.jsp"" />  
                                                    <DATA dataType=""URI"" name=""mediusLobbyListURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Lobby_List.jsp"" />  
                                                    <DATA dataType=""URI"" name=""mediusChatLobbyURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Chat_Lobby.jsp"" />    
                                                    <DATA dataType=""URI"" name=""mediusChallengePopupURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Challenge_Popup.jsp"" />  
                                                    <DATA dataType=""URI"" name=""mediusAcceptPopupURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/medius/Medius_Accept_Popup.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""tickerStrURL"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/ticker/TickerStr.jsp"" />  
                                                    <DATA dataType=""URI"" name=""rankingsURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/ranks/rankings.jsp"" />  
                                                    <DATA dataType=""URI"" name=""tourLaunchPopupURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/tourney/Tourney_AutoLaunch.jsp"" />   
                                                    <DATA dataType=""DATA"" name=""tourDataURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/tourney/Tourney_CheckIn.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""teamTourneyMatchDataURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/teamtourney/TeamTourney_MatchData.jsp?teamTourID=%d"" />  
                                                    <DATA dataType=""DATA"" name=""teamTourneyForfeitURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/teamtourney/TeamTourney_ForfeitTeam_Submit.jsp?teamTourTeamID=%d&amp;teamTourBracketID=%d&amp;teamTourID=%d"" />  
                                                    <DATA dataType=""DATA"" name=""tourForfeitURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/tourney/Tourney_Forfeit_Submit.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""getLadderMatchDataURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/ladder/Ladder_GetMatchData.jsp?ladderMatchID=%d"" />  
                                                    <DATA dataType=""DATA"" name=""getForfeitLadderMatchURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/ladder/Ladder_Forfeit_Submit.jsp?ladderMatchID=%d&amp;clanID=%d"" />  
                                                    <DATA dataType=""DATA"" name=""downloadPatch"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/download/patchDownload.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""playerStatsURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/stats/Stats_GetPlayerStats.jsp?PlayerID=%d&amp;gameMode=%d"" />  
                                                    <DATA dataType=""DATA"" name=""playerProfileURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/profile/Profile_GetPlayerProfile.jsp?PlayerID=%d"" />  
                                                    <DATA dataType=""DATA"" name=""rankInfoURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/stats/Stats_CareerRankInfo.jsp?playerList=""  />   
                                                    <DATA dataType=""URI"" name=""downloadVerificationURI"" value=""http://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/commerce/Commerce_VerifySubmit.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""purchaseListURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/commerce/Commerce_PurchaseList.jsp?categoryID=default"" />  
                                                    <DATA dataType=""DATA"" name=""createVerifiedFileGameURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/commerce/Commerce_GameCreatorFileVerification.jsp?fileList=$1&amp;userPassword=$2&amp;spectatorPassword=$3"" />  
                                                    <DATA dataType=""DATA"" name=""joinVerifiedFileGameURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/commerce/Commerce_GameJoinerFileVerification.jsp?fileList=$1&amp;userPassword=$2&amp;ticket=$3"" />  
                                                    <DATA dataType=""DATA"" name=""spectateVerifiedFileGameURI"" value=""https://resistanceps3.svo.online.scea.com:10061/RESISTANCE_SVML/commerce/Commerce_GameSpectatorFileVerification.jsp?fileList=$1&amp;spectatorPassword=$2&amp;ticket=$3"" />  
                                                    <DATA dataType=""DATA"" name=""TicketLoginURI"" value=""https://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/SP_Login_Submit.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""SetIgnoreListURI"" value=""https://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/SP_UpdateIgnoreList_Submit.jsp"" />  
                                                    <DATA dataType=""DATA"" name=""SetUniversePasswordURI"" value=""https://resistanceps3.svo.online.scea.com:10060/RESISTANCE_SVML/account/SP_SetPassword_Submit.jsp"" />  
  
                                                    <BROWSER_INIT name=""init"" />  
                                                </SVML>");
                                            }

                                            resp.OutputStream.Write(uriStore);
#if DEBUG
                                            LoggerAccessor.LogInfo($"Start URIStore for Resistance: Fall of Man SENT!");
#endif
                                        }
                                        break;
                                }
                                break;
                            }

                        #region TicketLogin
                        case "/RESISTANCE_SVML/account/SP_Login_Submit.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "POST":
                                    HttpListenerRequest req = context.Request;
                                    HttpListenerResponse resp = context.Response;

                                    string psnname = string.Empty;

                                    resp.Headers.Set("Content-Type", "text/svml;charset=UTF-8");
                                    string clientMac = req.Headers.Get("X-SVOMac");

                                    string sig = HttpUtility.ParseQueryString(req.Url.Query).Get("sig");
                                    string serverMac = SVOProcessor.CalcuateSVOMac(clientMac);

                                    resp.Headers.Set("X-SVOMac", serverMac);
                                    if (string.IsNullOrEmpty(serverMac))
                                    {
                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                        return;
                                    }
                                    else
                                    {
                                        if (!req.HasEntityBody)
                                        {
                                            response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                                            return;
                                        }
                                        int appId = 20174;
                                        string s = string.Empty;

                                        // Get the data from the HTTP stream
                                        using (StreamReader reader = new(req.InputStream, req.ContentEncoding))
                                        {
                                            // Convert the data to a string and display it on the console.
                                            s = reader.ReadToEnd();
                                            reader.Close();
                                        }

                                        byte[] bytes = Encoding.ASCII.GetBytes(s);
                                        int AcctNameLen = Convert.ToInt32(bytes.GetValue(81));

                                        string acctName = s.Substring(82, 32);

                                        string acctNameREX = Regex.Replace(acctName, @"[^a-zA-Z0-9]+", string.Empty);

#if DEBUG
                                        LoggerAccessor.LogInfo($"Logging user {acctNameREX} into SVO...\n");
#endif
                                        int accountId = -1;
                                        string langId = string.Empty;

                                        try
                                        {
                                            await SVOServerConfiguration.Database.GetAccountByName(acctNameREX, appId).ContinueWith((r) =>
                                            {
                                                //Found in database so keep.
                                                langId = req.Url.Query.Substring(94, req.Url.Query.Length - 94);
                                                if (r.Result != null)
                                                    accountId = r.Result.AccountId;
                                            });
                                        }
                                        catch (Exception)
                                        {
                                            langId = req.Url.Query.Substring(94, req.Url.Query.Length - 94);
                                            accountId = 0;
                                        }
                                    

                                        int clanId = -1;

                                        response.AppendHeader("Set-Cookie", $"AcctID=1");
                                        response.AppendHeader("Set-Cookie", $"NPCountry=us");
                                        response.AppendHeader("Set-Cookie", $"ClanID={clanId}");
                                        response.AppendHeader("Set-Cookie", $"AuthKeyTime=03-11-2023 16:03:41");
                                        response.AppendHeader("Set-Cookie", $"NPLang=1");
                                        response.AppendHeader("Set-Cookie", $"ModerateMode=false");
                                        response.AppendHeader("Set-Cookie", $"TimeZone=PST");
                                        response.AppendHeader("Set-Cookie", $"AssocID=-1");
                                        response.AppendHeader("Set-Cookie", $"NPContentRating=201326592");
                                        response.AppendHeader("Set-Cookie", $"AuthKey=nRqnf97f~UaSANLErurJIzq9GXGWqWCADdA3TfqUIVXXisJyMnHsQ34kA&C^0R#&~JULZ7xUOY*rXW85slhQF&P&Eq$7kSB&VBtf`V8rb^BC`53jGCgIT");
                                        response.AppendHeader("Set-Cookie", $"AcctName={psnname}");
                                        response.AppendHeader("Set-Cookie", $"OwnerID=-255");
                                        response.AppendHeader("Set-Cookie", $"Sig={sig}==");
                                        response.AppendHeader("Set-Cookie", $"AppId={appId}");


                                        //resp.SetCookie(new Cookie("LangID", langId));
                                        //resp.SetCookie(new Cookie("AcctID", accountId.ToString()));

                                        string sp_Login = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                            "<SVML>\r\n" +
                                            "    <SP_Login>\r\n" +
                                            "        <status>\r\n" +
                                            "            <id>20600</id>\r\n" +
                                            "            <message>ACCT_LOGIN_SUCCESS</message>\r\n" +
                                            "        </status>\r\n" +
                                           $"        <accountID>1</accountID>\r\n" +
                                            "        <userContext>0</userContext>\r\n" +
                                            "    </SP_Login>\r\n\t\r\n" +
                                            "</SVML>";

                                        resp.OutputStream.Write(Encoding.ASCII.GetBytes(sp_Login));
#if DEBUG
                                        LoggerAccessor.LogInfo($"Player {psnname} successfully logged into SVO!\n");
#endif
                                    }
                                    break;
                            }
                            break;
                        #endregion

                        #region Game

                        case "/RESISTANCE_SVML/game/Game_Create.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":
                                    HttpListenerRequest req = context.Request;
                                    HttpListenerResponse resp = context.Response;
                                    resp.Headers.Set("Content-Type", "text/svml;charset=UTF-8");


                                    string clientMac = req.Headers.Get("X-SVOMac");
                                    string serverMac = SVOProcessor.CalcuateSVOMac(clientMac);
                                    if (string.IsNullOrEmpty(serverMac))
                                    {
                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                        return;
                                    }
                                    else
                                    {
                                        if (!req.HasEntityBody)
                                        {
                                            response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                                            return;
                                        }
                                        resp.Headers.Set("X-SVOMac", serverMac);

                                        resp.Headers.Set("x-statuscode", "0");
                                        resp.StatusCode = 200;

                                        string game_Create_Submit = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                            "<SVML>\r\n" +
                                            "    <Create_Game>\r\n" +
                                            "       <GamePlayer>\r\n" +
                                            "        <status>\r\n" +
                                            "            <id>20422</id>\r\n" +
                                            "            <message>GAME_CREATE_SUCCESS</message>\r\n" +
                                            "        </status>\r\n" +
                                            $"       <gameID>{HttpUtility.ParseQueryString(req.Url.Query).Get("scertGameID")}</gameID>\r\n" +
                                            "       </GamePlayer>\r\n" +
                                            "    </Create_Game>\r\n" +
                                            "</SVML>";

                                        resp.OutputStream.Write(Encoding.ASCII.GetBytes(game_Create_Submit));
#if DEBUG
                                        LoggerAccessor.LogInfo($"SVO Game created with SVOGameID {HttpUtility.ParseQueryString(req.Url.Query).Get("scertGameID")}");
#endif
                                    }
                                    break;

                            }
                            break;

                        case "/BOURBON_XML/game/Game_Create_Player_Submit.do":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":
                                    HttpListenerRequest req = context.Request;
                                    HttpListenerResponse resp = context.Response;
                                    resp.Headers.Set("Content-Type", "text/svml;charset=UTF-8");


                                    string clientMac = req.Headers.Get("X-SVOMac");
                                    string serverMac = SVOProcessor.CalcuateSVOMac(clientMac);
                                    if (string.IsNullOrEmpty(serverMac))
                                    {
                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                        return;
                                    }
                                    else
                                    {
                                        if (!req.HasEntityBody)
                                        {
                                            response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                                            return;
                                        }

                                        resp.Headers.Set("X-SVOMac", serverMac);

                                        resp.Headers.Set("x-statuscode", "0");
                                        resp.StatusCode = 200;

                                        string game_Create_Submit = @"<XML>
                                            <GamePlayer>
                                                <status>
                                                    <id>20422</id>
                                                    <message>GAME_CREATE_PLAYER_SUCCESS</message>
                                                </status>
                                            </GamePlayer>
                                        </XML>";

                                        resp.OutputStream.Write(Encoding.ASCII.GetBytes(game_Create_Submit));
                                        LoggerAccessor.LogInfo($"SVO Game Player created with SVOGameID {HttpUtility.ParseQueryString(req.Url.Query).Get("SVOGameID")} and playerside {HttpUtility.ParseQueryString(req.Url.Query).Get("playerSide")}");
                                    }
                                    break;

                            }
                            break;

                        case "/RESISTANCE_SVML/game/Game_PostBinaryStats_Submit.jsp":
                            switch (context.Request.HttpMethod)
                            {
                                case "POST":
                                    HttpListenerRequest req = context.Request;
                                    HttpListenerResponse resp = context.Response;
                                    resp.Headers.Set("Content-Type", "text/svml;charset=UTF-8");


                                    string clientMac = req.Headers.Get("X-SVOMac");
                                    string serverMac = SVOProcessor.CalcuateSVOMac(clientMac);
                                    if (serverMac == null)
                                    {
                                        string forbidden = "500 Forbidden";
                                        LoggerAccessor.LogWarn("Client connected abnormally... returning 500 Forbidden");

                                        resp.StatusCode = 500;
                                        resp.OutputStream.Write(Encoding.ASCII.GetBytes(forbidden));
                                    }
                                    else
                                    {

                                        var formData = HttpUtility.ParseQueryString(req.QueryString.ToString());

                                        Console.WriteLine(formData.AllKeys.ToString());

                                        byte[] urlEncoded;

                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            StreamWriter writer = new StreamWriter(ms);


                                            // Find number of bytes in stream.
                                            int strLen = Convert.ToInt32(req.ContentLength64);
                                            // Create a byte array.
                                            urlEncoded = new byte[strLen];

                                            req.InputStream.Read(urlEncoded, 0, strLen);

                                            //You have to rewind the MemoryStream before copying
                                            ms.Read(urlEncoded, 0, strLen);

                                            Console.WriteLine($"Stats attempting to be uploaded: {urlEncoded}");

                                            /*
                                            using (FileStream fs = new FileStream($"./BOURBON_XML/fileservices/{toUpload}", FileMode.OpenOrCreate))
                                            {
                                                fs.Write(strArr, 0, strLen);
                                                fs.Flush();
                                                Logger.Info($"File {toUpload} has been written to SVO\n");
                                            }
                                            */
                                        }


                                        // Split the URL-encoded string based on "endStatus"
                                        string[] parts = Encoding.UTF8.GetString(urlEncoded, 0, urlEncoded.Length).Split(new[] { "&endStatus=1", "&endStatus=2", "&endStatus=3", "&endStatus=4", "&endStatus=5", "&endStatus=6", "&endStatus=7", }, StringSplitOptions.None);

                                        // Create a directory to store the binary blob files
                                        string directoryPath = "wwwsvoroot/BOURBON_XML/stats/StatBlobs/";
                                        Directory.CreateDirectory(directoryPath);

                                        // Initialize a list to store statistics for each part
                                        List<Dictionary<string, string>> statisticsList = new List<Dictionary<string, string>>();




                                        // Iterate through each part
                                        foreach (var part in parts)
                                        {
                                            // Parse the part as a URL-encoded string
                                            var parsedData = HttpUtility.ParseQueryString(part);

                                            // Check if the part contains the "AccountID" key
                                            if (parsedData.AllKeys.Contains("AccountID"))
                                            {
                                                // Extract the AccountID and statistics for the part
                                                string accountId = parsedData["AccountID"];
                                                Dictionary<string, string> statistics = new Dictionary<string, string>();

                                                // Iterate through all keys except "AccountID" to collect statistics
                                                foreach (var key in parsedData.AllKeys)
                                                {
                                                    if (key != "AccountID")
                                                    {
                                                        statistics[key] = parsedData[key];
                                                    }

                                                    switch (key)
                                                    {
                                                        //PlayerDetails
                                                        case "type1":
                                                            {
                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data1"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        //PlayerSummary
                                                        case "type2":
                                                            {
                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data2"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        case "type3":
                                                            {
                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data3"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");

                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        case "type4":
                                                            {
                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data4"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        case "type5":
                                                            {
                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data5"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        case "type6":
                                                            {

                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data6"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        //TroopWeaponsSummary
                                                        case "type7":
                                                            {
                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data7"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");

                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        // StarhawkWeaponsSummary
                                                        case "type8":
                                                            {

                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data8"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");

                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        case "type9":
                                                            {

                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data9"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        case "type100":
                                                            {

                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data100"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        case "type102":
                                                            {

                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data102"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        case "type300":
                                                            {

                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data300"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        case "type500":
                                                            {
                                                                // Convert Base64 to bytes
                                                                byte[] data = Convert.FromBase64String(parsedData["data500"]);
                                                                string filePath = Path.Combine(directoryPath, $"svoStatsBlob-a{accountId}-{key}.bin");
                                                                File.WriteAllBytes(filePath, data);
                                                            }
                                                            break;
                                                        default:
                                                            {
                                                                LoggerAccessor.LogWarn($"Unhandled Stat type! {key}");

                                                            }
                                                            break;
                                                    }
                                                }

                                                // Store the statistics in the list
                                                statisticsList.Add(statistics);
                                                //Program.Database
                                            }
                                        }

                                        //resp.Headers.Set("X-SVOMac", serverMac);


                                        resp.Headers.Set("x-statuscode", "0");
                                        resp.StatusCode = 200;

                                        string game_Create_Submit = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                            "<SVML>\r\n" +
                                            "    <Finish_Game>\r\n" +
                                            "        <status>\r\n" +
                                            "            <id>41010</id>\r\n" +
                                            "            <message>GAME_FINISH_SUCCESS</message>\r\n" +
                                            "        </status>\r\n" +
                                            "    </Finish_Game>\r\n" +
                                            "</SVML>";


                                        resp.OutputStream.Write(Encoding.ASCII.GetBytes(game_Create_Submit));
                                    }


                                    break;

                            }
                            break;

                            #endregion

                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[SVO] - RFOM_SVO thrown an assertion - {ex}");
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
        }
    }

}