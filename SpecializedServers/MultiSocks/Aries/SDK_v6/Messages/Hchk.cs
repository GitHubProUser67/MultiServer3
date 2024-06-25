using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class Hchk : AbstractMessage
    {
        public override string _Name { get => "hchk"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            User? user = client.User;
            if (user == null) return;

            // THANK YOU EAEMU https://github.com/teknogods/eaEmu/blob/master/eaEmu/ea/games/pcburnout08.py
            user.SendPlusWho(user);

            client.SendMessage(this);
        }
    }
}
