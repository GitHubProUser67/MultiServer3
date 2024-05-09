namespace MultiSocks.DirtySocks.Messages
{
    public class RankOut : AbstractMessage
    {
        public override string _Name { get => "rank"; }

        public string RANK { get; set; } = "Unranked";
        public string TIME { get; set; } = "1707089968";
    }
}
