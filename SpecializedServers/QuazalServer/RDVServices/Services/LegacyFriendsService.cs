using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using Newtonsoft.Json;
using QuazalServer.RDVServices.Entities;

namespace QuazalServer.RDVServices.Services
{
	/// <summary>
	/// User friends service
	/// </summary>
	[RMCService(RMCProtocolId.LegacyFriendsService)]
	public class LegacyFriendsService : RMCServiceBase
	{
		[RMCMethod(3)]
		public RMCResult AddFriendWithDetails(string strPlayerName, uint uiDetails, string strMessage)
		{
			UNIMPLEMENTED();
            return Result(new { retVal = true });
        }

		[RMCMethod(10)]
		public RMCResult GetDetailedList(byte byRelationship, bool bReversed)
        {
            // TODO, relationship means to switch to relationship list, bReversed I assume is order.

            if (Context != null && Context.Client.PlayerInfo != null)
            {
                PlayerInfo? plInfo = Context.Client.PlayerInfo;

				if (File.Exists(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{plInfo.Name}_{plInfo.PID}_legacy_friends-list.json"))
				{
                    List<FriendData>? list = JsonConvert.DeserializeObject<List<FriendData>>(File.ReadAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{plInfo.Name}_{plInfo.PID}_legacy_friends-list.json"));

                    if (bReversed)
                        list?.Reverse();

                    if (list != null)
                        return Result(list);
                }
				else
				{
                    User? user = DBHelper.GetUserByPID(plInfo.PID, Context.Handler.AccessKey);

                    if (user != null)
					{
                        var result = new List<FriendData>
                        {
                            new() {
                                m_pid = plInfo.PID,
                                m_strName = plInfo.Name,
                                m_byRelationship = 0,
                                m_uiDetails = user.UiGroups,
                                m_strStatus = string.Empty
                            }
                        };

                        Directory.CreateDirectory(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}");

                        File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{plInfo.Name}_{plInfo.PID}_legacy_friends-list.json", JsonConvert.SerializeObject(result, Formatting.Indented));

                        if (bReversed)
                            result?.Reverse();

                        if (result != null)
                            return Result(result);
                    }
                }
            }

            return Error(0);
        }
	}
}