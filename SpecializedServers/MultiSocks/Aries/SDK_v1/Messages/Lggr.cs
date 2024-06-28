using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Lggr : AbstractMessage
    {
        public override string _Name { get => "lggr"; }

        public string? STR { get; set; }
    }
}
