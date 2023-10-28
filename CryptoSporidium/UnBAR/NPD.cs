using System.Numerics;

namespace CryptoSporidium.UnBAR
{
    internal class NPD
    {
        private byte[] magic;
        private long version;
        private long license;
        private long type;
        private byte[] content_id;
        private byte[] digest;
        private byte[] titleHash;
        private byte[] devHash;
        private BigInteger unknown3;
        private BigInteger unknown4;

        private NPD()
        {
            magic = new byte[4];
            content_id = new byte[48];
            digest = new byte[16];
            titleHash = new byte[16];
            devHash = new byte[16];
        }

        public static NPD createNPD(byte[] npd)
        {
            NPD npd1 = new NPD();
            ConversionUtils.arraycopy(npd, 0, npd1.magic, 0L, 4);
            npd1.version = ConversionUtils.be32(npd, 4);
            npd1.license = ConversionUtils.be32(npd, 8);
            npd1.type = ConversionUtils.be32(npd, 12);
            ConversionUtils.arraycopy(npd, 16, npd1.content_id, 0L, 48);
            ConversionUtils.arraycopy(npd, 64, npd1.digest, 0L, 16);
            ConversionUtils.arraycopy(npd, 80, npd1.titleHash, 0L, 16);
            ConversionUtils.arraycopy(npd, 96, npd1.devHash, 0L, 16);
            npd1.unknown3 = ConversionUtils.be64(npd, 112);
            npd1.unknown4 = ConversionUtils.be64(npd, 120);
            if (!npd1.validate())
                npd1 = null;
            return npd1;
        }

        public byte[] getDevHash() => devHash;

        public byte[] getDigest() => digest;

        public long getLicense() => license;

        public long getVersion() => version;

        private bool validate() => magic[0] == 78 && magic[1] == 80 && magic[2] == 68 && magic[3] == 0 && unknown3.CompareTo(BigInteger.Zero) == 0 && unknown4.CompareTo(BigInteger.Zero) == 0;
    }
}