using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class CusrIn : AbstractMessage
    {
        public override string _Name { get => "cusr"; }

        public string? PERS { get; set; }
        public string? SKEY { get; set; }
        public string? CMD { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            client.SendMessage(new CusrOut());
        }
    }
}
