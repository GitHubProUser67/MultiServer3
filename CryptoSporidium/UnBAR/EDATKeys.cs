namespace MultiServer.CryptoSporidium.UnBAR
{
    internal class EDATKeys
    {
        public static byte[] npdrm_omac_key2 = new byte[16]
        {
           107,
           165,
           41,
           118,
           239,
           218,
           22,
           239,
           60,
           51,
           159,
           178,
           151,
           30,
           37,
           107
        };

        public static byte[] npdrm_omac_key3 = new byte[16]
        {
           155,
           81,
           95,
           234,
           207,
           117,
           6,
           73,
           129,
           170,
           96,
           77,
           145,
           165,
           78,
           151
        };

        public static byte[] SDATKEY = new byte[16]
        {
           13,
           101,
           94,
           248,
           230,
           116,
           169,
           138,
           184,
           80,
           92,
           250,
           125,
           1,
           41,
           51
        };

        public static byte[] EDATKEY = new byte[16]
        {
           190,
           149,
           156,
           168,
           48,
           141,
           239,
           162,
           229,
           225,
           128,
           198,
           55,
           18,
           169,
           174
        };
        public static byte[] EDATIV = new byte[16];

        public static byte[] EDATHASH = new byte[16]
        {
           239,
           254,
           91,
           209,
           101,
           46,
           235,
           193,
           25,
           24,
           207,
           124,
           4,
           212,
           240,
           17
        };
    }
}
