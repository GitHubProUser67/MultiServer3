using CustomLogger;
using MultiSocks.DirtySocks.DataStore;

namespace MultiSocks.DirtySocks
{
    public class DirtySocksServer
    {
        public static bool IsStarted = false;
        public static IDatabase Database = new DirtySocksJSONDatabase();
        private AbstractDirtySockServer? RedirectorSSX3_NTSC_A;
        private AbstractDirtySockServer? RedirectorSSX3_PAL;
        private AbstractDirtySockServer? RedirectorTSBO_NTSC_A;
        private AbstractDirtySockServer? RedirectorTSBO_PAL;
        private AbstractDirtySockServer? RedirectorBOPULTIMATEBOX_PS3;
        private AbstractDirtySockServer? RedirectorBOP_PS3;
        private AbstractDirtySockServer? BurnoutParadiseUltimateBoxMatchmaker;
        private AbstractDirtySockServer? BurnoutParadisePS3Matchmaker;
        private AbstractDirtySockServer? SimsMatchmaker;
        private AbstractDirtySockServer? SSX3Matchmaker;

        public Task Run()
        {
            LoggerAccessor.LogInfo("DirtySocks Server initiated...");

            try
            {
                RedirectorSSX3_NTSC_A = new RedirectorServer(11000, MultiSocksServerConfiguration.ServerBindAddress, 11051, false, "SSX-ER-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorSSX3_NTSC_A] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorSSX3_PAL = new RedirectorServer(11050, MultiSocksServerConfiguration.ServerBindAddress, 11051, false, "SSX-ER-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorSSX3_PAL] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_NTSC_A = new RedirectorServer(11100, MultiSocksServerConfiguration.ServerBindAddress, 11101, false, "TSBO", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_NTSC_A] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_PAL = new RedirectorServer(11140, MultiSocksServerConfiguration.ServerBindAddress, 11101, false, "TSBO", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_PAL] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOP_PS3 = new RedirectorServer(21850, MultiSocksServerConfiguration.ServerBindAddress, 21851, false, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOP_PS3] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOPULTIMATEBOX_PS3 = new RedirectorServer(21870, MultiSocksServerConfiguration.ServerBindAddress, 21871, false, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOPULTIMATEBOX_PS3] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePS3Matchmaker = new MatchmakerServer(21851, true, null, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadisePS3Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadiseUltimateBoxMatchmaker = new MatchmakerServer(21871, true, null, "BURNOUT5", "PS3");
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
                }, "TSBO", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SimsMatchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                SSX3Matchmaker = new MatchmakerServer(11051, false, null, "SSX-ER-PS2-2004", "PS2");
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
