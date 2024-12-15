﻿using CustomLogger;
using Horizon.MUM.Models;
using NetworkLibrary.Extension;
using NetworkLibrary.HTTP;
using System.Security.Cryptography;
using System.Text;

namespace Horizon.SERVER.Extension.PlayStationHome
{
    public class HomeGuestJoiningSystem
    {
        private static readonly byte[] RandCRCKey = ByteUtils.GenerateRandomBytes(24);
        private static readonly byte[] RandCRCIV = ByteUtils.GenerateRandomBytes(8);

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
                                    if (client.CurrentGame == homeLobby)
                                        continue;

                                    client.LobbyKeyOverride = SceneCrc;
                                    if (!string.IsNullOrEmpty(client.ClientHomeData?.Type) && (client.ClientHomeData.Type.Contains("HDK") || client.ClientHomeData.Type == "Online Debug"))
                                        _ = HomeRTMTools.SendRemoteCommand(client, $"lc Debug.System( 'map {ssfwSceneNameResult}' )");
                                    else
                                        _ = HomeRTMTools.SendRemoteCommand(client, $"map {ssfwSceneNameResult}");
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

        public static Task<List<string>> getCrcList(string targetClientIp, string? AccessToken, bool Retail, bool AllClients)
        {
            bool AccessTokenProvided = !string.IsNullOrEmpty(AccessToken);
            List<ClientObject>? clients = null;
            List<string> crcList = new();

            if (AllClients)
                clients = MediusClass.Manager.GetClients(Retail ? 20374 : 20371);
            else if (AccessTokenProvided)
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
            TripleDES des = TripleDES.Create();

            des.Mode = CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;
            des.Key = RandCRCKey;
            des.IV = RandCRCIV;

            ICryptoTransform cryptoTransform = des.CreateEncryptor();

            byte[] SaltedDateTimeBytes = Encoding.UTF8.GetBytes("S1l3" + dateSalt.ToString());
            byte[] PassCode = Encoding.UTF8.GetBytes(salt1 + salt2 + "H3m0");

            crc.SlurpBlock(cryptoTransform.TransformFinalBlock(PassCode, 0, PassCode.Length), 0, PassCode.Length);

            res1 = crc.Crc32Result;

            crc.SlurpBlock(cryptoTransform.TransformFinalBlock(SaltedDateTimeBytes, 0, SaltedDateTimeBytes.Length), 0, SaltedDateTimeBytes.Length);
			
            des.Dispose();

            res2 = crc.Crc32Result;

            return TimeZoneInfo.Local.IsDaylightSavingTime(dateSalt) ? ((res1 ^ dateSalt.Minute).ToString("X8") + (dateSalt.Day ^ dateSalt.DayOfYear ^ res2).ToString("X8"))
                : ((dateSalt.Minute ^ res2).ToString("X8") + (dateSalt.Hour ^ res1 ^ dateSalt.Month).ToString("X8"));
        }
    }
}