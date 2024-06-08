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
            
            if (NAME == "7" || NAME == "client.cfg")
            {
                if (context.Project == "BURNOUT5")
                {
                    if ((!string.IsNullOrEmpty(context.Project) && context.Project.Contains("PSN")) || (!string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3")))
                    {
                        client.SendMessage(new BOPNewsOut()
                        {
                            NEWS_URL = "\"http://gos.ea.com/easo/editorial/common/2008/news/news.jsp?lang=%s&from=%s&game=Burnout&platform=ps3\"",
                            LIVE_NEWS_URL = (!MultiSocksServerConfiguration.RPCS3Workarounds) ? "\"http://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%s&from=%s&game=Burnout&platform=ps3&env=live&nToken=%s\"" : null, // RPCS3 Emulator not emulate the webbrowser (causes the game to be stuck at boot).
                            LIVE_NEWS2_URL = "\"http://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%s&from=%s&game=Burnout&platform=ps3&env=live&nToken=%s\"",
                            STORE_DLC_URL = "\"http://pctrial.burnoutweb.ea.com/pcstore/store_dlc.php?lang=%s&from=%s&game=Burnout&platform=ps3&env=live&nToken=%s&prodid=%s\"",
                            STORE_URL = "\"http://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%s&from=%s&game=Burnout&platform=ps3&env=live&nToken=%s\"",
                            TOSAC_URL = "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=accept&lang=%s&platform=ps3&from=%s\"",
                            TOSA_URL = "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=view&lang=%s&platform=ps3&from=%s\"",
                            TOS_URL = "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?lang=%s&platform=ps3&from=%s\""
                        });
                    }
                    else
                    {
                        client.SendMessage(new BOPNewsOut()
                        {
                            NEWS_URL = "\"http://gos.ea.com/easo/editorial/common/2008/news/news.jsp?lang=%s&from=%s&game=Burnout&platform=pc\"",
                            LIVE_NEWS_URL = "\"http://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%s&from=%s&game=Burnout&platform=pc&env=live&nToken=%s\"",
                            LIVE_NEWS2_URL = "\"http://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%s&from=%s&game=Burnout&platform=pc&env=live&nToken=%s\"",
                            STORE_DLC_URL = "\"http://pctrial.burnoutweb.ea.com/pcstore/store_dlc.php?lang=%s&from=%s&game=Burnout&platform=pc&env=live&nToken=%s&prodid=%s\"",
                            STORE_URL = "\"http://pctrial.burnoutweb.ea.com/t2b/page/index.php?lang=%s&from=%s&game=Burnout&platform=pc&env=live&nToken=%s\"",
                            TOSAC_URL = "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=accept&lang=%s&platform=pc&from=%s\"",
                            TOSA_URL = "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?style=view&lang=%s&platform=pc&from=%s\"",
                            TOS_URL = "\"http://gos.ea.com/easo/editorial/common/2008/tos/tos.jsp?lang=%s&platform=pc&from=%s\""
                        });
                    }
                } 
                else
                    client.SendMessage(new NewsOut());
            }
            else if (NAME == "0")
                client.SendMessage(new Newsnew0() { BUDDYRESOURCE = context.Project });
            else if (NAME == "1" || NAME == "3")
                client.SendMessage(Encoding.ASCII.GetBytes("MultiServer Driven EA Server."));
            else if (NAME == "8")
            {
                User? user = client.User;
                if (user == null) return;

                if (!string.IsNullOrEmpty(context.Project))
                {
                    if (context.Project.Contains("DPR-09"))
                        user.SendPlusWho(user, "DPR-09");
                    else if (context.Project.Contains("BURNOUT5"))
                        user.SendPlusWho(user, "BURNOUT5");
                }
                else
                    user.SendPlusWho(user, string.Empty);

                client.SendMessage(new Newsnew8());
            }
            // NCAA MM 06
            else if (NAME == "quickmsgs.en")
            {
                client.SendMessage(new NewsOut());
            }
            else if (NAME.Contains("webconfig."))
            {
                client.SendMessage(new WebConfigNewsOut() { BILLBOARD_URL = "http://gos.ea.com/easo/", BILLBOARD_TEXT = "Test" });
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[DirtySocks] - News - Client Requested an unknown config type: {NAME}, not responding");
        }
    }
}
