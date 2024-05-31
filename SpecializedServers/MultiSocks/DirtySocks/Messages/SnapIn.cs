namespace MultiSocks.DirtySocks.Messages
{
    public class SnapIn : AbstractMessage
    {
        public override string _Name { get => "snap"; }
        public string? INDEX { get; set; }
        public string? FIND { get; set; }
        public string? START { get; set; }
        public string? RANGE { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new SnapOut());
        }
    }
}
