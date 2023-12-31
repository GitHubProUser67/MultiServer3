using CustomLogger;

namespace SRVEmu
{
    public class DirtySocksServer
    {
        public static bool IsStarted = false;
        AbstractDirtySockServer? RedirectorTSBO_NTSC_A;
        AbstractDirtySockServer? RedirectorTSBO_PAL;
        AbstractDirtySockServer? RedirectorBOP_PS3;
        AbstractDirtySockServer? RedirectorBOP_PC;
        AbstractDirtySockServer? BurnoutMatchmaker;
        AbstractDirtySockServer? SimsMatchmaker;

        public Task Run()
        {
            LoggerAccessor.LogInfo("SRVEmu Server initiated...");

            try
            {
                RedirectorTSBO_NTSC_A = new RedirectorServer(11100, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 10902, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_NTSC_A] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_PAL = new RedirectorServer(11140, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 10902, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_PAL] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOP_PS3 = new RedirectorServer(21870, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 10901, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOP_PS3] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOP_PC = new RedirectorServer(21841, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 10901, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOP_PC] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutMatchmaker = new MatchmakerServer(10901, true);
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
