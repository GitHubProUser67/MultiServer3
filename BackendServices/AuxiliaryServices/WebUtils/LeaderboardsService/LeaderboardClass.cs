using WebUtils.LeaderboardsService.NDREAMS;
using WebUtils.LeaderboardsService.VEEMEE;

namespace WebUtils.LeaderboardsService
{
    public class LeaderboardClass
    {
        public static string? APIPath { get; set; }

        public static void ScheduledUpdate(object? state)
        {
            DateTime dateTime = DateTime.Now;
            GFScoreBoardData.SanityCheckLeaderboards($"{APIPath}/VEEMEE/gofish", dateTime.AddDays(-1));
            olmScoreBoardData.SanityCheckLeaderboards($"{APIPath}/VEEMEE/olm", dateTime.AddDays(-7));
            GSScoreBoardData.SanityCheckLeaderboards($"{APIPath}/VEEMEE/goalie", dateTime.AddDays(-1));
            GSScoreBoardData.SanityCheckLeaderboards($"{APIPath}/VEEMEE/sfrgbt", dateTime.AddDays(-1));
            OrbrunnerScoreBoardData.SanityCheckLeaderboards($"{APIPath}/NDREAMS/Aurora/Orbrunner", dateTime.AddDays(-1));
        }
    }
}
