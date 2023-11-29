using SRVEmu.Messages;

namespace SRVEmu.Model
{
    public class Room
    {
        public MatchmakerServer? Server;
        public int ID;
        public string? Name;
        public RoomUserCollection? Users;
        public List<Game> Games = new();
        public bool IsGlobal; //if a room is global, it is always open
        public int Max = 24;

        public bool AllInGame; //if session initiated between two users should extend to everyone

        public Dictionary<string, Chal> ChallengeMap = new();

        public Room()
        {
            Users = new RoomUserCollection(this);
        }

        public PlusRom GetInfo()
        {
            return new PlusRom()
            {
                I = ID.ToString(),
                N = Name,
                T = Users?.Count().ToString(),
                L = Max.ToString()
            };
        }

        public void BroadcastPopulation()
        {
            Server?.Users.Broadcast(new PlusPop() { Z = ID + "/" + Users?.Count().ToString() });
        }

        public void RemoveChallenges(User user)
        {
            lock (ChallengeMap)
            {
                var byMe = ChallengeMap.Where(x => x.Value._From == user.PersonaName).ToList();
                foreach (var chal in byMe)
                {
                    ChallengeMap.Remove(chal.Key);
                }
            }
        }
    }
}
