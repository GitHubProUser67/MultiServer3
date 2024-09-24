using HomeTools.ChannelID;
using System.Collections.Concurrent;
using System.Xml;

namespace SSFWServer
{
    public class ScenelistParser
    {
        public static ConcurrentDictionary<string, string> sceneDictionary = new();

        public static void UpdateSceneDictionary(object? state)
        {
            if (File.Exists(SSFWServerConfiguration.ScenelistFile))
            {
                CustomLogger.LoggerAccessor.LogInfo($"[SSFW] - ScenelistParser: Dictionary refresh started at: {DateTime.Now}");

                sceneDictionary.Clear();

                XmlDocument xmlDoc = new();
                xmlDoc.Load(SSFWServerConfiguration.ScenelistFile);

                // Select all SCENE elements in the XML
                XmlNodeList? sceneNodes = xmlDoc.SelectNodes("//REGION/SCENE[@Type='Home' or @Type='Clubhouse' or @Type='PrivateNoVideo'] | //SCENE[@Type='Home' or @Type='Clubhouse' or @Type='PrivateNoVideo']");

                if (sceneNodes != null)
                {
                    // Extract ChannelID and SCENE ID from each SCENE element
                    Parallel.ForEach(sceneNodes.Cast<XmlNode>(), sceneNode => {
                        if (sceneNode.Attributes != null)
                        {
                            string? ID = sceneNode.Attributes["ID"]?.Value;

                            if (!string.IsNullOrEmpty(ID))
                            {
                                string? channelID = sceneNode.Attributes["ChannelID"]?.Value;

                                if (string.IsNullOrEmpty(channelID))
                                    channelID = sceneNode.Attributes["SceneID"]?.Value;

                                if (!string.IsNullOrEmpty(channelID))
                                {
                                    SceneKey? sceneKey = new(channelID);
                                    try
                                    {
                                        SIDKeyGenerator.Instance.VerifyNewerKey(sceneKey);
                                        string SceneID = SIDKeyGenerator.Instance.ExtractSceneIDNewerType(sceneKey).ToString("X").PadLeft(4, '0'); // SSFW was introduced in modern Home only, safe to assume only newer type.
                                        if (!sceneDictionary.ContainsKey(ID))
                                        {
#if DEBUG
                                            CustomLogger.LoggerAccessor.LogInfo($"[SSFW] - ScenelistParser: Added entry: ID:{ID}|SceneID:{SceneID}");
#endif

                                            sceneDictionary.TryAdd(ID, SceneID);
                                        }
                                    }
                                    catch (SceneKeyException)
                                    {
                                        // Not Important
                                    }
                                    catch (Exception)
                                    {
                                        // Not Important
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }
    }
}
