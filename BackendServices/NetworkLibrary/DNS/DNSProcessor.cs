using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkLibrary.DNS
{
    public class DNSProcessor
    {
        public static byte[] MakeDnsResponsePacket(byte[] Req, List<IPAddress> Ips)
        {
            if (Req.Length < 12 || Ips == null)
                return null;

            List<byte> ans = new List<byte>();
            //https://web.archive.org/web/20150326065952/http://www.ccs.neu.edu/home/amislove/teaching/cs4700/fall09/handouts/project1-primer.pdf
            //Header
            ans.AddRange(new byte[2] { Req[0], Req[1] });//ID
            if (Ips.Count == 0)
            {
                ans.AddRange(new byte[2] { 0x81, 0x83 });
                Ips.Add(IPAddress.None); // NXDOMAIN
            }
            else
                ans.AddRange(new byte[2] { 0x81, 0x80 }); //OPCODE & RCODE etc...
            ans.AddRange(new byte[2] { Req[4], Req[5] }); // QDCOUNT (copy from request)
            ans.AddRange(BitConverter.GetBytes(!BitConverter.IsLittleEndian ? EndianTools.EndianUtils.ReverseUshort((ushort)IPAddress.HostToNetworkOrder((short)Ips.Count)) : (ushort)IPAddress.HostToNetworkOrder((short)Ips.Count))); // ANCOUNT (number of answers)
            ans.AddRange(new byte[4]); // NSCOUNT & ARCOUNT (not used)
            for (int i = 12; i < Req.Length; i++) ans.Add(Req[i]);
            foreach (IPAddress ip in Ips)
            {
                byte[] addrBytes = ip.GetAddressBytes();

                ans.AddRange(new byte[2] { 0xC0, 0x0C }); // Pointer to domain name in query

                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    ans.AddRange(new byte[4] { 0x00, 0x1C, 0x00, 0x01 }); // Type AAAA (IPv6), Class IN
                    ans.AddRange(new byte[4] { 0x00, 0x00, 0x00, 0x14 }); // TTL (20 seconds)
                }
                else
                {
                    ans.AddRange(new byte[4] { 0x00, 0x01, 0x00, 0x01 }); // Type A (IPv4), Class IN
                    ans.AddRange(new byte[4] { 0x00, 0x00, 0x00, 0x14 }); // TTL (20 seconds)
                }

                ans.AddRange(new byte[2] { 0x00, (byte)addrBytes.Length }); // Data length (4 bytes for IPv4, 16 bytes for IPv6)
                ans.AddRange(addrBytes);
            }
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

        public static byte[] TrimArray(byte[] arr)
        {
            int i = arr.Length - 1;
            while (arr[i] == 0) i--;
            byte[] data = new byte[i + 1];
            Array.Copy(arr, data, i + 1);
            return data;
        }
    }
}
