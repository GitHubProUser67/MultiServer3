using ComponentAce.Compression.Libs.zlib;
using System.Security.Cryptography;

namespace MultiSocks.Blaze.Local_Profile
{
    public class Local_Profile
    {
        public static byte[] CreateProfile(int ID, string AUTH)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "/static/Blaze/conf/temp_profile.bin"))
            {
                byte[] tempprofile = File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/Blaze/conf/temp_profile.bin");
                MemoryStream res = new();
                MemoryStream tmpres = new();
                byte[] tmp = new byte[tempprofile.Length];
                for (int i = 0; i < tmp.Length; i++)
                    tmp[i] = tempprofile[i];
                tmp[0x16C] = (byte)(ID & 0xFF);
                tmp[0x16D] = (byte)((ID >> 8) & 0xFF);
                tmp[0x16E] = (byte)((ID >> 16) & 0xFF);
                tmp[0x16F] = (byte)((ID >> 24) & 0xFF);
                for (int i = 0; i < 0x6C; i++)
                    tmp[0xF5 + i] = (byte)AUTH[i];
                MemoryStream zipout = new();
                ZOutputStream outstream = new(zipout, false);
                outstream.Write(tmp, 0, tmp.Length);
                outstream.finish();
                outstream.Close();
                byte[] fileres = zipout.ToArray();
                int len = tmp.Length;
                tmpres.WriteByte((byte)((len >> 24) & 0xFF));
                tmpres.WriteByte((byte)((len >> 16) & 0xFF));
                tmpres.WriteByte((byte)((len >> 8) & 0xFF));
                tmpres.WriteByte((byte)(len & 0xFF));
                tmpres.Write(fileres, 0, fileres.Length);
                res.Write(new SHA1CryptoServiceProvider().ComputeHash(tmpres.ToArray()), 0, 0x14);
                res.Write(tmpres.ToArray(), 0, (int)tmpres.Length);
                return res.ToArray();
            }

            return Array.Empty<byte>();
        }

        public struct PlayerFileProfile
        {
            public long PID;
            public long UID;
            public string AUTH;
            public string AUTH2;
            public string NAME;
        }

        public static PlayerFileProfile GetFromFile(string path)
        {
            PlayerFileProfile res = new();
             string[] lines = File.ReadAllLines(path);
             foreach (string line in lines)
             {
                 string[] parts = line.Split('=');
                 if(parts.Length == 2)
                     switch (parts[0].Trim())
                     {
                         case "PID":
                             res.PID = BlazeUtils.ConvertHex(parts[1].Trim());
                             break;
                         case "UID":
                             res.UID = BlazeUtils.ConvertHex(parts[1].Trim());
                             break;
                         case "AUTH":
                             res.AUTH = parts[1].Trim();
                             break;
                         case "AUTH2":
                             res.AUTH2 = parts[1].Trim();
                             break;
                         case "DSNM":
                             res.NAME = parts[1].Trim();
                             break;
                     }
             }
            return res;
        }
    }
}
