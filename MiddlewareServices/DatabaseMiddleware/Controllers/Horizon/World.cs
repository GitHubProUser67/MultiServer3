using DatabaseMiddleware.SQLiteEngine;
using System.Runtime.InteropServices;

namespace DatabaseMiddleware.Controllers.Horizon
{
    public class World
    {
        public MiddlewareSQLiteContext? db { get; set; }

        public World()
        {
            db = SQLiteConnector.GetConnectionsByDatabaseName("medius_database")?.FirstOrDefault();
        }

        public Task<dynamic?> getChannels()
        {
            return Task.FromResult<dynamic?>(db?.Channels?.ToList());
        }

        public Task<dynamic?> getLocations()
        {
            return Task.FromResult<dynamic?>(db?.Locations?.ToList());
        }

        public Task<dynamic?> getLocations(int appId)
        {
            int? app_id_group = (from a in db?.DimAppIds
                                where a.AppId == appId
                                select a.GroupId).FirstOrDefault();

            List<int> app_ids_in_group = (from a in db?.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == appId
                                    select a.AppId).ToList();

            return Task.FromResult<dynamic?>(db?.Locations?.Where(x => app_ids_in_group.Contains(x.AppId)).ToList().GroupBy(x => x.Id).Select(x => x.FirstOrDefault()));
        }
    }
}
