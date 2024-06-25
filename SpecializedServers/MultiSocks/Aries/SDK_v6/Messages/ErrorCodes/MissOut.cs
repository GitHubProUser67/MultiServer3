namespace MultiSocks.Aries.SDK_v6.Messages.ErrorCodes
{
    public class MissOut : AbstractMessage
    {
        public override string _Name { get; }

        // Constructor to initialize _Name
        public MissOut(string MsgName)
        {
            _Name = MsgName + "miss";
        }
    }
}
