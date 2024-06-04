namespace MultiSocks.DirtySocks.Messages
{
    public class PrivIn : AbstractMessage
    {
        public override string _Name { get => "priv"; }
        public string? MODE { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (!string.IsNullOrEmpty(MODE))
            {
                if (MODE == "ON")
                {

                    client.SendMessage(new PrivOut()
                    {
                        PRIV = "1",
                    });
                }
                else
                {

                    client.SendMessage(new PrivOut()
                    {
                        PRIV = "0",
                    });
                }
            } else
            {
                //We send Private Messenging to 0 in case MODE is not set!
                client.SendMessage(new PrivOut()
                {
                    PRIV = "0",
                });
            }
        }
    }
}
