using Newtonsoft.Json;

namespace Alcatraz.Context.Entities
{
	public class User
	{
		public uint Id { get; set; }
		
		public string Username { get; set; }
		public string PlayerNickName { get; set; }
		[JsonIgnore]
		public string Password { get; set; }
        public int RewardFlags { get; set; }
    }
}
