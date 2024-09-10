using CustomLogger;
using CyberBackendLibrary.HTTP;
using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIService.VEEMEE.wardrobe_wars
{
    internal class Podium
    {
        ///<summary>
        /// First entry must equal 1 to verify <br/>
        /// Second entry is for WW.Bracelet <br/>
        /// Third entry is for refresh timer <br/>
        /// Fourth entry is for display time for screens <br/>
        /// Fifth entry is for Refreshing Score on Kiosk <br/>
        /// Sixth entry is for Refreshing Score on Podium
        /// </summary> 
        public static string Verify(byte[] PostData, string ContentType, string apiPath)
        {
            string territory = string.Empty;
            string region = string.Empty;
            string time = string.Empty;
            string psnid = string.Empty;
            string language = string.Empty;
            string bracelet = string.Empty;

            string boundary = HTTPProcessor.ExtractBoundary(ContentType);

            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);


                    string serverFilePath = $"{apiPath}/VEEMEE/WW-Prod/User_Data";
                    Directory.CreateDirectory(serverFilePath);

                    territory = data.GetParameterValue("territory");
                    region = data.GetParameterValue("region");
                    time = data.GetParameterValue("time");
                    psnid = data.GetParameterValue("psnid");
                    language = data.GetParameterValue("language");
                    //if secure url send
                    bracelet = data.GetParameterValue("bracelet");


#if DEBUG
                    LoggerAccessor.LogInfo($"[VEEMEE] - Verify Details: POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
#endif

                    ms.Flush();
                }

                return "1,http://ww-prod.destinations.scea.com/WardrobeWars/bracelet/,30,60,60,60";
            }

            return null;
        }

        /// <summary>
        /// Entry 1 tells if Podium is active or not
        /// Entry 2 is PSNID
        /// Entry 3 is Score
        /// Entry 4 is id
        /// Entry 5 is localvote
        /// </summary>
        public static string CheckPodium(byte[] PostData, string ContentType, string apiPath)
        {
            string id = string.Empty;
            string previousid = string.Empty;
            string limitLocal = string.Empty;

            string psnNameFromFileName = string.Empty;
            int indexId = -1;

            if (PostData != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {

                    Dictionary<string, string> urlEncodedData = HTTPProcessor.ExtractAndSortUrlEncodedPOSTData(PostData);


                    urlEncodedData.TryGetValue("id", out id);
                    urlEncodedData.TryGetValue("previous", out previousid);
                    urlEncodedData.TryGetValue("limitLocal", out limitLocal);

                    //indexId = Convert.ToInt32(id.Split(".").First());
                    LoggerAccessor.LogInfo($"id {id} previous {previousid} limitLocal {limitLocal}");



                    string serverFilePath = $"{apiPath}/VEEMEE/WW-Prod/User_Data/{DateTime.UtcNow.ToString("yyyy-MM-dd")}/";

                    if (Directory.Exists(serverFilePath))
                    {

                        string[] userProfiles = Directory.GetFiles(serverFilePath);

                        //string index = userProfiles[Convert.ToInt32(id)].

                        //psnNameFromFileName = userProfileIndexSelected.Split("/").First();

                        psnNameFromFileName = userProfiles[0];
                    } else
                    {
                        LoggerAccessor.LogError($"No date directory found for today in path {serverFilePath}!");
                    }




#if DEBUG
                    LoggerAccessor.LogInfo($"[VEEMEE] - Podium Details: POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
#endif

                    ms.Flush();
                }

                return $"1,{psnNameFromFileName},0,{indexId},0";
            }

            return null;
        }

        /// <summary>
        /// Entry 1 tells 1 is successful Vote_Successful, 2 is Vote_Failure
        /// Entry 2 is if Vote_Successful send entrant score
        /// </summary>
        public static string RequestVote(byte[] PostData, string ContentType)
        {
            string id = string.Empty;
            string entrant_id = string.Empty;
            string vote = string.Empty;

            if (PostData != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {

                    Dictionary<string, string> urlEncodedData = HTTPProcessor.ExtractAndSortUrlEncodedPOSTData(PostData);


                    urlEncodedData.TryGetValue("id", out id);
                    urlEncodedData.TryGetValue("entrant_id", out entrant_id);
                    urlEncodedData.TryGetValue("vote", out vote);

                    LoggerAccessor.LogInfo($"id {id} entrant_id {entrant_id} vote {vote}");


#if DEBUG
                    LoggerAccessor.LogInfo($"[VEEMEE] - Podium_Vote Details: POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
#endif

                    ms.Flush();
                }

                return "1,10";
            }

            return null;
        }

        /// <summary>
        /// Entry 1 tells 1 is successful score return, 0 or any other is not able to fetch
        /// Entry 2 is if Vote_Successful send entrant score
        /// Entry 3 is bool localVoted
        /// </summary>
        public static string RequestScore(byte[] PostData, string ContentType)
        {
            string id = string.Empty;
            string entrant_id = string.Empty;
            string config = string.Empty;
            string product = string.Empty;

            if (PostData != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {

                    Dictionary<string, string> urlEncodedData = HTTPProcessor.ExtractAndSortUrlEncodedPOSTData(PostData);


                    urlEncodedData.TryGetValue("id", out id);
                    urlEncodedData.TryGetValue("entrant_id", out entrant_id);


                    LoggerAccessor.LogInfo($"id {id} entrant_id {entrant_id} vote");

#if DEBUG
                    LoggerAccessor.LogInfo($"[VEEMEE] - Podium_Score Details: POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
#endif





                    ms.Flush();
                }

                return "1,10,3";
            }

            return null;
        }

        /// <summary>
        /// Entry 1 ~= WWReward.NO_RETURN = 0
        /// Entry 2 is how many rewards
        /// Entry 3 is for each index, AddRewardTicket
        /// </summary>
        public static string RequestRewards(byte[] PostData, string ContentType, string apiPath)
        {
            string[] rewardsTXT = null;
            string id = string.Empty;
            string entrant_id = string.Empty;

            Dictionary<string, string> urlEncodedData = HTTPProcessor.ExtractAndSortUrlEncodedPOSTData(PostData);

            string RewardsResponse = string.Empty;

            if (PostData != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {

                    urlEncodedData.TryGetValue("id", out id);
                    urlEncodedData.TryGetValue("entrant_id", out entrant_id);
#if DEBUG
                    LoggerAccessor.LogInfo($"[VEEMEE] - Rewards Details: POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
#endif

                    string serverFilePath = $"{apiPath}/VEEMEE/WW-Prod/Server_Data";
                    Directory.CreateDirectory(serverFilePath);

                    if (File.Exists(serverFilePath + "/Rewards.txt"))
                    {
                        rewardsTXT = File.ReadAllLines(serverFilePath + "/Rewards.txt");

                        RewardsResponse = $"1,{rewardsTXT.Length},";

                        foreach (string rewardUUID in rewardsTXT)
                        {
                            RewardsResponse = RewardsResponse + $",{rewardUUID}";
                        }

                    }
                    else
                    {
                        LoggerAccessor.LogWarn($"Using fallback Rewards txt! Please provide one in {serverFilePath + "/Rewards.txt"}");

                        RewardsResponse = $"1,1,DF7977BE-28684CDE-A2FE030B-7416242A";

                    }

                    ms.Flush();
                }

                return RewardsResponse;
            }

            return null;
        }

        /// <summary>
        /// Returns XML for various parts of Screens
        /// </summary>
        public static string RequestScreens(byte[] PostData, string ContentType, string apiPath)
        {

            string screensXML = string.Empty;
            string id = string.Empty;
            string entrant_id = string.Empty;


            if (PostData != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {

                    Dictionary<string, string> urlEncodedData = HTTPProcessor.ExtractAndSortUrlEncodedPOSTData(PostData);


                    urlEncodedData.TryGetValue("id", out id);
                    urlEncodedData.TryGetValue("entrant_id", out entrant_id);
#if DEBUG
                    LoggerAccessor.LogInfo($"[VEEMEE] - Rewards Details: POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
#endif

                    string serverFilePath = $"{apiPath}/VEEMEE/WW-Prod/Server_Data";
                    Directory.CreateDirectory(serverFilePath);
                    
                    if(File.Exists(serverFilePath + "/Screens.xml"))
                    {
                        screensXML = File.ReadAllText(serverFilePath + "/Screens.xml");


                    } else
                    {
                        LoggerAccessor.LogWarn($"Using fallback Screens xml! Please provide one in {serverFilePath + "/Screens.xml"}");

                        screensXML = @"<Result>
    <!-- Winners -->
    <Winners>
        <Winner>
            <name>NAME_OF_WINNER</name>
            <type>TYPE</type>
            <score>10</score>
            <date></date>
            <tex>TEXTURE</tex>
        </Winner>
    </Winners>
    <!-- theme -->
    <Theme>Any Theme!</Theme>
    <!-- Rewards -->
    <Rewards>
    <!-- Reward Types PARTICIPANT_PRIZE/1 DAILY_WINNER_PRIZE/2 WEEKLY_WINNER_PRIZE/3 MONTHLY_WINNER_PRIZE/4 -->
        <Reward>
            <name>DF7977BE-28684CDE-A2FE030B-7416242A</name>
            <type>1</type>
            <tex>TEXTURE</tex>
        </Reward>
    </Rewards>
    <!-- Screens -->
    <Screens>
    <!-- Screen Types Winner/1 Theme/2 Reward/3 -->
        <Screen id=""1"">
            <Display>
                <type>1</type>
                <index>1</index>
            </Display>
        </Screen>
        
        <Screen id=""2"">
            <Display>
                <type>2</type>
                <index>2</index>
            </Display>
        </Screen>
        
        <Screen id=""3"">
            <Display>
                <type>3</type>
                <index>3</index>
            </Display>
        </Screen>
    </Screens>
</Result>";
                    }


                    ms.Flush();
                }

                return screensXML;
            }

            return null;
        }

        /// <summary>
        /// Entry 1 tells 1 is successful score return, 0 or any other is not able to fetch
        /// Entry 2 is if Vote_Successful send entrant score
        /// Entry 3 is bool localVoted
        /// </summary>
        public static string PostPhotoPart1(byte[] PostData, string ContentType)
        {
            string territory = string.Empty;
            string Region = string.Empty;
            string time = string.Empty;
            string psnid = string.Empty;
            string bracelet = string.Empty;
            string secureme = string.Empty;

            if (PostData != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {

                    Dictionary<string, string> urlEncodedData = HTTPProcessor.ExtractAndSortUrlEncodedPOSTData(PostData);


                    urlEncodedData.TryGetValue("territory", out territory);
                    urlEncodedData.TryGetValue("Region", out Region);
                    urlEncodedData.TryGetValue("time", out time);
                    urlEncodedData.TryGetValue("psnid", out psnid);
                    urlEncodedData.TryGetValue("bracelet", out bracelet);
                    urlEncodedData.TryGetValue("secureme", out secureme);



#if DEBUG
                    LoggerAccessor.LogInfo($"[VEEMEE] - Podium_Score Details: POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
#endif





                    ms.Flush();
                }
                //return id
                return $"{psnid}_{DateTime.UtcNow.ToString("yyyy-MM-dd")}";
            }

            return null;
        }

        //Save Part2
        public static string PostPhotoPart2(byte[] PostData, string ContentType, string apiPath, bool isSecure)
        {
            string territory = string.Empty;
            string Region = string.Empty;
            string time = string.Empty;
            string psnid = string.Empty;
            string bracelet = string.Empty;
            string secureme = string.Empty;
            string thefile = string.Empty;


            string serverFilePath = $"{apiPath}/VEEMEE/WW-Prod/User_Data/{DateTime.UtcNow.ToString("yyyy-MM-dd")}";
            Directory.CreateDirectory(serverFilePath);


            string boundary = HTTPProcessor.ExtractBoundary(ContentType);

            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);


                    territory = data.GetParameterValue("territory");
                    Region = data.GetParameterValue("region");
                    time = data.GetParameterValue("time");
                    psnid = data.GetParameterValue("psnid");

                    if(isSecure)
                    {
                        bracelet = data.GetParameterValue("bracelet");
                    } else
                    {
                        secureme = data.GetParameterValue("secureme");
                    }
                    thefile = data.GetParameterValue("thefile");

                    // Write the file data directly to the file.
                    using (var fileStream = File.Create(serverFilePath + $"/{psnid}_{DateTime.UtcNow.ToString("hh-mm-ss")}.jpg"))
                    {
                        data.Files.FirstOrDefault().Data.CopyTo(fileStream);
                    }


#if DEBUG
                    LoggerAccessor.LogInfo($"[VEEMEE] - Podium_Score Details: POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
#endif





                    ms.Flush();
                }
                //return id
                return $"{psnid}_{DateTime.UtcNow.ToString("yyyy-MM-dd")}";
            }

            return null;
        }

    }
}
