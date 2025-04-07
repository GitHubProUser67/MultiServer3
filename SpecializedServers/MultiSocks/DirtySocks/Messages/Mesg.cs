using MultiSocks.DirtySocks;

namespace MultiSocks.DirtySocks.Messages
{
    public class Mesg : AbstractMessage
    {
        public override string _Name { get => "mesg"; }

        public string? PRIV { get; set; }
        public string TEXT { get; set; } = string.Empty;
        public string? ATTR { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null || !client.HasAuth()) return;
            Model.User user = client.User;
            PlusMesg mesg = new()
            {
                N = user?.PersonaName,
                T = TEXT,
            };

            //where is this message going
            Model.Room? room = user?.CurrentRoom;

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
