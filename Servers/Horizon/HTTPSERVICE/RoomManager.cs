using Horizon.SERVER;
using NetworkLibrary.Extension;
using WebAPIService.WebCrypto;
using Newtonsoft.Json;

namespace Horizon.HTTPSERVICE
{
    public static class RoomManager
    {
        private static readonly byte[] RandSecSaltKey = ByteUtils.GenerateRandomBytes((ushort)NetObfuscator.SecSalt.Length);

        private static readonly ConcurrentList<Room> rooms = new ConcurrentList<Room>();

        private static object _Lock = new object();

        // Update or Create a Room based on the provided parameters
        public static void UpdateOrCreateRoom(string appId, string? gameName, int? gameId, string? worldId, string? accountName, int accountDmeId, string? languageType, bool host)
        {
            lock (_Lock)
            {
                Room? roomToUpdate = rooms.FirstOrDefault(r => r.AppId == appId);

                if (roomToUpdate == null)
                {
                    roomToUpdate = new Room { AppId = appId, Worlds = new List<World>() };
                    rooms.Add(roomToUpdate);
                }

                if (worldId != null)
                {
                    World? worldToUpdate = roomToUpdate.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                    if (worldToUpdate == null && !string.IsNullOrEmpty(worldId))
                    {
                        worldToUpdate = new World { WorldId = worldId, GameSessions = new List<GameList>() };
                        roomToUpdate.Worlds?.Add(worldToUpdate);
                    }

                    GameList? gameToUpdate = worldToUpdate?.GameSessions?.FirstOrDefault(w => w.Name == gameName);

                    if (gameToUpdate == null && !string.IsNullOrEmpty(gameName) && gameId.HasValue)
                    {
                        gameToUpdate = new GameList { DmeWorldId = gameId.Value, Name = gameName, CreationDate = DateTime.Now.ToUniversalTime(), Clients = new List<Player>() };
                        worldToUpdate?.GameSessions?.Add(gameToUpdate);
                    }

                    Player? playerToUpdate = gameToUpdate?.Clients?.FirstOrDefault(p => p.Name == accountName);

                    if (playerToUpdate == null && !string.IsNullOrEmpty(gameToUpdate?.Name) && !string.IsNullOrEmpty(accountName) && !string.IsNullOrEmpty(languageType))
                    {
                        if (gameToUpdate.Name.Contains("AP|"))
                        {
                            Player? playerToUpdatehashed = gameToUpdate.Clients?.FirstOrDefault(p => p.Name == CipherString(accountName, HorizonServerConfiguration.MediusAPIKey));
                            if (playerToUpdatehashed == null)
                            {
                                playerToUpdate = new Player { DmeId = accountDmeId, Name = CipherString(accountName, HorizonServerConfiguration.MediusAPIKey), Languages = languageType, Host = host };
                                gameToUpdate.Clients?.Add(playerToUpdate);
                            }
                        }
                        else
                        {
                            playerToUpdate = new Player { DmeId = accountDmeId, Name = accountName, Languages = languageType, Host = host };
                            gameToUpdate.Clients?.Add(playerToUpdate);
                        }
                    }
                    else if (playerToUpdate != null)
                    {
                        playerToUpdate.Host = host;
                        playerToUpdate.Languages = languageType;
                    }
                }
            }
        }

        // Remove a user from a specific room based on the provided parameters
        public static void RemoveUserFromGame(string appId, string gameName, string worldId, string accountName)
        {
            Room? roomToRemoveUser = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToRemoveUser != null)
            {
                World? WorldToRemoveUser = roomToRemoveUser.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                if (WorldToRemoveUser != null)
                {
                    GameList? GameToRemoveUser = WorldToRemoveUser.GameSessions?.FirstOrDefault(w => w.Name == gameName);

                    if (GameToRemoveUser != null && !string.IsNullOrEmpty(GameToRemoveUser.Name))
                    {
                        if (GameToRemoveUser.Name.Contains("AP|"))
                            GameToRemoveUser.Clients?.RemoveAll(p => p.Name == CipherString(accountName, HorizonServerConfiguration.MediusAPIKey));
                        else
                            GameToRemoveUser.Clients?.RemoveAll(p => p.Name == accountName);
                    }
                }
            }
        }

        // Remove a world from a specific room based on the provided parameters
        public static void RemoveWorld(string appId, string? worldId)
        {
            Room? roomToRemove = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToRemove != null)
            {
                World? worldToRemove = roomToRemove.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                if (worldToRemove != null)
                    roomToRemove.Worlds?.RemoveAll(w => w.WorldId == worldId);
            }
        }

        // Remove a game from a specific room based on the provided parameters
        public static void RemoveGame(string appId, string? worldId, string? gameName)
        {
            if (!string.IsNullOrEmpty(gameName))
            {
                Room? roomToRemove = rooms.FirstOrDefault(r => r.AppId == appId);

                if (roomToRemove != null)
                {
                    World? worldToRemove = roomToRemove.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                    if (worldToRemove != null)
                    {
                        GameList? gameToRemove = worldToRemove.GameSessions?.FirstOrDefault(w => w.Name == gameName);

                        if (gameToRemove != null)
                            worldToRemove.GameSessions?.RemoveAll(w => w.Name == gameName);
                    }
                }
            }
        }

        public static void UpdateGameName(string appId, string? worldId, string? previousGameName, string? gameName)
        {
            if (!string.IsNullOrEmpty(previousGameName) && !string.IsNullOrEmpty(gameName))
            {
                Room? roomToRemove = rooms.FirstOrDefault(r => r.AppId == appId);

                if (roomToRemove != null)
                {
                    World? worldToRemove = roomToRemove.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                    if (worldToRemove != null)
                    {
                        GameList? gameToUpdate = worldToRemove.GameSessions?.FirstOrDefault(w => w.Name == previousGameName);

                        if (gameToUpdate != null)
                            gameToUpdate.Name = gameName;
                    }
                }
            }
        }

        // Remove a Room by AppId
        public static void RemoveRoom(string appId)
        {
            rooms.RemoveAll(r => r.AppId == appId);
        }

        // Get a list of all Rooms
        public static List<Room> GetAllRooms()
        {
            return rooms.ToList();
        }

        public static List<KeyValuePair<string, int>> GetAllLoggedInUsers()
        {
            List<KeyValuePair<string, int>> usersList = new();

            foreach (var user in MediusClass.Manager.GetClients(0))
            {
                if (user.IsLoggedIn && !string.IsNullOrEmpty(user.AccountName))
                    usersList.Add(new KeyValuePair<string, int>(user.AccountName, user.ApplicationId));
            }

            return usersList;
        }

        // Serialize the RoomConfig to JSON
        public static string ToJson()
        {
            return "{\"usernames\":" + JsonConvert.SerializeObject(GetAllLoggedInUsers()) + ",\"rooms\":" + JsonConvert.SerializeObject(GetAllRooms()) + "}";
        }

        private static string CipherString(string input, string key)
        {
            int i;
            byte[] secSalt = new byte[RandSecSaltKey.Length];

            for (i = 0; i < RandSecSaltKey.Length; i++)
            {
                if (i == 0)
                    secSalt[i] = (byte)(NetObfuscator.SecSalt[i] ^ RandSecSaltKey[i] ^ (i * 2));
                else
                    secSalt[i] = (byte)(NetObfuscator.SecSalt[i] ^ RandSecSaltKey[i] ^ secSalt[i - 1]);
            }

            return $"<Secure RNG=\"{BitConverter.ToString(secSalt).Replace("-", string.Empty)}\">" + NetObfuscator.Encrypt(WebCryptoClass.EncryptCBC(input, key, WebCryptoClass.IdentIV), secSalt, (byte)key.Aggregate(0, (current, c) => current ^ c)) + "</Secure>";
        }
    }

    public class Room
    {
        public string? AppId { get; set; }
        public List<World>? Worlds { get; set; }
    }

    public class World
    {
        public string? WorldId { get; set; }
        public List<GameList>? GameSessions { get; set; }
    }

    public class GameList
    {
        public int DmeWorldId { get; set; }
        public string? Name { get; set; }
        public DateTime CreationDate { get; set; }
        public List<Player>? Clients { get; set; }
    }

    public class Player
    {
        public int DmeId { get; set; }
        public bool Host { get; set; }
        public string? Name { get; set; }
        public string? Languages { get; set; }
    }
}
