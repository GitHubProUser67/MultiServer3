using System.Linq;
using System;
using System.IO;
using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using NetworkLibrary.HTTP;
using static WebAPIService.OHS.UserCounter;
using HashLib;

namespace WebAPIService.OHS
{
    public class User
    {
        public static string ClearEntry(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            bool isCleared = false;
            string dataforohs = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    JToken Token = JToken.Parse(dataforohs);

                    object user = Utils.JtokenUtils.GetValueFromJToken(Token, "user");

                    Directory.CreateDirectory(directorypath + $"/User_Profiles");

                    string profiledatastring = directorypath + $"/User_Profiles/{user}.json";

                    if (File.Exists(profiledatastring))
                    {
                        string profiledata = File.ReadAllText(profiledatastring);

                        if (!string.IsNullOrEmpty(profiledata))
                        {
                            JObject jObject = JObject.Parse(profiledata);

                            if (jObject != null)
                            {
                                isCleared = true;

                                jObject.DescendantsAndSelf().FirstOrDefault(t => t.Path == (string)Utils.JtokenUtils.GetValueFromJToken(Token, "key"))?.Remove();

                                File.WriteAllText(profiledatastring, jObject.ToString(Formatting.Indented));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (!isCleared)
                    return null;
                else
                    return "{ }";
            }
            else
            {
                if (!isCleared)
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ }} }}", game);
            }

            return dataforohs;
        }

        public static string Set(byte[] PostData, string ContentType, string directorypath, string batchparams, bool global, int game)
        {
            string dataforohs = null;
            string output = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    JToken Token = JToken.Parse(dataforohs);

                    object value = Utils.JtokenUtils.GetValueFromJToken(Token, "value");

                    object key = Utils.JtokenUtils.GetValueFromJToken(Token, "key");

                    object user = Utils.JtokenUtils.GetValueFromJToken(Token, "user");

                    Directory.CreateDirectory(directorypath + $"/User_Profiles");

                    if (!global)
                    {
                        string profiledatastring = directorypath + $"/User_Profiles/{user}.json";

                        if (File.Exists(profiledatastring))
                        {
                            string profiledata = File.ReadAllText(profiledatastring);

                            if (!string.IsNullOrEmpty(profiledata))
                            {
                                JObject jObject = JObject.Parse(profiledata);

                                if (jObject != null)
                                {
                                    // Check if the key name already exists in the JSON
                                    JToken existingKey = jObject.DescendantsAndSelf().FirstOrDefault(t => t.Path == (string)key);

                                    if (existingKey != null && value != null)
                                        // Update the value of the existing key
                                        existingKey.Replace(JToken.FromObject(value));
                                    else if (key != null && value != null)
                                    {
                                        JToken KeyEntry = jObject["key"];

                                        if (KeyEntry != null)
                                            // Step 2: Add a new entry to the "Key" object
                                            KeyEntry[key] = JToken.FromObject(value);
                                    }

                                    File.WriteAllText(profiledatastring, jObject.ToString(Formatting.Indented));
                                }
                            }
                        }
                        else if (key != null)
                        {
                            string keystring = key.ToString();

                            if (keystring != null && user != null && value != null)
                            {
                                // Create a new profile with the key field
                                OHSUserProfile newProfile = new OHSUserProfile
                                {
                                    user = user.ToString(),
                                    key = new JObject { { keystring, JToken.FromObject(value) } }
                                };

                                File.WriteAllText(profiledatastring, JsonConvert.SerializeObject(newProfile));
                            }
                        }
                    }
                    else
                    {
                        string globaldatastring = directorypath + "/Global.json";

                        if (File.Exists(globaldatastring))
                        {
                            string globaldata = File.ReadAllText(globaldatastring);

                            if (!string.IsNullOrEmpty(globaldata))
                            {
                                JObject jObject = JObject.Parse(globaldata);

                                if (jObject != null && value != null)
                                {
                                    // Check if the key name already exists in the JSON
                                    JToken existingKey = jObject.DescendantsAndSelf().FirstOrDefault(t => t.Path == (string)key);

                                    if (existingKey != null)
                                        // Update the value of the existing key
                                        existingKey.Replace(JToken.FromObject(value));
                                    else if (key != null)
                                    {
                                        JToken KeyEntry = jObject["key"];

                                        if (KeyEntry != null)
                                            // Step 2: Add a new entry to the "Key" object
                                            KeyEntry[key] = JToken.FromObject(value);
                                    }

                                    File.WriteAllText(globaldatastring, jObject.ToString(Formatting.Indented));
                                }
                            }
                        }
                        else if (key != null)
                        {
                            string keystring = key.ToString();

                            if (keystring != null && value != null)
                            {
                                // Create a new profile with the key field
                                OHSGlobalProfile newProfile = new OHSGlobalProfile
                                {
                                    Key = new JObject { { keystring, JToken.FromObject(value) } }
                                };

                                File.WriteAllText(globaldatastring, JsonConvert.SerializeObject(newProfile));
                            }
                        }
                    }

                    if (value != null)
                        output = LuaUtils.ConvertJTokenToLuaTable(JToken.FromObject(value), true);
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return null;
                else
                    return output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string Get_All(byte[] PostData, string ContentType, string directorypath, string batchparams, bool global, int game)
        {
            string dataforohs = string.Empty;
            string output = string.Empty;
            string projectName = string.Empty;

            if (string.IsNullOrEmpty(batchparams))
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        projectName = data.GetParameterValue("project");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    // Parsing the JSON string
                    JObject jsonObject = JObject.Parse(dataforohs);

                    if (!global)
                    {
                        // Getting the value of the "user" field
                        dataforohs = (string)jsonObject["user"];

                        if (!string.IsNullOrEmpty(dataforohs) && File.Exists(directorypath + $"/User_Profiles/{dataforohs}.json"))
                        {
                            string tempreader = File.ReadAllText(directorypath + $"/User_Profiles/{dataforohs}.json");

                            if (!string.IsNullOrEmpty(tempreader))
                            {
                                // Parse the JSON string to a JObject
                                jsonObject = JObject.Parse(tempreader);

                                // Check if the "key" property exists and if it is an object
                                if (jsonObject.TryGetValue("key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                    // Convert the JToken to a Lua table-like string
                                    output = LuaUtils.ConvertJTokenToLuaTable(keyValueToken, true); // Nested, because we expect the array instead.
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(directorypath + $"/Global.json"))
                        {
                            string tempreader = File.ReadAllText(directorypath + $"/Global.json");

                            if (!string.IsNullOrEmpty(tempreader))
                            {
                                // Parse the JSON string to a JObject
                                jsonObject = JObject.Parse(tempreader);	

                                // Check if the "key" property exists and if it is an object
                                if (jsonObject.TryGetValue("key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                    // Convert the JToken to a Lua table-like string
                                    output = LuaUtils.ConvertJTokenToLuaTable(keyValueToken, true); // Nested, because we expect the array instead.
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return "{ }";
                else
                    return output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string Get(byte[] PostData, string ContentType, string directorypath, string batchparams, bool global, int game)
        {
            string dataforohs = null;
            string output = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    // Parsing the JSON string
                    JObject jsonObject = JObject.Parse(dataforohs);
                    string ohsKey = (string)jsonObject["key"];

                    if (!string.IsNullOrEmpty(ohsKey))
                    {
                        if (!global)
                        {
                            // Getting the value of the "user" field
                            string ohsUserName = (string)jsonObject["user"];

                            if (!string.IsNullOrEmpty(ohsUserName) && File.Exists(directorypath + $"/User_Profiles/{ohsUserName}.json"))
                            {
                                string userprofile = File.ReadAllText(directorypath + $"/User_Profiles/{ohsUserName}.json");

                                if (!string.IsNullOrEmpty(userprofile))
                                {
                                    // Parse the JSON string to a JObject
                                    jsonObject = JObject.Parse(userprofile);

                                    // Check if the "key" property exists and if it is an object
                                    if (jsonObject.TryGetValue("key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                    {
                                        if (((JObject)keyValueToken).TryGetValue(ohsKey, out JToken wishlistToken))
                                            output = LuaUtils.ConvertJTokenToLuaTable(wishlistToken, true);
                                    }
                                }
                            }
                            else
                            {
                                switch (ohsKey)
                                {
                                    case "timeStamp":
                                        if (directorypath.Contains("casino"))
                                            output = "nil";
                                        break;
                                    case "GameState":
                                        if (directorypath.Contains("shooter_game"))
                                            output = "{ [\"currentLevel\"] = 1, [\"currentMaxLevel\"] = 50, [\"items\"] = {\t{ type = \"guns\"  \t\t , name=\"repeater\"\t\t\t, level=1 , inUse = false }\r\n" +
                                                ",\t{ type = \"tank\"  \t\t , name=\"plating1\"\t\t\t, level=0 , inUse = false }\r\n" +
                                                ",\t{ type = \"thrusters\" , name=\"HoverFan\"\t\t\t, level=1 , inUse = false }\r\n" +
                                                ",\t{ type = \"thrusters\" , name=\"HoverFan\"\t\t\t, level=1 , inUse = false }\r\n" +
                                                ",\t{ type = \"thrusters\" , name=\"HoverFan\"\t\t\t, level=1 , inUse = false }\r\n" +
                                                "}, [\"loadout\"] = { { mount='thrusters' , slot='left'  \t\t\t ,name=\"HoverFan\", level=1 }\r\n" +
                                                ", { mount='thrusters' , slot='right' \t\t\t ,name=\"HoverFan\", level=1 }\r\n" +
                                                ", { mount='thrusters' , slot='rear'  \t\t\t ,name=\"HoverFan\", level=1 }\r\n" +
                                                ", { mount='guns'      , slot=1       \t\t\t ,name=\"repeater\", level=1 }\r\n" +
                                                ", { mount='guns'      , slot=2       \t\t\t ,name=\"none\"    , level=0 }\r\n" +
                                                ", { mount='missiles'  , slot=1       \t\t\t ,name=\"none\"\t\t , level=0 }\r\n" +
                                                ", { mount='missiles'  , slot=2       \t\t\t ,name=\"none\"\t\t , level=0 }\r\n" +
                                                ", { mount='counters'  , slot=1       \t\t\t ,name=\"none\"    , level=0 }\r\n" +
                                                ", { mount='counters'  , slot=2       \t\t\t ,name=\"none\"    , level=0 }\r\n" +
                                                ", { mount='burner'    , slot=1       \t\t\t ,name=\"none\"    , level=0 }\r\n" +
                                                ", { mount='tank'      , slot=1       \t\t\t ,name=\"plating1\", level=0 }\r\n" +
                                                ", { mount='module'    , slot='fireRateAug' ,name=\"none\" \t , level=0 }\r\n" +
                                                ", { mount='module'    , slot='handlingAug' ,name=\"none\" \t , level=0 }\r\n" +
                                                ", { mount='module'    , slot='engineAug'\t ,name=\"none\" \t , level=0 }\r\n" +
                                                ", { mount='module'    , slot='targeting'\t ,name=\"none\"    , level=0 }\r\n" +
                                                ", { mount='module'    , slot='ammoStore'\t ,name=\"none\" \t , level=0 }\r\n" +
                                                ", { mount='module'    , slot='armour'\t\t\t ,name=\"none\" \t , level=0 }\r\n" +
                                                ", { mount='module'    , slot='autoRepair'\t ,name=\"none\" \t , level=0 }\r\n" +
                                                ", { mount='module'    , slot='heatSink'\t\t ,name=\"none\" \t , level=0 }\r\n" +
                                                "}, [\"scores\"] = { } }";
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (File.Exists(directorypath + $"/Global.json"))
                            {
                                string globaldata = File.ReadAllText(directorypath + $"/Global.json");

                                if (!string.IsNullOrEmpty(globaldata))
                                {
                                    // Parse the JSON string to a JObject
                                    jsonObject = JObject.Parse(globaldata);

                                    // Check if the "key" property exists and if it is an object
                                    if (jsonObject.TryGetValue("key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                    {
                                        if (((JObject)keyValueToken).TryGetValue(ohsKey, out JToken wishlistToken))
                                            output = LuaUtils.ConvertJTokenToLuaTable(wishlistToken, true);
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(ohsKey))
                            {
                                switch (ohsKey)
                                {
                                    case "vickie_version":
                                        output = "{ [\"vickie_version\"] = 7 }";
                                        break;
                                    case "global_data":
                                        if (directorypath.Contains("Uncharted3"))
                                            output = "{ [\"unlocks\"] = \"WAVE3\",[\"community_score\"] = 1,[\"challenges\"] = { [\"accuracy\"] = 1 } }";
                                        else if (directorypath.Contains("Halloween2012"))
                                            output = "{ [\"unlocks\"] = { [\"dance\"] = { [\"open\"] = \"20230926113000\", [\"closed\"] = \"20990926163000\" }, [\"limbo\"] = { [\"open\"] = \"20230926113000\"," +
                                                " [\"closed\"] = \"20990926163000\" }, [\"hemlock\"] = { [\"open\"] = \"20230926113000\", [\"closed\"] = \"20990926163000\" }, [\"wolfsbane\"] = { [\"open\"] =" +
                                                " \"20230926113000\", [\"closed\"] = \"20990926163000\" } } }";
                                        break;
                                    case "unlock_data":
                                        if (directorypath.Contains("killzone_3"))
                                            output = "{ [\"wave_1\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = true }, [\"wave_2\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = true }," +
                                                " [\"wave_3\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = true } }, { [\"wave_1\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = true }, [" +
                                                "\"wave_2\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = true }, [\"wave_3\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = true } }, { [\"" +
                                                "wave_1\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = true }, [\"wave_2\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = true }, [\"wave_3" +
                                                "\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = true } }";
                                        break;
                                    case "entries":
                                        if (directorypath.Contains("LockwoodTokens"))
                                            output = "\"" + string.Join("|", LkwdConstants.TokensUUIDs.Keys.ToList()) + "\"";
                                        break;
                                    case "DragonStatue":
                                        if (directorypath.Contains("LKWDShowEggs"))
                                            output = "\"999999999999\"";
                                        break;
                                    case "maxSceaPlazaReward":
										output = "{ [\"maxSceaPlazaReward\"] = 5 }";
                                        break;
                                    default:
                                        if (directorypath.Contains("gift_machine"))
                                        {
                                            string giftMachineEntriesDirectoryPath = directorypath + "Gift_Machine_Entries";
                                            string giftMachineEntryPath = giftMachineEntriesDirectoryPath + $"/{ohsKey}.txt";

                                            Directory.CreateDirectory(giftMachineEntriesDirectoryPath);

                                            if (File.Exists(giftMachineEntryPath))
                                                output = $"\"{File.ReadAllText(giftMachineEntryPath)}\"";
                                            else
                                            {
                                                LoggerAccessor.LogWarn($"[User] - Lockwood Gift Machine not found a UUID entry for item: {ohsKey} at path: {giftMachineEntryPath}");
                                                output = "\"\"";
                                            }
                                        }
                                        else
                                            LoggerAccessor.LogWarn($"[User] - Unknown Global entry: {ohsKey} , breakage is to be expected!");
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return "{ }";
                else
                    return output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string GetMany(byte[] PostData, string ContentType, string directorypath, string batchparams, bool global, int game)
        {
            string dataforohs = null;
            string output = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    // Parsing the JSON string
                    JObject jsonObject = JObject.Parse(dataforohs);

                    // Getting the value of the "user" field as an array
                    JArray usersArray = (JArray)jsonObject["users"];

                    string ohsKey = (string)jsonObject["key"];

                    if (usersArray != null && !string.IsNullOrEmpty(ohsKey))
                    {
                        output = "{"; // Initialize output string

                        foreach (var userToken in usersArray)
                        {
                            string ohsUserName = userToken.Value<string>();

                            try
                            {
                                if (!string.IsNullOrEmpty(ohsUserName) && File.Exists(directorypath + $"/User_Profiles/{ohsUserName}.json"))
                                {
                                    string userprofile = File.ReadAllText(directorypath + $"/User_Profiles/{ohsUserName}.json");

                                    if (!string.IsNullOrEmpty(userprofile))
                                    {
                                        // Parse the JSON string to a JObject
                                        jsonObject = JObject.Parse(userprofile);

                                        // Check if the "key" property exists and if it is an object
                                        if (jsonObject.TryGetValue("key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                        {
                                            if (((JObject)keyValueToken).TryGetValue(ohsKey, out JToken wishlistToken))
                                            {
                                                string outputOriginal = LuaUtils.ConvertJTokenToLuaTable(wishlistToken, true);

                                                if (ohsUserName == usersArray.Last().ToString())
                                                    output += $"[\"{ohsUserName}\"] = {outputOriginal}";
                                                else
                                                    output += $"[\"{ohsUserName}\"] = {outputOriginal} , ";
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                LoggerAccessor.LogWarn($"[OHS] user/getmany/ caught error from '{ohsUserName}' with exception {e}");
                            }
                        }

                        output += '}';
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return "{ }";
                else
                    return output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string Gets(byte[] PostData, string ContentType, string directorypath, string batchparams, bool global, int game)
        {
            string dataforohs = null;
            string output = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    // Parsing the JSON string
                    JObject globalProfile = JObject.Parse(dataforohs);

                    // Getting the value of the "user" field
                    dataforohs = (string)globalProfile["user"];
                    string[] keys = globalProfile["keys"]?.ToObject<string[]>();

                    if (!global)
                    {
                        if (keys != null && !string.IsNullOrEmpty(dataforohs) && File.Exists(directorypath + $"/User_Profiles/{dataforohs}.json"))
                        {
                            string userprofile = File.ReadAllText(directorypath + $"/User_Profiles/{dataforohs}.json");

                            if (!string.IsNullOrEmpty(userprofile))
                            {
                                // Check if the "key" property exists and if it is an object
                                if (JObject.Parse(userprofile).TryGetValue("key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                {
                                    JObject keyObject = (JObject)keyValueToken;

                                    StringBuilder st = new StringBuilder("{ ");

                                    foreach (string key in keys)
                                    {
                                        // Check if the specific key exists in the JObject
                                        if (keyObject.TryGetValue(key, out JToken valueToken))
                                        {
                                            if (st.Length != 2)
                                                st.Append($", [\"{key}\"] = " + LuaUtils.ConvertJTokenToLuaTable(valueToken, false));
                                            else
                                                st.Append($"[\"{key}\"] = " + LuaUtils.ConvertJTokenToLuaTable(valueToken, false));
                                        }
                                    }

                                    st.Append(" }");
                                    output = st.ToString();
                                }
                            }
                        }
                    }
                    else if (keys != null)
                    {
                        if (File.Exists(directorypath + $"/Global.json"))
                        {
                            string globaldata = File.ReadAllText(directorypath + $"/Global.json");

                            if (!string.IsNullOrEmpty(globaldata))
                            {
                                // Check if the "key" property exists and if it is an object
                                if (JObject.Parse(globaldata).TryGetValue("key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                {
                                    JObject keyObject = (JObject)keyValueToken;

                                    StringBuilder st = new StringBuilder("{ ");

                                    foreach (string key in keys)
                                    {
                                        // Check if the specific key exists in the JObject
                                        if (keyObject.TryGetValue(key, out JToken valueToken))
                                        {
                                            if (st.Length != 2)
                                                st.Append($", [\"{key}\"] = " + LuaUtils.ConvertJTokenToLuaTable(valueToken, false));
                                            else
                                                st.Append($"[\"{key}\"] = " + LuaUtils.ConvertJTokenToLuaTable(valueToken, false));
                                        }
                                    }

                                    st.Append(" ]");
                                    output = st.ToString();
                                }
                            }
                        }
                        else if (keys.Contains("heatmap_samples_to_send") && keys.Contains("heatmap_sample_period"))
                            output = "{[\"heatmap_samples_to_send\"] = 1, [\"heatmap_sample_period\"] = 5}";
                        else if (directorypath.Contains("LockwoodTokens"))
                        {
                            StringBuilder tokenSt = new StringBuilder("{");

                            foreach (string uuid in keys)
                            {
                                if (LkwdConstants.TokensUUIDs.ContainsKey(uuid))
                                {
                                    if (tokenSt.Length != 1)
                                        tokenSt.Append($",[\"Lockwood Token Pack {LkwdConstants.TokensUUIDs[uuid]}\"] = \"{uuid}\"");
                                    else
                                        tokenSt.Append($"[\"Lockwood Token Pack {LkwdConstants.TokensUUIDs[uuid]}\"] = \"{uuid}\"");
                                }
                            }

                            output = tokenSt.ToString() + '}';
                        }
                        else if (directorypath.Contains("gift_machine"))
                        {
                            string giftMachineEntriesDirectoryPath = directorypath + "Gift_Machine_Entries";
                            StringBuilder uuidListSt = new StringBuilder("{");

                            Directory.CreateDirectory(giftMachineEntriesDirectoryPath);

                            foreach (string ohsKey in keys)
                            {
                                string giftMachineEntryPath = giftMachineEntriesDirectoryPath + $"/{ohsKey}.txt";

                                if (File.Exists(giftMachineEntryPath))
                                {
                                    if (uuidListSt.Length != 1)
                                        uuidListSt.Append($",[\"{ohsKey}\"] = \"{File.ReadAllText(giftMachineEntryPath)}\"");
                                    else
                                        uuidListSt.Append($"[\"{ohsKey}\"] = \"{File.ReadAllText(giftMachineEntryPath)}\"");
                                }
                                else
                                {
                                    LoggerAccessor.LogWarn($"[User] - Lockwood Gift Machine not found a UUID entry for item: {ohsKey} at path: {giftMachineEntryPath}");

                                    if (uuidListSt.Length != 1)
                                        uuidListSt.Append($",[\"{ohsKey}\"] = \"\"");
                                    else
                                        uuidListSt.Append($"[\"{ohsKey}\"] = \"\"");
                                }
                            }

                            output = uuidListSt.ToString() + '}';
                        }
                        else if (directorypath.Contains("lockwood_life"))
                        {
                            StringBuilder resultListSt = new StringBuilder("{");

                            foreach (string ohsKey in keys)
                            {
                                if (ohsKey.Equals("NUM_LEVELS"))
                                {
                                    if (resultListSt.Length != 1)
                                        resultListSt.Append($",[\"{ohsKey}\"] = 99");
                                    else
                                        resultListSt.Append($"[\"{ohsKey}\"] = 99");
                                }
                                else if (ohsKey.Equals("SCENE_LIST"))
                                {
                                    StringBuilder sceneListSt = new StringBuilder("{");

                                    foreach (string sceneKey in LkwdConstants.LockwoodLifeSceneList)
                                    {
                                        if (sceneListSt.Length != 1)
                                            sceneListSt.Append($",\"{sceneKey}\"");
                                        else
                                            sceneListSt.Append($"\"{sceneKey}\"");
                                    }

                                    if (resultListSt.Length != 1)
                                        resultListSt.Append($",[\"{ohsKey}\"] = {sceneListSt.ToString()}}}");
                                    else
                                        resultListSt.Append($"[\"{ohsKey}\"] = {sceneListSt.ToString()}}}");
                                }
                            }

                            output = resultListSt.ToString() + '}';
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return "{ }";
                else
                    return output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ }} }}", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string User_Id(byte[] PostData, string ContentType, string batchparams, int game)
        {
            string dataforohs = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                    dataforohs = (string)JObject.Parse(dataforohs)["user"];
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(dataforohs))
                    return null;
                else
                    return UniqueNumberGenerator.GenerateUniqueNumber(dataforohs).ToString();
            }
            else
            {
                if (string.IsNullOrEmpty(dataforohs))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {UniqueNumberGenerator.GenerateUniqueNumber(dataforohs)} }}", game);
            }

            return dataforohs;
        }

        public static string User_GetWritekey(byte[] PostData, string ContentType, string batchparams, int game)
        {
            string dataforohs = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    // Parsing the JSON string
                    JObject jsonObject = JObject.Parse(dataforohs);

                    dataforohs = GetFirstEightCharacters(CalculateMD5HashToHexadecimal((string)jsonObject["user"]));
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(dataforohs))
                    return null;
                else
                    return "{ [\"writeKey\"] = \"" + dataforohs + "\" }";
            }
            else
            {
                if (string.IsNullOrEmpty(dataforohs))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"writeKey\"] = \"{dataforohs}\" }} }}", game);
            }

            return dataforohs;
        }

        public static string CalculateMD5HashToHexadecimal(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            byte[] hashBytes = NetHasher.ComputeMD5(Encoding.UTF8.GetBytes(input));

            // Convert the byte array to a hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }

        public static string GetFirstEightCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            if (input.Length >= 8)
                return input.Substring(0, 8);

            else
                // If the input is less than 8 characters, you can handle it accordingly
                // For simplicity, let's just pad with zeros in this case
                return input.PadRight(8, '0');
        }

        public class OHSGlobalProfile
        {
            public object Key { get; set; }
        }

        public static class UniqueNumberGenerator
        {
            // Function to generate a unique number based on a string using MD5
            public static int GenerateUniqueNumber(string inputString)
            {
                byte[] MD5Data = NetHasher.ComputeMD5(Encoding.UTF8.GetBytes("0HS0000000000000A" + inputString));

                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(MD5Data);

                // To get a small integer within Lua int bounds, take the least significant 16 bits of the hash and convert to int16
                return Math.Abs(BitConverter.ToUInt16(MD5Data, 0));
            }
        }
    }
}
