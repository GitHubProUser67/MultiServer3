using MultiSocks.Aries.Messages;
using System.Text;

namespace MultiSocks.Aries.Model
{
    public class AriesUser
    {
        public int ID;
        public AriesGame? CurrentGame;
        public AriesRoom? CurrentRoom;
        public AriesClient? Connection;
        public string LADDR = "127.0.0.1";
        public string ADDR = "127.0.0.1";
        public string Username = "brobot24";
        public string LOC = "frFR";
        public string MAC = string.Empty;
        public string?[] Personas = new string[4];
        public string Auxiliary = string.Empty;
        private string[] Parameters = new string[] { "PUSMC01?????", string.Empty, string.Empty, "-1", "-1", string.Empty, "d" };

        public int SelectedPersona = -1;

        public DateTime? JoinedGameSince = null;

        public string? PersonaName { get => SelectedPersona == -1 ? null : Personas[SelectedPersona]; }

        public void SelectPersona(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            SelectedPersona = Array.IndexOf(Personas, name);
        }

        public void SetParametersFromString(string Parameters)
        {
            List<string> ParametersToAdd = new();

            foreach (string param in Parameters.Split(','))
            {
                ParametersToAdd.Add(param);
            }

            SetParameters(ParametersToAdd);
        }

        public void SetParameters(List<string> Parameters)
        {
            this.Parameters = new string[Parameters.Count];

            Parameters.CopyTo(this.Parameters);
        }

        public string GetParametersString(Func<int, string, string>? CustomParameterProcess = null)
        {
            StringBuilder st = new();

            int i = 0;

            foreach (string param in Parameters)
            {
                if (st.Length != 0)
                {
                    if (CustomParameterProcess != null)
                        st.Append("," + CustomParameterProcess(i, param));
                    else
                        st.Append("," + param);
                }
                else
                {
                    if (CustomParameterProcess != null)
                        st.Append(CustomParameterProcess(i, param));
                    else
                        st.Append(param);
                }

                i++;
            }

            return st.ToString();
        }

        public PlusUser GetInfo()
        {
            return new PlusUser()
            {
                I = ID.ToString(),
                N = PersonaName ?? string.Empty,
                M = Username,
                A = ADDR,
                X = Auxiliary,
                G = (CurrentGame?.ID ?? 0).ToString(),
                P = Connection?.Ping.ToString()
            };
        }

        public void SendPlusWho(AriesUser user, string? VERS = null)
        {
            //send who to this user to tell them who they are

            PlusUser info = user.GetInfo();

            PlusWho who;

            if (!string.IsNullOrEmpty(VERS) && VERS.Contains("MOH2"))
            {
                who = new PlusWho()
                {
                    LO = LOC,
                    C = "4000,,7,1,1,,1,1,5553",
                    F = "U",
                    LV = "1049601",
                    HW = "0",
                    P = "211",
                    S = "1,2,3,4,5,6,7,493E0,C350",
                    MD = "0",
                    US = "0",
                    CI = "0",
                    CL = "511",
                    RGC = "0",
                    CT = "0",
                    AT = string.Empty,
                    RF = "0",
                    RG = (user.CurrentGame != null) ? user.CurrentGame.ID.ToString() : "0",
                    RM = "0",
                    RP = user.CurrentRoom?.Users?.Count().ToString(),
                    I = info.I ?? string.Empty,
                    N = info.N,
                    M = info.M,
                    A = info.A ?? string.Empty,
                    LA = user.LADDR ?? string.Empty,
                    X = info.X,
                    R = user.CurrentRoom?.Name,
                    RI = user.CurrentRoom?.ID.ToString()
                };
            } 
            else if (!string.IsNullOrEmpty(VERS) && VERS.Contains("FLM/A1"))
            {
                who = new PlusWho()
                {
                    LO = LOC,
                    C = "0",
                    F = "U",
                    LV = "1049601",
                    HW = "0",
                    G = "0",
                    P = "1",
                    S = "0",
                    MD = "0",
                    US = "0",
                    CI = "0",
                    CL = "511",
                    RGC = "0",
                    CT = "0",
                    AT = string.Empty,
                    RF = "0",
                    RG = (user.CurrentGame != null) ? user.CurrentGame.ID.ToString() : "0",
                    RM = "0",
                    RP = user.CurrentRoom?.Users?.Count().ToString(),
                    I = info.I ?? string.Empty,
                    N = info.N,
                    M = info.M,
                    A = info.A ?? string.Empty,
                    LA = user.LADDR ?? string.Empty,
                    X = info.X,
                    R = user.CurrentRoom?.Name,
                    RI = user.CurrentRoom?.ID.ToString()
                };
            }
            else if (!string.IsNullOrEmpty(VERS) && (VERS.Contains("BURNOUT5") || VERS.Contains("DPR-09")))
            {
                who = new PlusWho()
                {
                    I = info.I ?? string.Empty,
                    N = info.N,
                    M = info.M,
                    F = "U",
                    A = info.A ?? string.Empty,
                    P = "1",
                    S = ",,",
                    G = user.CurrentGame?.ID.ToString(),
                    AT = string.Empty,
                    CL = "511",
                    LV = "1049601",
                    MD = "0",
                    LA = user.LADDR ?? string.Empty,
                    HW = "0",
                    RP = "0",
                    MA = "$000000000000",
                    LO = LOC,
                    X = info.X,
                    US = "0",
                    PRES = "1",
                    VER = "7",
                    C = ",,,,,,,,"
                };
            }
            else
                who = new PlusWho()
                {
                    I = info.I ?? string.Empty,
                    N = info.N,
                    M = info.M,
                    A = info.A ?? string.Empty,
                    X = info.X,
                    R = user.CurrentRoom?.Name,
                    RI = user.CurrentRoom?.ID.ToString(),
                    RF = "C", 
                    RT = "1",
                    S = string.Empty,
                    F = string.Empty,
                };

            user.Connection?.SendMessage(who);
        }

        public Onln SendOnlnOut(AriesUser user, string VERS = "")
        {
            //send who to this user to tell them who they are

            PlusUser info = user.GetInfo();

            Onln onln;

            if (VERS.Contains("MOH2"))
            {
                onln = new Onln()
                {
                    LO = LOC,
                    C = "4000,,7,1,1,,1,1,5553",
                    F = "U",
                    LV = "1049601",
                    HW = "0",
                    P = "211",
                    S = "1,2,3,4,5,6,7,493E0,C350",
                    MD = "0",
                    US = "0",
                    CI = "0",
                    CL = "511",
                    RGC = "0",
                    CT = "0",
                    AT = string.Empty,
                    RF = "0",
                    RG = (user.CurrentGame != null) ? user.CurrentGame.ID.ToString() : "0",
                    RM = "0",
                    RP = user.CurrentRoom?.Users?.Count().ToString(),
                    I = info.I ?? string.Empty,
                    N = info.N,
                    M = info.M,
                    A = info.A ?? string.Empty,
                    LA = user.LADDR ?? string.Empty,
                    X = info.X,
                    R = user.CurrentRoom?.Name,
                    RI = user.CurrentRoom?.ID.ToString()
                };
            }
            else if (VERS.Contains("BURNOUT5"))
            {
                onln = new Onln()
                {
                    I = info.I ?? string.Empty,
                    N = info.N,
                    M = info.M,
                    F = "U",
                    A = info.A ?? string.Empty,
                    P = "1",
                    S = ",,",
                    G = user.CurrentGame?.ID.ToString(),
                    AT = string.Empty,
                    CL = "511",
                    LV = "1049601",
                    MD = "0",
                    LA = user.LADDR ?? string.Empty,
                    HW = "0",
                    RP = "0",
                    MA = "$000000000000",
                    LO = LOC,
                    X = info.X,
                    US = "0",
                    PRES = "1",
                    VER = "7",
                    C = ",,,,,,,,"
                };
            }
            else
                onln = new Onln()
                {
                    I = info.I ?? string.Empty,
                    N = info.N,
                    M = info.M,
                    A = info.A ?? string.Empty,
                    X = info.X,
                    R = user.CurrentRoom?.Name,
                    RI = user.CurrentRoom?.ID.ToString(),
                    RF = "C",
                    RT = "1"
                };

            return onln;
        }
    }
}
