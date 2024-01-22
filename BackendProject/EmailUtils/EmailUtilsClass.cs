using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using EAGetMail;

namespace BackendProject.EmailUtils
{
    public class EmailUtilsClass
    {
        public bool IsListenStarted = false;
        public bool enableSSL = true;
        public ushort Flag = 1;
        public int wait = 5;
        public int SendPortNumber = 0;
        public int ReceivePortNumber = 0;
        public string strConcat = string.Empty;
        public string smtpAddress = string.Empty;
        public string imapAddress = string.Empty;
        public string emailFromAddress = string.Empty;
        public string password = string.Empty;
        public string emailToAddress = string.Empty;
        public JsonStorage storage = new();

        public EmailUtilsClass(string mailoperator, string mailreceiver, string mailoperatorpassword, string smtpAddress = "smtp.gmail.com", string imapAddress = "imap.gmail.com", int SendPortNumber = 587, int ReceivePortNumber = 993, bool ssl = true)
        {
            emailFromAddress = mailoperator;
            emailToAddress = mailreceiver;
            password = mailoperatorpassword;
            this.smtpAddress = smtpAddress;
            this.imapAddress = imapAddress;
            this.SendPortNumber = SendPortNumber;
            this.ReceivePortNumber = ReceivePortNumber;
            enableSSL = ssl;
        }

        public void SendEmail(string text, string subject)
        {
            using (MailMessage mail = new())
            {
                mail.From = new System.Net.Mail.MailAddress(emailFromAddress);
                mail.To.Add(emailToAddress);
                mail.Subject = subject;
                mail.Body = text;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new(smtpAddress, SendPortNumber))
                {
                    smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                    smtp.EnableSsl = enableSSL;
                    smtp.Send(mail);
                }
            }
        }

        // Read last Email from a list of all Emails in the Inbox
        public List<(string, string, string, DateTime)> ReadLastEmail(MailClient? oClient, MailServer oServer, GetMailInfosOptionType type)
        {
            List<(string, string, string, DateTime)> MailsList = new();

            if (oClient != null)
            {
                oClient.Connect(oServer);

                // retrieve unread/new email only
                oClient.GetMailInfosParam.Reset();
                oClient.GetMailInfosParam.GetMailInfosOptions = type;

                // Only the last (recent/latest) Email
                foreach (MailInfo info in oClient.GetMailInfos()) // Impossible to use parallel tasks, as the mail is thread sensitive.
                {
                    Mail oMail = oClient.GetMail(info);

                    if (!string.IsNullOrEmpty(oMail.TextBody.ToString()))
                    {
                        string subject = oMail.Subject.Replace(" (Trial Version)", string.Empty);

                        if (string.IsNullOrEmpty(oMail.From.Name))
                        {
                            Match match = new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b").Match(subject);

                            if (match.Success)
                                MailsList.Add((match.Value, subject, oMail.TextBody, oMail.SentDate));
                            else
                                MailsList.Add(("No Sender Found", subject, oMail.TextBody, oMail.SentDate));
                        }
                        else
                            MailsList.Add((oMail.From.Name, subject, oMail.TextBody, oMail.SentDate));

                        // Mark unread email as read, next time this email won't be retrieved again
                        if (!info.Read)
                            oClient.MarkAsRead(info, true);

                        // New Mail Exists
                        Flag = 0;
                    }
                }

                oClient.Close();
                oClient = null;
            }

            return MailsList;
        }

        public List<(string, string, string, DateTime)> ReadEmail(ServerProtocol prot, GetMailInfosOptionType type)
        {
            return ReadLastEmail(new MailClient("TryIt"), new MailServer(imapAddress, emailToAddress, password, prot)
            {
                // Enabling SSL Connection
                SSLConnection = enableSSL,
                Port = ReceivePortNumber
            }, type);
        }

        public void WaitForCommand(int sec)
        {
            while (sec >= 0)
            {
                CustomLogger.LoggerAccessor.LogDebug("[EmailUtils] - Waiting for {0} seconds for the Operator to send Command", sec);
                Thread.Sleep(sec * 1000);
                sec--;
            }
        }

        public Task MailListening(GetMailInfosOptionType type)
        {
            IsListenStarted = true;

            CustomLogger.LoggerAccessor.LogInfo("[EmailUtils] Reading mail inbox...");

            while (IsListenStarted)
            {
                try
                {
                    List<(string, string, string, DateTime)> MailsList = ReadEmail(ServerProtocol.Imap4, type);

                    switch (Flag)
                    {
                        // Flag= 0 => New Mail Arrived
                        case 0:
                            if (MailsList.Count > 0)
                                storage.RegisterMessages(MailsList);
                            break;
                        case 1:
                            // Will sleep for "wait" secs...
                            WaitForCommand(wait);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.LoggerAccessor.LogError($"[EmailUtils] - MailListening thrown an assertion: {ex}");

                    Thread.Sleep(1000);
                }

                Thread.Sleep(1);
            }

            return Task.CompletedTask;
        }
    }
}
