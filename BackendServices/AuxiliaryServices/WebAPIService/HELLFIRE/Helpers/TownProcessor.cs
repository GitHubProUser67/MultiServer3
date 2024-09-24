using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;
using WebAPIService.VEEMEE;

namespace WebAPIService.HELLFIRE.Helpers
{
    public class TownProcessor
    {
        public static string CreateBuilding(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string Orientation = string.Empty;
            string Type = string.Empty;
            string TownID = string.Empty;
            string Index = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    Orientation = data.GetParameterValue("Orientation");
                    Type = data.GetParameterValue("Type");
                    TownID = data.GetParameterValue("TownID");
                    Index = data.GetParameterValue("Index");
                    ms.Flush();
                }

                DateTime CurrentTime = DateTime.Now;

                if (File.Exists($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"))
                    File.WriteAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml",
                    Regex.Replace(File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"),
                    $"<{Index:F6}>(<TimeBuilt>).*?(</TimeBuilt>)(<Orientation>).*?(</Orientation>)(<Index>){Index:F6}(</Index>)(<Type>).*?(</Type>)</{Index:F6}>",
                    $"<{Index:F6}><TimeBuilt>{CurrentTime}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></{Index:F6}>"));

                return $"<Response><TimeBuilt>{CurrentTime}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></Response>";
            }

            return "<Response></Response>";
        }

        public static string UpdateBuildings(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string TownID = string.Empty;
            string BuildingDataEncoded = string.Empty;
            string TotalPopulation = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    TownID = data.GetParameterValue("TownID");
                    BuildingDataEncoded = data.GetParameterValue("BuildingData");
                    TotalPopulation = data.GetParameterValue("TotalPopulation");
                    ms.Flush();
                }


                LoggerAccessor.LogInfo($"BuildingData: {BuildingDataEncoded} \n TotalPop: {TotalPopulation}");

                DateTime CurrentTime = DateTime.Now;

                if (File.Exists($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"))
                {
                    List<BuildingData> buildingDataList = JsonConvert.DeserializeObject<List<BuildingData>>(BuildingDataEncoded);

                    //XmlDocument townXml = new XmlDocument();
                    //string townXmlFile = File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml");
                    //townXml.LoadXml("<root>" + townXmlFile + "</root>");

                    foreach (var BuildingData in buildingDataList)
                    {
                        string BuildingIndex = BuildingData.Index.ToString();


                        if (File.Exists($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"))
                        {
                            LoggerAccessor.LogInfo($"Building Index: {BuildingIndex} Updated with {BuildingData.Money} money, {BuildingData.WorkersSpent} WorkersSpent, {BuildingData.Population} population");

                            File.WriteAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml",
                            Regex.Replace(File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"),
                            $"<{BuildingIndex:F6}>(<TimeBuilt>).*?(</TimeBuilt>)(<Orientation>).*?(</Orientation>)(<Index>){BuildingIndex:F6}(</Index>)(<Type>).*?(</Type>)</{BuildingIndex:F6}>",
                            $"<{BuildingIndex:F6}><TimeBuilt>{CurrentTime}</TimeBuilt><Orientation></Orientation><Index>{BuildingIndex:F6}</Index><Type></Type><Money>{BuildingData.Money}</Money><Population>{BuildingData.Population}</Population><WorkersSpent>{BuildingData.WorkersSpent}</WorkersSpent></{BuildingIndex:F6}>"));
                        }
                        

                        /*
                        // Find the node corresponding to the target index
                        string xpathTarget = $"/Grid/{BuildingIndex + .000000}";
                        XmlNode targetNode = townXml.SelectSingleNode(xpathTarget);

                        BuildingData matchingData = buildingDataList.Find(d => d.Index == Convert.ToInt32(targetNode["Index"].InnerText));

                        if (matchingData != null)
                        {

                            if (targetNode != null)
                            {
                                XmlElement moneyEntry = townXml.CreateElement("Money");
                                XmlElement popEntry = townXml.CreateElement("Population");
                                XmlElement IndexEntry = townXml.CreateElement("Index");
                                XmlElement WorkersSpentEntry = townXml.CreateElement("WorkersSpent");

                                moneyEntry.InnerText = BuildingData.Money.ToString();
                                popEntry.InnerText = BuildingData.Population.ToString();
                                WorkersSpentEntry.InnerText = BuildingData.WorkersSpent.ToString();

                                // Update the child nodes
                                targetNode.AppendChild(moneyEntry);
                                targetNode.AppendChild(popEntry);
                                targetNode.AppendChild(WorkersSpentEntry);
                            }
                        }
                        */
                    }

                    //File.WriteAllText(townXmlFile, townXml.OuterXml);


                    
                    
                }

                //return $"<Response><TimeBuilt>{CurrentTime}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></Response>";

                return "<Response></Response>";
            }

            return "<Response></Response>";
        }

        public static string RemoveBuilding(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string xmlprofile = string.Empty;
            string TownID = string.Empty;
            string BuildingIndex = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    TownID = data.GetParameterValue("TownID");
                    BuildingIndex = data.GetParameterValue("BuildingIndex");
                    ms.Flush();
                }

                if (File.Exists($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"))
                    File.WriteAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml",
                    Regex.Replace(File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"),
                    $"<{BuildingIndex:F6}>(<TimeBuilt>).*?(</TimeBuilt>)(<Orientation>).*?(</Orientation>)(<Index>){BuildingIndex:F6}(</Index>)(<Type>).*?(</Type>)</{BuildingIndex:F6}>",
                    $"<{BuildingIndex:F6}><TimeBuilt></TimeBuilt><Orientation></Orientation><Index>{BuildingIndex}</Index><Type></Type></{BuildingIndex:F6}>"));
            }

            return "<Response></Response>";
        }

    }
}

public class BuildingData
{
    public double Money { get; set; }
    public double Population { get; set; }
    public int Index { get; set; }
    public int WorkersSpent { get; set; }
}