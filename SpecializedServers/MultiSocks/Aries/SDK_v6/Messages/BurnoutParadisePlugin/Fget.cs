using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin
{
    public class Fget : AbstractMessage
    {
        public override string _Name { get => "fget"; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
        {
            User? user = client.User;
            if (user == null) return;

            user.SendPlusWho(user);

            client.SendMessage(new PlusFup());

            client.SendMessage(this);
        }
    }
}
