using DatabaseMiddleware.SQLiteEngine;
using Horizon.LIBRARY.Database.Entities;
using Horizon.LIBRARY.Database.Models;
using System.Data.Entity;
using System.Runtime.InteropServices;
using AppIdDTO = Horizon.LIBRARY.Database.Models.AppIdDTO;

namespace DatabaseMiddleware.Controllers.Horizon
{
    public class Keys
    {
        public MiddlewareSQLiteContext? db { get; set; }

        public Keys()
        {
            db = SQLiteConnector.GetConnectionsByDatabaseName("medius_database")?.FirstOrDefault();
        }

        public Task<List<AppIdDTO>> getAppIds()
        {
            List<DimAppIds>? app_ids = null;
            List<DimAppGroups>? app_groups = null;
            List<AppIdDTO> results = new();

            app_ids = (from app_id in db?.DimAppIds
                            select app_id).ToList();
            app_groups = (from app_group in db?.DimAppGroups
                               select app_group).ToList();

            var groupings = app_ids.GroupBy(x => x.GroupId);
            foreach (var grouping in groupings)
            {
                var group = app_groups.FirstOrDefault(x => x.GroupId == grouping.Key);

                if (group == null)
                    results.AddRange(grouping.Select(x => new AppIdDTO() { Name = x.AppName, AppIds = new List<int>() { x.AppId } }));
                else
                    results.Add(new AppIdDTO() { Name = group.GroupName, AppIds = grouping.Select(x => x.AppId).ToList() });
            }

            return Task.FromResult(results);
        }

        public async Task<Dictionary<string, string>> getSettings(int appId)
        {
            return await (from s in db?.ServerSettings
                          where s.AppId == appId
                          select new { s.Name, s.Value }).ToDictionaryAsync(x => x.Name, x => x.Value);
        }

        public void setSettings(int appId, Dictionary<string, string> settings)
        {
            foreach (var setting in settings)
            {
                var existingSetting = db?.ServerSettings?.Find(appId, setting.Key);
                if (existingSetting == null)
                {
                    existingSetting = new ServerSetting() { AppId = appId, Name = setting.Key, Value = setting.Value };
                    db?.ServerSettings?.Add(existingSetting);
                }
                else
                    existingSetting.Value = setting.Value;
            }

            db?.SaveChanges();
        }

        public async Task<dynamic?> getEULA(int policyType, int appId, int? eulaId, DateTime? fromDt, DateTime? toDt)
        {
            dynamic? eula = null;
            DateTime now = DateTime.UtcNow;

            if (policyType == 0)
            {
                eula = db?.DimEula?.Where(x => x.AppId == appId
                && x.PolicyType == 0)
                    .FirstOrDefault();
            }
            else if (policyType == 1)
            {
                eula = db?.DimEula?.Where(x => x.AppId == appId
                && x.PolicyType == 1)
                    .FirstOrDefault();
            }
            else if (eulaId != null)
            {
                eula = (from e in db?.DimEula
                        where e.Id == eulaId
                        select e).FirstOrDefault();
            }
            else if (fromDt != null && toDt != null)
            {
                eula = (from e in db?.DimEula
                        where e.FromDt <= fromDt
                        && e.AppId == appId
                        && (e.ToDt == null || e.ToDt >= toDt)
                        select e).FirstOrDefault();
            }
            else if (fromDt != null && toDt == null)
            {
                eula = (from e in db?.DimEula
                        where e.FromDt <= fromDt
                        && e.AppId == appId
                        && (e.ToDt == null || e.ToDt >= now)
                        select e).FirstOrDefault();
            }
            else
                return null;

            return eula;
        }

        public async Task<dynamic?> deleteEULA(int id)
        {
            var eula = db?.DimEula?.FirstOrDefault(x => x.Id == id);
            if (eula == null)
                return null;

            db?.DimEula?.Remove(eula);
            db?.SaveChanges();

            return "EULA Deleted";
        }

        public async Task<dynamic?> updateEULA(ChangeEulaDTO request)
        {
            var eula = db?.DimEula?.FirstOrDefault(x => x.Id == request.Id);
            if (eula == null)
                return null;

            db?.DimEula?.Attach(eula);
            db.Entry(eula).State = EntityState.Modified;

            eula.EulaTitle = request.EulaTitle ?? eula.EulaTitle;
            eula.EulaBody = request.EulaBody ?? eula.EulaBody;
            eula.ModifiedDt = DateTime.UtcNow;
            eula.FromDt = request.FromDt ?? eula.FromDt;
            eula.ToDt = request.ToDt ?? eula.ToDt;
            eula.AppId = request.AppId;

            db?.SaveChanges();

            return "EULA Changed";
        }

        public async Task<dynamic> postEULA(AddEulaDTO request)
        {
            var eula = new DimEula()
            {
                EulaTitle = request.EulaTitle,
                EulaBody = request.EulaBody,
                FromDt = request.FromDt ?? DateTime.UtcNow,
                ToDt = request.ToDt,
                CreateDt = DateTime.UtcNow,
                AppId = request.AppId,
            };

            db?.DimEula?.Add(eula);
            db?.SaveChanges();

            return "EULA Added";
        }

        public async Task<dynamic?> getAnnouncements(int? accouncementId, int? appId, DateTime? fromDt, DateTime? toDt, int AppId)
        {
            dynamic? announcement = null;
            DateTime now = DateTime.UtcNow;
            if (accouncementId != null)
            {
                announcement = (from a in db?.DimAnnouncements
                                where a.Id == accouncementId
                                select a).FirstOrDefault();
            }
            else if (fromDt != null && toDt != null && appId != null)
            {
                announcement = (from a in db?.DimAnnouncements
                                where a.AppId == AppId && a.FromDt <= fromDt
                        && (a.ToDt == null || a.ToDt >= toDt)
                                select a).FirstOrDefault();
            }
            else if (fromDt != null && toDt == null)
            {
                announcement = (from a in db?.DimAnnouncements
                                where a.AppId == AppId && a.FromDt <= fromDt
                        && (a.ToDt == null || a.ToDt >= now)
                                select a).FirstOrDefault();
            }
            else
                return null;

            return announcement;
        }

        public async Task<dynamic> getAnnouncementsList(int AppId, DateTime? Dt, int TakeSize = 10)
        {
            dynamic? announcements = null;
            if (Dt == null)
                Dt = DateTime.UtcNow;
            DateTime now = DateTime.UtcNow;
            announcements = (from a in db?.DimAnnouncements
                             orderby a.FromDt descending
                             where a.AppId == AppId && a.FromDt <= Dt
                     && (a.ToDt == null || a.ToDt >= Dt)
                             select a).Take(TakeSize).ToList();

            return announcements;
        }

        public async Task<dynamic?> deleteAnnouncement(int id)
        {
            var announcement = db?.DimAnnouncements?.FirstOrDefault(x => x.Id == id);
            if (announcement == null)
                return null;

            db?.DimAnnouncements?.Remove(announcement);
            db?.SaveChanges();

            return "Announcement Deleted";
        }

        public async Task<dynamic?> updateAnnouncement(ChangeAnnouncementDTO request)
        {
            var announcement = db?.DimAnnouncements?.FirstOrDefault(x => x.Id == request.Id);
            if (announcement == null)
                return null;

            db?.DimAnnouncements?.Attach(announcement);
            db.Entry(announcement).State = EntityState.Modified;

            announcement.AnnouncementTitle = request.AnnouncementTitle ?? announcement.AnnouncementTitle;
            announcement.AnnouncementBody = request.AnnouncementBody ?? announcement.AnnouncementBody;
            announcement.ModifiedDt = DateTime.UtcNow;
            announcement.FromDt = request.FromDt ?? announcement.FromDt;
            announcement.ToDt = request.ToDt ?? announcement.ToDt;
            announcement.AppId = request.AppId;

            db?.SaveChanges();

            return "Announcement Changed";
        }

        public async Task<dynamic> postAnnouncement(AddAnnouncementDTO request)
        {
            var announcement = new DimAnnouncements()
            {
                AnnouncementTitle = request.AnnouncementTitle,
                AnnouncementBody = request.AnnouncementBody,
                FromDt = request.FromDt ?? DateTime.UtcNow,
                ToDt = request.ToDt,
                CreateDt = DateTime.UtcNow,
                AppId = request.AppId,
            };

            db?.DimAnnouncements?.Add(announcement);
            db?.SaveChanges();

            return "Announcement Added";
        }

        public async Task<dynamic> postMaintenanceFlag(MaintenanceDTO request)
        {
            var existingData = db?.ServerFlags?.Where(acs => acs.ServerFlag == "maintenance_mode").FirstOrDefault();
            if (existingData != null)
            {
                existingData.Value = request.IsActive.ToString();
                existingData.FromDt = request.FromDt;
                existingData.ToDt = request.ToDt;
                db?.ServerFlags?.Attach(existingData);
                db.Entry(existingData).State = EntityState.Modified;
            }
            else
            {
                var flag = new ServerFlags()
                {
                    ServerFlag = "maintenance_mode",
                    FromDt = request.FromDt,
                    ToDt = request.ToDt,
                    Value = request.IsActive.ToString()
                };
                db?.ServerFlags?.Add(flag);
            }
            db?.SaveChanges();

            return "Maintenance Flag Added";
        }

        public async Task<dynamic> getServerFlags()
        {
            var flags = (from sg in db?.ServerFlags
                         select sg).ToList();

            return new ServerFlagsDTO()
            {
                MaintenanceMode = flags.Where(f => f.ServerFlag == "maintenance_mode").Select(f => new MaintenanceDTO()
                {
                    IsActive = bool.Parse(f.Value),
                    FromDt = f.FromDt,
                    ToDt = f.ToDt
                }).FirstOrDefault(),
            };
        }

        public async Task<dynamic?> getLocations()
        {
            var locations = db?.Locations?.ToList();

            return locations;
        }

        public async Task<dynamic?> getLocations(int appId)
        {
            var app_id_group = (from a in db?.DimAppIds
                                where a.AppId == appId
                                select a.GroupId).FirstOrDefault();

            var app_ids_in_group = (from a in db?.DimAppIds
                                    where (a.GroupId == app_id_group && a.GroupId != null) || a.AppId == appId
                                    select a.AppId).ToList();

            var locations = db?.Locations?.Where(x => app_ids_in_group.Contains(x.AppId)).ToList();

            return locations?.GroupBy(x => x.Id).Select(x => x.FirstOrDefault());
        }
    }
}
