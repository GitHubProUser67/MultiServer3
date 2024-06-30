namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Rept : AbstractMessage
    {
        public override string _Name { get => "rept"; }
        public string? PERS { get; set; } // Persona of user to report
        public string? LANG { get; set; } // Player Language
        public string? PROD { get; set; } // OPTIONAL: product-platform-year 
        public string? TYPE { get; set; } // Report Type
        public string? TEXT { get; set; } // Report Description

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            client.SendMessage(new ReptOut());
        }
    }

    public class ReptOut : AbstractMessage
    {
        public override string _Name { get => "rept"; }
    }
}
