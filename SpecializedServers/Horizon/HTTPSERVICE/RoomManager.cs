using CyberBackendLibrary.Extension;
using Newtonsoft.Json;
using System.Text;

namespace Horizon.HTTPSERVICE
{
    public class RoomManager
    {
        private static (ConcurrentList<KeyValuePair<string, int>>, ConcurrentList<Room>) rooms = (
            new ConcurrentList<KeyValuePair<string, int>>(),
            new ConcurrentList<Room>()
        );

        public static void AddOrUpdateUser(string? accountName, int appid)
        {
            if (!string.IsNullOrEmpty(accountName))
            {
                KeyValuePair<string, int> newUser = new(accountName, appid);

                foreach (KeyValuePair<string, int> User in rooms.Item1.ToList())
                {
                    if (User.Key.Equals(accountName))
                    {
                        rooms.Item1.RemoveAll(keypair => keypair.Key.Equals(accountName));
                        break;
                    }
                }

                rooms.Item1.Add(newUser);
            }
        }

        // Update or Create a Room based on the provided parameters
        public static void UpdateOrCreateRoom(string appId, string? gameName, uint? gameId, string? worldId, string? accountName, int accountDmeId, string? languageType, bool host)
        {
            Room? roomToUpdate = rooms.Item2.FirstOrDefault(r => r.AppId == appId);

            if (roomToUpdate == null)
            {
                roomToUpdate = new Room { AppId = appId, Worlds = new List<World>() };
                rooms.Item2.Add(roomToUpdate);
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
                        Player? playerToUpdatehashed = gameToUpdate.Clients?.FirstOrDefault(p => p.Name == XORString(accountName, HorizonServerConfiguration.MediusAPIKey));
                        if (playerToUpdatehashed == null)
                        {
                            playerToUpdate = new Player { DmeId = accountDmeId, Name = XORString(accountName, HorizonServerConfiguration.MediusAPIKey), Languages = languageType, Host = host };
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

        // Remove a user from a specific room based on the provided parameters
        public static void RemoveUserFromGame(string appId, string gameName, string worldId, string accountName)
        {
            Room? roomToRemoveUser = rooms.Item2.FirstOrDefault(r => r.AppId == appId);

            if (roomToRemoveUser != null)
            {
                World? WorldToRemoveUser = roomToRemoveUser.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                if (WorldToRemoveUser != null)
                {
                    GameList? GameToRemoveUser = WorldToRemoveUser.GameSessions?.FirstOrDefault(w => w.Name == gameName);

                    if (GameToRemoveUser != null && !string.IsNullOrEmpty(GameToRemoveUser.Name))
                    {
                        if (GameToRemoveUser.Name.Contains("AP|"))
                            GameToRemoveUser.Clients?.RemoveAll(p => p.Name == XORString(accountName, HorizonServerConfiguration.MediusAPIKey));
                        else
                            GameToRemoveUser.Clients?.RemoveAll(p => p.Name == accountName);
                    }
                }
            }
        }

        // Remove a world from a specific room based on the provided parameters
        public static void RemoveWorld(string appId, string? worldId)
        {
            Room? roomToRemove = rooms.Item2.FirstOrDefault(r => r.AppId == appId);

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
                Room? roomToRemove = rooms.Item2.FirstOrDefault(r => r.AppId == appId);

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
                Room? roomToRemove = rooms.Item2.FirstOrDefault(r => r.AppId == appId);

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
            lock (rooms.Item2)
                rooms.Item2.RemoveAll(r => r.AppId == appId);
        }

        public static void RemoveUser(string? accountName)
        {
            if (!string.IsNullOrEmpty(accountName))
                rooms.Item1.RemoveAll(keypair => keypair.Key.Equals(accountName));
        }

        // Get a list of all Rooms
        public static List<Room> GetAllRooms()
        {
            return rooms.Item2.ToList();
        }

        public static List<KeyValuePair<string, int>> GetAllUsers()
        {
            return rooms.Item1.ToList();
        }

        // Serialize the RoomConfig to JSON
        public static string ToJson()
        {
            return "{\"usernames\":" + JsonConvert.SerializeObject(GetAllUsers()) + ",\"rooms\":" + JsonConvert.SerializeObject(GetAllRooms()) + "}";
        }

        private static string XORString(string input, string? key)
        {
            if (string.IsNullOrEmpty(key))
                key = "@00000000000!00000000000!";

            StringBuilder result = new();

            for (int i = 0; i < input.Length; i++)
            {
                result.Append((char)(input[i] ^ key[i % key.Length]));
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(result.ToString()));
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
        public uint DmeWorldId { get; set; }
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
