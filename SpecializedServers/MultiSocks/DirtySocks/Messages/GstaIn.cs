using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class GstaIn : AbstractMessage
    {
        public override string _Name { get => "gsta"; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (context is not MatchmakerServer mc) return;

            User? user = client.User;
            if (user == null) return;

            if (user.CurrentGame != null) // BIG WIP, DOES NOT WORK!
            {
                user.CurrentGame.SetGameStatus(true);

                lock (mc.Games)
                    mc.Games.UpdateGame(user.CurrentGame);

                user.Connection?.SendMessage(new GstaOut());

                user.SendPlusWho(user, !string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5") ? "BURNOUT5" : string.Empty);

                user.Connection?.SendMessage(user.CurrentGame.GetPlusSesV2());

                // user.CurrentGame.StartGame();
            }
            else
            {
                // TODO SEND DIRTYSOCKS ERROR!
            }
        }
    }
}
