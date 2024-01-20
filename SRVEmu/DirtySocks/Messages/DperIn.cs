namespace SRVEmu.DirtySocks.Messages
{
    public class DperIn : AbstractMessage
    {
        public override string _Name { get => "dper"; }

        public string? PERS { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null || !client.HasAuth()) return;

            int index = mc.Database.DeletePersona(client.User.ID, PERS);
            if (index == -1)
                return;
            Model.User user = client.User;
            for (int i = index; i < 4; i++)
            {
                user.Personas[index] = i == 4 ? null : user.Personas[index + 1];
            }

            client.SendMessage(new DperOut()
            {
                NAME = user.Username,
                PERS = PERS
            });
        }
    }
}
