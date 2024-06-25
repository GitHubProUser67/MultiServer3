using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastleLibrary.Utils.SSC2
{
    public class CryptSSC2T
    {
        public byte idx;
        public long crc;
        public byte[] sbox = new byte[256];
    }
}
