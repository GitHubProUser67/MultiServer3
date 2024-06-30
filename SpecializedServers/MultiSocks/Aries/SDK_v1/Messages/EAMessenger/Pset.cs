using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Pset : AbstractMessage
    {
        public override string _Name { get => "PSET"; }
        public string? RSRC { get; set; }
        public string? SHOW { get; set; }
        public string? PROD { get; set; }
        public string? STAT { get; set; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            client.SendMessage(new PgetOut()
            {
                USER = "TEMP"
            });

            client.SendMessage(new PsetOut()
            {
                CHNG = "1",
                SHOW = GetInputCacheValue("SHOW"),
                PROD = GetInputCacheValue("PROD"),
                STAT = GetInputCacheValue("STAT"),
                P = "en",
                en = "en"
            });

        }
    }

    public class PsetOut : AbstractMessage
    {
        public override string _Name { get => "PRES"; }
        public string? SHOW { get; set; }
        public string? CHNG { get; set; } = "1";
        public string? TITL { get; set; }
        public string? PROD { get; set; }
        public string? EXTR { get; set; }
        public string? STAT { get; set; }
        public string? ATTR { get; set; }
        public string? P { get; set; }
        public string? en { get; set; }
    }

    public class PgetOut : AbstractMessage
    {
        public override string _Name { get => "PGET"; }
        public string? USER { get; set; }
    }
}
