using MultiSocks.Aries.SDK_v6;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class PersIn : AbstractMessage
    {
        public override string _Name { get => "pers"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            if (context is not MatchmakerServerV1) return;

            Model.User? user = client.User;
            if (user == null || user.SelectedPersona != -1) return;
            user.SelectPersona(GetInputCacheValue("PERS"));
            if (user.SelectedPersona == -1) return; //failed?

            client.SendMessage(new PersOut()
            {
                A = user.ADDR,
                NAME = user.Username,
                PERS = user.PersonaName,
                LAST = "2018.1.1-00:00:00",
                PLAST = "2018.1.1-00:00:00",
                SINCE = "2008.1.1-00:00:00",
                PSINCE = "2008.1.1-00:00:00",
                LKEY = "000000000000000000000000000",
                STAT = ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
                LOC = user.LOC,
                MA = GetInputCacheValue("MAC") ?? string.Empty,
                LA = user.LADDR,
                IDLE = "50000"
            });

            /* Explanation, Burnout Paradise (PC UltimateBox most specifically, but others are impacted too) has a bug client side, where there is a race condition with this specific +who.
               They expect it to arrive after newsnew8 because of the delay original server had, so we must "simulate" this delay. */
            user.SendPlusWho(user, !string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5"));
        }
    }
}
