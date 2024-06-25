using CustomLogger;
using MultiSocks.Aries.DataStore;

namespace MultiSocks.Aries
{
    public class AriesServer : IDisposable
    {
        public static IDatabase? Database = null;
        private readonly SDK_v6.AbstractAriesServer? RedirectorBOP_PS3;
        private readonly SDK_v6.AbstractAriesServer? RedirectorBOPULTIMATEBOX_PS3;
        private readonly SDK_v6.AbstractAriesServer? RedirectorHASBROFAMILYGAMENIGHT_PS3;

        private readonly SDK_v6.AbstractAriesServer? BurnoutParadisePS3Matchmaker;
        private readonly SDK_v6.AbstractAriesServer? BurnoutParadisePS3UltimateBoxMatchmaker;
        private readonly SDK_v6.AbstractAriesServer? HASBROFAMILYGAMENIGHTPS3Matchmaker;

        private bool disposedValue;

        public AriesServer(CancellationToken cancellationToken)
        {
            string ListenIP = MultiSocksServerConfiguration.UsePublicIPAddress ? CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();

            Database = new DirtySocksJSONDatabase();

            #region Redirector

            #region Burnout Paradise PS3
            try
            {
                RedirectorBOP_PS3 = new SDK_v6.RedirectorServer(21850, ListenIP, 21851, false, "BURNOUT5", "PS3");
                LoggerAccessor.LogInfo($"[Redirector] BOP PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] BOP PS3 Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOPULTIMATEBOX_PS3 = new SDK_v6.RedirectorServer(21870, ListenIP, 21871, false, "BURNOUT5", "PS3");
                LoggerAccessor.LogInfo($"[Redirector] BOPULTIMATEBOX PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] BOPULTIMATEBOX PS3 Failed to start! Exception: {ex}");
            }

            #endregion

            #region Hasbro Family Game Night PS3
            try
            {
                RedirectorHASBROFAMILYGAMENIGHT_PS3 = new SDK_v6.RedirectorServer(32950, ListenIP, 32951, false, "DPR-09", "PS3");
                LoggerAccessor.LogInfo($"[Redirector] HASBROFAMILYGAMENIGHT PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] HASBROFAMILYGAMENIGHT PS3 Failed to start! Exception: {ex}");
            }
            #endregion

            #endregion

            #region Matchmaker

            try
            {
                BurnoutParadisePS3Matchmaker = new SDK_v6.MatchmakerServer(21851, false, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadise PS3 Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePS3UltimateBoxMatchmaker = new SDK_v6.MatchmakerServer(21871, false, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadise PS3 UltimateBox Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                HASBROFAMILYGAMENIGHTPS3Matchmaker = new SDK_v6.MatchmakerServer(32951, false, "DPR-09", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Hasbro Family Game Night NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            #endregion

            LoggerAccessor.LogInfo("Aries Servers initiated...");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose all servers

                    // Redirectors
                    RedirectorBOP_PS3?.Dispose();
                    RedirectorBOPULTIMATEBOX_PS3?.Dispose();
                    RedirectorHASBROFAMILYGAMENIGHT_PS3?.Dispose();

                    //Matchmakers
                    BurnoutParadisePS3Matchmaker?.Dispose();
                    BurnoutParadisePS3UltimateBoxMatchmaker?.Dispose();
                    HASBROFAMILYGAMENIGHTPS3Matchmaker?.Dispose();

                    //Database
                    Database = null;

                    LoggerAccessor.LogWarn("Aries Servers stopped...");
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~AriesServer()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
