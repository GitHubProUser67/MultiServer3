using CustomLogger;

namespace SRVEmu
{
    public class SRVEmuServer
    {
        public static bool IsStarted = false;
        AbstractDirtySockServer? RedirectorTSBO_NTSC_A;
        AbstractDirtySockServer? RedirectorTSBO_PAL;
        AbstractDirtySockServer? RedirectorBOP_PS3;
        AbstractDirtySockServer? Matchmaker;
        AbstractDirtySockServer? SimsMatchmaker;

        public Task Run()
        {
            LoggerAccessor.LogInfo("SRVEmu Server initiated...");

            try
            {
                RedirectorTSBO_NTSC_A = new RedirectorServer(11100, CryptoSporidium.MiscUtils.GetLocalIPAddress().ToString(), 10902, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_NTSC_A] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_PAL = new RedirectorServer(11140, CryptoSporidium.MiscUtils.GetLocalIPAddress().ToString(), 10902, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_PAL] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOP_PS3 = new RedirectorServer(21870, CryptoSporidium.MiscUtils.GetLocalIPAddress().ToString(), 10901, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOP_PS3] Failed to start! Exception: {ex}");
            }

            try
            {
                SimsMatchmaker = new MatchmakerServer(10901, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                // Create a List to store pairs of string and bool
                List<Tuple<string, bool>> SimsRooms = new()
                {
                    // Add some sample data
                    new Tuple<string, bool>("Veronaville", true),
                    new Tuple<string, bool>("Strangetown", true),
                    new Tuple<string, bool>("Pleasantview", true),
                    new Tuple<string, bool>("Belladonna Cove", true),
                    new Tuple<string, bool>("Riverblossom Hills", true)
                };

                SimsMatchmaker = new MatchmakerServer(10902, false, SimsRooms);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SimsMatchmaker] Failed to start! Exception: {ex}");
            }

            IsStarted = true;

            while (IsStarted)
            {

            }

            return Task.CompletedTask;
        }
    }
}
