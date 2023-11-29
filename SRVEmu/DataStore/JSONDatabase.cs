using Newtonsoft.Json;
using System.Text.RegularExpressions;
using CustomLogger;

namespace SRVEmu.DataStore
{
    public class JSONDatabase : IDatabase
    {
        public int AutoInc = 1;
        public List<DbAccount> Accounts = new();
        public HashSet<string> Personas = new();

        public JSONDatabase()
        {
            //load from disk.
            LoggerAccessor.LogInfo("Loading Database...");

            try
            {
                if (File.Exists(SRVEMUServerConfiguration.DatabaseConfig))
                {
                    using (FileStream file = File.Open(SRVEMUServerConfiguration.DatabaseConfig, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        using (StreamReader io = new(file))
                        {
                            var extractedact = JsonConvert.DeserializeObject<List<DbAccount>>(io.ReadToEnd());
                            if (extractedact != null)
                                Accounts = extractedact;
                            io.Close();
                        }
                    }
                    Personas.Clear();
                    if (Accounts != null)
                    {
                        foreach (var user in Accounts)
                        {
                            foreach (var persona in user.Personas)
                            {
                                Personas.Add(persona);
                            }
                        }
                        if (Accounts.Count > 0)
                            AutoInc = Accounts.Max(x => x.ID) + 1;
                    }
                }
                else
                {
                    LoggerAccessor.LogWarn("Database file not existant. Starting with a blank slate.");
                    Save();
                }
            }
            catch (Exception)
            {
                LoggerAccessor.LogWarn("Error loading database! Starting with a blank slate.");
                Save();
            }
        }

        public bool CreateNew(DbAccount info)
        {
            if (info.Username == null) return false;
            lock (Accounts)
            {
                if (GetByName(info.Username) != null) return false; //already exists
                info.ID = AutoInc++;
                Accounts.Add(info);
                Save();
            }
            return true;
        }

        public DbAccount? GetByName(string? username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            lock (Accounts)
            {
                return Accounts.FirstOrDefault(x => x.Username == username);
            }
        }

        public int AddPersona(int id, string persona)
        {
            Regex regex = new(@"[a-zA-Z0-9\s]");
            if (!regex.IsMatch(persona)) return -1;
            var index = 0;
            lock (Accounts)
            {
                var acct = Accounts.FirstOrDefault(x => x.ID == id);
                if (acct == null || acct.Personas.Count == 4) return -1;
                if (Personas.Contains(persona)) return -2;
                Personas.Add(persona);
                acct.Personas.Add(persona);
                index = acct.Personas.Count;
                Save();
            }
            return index;
        }

        public int DeletePersona(int id, string persona)
        {
            var index = 0;
            lock (Accounts)
            {
                var acct = Accounts.FirstOrDefault(x => x.ID == id);
                if (acct == null) return -1;
                index = acct.Personas.IndexOf(persona);
                if (index != -1)
                {
                    Personas.Remove(persona);
                    acct.Personas.Remove(persona);
                    Save();
                }
            }
            return index;
        }

        private void Save()
        {
            string data;
            lock (Accounts)
            {
                data = JsonConvert.SerializeObject(Accounts);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(SRVEMUServerConfiguration.DatabaseConfig));
            using (FileStream file = File.Open(SRVEMUServerConfiguration.DatabaseConfig, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter io = new(file))
                {
                    io.Write(data);
                    io.Flush();
                }
            }
        }
    }
}
