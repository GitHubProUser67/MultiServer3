namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class OnlnIn : AbstractMessage
    {
        public override string _Name { get => "onln"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            if (context is not MatchmakerServerV1 mc) return;

            string? PERS = GetInputCacheValue("PERS");

            Model.User? user = !string.IsNullOrEmpty(PERS) ? mc.Users.GetUserByPersonaName(PERS) : client.User;
            if (user == null) return;

            client.SendMessage(user.SendOnlnOut(user));
        }
    }
}
