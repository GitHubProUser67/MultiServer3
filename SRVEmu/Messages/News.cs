using CryptoSporidium;
using System.Text;

namespace SRVEmu.Messages
{
    public class News : AbstractMessage
    {
        public override string _Name { get => "news"; }

        public string? NAME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            var mc = context as MatchmakerServer;
            if (mc == null) return;

            if (NAME == "client.cfg") // TODO, do a real config file.
            {
                byte[] NewsPayload = new byte[] { 0x6e, 0x65, 0x77, 0x73, 0x6e, 0x65, 0x77, 0x37, 0x00, 0x00, 0x01, 0x80 };

                byte[] newstext = Encoding.ASCII.GetBytes($"BUDDY_SERVER={MiscUtils.GetLocalIPAddress()}\nBUDDY_PORT=10899 BUDDY_URL=http://ps3burnout08.ea.com/\n" +
                    "TOSAC_URL=http://ps3burnout08.ea.com/TOSAC.txt\nTOSA_URL=http://ps3burnout08.ea.com/TOSA.txt\n" +
                    "TOS_URL=http://ps3burnout08.ea.com/TOS.txt\nNEWS_TEXT=VTSTech.is.reviving.games_NEWS\n" +
                    "TOS_TEXT=VTSTech.is.reviving.games_TOS\nNEWS_DATE=2023.12.06-02:48:57 NEWS_URL=http://ps3burnout08.ea.com/news.txt\n"); // Lenght is VERY IMPORTANT.

                client.SendMessage(MiscUtils.Combinebytearay(NewsPayload, MiscUtils.Combinebytearay(newstext, new byte[] { 0x00 })));
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[DirtySocks] - News - Client Requested an unknown config type: {NAME}, not responding");
        }
    }
}
