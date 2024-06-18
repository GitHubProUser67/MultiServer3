namespace MultiSocks.DirtySocks.Messages
{
    public class QuikIn : AbstractMessage
    {
        public override string _Name { get => "quik"; }
        public string? KIND { get; set; }
        public string? QMFP { get; set; }
        public string? AUTO { get; set; }
        public string? VOIP { get; set; }
        public string? GAME { get; set; }
        public string? CHAR { get; set; }
        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new QuikOut());
            // For now I keep game create commented out until I know proper flow for game creation if no matches were found
            //client.SendMessage(new GcreOut());
        }
    }
}
