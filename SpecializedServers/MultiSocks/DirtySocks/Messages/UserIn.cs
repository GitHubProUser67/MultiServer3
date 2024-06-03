namespace MultiSocks.DirtySocks.Messages
{
    public class UserIn : AbstractMessage
    {
        public override string _Name { get => "user"; }
        public string? PERS { get; set; }
        public string? NAME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            //TODO: provide actual user info
            Model.User? user = client.User;
            if (user == null) return;

            UserOut result = new()
            {
                PERS = user.PersonaName,
                MESG = user.Username,
                ADDR = client.IP,
            };

            client.SendMessage(result);
        }
    }
}
