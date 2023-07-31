using System.Numerics;

namespace PSMultiServer.Addons.CRYPTOSPORIDIUM.UnBAR
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
            this.magic = new byte[4];
            this.content_id = new byte[48];
            this.digest = new byte[16];
            this.titleHash = new byte[16];
            this.devHash = new byte[16];
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
                npd1 = (NPD)null;
            return npd1;
        }

        public byte[] getDevHash() => this.devHash;

        public byte[] getDigest() => this.digest;

        public long getLicense() => this.license;

        public long getVersion() => this.version;

        private bool validate() => this.magic[0] == (byte)78 && this.magic[1] == (byte)80 && this.magic[2] == (byte)68 && this.magic[3] == (byte)0 && this.unknown3.CompareTo(BigInteger.Zero) == 0 && this.unknown4.CompareTo(BigInteger.Zero) == 0;
    }
}
