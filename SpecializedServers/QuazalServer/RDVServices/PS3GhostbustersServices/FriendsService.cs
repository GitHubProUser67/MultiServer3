using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3GhostbustersServices
{
    /// <summary>
    /// User friends service
    /// </summary>
    [RMCService(RMCProtocolId.FriendsService)]
	public class FriendsService : RMCServiceBase
	{
		[RMCMethod(1)]
		public void AddFriend()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(2)]
		public RMCResult AddFriendByName(string strPlayerName, uint uiDetails, string strMessage)
		{
            UNIMPLEMENTED();
            return Result(new { retVal = true });
		}

		[RMCMethod(3)]
		public void AddFriendWithDetails()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(4)]
		public void AddFriendByNameWithDetails()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(5)]
		public RMCResult AcceptFriendship(uint uiPlayer)
		{
            UNIMPLEMENTED();
            return Result(new { retVal = true });
		}

		[RMCMethod(6)]
		public RMCResult DeclineFriendship(uint uiPlayer)
		{
			if (Context != null && Context.Client.PlayerInfo != null)
			{
                PlayerInfo? plInfo = Context.Client.PlayerInfo;
                uint myUserPid = plInfo.PID;

                // send notification
                NotificationEvent notification = new(NotificationEventsType.FriendEvent, 0)
                {
                    m_pidSource = myUserPid,
                    m_uiParam1 = myUserPid, // i'm just guessing
                    m_uiParam2 = 3
                };

                // send to proper client
                QClient? qClient = Context.Handler.GetQClientByClientPID(uiPlayer);

                if (qClient != null)
                    NotificationQueue.SendNotification(Context.Handler, qClient, notification);
            }

            return Result(new { retVal = true });
		}

		[RMCMethod(7)]
		public void BlackList()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(8)]
		public void BlackListByName()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(9)]
		public RMCResult ClearRelationship(uint uiPlayer)
		{
            UNIMPLEMENTED();
            return Result(new { retVal = true });
		}

		[RMCMethod(10)]
		public void UpdateDetails()
		{
			UNIMPLEMENTED();
        }

		[RMCMethod(11)]
		public void GetList()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(12)]
		public RMCResult GetDetailedList(byte byRelationship, bool bReversed)
		{
            // is that always has to be empty?
            var result = new List<FriendData>();

            return Result(result);
		}

		[RMCMethod(13)]
		public RMCResult GetRelationships(int offset, int size)
		{
			var onlinePlayerIds = NetworkPlayers.Players.Select(x => x.PID);

            RelationshipsResult result = new();

			return Result(result);
		}
	}
}
