namespace MultiSocks.Aries.SDK_v6.Messages.ErrorCodes
{
    public class AcctDupl : AbstractMessage
    {
        public override string _Name { get => "acctdupl"; }

        public string? OPTS { get; set; }
    }
}
