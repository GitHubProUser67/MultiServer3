using CustomLogger;

namespace Horizon.MUM
{
    public class MumPartyHandler
    {
        private readonly static List<Party> AccessibleParties = new();

        public static Task AddMumParty(Party partytotoadd)
        {
            lock (AccessibleParties)
            {
                AccessibleParties.Add(partytotoadd);
            }

            return Task.CompletedTask;
        }

        public static Task UpdateMumParty(int index, Party partytoupdate)
        {
            lock (AccessibleParties)
            {
                AccessibleParties[index] = partytoupdate;
            }

            return Task.CompletedTask;
        }

        public static int GetIndexOfLocalPartyByNameAndAppId(string chatchannelname, string? partyName, int AppId)
        {
            if (string.IsNullOrEmpty(partyName))
                return -1;

            try
            {
                // Check if the AccessibleParties list is not empty
                if (AccessibleParties.Count > 0)
                {
                    // Find the index of the party that matches the given name and ID
                    int index = AccessibleParties
                        .FindIndex(party => party.PartyName == partyName && party.ApplicationId == AppId && party.ChatChannel?.Name == chatchannelname);

                    // If a matching party is found, return its index
                    if (index != -1)
                        return index;
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[MUM] - GetIndexOfLocalPartyByNameAndAppId thrown an exception: {e}");
            }

            // If no matching party is found, return -1
            return -1;
        }
    }
}
