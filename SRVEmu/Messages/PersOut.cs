namespace SRVEmu.Messages
{
    public class PersOut : AbstractMessage
    {
        public override string _Name { get => "pers"; }

        public string? A { get; set; }
        public string EXtelemetry { get; set; } = "159.153.244.82,9983,enUS,^\xf1\xfe\xf6\xd0\xcd\xc5\xcb\x9f\xb5\xa8\xf2\xa8\xa0\xe3\xa8\xa0\x98\xa0\xcb\xa3\xb7\x8c\x9a\xb2\xac\xc8\xdc\x89\xf6\xa6\x8e\x98\xb5\xea\xd0\x91\xa3\xc6\xcc\xb1\xac\xc8\xc0\x81\x83\x86\x8c\x98\xb0\xe0\xc0\x81\xa3\xec\x8c\x99\xb5\xf0\xe0\xa1\xc6\x85\xd4\xa1\xaf\x84\xd5\x93\xe7\xed\xdb\xba\xf4\xda\xc8\x81\x83\x86\xce\x97\xee\xc2\xc5\xe1\x92\xc6\xcc\x99\xb4\xe0\xe0\xb1\xc3\xa6\xce\x98\xac\xca\xb9\xab\xb5\x8a\x80";
        public string IDLE { get; set; } = "50000";
        public string LA { get; set; } = "5.84.34.44";
        public string LOC { get; set; } = "enUS";
        public string? MA { get; set; }
        public string PSINCE { get; set; } = "2009.2.8-09:41:00";
        public string SINCE { get; set; } = "2009.2.8-09:41:00";
        public string? NAME { get; set; }
        public string? PERS { get; set; }
        public string LAST { get; set; } = "2009.2.8-09:46:37";
        public string PLAST { get; set; } = "2009.2.8-09:46:00";
        public string LKEY { get; set; } = "3fcf27540c92935b0a66fd3b0000283c";
    }
}
