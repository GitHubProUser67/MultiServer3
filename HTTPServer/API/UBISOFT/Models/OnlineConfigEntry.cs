﻿namespace HTTPServer.API.UBISOFT.Models
{
    public class OnlineConfigEntry
    {
        public OnlineConfigEntry()
        {
            Values = new List<string>();
        }

        public string? Name { get; set; }
        public IEnumerable<string> Values { get; set; }
    }
}
