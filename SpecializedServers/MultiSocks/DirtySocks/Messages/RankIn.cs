using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class RankIn : AbstractMessage
    {
        public override string _Name { get => "rank"; }
        public string? WHEN { get; set; }
        public string? REPT { get; set; }
        public string? AUTH { get; set; }
        public string? VENUE { get; set; }
        public string? SKU { get; set; }
        public string? NAME0 { get; set; }
        public string? TEAM0 { get; set; }
        public string? WEIGHT0 { get; set; }
        public string? NAME1 { get; set; }
        public string? TEAM1 { get; set; }
        public string? WEIGHT1 { get; set; }
        public string? USERPARAMS { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (context is not MatchmakerServer mc) return;

            User? user = client.User;
            if (user == null) return;

            client.SendMessage(new RankOut());

            user.CurrentGame?.BroadcastPopulation();
            user.CurrentGame?.SetGameStatus(false);
        }
    }
}
