using Newtonsoft.Json;
using System.Text;

namespace BackendProject.EmailUtils
{
    public class JsonStorage
    {
        private List<Message> messages;

        public JsonStorage()
        {
            messages = new List<Message>();

            // Timer for scheduled updates every 24 hours
            _ = new Timer(ScheduledCleanup, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));
        }

        public void RegisterMessage(string sender, string subject, string recipient, DateTime date)
        {
            if (!messages.Any(m =>
            m.Sender == sender &&
            m.Recipient == recipient &&
            m.Subject == subject &&
            m.Date == date))
                messages.Add(new Message()
                {
                    Sender = sender,
                    Recipient = Convert.ToBase64String(Encoding.UTF8.GetBytes(recipient)),
                    Subject = subject,
                    Date = date
                });
        }

        public void RegisterMessages(List<(string, string, string, DateTime)> MailsList)
        {
            foreach ((string sender, string subject, string recipient, DateTime date) in MailsList)
            {
                if (!messages.Any(m =>
                m.Sender == sender &&
                m.Recipient == recipient &&
                m.Subject == subject &&
                m.Date == date))
                    messages.Add(new Message()
                    {
                        Sender = sender,
                        Recipient = Convert.ToBase64String(Encoding.UTF8.GetBytes(recipient)),
                        Subject = subject,
                        Date = date
                    });
            }
        }

        public string GetAllMessagesAsJson()
        {
            return JsonConvert.SerializeObject(messages, Formatting.Indented);
        }

        public void ScheduledCleanup(object? state)
        {
            messages.Clear();
        }
    }

    public class Message
    {
        public string? Sender { get; set; }
        public string? Recipient { get; set; }
        public string? Subject { get; set; }
        public DateTime Date { get; set; }
    }
}
