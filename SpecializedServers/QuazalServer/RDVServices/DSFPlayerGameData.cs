using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;

namespace QuazalServer.RDVServices
{
	public class DSFPlayerGameData : IPlayerDataStore
	{
		public DSFPlayerGameData(PlayerInfo owner)
		{
			Owner = owner;

			CurrentGatheringId = uint.MaxValue;
			CurrentSessionTypeID = uint.MaxValue;
			CurrentSessionID = uint.MaxValue;
		}

		// when player dropped, game data will be destroyed
		// so we need to remove player from game session and gatherings

		public void OnDropped()
		{
			// BUG: there is a bug in dropping sessions
			PartySessions.UpdateGatheringParticipation(Owner, uint.MaxValue);
			GameSessions.UpdateSessionParticipation(Owner, uint.MaxValue, uint.MaxValue, false);
		}

		public readonly PlayerInfo Owner;

		public uint CurrentGatheringId { get; set; }
		public uint CurrentSessionTypeID { get; set; }
		public uint CurrentSessionID { get; set; }
		public PresenceElement? CurrentPresence { get; set; }
	}

	public static class DSFPlayerInfoExtensions
	{
		public static DSFPlayerGameData GameData(this PlayerInfo plInfo)
		{
			return plInfo.GetData<DSFPlayerGameData>();
		}
	}
}
