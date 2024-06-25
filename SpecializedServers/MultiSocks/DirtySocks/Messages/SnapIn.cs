namespace MultiSocks.DirtySocks.Messages
{
    public class SnapIn : AbstractMessage
    {
        public override string _Name { get => "snap"; }
        public string? INDEX { get; set; }
        public string? FIND { get; set; }
        public string? START { get; set; }
        public string? RANGE { get; set; }
        public string? CHAN {  get; set; }
        public string? SEQN { get; set; }
        public string? CI { get; set; }
        public string? II { get; set; }
        public string? VI { get; set; }
        public string? COLS { get; set; }
        public string? VIEW { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new SnapOut());
        }
    }
}
