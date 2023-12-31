using SRVEmu.Messages;

namespace SRVEmu.Model
{
    public class User
    {
        public int ID;
        public Game? CurrentGame;
        public Room? CurrentRoom;
        public DirtySockClient? Connection;
        public string? Username;
        public string[] Personas = new string[4];
        public string Auxiliary = string.Empty;

        public int SelectedPersona = -1;

        public string? PersonaName { get => (SelectedPersona == -1) ? null : Personas[SelectedPersona]; }

        public void SelectPersona(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            SelectedPersona = Array.IndexOf(Personas, name);
        }

        public PlusUser GetInfo()
        {
            return new PlusUser()
            {
                I = ID.ToString(),
                N = PersonaName ?? string.Empty,
                M = Username,
                A = Connection?.IP,
                X = Auxiliary,
                G = (CurrentGame?.ID ?? 0).ToString(),
                P = Connection?.Ping.ToString()
            };
        }

        public void SendPlusWho(User user)
        {
            //send who to this user to tell them who they are

            PlusUser info = user.GetInfo();

            PlusWho who = new()
            {
                I = info.I ?? string.Empty,
                N = info.N,
                M = info.M,
                A = info.A ?? string.Empty,
                X = info.X,
                R = user.CurrentRoom?.Name,
                RI = user.CurrentRoom?.ID.ToString()
            };

            user.Connection?.SendMessage(who);
        }
    }
}
