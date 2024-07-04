using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using Newtonsoft.Json;
using QuazalServer.RDVServices.Entities;
using QuazalServer.RDVServices.RMC;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace QuazalServer.RDVServices.v2Services
{
    /// <summary>
    /// User friends service
    /// </summary>
    [RMCService(RMCProtocolId.FriendsService)]
	public class LegacyFriendsService : RMCServiceBase
	{
		[RMCMethod(3)]
		public RMCResult UpdateDetails(uint uiPlayer, uint uiDetails)
        {
            if (Context != null && Context.Client.PlayerInfo != null)
            {
                PlayerInfo? plInfo = Context.Client.PlayerInfo;

                if (File.Exists(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{Context.Handler.AccessKey}/{plInfo.Name}_{plInfo.PID}_legacy_friends-list.json"))
                {
                    List<FriendData>? list = JsonConvert.DeserializeObject<List<FriendData>>(File.ReadAllText(QuazalServerConfiguration.QuazalStaticFolder
                        + $"/Accounts/{Context.Handler.AccessKey}/{plInfo.Name}_{plInfo.PID}_legacy_friends-list.json"));

                    if (list != null)
                    {
                        FriendData? data = list.Where(detail => detail.m_pid == uiPlayer).FirstOrDefault();

                        if (data != null)
                        {
                            data.m_uiDetails = uiDetails;

                            File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder
                                + $"/Accounts/{Context.Handler.AccessKey}/{plInfo.Name}_{plInfo.PID}_legacy_friends-list.json",
                                JsonConvert.SerializeObject(list, Formatting.Indented));

                            return Result(new { retVal = true });
                        }
                    }
                }
            }

            return Result(new { retVal = false });
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
                    var keypair = DBHelper.GetUserByPID(plInfo.PID, Context.Handler.AccessKey);

                    User? user = keypair?.Item3;

                    if (user != null)
					{
                        List<FriendData> result = new();

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