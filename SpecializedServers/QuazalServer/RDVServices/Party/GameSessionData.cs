using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.DDL;
using CustomLogger;

namespace QuazalServer.RDVServices
{
	public static class GameSessions
	{
		public static readonly List<GameSessionData> SessionList = new List<GameSessionData>();

		// NOTE: returns true when session is no longer valid
		public static bool RemovePlayerFromSession(GameSessionData session, uint principalId)
		{
			session.PublicParticipants.Remove(principalId);
			session.Participants.Remove(principalId);

			session.Attributes[(uint)GameSessionAttributeType.FilledPublicSlots] = (uint)session.PublicParticipants.Count;
			session.Attributes[(uint)GameSessionAttributeType.FilledPrivateSlots] = (uint)session.Participants.Count;

			if (session.PublicParticipants.Count == 0 && session.Participants.Count == 0)
                return true;

            return false;
		}

		public static void AddPlayerToSession(GameSessionData session, uint principalId, bool isPrivate)
		{
			if (isPrivate)
				session.Participants.Add(principalId);
			else
				session.PublicParticipants.Add(principalId);

			session.Attributes[(uint)GameSessionAttributeType.FilledPublicSlots] = (uint)session.PublicParticipants.Count;
			session.Attributes[(uint)GameSessionAttributeType.FilledPrivateSlots] = (uint)session.Participants.Count;
		}

		public static void UpdateSessionParticipation(PlayerInfo player, uint newSessionId, uint newSessionTypeId, bool isPrivate)
		{
			var oldSessionId = player.GameData().CurrentSessionID;
			var oldSessionTypeId = player.GameData().CurrentSessionTypeID;

			if (oldSessionId == newSessionId)
				return;

			// set new participation
			player.GameData().CurrentSessionID = newSessionId;
			player.GameData().CurrentSessionTypeID = newSessionTypeId;

			var newSession = SessionList.FirstOrDefault(x => x.Id == newSessionId && x.TypeID == newSessionTypeId);
			if (newSession != null)
			{
				AddPlayerToSession(newSession, player.PID, isPrivate);
			}

			// remove participation from old session
			var oldSession = SessionList.FirstOrDefault(x => x.Id == oldSessionId && x.TypeID == oldSessionTypeId);
			if (oldSession != null)
			{
				if (RemovePlayerFromSession(oldSession, player.PID))
				{
					LoggerAccessor.LogWarn($"UpdateSessionParticipation - Auto-deleted session {oldSession.Id}");
					SessionList.Remove(oldSession);
				}
			}
		}
	}

	public class GameSessionData
	{
		public GameSessionData()
		{
			Attributes = new Dictionary<uint, uint>();
			HostURLs = new List<StationURL>();
			Participants = new HashSet<uint>();
			PublicParticipants = new HashSet<uint>();
		}

		public uint Id { get; set; }
		public uint TypeID { get; set; }
		public Dictionary<uint, uint> Attributes { get; set; }
		public uint HostPID { get; set; }
		public List<StationURL> HostURLs { get; set; }
		public HashSet<uint> Participants { get; set; } // ID, Private
		public HashSet<uint> PublicParticipants { get; set; } // ID, Public

		public uint[] AllParticipants
		{
			get
			{
				return Participants.Concat(PublicParticipants).ToArray();
			}
		}
	}
}
