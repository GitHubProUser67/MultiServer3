using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.DirtySocks.Messages
{
    public class RcatOut : AbstractMessage { 
        public override string _Name { get => "rcat"; }
        public string? CAT { get; set; }
    }
}
