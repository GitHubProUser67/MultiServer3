using CustomLogger;
using Horizon.MUM.Models;
using Horizon.RT.Common;
using Horizon.RT.Models;
using NetworkLibrary.HTTP;
using System.Text;

namespace Horizon.SERVER.Extension.PlayStationHome
{
    public class HomeGuestJoiningSystem
    {
        public static Task<bool> SendCrcOverride(string targetClientIp, string? AccessToken, string SceneCrc, bool Retail)
        {
            bool AccessTokenProvided = !string.IsNullOrEmpty(AccessToken);
            List<ClientObject>? clients = null;

            if (AccessTokenProvided)
            {
                ClientObject? client = MediusClass.Manager.GetClientByAccessToken(AccessToken, Retail ? 20374 : 20371);
                if (client != null)
                {
                    clients = new()
                    {
                        client
                    };
                }
            }
            else
                clients = MediusClass.Manager.GetClientsByIp(targetClientIp, Retail ? 20374 : 20371);

            if (clients != null)
            {
                foreach (Game homeLobby in MediusClass.Manager.GetAllGamesByAppId(Retail ? 20374 : 20371))
                {
                    if (homeLobby.Host != null && !string.IsNullOrEmpty(homeLobby.GameName) && homeLobby.GameName.StartsWith("AP|") && homeLobby.GameName.Split('|').Length >= 5)
                    {
                        string LobbyName = homeLobby.GameName!.Split('|')[5];

                        if (GetGJSCRC(homeLobby.Host.AccountName!, LobbyName + "H3m0", homeLobby.utcTimeCreated) == SceneCrc)
                        {
                            string ssfwSceneNameResult = HTTPProcessor.RequestURLPOST($"{HorizonServerConfiguration.SSFWUrl}/WebService/GetSceneLike/", new Dictionary<string, string>() { { "like", LobbyName } }, string.Empty, "text/plain");

                            if (!string.IsNullOrEmpty(ssfwSceneNameResult))
                            {
                                foreach (ClientObject client in clients)
                                {
                                    client.LobbyKeyOverride = SceneCrc;
                                    _ = HomeRTMTools.SendRemoteCommand(client, $"lc Debug.System( 'map {ssfwSceneNameResult}' )");
                                    if (!string.IsNullOrEmpty(client.SSFWid) && !string.IsNullOrEmpty(homeLobby.Host.AccountName))
                                        HTTPProcessor.RequestURLPOST($"{HorizonServerConfiguration.SSFWUrl}/WebService/ApplyLayoutOverride/", new Dictionary<string, string>() { { "sessionid", client.SSFWid }, { "targetUserName", homeLobby.Host.AccountName } }, string.Empty, "text/plain");
                                }

                                return Task.FromResult(true);
                            }

                            LoggerAccessor.LogError($"[HomeGuestJoiningSystem] - {LobbyName} didn't match any SSFW entry!");

                            return Task.FromResult(false);
                        }
                    }
                }

                LoggerAccessor.LogError($"[HomeGuestJoiningSystem] - {SceneCrc} didn't match any Private lobby!");

                return Task.FromResult(false);
            }

            LoggerAccessor.LogError($"[HomeGuestJoiningSystem] - {(!AccessTokenProvided ? $"Ip:{targetClientIp}" : $"AccessToken:{AccessToken}")} didn't return any Medius clients!");

            return Task.FromResult(false);
        }

        public static Task<List<string>> getCrcList(string targetClientIp, string? AccessToken, bool Retail)
        {
            bool AccessTokenProvided = !string.IsNullOrEmpty(AccessToken);
            List<ClientObject>? clients = null;
            List<string> crcList = new();

            if (AccessTokenProvided)
            {
                ClientObject? client = MediusClass.Manager.GetClientByAccessToken(AccessToken, Retail ? 20374 : 20371);
                if (client != null)
                {
                    clients = new()
                    {
                        client
                    };
                }
            }
            else
                clients = MediusClass.Manager.GetClientsByIp(targetClientIp, Retail ? 20374 : 20371);

            if (clients != null)
            {
                foreach (ClientObject client in clients)
                {
                    if (client.CurrentGame != null && client.CurrentGame.Host != null && !string.IsNullOrEmpty(client.CurrentGame.GameName) && client.CurrentGame.GameName.StartsWith("AP|") && client.CurrentGame.GameName.Split('|').Length >= 5)
                        crcList.Add($"{client.AccountName}|{GetGJSCRC(client.CurrentGame.Host.AccountName!, client.CurrentGame.GameName!.Split('|')[5] + "H3m0", client.CurrentGame.utcTimeCreated)}");
                }

                return Task.FromResult(crcList);
            }

            LoggerAccessor.LogError($"[HomeGuestJoiningSystem] - {(!AccessTokenProvided ? $"Ip:{targetClientIp}" : $"AccessToken:{AccessToken}")} didn't return any Medius clients!");

            return Task.FromResult(crcList);
        }

        public static string GetGJSCRC(string salt1, string salt2, DateTime dateSalt)
        {
            int res1;
            int res2;

            Ionic.Crc.CRC32? crc = new();

            byte[] SaltedDateTimeBytes = Encoding.UTF8.GetBytes("S1l3" + dateSalt.ToString());
            byte[] PassCode = Encoding.UTF8.GetBytes(salt1 + salt2 + "H3m0");

            crc.SlurpBlock(PassCode, 0, PassCode.Length);

            res1 = crc.Crc32Result;

            crc.SlurpBlock(SaltedDateTimeBytes, 0, SaltedDateTimeBytes.Length);

            res2 = crc.Crc32Result;

            return TimeZoneInfo.Local.IsDaylightSavingTime(dateSalt) ? ((res1 ^ dateSalt.Minute).ToString("X8") + (dateSalt.Day ^ dateSalt.DayOfYear ^ res2).ToString("X8"))
                : ((dateSalt.Minute ^ res2).ToString("X8") + (dateSalt.Hour ^ res1 ^ dateSalt.Month).ToString("X8"));
        }
    }
}
