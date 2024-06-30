namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Auxi : AbstractMessage
    {
        public override string _Name { get => "auxi"; }

        public string TEXT { get; set; } = string.Empty;

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            var mc = context as MatchmakerServerV1;
            if (mc == null) return;

            SDK_v1.Model.User? user = client.User;
            if (user == null) return;

            user.Auxiliary = TEXT;
            client.SendMessage(this);
        }
    }
}
