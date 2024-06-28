using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Quik : AbstractMessage
    {
        public override string _Name { get => "quik"; }
        public string? KIND { get; set; }
        public string? QMFP { get; set; }
        public string? AUTO { get; set; }
        public string? VOIP { get; set; }
        public string? GAME { get; set; }
        public string? CHAR { get; set; }
        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            client.SendMessage(new QuikOut() { COUNT = "0" });
            // For now I keep game create commented out until I know proper flow for game creation if no matches were found
            //client.SendMessage(new GcreOut());
        }
    }

    public class QuikOut : AbstractMessage
    {
        public override string _Name { get => "quik"; }
        public string? COUNT { get; set;}
    }
}
