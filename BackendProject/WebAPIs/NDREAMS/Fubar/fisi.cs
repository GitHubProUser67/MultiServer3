using CustomLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendProject.WebAPIs.NDREAMS.Fubar
{
    public class fisi
    {
        public static string? fisiProcess(byte[]? PostData, string? contentType) {

            LoggerAccessor.LogInfo($"FUBAR POSTDATA: {Encoding.UTF8.GetString(PostData)}");


            return "";
        }
    }
}
