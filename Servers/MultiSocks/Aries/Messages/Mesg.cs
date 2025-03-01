namespace MultiSocks.Aries.Messages
{
    public class Mesg : AbstractMessage
    {
        public override string _Name { get => "mesg"; }

        public string? PRIV { get; set; }
        public string TEXT { get; set; } = string.Empty;
        public string? ATTR { get; set; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not MatchmakerServer mc || !client.HasAuth()) return;

            Model.AriesUser? user = client.User;
            if (user == null) return;

            PlusMesg mesg = new()
            {
                N = user?.PersonaName,
                T = TEXT,
            };

            //where is this message going
            Model.AriesRoom? room = user?.CurrentRoom;

            if (!string.IsNullOrEmpty(PRIV))
            {
                if (ATTR != null && ATTR.Length > 1 && ATTR[0] == 'N')
                    mesg.F = "EP" + ATTR[1..];
                mc.SendToPersona(PRIV, mesg);
            }
            else
                room?.Users?.Broadcast(mesg);
        }
    }
}
