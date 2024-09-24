namespace MultiSocks.Aries.Messages
{
    public class AcctDupl : AbstractMessage
    {
        public override string _Name { get => "acctdupl"; }

        public string? OPTS { get; set; }
    }
}
