using Newtonsoft.Json;
using System.Text;

namespace Horizon.HTTPSERVICE
{
    public class CrudCIDManager
    {
        private static readonly List<User> users = new();

        // Update or Create a User based on the provided parameters
        public static void CreateUser(string? UserName, string? MachineID)
        {
            if (string.IsNullOrEmpty(UserName))
                return;

            if (string.IsNullOrEmpty(MachineID))
                MachineID = "empty";

            lock (users)
            {
                User? userToUpdate = users.FirstOrDefault(u => u.UserName == UserName && u.MachineID == MachineID);

                if (userToUpdate == null)
                {
                    userToUpdate = new User { UserName = UserName, MachineID = MachineID };
                    users.Add(userToUpdate);
                }
            }
        }

        // Remove a User from a specific room based on the provided parameters
        public static void RemoveUserFromGame(string? UserName, string? MachineID)
        {
            if (string.IsNullOrEmpty(UserName))
                return;

            if (string.IsNullOrEmpty(MachineID))
                MachineID = "empty";

            lock (users)
            {
                users.RemoveAll(u => u.UserName == UserName && u.MachineID == MachineID);
            }
        }

        // Get a list of all Users
        public static List<User> GetAllUsers()
        {
            lock (users)
                return users;
        }

        // Serialize the Users list to JSON
        public static string ToJson(bool encrypt)
        {
            lock (users)
            {
                string JsonData = JsonConvert.SerializeObject(users, Formatting.Indented);
                return encrypt ? XORString(JsonData, HorizonServerConfiguration.MediusAPIKey) : JsonData;
            }
        }

        private static string XORString(string input, string? key)
        {
            if (string.IsNullOrEmpty(key))
                key = "@00000000000!00000000000!";

            StringBuilder result = new();

            for (int i = 0; i < input.Length; i++)
            {
                result.Append((char)(input[i] ^ key[i % key.Length]));
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(result.ToString()));
        }
    }

    public class User
    {
        public string? UserName { get; set; }
        public string? MachineID { get; set; }
    }
}
