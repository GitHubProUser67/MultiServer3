namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Mesg : AbstractMessage
    {
        public override string _Name { get => "mesg"; }

        public string? PRIV { get; set; }
        public string? TEXT { get; set; } = "";
        public string? ATTR { get; set; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            string? TEXT = GetInputCacheValue("TEXT");
            string? personaToTarget = GetInputCacheValue("PRIV");
            string? Attributes = GetInputCacheValue("ATTR");

            var mc = context as MatchmakerServerV1;
            if (mc == null || !client.HasAuth()) return;
            Model.User user = client.User;
            PlusMesg mesg = new()
            {
                N = user?.PersonaName,
                T = TEXT,
            };

            //where is this message going
            Model.Room? room = user?.CurrentRoom;

            ;
            if (!string.IsNullOrEmpty(personaToTarget))
            {
                if (Attributes != null && Attributes.Length > 1 && Attributes[0] == 'N')
                    mesg.F = "EP" + Attributes[1..];
                mc.SendToPersona(personaToTarget, mesg);
            }
            else
                room?.Users?.Broadcast(mesg);
        }
    }
}
