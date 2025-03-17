﻿using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using HttpMultipartParser;

namespace WebAPIService.VEEMEE.audi_sled
{
    internal static class SledMpScoreProcessor
    {
        private static object _Lock = new object();

        public static string SetUserDataPOST(byte[] PostData, string boundary, string apiPath)
        {
            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                try
                {
                    using (MemoryStream copyStream = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(copyStream, boundary);
                        string key = data.GetParameterValue("key");
                        if (key != "k7dEUsKF3YvrfAxg")
                        {
                            CustomLogger.LoggerAccessor.LogError("[VEEMEE] - audi_sledmp - Client tried to push invalid key! Invalidating request.");
                            return null;
                        }
                        string psnid = data.GetParameterValue("psnid");
                        float time = (float)double.Parse(data.GetParameterValue("time"), CultureInfo.InvariantCulture);
                        float points = (float)double.Parse(data.GetParameterValue("points"), CultureInfo.InvariantCulture);
                        string directoryPath = $"{apiPath}/VEEMEE/audi_sledmp/User_Data";
                        string filePath = $"{directoryPath}/{psnid}.xml";
                        string highestScorePath = $"{directoryPath}/{psnid}_highest_score.txt";
                        int numOfRaces = 1;

                        Directory.CreateDirectory(directoryPath);

                        if (File.Exists(filePath))
                        {
                            // Load the XML string into an XmlDocument
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml($"<xml>{File.ReadAllText(filePath)}</xml>");

                            // Find the <points> element
                            XmlElement xmlElement = xmlDoc.SelectSingleNode("/xml/scores/entry/points") as XmlElement;

                            if (xmlElement != null)
                                // Replace the value of <points> with a new value
                                xmlElement.InnerText = points.ToString().Replace(",", ".");

                            // Find the <races> element
                            xmlElement = xmlDoc.SelectSingleNode("/xml/scores/entry/races") as XmlElement;

                            if (xmlElement != null)
                            {
                                numOfRaces = int.Parse(xmlElement.InnerText) + 1;
                                // Replace the value of <races> with a new value
                                xmlElement.InnerText = numOfRaces.ToString();
                            }

                            lock (_Lock)
                            {
                                if (File.Exists(highestScorePath))
                                {
                                    float currentTime = (float)double.Parse(File.ReadAllText(highestScorePath).Split(":")[0], CultureInfo.InvariantCulture);
                                    if (currentTime < time)
                                        File.WriteAllText(highestScorePath, time.ToString().Replace(",", ".") + $":{psnid}:{numOfRaces}");
                                }
                                else
                                    File.WriteAllText(highestScorePath, time.ToString().Replace(",", ".") + $":{psnid}:{numOfRaces}");
                            }

                            // Find the <time> element
                            xmlElement = xmlDoc.SelectSingleNode("/xml/scores/entry/time") as XmlElement;

                            if (xmlElement != null)
                                // Replace the value of <time> with a new value
                                xmlElement.InnerText = time.ToString().Replace(",", ".");

                            string XmlResult = xmlDoc.OuterXml.Replace("<xml>", string.Empty).Replace("</xml>", string.Empty);
                            File.WriteAllText(filePath, XmlResult);
                            return XmlResult;
                        }
                        else
                        {
                            lock (_Lock)
                            {
                                if (File.Exists(highestScorePath))
                                {
                                    float currentTime = (float)double.Parse(File.ReadAllText(highestScorePath).Split(":")[0], CultureInfo.InvariantCulture);
                                    if (currentTime < time)
                                        File.WriteAllText(highestScorePath, time.ToString().Replace(",", ".") + $":{psnid}:{numOfRaces}");
                                }
                                else
                                    File.WriteAllText(highestScorePath, time.ToString().Replace(",", ".") + $":{psnid}:{numOfRaces}");
                            }

                            string XmlData = $"<scores><entry><psnid>{psnid}</psnid><races>1</races><points>{points.ToString().Replace(",", ".")}</points><time>{time.ToString().Replace(",", ".")}</time></entry></scores>";
                            File.WriteAllText(filePath, XmlData);
                            return XmlData;
                        }
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.LoggerAccessor.LogError($"[SledMpScoreProcessor] - SetUserDataPOST thrown an assertion. (Exception: {ex})");
                }
            }

            return null;
        }

        public static string GetUserDataPOST(byte[] PostData, string boundary, string apiPath)
        {
            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                try
                {
                    using (MemoryStream copyStream = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(copyStream, boundary);
                        string key = data.GetParameterValue("key");
                        if (key != "k7dEUsKF3YvrfAxg")
                        {
                            CustomLogger.LoggerAccessor.LogError("[VEEMEE] - audi_sledmp - Client tried to push invalid key! Invalidating request.");
                            return null;
                        }
                        string psnid = data.GetParameterValue("psnid");
                        string directoryPath = $"{apiPath}/VEEMEE/audi_sledmp/User_Data";
                        string filePath = $"{directoryPath}/{psnid}.xml";

                        if (File.Exists(filePath))
                            return File.ReadAllText(filePath);

                        return $"<scores><entry><psnid>{psnid}</psnid><races>0</races><points>0</points><time>0</time></entry></scores>";
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.LoggerAccessor.LogError($"[SledMpScoreProcessor] - GetUserDataPOST thrown an assertion. (Exception: {ex})");
                }
            }

            return null;
        }

        public static string GetHigherUserScorePOST(byte[] PostData, string boundary, string apiPath)
        {
            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                try
                {
                    using (MemoryStream copyStream = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(copyStream, boundary);
                        string key = data.GetParameterValue("key");
                        if (key != "k7dEUsKF3YvrfAxg")
                        {
                            CustomLogger.LoggerAccessor.LogError("[VEEMEE] - audi_sledmp - Client tried to push invalid key! Invalidating request.");
                            return null;
                        }
                        string psnid = data.GetParameterValue("psnid");
                        string directoryPath = $"{apiPath}/VEEMEE/audi_sledmp/User_Data";
                        string filePath = $"{directoryPath}/{psnid}.xml";
                        string highestScorePath = $"{directoryPath}/{psnid}_highest_score.txt";

                        if (File.Exists(filePath) && File.Exists(highestScorePath))
                        {
                            string scoreData;
                            lock (_Lock)
                            {
                                scoreData = File.ReadAllText(highestScorePath).Split(":")[0];
                            }
                            return Regex.Replace(File.ReadAllText(filePath), @"<time>\d+(\.\d+)?</time>", $"<time>{scoreData}</time>");
                        }
                        return $"<scores><entry><psnid>{psnid}</psnid><races>0</races><points>0</points><time>0</time></entry></scores>";
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.LoggerAccessor.LogError($"[SledMpScoreProcessor] - GetHigherUserScorePOST thrown an assertion. (Exception: {ex})");
                }
            }

            return null;
        }

        public static string GetGlobalTablePOST(byte[] PostData, string boundary, string apiPath)
        {
            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                try
                {
                    using (MemoryStream copyStream = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(copyStream, boundary);
                        string key = data.GetParameterValue("key");
                        if (key != "k7dEUsKF3YvrfAxg")
                        {
                            CustomLogger.LoggerAccessor.LogError("[VEEMEE] - audi_sledmp - Client tried to push invalid key! Invalidating request.");
                            return null;
                        }
                        string psnid = data.GetParameterValue("psnid");
                        string title = data.GetParameterValue("title");
                        return GenerateGlobalScoreXML(GetTopScores($"{apiPath}/VEEMEE/audi_sledmp/User_Data"), title);
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.LoggerAccessor.LogError($"[SledMpScoreProcessor] - GetHigherUserScorePOST thrown an assertion. (Exception: {ex})");
                }
            }

            return null;
        }

        private static string GenerateGlobalScoreXML(List<(string, float, string)> scores, string title)
        {
            if (scores.Count > 8)
                throw new InvalidDataException("[SledMpScoreProcessor] - GenerateGlobalScoreXML received an invalid count of scores, only takes up to 8 entries.");

            int iY = 142; // Initial Y position
            StringBuilder data = new StringBuilder($"<XML><PAGE><TEXT X=\"100\" Y=\"70\" col=\"#FFFFFF\" size=\"4\">{title}</TEXT>");

            for (int i = 0; i < scores.Count; i++)
            {
                float score = scores[i].Item2;
                data.AppendFormat("<TEXT X=\"100\" Y=\"{0}\" col=\"#FFFFFF\" size=\"3\">{1}</TEXT>", iY + 7, i + 1);
                data.AppendFormat("<TEXT X=\"190\" Y=\"{0}\" col=\"#FFFFFF\" size=\"3\">{1}</TEXT>", iY + 5, scores[i].Item1);
                data.AppendFormat("<TEXT X=\"800\" Y=\"{0}\" col=\"#FFFFFF\" size=\"3\">{1}</TEXT>", iY + 5, scores[i].Item3);
                data.AppendFormat("<TEXT X=\"1060\" Y=\"{0}\" col=\"#FFFFFF\" size=\"3\">{1}</TEXT>", iY + 5, AudiSledSecondsAsString(score));

                iY += 46; // Move down for next entry
            }

            data.Append("</PAGE></XML>");

            return data.ToString();
        }

        private static string AudiSledSecondsAsString(float time)
        {
            if (time < float.Epsilon)
                return " -- : -- . --";

            int seconds = (int)Math.Floor(time);
            if (seconds < 0)
            {
                seconds = 0;
            }

            int hundreds = (int)Math.Floor(((time - seconds) * 100) + 0.5);
            if (hundreds < 0)
            {
                hundreds = 0;
            }

            int minutes = seconds / 60;
            seconds = seconds % 60;

            return string.Format("{0:D2}:{1:D2}.{2:D2}", minutes, seconds, hundreds);
        }

        private static List<(string, float, string)> GetTopScores(string directoryPath)
        {
            List<(string, float, string)> scores = new List<(string, float, string)>();

            if (Directory.Exists(directoryPath))
            {
                lock (_Lock)
                {
                    foreach (string file in Directory.GetFiles(directoryPath, "*_highest_score.txt"))
                    {
                        try
                        {
                            string[] scoreData = File.ReadAllText(file).Split(":");
                            if (scoreData.Length == 3)
                                scores.Add((scoreData[1], (float)double.Parse(scoreData[0], CultureInfo.InvariantCulture), scoreData[2]));
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return scores.OrderByDescending(s => s).Take(8).ToList();
        }
    }
}
