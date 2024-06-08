using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class HchkIn : AbstractMessage
    {
        public override string _Name { get => "hchk"; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            User? user = client.User;
            if (user == null) return;

            // THANK YOU EAEMU https://github.com/teknogods/eaEmu/blob/master/eaEmu/ea/games/pcburnout08.py

            if (!string.IsNullOrEmpty(context.Project))
            {
                if (context.Project.Contains("DPR-09"))
                    user.SendPlusWho(user, "DPR-09");
                else if (context.Project.Contains("BURNOUT5"))
                    user.SendPlusWho(user, "BURNOUT5");
            }
            else
                user.SendPlusWho(user, string.Empty);

            client.SendMessage(new HchkOut());
        }
    }
}
