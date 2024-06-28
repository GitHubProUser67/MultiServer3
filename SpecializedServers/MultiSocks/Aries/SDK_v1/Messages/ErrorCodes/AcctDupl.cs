using MultiSocks.Aries.SDK_v1.Messages;

namespace MultiSocks.Aries.SDK_v1.Messages.ErrorCodes
{
    public class AcctDupl : AbstractMessage
    {
        public override string _Name { get => "acctdupl"; }

        public string? OPTS { get; set; }
    }
}
