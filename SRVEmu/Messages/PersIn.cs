namespace SRVEmu.Messages
{
    public class PersIn : AbstractMessage
    {
        public override string _Name { get => "pers"; }

        public string? PERS { get; set; }
        public string? MADDR { get; set; }
        public string? MAC { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            Model.User? user = client.User;
            if (user == null || user.SelectedPersona != -1) return;
            user.SelectPersona(PERS);
            if (user.SelectedPersona == -1) return; //failed?

            client.SendMessage(new PersOut()
            {
                NAME = user.Username,
                PERS = user.PersonaName,
                MA = MAC,
                A = client.IP
            });

            user.SendPlusWho(user);
        }
    }
}
