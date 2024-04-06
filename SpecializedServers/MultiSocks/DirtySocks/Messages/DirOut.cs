namespace MultiSocks.DirtySocks.Messages
{
    public class DirOut : AbstractMessage
    {
        public override string _Name { get => "@dir"; }

        public string ADDR { get; set; } = "86.154.114.253";
        public string PORT { get; set; } = "10901";
        public string SESS { get; set; } = "1072010288";
        public string MASK { get; set; } = "0295f3f70ecb1757cd7001b9a7a5eac8";
    }
}
