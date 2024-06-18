using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class ChalOut : AbstractMessage
    {
        public override string _Name { get => "chal"; }

        public string? MODE { get; set; }

    }
}
