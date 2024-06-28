using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class PlusMesg : AbstractMessage
    {
        public override string _Name { get => "+msg"; }

        public string? F { get; set; }
        public string? T { get; set; }
        public string? N { get; set; }
    }
}
