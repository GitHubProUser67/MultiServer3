using CustomLogger;

namespace SRVEmu
{
    public class SRVEmuServer
    {
        public static bool IsStarted = false;
        AbstractDirtySockServer? RedirectorTSBO_NTSC_A;
        AbstractDirtySockServer? RedirectorTSBO_PAL;
        AbstractDirtySockServer? Matchmaker;

        public Task Run()
        {
            LoggerAccessor.LogInfo("SRVEmu Server initiated...");

            try
            {
                RedirectorTSBO_NTSC_A = new RedirectorServer(11100, CryptoSporidium.MiscUtils.GetLocalIPAddress().ToString(), 10901);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_NTSC_A] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_PAL = new RedirectorServer(11140, CryptoSporidium.MiscUtils.GetLocalIPAddress().ToString(), 10901);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_PAL] Failed to start! Exception: {ex}");
            }

            try
            {
                Matchmaker = new MatchmakerServer(10901);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Matchmaker] Failed to start! Exception: {ex}");
            }

            IsStarted = true;

            while (IsStarted)
            {

            }

            return Task.CompletedTask;
        }
    }
}
