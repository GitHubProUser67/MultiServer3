using BackendProject.MiscUtils;
using CustomLogger;
using SRVEmu.DirtySocks.DataStore;

namespace SRVEmu.DirtySocks
{
    public class DirtySocksServer
    {
        public static bool IsStarted = false;
        public static IDatabase Database = new DirtySocksJSONDatabase();
        private AbstractDirtySockServer? RedirectorTSBO_NTSC_A;
        private AbstractDirtySockServer? RedirectorTSBO_PAL;
        private AbstractDirtySockServer? RedirectorBOPULTIMATEBOX_PS3;
        private AbstractDirtySockServer? RedirectorBOP_PS3;
        private AbstractDirtySockServer? BurnoutParadiseUltimateBoxMatchmaker;
        private AbstractDirtySockServer? BurnoutParadisePS3Matchmaker;
        private AbstractDirtySockServer? SimsMatchmaker;

        public Task Run()
        {
            LoggerAccessor.LogInfo("DirtySocks Server initiated...");

            try
            {
                RedirectorTSBO_NTSC_A = new RedirectorServer(11100, VariousUtils.GetLocalIPAddress().ToString(), 11101, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_NTSC_A] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_PAL = new RedirectorServer(11140, VariousUtils.GetLocalIPAddress().ToString(), 11101, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_PAL] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOP_PS3 = new RedirectorServer(21850, VariousUtils.GetLocalIPAddress().ToString(), 21851, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOP_PS3] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOPULTIMATEBOX_PS3 = new RedirectorServer(21870, VariousUtils.GetLocalIPAddress().ToString(), 21871, false);
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
