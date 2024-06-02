namespace MultiSocks.DirtySocks.Messages
{
    public class UatrIn : AbstractMessage
    {
        public override string _Name { get => "uatr"; }
        public string? ATTR { get; set; }
        public string? HWFLAG { get; set; }
        public string? HWMASK { get; set; }
        public string? FLAGS { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {

            client.SendMessage(new UatrOut());

            client.User.SendPlusWho(client.User, context.Project);
        }
    }
}
