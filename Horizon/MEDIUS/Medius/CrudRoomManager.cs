using CustomLogger;
using Newtonsoft.Json;
using System.Net;

namespace Horizon.MEDIUS.Medius
{
    public class CrudRoomManager
    {
        private static List<Room> rooms = new List<Room>();

        public static Task RefreshRooms()
        {
            // URI prefixes are required,
            var prefixes = new List<string>() { "http://*:61920/" };

            // Create a listener.
            HttpListener listener = new();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();
            LoggerAccessor.LogInfo("[CrudRoomManager] - Listening on port 61920...");
            while (MediusClass.started)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = listener.GetContext();

                LoggerAccessor.LogInfo($"[CrudRoomManager] - Recived request for {context.Request.Url}");

                if (context.Request.HttpMethod == "GET" && context.Request.Url != null)
                {
                    switch (context.Request.Url.AbsolutePath)
                    {
                        case "/GetRooms/":
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            // Construct a response.
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(ToJson());

                            if (context.Response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    context.Response.ContentLength64 = buffer.Length;
                                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                    context.Response.OutputStream.Close();
                                }
                                catch (Exception)
                                {
                                    // Not Important;
                                }
                            }
                            break;
                        case "/favicon.ico":
                            if (File.Exists(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico"))
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.OK;
                                // Construct a response.
                                byte[] bufferfavicon = File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico");

                                if (context.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        context.Response.ContentLength64 = bufferfavicon.Length;
                                        context.Response.OutputStream.Write(bufferfavicon, 0, bufferfavicon.Length);
                                        context.Response.OutputStream.Close();
                                    }
                                    catch (Exception)
                                    {
                                        // Not Important;
                                    }
                                }
                            }
                            else
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            break;
                        default:
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            break;
                    }
                }
                else
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                context.Response.Close();
            }

            listener.Stop();

            return Task.CompletedTask;
        }

        // Update or Create a Room based on the provided parameters
        public static void UpdateOrCreateRoom(string appId, string gameName, string worldId, string accountName, string languageType, bool host)
        {
            Room? roomToUpdate = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToUpdate == null)
            {
                roomToUpdate = new Room { AppId = appId, Worlds = new List<World>() };
                rooms.Add(roomToUpdate);
            }

            World? worldToUpdate = roomToUpdate.Worlds.FirstOrDefault(w => w.WorldId == worldId);

            if (worldToUpdate == null)
            {
                worldToUpdate = new World { WorldId = worldId, GameSessions = new List<GameList>() };
                roomToUpdate.Worlds.Add(worldToUpdate);
            }

            GameList? gameToUpdate = worldToUpdate.GameSessions.FirstOrDefault(w => w.Name == gameName);

            if (gameToUpdate == null)
            {
                gameToUpdate = new GameList { Name = gameName, Clients = new List<Player>() };
                worldToUpdate.GameSessions.Add(gameToUpdate);
            }

            Player? playerToUpdate = gameToUpdate.Clients.FirstOrDefault(p => p.Name == accountName);

            if (playerToUpdate == null)
            {
                playerToUpdate = new Player { Name = accountName, Languages = languageType, Host = host };
                gameToUpdate.Clients.Add(playerToUpdate);
            }
            else
            {
                playerToUpdate.Host = host;
                playerToUpdate.Languages = languageType;
            }
        }

        // Remove a user from a specific room based on the provided parameters
        public static void RemoveUser(string appId, string gameName, string worldId, string accountName)
        {
            var roomToRemoveUser = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToRemoveUser != null)
            {
                var gameToRemoveUser = roomToRemoveUser.Worlds.FirstOrDefault(w => w.WorldId == worldId);

                if (gameToRemoveUser != null)
                {
                    var worldToRemoveUser = gameToRemoveUser.GameSessions.FirstOrDefault(w => w.Name == gameName);

                    if (worldToRemoveUser != null)
                        worldToRemoveUser.Clients.RemoveAll(p => p.Name == accountName);
                }
            }
        }

        // Remove a world from a specific room based on the provided parameters
        public static void RemoveWorld(string appId, string worldId)
        {
            var roomToRemoveUser = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToRemoveUser != null)
            {
                var gameToRemoveUser = roomToRemoveUser.Worlds.FirstOrDefault(w => w.WorldId == worldId);

                if (gameToRemoveUser != null)
                    roomToRemoveUser.Worlds.RemoveAll(w => w.WorldId == worldId);
            }
        }

        // Remove a game from a specific room based on the provided parameters
        public static void RemoveGame(string appId, string worldId, string gameName)
        {
            var roomToRemoveUser = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToRemoveUser != null)
            {
                var gameToRemoveUser = roomToRemoveUser.Worlds.FirstOrDefault(w => w.WorldId == worldId);

                if (gameToRemoveUser != null)
                {
                    var worldToRemoveUser = gameToRemoveUser.GameSessions.FirstOrDefault(w => w.Name == gameName);

                    if (worldToRemoveUser != null)
                        gameToRemoveUser.GameSessions.RemoveAll(w => w.Name == gameName);
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
            return rooms;
        }

        // Serialize the RoomConfig to JSON
        public static string ToJson()
        {
            return JsonConvert.SerializeObject(rooms, Formatting.Indented);
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
        public string? Name { get; set; }
        public List<Player>? Clients { get; set; }
    }

    public class Player
    {
        public bool Host { get; set; }
        public string? Name { get; set; }
        public string? Languages { get; set; }
    }
}
