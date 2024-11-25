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
                        Ionic.Crc.CRC32? crc = new();

                        byte[] APPassCode = Encoding.UTF8.GetBytes(homeLobby.Host.AccountName + homeLobby.GameName!.Split('|')[5] + "H3m0");

                        crc.SlurpBlock(APPassCode, 0, APPassCode.Length);

                        if ($"{crc.Crc32Result:X4}" == SceneCrc)
                        {
                            string SSFWIp = HorizonServerConfiguration.SSFWAddress;
                            if (SSFWIp.Length > 15)
                                SSFWIp = "[" + SSFWIp + "]"; // Format the hostname if it's a IPV6 url format.

                            string ssfwSceneNameResult = HTTPProcessor.RequestURLPOST($"http://{SSFWIp}:{HorizonServerConfiguration.SSFWPort}/WebService/GetSceneLike/", new Dictionary<string, string>() { { "like", LobbyName } }, string.Empty, "text/plain");

                            if (!string.IsNullOrEmpty(ssfwSceneNameResult))
                            {
                                foreach (ClientObject client in clients)
                                {
                                    client.LobbyKeyOverride = SceneCrc;
                                    _ = HomeRTMTools.SendRemoteCommand(client, $"lc Debug.System( 'map {ssfwSceneNameResult}' )");
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
                    {
                        string LobbyName = client.CurrentGame.GameName!.Split('|')[5];
                        Ionic.Crc.CRC32? crc = new();

                        byte[] APPassCode = Encoding.UTF8.GetBytes(client.CurrentGame.Host.AccountName + client.CurrentGame.GameName!.Split('|')[5] + "H3m0");

                        crc.SlurpBlock(APPassCode, 0, APPassCode.Length);

                        crcList.Add($"{client.AccountName}|{crc.Crc32Result:X4}");
                    }
                }

                return Task.FromResult(crcList);
            }

            LoggerAccessor.LogError($"[HomeGuestJoiningSystem] - {(!AccessTokenProvided ? $"Ip:{targetClientIp}" : $"AccessToken:{AccessToken}")} didn't return any Medius clients!");

            return Task.FromResult(crcList);
        }
    }
}
