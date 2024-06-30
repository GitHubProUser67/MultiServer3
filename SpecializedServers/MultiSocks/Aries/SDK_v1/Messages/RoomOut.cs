using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class RoomOut : AbstractMessage
    {
        public override string _Name { get => "room"; }

        public string? IDENT { get; set; }
        public string? NAME { get; set; }
        public string? HOST { get; set; }
        public string? DESC { get; set; }
        public string? COUNT { get; set; }
        public string? LIMIT { get; set; }
        public string? MAX { get; set; }
        public string FLAGS { get; set; } = "C";
    }
}
