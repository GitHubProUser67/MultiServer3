using System.Collections.Concurrent;
using System.Data.SQLite;

namespace DatabaseMiddleware.SQLiteEngine
{
    public static class SQLiteConnector
    {
        private static readonly ConcurrentDictionary<Tuple<string, string>, MiddlewareSQLiteContext> Connections = new();

        public static Task AddDatabases(List<string>? DatabasesfilePaths)
        {
            if (DatabasesfilePaths == null)
                return Task.CompletedTask;

            foreach (string parameters in DatabasesfilePaths)
            {
                string[] paramslist = parameters.Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (paramslist.Length == 2)
                {
                    string? directoryPath = Path.GetDirectoryName(paramslist[1]);

                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                        lock (Connections)
                        {
                            if (!Connections.ContainsKey(Tuple.Create(paramslist[0], paramslist[1])))
                                Connections.TryAdd(Tuple.Create(paramslist[0], paramslist[1]), CreateConnection(paramslist[1]));
                        }
                    }
                }
                else
                    CustomLogger.LoggerAccessor.LogError($"[SQLiteConnector] - Parameter string:{parameters} contains invalid entries or is in a wrong format, skipping!");
            }

            return Task.CompletedTask;
        }

        public static IEnumerable<MiddlewareSQLiteContext>? GetConnectionsByDatabaseName(string DbName)
        {
            List<MiddlewareSQLiteContext> connList = new();

            lock (Connections)
            {
                foreach (Tuple<string, string> key in Connections.Keys)
                {
                    if (key.Item1 == DbName)
                        connList.Add(Connections[key]);
                }
            }

            if (connList.Count > 0)
                return connList;

            return null;
        }

        public static IEnumerable<MiddlewareSQLiteContext>? GetAllActiveConnections()
        {
            lock (Connections)
            {
                if (Connections.IsEmpty)
                    return null;
                else
                    return Connections.Values.Where(pair => pair.Database.Connection.State == System.Data.ConnectionState.Open);
            }
        }

        public static Task StartAllDatabases()
        {
            lock (Connections)
            {
                foreach (KeyValuePair<Tuple<string, string>, MiddlewareSQLiteContext> kvp in Connections)
                {
                    try
                    {
                        if (kvp.Value.Database.Connection.State != System.Data.ConnectionState.Open)
                        {
                            // Initialize the database
                            kvp.Value.Database.Initialize(true);

                            // Open the connection
                            kvp.Value.Database.Connection.Open();

                            CustomLogger.LoggerAccessor.LogInfo($"[SQLConnector] - SQL Database identified as {kvp.Key.Item1}:{kvp.Key.Item2} connected successfully!");
                        }
                        else
                            CustomLogger.LoggerAccessor.LogWarn($"[SQLConnector] - SQL Database identified as {kvp.Key.Item1}:{kvp.Key.Item2} was already connected.");
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.LoggerAccessor.LogError($"[SQLConnector] - SQL Database identified as {kvp.Key.Item1}:{kvp.Key.Item2} Failed to connect with exception:{ex}");
                    }
                }
            }

            return Task.CompletedTask;
        }

        public static Task StopAllDatabases()
        {
            lock (Connections)
            {
                foreach (MiddlewareSQLiteContext context in Connections.Values)
                {
                    context.Database.Connection.Close();
                    context.Database.Connection.Dispose();
                    context.Dispose();
                }

                GC.Collect(); // Makes sure to disconnect dead/ghosts sessions.

                Connections.Clear();
            }

            return Task.CompletedTask;
        }

        public static MiddlewareSQLiteContext CreateConnection(string filePath)
        {
            string? directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);

            return new MiddlewareSQLiteContext(new SQLiteConnection($"Data Source={filePath};Version=3;New=True;Compress=True;foreign keys=true;"), true);
        }
    }
}
