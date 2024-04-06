using MultiSocks.DirtySocks.Model;
using System.Text;

namespace MultiSocks.DirtySocks.Messages
{
    public class NewsIn : AbstractMessage
    {
        public override string _Name { get => "news"; }

        public string? NAME { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (context is not MatchmakerServer) return;

            // Todo, send proper news data for burnout.

            if (NAME == "client.cfg") // TODO, do a real config file.
                client.SendMessage(new NewsOut()
                {
                    NEWS_URL = ((!string.IsNullOrEmpty(context.Project) && context.Project.Contains("PSN")) || (!string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3")))
                   ? "\"http://gos.ea.com/easo/editorial/common/2008/news/news.jsp?lang=%s&from=%s&game=Burnout&platform=ps3\"" : "\"http://gos.ea.com/easo/editorial/common/2008/news/news.jsp?lang=%s&from=%s&game=Burnout&platform=pc\"",
                    LIVE_NEWS_URL = ((!string.IsNullOrEmpty(context.Project) && context.Project.Contains("PSN")) || (!string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3"))) 
                   ? "\"http://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%s&from=%s&game=Burnout&platform=ps3&env=live&nToken=%s\"" : "\"http://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%s&from=%s&game=Burnout&platform=pc&env=live&nToken=%s\"",
                    LIVE_NEWS2_URL = ((!string.IsNullOrEmpty(context.Project) && context.Project.Contains("PSN")) || (!string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3")))
                   ? "\"http://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%s&from=%s&game=Burnout&platform=ps3&env=live&nToken=%s\"" : "\"http://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%s&from=%s&game=Burnout&platform=pc&env=live&nToken=%s\"",
                    STORE_DLC_URL = ((!string.IsNullOrEmpty(context.Project) && context.Project.Contains("PSN")) || (!string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3")))
                   ? "\"http://pctrial.burnoutweb.ea.com/pcstore/store_dlc.php?lang=%s&from=%s&game=Burnout&platform=ps3&env=live&nToken=%s&prodid=%s\"" : "\"http://pctrial.burnoutweb.ea.com/pcstore/store_dlc.php?lang=%s&from=%s&game=Burnout&platform=pc&env=live&nToken=%s&prodid=%s\"",
                    STORE_URL = ((!string.IsNullOrEmpty(context.Project) && context.Project.Contains("PSN")) || (!string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3")))
                   ? "\"http://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%s&from=%s&game=Burnout&platform=ps3&env=live&nToken=%s\"" : "\"http://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%s&from=%s&game=Burnout&platform=pc&env=live&nToken=%s\"",
                    TOSAC_URL = ((!string.IsNullOrEmpty(context.Project) && context.Project.Contains("PSN")) || (!string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3")))
                   ? "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=accept&lang=%s&platform=ps3&from=%s\"" : "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=accept&lang=%s&platform=pc&from=%s\"",
                    TOSA_URL = ((!string.IsNullOrEmpty(context.Project) && context.Project.Contains("PSN")) || (!string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3")))
                   ? "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=view&lang=%s&platform=ps3&from=%s\"" : "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=view&lang=%s&platform=pc&from=%s\"",
                    TOS_URL = ((!string.IsNullOrEmpty(context.Project) && context.Project.Contains("PSN")) || (!string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3")))
                   ? "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?lang=%s&platform=ps3&from=%s\"" : "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?lang=%s&platform=pc&from=%s\"",
                });
            else if (NAME == "0" || NAME == "1" || NAME == "3")
                client.SendMessage(Encoding.ASCII.GetBytes("MultiServer Driven EA Server."));
            else if (NAME == "8")
            {
                User? user = client.User;
                if (user == null) return;

                PlusUser info = user.GetInfo();

                client.SendMessage(new Newsnew8()
                {

                });

                user.SendPlusWho(user, !string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5") ? "BURNOUT5" : string.Empty);
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[DirtySocks] - News - Client Requested an unknown config type: {NAME}, not responding");
        }
    }
}
