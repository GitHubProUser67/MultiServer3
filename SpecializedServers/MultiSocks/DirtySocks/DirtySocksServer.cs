using CustomLogger;
using MultiSocks.DirtySocks.DataStore;

namespace MultiSocks.DirtySocks
{
    public class DirtySocksServer : IDisposable
    {
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
        private bool disposedValue;

        public DirtySocksServer(CancellationToken cancellationToken)
        {
            string ListenIP = MultiSocksServerConfiguration.UsePublicIPAddress ? CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();

            try
            {
                RedirectorSSX3_NTSC_A = new RedirectorServer(11000, ListenIP, 11051, false, "SSX-ER-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorSSX3_NTSC_A] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorSSX3_PAL = new RedirectorServer(11050, ListenIP, 11051, false, "SSX-ER-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorSSX3_PAL] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_NTSC_A = new RedirectorServer(11100, ListenIP, 11101, false, "TSBO", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_NTSC_A] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_PAL = new RedirectorServer(11140, ListenIP, 11101, false, "TSBO", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorTSBO_PAL] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOP_PS3 = new RedirectorServer(21850, ListenIP, 21851, false, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOP_PS3] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOPULTIMATEBOX_PS3 = new RedirectorServer(21870, ListenIP, 21871, false, "BURNOUT5", "PS3");
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

            LoggerAccessor.LogInfo("DirtySocks Servers initiated...");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose all servers
                    RedirectorSSX3_NTSC_A?.Dispose();
                    RedirectorSSX3_PAL?.Dispose();
                    RedirectorTSBO_NTSC_A?.Dispose();
                    RedirectorTSBO_PAL?.Dispose();
                    RedirectorBOP_PS3?.Dispose();
                    RedirectorBOPULTIMATEBOX_PS3?.Dispose();
                    BurnoutParadisePS3Matchmaker?.Dispose();
                    BurnoutParadiseUltimateBoxMatchmaker?.Dispose();
                    SimsMatchmaker?.Dispose();
                    SSX3Matchmaker?.Dispose();

                    LoggerAccessor.LogWarn("DirtySocks Servers stopped...");
                }

                // TODO: lib�rer les ressources non manag�es (objets non manag�s) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour lib�rer les ressources non manag�es
        // ~DirtySocksServer()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la m�thode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la m�thode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
