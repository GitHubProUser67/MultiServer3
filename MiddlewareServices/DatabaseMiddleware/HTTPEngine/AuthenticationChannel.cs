using DatabaseMiddleware.Models;
using Newtonsoft.Json;

namespace DatabaseMiddleware.HTTPEngine
{
    public static class AuthenticationChannel

    {
        private static List<Authentication> CurrentAuths = new();

        // Add a new user
        public static void AddAuthentificationData(Authentication newAuth)
        {
            lock (CurrentAuths)
            {
                // Check if the user already exists
                if (CurrentAuths.Any(auth => auth.User.AccountId == newAuth.User.AccountId && (auth.Expiration == null || auth.Expiration > DateTime.Now)))
                    throw new InvalidOperationException("User with the same AccountId already exists.");

                // We assume there can be some expired users, simply use the slot for opimization.
                int existingAuthIndex = CurrentAuths.FindIndex(auth => auth.User.AccountId == newAuth.User.AccountId);
                if (existingAuthIndex != -1)
                    // Replace the existing authentication entry
                    CurrentAuths[existingAuthIndex] = newAuth;
                else
                    // Add the new authentication entry
                    CurrentAuths.Add(newAuth);
            }
        }

        // Remove a user by id
        public static void RemoveAuthentificationDataById(int AccountId)
        {
            lock (CurrentAuths)
            {
                Authentication? AuthentificationDataToRemove = CurrentAuths.Find(auth => auth.User.AccountId == AccountId && (auth.Expiration == null || auth.Expiration > DateTime.Now));
                if (AuthentificationDataToRemove != null)
                    CurrentAuths.Remove(AuthentificationDataToRemove);
            }
        }

        // Remove a user by username
        public static void RemoveAuthentificationDataByUsername(string username)
        {
            lock (CurrentAuths)
            {
                Authentication? AuthentificationDataToRemove = CurrentAuths.Find(auth => auth.User.AccountName == username && (auth.Expiration == null || auth.Expiration > DateTime.Now));
                if (AuthentificationDataToRemove != null)
                    CurrentAuths.Remove(AuthentificationDataToRemove);
            }
        }

        // Update user information
        public static void UpdateAllAuthentificationDataPropertiesById(int AccountId, MiddlewareUser updatedUser)
        {
            lock (CurrentAuths)
            {
                Authentication? AuthentificationDataToUpdate = CurrentAuths.Find(auth => auth.User.AccountId == AccountId && (auth.Expiration == null || auth.Expiration > DateTime.Now));
                if (AuthentificationDataToUpdate != null)
                {
                    // Update user properties
                    AuthentificationDataToUpdate.User.AccountName = updatedUser.AccountName;
                    AuthentificationDataToUpdate.User.Password = updatedUser.Password;
                    AuthentificationDataToUpdate.User.Roles = updatedUser.Roles;

                    if (AuthentificationDataToUpdate.User.Roles != null && AuthentificationDataToUpdate.User.Roles.Contains("database"))
                        AuthentificationDataToUpdate.Expiration = null;

                    int existingAuthIndex = CurrentAuths.IndexOf(AuthentificationDataToUpdate);
                    if (existingAuthIndex != -1)
                        // Update the user in the list of authenticated users
                        CurrentAuths[existingAuthIndex] = AuthentificationDataToUpdate;
                }
            }
        }

        // Get user by username
        public static MiddlewareUser? GetUserByUsername(string username)
        {
            lock (CurrentAuths)
                return CurrentAuths.Find(auth => auth.User.AccountName == username && (auth.Expiration == null || auth.Expiration > DateTime.Now))?.User;
        }

        // Get user by id
        public static MiddlewareUser? GetUserById(int AccountId)
        {
            lock (CurrentAuths)
                return CurrentAuths.Find(auth => auth.User.AccountId == AccountId && (auth.Expiration == null || auth.Expiration > DateTime.Now))?.User;
        }

        // Get user by token
        public static MiddlewareUser? GetUserByToken(string? Token)
        {
            if (string.IsNullOrEmpty(Token))
                return null;

            lock (CurrentAuths)
                return CurrentAuths.Find(auth => auth.Token == Token && (auth.Expiration == null || auth.Expiration > DateTime.Now))?.User;
        }

        // Get token by username
        public static string? GetTokenByUsername(string username)
        {
            lock (CurrentAuths)
                return CurrentAuths.Find(auth => auth.User.AccountName == username && (auth.Expiration == null || auth.Expiration > DateTime.Now))?.Token;
        }

        // Get token by id
        public static string? GetTokenById(int AccountId)
        {
            lock (CurrentAuths)
                return CurrentAuths.Find(auth => auth.User.AccountId == AccountId && (auth.Expiration == null || auth.Expiration > DateTime.Now))?.Token;
        }

        // Get next available Id
        public static int GetNextAvailableId()
        {
            lock (CurrentAuths)
                return CurrentAuths.Count;
        }

        public static void LoadExistingAuths()
        {
            if (File.Exists(DatabaseMiddlewareServerConfiguration.AuthenticationsList))
            {
                CurrentAuths = JsonConvert.DeserializeObject<List<Authentication>>(File.ReadAllText(DatabaseMiddlewareServerConfiguration.AuthenticationsList)) ?? new();
            }
        }

        public static void ScheduledUpdate(object? state)
        {
            CustomLogger.LoggerAccessor.LogDebug($"[AuthentificationChannel] - Refresh at {DateTime.Now}");

            lock (CurrentAuths)
            {
                string? directoryPath = Path.GetDirectoryName(DatabaseMiddlewareServerConfiguration.AuthenticationsList);

                CurrentAuths.RemoveAll(auth => auth.Expiration != null && auth.Expiration < DateTime.Now);

                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    File.WriteAllText(DatabaseMiddlewareServerConfiguration.AuthenticationsList, JsonConvert.SerializeObject(CurrentAuths, Formatting.Indented));
                }
            }
        }
    }
}
