namespace DatabaseMiddleware.Models
{
    public class MiddlewareUser
    {
        public int AccountId { get; set; }
        public string? AccountName { get; set; }
        public string? Password { get; set; }
        public List<string>? Roles { get; set; }

        public MiddlewareUser(int AccountId, string AccountName, string Password, List<string>? Roles = null)
        {
            this.AccountId = AccountId;
            this.AccountName = AccountName;
            this.Password = Password;
            this.Roles = Roles;
        }

        public void AddRole(string role)
        {
            if (Roles != null && !Roles.Contains(role))
                Roles.Add(role);
        }

        public void RemoveRole(string role)
        {
            if (Roles != null && Roles.Contains(role))
                Roles.Remove(role);
        }

        public bool HasRole(string role)
        {
            return Roles != null && Roles.Contains(role);
        }
    }
}
