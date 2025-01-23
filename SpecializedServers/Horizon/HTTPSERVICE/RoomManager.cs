using Horizon.SERVER;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NetworkLibrary.Crypto;
using NetworkLibrary.Extension.Csharp;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Horizon.HTTPSERVICE
{
    public class RoomManager
    {
        private const string GenerateRoomsIV = "mvIkdXFdieqjwIEmsNisg..Tuu|:nhVd2wwNEx\\78oS<q{Zxmo8GKLTuhoHlxiju" +
            "WH.qoi2Oq;m]8;3vgFZDGfDdt:uIk}HEf}uT2z}tY{miqr6S5NQPLyYlf9PHTNNuVD.T3Y]4yxQz{qoPoQVFpWhquFR;y|2xiQ{n" +
            "PWQW45dfXMX<<\\PiuXgiT2t48gpttN<xRmWt<<y<}v}m;e:FKRWEdXgsnqVk5N{kWQpk3[}qrfRvsMGxpROwfgqSRi{x:8\\Pn]" +
            "QmSk|HN6sULuq7N3{Sn\\x349OZE\\}XOG;UYzqDUNx9vOIEE\\X992oN3HvKpQ9}:P|iMWZ}jH9j2GJKx5;rxxgLfNejdOQz[Ny" +
            "Yjsnmo]jkte;y:q}f]GH}X<gpqIFYSiZGK94}kT3F<.Gqnudfemst:x:P2ynE.xp:lE:n3q8yesg<xhF3do6Ntxo6:8FX{xxT8Mu" +
            "l:[K]}}q;ex.ZGMJWhVzhe{4tdVNK2UP]gMZv.jVk4eOG3Fz8M[D4zFltyv[7QDJFQ2urFhxDFM[svRqNvWfPzq4yrzUDrtwLG8G" +
            "UYqZn6X}YjH|X4LN]L8Z\\Ldp2O.8;6K7qj3Wfivo}s|WvWGht]I\\]5:GLEOx{34DjPzRQEiqKDyxsx8je8RjQi\\JRhddVuZ{P" +
            "}<uW\\TF]6ddwoSuDn.|E3wH7:K3JzWJxrp\\.KLTuhoHlxijuWH.qoi2OxgLIpR75UFVDGfDdt:uIk}HEf}ut.Y}xWUtd48uq5\\" +
            "r9QSQdWe:<E:trJ||X3Ug4uywzoK{LoPRGpUNXpG96qGSviRT3FF4Z45dfXMX<<\\PiuXgiKTDF9.d3.M4.PENd2jR.{;mNEPNkF8" +
            "Kw\\[f6mj9z3t{2WMxyDJuHEQ5oqtGQzjevjgOSUiY:{tnyp\\gVNnYt\\5UjGf6vR5MlvsXF.N:ivPSqSKTojD6DJtx9{\\f{D[P" +
            "99ypDDnrK5x6E6hFsQmF;iIp72YVpt|\\Stx8igN:Xf9<<e.eQksnmo]jkte;y:q}f]GH}XzIf}IJYSiZGK94K[{3D6xfFuMl\\hF" +
            "g{8.DI;uuL4RJ:pD6i76sPSNM8xgxKerOWw8ok:7|9sNhL3P[V:KK]}}q;ex.ZGMJqgXPfkFw9XW:E;W8l]txw:2GN7yRQ:HIKMHr" +
            "}t{xqq:tNI2JFHgWsFwIpJL6slv;IllDZ;J\\iyxsEx;M}FsqjWqV}.4KGdY5K3\\d2FNFPROWWGiUj:oHLOpdqiP8Qnm6xG{RY]I" +
            "]F.j@@";

        private static byte[]? RoomsIV = null;

        private static readonly ConcurrentList<Room> rooms = new ConcurrentList<Room>();

        // Update or Create a Room based on the provided parameters
        public static void UpdateOrCreateRoom(string appId, string? gameName, int? gameId, string? worldId, string? accountName, int accountDmeId, string? languageType, bool host)
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
        public static IEnumerable<Room> GetAllRooms(bool extraDetails)
        {
            if (extraDetails)
                return rooms;

            return rooms.AsParallel().Select(AnonymizeRoom);
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
        public static string ToJson(bool extraDetails)
        {
            return "{\"usernames\":" + JsonConvert.SerializeObject(GetAllLoggedInUsers()) + ",\"rooms\":" + JsonConvert.SerializeObject(GetAllRooms(extraDetails)) + "}";
        }

        private static string CipherString(string input, string key)
        {
            byte[] secSalt = NetObfuscator.SecSalt;

            RoomsIV ??= ConstructIV();

            return $"<Secure RNG=\"{BitConverter.ToString(secSalt).Replace("-", string.Empty)}\">" + NetObfuscator.Encrypt(WebCrypto.EncryptCBC(input, key, RoomsIV), secSalt, (byte)key.Aggregate(0, (current, c) => current ^ c)) + "</Secure>";
        }

        #region Anonymizer
        private static Room AnonymizeRoom(Room room)
        {
            return new Room
            {
                AppId = room.AppId,
                Worlds = room.Worlds?.Select(AnonymizeWorld).ToList()
            };
        }

        private static World AnonymizeWorld(World world)
        {
            return new World
            {
                WorldId = world.WorldId,
                GameSessions = world.GameSessions?.Select(AnonymizeGameList).ToList()
            };
        }

        private static GameList AnonymizeGameList(GameList game)
        {
            return new GameList
            {
                DmeWorldId = -1,
                Name = game.Name,
                CreationDate = game.CreationDate,
                Clients = game.Clients?.Select(AnonymizePlayer).ToList()
            };
        }

        private static Player AnonymizePlayer(Player player)
        {
            return new Player
            {
                DmeId = -1,
                Host = player.Host,
                Name = player.Name,
                Languages = player.Languages
            };
        }
        #endregion

        private static byte[] ConstructIV()
        {
            using MemoryStream ms = new MemoryStream();

            if (!CSharpCompilation.Create(
                "DynamicAssembly",
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(Encoding.UTF8.GetString(Convert.FromBase64String(NetObfuscator.Decrypt(GenerateRoomsIV, NetObfuscator.GenSalt, 0xAA)))) },
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            ).Emit(ms).Success)
                return RandomNumberGenerator.GetBytes(16);

            ms.Seek(0, SeekOrigin.Begin);

            return Assembly.Load(ms.ToArray()).GetType(Encoding.UTF8.GetString(Convert.FromBase64String(NetObfuscator.Decrypt("KEpd769y\\6dQ97d2HrmJRLttUDUxEo3QUH:lVt{uuVSv[FRW[o8IDD@@", NetObfuscator.GenSalt, 0xAA))))?
                .GetMethod(Encoding.UTF8.GetString(Convert.FromBase64String(NetObfuscator.Decrypt("iWi:U:<onVOrijt;ZY]x9T@@", NetObfuscator.GenSalt, 0xAA))))?.Invoke(null, null) as byte[] ?? RandomNumberGenerator.GetBytes(16);
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
