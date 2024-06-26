using MultiSocks.Aries.SDK_v6.Messages.BurnoutParadisePlugin;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class NewsIn : AbstractMessage
    {
        public override string _Name { get => "news"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not MatchmakerServer) return;

            string? NAME = GetInputCacheValue("NAME");

            if (!string.IsNullOrEmpty(NAME))
            {
                switch (NAME)
                {
                    case "client.cfg":
                        if (!string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5"))
                        {
                            if (!string.IsNullOrEmpty(context.SKU) && context.SKU.Equals("PS3"))
                            {
                                client.SendMessage(new BOPNewsOut()
                                {
                                    BUDDY_SERVER = context.listenIP,
                                    QOS_LOBBY = context.listenIP,
                                    GPS_REGIONS = $"{context.listenIP},{context.listenIP},{context.listenIP},{context.listenIP}",
                                    NEWS_URL = "\"http://gos.ea.com/easo/editorial/common/2008/news/news.jsp?lang=%s&from=%s&game=Burnout&platform=ps3\"",
                                    LIVE_NEWS_URL = !MultiSocksServerConfiguration.RPCS3Workarounds ? "\"http://gos.ea.com/easo/editorial/Burnout/2008/livedata/main.jsp?lang=%s&from=%s&game=Burnout&platform=ps3&env=live&nToken=%s\"" : null, // RPCS3 Emulator not emulate the webbrowser (causes the game to be stuck at boot).
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
                                    BUDDY_SERVER = context.listenIP,
                                    QOS_LOBBY = context.listenIP,
                                    GPS_REGIONS = $"{context.listenIP},{context.listenIP},{context.listenIP},{context.listenIP}",
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
                        break;
                    case "8":
                        client.SendMessage(new Newsnew8());
                        break;
                    default:
                        CustomLogger.LoggerAccessor.LogWarn($"[DirtySocks] - News - Client Requested an unknown config type: {NAME}, not responding");
                        break;
                }
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[DirtySocks] - News - Client Requested an empty config type, not responding");
        }
    }
}
