namespace MultiSocks.DirtySocks.Messages
{
    public class PersIn : AbstractMessage
    {
        public override string _Name { get => "pers"; }

        public string? PERS { get; set; }
        public string? MADDR { get; set; }
        public string? MAC { get; set; }
        public string? MID { get; set; }
        public string? PID { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            Model.User? user = client.User;
            if (user == null || user.SelectedPersona != -1) return;
            user.SelectPersona(PERS);
            if (user.SelectedPersona == -1) return; //failed?

            if (!string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5"))
                client.SendMessage(new PersOut()
                {
                    NAME = user.Username,
                    PERS = user.PersonaName,
                    LAST = "2018.1.1-00:00:00",
                    PLAST = "2018.1.1-00:00:00",
                    SINCE = "2008.1.1-00:00:00",
                    PSINCE = "2008.1.1-00:00:00",
                    LKEY = "000000000000000000000000000",
                    STAT = ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
                    LOC = "frFR",
                    MA = MAC,
                    LA = client.IP,
                    IDLE = "50000"
                });
            else
            {
                client.SendMessage(new PersOut()
                {
                    NAME = user.Username,
                    PERS = user.PersonaName,
                    MA = MAC,
                    A = client.IP
                });

                user.SendPlusWho(user, !string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5") ? "BURNOUT5" : string.Empty);
            }
        }
    }
}
