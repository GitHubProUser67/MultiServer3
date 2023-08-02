namespace PSMultiServer.PoodleHTTP.Addons.SVO
{
    public class PrepareFolder
    {
        public static void Prepare()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}HUBPS3_SVML/tracking/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}HUBPS3_SVML/tracking/");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}HUBPS3_SVML/fileservices/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}HUBPS3_SVML/fileservices/");
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}dataloaderweb/queue/"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}dataloaderweb/queue/");
            }
        }
    }
}
