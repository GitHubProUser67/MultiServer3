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

            if (user.CurrentGame != null)
            {
                user.CurrentGame.SetGameStatus(true);

                lock (mc.Games)
                    mc.Games.UpdateGame(user.CurrentGame);

                user.Connection?.SendMessage(new GstaOut());

                if (!string.IsNullOrEmpty(context.Project))
                {
                    if (context.Project.Contains("DPR-09"))
                        user.SendPlusWho(user, "DPR-09");
                    else if (context.Project.Contains("BURNOUT5"))
                        user.SendPlusWho(user, "BURNOUT5");
                    else
                        user.SendPlusWho(user, string.Empty);
                }
                else
                    user.SendPlusWho(user, string.Empty);

                user.CurrentGame.BroadcastPopulation();

                user.CurrentGame.BroadcastPlusSesV2();
            }
            else
            {
                // TODO SEND DIRTYSOCKS ERROR!
            }
        }
    }
}
