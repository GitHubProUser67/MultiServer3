using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class ChalOut : AbstractMessage
    {
        public override string _Name { get => "chal"; }

        public string? HOST { get; set; }
        public string? KIND { get; set; }
        public string? PERS { get; set; }

    }
}
