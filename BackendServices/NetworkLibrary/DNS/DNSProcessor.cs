using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkLibrary.DNS
{
    public class DNSProcessor
    {
        public static byte[] MakeDnsResponsePacket(byte[] Req, IPAddress Ip)
        {
            if (Req.Length < 12)
                return null;

            List<byte> ans = new List<byte>();
            //https://web.archive.org/web/20150326065952/http://www.ccs.neu.edu/home/amislove/teaching/cs4700/fall09/handouts/project1-primer.pdf
            //Header
            ans.AddRange(new byte[] { Req[0], Req[1] });//ID
            if (Ip == IPAddress.None)
                ans.AddRange(new byte[] { 0x81, 0x83 });
            else
                ans.AddRange(new byte[] { 0x81, 0x80 }); //OPCODE & RCODE etc...
            ans.AddRange(new byte[8] { Req[4], Req[5], Req[4], Req[5], 0x00, 0x00, 0x00, 0x00 }); //QDCount/ANCount/NSCount & ARCount
            for (int i = 12; i < Req.Length; i++) ans.Add(Req[i]);
            ans.AddRange(new byte[] { 0xC0, 0xC });
            if (Ip.AddressFamily == AddressFamily.InterNetworkV6)
                ans.AddRange(new byte[] { 0, 0x1c, 0, 1, 0, 0, 0, 0x14, 0, 0x10 }); //20 seconds, 0x10 is ipv6 length
            else
                ans.AddRange(new byte[] { 0, 1, 0, 1, 0, 0, 0, 0x14, 0, 4 });
            ans.AddRange(Ip.GetAddressBytes());
            return ans.ToArray();
        }

        public static List<string> GetDnsName(byte[] Req)
        {
            int reqLength = Req.Length;
            if (reqLength < 13)
                return null;

            int type = Req[2] >> 3 & 0xF;
            List<string> addr = new List<string>();
            if (type == 0)
            {
                int i = 12;
                int lenght = Req[12];
                int reqLengthMinusOne = reqLength - 1;

                while (lenght > 0)
                {
                    int incrementedIndex = i + 1;
                    if (reqLengthMinusOne >= incrementedIndex)
                    {
                        byte[] tmp = new byte[i + lenght];
                        Buffer.BlockCopy(Req, incrementedIndex, tmp, 0, lenght);
                        string partialaddr = TrimByteArraytoString(tmp);
                        if (partialaddr != null) addr.Add(partialaddr);
                        i += lenght + 1;
                        lenght = Req[i];
                    }
                }
            }
            return addr;
        }

        /// <summary>
        /// Trim a byte array to a string result.
        /// <para>Coupe un tableau de bytes et convertis le résultat en string.</para>
        /// </summary>
        /// <param name="str">The input byte array to trim.</param>
        /// <returns>A string.</returns>
        private static string TrimByteArraytoString(byte[] str)
        {
            int i = str.Length - 1;
            while (str[i] == 0)
            {
                Array.Resize(ref str, i);
                i -= 1;
            }
            //if (res.ToLower() == "www") return null; Some sites do not work without www
            /* else*/
            return Encoding.ASCII.GetString(str);
        }
    }
}
