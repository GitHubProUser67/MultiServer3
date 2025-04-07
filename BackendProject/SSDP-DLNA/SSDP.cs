﻿//Dr Gadgit from the Code project http://www.codeproject.com/Articles/893791/DLNA-made-easy-and-Play-To-for-any-device
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BackendProject.SSDP_DLNA
{
    // This class is used to broadcast a SSDP message using UDP on port 1900 and to then wait for any replies send back on the LAN
    public static class SSDP
    {
        private static Socket? UdpSocket = null;
        public static string Servers = string.Empty;
        private static string NewServer = string.Empty;
        private static Thread? THSend = null;
        private static bool Running = false;

        public static void Start()
        {
            // Stop should be called in about 12 seconds which will kill the thread
            if (Running) return;
            Running = true;
            NewServer = string.Empty;
            Thread THSend = new(SendRequest);
            THSend.Start();
            Thread TH = new(Stop);
            TH.Start();
        }

        public static void Stop()
        {
            // OK time is up so lets return our DLNA server list
            Thread.Sleep(9000);
            Running = false;
            try
            {
                Thread.Sleep(1000);
                if (UdpSocket != null)
                    UdpSocket.Close();
                if (THSend != null)
                    THSend.Abort();
            }
            catch { ;}
            if (NewServer.Length > 0) Servers = NewServer.Trim(); // Bank in our new servers
        }

        private static void SendRequest()
        {
            try { SendRequestNow(); }
            catch { ;}
        }

        private static void SendRequestNow()
        {
            // Uses UDP Multicast on 239.255.255.250 with port 1900 to send out invitations that are slow to be answered
            IPEndPoint LocalEndPoint = new(IPAddress.Any, 6000);
            IPEndPoint MulticastEndPoint = new(IPAddress.Parse("239.255.255.250"), 1900); // SSDP port
            Socket? UdpSocket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpSocket.Bind(LocalEndPoint);
            UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(MulticastEndPoint.Address, IPAddress.Any));
            UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
            UdpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
            string SearchString = "M-SEARCH * HTTP/1.1\r\nHOST:239.255.255.250:1900\r\nMAN:\"ssdp:discover\"\r\nST:ssdp:all\r\nMX:3\r\n\r\n";
            UdpSocket.SendTo(Encoding.UTF8.GetBytes(SearchString), SocketFlags.None, MulticastEndPoint);
            byte[] ReceiveBuffer = new byte[4000];
            int ReceivedBytes = 0;
            int Count = 0;
            while (Running && Count < 100)
            {
                // Keep loopping until we timeout or stop is called but do wait for at least ten seconds 
                Count++;
                if (UdpSocket.Available > 0)
                {
                    ReceivedBytes = UdpSocket.Receive(ReceiveBuffer, SocketFlags.None);
                    if (ReceivedBytes > 0)
                    {
                        string Data = Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes);
                        if (Data.ToUpper().IndexOf("LOCATION: ") > -1)
                        {
                            //ChopOffAfter is an extended string method added in Helper.cs
                            Data = Data.ChopOffBefore("LOCATION: ").ChopOffAfter(Environment.NewLine);
                            if (NewServer.ToLower().IndexOf(Data.ToLower()) == -1)
                                NewServer += " " + Data;
                        }
                    }
                }
                else
                    Thread.Sleep(100);
            }
            if (NewServer.Length > 0) Servers = NewServer.Trim(); // Bank in our new servers nice and quick with minute risk of thread error due to not locking
            UdpSocket.Close();
            THSend = null;
            UdpSocket = null;
        }
    }

    public class LinkObject
    {
        [JsonProperty("link")]
        public string? Link { get; set; }
    }
}

