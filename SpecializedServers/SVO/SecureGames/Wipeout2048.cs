
using CustomLogger;
using CyberBackendLibrary.DataTypes;
using HttpMultipartParser;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WatsonWebserver.Core;

namespace SVO
{
    public class Wipeout2048
    {
        public static async Task<bool> Wipeout2048_OTG(HttpContextBase ctx, string absolutepath)
        {
            try
            {
                if (ctx.Request.Url == null)
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    ctx.Response.ContentType = "text/plain";
                    return await ctx.Response.Send();
                }

                string method = ctx.Request.Method.ToString();

                switch (absolutepath)
                {
                    #region Wipeout2048
                    case "/wox_ws/rest/main/Start":

                        switch (method)
                        {
                            case "GET":
                                ctx.Response.ChunkedTransfer = true;

                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", string.Empty);

                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<Start>" +
                                        "<DATA dataType=\"URI\" name=\"startURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/main/Start\"/>" +
                                        "<DATA dataType=\"URI\" name=\"ticketLoginURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/account/TicketLogin\"/>" +
                                        "<DATA dataType=\"URI\" name=\"friendsUploadURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/account/Friends\"/>" +
                                        "<DATA dataType=\"URI\" name=\"friendsDownloadURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/account/Friends\"/>" +
                                        "<DATA dataType=\"URI\" name=\"binaryUploadURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/binary/Upload\"/>" +
                                        "<DATA dataType=\"URI\" name=\"binaryDownloadURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/binary/Download\"/>" +
                                        "<DATA dataType=\"URI\" name=\"eulaURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/legal/Eula\"/>" +
                                        "<DATA dataType=\"URI\" name=\"announcementsURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/legal/Announcements\"/>" +
                                        "<DATA dataType=\"URI\" name=\"postScoreURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/games/PostScore\"/>" +
                                        "<DATA dataType=\"URI\" name=\"postWO2048ScoreURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/games/PostWO2048Score\"/>" +
                                        "<DATA dataType=\"URI\" name=\"leaderboardsURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/{leaderboardId}\"/>" +
                                        "<DATA dataType=\"URI\" name=\"leaderboardsListURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/GetList\"/>" +
                                        "<DATA dataType=\"URI\" name=\"leaderboardsPageURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/GetPage\"/>" +
                                        "<DATA dataType=\"URI\" name=\"mediusStatsURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/GetMediusStats\"/>" +
                                        "<DATA dataType=\"URI\" name=\"playerTimesURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/GetPlayerTimes\"/>" +
                                        "<DATA dataType=\"URI\" name=\"guessRankingURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/GuessRanking\"/>" +
                                        "<DATA dataType=\"URI\" name=\"rankedConfigURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/GetRankedConfig\"/>" +
                                        "<DATA dataType=\"URI\" name=\"leaderboardScoreRangeURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lbNGP/GetLeaderboardRange\"/>" +
                                        "<DATA dataType=\"URI\" name=\"leaderboards2048URL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lbNGP/{leaderboardId}\"/>" +
                                        "<DATA dataType=\"URI\" name=\"leaderboards2048PageURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lbNGP/GetPage\"/>" +
                                        "<DATA dataType=\"URI\" name=\"leaderboards2048XPURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lbNGP/GetXPLeaderboard\"/>" +
                                        "<DATA dataType=\"URI\" name=\"ghostUploadURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/ghost/Upload\"/>" +
                                        "<DATA dataType=\"URI\" name=\"ghostDownloadURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/ghost/Download\"/>" +
                                        "<DATA dataType=\"URI\" name=\"friendActivitiesURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/activities/FriendActivities\"/>" +
                                        "<DATA dataType=\"URI\" name=\"uploadEventsURL\" value=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/activities/UploadEvents\"/>" +
                                        "<DATA dataType=\"URI\" name=\"frameDataUploadURL\" value=\"https://wipeout2048.online.scee.com:10062/FrameRateAnalizer/gui/main/uploadFrameData\"/>" +
                                        "</Start>"));
                        }
                        break;

                    case "/wox_ws/rest/lb/GetRankedConfig":

                        switch (method)
                        {
                            case "GET":
                                ctx.Response.ChunkedTransfer = true;

                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", string.Empty);

                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<GetRankedConfig>\r\n\t" +
                                        "<stage1>\r\n\t\t" +
                                        "<modes size=\"8\">\r\n\t\t\t" +
                                        "<mode name=\"Multiplayer Single Race\" minplayer=\"5\" maxplayer=\"8\">\r\n\t\t\t\t" +
                                        "<points position=\"1\" value=\"16\"/>\r\n\t\t\t\t" +
                                        "<points position=\"2\" value=\"14\"/>\r\n\t\t\t\t" +
                                        "<points position=\"3\" value=\"12\"/>\r\n\t\t\t\t" +
                                        "<points position=\"4\" value=\"10\"/>\r\n\t\t\t\t" +
                                        "<points position=\"5\" value=\"8\"/>\r\n\t\t\t\t" +
                                        "<points position=\"6\" value=\"6\"/>\r\n\t\t\t\t" +
                                        "<points position=\"7\" value=\"4\"/>\r\n\t\t\t\t" +
                                        "<points position=\"8\" value=\"2\"/>\r\n\t\t\t" +
                                        "</mode>\r\n\r\n\t\t\t" +
                                        "<mode name=\"Multiplayer Single Race\" minplayer=\"2\" maxplayer=\"4\">\r\n\t\t\t\t" +
                                        "<points position=\"1\" value=\"10\"/>\r\n\t\t\t\t" +
                                        "<points position=\"2\" value=\"8\"/>\r\n\t\t\t\t" +
                                        "<points position=\"3\" value=\"6\"/>\r\n\t\t\t\t" +
                                        "<points position=\"4\" value=\"4\"/>\r\n\t\t\t" +
                                        "</mode>\r\n\r\n\t\t\t" +
                                        "<mode name=\"Multiplayer Tournament\" minplayer=\"5\" maxplayer=\"8\">\r\n\t\t\t\t" +
                                        "<points position=\"1\" value=\"16\"/>\r\n\t\t\t\t" +
                                        "<points position=\"2\" value=\"14\"/>\r\n\t\t\t\t" +
                                        "<points position=\"3\" value=\"12\"/>\r\n\t\t\t\t" +
                                        "<points position=\"4\" value=\"10\"/>\r\n\t\t\t\t" +
                                        "<points position=\"5\" value=\"8\"/>\r\n\t\t\t\t" +
                                        "<points position=\"6\" value=\"6\"/>\r\n\t\t\t\t" +
                                        "<points position=\"7\" value=\"4\"/>\r\n\t\t\t\t" +
                                        "<points position=\"8\" value=\"2\"/>\r\n\t\t\t" +
                                        "</mode>\r\n\r\n\t\t\t" +
                                        "<mode name=\"Multiplayer Tournament\" minplayer=\"2\" maxplayer=\"4\">\r\n\t\t\t\t" +
                                        "<points position=\"1\" value=\"10\"/>\r\n\t\t\t\t" +
                                        "<points position=\"2\" value=\"8\"/>\r\n\t\t\t\t" +
                                        "<points position=\"3\" value=\"6\"/>\r\n\t\t\t\t" +
                                        "<points position=\"4\" value=\"4\"/>\r\n\t\t\t" +
                                        "</mode>\r\n\r\n\t\t\t" +
                                        "<mode name=\"Multiplayer Elimination\" minplayer=\"5\" maxplayer=\"8\">\r\n\t\t\t\t" +
                                        "<points position=\"1\" value=\"16\"/>\r\n\t\t\t\t" +
                                        "<points position=\"2\" value=\"14\"/>\r\n\t\t\t\t" +
                                        "<points position=\"3\" value=\"12\"/>\r\n\t\t\t\t" +
                                        "<points position=\"4\" value=\"10\"/>\r\n\t\t\t\t" +
                                        "<points position=\"5\" value=\"8\"/>\r\n\t\t\t\t" +
                                        "<points position=\"6\" value=\"6\"/>\r\n\t\t\t\t" +
                                        "<points position=\"7\" value=\"4\"/>\r\n\t\t\t\t" +
                                        "<points position=\"8\" value=\"2\"/>\r\n\t\t\t" +
                                        "</mode>\r\n\r\n\t\t\t" +
                                        "<mode name=\"Multiplayer Elimination\" minplayer=\"2\" maxplayer=\"4\">\r\n\t\t\t\t" +
                                        "<points position=\"1\" value=\"10\"/>\r\n\t\t\t\t" +
                                        "<points position=\"2\" value=\"8\"/>\r\n\t\t\t\t" +
                                        "<points position=\"3\" value=\"6\"/>\r\n\t\t\t\t" +
                                        "<points position=\"4\" value=\"4\"/>\r\n\t\t\t" +
                                        "</mode>\r\n\r\n\t\t\t" +
                                        "<mode name=\"Multiplayer Nitro Battle\" minplayer=\"5\" maxplayer=\"8\">\r\n\t\t\t\t" +
                                        "<points position=\"1\" value=\"16\"/>\r\n\t\t\t\t" +
                                        "<points position=\"2\" value=\"14\"/>\r\n\t\t\t\t" +
                                        "<points position=\"3\" value=\"12\"/>\r\n\t\t\t\t" +
                                        "<points position=\"4\" value=\"10\"/>\r\n\t\t\t\t" +
                                        "<points position=\"5\" value=\"8\"/>\r\n\t\t\t\t" +
                                        "<points position=\"6\" value=\"6\"/>\r\n\t\t\t\t" +
                                        "<points position=\"7\" value=\"4\"/>\r\n\t\t\t\t" +
                                        "<points position=\"8\" value=\"2\"/>\r\n\t\t\t" +
                                        "</mode>\r\n\r\n\t\t\t" +
                                        "<mode name=\"Multiplayer Nitro Battle\" minplayer=\"2\" maxplayer=\"4\">\r\n\t\t\t\t" +
                                        "<points position=\"1\" value=\"10\"/>\r\n\t\t\t\t" +
                                        "<points position=\"2\" value=\"8\"/>\r\n\t\t\t\t" +
                                        "<points position=\"3\" value=\"6\"/>\r\n\t\t\t\t" +
                                        "<points position=\"4\" value=\"4\"/>\r\n\t\t\t" +
                                        "</mode>\r\n\t\t" +
                                        "</modes>\r\n\r\n\t" +
                                        "</stage1>\r\n\r\n\r\n\t" +
                                        "<stage2>\r\n\t\t" +
                                        "<basepoints>10</basepoints>\r\n\t\t" +
                                        "<quitpoints>100</quitpoints>\r\n\t\t" +
                                        "<tournypoints>50</tournypoints>\r\n\t\t" +
                                        "<tournyraces>4</tournyraces>\r\n\t\t" +
                                        "<relrankpoints>10</relrankpoints>\r\n\t" +
                                        "</stage2>\r\n\r\n\t" +
                                        "<stage3 awards=\"5\" badges=\"0\">\r\n\t\t" +
                                        "<table type=\"award\" category=\"weapons\" size=\"18\">\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_QUAKER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_ROCKETEER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_MISSILER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_MINER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_BOMBER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_CANNONEER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_PLASMAIFIER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_LEECHER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_QUAKE_WIPEOUT\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_ROCKET_WIPEOUT\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_MISSILE_WIPEOUT\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_MINE_WIPEOUT\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_BOMB_WIPEOUT\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_CANNON_WIPEOUT\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_PLASMA_WIPEOUT\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_LEECH_WIPEOUT\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_FURY\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_ANNIHILATOR\" value=\"0\"/>\r\n\t\t" +
                                        "</table>\r\n\r\n\t\t" +
                                        "<table type=\"award\" category=\"onlineplayteams\" size=\"5\">\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_RECRUIT\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_REGULAR\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_VETERAN\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_LEGEND\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_ZONER\" value=\"0\"/>\r\n\t\t" +
                                        "</table>\r\n\r\n\r\n\r\n\t\t" +
                                        "<table type=\"award\" category=\"tournaments\" size=\"10\">\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_POISONOUS\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_SHINY\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_SHARP\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_SPOOKY\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_POISHINARPOOKY\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_DEADLY\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_DAZZLER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_BARBED\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_NIGHTMARE\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_DEZZARMARE\" value=\"0\"/>\r\n\t\t" +
                                        "</table>\r\n\r\n\t\t" +
                                        "<table type=\"award\" category=\"tracks\" size=\"8\">\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_WINDOW_SHOPPER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_ESKIMO\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_PASSENGER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_CITYSLICKER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_SCUBA_DIVER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_BEACH_BUM\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_HIGH_ROLLER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_AIR_CADET\" value=\"0\"/>\r\n\t\t" +
                                        "</table>\r\n\r\n\r\n\t\t" +
                                        "<table type=\"award\" category=\"badges\" size=\"31\">\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_COME_BACK\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_SURVIVOR\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_GLOBETROTTER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_SPIROGYRA\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_YEAR_TO_REMEMBER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_DEFENDER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_PROTECTOR\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_LAZY\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_SONIC_BOOM\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_WARP\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_UNQUAKABLE\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_UNSTOPPABLE\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_UNBREAKABLE\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_INDESTRUCTIBLE\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_UNBEATABLE\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_NEAR_DEATH_EXPERIENCE\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_MINE_SWEEPER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_BOMB_DISPOSER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_FEROCIOUS\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_PHOTO_FINISH\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_PRISTINE\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_AIRTIME\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_REFLEX\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_TRANSONIC\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_CLOSE_CALL\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_RUTHLESS\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_DESTRUCTION_DEALER\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_DESTRUCTION_DEVA\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_TORTURE\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_SUBSONIC\" value=\"0\"/>\r\n\t\t\t" +
                                        "<entry type=\"BADGE_NAME_JUST_IN_TIME\" value=\"0\"/>\r\n\t\t" +
                                        "</table>\r\n\r\n\t" +
                                        "</stage3>\r\n\r\n\t" +
                                        "<stage4>\r\n\t\t" +
                                        "<scaler size=\"4\">\r\n\t\t\t" +
                                        "<multiplier class=\"Venom\" value=\"1\"/>\r\n\t\t\t" +
                                        "<multiplier class=\"Flash\" value=\"1\"/>\r\n\t\t\t" +
                                        "<multiplier class=\"Rapier\" value=\"1.2\"/>\r\n\t\t\t" +
                                        "<multiplier class=\"Phantom\" value=\"1.2\"/>\r\n\t\t" +
                                        "</scaler>\r\n\t" +
                                        "</stage4>\r\n\r\n\t" +
                                        "<stage5 maxlevels=\"101\" maxpoints=\"2199999\">\r\n\t\t" +
                                        "<table>\r\n\t\t\t" +
                                        "<rank level=\"1\" minpoints=\"1\" maxpoints=\"199\" name=\"RANK_TRAINEE_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"2\" minpoints=\"200\" maxpoints=\"399\" name=\"RANK_TRAINEE_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"3\" minpoints=\"400\" maxpoints=\"599\" name=\"RANK_TRAINEE_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"4\" minpoints=\"600\" maxpoints=\"799\" name=\"RANK_TRAINEE_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"5\" minpoints=\"800\" maxpoints=\"999\" name=\"RANK_TRAINEE_5\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"6\" minpoints=\"1000\" maxpoints=\"1499\" name=\"RANK_ROOKIE_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"7\" minpoints=\"1500\" maxpoints=\"1999\" name=\"RANK_ROOKIE_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"8\" minpoints=\"2000\" maxpoints=\"2499\" name=\"RANK_ROOKIE_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"9\" minpoints=\"2500\" maxpoints=\"2999\" name=\"RANK_ROOKIE_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"10\" minpoints=\"3000\" maxpoints=\"3499\" name=\"RANK_ROOKIE_5\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"11\" minpoints=\"3500\" maxpoints=\"4499\" name=\"RANK_AMATEUR_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"12\" minpoints=\"4500\" maxpoints=\"5499\" name=\"RANK_AMATEUR_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"13\" minpoints=\"5500\" maxpoints=\"6499\" name=\"RANK_AMATEUR_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"14\" minpoints=\"6500\" maxpoints=\"7499\" name=\"RANK_AMATEUR_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"15\" minpoints=\"7500\" maxpoints=\"8499\" name=\"RANK_AMATEUR_5\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"16\" minpoints=\"8500\" maxpoints=\"9999\" name=\"RANK_PROFESSIONAL_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"17\" minpoints=\"10000\" maxpoints=\"11499\" name=\"RANK_PROFESSIONAL_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"18\" minpoints=\"11500\" maxpoints=\"12999\" name=\"RANK_PROFESSIONAL_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"19\" minpoints=\"13000\" maxpoints=\"14499\" name=\"RANK_PROFESSIONAL_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"20\" minpoints=\"14500\" maxpoints=\"15999\" name=\"RANK_PROFESSIONAL_5\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"21\" minpoints=\"16000\" maxpoints=\"17999\" name=\"RANK_EXPERT_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"22\" minpoints=\"18000\" maxpoints=\"19999\" name=\"RANK_EXPERT_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"23\" minpoints=\"20000\" maxpoints=\"21999\" name=\"RANK_EXPERT_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"24\" minpoints=\"22000\" maxpoints=\"23999\" name=\"RANK_EXPERT_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"25\" minpoints=\"24000\" maxpoints=\"25999\" name=\"RANK_EXPERT_5\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"26\" minpoints=\"26000\" maxpoints=\"29999\" name=\"RANK_VETERAN_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"27\" minpoints=\"30000\" maxpoints=\"33999\" name=\"RANK_VETERAN_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"28\" minpoints=\"34000\" maxpoints=\"37999\" name=\"RANK_VETERAN_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"29\" minpoints=\"38000\" maxpoints=\"41999\" name=\"RANK_VETERAN_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"30\" minpoints=\"42000\" maxpoints=\"45999\" name=\"RANK_VETERAN_5\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"31\" minpoints=\"46000\" maxpoints=\"51999\" name=\"RANK_MASTER_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"32\" minpoints=\"52000\" maxpoints=\"57999\" name=\"RANK_MASTER_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"33\" minpoints=\"58000\" maxpoints=\"63999\" name=\"RANK_MASTER_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"34\" minpoints=\"64000\" maxpoints=\"69999\" name=\"RANK_MASTER_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"35\" minpoints=\"70000\" maxpoints=\"75999\" name=\"RANK_MASTER_5\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"36\" minpoints=\"76000\" maxpoints=\"83999\" name=\"RANK_ELITE_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"37\" minpoints=\"84000\" maxpoints=\"91999\" name=\"RANK_ELITE_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"38\" minpoints=\"92000\" maxpoints=\"99999\" name=\"RANK_ELITE_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"39\" minpoints=\"100000\" maxpoints=\"107999\" name=\"RANK_ELITE_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"40\" minpoints=\"108000\" maxpoints=\"115999\" name=\"RANK_ELITE_5\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"41\" minpoints=\"116000\" maxpoints=\"125999\" name=\"RANK_HERO_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"42\" minpoints=\"126000\" maxpoints=\"135999\" name=\"RANK_HERO_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"43\" minpoints=\"136000\" maxpoints=\"145999\" name=\"RANK_HERO_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"44\" minpoints=\"146000\" maxpoints=\"155999\" name=\"RANK_HERO_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"45\" minpoints=\"156000\" maxpoints=\"165999\" name=\"RANK_HERO_5\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"46\" minpoints=\"166000\" maxpoints=\"177999\" name=\"RANK_LEGEND_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"47\" minpoints=\"178000\" maxpoints=\"189999\" name=\"RANK_LEGEND_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"48\" minpoints=\"190000\" maxpoints=\"201999\" name=\"RANK_LEGEND_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"49\" minpoints=\"202000\" maxpoints=\"213999\" name=\"RANK_LEGEND_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"50\" minpoints=\"214000\" maxpoints=\"225999\" name=\"RANK_LEGEND_5\"/>\r\n\r\n\r\n\t\t\t" +
                                        "<rank level=\"51\" minpoints=\"226000\" maxpoints=\"237999\" name=\"RANK_CONQUEROR_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"52\" minpoints=\"238000\" maxpoints=\"249999\" name=\"RANK_CONQUEROR_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"53\" minpoints=\"250000\" maxpoints=\"261999\" name=\"RANK_CONQUEROR_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"54\" minpoints=\"262000\" maxpoints=\"273999\" name=\"RANK_CONQUEROR_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"55\" minpoints=\"274000\" maxpoints=\"285999\" name=\"RANK_CONQUEROR_5\"/>\r\n\t\t\t" +
                                        "<rank level=\"56\" minpoints=\"286000\" maxpoints=\"297999\" name=\"RANK_CONQUEROR_6\"/>\r\n\t\t\t" +
                                        "<rank level=\"57\" minpoints=\"298000\" maxpoints=\"309999\" name=\"RANK_CONQUEROR_7\"/>\r\n\t\t\t" +
                                        "<rank level=\"58\" minpoints=\"310000\" maxpoints=\"321999\" name=\"RANK_CONQUEROR_8\"/>\r\n\t\t\t" +
                                        "<rank level=\"59\" minpoints=\"322000\" maxpoints=\"333999\" name=\"RANK_CONQUEROR_9\"/>\r\n\t\t\t" +
                                        "<rank level=\"60\" minpoints=\"334000\" maxpoints=\"345999\" name=\"RANK_CONQUEROR_10\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"61\" minpoints=\"346000\" maxpoints=\"357999\" name=\"RANK_GURU_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"62\" minpoints=\"358000\" maxpoints=\"369999\" name=\"RANK_GURU_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"63\" minpoints=\"370000\" maxpoints=\"381999\" name=\"RANK_GURU_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"64\" minpoints=\"382000\" maxpoints=\"393999\" name=\"RANK_GURU_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"65\" minpoints=\"394000\" maxpoints=\"405999\" name=\"RANK_GURU_5\"/>\r\n\t\t\t" +
                                        "<rank level=\"66\" minpoints=\"406000\" maxpoints=\"417999\" name=\"RANK_GURU_6\"/>\r\n\t\t\t" +
                                        "<rank level=\"67\" minpoints=\"418000\" maxpoints=\"429999\" name=\"RANK_GURU_7\"/>\r\n\t\t\t" +
                                        "<rank level=\"68\" minpoints=\"430000\" maxpoints=\"441999\" name=\"RANK_GURU_8\"/>\r\n\t\t\t" +
                                        "<rank level=\"69\" minpoints=\"442000\" maxpoints=\"453999\" name=\"RANK_GURU_9\"/>\r\n\t\t\t" +
                                        "<rank level=\"70\" minpoints=\"454000\" maxpoints=\"465999\" name=\"RANK_GURU_10\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"71\" minpoints=\"466000\" maxpoints=\"477999\" name=\"RANK_SAGE_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"72\" minpoints=\"478000\" maxpoints=\"489999\" name=\"RANK_SAGE_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"73\" minpoints=\"490000\" maxpoints=\"501999\" name=\"RANK_SAGE_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"74\" minpoints=\"502000\" maxpoints=\"513999\" name=\"RANK_SAGE_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"75\" minpoints=\"514000\" maxpoints=\"525999\" name=\"RANK_SAGE_5\"/>\r\n\t\t\t" +
                                        "<rank level=\"76\" minpoints=\"526000\" maxpoints=\"537999\" name=\"RANK_SAGE_6\"/>\r\n\t\t\t" +
                                        "<rank level=\"77\" minpoints=\"538000\" maxpoints=\"549999\" name=\"RANK_SAGE_7\"/>\r\n\t\t\t" +
                                        "<rank level=\"78\" minpoints=\"550000\" maxpoints=\"561999\" name=\"RANK_SAGE_8\"/>\r\n\t\t\t" +
                                        "<rank level=\"79\" minpoints=\"562000\" maxpoints=\"573999\" name=\"RANK_SAGE_9\"/>\r\n\t\t\t" +
                                        "<rank level=\"80\" minpoints=\"574000\" maxpoints=\"585999\" name=\"RANK_SAGE_10\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"81\" minpoints=\"586000\" maxpoints=\"597999\" name=\"RANK_SAVANT_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"82\" minpoints=\"598000\" maxpoints=\"609999\" name=\"RANK_SAVANT_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"83\" minpoints=\"610000\" maxpoints=\"621999\" name=\"RANK_SAVANT_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"84\" minpoints=\"622000\" maxpoints=\"633999\" name=\"RANK_SAVANT_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"85\" minpoints=\"634000\" maxpoints=\"645999\" name=\"RANK_SAVANT_5\"/>\r\n\t\t\t" +
                                        "<rank level=\"86\" minpoints=\"646000\" maxpoints=\"657999\" name=\"RANK_SAVANT_6\"/>\r\n\t\t\t" +
                                        "<rank level=\"87\" minpoints=\"658000\" maxpoints=\"669999\" name=\"RANK_SAVANT_7\"/>\r\n\t\t\t" +
                                        "<rank level=\"88\" minpoints=\"670000\" maxpoints=\"681999\" name=\"RANK_SAVANT_8\"/>\r\n\t\t\t" +
                                        "<rank level=\"89\" minpoints=\"682000\" maxpoints=\"693999\" name=\"RANK_SAVANT_9\"/>\r\n\t\t\t" +
                                        "<rank level=\"90\" minpoints=\"694000\" maxpoints=\"705999\" name=\"RANK_SAVANT_10\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"91\" minpoints=\"706000\" maxpoints=\"717999\" name=\"RANK_IMMORTAL_1\"/>\r\n\t\t\t" +
                                        "<rank level=\"92\" minpoints=\"718000\" maxpoints=\"729999\" name=\"RANK_IMMORTAL_2\"/>\r\n\t\t\t" +
                                        "<rank level=\"93\" minpoints=\"730000\" maxpoints=\"741999\" name=\"RANK_IMMORTAL_3\"/>\r\n\t\t\t" +
                                        "<rank level=\"94\" minpoints=\"742000\" maxpoints=\"753999\" name=\"RANK_IMMORTAL_4\"/>\r\n\t\t\t" +
                                        "<rank level=\"95\" minpoints=\"754000\" maxpoints=\"765999\" name=\"RANK_IMMORTAL_5\"/>\r\n\t\t\t" +
                                        "<rank level=\"96\" minpoints=\"766000\" maxpoints=\"777999\" name=\"RANK_IMMORTAL_6\"/>\r\n\t\t\t" +
                                        "<rank level=\"97\" minpoints=\"778000\" maxpoints=\"789999\" name=\"RANK_IMMORTAL_7\"/>\r\n\t\t\t" +
                                        "<rank level=\"98\" minpoints=\"790000\" maxpoints=\"801999\" name=\"RANK_IMMORTAL_8\"/>\r\n\t\t\t" +
                                        "<rank level=\"99\" minpoints=\"802000\" maxpoints=\"813999\" name=\"RANK_IMMORTAL_9\"/>\r\n\t\t\t" +
                                        "<rank level=\"100\" minpoints=\"814000\" maxpoints=\"873999\" name=\"RANK_IMMORTAL_10\"/>\r\n\r\n\t\t\t" +
                                        "<rank level=\"101\" minpoints=\"874000\" maxpoints=\"899999\" name=\"RANK_OVERLORD_1\"/>\r\n\r\n\t\t" +
                                        "</table>\r\n\r\n\t" +
                                        "</stage5>\r\n" +
                                        "</GetRankedConfig>"));
                        }
                        break;

                    case "/wox_ws/rest/legal/Eula":

                        switch (method)
                        {
                            case "GET":
                                ctx.Response.ChunkedTransfer = true;

                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", string.Empty);

                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                    "<Eula>" +
                                    "Online Interactions Not Rated by the ESRB\r\n\r\n" +
                                    "ONLINE USER AGREEMENT\r\n\r\n" +
                                    "PLEASE READ THE ENTIRE AGREEMENT AND INDICATE WHETHER OR NOT YOU AGREE TO ITS TERMS BY CLICKING THE 'ACCEPT' OR 'DECLINE' BUTTON AT THE BOTTOM OF THIS SCREEN. " +
                                    "IF YOU CLICK THE 'DECLINE' BUTTON YOU WILL NOT BE ABLE TO PLAY THE ONLINE VERSION OF THIS GAME. THERE ARE NO REFUNDS OF THE ONLINE VERSION OF THIS GAME, SO PLEASE READ THE USER AGREEMENT PRIOR TO PURCHASE. FOR THE PACKAGED VERSION OF THE GAME, CHECK WITH SONY COMPUTER ENTERTAINMENT AMERICA INC. ('SCEA') CONSUMER SERVICES AT 1-800-345-7669 FOR REFUND OR RETURN INFORMATION. PLEASE HAVE YOUR PURCHASE RECEIPT AVAILABLE.\r\n\r\n" +
                                    "1. ACCEPTANCE OF AGREEMENT. This Agreement can be accepted only by an adult 18 years or older. By clicking the 'ACCEPT' button, you affirm that you are over 18 years old and you are accepting this Agreement on your own behalf or on behalf of your minor child (under 18).\r\n\r\n" +
                                    "2. GRANT OF LICENSE. SCEA grants you a non-exclusive right to use this software for personal, non-commercial play on a PlayStation.. computer entertainment system only. You may not (i) rent, lease or sublicense the software, (ii) modify, adapt, translate, reverse engineer, decompile or disassemble the software, (iii) attempt to create the source code from the object code for the software, or (iv) download game content for any purpose other than game play. You have no proprietary rights in any game content including game play statistics. SCEA may modify such content at any time for any reason. This software may include time and use restrictions. For time and use restrictions regarding this software, visit www.us.playstation.comsupportuseragreement.\r\n\r\n" +
                                    "3. AUTHENTICATION/SERVICE. SCEA may retrieve information about a user's hardware and software for authentication, copy protection, account blocking, system monitoring/diagnostics, rule enforcements, game management and other purposes. SCEA does not guarantee the continuous operation of the game servers and shall not be responsible for any delay or failure of the game servers to perform.\r\n\r\n" +
                                    "4. MAINTENANCE AND UPGRADES. Certain versions of hardware operating system software or other firmware ('Firmware') may be necessary in order for this game to play on your PlayStation.. computer entertainment system. The game software on this disc may check for the appropriate Firmware and if it does not find the correct Firmware versions, it may automatically update your Firmware. Without limitation, such automatic updates or upgrades may change your current operating system, cause a loss of data, content, functionalities or utilities. It is recommended that you regularly back up any data located on the hard disk that is of a type that can be backed up. Loss of data is the user's responsibility.\r\n\r\n" +
                                    "5. SEPARATE USER ACCOUNTS/COLLECTION OF INFORMATION. Some games/services may ask you to create an account with a user, player or other game name ('Game Name') and password. You may also be asked to select or provide additional information for a game profile. This information may be provided to any tournament website established by SCEA or its partners in connection with this game. When you choose a Game Name, choose an alias to protect your identity. When you choose a password, choose a unique combination of letters and numbers unrelated to your Game Name or to any information you may share with other players in the game. If your account is inactive for an extended period your account may be deactivated. To inquire about a deactivated account, please contact SCEA Consumer Services at 1-800-345-7669.\r\n\r\n" +
                                    "6. PROTECTION OF IDENTITY/NO EXPECTATION OF PRIVACY. You have no expectation of privacy or confidentiality in the personal information or User Material you may intentionally or unintentionally disclose. You should avoid saying anything personally identifying in chat.\r\n\r\n" +
                                    "7. ONLINE CONDUCT. When you play, you agree to be respectful of your fellow players and never to engage in any behavior that would be abusive or offensive to other players, disruptive of the game experience, fraudulent or otherwise illegal. This includes but is not limited to:\r\n\r\n" +
                                    "(a) Harassing or intimidating other players;\r\n\r\n" +
                                    "(b) Using language, selecting user, character, clan or team names or creating any other content that may be racially, ethnically or religiously offensive, sexually abusive, obscene or defamatory;\r\n\r\n" +
                                    "(c) Selecting as a user, character, clan or team name any word, symbol or combination of words and symbols which is identical to or substantially similar to any character, team, weapon, vehicle or other element which appears in this game;\r\n\r\n" +
                                    "(d) Using content that is commercial in nature such as advertisements, solicitations and promotions for goods or services;\r\n\r\n" +
                                    "(e) Falsely representing that you are an employee of Sony Corporation, SCEA, or any other affiliated or related company;\r\n\r\n" +
                                    "(f) Disrupting the normal flow of chat;\r\n\r\n" +
                                    "(g) Making a false report of user abuse to SCEA Consumer Services;\r\n\r\n" +
                                    "(h) Violating any local, state or national law;\r\n\r\n" +
                                    "(i) Using a cheat code, cheat device or any device that modifies the executable game code or data. For a detailed explanation of the SCEA policy on cheating, visit www.us.playstation.com/onlinecheating.\r\n\r\n" +
                                    "(j) Any attempt to deliberately alter, damage or undermine the legitimate operation of this game, including but not limited to exploiting the ranking system by creating 'dummy accounts'.\r\n\r\n" +
                                    "8. REPORTING ABUSE. To report violations of this Agreement or to inquire about a blocked account, call SCEA Consumer Services at 1-800-345-7669.\r\n\r\n" +
                                    "9. AGREEMENT VIOLATIONS. If you violate this Agreement in any manner, SCEA may, at its discretion and without notice, temporarily or permanently block your account and/or reset your stats and/or rankings in this game and any related games.\r\n\r\n" +
                                    "10. HOTSPOT AND INTERNET SERVICE PROVIDERS. SCEA and its affiliated companies are not associated with any of the Internet service providers (ISPs) including hotspot operators (collectively 'ISPs'). SCEA is not responsible for any damages or injury arising from or related to your use of these ISP services. When you access the Internet, you are providing information directly to the ISP and not to SCEA. This information is collected by the ISP and is not shared with SCEA. Before accessing the Internet, you should carefully read the Service Provider's user agreement/terms and conditions and privacy policy carefully.\r\n\r\n" +
                                    "11. WARRANTY/DISCLAIMER/LIABILITY LIMITATIONS. EXCEPT AS PROVIDED HEREIN, THE SOFTWARE AND ALL RELATED SERVICES ARE PROVIDED 'AS IS' AND, TO THE MAXIMUM EXTENT PROVIDED UNDER LAW, SCEA DISCLAIMS ALL WARRANTIES OF ANY KIND, WHETHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OF MERCHANTABILITY OR FITNESS FOR A PARTICULAR PURPOSE. Without limiting the foregoing, SCEA does not promise that this software will work properly with all memory card storage or other peripheral devices. From time to time, there may be problems related to availability, access, delay and failure to perform that are beyond the immediate and reasonable control of SCEA. In the event of a dispute regarding the online functionality of this software, you agree that the sole liability of SCEA and its affiliated companies will be limited to repair or replacement of the game software at SCEA's option. SCEA may, at its sole discretion, discontinue hosting the game server at any time. SCEA has no liability for such discontinuance. SCEA has no liability for any violation of this Agreement by you or by any other player.\r\n\r\n" +
                                    "12. MODIFICATION. SCEA at its sole discretion may modify the terms of this Agreement at any time. You are responsible for reviewing the terms of this Agreement each time you log in to play. By accepting this Agreement and by playing the game online, you agree to be bound by all current terms of the Agreement. To print out a current copy of this Agreement using your computer, go to www.us.playstation.com/support/useragreement.\r\n\r\n" +
                                    "13. USER GENERATED CONTENT.\r\n\r\n" +
                                    "This game may include tools that give you the ability to communicate with other players and to post pictures, photographs, videos, game-related materials and other information, either on particular versions of the game you play or in community areas (individually and collectively, 'User Generated Content'). When using these tools, you are solely responsible for your own User Generated Content. By posting or submitting User Generated Content in any manner, you grant SCEA, our affiliates, licensors, and distributors a worldwide, perpetual, non-exclusive, sublicenseable and transferable license to use, copy, display, perform, distribute, adapt, modify and promote the User Generated Content in any medium. You represent and warrant that you own or have all necessary rights and licenses to use and authorize SCEA to use all patent, trademark, trade secret, copyright or other proprietary rights in any and all User Generated Content to enable inclusion and use of the User Generated Content in the manner contemplated by this Agreement.\r\n\r\n" +
                                    "To comply with the terms of this Agreement, you will not submit any User Generated Content that (a) is protected by copyright, patent, trademark, trade secret or otherwise subject to third party proprietary rights, including rights of privacy and publicity (unless you are or have permission from the rightful owner); (b) is fraudulent or a misrepresentation that could damage SCEA or any third party; (c) is unlawful, obscene, defamatory, threatening, harassing, predatory, pornographic, hateful, racially or ethnically offensive, or encourages conduct that would violate any law, or is otherwise inappropriate; (d) is an advertisement or solicitation of business, (e) is an impersonation of another person; or (e) violates any of the rules of Online Conduct, other terms of this Agreement or any other terms related to this game.\r\n\r\n" +
                                    "You agree that SCEA is not responsible or liable for User Generated Content submitted or posted by you or by others. SCEA does not claim ownership of any User Generated Content that you submit or make available as part of the game, and SCEA expressly disclaims any and all liability in connection with any User Generated Content. User Generated Content will belong to you or your licensors. SCEA has no duty to pre-screen User Generated Content. SCEA has the right to edit, remove, or refuse to post any submitted User Generated Content for any reason without prior notice, but is not responsible for any failure or delay in doing so.\r\n\r\n" +
                                    "14. MISCELLANEOUS. This Agreement shall be construed and interpreted in accordance with the laws of the State of California applying to contracts fully executed and performed within the State of California. Both parties submit to personal jurisdiction in California and further agree that any dispute arising from or relating to this Agreement shall be brought in a court within San Mateo County, California. If any provision of this Agreement shall be held invalid or unenforceable, in whole or in part, such provision shall be modified to the minimum extent necessary to make it valid and enforceable, and the validity and enforceability of all other provisions of this Agreement shall not be affected thereby. This Agreement constitutes the entire agreement between the parties related to the subject matter hereof and supersedes all prior oral and written and all contemporaneous oral negotiations, commitments, and understandings of the parties, all of which are merged herein.\r\n\r\n" +
                                    "</Eula>"));
                        }
                        break;

                    case "/wox_ws/rest/legal/Announcements":

                        switch (method)
                        {
                            case "GET":
                                ctx.Response.ChunkedTransfer = true;

                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", string.Empty);

                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<Announcements><Announcement>WipEout.. HD Patch 2.51 information\r\n\r\n" +
                                        "- Spectate - Zone Battle - Fix crash when cycling through the ships.\r\n" +
                                        "- Restores Online Rank Progress\r\n\r\n" +
                                        "</Announcement></Announcements>"));
                        }
                        break;

                    case "/wox_ws/rest/lb/GetList":

                        switch (method)
                        {
                            case "GET":
                                ctx.Response.ChunkedTransfer = true;

                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", string.Empty);

                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<Leaderboard>" +
                                        "<GetList>" +
                                        "<lb id=\"1000004\" tr=\"0\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000004\"/><lb id=\"1010004\" tr=\"0\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010004\"/><lb id=\"1020004\" tr=\"0\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020004\"/><lb id=\"1030004\" tr=\"0\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030004\"/><lb id=\"1000104\" tr=\"1\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000104\"/><lb id=\"1010104\" tr=\"1\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010104\"/><lb id=\"1020104\" tr=\"1\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020104\"/><lb id=\"1030104\" tr=\"1\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030104\"/><lb id=\"1000204\" tr=\"2\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000204\"/><lb id=\"1010204\" tr=\"2\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010204\"/><lb id=\"1020204\" tr=\"2\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020204\"/><lb id=\"1030204\" tr=\"2\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030204\"/><lb id=\"1000304\" tr=\"3\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000304\"/><lb id=\"1010304\" tr=\"3\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010304\"/><lb id=\"1020304\" tr=\"3\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020304\"/><lb id=\"1030304\" tr=\"3\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030304\"/><lb id=\"1000404\" tr=\"4\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000404\"/><lb id=\"1010404\" tr=\"4\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010404\"/><lb id=\"1020404\" tr=\"4\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020404\"/><lb id=\"1030404\" tr=\"4\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030404\"/><lb id=\"1000504\" tr=\"5\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000504\"/><lb id=\"1010504\" tr=\"5\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010504\"/><lb id=\"1020504\" tr=\"5\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020504\"/><lb id=\"1030504\" tr=\"5\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030504\"/><lb id=\"1000604\" tr=\"6\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000604\"/><lb id=\"1010604\" tr=\"6\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010604\"/><lb id=\"1020604\" tr=\"6\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020604\"/><lb id=\"1030604\" tr=\"6\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030604\"/><lb id=\"1000704\" tr=\"7\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000704\"/><lb id=\"1010704\" tr=\"7\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010704\"/><lb id=\"1020704\" tr=\"7\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020704\"/><lb id=\"1030704\" tr=\"7\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030704\"/><lb id=\"1000804\" tr=\"8\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000804\"/><lb id=\"1010804\" tr=\"8\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010804\"/><lb id=\"1020804\" tr=\"8\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020804\"/><lb id=\"1030804\" tr=\"8\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030804\"/><lb id=\"1000904\" tr=\"9\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1000904\"/><lb id=\"1010904\" tr=\"9\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1010904\"/><lb id=\"1020904\" tr=\"9\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1020904\"/><lb id=\"1030904\" tr=\"9\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1030904\"/><lb id=\"1001004\" tr=\"10\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001004\"/><lb id=\"1011004\" tr=\"10\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011004\"/><lb id=\"1021004\" tr=\"10\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021004\"/><lb id=\"1031004\" tr=\"10\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031004\"/><lb id=\"1001104\" tr=\"11\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001104\"/><lb id=\"1011104\" tr=\"11\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011104\"/><lb id=\"1021104\" tr=\"11\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021104\"/><lb id=\"1031104\" tr=\"11\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031104\"/><lb id=\"1001204\" tr=\"12\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001204\"/><lb id=\"1011204\" tr=\"12\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011204\"/><lb id=\"1021204\" tr=\"12\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021204\"/><lb id=\"1031204\" tr=\"12\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031204\"/><lb id=\"1001304\" tr=\"13\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001304\"/><lb id=\"1011304\" tr=\"13\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011304\"/><lb id=\"1021304\" tr=\"13\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021304\"/><lb id=\"1031304\" tr=\"13\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031304\"/><lb id=\"1001404\" tr=\"14\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001404\"/><lb id=\"1011404\" tr=\"14\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011404\"/><lb id=\"1021404\" tr=\"14\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021404\"/><lb id=\"1031404\" tr=\"14\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031404\"/><lb id=\"1001504\" tr=\"15\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001504\"/><lb id=\"1011504\" tr=\"15\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011504\"/><lb id=\"1021504\" tr=\"15\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021504\"/><lb id=\"1031504\" tr=\"15\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031504\"/><lb id=\"1001604\" tr=\"16\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001604\"/><lb id=\"1011604\" tr=\"16\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011604\"/><lb id=\"1021604\" tr=\"16\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021604\"/><lb id=\"1031604\" tr=\"16\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031604\"/><lb id=\"1001704\" tr=\"17\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001704\"/><lb id=\"1011704\" tr=\"17\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011704\"/><lb id=\"1021704\" tr=\"17\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021704\"/><lb id=\"1031704\" tr=\"17\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031704\"/><lb id=\"1001804\" tr=\"18\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001804\"/><lb id=\"1011804\" tr=\"18\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011804\"/><lb id=\"1021804\" tr=\"18\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021804\"/><lb id=\"1031804\" tr=\"18\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031804\"/><lb id=\"1001904\" tr=\"19\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1001904\"/><lb id=\"1011904\" tr=\"19\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1011904\"/><lb id=\"1021904\" tr=\"19\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1021904\"/><lb id=\"1031904\" tr=\"19\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1031904\"/><lb id=\"1002004\" tr=\"20\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1002004\"/><lb id=\"1012004\" tr=\"20\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1012004\"/><lb id=\"1022004\" tr=\"20\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1022004\"/><lb id=\"1032004\" tr=\"20\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1032004\"/><lb id=\"1002104\" tr=\"21\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1002104\"/><lb id=\"1012104\" tr=\"21\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1012104\"/><lb id=\"1022104\" tr=\"21\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1022104\"/><lb id=\"1032104\" tr=\"21\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1032104\"/><lb id=\"1002204\" tr=\"22\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1002204\"/><lb id=\"1012204\" tr=\"22\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1012204\"/><lb id=\"1022204\" tr=\"22\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1022204\"/><lb id=\"1032204\" tr=\"22\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1032204\"/><lb id=\"1002304\" tr=\"23\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1002304\"/><lb id=\"1012304\" tr=\"23\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1012304\"/><lb id=\"1022304\" tr=\"23\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1022304\"/><lb id=\"1032304\" tr=\"23\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1032304\"/><lb id=\"1002404\" tr=\"24\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1002404\"/><lb id=\"1012404\" tr=\"24\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1012404\"/><lb id=\"1022404\" tr=\"24\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1022404\"/><lb id=\"1032404\" tr=\"24\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1032404\"/><lb id=\"1002504\" tr=\"25\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1002504\"/><lb id=\"1012504\" tr=\"25\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1012504\"/><lb id=\"1022504\" tr=\"25\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1022504\"/><lb id=\"1032504\" tr=\"25\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1032504\"/><lb id=\"1002604\" tr=\"26\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1002604\"/><lb id=\"1012604\" tr=\"26\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1012604\"/><lb id=\"1022604\" tr=\"26\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1022604\"/><lb id=\"1032604\" tr=\"26\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1032604\"/><lb id=\"1002704\" tr=\"27\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1002704\"/><lb id=\"1012704\" tr=\"27\" gm=\"0\" rt=\"4\" sc=\"1\" sId=\"SP_TIME_TRIAL.Flash._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1012704\"/><lb id=\"1022704\" tr=\"27\" gm=\"0\" rt=\"4\" sc=\"2\" sId=\"SP_TIME_TRIAL.Rapier._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1022704\"/><lb id=\"1032704\" tr=\"27\" gm=\"0\" rt=\"4\" sc=\"3\" sId=\"SP_TIME_TRIAL.Phantom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1032704\"/><lb id=\"1100004\" tr=\"0\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100004\"/><lb id=\"1110004\" tr=\"0\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110004\"/><lb id=\"1120004\" tr=\"0\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120004\"/><lb id=\"1130004\" tr=\"0\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130004\"/><lb id=\"1100104\" tr=\"1\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100104\"/><lb id=\"1110104\" tr=\"1\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110104\"/><lb id=\"1120104\" tr=\"1\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120104\"/><lb id=\"1130104\" tr=\"1\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130104\"/><lb id=\"1100204\" tr=\"2\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100204\"/><lb id=\"1110204\" tr=\"2\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110204\"/><lb id=\"1120204\" tr=\"2\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120204\"/><lb id=\"1130204\" tr=\"2\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130204\"/><lb id=\"1100304\" tr=\"3\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100304\"/><lb id=\"1110304\" tr=\"3\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110304\"/><lb id=\"1120304\" tr=\"3\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120304\"/><lb id=\"1130304\" tr=\"3\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130304\"/><lb id=\"1100404\" tr=\"4\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100404\"/><lb id=\"1110404\" tr=\"4\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110404\"/><lb id=\"1120404\" tr=\"4\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120404\"/><lb id=\"1130404\" tr=\"4\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130404\"/><lb id=\"1100504\" tr=\"5\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100504\"/><lb id=\"1110504\" tr=\"5\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110504\"/><lb id=\"1120504\" tr=\"5\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120504\"/><lb id=\"1130504\" tr=\"5\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130504\"/><lb id=\"1100604\" tr=\"6\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100604\"/><lb id=\"1110604\" tr=\"6\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110604\"/><lb id=\"1120604\" tr=\"6\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120604\"/><lb id=\"1130604\" tr=\"6\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130604\"/><lb id=\"1100704\" tr=\"7\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100704\"/><lb id=\"1110704\" tr=\"7\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110704\"/><lb id=\"1120704\" tr=\"7\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120704\"/><lb id=\"1130704\" tr=\"7\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130704\"/><lb id=\"1100804\" tr=\"8\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100804\"/><lb id=\"1110804\" tr=\"8\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110804\"/><lb id=\"1120804\" tr=\"8\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120804\"/><lb id=\"1130804\" tr=\"8\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130804\"/><lb id=\"1100904\" tr=\"9\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1100904\"/><lb id=\"1110904\" tr=\"9\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1110904\"/><lb id=\"1120904\" tr=\"9\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1120904\"/><lb id=\"1130904\" tr=\"9\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1130904\"/><lb id=\"1101004\" tr=\"10\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101004\"/><lb id=\"1111004\" tr=\"10\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111004\"/><lb id=\"1121004\" tr=\"10\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121004\"/><lb id=\"1131004\" tr=\"10\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131004\"/><lb id=\"1101104\" tr=\"11\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101104\"/><lb id=\"1111104\" tr=\"11\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111104\"/><lb id=\"1121104\" tr=\"11\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121104\"/><lb id=\"1131104\" tr=\"11\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131104\"/><lb id=\"1101204\" tr=\"12\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101204\"/><lb id=\"1111204\" tr=\"12\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111204\"/><lb id=\"1121204\" tr=\"12\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121204\"/><lb id=\"1131204\" tr=\"12\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131204\"/><lb id=\"1101304\" tr=\"13\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101304\"/><lb id=\"1111304\" tr=\"13\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111304\"/><lb id=\"1121304\" tr=\"13\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121304\"/><lb id=\"1131304\" tr=\"13\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131304\"/><lb id=\"1101404\" tr=\"14\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101404\"/><lb id=\"1111404\" tr=\"14\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111404\"/><lb id=\"1121404\" tr=\"14\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121404\"/><lb id=\"1131404\" tr=\"14\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131404\"/><lb id=\"1101504\" tr=\"15\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101504\"/><lb id=\"1111504\" tr=\"15\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111504\"/><lb id=\"1121504\" tr=\"15\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121504\"/><lb id=\"1131504\" tr=\"15\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131504\"/><lb id=\"1101604\" tr=\"16\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101604\"/><lb id=\"1111604\" tr=\"16\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111604\"/><lb id=\"1121604\" tr=\"16\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121604\"/><lb id=\"1131604\" tr=\"16\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131604\"/><lb id=\"1101704\" tr=\"17\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101704\"/><lb id=\"1111704\" tr=\"17\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111704\"/><lb id=\"1121704\" tr=\"17\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121704\"/><lb id=\"1131704\" tr=\"17\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131704\"/><lb id=\"1101804\" tr=\"18\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101804\"/><lb id=\"1111804\" tr=\"18\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111804\"/><lb id=\"1121804\" tr=\"18\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121804\"/><lb id=\"1131804\" tr=\"18\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131804\"/><lb id=\"1101904\" tr=\"19\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1101904\"/><lb id=\"1111904\" tr=\"19\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1111904\"/><lb id=\"1121904\" tr=\"19\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1121904\"/><lb id=\"1131904\" tr=\"19\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1131904\"/><lb id=\"1102004\" tr=\"20\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1102004\"/><lb id=\"1112004\" tr=\"20\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1112004\"/><lb id=\"1122004\" tr=\"20\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1122004\"/><lb id=\"1132004\" tr=\"20\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1132004\"/><lb id=\"1102104\" tr=\"21\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1102104\"/><lb id=\"1112104\" tr=\"21\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1112104\"/><lb id=\"1122104\" tr=\"21\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1122104\"/><lb id=\"1132104\" tr=\"21\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1132104\"/><lb id=\"1102204\" tr=\"22\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1102204\"/><lb id=\"1112204\" tr=\"22\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1112204\"/><lb id=\"1122204\" tr=\"22\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1122204\"/><lb id=\"1132204\" tr=\"22\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1132204\"/><lb id=\"1102304\" tr=\"23\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1102304\"/><lb id=\"1112304\" tr=\"23\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1112304\"/><lb id=\"1122304\" tr=\"23\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1122304\"/><lb id=\"1132304\" tr=\"23\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1132304\"/><lb id=\"1102404\" tr=\"24\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1102404\"/><lb id=\"1112404\" tr=\"24\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1112404\"/><lb id=\"1122404\" tr=\"24\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1122404\"/><lb id=\"1132404\" tr=\"24\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1132404\"/><lb id=\"1102504\" tr=\"25\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1102504\"/><lb id=\"1112504\" tr=\"25\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1112504\"/><lb id=\"1122504\" tr=\"25\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1122504\"/><lb id=\"1132504\" tr=\"25\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1132504\"/><lb id=\"1102604\" tr=\"26\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1102604\"/><lb id=\"1112604\" tr=\"26\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1112604\"/><lb id=\"1122604\" tr=\"26\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1122604\"/><lb id=\"1132604\" tr=\"26\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1132604\"/><lb id=\"1102704\" tr=\"27\" gm=\"1\" rt=\"4\" sc=\"0\" sId=\"SP_SINGLE_RACE.Venom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1102704\"/><lb id=\"1112704\" tr=\"27\" gm=\"1\" rt=\"4\" sc=\"1\" sId=\"SP_SINGLE_RACE.Flash._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1112704\"/><lb id=\"1122704\" tr=\"27\" gm=\"1\" rt=\"4\" sc=\"2\" sId=\"SP_SINGLE_RACE.Rapier._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1122704\"/><lb id=\"1132704\" tr=\"27\" gm=\"1\" rt=\"4\" sc=\"3\" sId=\"SP_SINGLE_RACE.Phantom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1132704\"/><lb id=\"1200003\" tr=\"0\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200003\"/><lb id=\"1210003\" tr=\"0\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210003\"/><lb id=\"1220003\" tr=\"0\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220003\"/><lb id=\"1230003\" tr=\"0\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230003\"/><lb id=\"1200103\" tr=\"1\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200103\"/><lb id=\"1210103\" tr=\"1\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210103\"/><lb id=\"1220103\" tr=\"1\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220103\"/><lb id=\"1230103\" tr=\"1\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230103\"/><lb id=\"1200203\" tr=\"2\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200203\"/><lb id=\"1210203\" tr=\"2\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210203\"/><lb id=\"1220203\" tr=\"2\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220203\"/><lb id=\"1230203\" tr=\"2\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230203\"/><lb id=\"1200303\" tr=\"3\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200303\"/><lb id=\"1210303\" tr=\"3\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210303\"/><lb id=\"1220303\" tr=\"3\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220303\"/><lb id=\"1230303\" tr=\"3\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230303\"/><lb id=\"1200403\" tr=\"4\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200403\"/><lb id=\"1210403\" tr=\"4\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210403\"/><lb id=\"1220403\" tr=\"4\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220403\"/><lb id=\"1230403\" tr=\"4\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230403\"/><lb id=\"1200503\" tr=\"5\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200503\"/><lb id=\"1210503\" tr=\"5\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210503\"/><lb id=\"1220503\" tr=\"5\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220503\"/><lb id=\"1230503\" tr=\"5\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230503\"/><lb id=\"1200603\" tr=\"6\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200603\"/><lb id=\"1210603\" tr=\"6\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210603\"/><lb id=\"1220603\" tr=\"6\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220603\"/><lb id=\"1230603\" tr=\"6\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230603\"/><lb id=\"1200703\" tr=\"7\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200703\"/><lb id=\"1210703\" tr=\"7\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210703\"/><lb id=\"1220703\" tr=\"7\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220703\"/><lb id=\"1230703\" tr=\"7\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230703\"/><lb id=\"1200803\" tr=\"8\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200803\"/><lb id=\"1210803\" tr=\"8\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210803\"/><lb id=\"1220803\" tr=\"8\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220803\"/><lb id=\"1230803\" tr=\"8\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230803\"/><lb id=\"1200903\" tr=\"9\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1200903\"/><lb id=\"1210903\" tr=\"9\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1210903\"/><lb id=\"1220903\" tr=\"9\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1220903\"/><lb id=\"1230903\" tr=\"9\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1230903\"/><lb id=\"1201003\" tr=\"10\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201003\"/><lb id=\"1211003\" tr=\"10\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211003\"/><lb id=\"1221003\" tr=\"10\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221003\"/><lb id=\"1231003\" tr=\"10\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231003\"/><lb id=\"1201103\" tr=\"11\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201103\"/><lb id=\"1211103\" tr=\"11\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211103\"/><lb id=\"1221103\" tr=\"11\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221103\"/><lb id=\"1231103\" tr=\"11\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231103\"/><lb id=\"1201203\" tr=\"12\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201203\"/><lb id=\"1211203\" tr=\"12\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211203\"/><lb id=\"1221203\" tr=\"12\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221203\"/><lb id=\"1231203\" tr=\"12\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231203\"/><lb id=\"1201303\" tr=\"13\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201303\"/><lb id=\"1211303\" tr=\"13\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211303\"/><lb id=\"1221303\" tr=\"13\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221303\"/><lb id=\"1231303\" tr=\"13\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231303\"/><lb id=\"1201403\" tr=\"14\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201403\"/><lb id=\"1211403\" tr=\"14\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211403\"/><lb id=\"1221403\" tr=\"14\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221403\"/><lb id=\"1231403\" tr=\"14\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231403\"/><lb id=\"1201503\" tr=\"15\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201503\"/><lb id=\"1211503\" tr=\"15\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211503\"/><lb id=\"1221503\" tr=\"15\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221503\"/><lb id=\"1231503\" tr=\"15\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231503\"/><lb id=\"1201603\" tr=\"16\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201603\"/><lb id=\"1211603\" tr=\"16\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211603\"/><lb id=\"1221603\" tr=\"16\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221603\"/><lb id=\"1231603\" tr=\"16\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231603\"/><lb id=\"1201703\" tr=\"17\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201703\"/><lb id=\"1211703\" tr=\"17\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211703\"/><lb id=\"1221703\" tr=\"17\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221703\"/><lb id=\"1231703\" tr=\"17\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231703\"/><lb id=\"1201803\" tr=\"18\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201803\"/><lb id=\"1211803\" tr=\"18\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211803\"/><lb id=\"1221803\" tr=\"18\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221803\"/><lb id=\"1231803\" tr=\"18\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231803\"/><lb id=\"1201903\" tr=\"19\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1201903\"/><lb id=\"1211903\" tr=\"19\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1211903\"/><lb id=\"1221903\" tr=\"19\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1221903\"/><lb id=\"1231903\" tr=\"19\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1231903\"/><lb id=\"1202003\" tr=\"20\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1202003\"/><lb id=\"1212003\" tr=\"20\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1212003\"/><lb id=\"1222003\" tr=\"20\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1222003\"/><lb id=\"1232003\" tr=\"20\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1232003\"/><lb id=\"1202103\" tr=\"21\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1202103\"/><lb id=\"1212103\" tr=\"21\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1212103\"/><lb id=\"1222103\" tr=\"21\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1222103\"/><lb id=\"1232103\" tr=\"21\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1232103\"/><lb id=\"1202203\" tr=\"22\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1202203\"/><lb id=\"1212203\" tr=\"22\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1212203\"/><lb id=\"1222203\" tr=\"22\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1222203\"/><lb id=\"1232203\" tr=\"22\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1232203\"/><lb id=\"1202303\" tr=\"23\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1202303\"/><lb id=\"1212303\" tr=\"23\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1212303\"/><lb id=\"1222303\" tr=\"23\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1222303\"/><lb id=\"1232303\" tr=\"23\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1232303\"/><lb id=\"1202403\" tr=\"24\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1202403\"/><lb id=\"1212403\" tr=\"24\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1212403\"/><lb id=\"1222403\" tr=\"24\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1222403\"/><lb id=\"1232403\" tr=\"24\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1232403\"/><lb id=\"1202503\" tr=\"25\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1202503\"/><lb id=\"1212503\" tr=\"25\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1212503\"/><lb id=\"1222503\" tr=\"25\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1222503\"/><lb id=\"1232503\" tr=\"25\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1232503\"/><lb id=\"1202603\" tr=\"26\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1202603\"/><lb id=\"1212603\" tr=\"26\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1212603\"/><lb id=\"1222603\" tr=\"26\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1222603\"/><lb id=\"1232603\" tr=\"26\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1232603\"/><lb id=\"1202703\" tr=\"27\" gm=\"2\" rt=\"3\" sc=\"0\" sId=\"SP_SPEED_LAP.Venom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1202703\"/><lb id=\"1212703\" tr=\"27\" gm=\"2\" rt=\"3\" sc=\"1\" sId=\"SP_SPEED_LAP.Flash._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1212703\"/><lb id=\"1222703\" tr=\"27\" gm=\"2\" rt=\"3\" sc=\"2\" sId=\"SP_SPEED_LAP.Rapier._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1222703\"/><lb id=\"1232703\" tr=\"27\" gm=\"2\" rt=\"3\" sc=\"3\" sId=\"SP_SPEED_LAP.Phantom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1232703\"/><lb id=\"1300009\" tr=\"0\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300009\"/><lb id=\"1300109\" tr=\"1\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300109\"/><lb id=\"1300209\" tr=\"2\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300209\"/><lb id=\"1300309\" tr=\"3\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300309\"/><lb id=\"1300409\" tr=\"4\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300409\"/><lb id=\"1300509\" tr=\"5\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300509\"/><lb id=\"1300609\" tr=\"6\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300609\"/><lb id=\"1300709\" tr=\"7\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300709\"/><lb id=\"1300809\" tr=\"8\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300809\"/><lb id=\"1300909\" tr=\"9\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1300909\"/><lb id=\"1301009\" tr=\"10\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301009\"/><lb id=\"1301109\" tr=\"11\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301109\"/><lb id=\"1301209\" tr=\"12\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301209\"/><lb id=\"1301309\" tr=\"13\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301309\"/><lb id=\"1301409\" tr=\"14\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301409\"/><lb id=\"1301509\" tr=\"15\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301509\"/><lb id=\"1301609\" tr=\"16\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301609\"/><lb id=\"1301709\" tr=\"17\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301709\"/><lb id=\"1301809\" tr=\"18\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301809\"/><lb id=\"1301909\" tr=\"19\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1301909\"/><lb id=\"1302009\" tr=\"20\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1302009\"/><lb id=\"1302109\" tr=\"21\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1302109\"/><lb id=\"1302209\" tr=\"22\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1302209\"/><lb id=\"1302309\" tr=\"23\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1302309\"/><lb id=\"1302409\" tr=\"24\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1302409\"/><lb id=\"1302509\" tr=\"25\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1302509\"/><lb id=\"1302609\" tr=\"26\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1302609\"/><lb id=\"1302709\" tr=\"27\" gm=\"3\" rt=\"9\" sId=\"SP_ZONE._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1302709\"/><lb id=\"1400004\" tr=\"0\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400004\"/><lb id=\"1410004\" tr=\"0\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410004\"/><lb id=\"1420004\" tr=\"0\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420004\"/><lb id=\"1430004\" tr=\"0\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430004\"/><lb id=\"1400104\" tr=\"1\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400104\"/><lb id=\"1410104\" tr=\"1\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410104\"/><lb id=\"1420104\" tr=\"1\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420104\"/><lb id=\"1430104\" tr=\"1\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430104\"/><lb id=\"1400204\" tr=\"2\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400204\"/><lb id=\"1410204\" tr=\"2\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410204\"/><lb id=\"1420204\" tr=\"2\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420204\"/><lb id=\"1430204\" tr=\"2\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430204\"/><lb id=\"1400304\" tr=\"3\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400304\"/><lb id=\"1410304\" tr=\"3\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410304\"/><lb id=\"1420304\" tr=\"3\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420304\"/><lb id=\"1430304\" tr=\"3\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430304\"/><lb id=\"1400404\" tr=\"4\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400404\"/><lb id=\"1410404\" tr=\"4\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410404\"/><lb id=\"1420404\" tr=\"4\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420404\"/><lb id=\"1430404\" tr=\"4\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430404\"/><lb id=\"1400504\" tr=\"5\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400504\"/><lb id=\"1410504\" tr=\"5\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410504\"/><lb id=\"1420504\" tr=\"5\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420504\"/><lb id=\"1430504\" tr=\"5\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430504\"/><lb id=\"1400604\" tr=\"6\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400604\"/><lb id=\"1410604\" tr=\"6\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410604\"/><lb id=\"1420604\" tr=\"6\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420604\"/><lb id=\"1430604\" tr=\"6\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430604\"/><lb id=\"1400704\" tr=\"7\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400704\"/><lb id=\"1410704\" tr=\"7\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410704\"/><lb id=\"1420704\" tr=\"7\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420704\"/><lb id=\"1430704\" tr=\"7\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430704\"/><lb id=\"1400804\" tr=\"8\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400804\"/><lb id=\"1410804\" tr=\"8\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410804\"/><lb id=\"1420804\" tr=\"8\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420804\"/><lb id=\"1430804\" tr=\"8\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430804\"/><lb id=\"1400904\" tr=\"9\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1400904\"/><lb id=\"1410904\" tr=\"9\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1410904\"/><lb id=\"1420904\" tr=\"9\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1420904\"/><lb id=\"1430904\" tr=\"9\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1430904\"/><lb id=\"1401004\" tr=\"10\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401004\"/><lb id=\"1411004\" tr=\"10\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411004\"/><lb id=\"1421004\" tr=\"10\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421004\"/><lb id=\"1431004\" tr=\"10\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431004\"/><lb id=\"1401104\" tr=\"11\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401104\"/><lb id=\"1411104\" tr=\"11\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411104\"/><lb id=\"1421104\" tr=\"11\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421104\"/><lb id=\"1431104\" tr=\"11\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431104\"/><lb id=\"1401204\" tr=\"12\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401204\"/><lb id=\"1411204\" tr=\"12\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411204\"/><lb id=\"1421204\" tr=\"12\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421204\"/><lb id=\"1431204\" tr=\"12\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431204\"/><lb id=\"1401304\" tr=\"13\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401304\"/><lb id=\"1411304\" tr=\"13\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411304\"/><lb id=\"1421304\" tr=\"13\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421304\"/><lb id=\"1431304\" tr=\"13\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431304\"/><lb id=\"1401404\" tr=\"14\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401404\"/><lb id=\"1411404\" tr=\"14\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411404\"/><lb id=\"1421404\" tr=\"14\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421404\"/><lb id=\"1431404\" tr=\"14\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431404\"/><lb id=\"1401504\" tr=\"15\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401504\"/><lb id=\"1411504\" tr=\"15\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411504\"/><lb id=\"1421504\" tr=\"15\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421504\"/><lb id=\"1431504\" tr=\"15\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431504\"/><lb id=\"1401604\" tr=\"16\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401604\"/><lb id=\"1411604\" tr=\"16\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411604\"/><lb id=\"1421604\" tr=\"16\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421604\"/><lb id=\"1431604\" tr=\"16\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431604\"/><lb id=\"1401704\" tr=\"17\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401704\"/><lb id=\"1411704\" tr=\"17\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411704\"/><lb id=\"1421704\" tr=\"17\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421704\"/><lb id=\"1431704\" tr=\"17\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431704\"/><lb id=\"1401804\" tr=\"18\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401804\"/><lb id=\"1411804\" tr=\"18\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411804\"/><lb id=\"1421804\" tr=\"18\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421804\"/><lb id=\"1431804\" tr=\"18\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431804\"/><lb id=\"1401904\" tr=\"19\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1401904\"/><lb id=\"1411904\" tr=\"19\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1411904\"/><lb id=\"1421904\" tr=\"19\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1421904\"/><lb id=\"1431904\" tr=\"19\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1431904\"/><lb id=\"1402004\" tr=\"20\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1402004\"/><lb id=\"1412004\" tr=\"20\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1412004\"/><lb id=\"1422004\" tr=\"20\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1422004\"/><lb id=\"1432004\" tr=\"20\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1432004\"/><lb id=\"1402104\" tr=\"21\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1402104\"/><lb id=\"1412104\" tr=\"21\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1412104\"/><lb id=\"1422104\" tr=\"21\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1422104\"/><lb id=\"1432104\" tr=\"21\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1432104\"/><lb id=\"1402204\" tr=\"22\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1402204\"/><lb id=\"1412204\" tr=\"22\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1412204\"/><lb id=\"1422204\" tr=\"22\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1422204\"/><lb id=\"1432204\" tr=\"22\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1432204\"/><lb id=\"1402304\" tr=\"23\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1402304\"/><lb id=\"1412304\" tr=\"23\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1412304\"/><lb id=\"1422304\" tr=\"23\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1422304\"/><lb id=\"1432304\" tr=\"23\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1432304\"/><lb id=\"1402404\" tr=\"24\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1402404\"/><lb id=\"1412404\" tr=\"24\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1412404\"/><lb id=\"1422404\" tr=\"24\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1422404\"/><lb id=\"1432404\" tr=\"24\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1432404\"/><lb id=\"1402504\" tr=\"25\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1402504\"/><lb id=\"1412504\" tr=\"25\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1412504\"/><lb id=\"1422504\" tr=\"25\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1422504\"/><lb id=\"1432504\" tr=\"25\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1432504\"/><lb id=\"1402604\" tr=\"26\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1402604\"/><lb id=\"1412604\" tr=\"26\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1412604\"/><lb id=\"1422604\" tr=\"26\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1422604\"/><lb id=\"1432604\" tr=\"26\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1432604\"/><lb id=\"1402704\" tr=\"27\" gm=\"4\" rt=\"4\" sc=\"0\" sId=\"MP_TIME_TRIAL.Venom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1402704\"/><lb id=\"1412704\" tr=\"27\" gm=\"4\" rt=\"4\" sc=\"1\" sId=\"MP_TIME_TRIAL.Flash._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1412704\"/><lb id=\"1422704\" tr=\"27\" gm=\"4\" rt=\"4\" sc=\"2\" sId=\"MP_TIME_TRIAL.Rapier._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1422704\"/><lb id=\"1432704\" tr=\"27\" gm=\"4\" rt=\"4\" sc=\"3\" sId=\"MP_TIME_TRIAL.Phantom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1432704\"/><lb id=\"1500004\" tr=\"0\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500004\"/><lb id=\"1510004\" tr=\"0\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510004\"/><lb id=\"1520004\" tr=\"0\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520004\"/><lb id=\"1530004\" tr=\"0\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530004\"/><lb id=\"1500104\" tr=\"1\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500104\"/><lb id=\"1510104\" tr=\"1\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510104\"/><lb id=\"1520104\" tr=\"1\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520104\"/><lb id=\"1530104\" tr=\"1\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530104\"/><lb id=\"1500204\" tr=\"2\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500204\"/><lb id=\"1510204\" tr=\"2\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510204\"/><lb id=\"1520204\" tr=\"2\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520204\"/><lb id=\"1530204\" tr=\"2\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530204\"/><lb id=\"1500304\" tr=\"3\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500304\"/><lb id=\"1510304\" tr=\"3\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510304\"/><lb id=\"1520304\" tr=\"3\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520304\"/><lb id=\"1530304\" tr=\"3\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530304\"/><lb id=\"1500404\" tr=\"4\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500404\"/><lb id=\"1510404\" tr=\"4\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510404\"/><lb id=\"1520404\" tr=\"4\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520404\"/><lb id=\"1530404\" tr=\"4\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530404\"/><lb id=\"1500504\" tr=\"5\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500504\"/><lb id=\"1510504\" tr=\"5\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510504\"/><lb id=\"1520504\" tr=\"5\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520504\"/><lb id=\"1530504\" tr=\"5\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530504\"/><lb id=\"1500604\" tr=\"6\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500604\"/><lb id=\"1510604\" tr=\"6\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510604\"/><lb id=\"1520604\" tr=\"6\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520604\"/><lb id=\"1530604\" tr=\"6\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530604\"/><lb id=\"1500704\" tr=\"7\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500704\"/><lb id=\"1510704\" tr=\"7\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510704\"/><lb id=\"1520704\" tr=\"7\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520704\"/><lb id=\"1530704\" tr=\"7\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530704\"/><lb id=\"1500804\" tr=\"8\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500804\"/><lb id=\"1510804\" tr=\"8\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510804\"/><lb id=\"1520804\" tr=\"8\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520804\"/><lb id=\"1530804\" tr=\"8\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530804\"/><lb id=\"1500904\" tr=\"9\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1500904\"/><lb id=\"1510904\" tr=\"9\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1510904\"/><lb id=\"1520904\" tr=\"9\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1520904\"/><lb id=\"1530904\" tr=\"9\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1530904\"/><lb id=\"1501004\" tr=\"10\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501004\"/><lb id=\"1511004\" tr=\"10\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511004\"/><lb id=\"1521004\" tr=\"10\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521004\"/><lb id=\"1531004\" tr=\"10\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531004\"/><lb id=\"1501104\" tr=\"11\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501104\"/><lb id=\"1511104\" tr=\"11\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511104\"/><lb id=\"1521104\" tr=\"11\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521104\"/><lb id=\"1531104\" tr=\"11\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531104\"/><lb id=\"1501204\" tr=\"12\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501204\"/><lb id=\"1511204\" tr=\"12\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511204\"/><lb id=\"1521204\" tr=\"12\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521204\"/><lb id=\"1531204\" tr=\"12\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531204\"/><lb id=\"1501304\" tr=\"13\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501304\"/><lb id=\"1511304\" tr=\"13\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511304\"/><lb id=\"1521304\" tr=\"13\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521304\"/><lb id=\"1531304\" tr=\"13\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531304\"/><lb id=\"1501404\" tr=\"14\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501404\"/><lb id=\"1511404\" tr=\"14\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511404\"/><lb id=\"1521404\" tr=\"14\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521404\"/><lb id=\"1531404\" tr=\"14\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531404\"/><lb id=\"1501504\" tr=\"15\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501504\"/><lb id=\"1511504\" tr=\"15\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511504\"/><lb id=\"1521504\" tr=\"15\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521504\"/><lb id=\"1531504\" tr=\"15\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531504\"/><lb id=\"1501604\" tr=\"16\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501604\"/><lb id=\"1511604\" tr=\"16\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511604\"/><lb id=\"1521604\" tr=\"16\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521604\"/><lb id=\"1531604\" tr=\"16\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531604\"/><lb id=\"1501704\" tr=\"17\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501704\"/><lb id=\"1511704\" tr=\"17\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511704\"/><lb id=\"1521704\" tr=\"17\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521704\"/><lb id=\"1531704\" tr=\"17\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531704\"/><lb id=\"1501804\" tr=\"18\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501804\"/><lb id=\"1511804\" tr=\"18\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511804\"/><lb id=\"1521804\" tr=\"18\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521804\"/><lb id=\"1531804\" tr=\"18\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531804\"/><lb id=\"1501904\" tr=\"19\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1501904\"/><lb id=\"1511904\" tr=\"19\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1511904\"/><lb id=\"1521904\" tr=\"19\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1521904\"/><lb id=\"1531904\" tr=\"19\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1531904\"/><lb id=\"1502004\" tr=\"20\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1502004\"/><lb id=\"1512004\" tr=\"20\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1512004\"/><lb id=\"1522004\" tr=\"20\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1522004\"/><lb id=\"1532004\" tr=\"20\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1532004\"/><lb id=\"1502104\" tr=\"21\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1502104\"/><lb id=\"1512104\" tr=\"21\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1512104\"/><lb id=\"1522104\" tr=\"21\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1522104\"/><lb id=\"1532104\" tr=\"21\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1532104\"/><lb id=\"1502204\" tr=\"22\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1502204\"/><lb id=\"1512204\" tr=\"22\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1512204\"/><lb id=\"1522204\" tr=\"22\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1522204\"/><lb id=\"1532204\" tr=\"22\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1532204\"/><lb id=\"1502304\" tr=\"23\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1502304\"/><lb id=\"1512304\" tr=\"23\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1512304\"/><lb id=\"1522304\" tr=\"23\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1522304\"/><lb id=\"1532304\" tr=\"23\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1532304\"/><lb id=\"1502404\" tr=\"24\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1502404\"/><lb id=\"1512404\" tr=\"24\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1512404\"/><lb id=\"1522404\" tr=\"24\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1522404\"/><lb id=\"1532404\" tr=\"24\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1532404\"/><lb id=\"1502504\" tr=\"25\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1502504\"/><lb id=\"1512504\" tr=\"25\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1512504\"/><lb id=\"1522504\" tr=\"25\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1522504\"/><lb id=\"1532504\" tr=\"25\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1532504\"/><lb id=\"1502604\" tr=\"26\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1502604\"/><lb id=\"1512604\" tr=\"26\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1512604\"/><lb id=\"1522604\" tr=\"26\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1522604\"/><lb id=\"1532604\" tr=\"26\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1532604\"/><lb id=\"1502704\" tr=\"27\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1502704\"/><lb id=\"1512704\" tr=\"27\" gm=\"5\" rt=\"4\" sc=\"1\" sId=\"MP_SINGLE_RACE.Flash._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1512704\"/><lb id=\"1522704\" tr=\"27\" gm=\"5\" rt=\"4\" sc=\"2\" sId=\"MP_SINGLE_RACE.Rapier._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1522704\"/><lb id=\"1532704\" tr=\"27\" gm=\"5\" rt=\"4\" sc=\"3\" sId=\"MP_SINGLE_RACE.Phantom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1532704\"/><lb id=\"1600008\" tr=\"0\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600008\"/><lb id=\"1610008\" tr=\"0\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610008\"/><lb id=\"1620008\" tr=\"0\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620008\"/><lb id=\"1630008\" tr=\"0\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630008\"/><lb id=\"1600108\" tr=\"1\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600108\"/><lb id=\"1610108\" tr=\"1\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610108\"/><lb id=\"1620108\" tr=\"1\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620108\"/><lb id=\"1630108\" tr=\"1\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630108\"/><lb id=\"1600208\" tr=\"2\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600208\"/><lb id=\"1610208\" tr=\"2\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610208\"/><lb id=\"1620208\" tr=\"2\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620208\"/><lb id=\"1630208\" tr=\"2\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630208\"/><lb id=\"1600308\" tr=\"3\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600308\"/><lb id=\"1610308\" tr=\"3\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610308\"/><lb id=\"1620308\" tr=\"3\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620308\"/><lb id=\"1630308\" tr=\"3\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630308\"/><lb id=\"1600408\" tr=\"4\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600408\"/><lb id=\"1610408\" tr=\"4\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610408\"/><lb id=\"1620408\" tr=\"4\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620408\"/><lb id=\"1630408\" tr=\"4\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630408\"/><lb id=\"1600508\" tr=\"5\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600508\"/><lb id=\"1610508\" tr=\"5\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610508\"/><lb id=\"1620508\" tr=\"5\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620508\"/><lb id=\"1630508\" tr=\"5\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630508\"/><lb id=\"1600608\" tr=\"6\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600608\"/><lb id=\"1610608\" tr=\"6\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610608\"/><lb id=\"1620608\" tr=\"6\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620608\"/><lb id=\"1630608\" tr=\"6\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630608\"/><lb id=\"1600708\" tr=\"7\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600708\"/><lb id=\"1610708\" tr=\"7\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610708\"/><lb id=\"1620708\" tr=\"7\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620708\"/><lb id=\"1630708\" tr=\"7\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630708\"/><lb id=\"1600808\" tr=\"8\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600808\"/><lb id=\"1610808\" tr=\"8\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610808\"/><lb id=\"1620808\" tr=\"8\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620808\"/><lb id=\"1630808\" tr=\"8\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630808\"/><lb id=\"1600908\" tr=\"9\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1600908\"/><lb id=\"1610908\" tr=\"9\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1610908\"/><lb id=\"1620908\" tr=\"9\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1620908\"/><lb id=\"1630908\" tr=\"9\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1630908\"/><lb id=\"1601008\" tr=\"10\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601008\"/><lb id=\"1611008\" tr=\"10\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611008\"/><lb id=\"1621008\" tr=\"10\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621008\"/><lb id=\"1631008\" tr=\"10\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631008\"/><lb id=\"1601108\" tr=\"11\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601108\"/><lb id=\"1611108\" tr=\"11\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611108\"/><lb id=\"1621108\" tr=\"11\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621108\"/><lb id=\"1631108\" tr=\"11\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631108\"/><lb id=\"1601208\" tr=\"12\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601208\"/><lb id=\"1611208\" tr=\"12\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611208\"/><lb id=\"1621208\" tr=\"12\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621208\"/><lb id=\"1631208\" tr=\"12\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631208\"/><lb id=\"1601308\" tr=\"13\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601308\"/><lb id=\"1611308\" tr=\"13\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611308\"/><lb id=\"1621308\" tr=\"13\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621308\"/><lb id=\"1631308\" tr=\"13\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631308\"/><lb id=\"1601408\" tr=\"14\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601408\"/><lb id=\"1611408\" tr=\"14\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611408\"/><lb id=\"1621408\" tr=\"14\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621408\"/><lb id=\"1631408\" tr=\"14\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631408\"/><lb id=\"1601508\" tr=\"15\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601508\"/><lb id=\"1611508\" tr=\"15\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611508\"/><lb id=\"1621508\" tr=\"15\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621508\"/><lb id=\"1631508\" tr=\"15\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631508\"/><lb id=\"1601608\" tr=\"16\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601608\"/><lb id=\"1611608\" tr=\"16\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611608\"/><lb id=\"1621608\" tr=\"16\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621608\"/><lb id=\"1631608\" tr=\"16\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631608\"/><lb id=\"1601708\" tr=\"17\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601708\"/><lb id=\"1611708\" tr=\"17\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611708\"/><lb id=\"1621708\" tr=\"17\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621708\"/><lb id=\"1631708\" tr=\"17\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631708\"/><lb id=\"1601808\" tr=\"18\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601808\"/><lb id=\"1611808\" tr=\"18\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611808\"/><lb id=\"1621808\" tr=\"18\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621808\"/><lb id=\"1631808\" tr=\"18\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631808\"/><lb id=\"1601908\" tr=\"19\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1601908\"/><lb id=\"1611908\" tr=\"19\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1611908\"/><lb id=\"1621908\" tr=\"19\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1621908\"/><lb id=\"1631908\" tr=\"19\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1631908\"/><lb id=\"1602008\" tr=\"20\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1602008\"/><lb id=\"1612008\" tr=\"20\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1612008\"/><lb id=\"1622008\" tr=\"20\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1622008\"/><lb id=\"1632008\" tr=\"20\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1632008\"/><lb id=\"1602108\" tr=\"21\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1602108\"/><lb id=\"1612108\" tr=\"21\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1612108\"/><lb id=\"1622108\" tr=\"21\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1622108\"/><lb id=\"1632108\" tr=\"21\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1632108\"/><lb id=\"1602208\" tr=\"22\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1602208\"/><lb id=\"1612208\" tr=\"22\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1612208\"/><lb id=\"1622208\" tr=\"22\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1622208\"/><lb id=\"1632208\" tr=\"22\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1632208\"/><lb id=\"1602308\" tr=\"23\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1602308\"/><lb id=\"1612308\" tr=\"23\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1612308\"/><lb id=\"1622308\" tr=\"23\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1622308\"/><lb id=\"1632308\" tr=\"23\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1632308\"/><lb id=\"1602408\" tr=\"24\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1602408\"/><lb id=\"1612408\" tr=\"24\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1612408\"/><lb id=\"1622408\" tr=\"24\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1622408\"/><lb id=\"1632408\" tr=\"24\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1632408\"/><lb id=\"1602508\" tr=\"25\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1602508\"/><lb id=\"1612508\" tr=\"25\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1612508\"/><lb id=\"1622508\" tr=\"25\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1622508\"/><lb id=\"1632508\" tr=\"25\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1632508\"/><lb id=\"1602608\" tr=\"26\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1602608\"/><lb id=\"1612608\" tr=\"26\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1612608\"/><lb id=\"1622608\" tr=\"26\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1622608\"/><lb id=\"1632608\" tr=\"26\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1632608\"/><lb id=\"1602708\" tr=\"27\" gm=\"6\" rt=\"8\" sc=\"0\" sId=\"SP_OVERALL.Venom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1602708\"/><lb id=\"1612708\" tr=\"27\" gm=\"6\" rt=\"8\" sc=\"1\" sId=\"SP_OVERALL.Flash._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1612708\"/><lb id=\"1622708\" tr=\"27\" gm=\"6\" rt=\"8\" sc=\"2\" sId=\"SP_OVERALL.Rapier._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1622708\"/><lb id=\"1632708\" tr=\"27\" gm=\"6\" rt=\"8\" sc=\"3\" sId=\"SP_OVERALL.Phantom._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1632708\"/><lb id=\"1700008\" tr=\"0\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700008\"/><lb id=\"1700108\" tr=\"1\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._02_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700108\"/><lb id=\"1700208\" tr=\"2\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._03_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700208\"/><lb id=\"1700308\" tr=\"3\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._04_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700308\"/><lb id=\"1700408\" tr=\"4\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._05_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700408\"/><lb id=\"1700508\" tr=\"5\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._06_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700508\"/><lb id=\"1700608\" tr=\"6\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._07_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700608\"/><lb id=\"1700708\" tr=\"7\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700708\"/><lb id=\"1700808\" tr=\"8\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._09_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700808\"/><lb id=\"1700908\" tr=\"9\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._10_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1700908\"/><lb id=\"1701008\" tr=\"10\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._11_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701008\"/><lb id=\"1701108\" tr=\"11\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._12_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701108\"/><lb id=\"1701208\" tr=\"12\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._13_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701208\"/><lb id=\"1701308\" tr=\"13\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._14_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701308\"/><lb id=\"1701408\" tr=\"14\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._15_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701408\"/><lb id=\"1701508\" tr=\"15\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._16_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701508\"/><lb id=\"1701608\" tr=\"16\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._17_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701608\"/><lb id=\"1701708\" tr=\"17\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._18_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701708\"/><lb id=\"1701808\" tr=\"18\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._19_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701808\"/><lb id=\"1701908\" tr=\"19\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._20_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1701908\"/><lb id=\"1702008\" tr=\"20\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._21_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1702008\"/><lb id=\"1702108\" tr=\"21\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._22_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1702108\"/><lb id=\"1702208\" tr=\"22\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._23_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1702208\"/><lb id=\"1702308\" tr=\"23\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._24_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1702308\"/><lb id=\"1702408\" tr=\"24\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._25_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1702408\"/><lb id=\"1702508\" tr=\"25\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._26_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1702508\"/><lb id=\"1702608\" tr=\"26\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._27_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1702608\"/><lb id=\"1702708\" tr=\"27\" gm=\"7\" rt=\"8\" sId=\"SP_DETONATOR._28_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1702708\"/><lb id=\"1800014\" gm=\"8\" rt=\"14\" sId=\"MP_RANKED\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/1800014\"/>" +
                                        "</GetList>" +
                                        "</Leaderboard>"));
                        }
                        break;

                    case "/wox_ws/rest/lb/GetMediusStats":

                        switch (method)
                        {
                            case "GET":

                                string name = string.Empty;

                                string authKey = string.Empty;

                                string timeZone = string.Empty;

                                string signature = string.Empty;
                                string id = string.Empty;

                                string? cookieHeader = ctx.Request.Headers.Get("Cookie");

                                if (cookieHeader != null)
                                {
                                    ctx.Response.ChunkedTransfer = true;

                                    string[] cookies = cookieHeader.Split(';');

                                    foreach (string cookie in cookies)
                                    {
                                        string[] parts = cookie.Trim().Split('=');

                                        string key = parts[0];
                                        string value = parts[1];

                                        if (key == "name")
                                            name = value;
                                        else if (key == "authKey")
                                            authKey = value;
                                        else if (key == "timeZone")
                                            timeZone = value;
                                        else if (key == "signature")
                                            signature = value;
                                        else if (key == "id")
                                            id = value;
                                    }

                                    ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                    ctx.Response.Headers.Set("Content-Language", string.Empty);

                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                    return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        $"<MediusStats accountId=\"{id}\" accountName=\"{name}\"/>"));
                                }
                                else
                                {
                                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    ctx.Response.ContentType = "text/plain";
                                    return await ctx.Response.Send();
                                }
                        }
                        break;

                    case "/wox_ws/rest/account/Friends":

                        switch (method)
                        {
                            case "POST":

                                string name = string.Empty;

                                string authKey = string.Empty;

                                string timeZone = string.Empty;

                                string signature = string.Empty;
                                string id = string.Empty;

                                string? cookieHeader = ctx.Request.Headers.Get("Cookie");

                                if (cookieHeader != null)
                                {
                                    ctx.Response.ChunkedTransfer = true;

                                    string[] cookies = cookieHeader.Split(';');

                                    foreach (string cookie in cookies)
                                    {
                                        string[] parts = cookie.Trim().Split('=');

                                        string key = parts[0];
                                        string value = parts[1];

                                        if (key == "name")
                                            name = value;
                                        else if (key == "authKey")
                                            authKey = value;
                                        else if (key == "timeZone")
                                            timeZone = value;
                                        else if (key == "signature")
                                            signature = value;
                                        else if (key == "id")
                                            id = value;
                                    }

                                    if (ctx.Request.HeaderExists("Content-Type"))
                                        ctx.Response.Headers.Set("Content-Type", ctx.Request.RetrieveHeaderValue("Content-Type"));
                                    else
                                        ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");

                                    ctx.Response.Headers.Set("Content-Language", string.Empty);

                                    byte[] buffer = ctx.Request.DataAsBytes;

                                    Directory.CreateDirectory($"{SVOServerConfiguration.SVOStaticFolder}/wox_ws/rest/account");

                                    using (FileStream fs = new($"{SVOServerConfiguration.SVOStaticFolder}/wox_ws/rest/account/Friends.{name}", FileMode.OpenOrCreate))
                                    {
                                        fs.Write(buffer, 0, buffer.Length);
                                        fs.Flush();
                                    }

                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;

                                    return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                       "<Friends><status value=\"0\"/></Friends>"));
                                }
                                else
                                {
                                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    ctx.Response.ContentType = "text/plain";
                                    return await ctx.Response.Send();
                                }
                        }
                        break;

                    case "/wox_ws/rest/account/TicketLogin":

                        switch (method)
                        {
                            case "POST":
                                ctx.Response.ChunkedTransfer = true;

                                string signature = string.Empty;

                                string signatureClass = string.Empty;

                                string userContext = string.Empty;

                                string languageId = string.Empty;

                                string timeZone = string.Empty;

                                string psnname = string.Empty;
                                int accountId = -1;

                                string url = ctx.Request.Url.RawWithQuery.ToString();

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

                                byte[] buffer = ctx.Request.DataAsBytes;

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
                                psnname = Encoding.ASCII.GetString(extractedData).Replace(" ", string.Empty);

                                if (DataTypesUtils.FindBytePattern(buffer, new byte[] { 0x52, 0x50, 0x43, 0x4E }) != -1)
                                    LoggerAccessor.LogInfo($"OTG_HTTPS : User {psnname} logged in and is on RPCN");
                                else
                                    LoggerAccessor.LogInfo($"OTG_HTTPS : User {psnname} logged in and is on PSN");

                                try
                                {
                                    await SVOServerConfiguration.Database.GetAccountByName(psnname, 23360).ContinueWith((r) =>
                                    {
                                        //Found in database so keep.
                                        string langId = ctx.Request.RetrieveQueryValue("languageId");
                                        string? accountName = r.Result.AccountName;
                                        accountId = r.Result.AccountId;
                                    });
                                }
                                catch (Exception)
                                {
                                    string langId = ctx.Request.RetrieveQueryValue("languageId");
                                    accountId = 0;
                                }

                                ctx.Response.Headers.Add("Set-Cookie", $"id=ddb4fac6-f908-33e5-80f9-febd2e2ef58f; Path=/");
                                ctx.Response.Headers.Add("Set-Cookie", $"name={psnname}; Path=/");
                                ctx.Response.Headers.Add("Set-Cookie", $"authKey=2b8e1723-9e40-41e6-a740-05ddefacfe94; Path=/");
                                ctx.Response.Headers.Add("Set-Cookie", $"timeZone=GMT; Path=/");
                                ctx.Response.Headers.Add("Set-Cookie", $"signature=ghpE-ws_dBmIY-WNbkCQb1NnamA; Path=/");

                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", string.Empty);

                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                    "<SP_Login>\r\n" +
                                    "   <status> \r\n" +
                                    "        <id>1</id> \r\n" +
                                    "        <message>Success</message> \r\n" +
                                    "   </status> \r\n" +
                                    $"   <accountID>{accountId}</accountID>\r\n\t" +
                                    $"   <languageID>{languageId}</languageID>\r\n" +
                                    $"   <userContext>{userContext}</userContext> \r\n" +
                                    "</SP_Login>"));
                        }
                        break;

                    case "/wox_ws/rest/lb/GetPlayerTimes":

                        switch (method)
                        {
                            case "GET":
                                ctx.Response.ChunkedTransfer = true;

                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", string.Empty);

                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<GetPlayerTimes><status value=\"0\"/></GetPlayerTimes>"));
                        }
                        break;

                    case "/wox_ws/rest/lb/GetPage":

                        switch (method)
                        {
                            case "GET":
                                ctx.Response.ChunkedTransfer = true;

                                string leaderboardId = string.Empty;

                                string row = string.Empty;

                                string pageSize = string.Empty;

                                string accountName = string.Empty;

                                string accountId = string.Empty;

                                string filterMode = string.Empty;

                                string url = ctx.Request.Url.RawWithQuery.ToString();

                                string[] urlParts = url.Split('?');
                                string basePath = urlParts[0];
                                string queryString = urlParts.Length > 1 ? urlParts[1] : string.Empty;

                                string[] parameters = queryString.Split('&');

                                foreach (string parameter in parameters)
                                {
                                    string[] parts = parameter.Split('=');

                                    string key = Uri.UnescapeDataString(parts[0]);
                                    string value = Uri.UnescapeDataString(parts[1]);

                                    if (key == "leaderboardId")
                                        leaderboardId = value;
                                    else if (key == "row")
                                        row = value;
                                    else if (key == "pageSize")
                                        pageSize = value;
                                    else if (key == "accountName")
                                        accountName = value;
                                    else if (key == "accountId")
                                        accountId = value;
                                    else if (key == "filterMode")
                                        filterMode = value;
                                }

                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", string.Empty);

                                byte[] GetPageData;

                                if (!string.IsNullOrEmpty(filterMode))
                                {
                                    GetPageData = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<Leaderboard>" +
                                        "<GetPage>" +
                                        $"<lb id=\"{leaderboardId}\" tr=\"7\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/{leaderboardId}\"/>" +
                                        "<Stats isFirst=\"true\" isLast=\"true\" row=\"0\" page=\"1\" size=\"1\" leaderboardSize=\"114585\" totalEntries=\"1\">" +
                                        "<Rank position=\"110718\" value=\"15453\" name=\"Guitar-man4\" id=\"1c96d187-6919-3ca7-b01d-cfcd4f96167e\" track=\"7\" speedClass=\"0\" team=\"13\" lapTime=\"3648\" raceTime=\"15453\" laps=\"4\" rankedScore=\"1010\" rank=\"6\" ghost=\"false\"/>" +
                                        "</Stats>" +
                                        "</GetPage>" +
                                        "</Leaderboard>");
                                }
                                else
                                {
                                    GetPageData = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<Leaderboard>" +
                                        "<GetPage>" +
                                        $"<lb id=\"{leaderboardId}\" tr=\"7\" gm=\"5\" rt=\"4\" sc=\"0\" sId=\"MP_SINGLE_RACE.Venom._08_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/{leaderboardId}\"/>" +
                                        "<Stats isFirst=\"true\" isLast=\"false\" row=\"0\" page=\"1\" size=\"1\" leaderboardSize=\"114585\" totalEntries=\"114585\">" +
                                        "<Rank position=\"1\" value=\"0\" name=\"Pyramat\" id=\"0002d65b-036b-3c9d-89b8-25ba46b857c7\" track=\"7\" speedClass=\"0\" team=\"4\" lapTime=\"-1\" raceTime=\"0\" laps=\"0\" rankedScore=\"1450\" rank=\"6\" ghost=\"false\"/>" +
                                        "</Stats>" +
                                        "</GetPage>" +
                                        "</Leaderboard>");
                                }

                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                return await ctx.Response.SendFinalChunk(GetPageData);
                        }
                        break;

                    case "/wox_ws/rest/lbNGP/GetXPLeaderboard":

                        switch (method)
                        {
                            case "GET":

                                string name = string.Empty;

                                string authKey = string.Empty;

                                string timeZone = string.Empty;

                                string signature = string.Empty;
                                string id = string.Empty;

                                string? cookieHeader = ctx.Request.Headers.Get("Cookie");

                                if (cookieHeader != null)
                                {
                                    ctx.Response.ChunkedTransfer = true;

                                    string[] cookies = cookieHeader.Split(';');

                                    foreach (string cookie in cookies)
                                    {
                                        string[] parts = cookie.Trim().Split('=');

                                        string key = parts[0];
                                        string value = parts[1];

                                        if (key == "name")
                                            name = value;
                                        else if (key == "authKey")
                                            authKey = value;
                                        else if (key == "timeZone")
                                            timeZone = value;
                                        else if (key == "signature")
                                            signature = value;
                                        else if (key == "id")
                                            id = value;
                                    }

                                    ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                    ctx.Response.Headers.Set("Content-Language", string.Empty);

                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                    return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        $"<XPLeaderboard/>"));
                                }
                                else
                                {
                                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    ctx.Response.ContentType = "text/plain";
                                    return await ctx.Response.Send();
                                }
                        }
                        break;

                    case "/wox_ws/rest/games/PostScore":

                        switch (method)
                        {
                            case "GET":

                                string name = string.Empty;

                                string authKey = string.Empty;

                                string timeZone = string.Empty;

                                string signature = string.Empty;
                                string id = string.Empty;

                                string? cookieHeader = ctx.Request.Headers.Get("Cookie");

                                if (cookieHeader != null)
                                {
                                    ctx.Response.ChunkedTransfer = true;

                                    string[] cookies = cookieHeader.Split(';');

                                    foreach (string cookie in cookies)
                                    {
                                        string[] parts = cookie.Trim().Split('=');

                                        string key = parts[0];
                                        string value = parts[1];

                                        if (key == "name")
                                            name = value;
                                        else if (key == "authKey")
                                            authKey = value;
                                        else if (key == "timeZone")
                                            timeZone = value;
                                        else if (key == "signature")
                                            signature = value;
                                        else if (key == "id")
                                            id = value;
                                    }

                                    Dictionary<string, string> parameterDictionary = new(); // Query sent are dynamic for this request.

                                    int questionMarkIndex = ctx.Request.Url.RawWithQuery.IndexOf("?");
                                    if (questionMarkIndex != -1) // If '?' is found
                                    {
                                        string trimmedurl = ctx.Request.Url.RawWithQuery[(questionMarkIndex + 1)..];
                                        foreach (string? UrlArg in System.Web.HttpUtility.ParseQueryString(trimmedurl).AllKeys) // Thank you WebOne.
                                        {
                                            if (!string.IsNullOrEmpty(UrlArg))
                                                parameterDictionary[UrlArg] = System.Web.HttpUtility.ParseQueryString(trimmedurl)[UrlArg] ?? string.Empty;
                                        }
                                    }

                                    ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                    ctx.Response.Headers.Set("Content-Language", string.Empty);

                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                    return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<PostScore>" +
                                        "</PostScore>"));
                                }
                                else
                                {
                                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    ctx.Response.ContentType = "text/plain";
                                    return await ctx.Response.Send();
                                }
                        }
                        break;

                    case "/wox_ws/rest/lb/GuessRanking":

                        switch (method)
                        {
                            case "GET":

                                string name = string.Empty;

                                string authKey = string.Empty;

                                string timeZone = string.Empty;

                                string signature = string.Empty;
                                string id = string.Empty;

                                string? cookieHeader = ctx.Request.Headers.Get("Cookie");

                                if (cookieHeader != null)
                                {
                                    ctx.Response.ChunkedTransfer = true;

                                    string[] cookies = cookieHeader.Split(';');

                                    foreach (string cookie in cookies)
                                    {
                                        string[] parts = cookie.Trim().Split('=');

                                        string key = parts[0];
                                        string value = parts[1];

                                        if (key == "name")
                                            name = value;
                                        else if (key == "authKey")
                                            authKey = value;
                                        else if (key == "timeZone")
                                            timeZone = value;
                                        else if (key == "signature")
                                            signature = value;
                                        else if (key == "id")
                                            id = value;
                                    }

                                    string leaderboardId = string.Empty;
                                    if (ctx.Request.QuerystringExists("leaderboardId"))
                                        leaderboardId = ctx.Request.RetrieveQueryValue("leaderboardId");

                                    ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                    ctx.Response.Headers.Set("Content-Language", string.Empty);

                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                    return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<Leaderboard>" +
                                        "<GuessRanking>" +
                                        $"<lb id=\"{leaderboardId}\" tr=\"0\" gm=\"0\" rt=\"4\" sc=\"0\" sId=\"SP_TIME_TRIAL.Venom._01_Track\" url=\"https://wipeout2048.online.scee.com:10062/wox_ws/rest/lb/{leaderboardId}\"/>" +
                                        "</GuessRanking>" +
                                        "</Leaderboard>"));
                                }
                                else
                                {
                                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    ctx.Response.ContentType = "text/plain";
                                    return await ctx.Response.Send();
                                }
                        }
                        break;

                    case "/wox_ws/rest/binary/Download":

                        switch (method)
                        {
                            case "GET":
                                string name = string.Empty;

                                string authKey = string.Empty;

                                string timeZone = string.Empty;

                                string signature = string.Empty;
                                string id = string.Empty;

                                string? cookieHeader = ctx.Request.Headers.Get("Cookie");

                                if (cookieHeader != null)
                                {
                                    ctx.Response.ChunkedTransfer = true;

                                    string[] cookies = cookieHeader.Split(';');

                                    foreach (string cookie in cookies)
                                    {
                                        string[] parts = cookie.Trim().Split('=');

                                        string key = parts[0];
                                        string value = parts[1];

                                        if (key == "name")
                                            name = value;
                                        else if (key == "authKey")
                                            authKey = value;
                                        else if (key == "timeZone")
                                            timeZone = value;
                                        else if (key == "signature")
                                            signature = value;
                                        else if (key == "id")
                                            id = value;
                                    }

                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                    ctx.Response.ContentType = "text/xml";
                                    ctx.Response.Headers.Set("Content-Language", string.Empty);

                                    string fileName = ctx.Request.RetrieveQueryValue("filename");

                                    if (!new Regex(@"\.[^.]+$").Match(fileName).Success) // We give a default extension if none found.
                                        fileName += ".bin";

                                    if (File.Exists($"{SVOServerConfiguration.SVOStaticFolder}/wox_ws/rest/fileservices/{name}/{fileName}"))
                                        return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes($"<BinaryDownload checksum=\"{SVOProcessor.CalcuateOTGSecuredHash("m4nT15")}\">\n" +
                                                $"        <Data>{File.ReadAllText($"{SVOServerConfiguration.SVOStaticFolder}/wox_ws/rest/fileservices/{name}/{fileName}")}</Data>\n" +
                                                $"    </BinaryDownload>"));
                                    else
                                        return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes($"<BinaryDownload checksum=\"{SVOProcessor.CalcuateOTGSecuredHash("m4nT15")}\">\n" +
                                                "        <Data></Data>\n" +
                                                $"    </BinaryDownload>"));
                                }
                                else
                                {
                                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    ctx.Response.ContentType = "text/plain";
                                    return await ctx.Response.Send();
                                }
                        }
                        break;

                    case "/wox_ws/rest/activities/FriendActivities":

                        switch (method)
                        {
                            case "GET":
                                string name = string.Empty;

                                string authKey = string.Empty;

                                string timeZone = string.Empty;

                                string signature = string.Empty;
                                string id = string.Empty;

                                string? cookieHeader = ctx.Request.Headers.Get("Cookie");

                                if (cookieHeader != null)
                                {
                                    ctx.Response.ChunkedTransfer = true;

                                    string[] cookies = cookieHeader.Split(';');

                                    foreach (string cookie in cookies)
                                    {
                                        string[] parts = cookie.Trim().Split('=');

                                        string key = parts[0];
                                        string value = parts[1];

                                        if (key == "name")
                                            name = value;
                                        else if (key == "authKey")
                                            authKey = value;
                                        else if (key == "timeZone")
                                            timeZone = value;
                                        else if (key == "signature")
                                            signature = value;
                                        else if (key == "id")
                                            id = value;
                                    }

                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                    ctx.Response.ContentType = "text/xml";
                                    ctx.Response.Headers.Set("Content-Language", string.Empty);

                                    string filter = ctx.Request.RetrieveQueryValue("filter");

                                    return await ctx.Response.SendFinalChunk(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                       "<FriendActivities>" +
                                       "</FriendActivities>"));
                                }
                                else
                                {
                                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    ctx.Response.ContentType = "text/plain";
                                    return await ctx.Response.Send();
                                }
                        }
                        break;
                    default:
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        return await ctx.Response.Send();

                        #endregion
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[OTG_HTTPS] - Wipeout2048_OTG thrown an assertion - {ex}");

                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                ctx.Response.ContentType = "text/plain";
                return await ctx.Response.Send();
            }

            ctx.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            ctx.Response.ContentType = "text/plain";
            return await ctx.Response.Send();
        }
    }
}
