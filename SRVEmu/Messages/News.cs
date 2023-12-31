using BackendProject;
using SRVEmu.Model;
using System.Text;

namespace SRVEmu.Messages
{
    public class News : AbstractMessage
    {
        public override string _Name { get => "news"; }

        public string? NAME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            MatchmakerServer? mc = context as MatchmakerServer;
            if (mc == null) return;

            // Todo, send proper news data.

            if (NAME == "client.cfg") // TODO, do a real config file.
            {
                client.SendMessage(new Newsnew7()
                {
                    
                });
            }
            else if (NAME == "8") // TODO, should we really create room here?
            {
                client.SendMessage(new Newsnew8()
                {

                });
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[DirtySocks] - News - Client Requested an unknown config type: {NAME}, not responding");
        }
    }
}
