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

            user.SendPlusWho(user, !string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5") ? "BURNOUT5" : string.Empty); // THANK YOU EAEMU https://github.com/teknogods/eaEmu/blob/master/eaEmu/ea/games/pcburnout08.py

            client.SendMessage(new HchkOut());
        }
    }
}
