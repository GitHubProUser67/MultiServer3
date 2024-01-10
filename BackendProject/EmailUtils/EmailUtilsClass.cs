using System.Net;
using System.Net.Mail;
using System.Text;
using EAGetMail;

namespace BackendProject.EmailUtils
{
    public class EmailUtilsClass
    {
        public bool IsListenStarted = false;
        public bool enableSSL = true;
        public int Flag = 1;
        public int wait = 5;
        public int SendPortNumber = 587;
        public string strConcat = string.Empty;
        public string smtpAddress = "smtp.gmail.com";
        public string imapAddress = "imap.gmail.com";
        public string emailFromAddress = "Operator@gmail.com";
        public string password = "password";
        public string emailToAddress = "Receiver@gmail.com";
        public JsonStorage storage = new();

        public EmailUtilsClass(string mailoperator, string mailreceiver, string mailoperatorpassword)
        {
            emailFromAddress = mailoperator;
            emailToAddress = mailreceiver;
            password = mailoperatorpassword;
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
        public List<(string, string, string, DateTime)> ReadLastEmail(MailClient oClient, MailServer oServer)
        {
            List<(string, string, string, DateTime)> MailsList = new();

            oClient.Connect(oServer);

            // retrieve unread/new email only
            oClient.GetMailInfosParam.Reset();
            oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;

            MailInfo[] infos = oClient.GetMailInfos();

            // Only the last (recent/latest) Email
            for (int i = infos.Length - 1; i > infos.Length - 2; i--)
            {
                MailInfo info = infos[i];

                Mail oMail = oClient.GetMail(info);

                if (!string.IsNullOrEmpty(oMail.TextBody.ToString()))
                {
                    MailsList.Add((oMail.From.Name, oMail.Subject, oMail.TextBody, oMail.SentDate));

                    // Mark unread email as read, next time this email won't be retrieved again
                    if (!info.Read)
                        oClient.MarkAsRead(info, true);

                    // New Mail Exists
                    Flag = 0;
                }
            }
            return MailsList;
        }

        public List<(string, string, string, DateTime)> ReadEmail(ServerProtocol prot)
        {
            return ReadLastEmail(new MailClient("TryIt"), new MailServer(imapAddress, emailToAddress, password, prot)
            {
                // Enabling SSL Connection
                SSLConnection = true,
                Port = 993
            });
        }

        public void GmailC2Prompt(string command)
        {
            CustomLogger.LoggerAccessor.LogInfo("[EmailUtils] - [GmailC2] IN->Email: {0}", command);
        }

        public void WaitForCommand(int sec)
        {
            while (sec >= 0)
            {
                CustomLogger.LoggerAccessor.LogInfo("[EmailUtils] - Waiting for {0} seconds for the Operator to send Command", sec);
                Thread.Sleep(sec * 1000);
                sec--;
            }
        }

        public Task MailListening()
        {
            StringBuilder strInput = new();

            IsListenStarted = true;

            while (IsListenStarted)
            {
                try
                {
                    CustomLogger.LoggerAccessor.LogInfo("[EmailUtils] Reading MAil Inbox...");

                    List<(string, string, string, DateTime)> MailsList = ReadEmail(ServerProtocol.Imap4);

                    // Flag= 0 => New Mail Arrived
                    if (Flag == 0)
                    {
                        if (MailsList.Count > 0)
                            storage.RegisterMessages(MailsList);
                    }
                    else if (Flag == 1)
                    {
                        // Will sleep for "wait" secs...
                        WaitForCommand(wait);
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.LoggerAccessor.LogError($"[EmailUtils] - MailListening thrown an assertion: {ex}");
                }
            }

            return Task.CompletedTask;
        }
    }
}
