using MultiSocks.Aries.Model;

namespace MultiSocks.Aries.Messages
{
    public class Fget : AbstractMessage
    {
        public override string _Name { get => "fget"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            AriesUser? user = client.User;
            if (user == null) return;

            user.SendPlusWho(user, context.Project);

            client.SendMessage(new PlusFup());

            client.SendMessage(this);
        }
    }
}
