using SRVEmu.DirtySocks.Model;
using System.Text;

namespace SRVEmu.DirtySocks.Messages
{
    public class News : AbstractMessage
    {
        public override string _Name { get => "news"; }

        public string? NAME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            MatchmakerServer? mc = context as MatchmakerServer;
            if (mc == null) return;

            // Todo, send proper news data for burnout.

            if (NAME == "client.cfg") // TODO, do a real config file.
            {
                client.SendMessage(new Newsnew7()
                {

                });
            }
            else if (NAME == "0" || NAME == "1" || NAME == "3")
                client.SendMessage(Encoding.ASCII.GetBytes("MultiServer Driven EA Server."));
            else if (NAME == "8")
            {
                User? user = client.User;
                if (user == null) return;

                client.SendMessage(new PlusFup()
                {

                });

                client.SendMessage(new Newsnew8()
                {

                });
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[DirtySocks] - News - Client Requested an unknown config type: {NAME}, not responding");
        }
    }
}
