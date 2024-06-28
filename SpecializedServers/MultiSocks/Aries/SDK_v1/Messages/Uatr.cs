using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Uatr : AbstractMessage
    {
        public override string _Name { get => "uatr"; }
        public string? ATTR { get; set; }
        public string? HWFLAG { get; set; }
        public string? HWMASK { get; set; }
        public string? FLAGS { get; set; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {

            client.SendMessage(new UatrOut());

            client.User.SendPlusWho(client.User);
        }
    }

    public class UatrOut : AbstractMessage
    {
        public override string _Name { get => "uatr"; }
    }
}
