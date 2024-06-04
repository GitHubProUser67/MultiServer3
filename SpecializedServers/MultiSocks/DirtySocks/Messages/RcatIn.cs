using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.DirtySocks.Messages
{
    internal class RcatIn : AbstractMessage
    {
        public override string _Name { get => "rcat"; }
        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new RcatOut()
            {
                CAT = "temp,,tp,res0,res1,res2,res3,1",
            });
        }
    }
}
