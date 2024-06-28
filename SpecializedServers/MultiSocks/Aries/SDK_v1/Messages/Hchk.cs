using MultiSocks.Aries.SDK_v1.Model;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Hchk : AbstractMessage
    {
        public override string _Name { get => "hchk"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            User? user = client.User;
            if (user == null) return;

            // THANK YOU EAEMU https://github.com/teknogods/eaEmu/blob/master/eaEmu/ea/games/pcburnout08.py
            user.SendPlusWho(user);

            client.SendMessage(this);
        }
    }
}
