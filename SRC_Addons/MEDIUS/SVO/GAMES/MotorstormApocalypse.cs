using DotNetty.Common.Internal.Logging;
using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace PSMultiServer.Addons.Medius.SVO.GAMES
{
    public class MotorstormApocalypse
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<MotorstormApocalypse>();
        public static async Task MotorstormApocalypse_SVO(HttpListenerContext context, string userAgent)
        {
            using (var response = context.Response)
            {
                try
                {
                    switch (context.Request.Url.AbsolutePath)
                    {

                        case "/motorstorm3ps3_xml/account/TicketLoginAction":

                            switch (context.Request.HttpMethod)
                            {
                                case "POST":

                                    response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                    response.Headers.Set("Content-Language", "");

                                    byte[] TicketLoginActionData = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<SP_Login>\r\n" +
                                        "   <status> \r\n" +
                                        "        <id>1</id> \r\n" +
                                        "        <message>Success</message> \r\n" +
                                        "   </status> \r\n" +
                                        "   <accountID>1</accountID>\r\n\t" +
                                        "   <languageID>1</languageID>\r\n" +
                                        "   <userContext>0</userContext> \r\n" +
                                        "</SP_Login>");

                                    Stream ros = response.OutputStream;

                                    if (ros.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = 200;
                                            response.SendChunked = true;
                                            ros.Write(TicketLoginActionData, 0, TicketLoginActionData.Length);
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

                        case "/motorstorm3ps3_xml/binary/actionList":

                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                    response.Headers.Set("Content-Language", "");

                                    byte[] actionListData = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<!-- Announcements -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"retrieveAnnounceementText\" value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/announcements/RetrieveAnnouncementTextAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"listCurrentAnnouncements\" value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/announcements/ListCurrentAnnouncementsAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"markAnnouncementRead\" value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/announcements/MarkAnnouncementReadAction\" />\r\n\t\r\n" +
                                        "<!-- Ghosts-->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"uploadGhostData\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/ghost/UploadGhostDataAction\" /> \r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"uploadGhost\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/ghost/uploadGhostData\" />\r\n\t<DATA dataType=\"URI\" name=\"downloadGhostData\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/ghost/DownloadGhostDataAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"downloadGhost\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/ghost/downloadGhostData\" />\r\n\r\n" +
                                        "<!-- Playlists -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"getPlaylists\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/playlist/GetPlaylistsAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"getPlaylist\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/playlist/GetPlaylistAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"generateNextGame\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/playlist/GenerateNextGameAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"joinPlaylist\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/playlist/JoinPlaylistAction\" />\r\n\r\n" +
                                        "<!-- Pictures -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"downloadPicture\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/pictures/DownloadPictureAction\" />\r\n\r\n" +
                                        "<!-- Lobbies -->\r\n\t\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"findLobbies\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/lobbies/FindLobbiesAction\" />\r\n\t\r\n" +
                                        "<!-- Vehicle Customizations -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"uploadVehicleCustomisationData\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/customisations/UploadVehicleCustomisationDataAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"downloadVehicleCustomisationData\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/customisations/customisations/DownloadVehicleCustomisationDataAction\" />\r\n\r\n" +
                                        "<!-- Gamemode -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"getGameModes\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/gamemode/GetGameModesAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"getGameModesAttributes\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/gamemode/GetGameModesAttributesAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"uploadCustomGameMode\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/gamemode/UploadGameModeFileAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"downloadCustomGameMode\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/gamemode/DownloadGameModeFileAction\" />\r\n\r\n" +
                                        "<!-- Game Configuration-->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"getAvailableGameConfiguration\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/gameconfig/GetAvailableGameConfigurationAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"getGameConfigurationyName\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/gameconfig/GetGameConfigurationyNameAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"createOrUpdateGameConfiguration\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/gameconfig/CreateOrUpdateGameConfigurationAction\" />\r\n" +
                                        "<!-- Game -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"createGame\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/game/CreateGameAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"leaveGame\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/game/LeaveGameAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"updateGame\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/game/UpdateGameAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"listGames\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/game/ListGamesAction\" />\r\n\t\r\n" +
                                        "<!-- Friends -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"getFriends\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/friends/GetFriendsAction\" />\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"syncFriends\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/friends/SyncFriendsAction\" />\r\n\t\r\n" +
                                        "<!-- Account -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"ticketLogin\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/account/TicketLoginAction\" />\r\n" +
                                        "<!-- Trophies -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"syncTrophies\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/trophies/SyncTrophiesAction\" />\r\n\t\r\n" +
                                        "<!-- Profile -->\r\n\t\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"verifyProfileVersion\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/profile/VerifyProfileVersionAction\" />\r\n" +
                                        "<!-- Misc -->\r\n\t" +
                                        "<DATA dataType=\"URI\" name=\"downloadBinary\"value=\"http://motorstorm3ps3.ws.online.scee.com:10060/motorstorm3ps3_xml/binary/DownloadBinaryAction\" />");

                                    Stream ros = response.OutputStream;

                                    if (ros.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = 200;
                                            response.SendChunked = true;
                                            ros.Write(actionListData, 0, actionListData.Length);
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

                        case "/motorstorm3ps3_xml/friends/SyncFriendsAction":

                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                    response.Headers.Set("Content-Language", "");

                                    byte[] SyncFriendsActionData = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<Friends><status value=\"0\"/></Friends>");

                                    Stream ros = response.OutputStream;

                                    if (ros.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = 200;
                                            response.SendChunked = true;
                                            ros.Write(SyncFriendsActionData, 0, SyncFriendsActionData.Length);
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

                        case "/motorstorm3ps3_xml/playlist/GetPlaylistsAction":

                            switch (context.Request.HttpMethod)
                            {
                                case "GET":

                                    response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                    response.Headers.Set("Content-Language", "");

                                    byte[] GetPlaylistsActionData = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                                        "<PlayLists>\r\n" +
                                        "<status value=\"0\"/>\r\n" +
                                        "<PlayListId>1</PlayListId>\r\n" +
                                        "</PlayLists>");

                                    Stream ros = response.OutputStream;

                                    if (ros.CanWrite)
                                    {
                                        try
                                        {
                                            response.StatusCode = 200;
                                            response.SendChunked = true;
                                            ros.Write(GetPlaylistsActionData, 0, GetPlaylistsActionData.Length);
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

                        default:

                            Logger.Warn($"SVO server : {userAgent} Requested a MotorstormApocalypse SVO Method that doesn't exist.");

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
                    }
                }
                catch (Exception ex)
                {
                    ServerConfiguration.LogError($"SVO Server : an error occured in Ps_Home Request type - {ex}");
                }

                context.Response.Close();

                GC.Collect();

                return;
            }
        }
    }
}
