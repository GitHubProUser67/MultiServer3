using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class FbstIn : AbstractMessage
    {
        public override string _Name { get => "fbst"; }
        public string? TKDNA { get; set; }
        public string? TKDNF { get; set; }
        public string? FBCHAL { get; set; }
        public string? CRSH { get; set; }
        public string? DIST { get; set; }
        public string? TIME { get; set; }
        public string? RIVALS { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            User? user = client.User;
            if (user == null) return;

            client.SendMessage(new FbstOut());
        }
    }
}