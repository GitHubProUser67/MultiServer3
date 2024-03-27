using DatabaseMiddleware.SQLiteEngine;
using Horizon.LIBRARY.Database.Entities;
using Horizon.LIBRARY.Database.Models;

namespace DatabaseMiddleware.Controllers.Horizon
{
    public class Logs
    {
        public MiddlewareSQLiteContext? db { get; set; }

        public Logs()
        {
            db = SQLiteConnector.GetConnectionsByDatabaseName("medius_database")?.FirstOrDefault();
        }

        public Task<dynamic> submitLog(LogDTO LogData)
        {
            db?.ServerLog?.Add(new ServerLog()
            {
                LogDt = LogData.Timestamp,
                AccountId = LogData.AccountId,
                MethodName = LogData.MethodName,
                LogTitle = LogData.LogTitle,
                LogMsg = LogData.LogMsg,
                LogStacktrace = LogData.LogStacktrace,
                Payload = LogData.Payload
            });

            return Task.FromResult<dynamic>("Log Saved!");
        }
    }
}
