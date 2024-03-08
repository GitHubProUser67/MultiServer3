using Newtonsoft.Json;

namespace QuazalServer.RDVServices.Entities
{
	public class User
	{
		public uint Id { get; set; }
		public string? Username { get; set; }
		public string? PlayerNickName { get; set; }
		public uint PID { get; set; }
        public string? Name { get; set; }
        public uint UiGroups { get; set; }
        public string? Email { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string? NotEffectiveMsg { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? ExpiredMsg { get; set; }

		[JsonIgnore]
		public string? Password { get; set; }
        [JsonIgnore]
        public int RewardFlags { get; set; }
    }
}
