using DatabaseMiddleware.SQLiteEngine;
using Horizon.LIBRARY.Database.Entities;
using Horizon.LIBRARY.Database.Models;

namespace DatabaseMiddleware.Controllers.HorizonDatabase
{
    public class Game
    {
        public MiddlewareSQLiteContext? db { get; set; }

        public Game()
        {
            db = SQLiteConnector.GetConnectionsByDatabaseName("medius_database")?.FirstOrDefault();
        }

        public Task<dynamic?> getGames()
        {
            return Task.FromResult<dynamic?>(db?.Game?.ToList());
        }

        public Task<dynamic?> getGame(int gameId)
        {
            return Task.FromResult<dynamic?>(db?.Game?.Where(g => g.GameId == gameId).Select(g => g).FirstOrDefault());
        }

        public Task<dynamic?> getGameHistory(int appId, int pageIndex, int pageSize)
        {
            var app_id_group = (from a in db?.DimAppIds
                                where a.AppId == appId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db?.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == appId
                                    select a.AppId).ToList();

            var games = db?.GameHistory?.Where(g => app_ids_in_group.Contains(g.AppId));
            var pageCount = games?.Count() / pageSize;

            if (games != null)
            {
                return Task.FromResult<dynamic?>(new
                {
                    Games = games.Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                    PageCount = pageCount
                });
            }

            return Task.FromResult<dynamic?>(null);
        }

        public Task<dynamic?> updateGameHistory(GameHistory game, string DatabaseUsername)
        {
            var existingGame = db?.GameHistory?.Where(g => g.Id == game.Id).Select(g => g).FirstOrDefault();
            if (existingGame != null)
            {
                existingGame.GameId = game.GameId;
                existingGame.AppId = game.AppId;
                existingGame.MinPlayers = game.MinPlayers;
                existingGame.MaxPlayers = game.MaxPlayers;
                existingGame.PlayerCount = game.PlayerCount;
                existingGame.PlayerListCurrent = game.PlayerListCurrent;
                existingGame.PlayerListStart = game.PlayerListStart;
                existingGame.PlayerSkillLevel = game.PlayerSkillLevel;
                existingGame.GameLevel = game.GameLevel;
                existingGame.GameName = game.GameName;
                existingGame.RuleSet = game.RuleSet;
                existingGame.GenericField1 = game.GenericField1;
                existingGame.GenericField2 = game.GenericField2;
                existingGame.GenericField3 = game.GenericField3;
                existingGame.GenericField4 = game.GenericField4;
                existingGame.GenericField5 = game.GenericField5;
                existingGame.GenericField6 = game.GenericField6;
                existingGame.GenericField7 = game.GenericField7;
                existingGame.GenericField8 = game.GenericField8;
                existingGame.WorldStatus = game.WorldStatus;
                existingGame.GameHostType = game.GameHostType;
                existingGame.GameCreateDt = game.GameCreateDt;
                existingGame.GameStartDt = game.GameStartDt;
                existingGame.Metadata = game.Metadata;
                existingGame.DatabaseUser = DatabaseUsername;

                db?.SaveChanges();

                return Task.FromResult<dynamic?>("Game History Updated!");
            }

            return Task.FromResult<dynamic?>(null);
        }

        public Task<dynamic> createGame(GameDTO game, string DatabaseUsername)
        {
            Horizon.LIBRARY.Database.Entities.Game newGame = new()
            {
                GameId = game.GameId,
                AppId = game.AppId,
                MinPlayers = game.MinPlayers,
                MaxPlayers = game.MaxPlayers,
                PlayerCount = game.PlayerCount,
                GameLevel = game.GameLevel,
                PlayerSkillLevel = game.PlayerSkillLevel,
                GameStats = game.GameStats,
                GameName = game.GameName,
                RuleSet = game.RuleSet,
                PlayerListCurrent = game.PlayerListCurrent,
                PlayerListStart = game.PlayerListStart,
                GenericField1 = game.GenericField1,
                GenericField2 = game.GenericField2,
                GenericField3 = game.GenericField3,
                GenericField4 = game.GenericField4,
                GenericField5 = game.GenericField5,
                GenericField6 = game.GenericField6,
                GenericField7 = game.GenericField7,
                GenericField8 = game.GenericField8,
                WorldStatus = game.WorldStatus,
                GameHostType = game.GameHostType,
                Metadata = game.Metadata,
                GameCreateDt = game.GameCreateDt ?? DateTime.UtcNow,
                DatabaseUser = DatabaseUsername
            };
            db?.Game?.Add(newGame);
            db?.SaveChanges();

            return Task.FromResult<dynamic>("Game Created!");
        }

        public async Task<dynamic?> updateGame(int gameId, GameDTO game, string DatabaseUsername)
        {
            var existingGame = db?.Game?.Where(g => g.GameId == gameId).Select(g => g).FirstOrDefault();

            if (existingGame != null)
            {
                // Catalog the historical game
                if (game.Destroyed)
                {
                    GameHistory newHistoricalGame = new GameHistory()
                    {
                        GameId = game.GameId,
                        AppId = game.AppId,
                        MinPlayers = game.MinPlayers,
                        MaxPlayers = game.MaxPlayers,
                        PlayerCount = game.PlayerCount,
                        PlayerListCurrent = game.PlayerListCurrent,
                        PlayerListStart = game.PlayerListStart,
                        GameLevel = game.GameLevel,
                        PlayerSkillLevel = game.PlayerSkillLevel,
                        GameStats = game.GameStats,
                        GameName = game.GameName,
                        RuleSet = game.RuleSet,
                        GenericField1 = game.GenericField1,
                        GenericField2 = game.GenericField2,
                        GenericField3 = game.GenericField3,
                        GenericField4 = game.GenericField4,
                        GenericField5 = game.GenericField5,
                        GenericField6 = game.GenericField6,
                        GenericField7 = game.GenericField7,
                        GenericField8 = game.GenericField8,
                        WorldStatus = game.WorldStatus,
                        GameHostType = game.GameHostType,
                        Metadata = existingGame.Metadata,
                        GameCreateDt = game.GameCreateDt,
                        GameStartDt = game.GameStartDt,
                        GameEndDt = game.GameEndDt,
                        DatabaseUser = DatabaseUsername
                    };
                    db?.GameHistory?.Add(newHistoricalGame);
                    db?.SaveChanges();

                    await deleteGame(existingGame.GameId);

                    return "Game Deleted!";
                }
                else
                {
                    existingGame.GameId = game.GameId;
                    existingGame.AppId = game.AppId;
                    existingGame.MinPlayers = game.MinPlayers;
                    existingGame.MaxPlayers = game.MaxPlayers;
                    existingGame.PlayerCount = game.PlayerCount;
                    existingGame.PlayerListCurrent = game.PlayerListCurrent;
                    existingGame.PlayerListStart = game.PlayerListStart;
                    existingGame.PlayerSkillLevel = game.PlayerSkillLevel;
                    existingGame.GameLevel = game.GameLevel;
                    existingGame.GameName = game.GameName;
                    existingGame.RuleSet = game.RuleSet;
                    existingGame.GenericField1 = game.GenericField1;
                    existingGame.GenericField2 = game.GenericField2;
                    existingGame.GenericField3 = game.GenericField3;
                    existingGame.GenericField4 = game.GenericField4;
                    existingGame.GenericField5 = game.GenericField5;
                    existingGame.GenericField6 = game.GenericField6;
                    existingGame.GenericField7 = game.GenericField7;
                    existingGame.GenericField8 = game.GenericField8;
                    existingGame.WorldStatus = game.WorldStatus;
                    existingGame.GameHostType = game.GameHostType;
                    existingGame.GameCreateDt = game.GameCreateDt;
                    existingGame.GameStartDt = game.GameStartDt;
                    existingGame.DatabaseUser = DatabaseUsername;

                    db?.SaveChanges();

                    return "Game Updated!";
                }
            }

            return null;
        }

        public Task<dynamic?> updateGame(int gameId, string MetaData)
        {
            var existingGame = db?.Game?.Where(g => g.GameId == gameId).Select(g => g).FirstOrDefault();

            if (existingGame != null)
            {
                existingGame.Metadata = MetaData;

                db?.SaveChanges();

                return Task.FromResult<dynamic?>("Game Updated!");
            }

            return Task.FromResult<dynamic?>(null);
        }

        public Task<dynamic> clearGames(string DatabaseUsername)
        {
            var games = db?.Game?.Where(x => x.DatabaseUser == DatabaseUsername).ToList();
            db?.Game?.RemoveRange(games);
            db?.SaveChanges();

            return Task.FromResult<dynamic>("Game Cleared!");
        }

        public Task<dynamic?> deleteGame(int gameId)
        {
            var existingGame = db?.Game?.Where(g => g.GameId == gameId).Select(g => g).FirstOrDefault();

            if (existingGame != null)
            {
                db?.Game?.Remove(existingGame);
                db?.SaveChanges();

                return Task.FromResult<dynamic?>("Game Deleted!");
            }

            return Task.FromResult<dynamic?>(null);
        }
    }
}
