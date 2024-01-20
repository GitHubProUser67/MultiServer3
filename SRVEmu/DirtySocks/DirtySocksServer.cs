using CustomLogger;

namespace SRVEmu.DirtySocks
{
    public class DirtySocksServer
    {
        public static bool IsStarted = false;
        AbstractDirtySockServer? RedirectorTSBO_NTSC_A;
        AbstractDirtySockServer? RedirectorTSBO_PAL;
        AbstractDirtySockServer? RedirectorBOPULTIMATEBOX_PS3;
        AbstractDirtySockServer? RedirectorBOP_PS3;
        AbstractDirtySockServer? BurnoutParadiseUltimateBoxMatchmaker;
        AbstractDirtySockServer? BurnoutParadisePS3Matchmaker;
        AbstractDirtySockServer? SimsMatchmaker;

        public Task Run()
        {
            LoggerAccessor.LogInfo("SRVEmu Server initiated...");

            try
            {
                RedirectorTSBO_NTSC_A = new RedirectorServer(11100, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 11101, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_NTSC_A] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_PAL = new RedirectorServer(11140, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 11101, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_PAL] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOP_PS3 = new RedirectorServer(21850, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 21851, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOP_PS3] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOPULTIMATEBOX_PS3 = new RedirectorServer(21870, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 21871, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOPULTIMATEBOX_PS3] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePS3Matchmaker = new MatchmakerServer(21851, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadisePS3Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadiseUltimateBoxMatchmaker = new MatchmakerServer(21871, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadiseUltimateBoxMatchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                SimsMatchmaker = new MatchmakerServer(11101, false, new List<Tuple<string, bool>>()
                {
                    new("Veronaville", true),
                    new("Strangetown", true),
                    new("Pleasantview", true),
                    new("Belladonna Cove", true),
                    new("Riverblossom Hills", true)
                });
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
