using MultiSocks.Aries.SDK_v6.Messages;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class PlusFup : AbstractMessage
    {
        public override string _Name { get => "+fup"; }
        public string FLUP { get; set; } = "0";
        public string PRES { get; set; } = string.Empty;
    }
}
