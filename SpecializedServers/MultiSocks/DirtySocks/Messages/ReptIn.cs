namespace MultiSocks.DirtySocks.Messages
{
    public class ReptIn : AbstractMessage
    {
        public override string _Name { get => "rept"; }
        public string? PERS { get; set; } // Persona of user to report
        public string? LANG { get; set; } // Player Language
        public string? PROD { get; set; } // OPTIONAL: product-platform-year 
        public string? TYPE { get; set; } // Report Type
        public string? TEXT { get; set; } // Report Description

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new ReptOut());
        }
    }
}
