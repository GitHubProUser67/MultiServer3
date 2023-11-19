using CustomLogger;
using Newtonsoft.Json;
using System.Net;

namespace Horizon.MEDIUS
{
    public class CrudRoomManager
    {
        private static List<Room> rooms = new List<Room>();

        public static Task RefreshRooms()
        {
            try
            {
                // URI prefixes are required,
                var prefixes = new List<string>() { "http://*:61920/" };

                // Create a listener.
                HttpListener? listener = new();
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
                    HttpListenerContext ctx = listener.GetContext();

                    LoggerAccessor.LogInfo($"[CrudRoomManager] - Recived request for {ctx.Request.Url}");

                    if (ctx.Request.HttpMethod == "OPTIONS")
                    {
                        ctx.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                        ctx.Response.AddHeader("Access-Control-Allow-Methods", "GET");
                        ctx.Response.AddHeader("Access-Control-Max-Age", "1728000");
                    }

                    ctx.Response.AppendHeader("Access-Control-Allow-Origin", "*");

                    if (ctx.Request.HttpMethod == "GET" && ctx.Request.Url != null)
                    {
                        switch (ctx.Request.Url.AbsolutePath)
                        {
                            case "/GetRooms/":
                                ctx.Response.AddHeader("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                // Construct a response.
                                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(ToJson());

                                if (ctx.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        ctx.Response.ContentLength64 = buffer.Length;
                                        ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                        ctx.Response.OutputStream.Flush();
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
                                    ctx.Response.AddHeader("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                    // Construct a response.
                                    byte[] bufferfavicon = File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico");

                                    if (ctx.Response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            ctx.Response.ContentLength64 = bufferfavicon.Length;
                                            ctx.Response.OutputStream.Write(bufferfavicon, 0, bufferfavicon.Length);
                                            ctx.Response.OutputStream.Flush();
                                        }
                                        catch (Exception)
                                        {
                                            // Not Important;
                                        }
                                    }
                                }
                                else
                                    ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                break;
                            default:
                                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                break;
                        }
                    }
                    else
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                    try
                    {
                        ctx.Response.OutputStream.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                        // outputstream has been disposed already.
                    }
                    ctx.Response.Close();
                }

                listener.Stop();
                listener = null;
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[CrudRoomManager] - Listener thrown an exception : {ex}");
            }

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

            World? worldToUpdate = roomToUpdate.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

            if (worldToUpdate == null)
            {
                worldToUpdate = new World { WorldId = worldId, GameSessions = new List<GameList>() };
                roomToUpdate.Worlds?.Add(worldToUpdate);
            }

            GameList? gameToUpdate = worldToUpdate.GameSessions?.FirstOrDefault(w => w.Name == gameName);

            if (gameToUpdate == null)
            {
                gameToUpdate = new GameList { Name = gameName, Clients = new List<Player>() };
                worldToUpdate.GameSessions?.Add(gameToUpdate);
            }

            Player? playerToUpdate = gameToUpdate.Clients?.FirstOrDefault(p => p.Name == accountName);

            if (playerToUpdate == null && !string.IsNullOrEmpty(gameToUpdate.Name))
            {
                if (gameToUpdate.Name.Contains("AP|"))
                {
                    Player? playerToUpdatehashed = gameToUpdate.Clients?.FirstOrDefault(p => p.Name == Misc.ComputeSaltedSHA256(accountName));
                    if (playerToUpdatehashed == null)
                    {
                        playerToUpdate = new Player { Name = Misc.ComputeSaltedSHA256(accountName), Languages = languageType, Host = host };
                        gameToUpdate.Clients?.Add(playerToUpdate);
                    }
                }
                else
                {
                    playerToUpdate = new Player { Name = accountName, Languages = languageType, Host = host };
                    gameToUpdate.Clients?.Add(playerToUpdate);
                }
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
                var WorldToRemoveUser = roomToRemoveUser.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                if (WorldToRemoveUser != null)
                {
                    var GameToRemoveUser = WorldToRemoveUser.GameSessions?.FirstOrDefault(w => w.Name == gameName);

                    if (GameToRemoveUser != null && !string.IsNullOrEmpty(GameToRemoveUser.Name))
                    {
                        if (GameToRemoveUser.Name.Contains("AP|"))
                            GameToRemoveUser.Clients?.RemoveAll(p => p.Name == Misc.ComputeSaltedSHA256(accountName));
                        else
                            GameToRemoveUser.Clients?.RemoveAll(p => p.Name == accountName);
                    }
                }
            }
        }

        // Remove a world from a specific room based on the provided parameters
        public static void RemoveWorld(string appId, string worldId)
        {
            var roomToRemove = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToRemove != null)
            {
                var gameToRemove = roomToRemove.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                if (gameToRemove != null)
                    roomToRemove.Worlds?.RemoveAll(w => w.WorldId == worldId);
            }
        }

        // Remove a game from a specific room based on the provided parameters
        public static void RemoveGame(string appId, string worldId, string? gameName)
        {
            if (!string.IsNullOrEmpty(gameName))
            {
                var roomToRemove = rooms.FirstOrDefault(r => r.AppId == appId);

                if (roomToRemove != null)
                {
                    var gameToRemove = roomToRemove.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                    if (gameToRemove != null)
                    {
                        var worldToRemove = gameToRemove.GameSessions?.FirstOrDefault(w => w.Name == gameName);

                        if (worldToRemove != null)
                            gameToRemove.GameSessions?.RemoveAll(w => w.Name == gameName);
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
