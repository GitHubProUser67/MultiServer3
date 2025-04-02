namespace MultiSocks.Aries.Messages
{
    public class PADD : AbstractMessage
    {
        public override string _Name { get => "PADD"; }
        public string? USER { get; set; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {

            string? USER = GetInputCacheValue("USER");

            PADD padd = new PADD()
            {
                USER = USER,
            };

            client.SendMessage(padd);
        }
    }
}