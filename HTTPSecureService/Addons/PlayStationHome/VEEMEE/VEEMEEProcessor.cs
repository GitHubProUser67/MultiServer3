using System.Text;
using System.Xml.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEProcessor
    {
        private static string Sha1Hash(string data)
        {
            var sha1 = SHA1.Create();

            byte[] hashBytes = sha1.ComputeHash(Misc.Combinebytearay(Encoding.UTF8.GetBytes("veemeeHTTPRequ9R3UMWDAT8F3*#@&$^"), Encoding.UTF8.GetBytes(data)));

            sha1.Dispose();

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public static byte[] sign(string jsonData)
        {
            try
            {
                string formattedJson = JToken.Parse(jsonData.Replace("\n", "")).ToString(Newtonsoft.Json.Formatting.None);

                string hash = Sha1Hash(formattedJson).ToUpper();

                JToken token = JToken.Parse(formattedJson);

                if (token.Type == JTokenType.Object)
                {
                    JObject obj = (JObject)token;
                    obj["hash"] = hash;
                    formattedJson = obj.ToString(Newtonsoft.Json.Formatting.None);
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray array = (JArray)token;
                    JObject obj = new();
                    obj["hash"] = hash;
                    array.Add(obj);
                    formattedJson = array.ToString(Newtonsoft.Json.Formatting.None);
                }

                return Encoding.UTF8.GetBytes(formattedJson);
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[VEEMEE] : Exception in sign, file might be incorrect : {ex}");

                return Encoding.UTF8.GetBytes("ERROR in sign");
            }
        }

        public static string UpdateSlot(string directoryPath, string psn_id, int slot_num, bool removemode)
        {
            bool found = false;

            if (slot_num != 0 && removemode)
            {
                if (File.Exists(directoryPath + $"{slot_num}.xml"))
                {
                    XDocument slotDocument = XDocument.Load(directoryPath + $"{slot_num}.xml");
                    XElement slotElement = slotDocument.Root;

                    if (slotElement != null)
                    {
                        XElement slotXElement = slotElement.Element("slot");
                        XElement expirationXElement = slotElement.Element("expiration");

                        if (slotXElement != null && expirationXElement != null)
                        {
                            string slotValue = slotXElement.Value;

                            if (slotValue == psn_id)
                            {
                                slotXElement.Value = "unnocupied";
                                expirationXElement.Value = "01/01/1970 00:00:00";
                                slotDocument.Save(directoryPath + $"{slot_num}.xml");

                                return "true";
                            }
                        }
                    }
                }
            }

            if (slot_num == 0)
            {
                string[] slotFiles;

                slotFiles = Directory.GetFiles(directoryPath, "*.xml");
                Array.Sort(slotFiles, new SlotFileComparer()); // Sort the array using the custom comparer

                foreach (string slotFile in slotFiles)
                {
                    if (File.Exists(slotFile) && !removemode)
                    {
                        XDocument slotDocument = XDocument.Load(slotFile);
                        XElement slotElement = slotDocument.Root;

                        if (slotElement != null)
                        {
                            XElement slotXElement = slotElement.Element("slot");
                            XElement expirationXElement = slotElement.Element("expiration");

                            if (slotXElement != null && expirationXElement != null)
                            {
                                string slotValue = slotXElement.Value;
                                string expirationDate = expirationXElement.Value;

                                if (expirationDate != null)
                                {
                                    DateTime expirationDatefromxml = DateTime.Parse(expirationDate);

                                    if (slotValue == psn_id && DateTime.Now < expirationDatefromxml)
                                        return Path.GetFileNameWithoutExtension(slotFile);

                                    if (slotValue == "unnocupied" || slotValue != "unnocupied" && expirationDatefromxml <= DateTime.Now)
                                    {
                                        slotXElement.Value = psn_id;
                                        expirationXElement.Value = DateTime.Now.AddSeconds(45).ToString();
                                        slotDocument.Save(slotFile);

                                        return Path.GetFileNameWithoutExtension(slotFile);
                                    }
                                }
                            }
                        }
                    }
                    else if (File.Exists(slotFile) && removemode)
                    {
                        XDocument slotDocument = XDocument.Load(slotFile);
                        XElement slotElement = slotDocument.Root;

                        if (slotElement != null)
                        {
                            XElement slotXElement = slotElement.Element("slot");
                            XElement expirationXElement = slotElement.Element("expiration");

                            if (slotXElement != null && expirationXElement != null)
                            {
                                string slotValue = slotXElement.Value;

                                if (slotValue == psn_id)
                                {
                                    slotXElement.Value = "unnocupied";
                                    expirationXElement.Value = "01/01/1970 00:00:00";
                                    slotDocument.Save(slotFile);

                                    found = true;
                                }
                            }
                        }
                    }
                }
            }

            if (!found)
                return "false";
            else
                return "true";
        }
    }

    public class VEEMEELoginCounter
    {
        private Dictionary<string, int> loginCounts;

        public VEEMEELoginCounter()
        {
            loginCounts = new Dictionary<string, int>();
        }

        public void ProcessLogin(string username)
        {
            if (loginCounts.ContainsKey(username))
                loginCounts[username]++;
            else
                loginCounts.Add(username, 1);
        }

        public int GetLoginCount(string username)
        {
            if (loginCounts.ContainsKey(username))
                return loginCounts[username];

            return 0;
        }
    }

    public static class VEEMEEProfileManager
    {
        public static byte[] ReadProfile(string psnid, string product, string hex, string salt)
        {
            if (hex == null || salt == null)
                return Encoding.UTF8.GetBytes("No Access.");

            string reader = null;

            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/User_Profiles/{psnid}.json"))
                reader = Encoding.UTF8.GetString(HTTPService.FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/User_Profiles/{psnid}.json", HTTPService.HTTPPrivateKey.HTTPPrivatekey));
            else
                reader = Encoding.UTF8.GetString(HTTPService.FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/default_profile.json", HTTPService.HTTPPrivateKey.HTTPPrivatekey));

            if (reader == null)
                reader = "";

            return VEEMEEProcessor.sign(reader);
        }

        public static async Task<byte[]> WriteProfile(string psnid, string profile)
        {
            await HTTPService.FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/User_Profiles/{psnid}.json", HTTPService.HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(profile), true);

            return VEEMEEProcessor.sign(profile);
        }
    }

    // Custom comparer to sort file names numerically
    public class SlotFileComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int slotNumberX = int.Parse(Path.GetFileNameWithoutExtension(x));
            int slotNumberY = int.Parse(Path.GetFileNameWithoutExtension(y));

            return slotNumberX.CompareTo(slotNumberY);
        }
    }
}
