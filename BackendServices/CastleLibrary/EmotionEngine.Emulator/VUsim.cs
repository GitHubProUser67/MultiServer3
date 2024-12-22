namespace EmotionEngine.Emulator
{
    public class VUsim
    {
        public VUops Source { get; set; } = new VUops();
        public VUops Target { get; set; } = new VUops();
        public VUops Destination { get; set; } = new VUops();
        public uint P { get; set; }
        public uint Q { get; set; }

        public VUsim()
        {

        }

        private uint CalculateEATAN(uint rawInput)
        {
            float[] eatanconst = new float[9] { 0.999999344348907f, -0.333298563957214f, 0.199465364217758f, -0.13085337519646f,
                            0.096420042216778f, -0.055909886956215f, 0.021861229091883f, -0.004054057877511f,
                            0.785398185253143f };
            PS2Float p = new PS2Float(rawInput);

            return new PS2Float(eatanconst[0]).Mul(p)
                .Add(new PS2Float(eatanconst[1]).Mul(p.Pow(3)))
                .Add(new PS2Float(eatanconst[2]).Mul(p.Pow(5)))
                .Add(new PS2Float(eatanconst[3]).Mul(p.Pow(7)))
                .Add(new PS2Float(eatanconst[4]).Mul(p.Pow(9)))
                .Add(new PS2Float(eatanconst[5]).Mul(p.Pow(11)))
                .Add(new PS2Float(eatanconst[6]).Mul(p.Pow(13)))
                .Add(new PS2Float(eatanconst[7]).Mul(p.Pow(15)))
                .Add(new PS2Float(eatanconst[8])).Raw;
        }

        public void EATAN()
        {
            P = CalculateEATAN(Source.X);
        }

        public void EATANxy()
        {
            PS2Float x = new PS2Float(Source.X);
            if (!x.IsZero())
                P = CalculateEATAN(new PS2Float(Source.Y).Div(x).Raw);
            else
                P = new PS2Float(0).Raw;
        }

        public void EATANxz()
        {
            PS2Float x = new PS2Float(Source.X);
            if (!x.IsZero())
                P = CalculateEATAN(new PS2Float(Source.Z).Div(x).Raw);
            else
                P = new PS2Float(0).Raw;
        }

        // Rounding can be slightly off: (PS2: 0x7FFFFFFF = 0x7FD76AA2 | SoftFloat: 0x7FFFFFFF = 0x7FD76AA1).
        public void ESIN()
        {
            float[] sinconsts = new float[5] { 1.0f, -0.166666567325592f, 0.008333025500178f, -0.000198074136279f, 0.000002601886990f };
            PS2Float p = new PS2Float(Source.X);

            P = new PS2Float(sinconsts[0]).Mul(p)
                .Add(new PS2Float(sinconsts[1]).Mul(p.Pow(3)))
                .Add(new PS2Float(sinconsts[2]).Mul(p.Pow(5)))
                .Add(new PS2Float(sinconsts[3]).Mul(p.Pow(7)))
                .Add(new PS2Float(sinconsts[4]).Mul(p.Pow(9))).Raw;
        }

        // Value can be slightly off: (PS2: 0x3F490FDB = 0x3EE970C2 | SoftFloat: 0x3F490FDB = 0x3EE970CF | IEEE: 0x3F490FDB = 0x3EE970B3).
        public void EEXP()
        {
            float[] consts = new float[6] { 0.249998688697815f, 0.031257584691048f, 0.002591371303424f,
                        0.000171562001924f, 0.000005430199963f, 0.000000690600018f };
            PS2Float p = new PS2Float(Source.X);

            P = PS2Float.One().Div(PS2Float.One()
                .Add(new PS2Float(consts[0]).Mul(p))
                .Add(new PS2Float(consts[1]).Mul(p.Pow(2)))
                .Add(new PS2Float(consts[2]).Mul(p.Pow(3)))
                .Add(new PS2Float(consts[3]).Mul(p.Pow(4)))
                .Add(new PS2Float(consts[4]).Mul(p.Pow(5)))
                .Add(new PS2Float(consts[5]).Mul(p.Pow(6)))
                .Pow(4)).Raw;
        }
    }
}
