using MultiSocks.Aries.SDK_v6.Messages;
using System.Text;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class NewsIn : AbstractMessage
    {
        public override string _Name { get => "news"; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            if (context is not MatchmakerServerV1) return;

            string? NAME = GetInputCacheValue("NAME");

            if (!string.IsNullOrEmpty(NAME))
            {
                switch (NAME) {

                    case "client.cfg":
                        client.SendMessage(new NewsOut());
                        break;
                    case "0":
                        client.SendMessage(new Newsnew0() { BUDDYRESOURCE = context.Project });
                        break;
                    case "1":
                        client.SendMessage(Encoding.ASCII.GetBytes("MultiServer Driven EA Server."));
                        break;
                    case "3":
                        client.SendMessage(Encoding.ASCII.GetBytes("MultiServer Driven EA Server."));
                        break;
                    case "7":
                        client.SendMessage(new NewsOut());
                        break;
                    case "8":
                        {
                            Model.User? user = client.User;
                            if (user == null) return;
                                user.SendPlusWho(user);


                            client.SendMessage(new Newsnew8());
                        }
                        break;
                    case "quickmsgs":
                        {
                            client.SendMessage(new NewsOut());
                            break;
                        }
                    case "quickmsgs.enUS":
                        {
                            client.SendMessage(new NewsOut());
                            break;
                        }
                    case "webconfig.":
                        {
                            client.SendMessage(new NewsOut());
                            break;
                        }
                    case "webconfig.enUS":
                        {
                            client.SendMessage(new NewsOut());
                            break;
                        }
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
