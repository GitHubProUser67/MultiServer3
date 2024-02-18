﻿using System.Security;
using System.Text.RegularExpressions;

namespace BackendProject.WebAPIs.OHS
{
    public class OHSClass : IDisposable
    {
        private static string[] Commands = { "/batch/", "/community/getscore/", "/community/updatescore/",
            "/global/set/", "/global/getall/", "/global/get/",
            "/userid/", "/user/getwritekey/", "/user/set/",
            "/user/getall/", "/user/get/", "/usercounter/set/",
            "/usercounter/getall/", "/usercounter/get/", "/userinventory/getglobalitems/",
            "/userinventory/getuserinventory/", "/leaderboard/requestbyusers/", "/leaderboard/requestbyrank/",
            "/leaderboard/update/", "/leaderboard/updatessameentry/", "/statistic/set/"};
        private string absolutepath;
        private string method;
        private int game;
        private bool disposedValue;

        public OHSClass(string method, string absolutepath, int game)
        {
            this.absolutepath = absolutepath;
            this.method = method;
            this.game = game;
        }

        public string? ProcessRequest(byte[] PostData, string? ContentType, string directoryPath)
        {
            if (string.IsNullOrEmpty(absolutepath) || string.IsNullOrEmpty(directoryPath))
                return null;

            string? res = null;

            directoryPath = RemoveCommands(directoryPath);

            Directory.CreateDirectory(directoryPath);

            switch (method)
            {
                case "POST":
                    if (ContentType != null && ContentType.Contains("multipart/form-data"))
                    {
                        if (absolutepath.Contains(Commands[0]))
                            res = Batch.Batch_Process(PostData, ContentType, directoryPath, game);
                        else if (absolutepath.Contains(Commands[1]))
                            res = Community.Community_Getscore(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[2]))
                            res = Community.Community_UpdateScore(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[3]))
                            res = User.Set(PostData, ContentType, directoryPath, string.Empty, true, game);
                        else if (absolutepath.Contains(Commands[4]))
                            res = User.Get_All(PostData, ContentType, directoryPath, string.Empty, true, game);
                        else if (absolutepath.Contains(Commands[5]))
                            res = User.Get(PostData, ContentType, directoryPath, string.Empty, true, game);
                        else if (absolutepath.Contains(Commands[6]))
                            res = User.User_Id(PostData, ContentType, string.Empty, game);
                        else if (absolutepath.Contains(Commands[7]))
                            res = User.User_GetWritekey(PostData, ContentType, string.Empty, game);
                        else if (absolutepath.Contains(Commands[8]))
                            res = User.Set(PostData, ContentType, directoryPath, string.Empty, false, game);
                        else if (absolutepath.Contains(Commands[9]))
                            res = User.Get_All(PostData, ContentType, directoryPath, string.Empty, false, game);
                        else if (absolutepath.Contains(Commands[10]))
                            res = User.Get(PostData, ContentType, directoryPath, string.Empty, false, game);
                        else if (absolutepath.Contains(Commands[11]))
                            res = UserCounter.Set(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[12]))
                            res = UserCounter.Get_All(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[13]))
                            res = UserCounter.Get(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[14]))
                            res = UserInventory.GetGlobalItems(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[15]))
                            res = UserInventory.GetUserInventory(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[16]))
                            res = Leaderboard.Leaderboard_RequestByUsers(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[17]))
                            res = Leaderboard.Leaderboard_RequestByRank(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[18]))
                            res = Leaderboard.Leaderboard_Update(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[19]))
                            res = Leaderboard.Leaderboard_UpdatesSameEntry(PostData, ContentType, directoryPath, string.Empty, game);
                        else if (absolutepath.Contains(Commands[20]))
                            res = Statistic.Set(PostData, ContentType);
                    }
                    break;
                default:
                    break;
            }

            return SecurityElement.Escape(res);
        }

        private static string RemoveCommands(string input)
        {
            string modifiedInput = input;

            foreach (string pattern in Commands)
            {
                string patternEscaped = Regex.Escape(pattern);
                modifiedInput = Regex.Replace(modifiedInput, patternEscaped, string.Empty);
            }

            return modifiedInput;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    absolutepath = string.Empty;
                    method = string.Empty;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~OHSClass()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
