namespace EmotionEngine.Emulator
{
    public class VUops
    {
        public uint X { get; set; }
        public uint Y { get; set; }
        public uint Z { get; set; }
        public uint W { get; set; }

        public VUops()
        {

        }

        public VUops(uint x, uint y, uint z, uint w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}
