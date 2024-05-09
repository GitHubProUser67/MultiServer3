using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class FgetIn : AbstractMessage
    {
        public override string _Name { get => "fget"; }
        public string? TAG { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            User? user = client.User;
            if (user == null) return;

            user.SendPlusWho(user, !string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5") ? "BURNOUT5" : string.Empty);

            client.SendMessage(new PlusFup());

            client.SendMessage(new FgetOut());
        }
    }
}
