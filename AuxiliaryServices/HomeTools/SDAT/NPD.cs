using System.Numerics;

namespace HomeTools.SDAT
{
    internal class NPD
    {
        private readonly byte[] magic;
        private long version;
        private long license;
        private long type;
        private readonly byte[] content_id;
        private readonly byte[] digest;
        private readonly byte[] titleHash;
        private readonly byte[] devHash;
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

        public static NPD CreateNPD(byte[] npd)
        {
            NPD npd1 = new();
            ConversionUtils.Arraycopy(npd, 0, npd1.magic, 0L, 4);
            npd1.version = ConversionUtils.Be32(npd, 4);
            npd1.license = ConversionUtils.Be32(npd, 8);
            npd1.type = ConversionUtils.Be32(npd, 12);
            ConversionUtils.Arraycopy(npd, 16, npd1.content_id, 0L, 48);
            ConversionUtils.Arraycopy(npd, 64, npd1.digest, 0L, 16);
            ConversionUtils.Arraycopy(npd, 80, npd1.titleHash, 0L, 16);
            ConversionUtils.Arraycopy(npd, 96, npd1.devHash, 0L, 16);
            npd1.unknown3 = ConversionUtils.Be64(npd, 112);
            npd1.unknown4 = ConversionUtils.Be64(npd, 120);
            if (!npd1.Validate())
                npd1 = null;
            return npd1;
        }

        public byte[] GetDevHash() => devHash;

        public byte[] GetDigest() => digest;

        public long GetLicense() => license;

        public long GetVersion() => version;

        private bool Validate() => magic[0] == 78 && magic[1] == 80 && magic[2] == 68 && magic[3] == 0 && unknown3.CompareTo(BigInteger.Zero) == 0 && unknown4.CompareTo(BigInteger.Zero) == 0;
    }
}