namespace MultiSocks.DirtySocks.Messages
{
    public class Auxi : AbstractMessage
    {
        public override string _Name { get => "auxi"; }

        public string TEXT { get; set; } = string.Empty;

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            Model.User? user = client.User;
            if (user == null) return;

            user.Auxiliary = TEXT;
            client.SendMessage(this);
        }
    }
}
