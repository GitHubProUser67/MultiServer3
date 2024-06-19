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
            {
                if (context.SKU == "PS3")
                    client.SendMessage(new PersOut()
                    {
                        A = user.ADDR,
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
                        LA = user.LADDR,
                        IDLE = "50000"
                    });
                else
                    client.SendMessage(new PersOut()
                    {
                        A = user.ADDR,
                        NAME = user.Username,
                        PERS = user.PersonaName,
                        LAST = "2018.1.1-00:00:00",
                        PLAST = "2018.1.1-00:00:00",
                        SINCE = "2008.1.1-00:00:00",
                        PSINCE = "2008.1.1-00:00:00",
                        LKEY = "SX74lCZjaiaR2COKCn4p3AAAKG4.",
                        STAT = ",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,",
                        LOC = "frFR",
                        MA = MAC,
                        LA = user.LADDR,
                        IDLE = "50000",
                        EXtelemetry = "159.153.244.82,9983,enUS,^\xf1\xfe\xf6\xd0\xcd\xc5\xcb\x9f\xb5\xa8\xf2\xa8\xa0\xe3\xa8\xa0\x98\xa0\xcb\xa3\xb7\x8c\x9a\xb2\xac\xc8\xdc\x89\xf6\xa6\x8e\x98\xb5\xea\xd0\x91\xa3\xc6\xcc\xb1\xac\xc8\xc0\x81\x83\x86\x8c\x98\xb0\xe0\xc0\x81\xa3\xec\x8c\x99\xb5\xf0\xe0\xa1\xc6\x85\xd4\xa1\xaf\x84\xd5\x93\xe7\xed\xdb\xba\xf4\xda\xc8\x81\x83\x86\xce\x97\xee\xc2\xc5\xe1\x92\xc6\xcc\x99\xb4\xe0\xe0\xb1\xc3\xa6\xce\x98\xac\xca\xb9\xab\xb5\x8a\x80"
                    });
            }
            else
            {
                client.SendMessage(new PersOut()
                {
                    NAME = user.Username,
                    PERS = user.PersonaName,
                    MA = MAC,
                    A = user.ADDR
                });
            }

            if (!string.IsNullOrEmpty(context.Project))
            {
                if (context.Project.Contains("DPR-09"))
                    user.SendPlusWho(user, "DPR-09");
                else if (context.Project.Contains("BURNOUT5"))
                    user.SendPlusWho(user, "BURNOUT5");
            }
            else
                user.SendPlusWho(user, string.Empty);
        }
    }
}
