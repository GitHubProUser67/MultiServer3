using System;
using System.Collections.Generic;
namespace WebAPIService.JUGGERNAUT.clearasil
{
    public class pushtime
    {
        public static string ProcessPushTime(Dictionary<string, string> QueryParameters, string apiPath)
        {
            if (QueryParameters != null)
            {
                string user = QueryParameters["user"];
                string time = QueryParameters["time"];

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(time))
                {
                    ScoreBoardData.UpdateTime(user, time);

                    try
                    {
                        ScoreBoardData.UpdateScoreboardXml(apiPath); // We finalized edit, so we issue a write.
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }

                    return string.Empty;
                }
            }

            return null;
        }
    }
}
