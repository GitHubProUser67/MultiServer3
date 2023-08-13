namespace PSMultiServer.PoodleHTTP.Addons.SVO
{
    public class PrepareFolder
    {
        public static void Prepare()
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}wox_ws/rest/account/");

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}HUBPS3_SVML/tracking/");

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}HUBPS3_SVML/fileservices/");

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}dataloaderweb/queue/");
        }
    }
}
