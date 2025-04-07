using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace MultiSocks.Blaze.PlayerData
{
    public class Player
    {
        public static readonly object _sync = new();
        public static List<PlayerInfo> AllPlayers = new();

        public struct NETDATA
        {
            public uint IP;
            public uint PORT;
        }

        public class PlayerInfo
        {
            public int ID;
            public long UserID = 0;
            public long PlayerID = 0;
            public string? AuthString;
            public string? Auth2String;
            public string? Name;
            public string GameState;
            public string IP;
            public string? pathtoprofile;
            public TcpClient Client;
            public NetworkStream ClientStream;
            public bool isActive = true;
            public bool Update = false;
            public bool SendOffers = false;
            public bool WaitsForJoining = false;
            public NETDATA EXIP;
            public NETDATA INIP;
            public Stopwatch PingTimer;

            public struct SettingEntry
            {
                public string Key;
                public string Data;
            }

            public List<SettingEntry> Settings;
            public string timestring;
            
            public PlayerInfo(int id, TcpClient client, NetworkStream clientstream)
            {
                ID = id;
                Client = client;
                ClientStream = clientstream;
                GameState = "boot";
                PingTimer = new Stopwatch();
                PingTimer.Start();
                IP = ((IPEndPoint?)Client.Client.RemoteEndPoint).Address.ToString();
                Settings = new List<SettingEntry>();
                timestring = string.Format(@"{0:yyyy-MM-dd_HHmmss}", DateTime.Now);
            }

            public uint GetIPvalue()
            {
                byte[] byteip = ((IPEndPoint?)Client.Client.RemoteEndPoint).Address.GetAddressBytes();
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(byteip);
                return BitConverter.ToUInt32(byteip, 0);
            }

            public void UpdateSettings(string key, string data)
            {
                lock (_sync)
                {
                    SettingEntry newset = new();
                    newset.Key = key;
                    newset.Data = data;
                    bool found = false;
                    for (int i = 0; i < Settings.Count; i++)
                        if (Settings[i].Key == key)
                        {
                            Settings[i] = newset;
                            found = true;
                            break;
                        }
                    if (!found)
                        Settings.Add(newset);
                    if (!string.IsNullOrEmpty(pathtoprofile))
                    {
                        string[] lines = File.ReadAllLines(pathtoprofile);
                        List<string> result = new();
                        for (int i = 0; i < 5; i++)
                            result.Add(lines[i]);
                        foreach (SettingEntry set in Settings)
                            result.Add(set.Key + "=" + set.Data);
                        File.WriteAllLines(pathtoprofile, result.ToArray());
                    }
                    Update = true;
                }
            }

            public string GetSettings()
            {
                lock (_sync)
                {
                    string res = string.Empty;
                    foreach (SettingEntry set in Settings)
                        res += "  " + set.Key + " = " + set.Data + "\n";
                    return res;
                }
            }

            public void SetJoinWaitState(bool state)
            {
                WaitsForJoining = state;
            }

            public void SetActiveState(bool state)
            {
                isActive = state;
            }
        }
    }
}
