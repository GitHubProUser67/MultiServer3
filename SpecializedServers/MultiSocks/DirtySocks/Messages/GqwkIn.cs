using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class GqwkIn : AbstractMessage
    {
        public override string _Name { get => "gqwk"; }
        public string? GS { get; set; }
        public string? GS0 { get; set; }
        public string? GS1 { get; set; }
        public string? GS2 { get; set; }
        public string? GS3 { get; set; }
        public string? GS4 { get; set; }
        public string? GS5 { get; set; }
        public string? GPS { get; set; }
        public string? FORCE_LEAVE { get; set; }
        public string? USERPARAMS { get; set; }
        public string USERFLAGS { get; set; } = "0";
        public string? NAME { get; set; }
        public string? MINSIZE { get; set; }
        public string? MAXSIZE { get; set; }
        public string? SYSFLAGS { get; set; }
        public string? PARAMS { get; set; }
        public string? SYSMASK { get; set; }
        public string? TYPE { get; set; }
        public string? MODE { get; set; }
        public string? SESSION { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (context is not MatchmakerServer mc) return;

            User? user = client.User;
            if (user == null) return;

            if (!string.IsNullOrEmpty(USERPARAMS))
                user.Params = USERPARAMS;

            if (!string.IsNullOrEmpty(FORCE_LEAVE) && FORCE_LEAVE == "1" && user.CurrentGame != null)
            {
                Game prevGame = user.CurrentGame;

                lock (mc.Games)
                {
                    if (prevGame.RemovePlayerByUsername(user.Username))
                        mc.Games.RemoveGame(prevGame);
                    else
                        mc.Games.UpdateGame(prevGame);
                }
            }

            Game? game = mc.Games.GamesSessions.Values.Where(game => !game.Priv).FirstOrDefault();

            if (game != null)
            {
                game.AddUser(user);

                user.CurrentGame = game;

                client.SendMessage(game.GetGqwkOut());

                if (!string.IsNullOrEmpty(context.Project))
                {
                    if (context.Project.Contains("DPR-09"))
                        user.SendPlusWho(user, "DPR-09");
                    else if (context.Project.Contains("BURNOUT5"))
                        user.SendPlusWho(user, "BURNOUT5");
                }
                else
                    user.SendPlusWho(user, string.Empty);

                game.BroadcastPopulation();
            }
            else
                client.SendMessage(new MissOut(_Name));
        }
    }
}
