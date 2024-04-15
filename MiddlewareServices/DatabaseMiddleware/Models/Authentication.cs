namespace DatabaseMiddleware.Models
{
    public class Authentication
    {
        public MiddlewareUser User { get; set; }
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }

        public Authentication(MiddlewareUser user)
        {
            User = user;
            Token = Guid.NewGuid().ToString("N");
            if (user.HasRole("user"))
                Expiration = DateTime.Now.AddDays(1); // Adding 1 day to the current time
        }
    }
}
