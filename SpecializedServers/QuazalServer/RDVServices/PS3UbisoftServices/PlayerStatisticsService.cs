using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3UbisoftServices
{
    [RMCService(RMCProtocolId.PlayerStatsService)]
	public class PlayerStatisticsService : RMCServiceBase
	{
		//public static List<StatisticsBoard> StatisticsBoards = new List<StatisticsBoard>();

		[RMCMethod(1)]
		public void PostTwitterMessage(string message)
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(2)]
		public RMCResult WritePlayerStats(IEnumerable<StatisticWriteWithBoard> playerStats)
		{
            UNIMPLEMENTED();
            return Error(0);
		}

		[RMCMethod(3)]
		public void WritePlayerStatsWithFriendsComparison(IEnumerable<StatisticWriteWithBoard> playerStats, List<uint> playerPIDs)
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(4)]
		public RMCResult ReadPlayerStats(IEnumerable<StatisticData> data, List<uint> playerIds)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		public class ScoreListResultTemp
        {
			public List<ScoreListRead>? list { get; set; }
			public uint playersTotal { get; set; }
		}

		[RMCMethod(5)]
		public RMCResult ReadStatsLeaderboardByRange(int boardId, int columnId, int rankStart, int numRows, int filterId, int filterValue)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(8)]
		public RMCResult ReadStatsLeaderboardByPIDs(IEnumerable<LeaderboardData> dataList, List<uint> playerPIDs)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(10)]
		public RMCResult ReadStatsLeaderboardKeyRanks(int boardId, int columnId)
		{
            UNIMPLEMENTED();
            return Error(0);
        }
	}
}
