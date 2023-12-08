using System.Text;

namespace QuazalServer.QNetZ
{
	public class KerberosTicket
	{
		public uint userPID;
		public byte[]? sessionKey;
		public uint serverPID;
		public byte[]? ticket;

		public KerberosTicket(uint pid, uint sPID, byte[] sesKey, byte[] ticketData)
		{
			userPID = pid;
			serverPID = sPID;
			sessionKey = sesKey;
			ticket = ticketData;
		}

		public KerberosTicket(byte[] encryptedBuffer)
		{
			// TODO: decrypt the token
		}

		public byte[]? toBuffer()
		{
			if (sessionKey != null && ticket != null)
			{
                MemoryStream m = new();
                m.Write(sessionKey, 0, 16);

                Helper.WriteU32(m, serverPID);
                Helper.WriteU32(m, (uint)ticket.Length);

                m.Write(ticket, 0, ticket.Length);

                byte[] buff = m.ToArray();
                byte[] key = Helper.DeriveKey(userPID);
                buff = Helper.Encrypt(key, buff);

                byte[] hmac = Helper.MakeHMAC(key, buff);

                m = new MemoryStream();
                m.Write(buff, 0, buff.Length);
                m.Write(hmac, 0, hmac.Length);

                return m.ToArray();
            }

			return null;
		}

		public override string ToString()
		{
			StringBuilder sb = new();
			sb.AppendLine("\t[Kerberos Ticket]");

			sb.Append("\t\t[Session Key : { ");
			if (sessionKey != null)
            {
                foreach (byte b in sessionKey)
                    sb.Append(b.ToString("X2") + " ");
            }
            sb.AppendLine("}]");

			sb.AppendLine("\t\t[Server PID : 0x" + serverPID.ToString("X8"));

			sb.Append("\t\t[Ticket Data : { ");
			if (ticket != null)
			{
                foreach (byte b in ticket)
                    sb.Append(b.ToString("X2") + " ");
            }
			sb.AppendLine("}]");

			return sb.ToString();
		}
	}
}
