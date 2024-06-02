namespace MultiSocks.DirtySocks.Messages
{
    public class WebConfigNewsOut : AbstractMessage
    {
        public override string _Name { get => "news"; }

        public string? BILLBOARD_URL = "http://gos.ea.com/easo/";
        public string? BILLBOARD_TEXT = "Billboard Text!";
    }
}
