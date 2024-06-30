using MultiSocks.Aries.SDK_v6.Messages;
using System.Text;

namespace MultiSocks.Aries.SDK_v6.Model
{
    public class User
    {
        public int ID;
        public Game? CurrentGame;
        public AriesClient? Connection;
        public string LADDR = "127.0.0.1";
        public string ADDR = "127.0.0.1";
        public string Username = "@brobot24";
        public string LOC = "frFR";
        public string MAC = string.Empty;
        public string[] Personas = new string[4];
        public string Auxiliary = string.Empty;
        private string[] Parameters = new string[] { "PUSMC01?????", string.Empty, string.Empty, "-1", "-1", string.Empty, "d" };

        public int SelectedPersona = -1;

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

        public void SendPlusWho(User user, bool async = false)
        {
            //send who to this user to tell them who they are

            PlusUser info = user.GetInfo();

            PlusWho who = new()
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
                MA = user.MAC,
                LO = LOC,
                X = info.X,
                US = "0",
                PRES = "1",
                VER = "7",
                C = ",,,,,,,,"
            };

            if (async)
                user.Connection?.EnqueueAsyncMessage(who);
            else
                user.Connection?.SendMessage(who);
        }

        public OnlnOut SendOnlnOut(User user)
        {
            //send who to this user to tell them who they are

            PlusUser info = user.GetInfo();

            return new OnlnOut()
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
                MA = user.MAC,
                LO = LOC,
                X = info.X,
                US = "0",
                PRES = "1",
                VER = "7",
                C = ",,,,,,,,"
            };
        }
    }
}
