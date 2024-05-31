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
        private AbstractDirtySockServer? RedirectorBOP_PS3;
        private AbstractDirtySockServer? BurnoutParadiseUltimateBoxMatchmaker;
        private AbstractDirtySockServer? RedirectorNFLStreet_NTSC;
        private AbstractDirtySockServer? Redirector007EverythingOrNothing_NTSC;
        private AbstractDirtySockServer? RedirectorBOPULTIMATEBOX_PS3;
        private AbstractDirtySockServer? RedirectorBOPULTIMATEBOX_PC;
        private AbstractDirtySockServer? RedirectorLordOfTheRingsTheReturnOfTheKing_NTSC;
        private AbstractDirtySockServer? RedirectorNASCAR08_PS3;
        private AbstractDirtySockServer? RedirectorNASCAR09_PS3;
        private AbstractDirtySockServer? RedirectorCRYSIS3_MPBETA_PS3;
        private AbstractDirtySockServer? BurnoutParadisePS3UltimateBoxMatchmaker;
        private AbstractDirtySockServer? BurnoutParadisePCUltimateBoxMatchmaker;
        private AbstractDirtySockServer? BurnoutParadisePS3Matchmaker;
        private AbstractDirtySockServer? EverythingOrNothing007_NTSC_Matchmaker;
        private AbstractDirtySockServer? LordOfTheRingsTheReturnOfTheKing_NTSC_Matchmaker;
        private AbstractDirtySockServer? NFLStreet_NTSCMatchmaker;
        private AbstractDirtySockServer? SimsMatchmaker;
        private AbstractDirtySockServer? SSX3Matchmaker;
        private bool disposedValue;

        public DirtySocksServer(CancellationToken cancellationToken)
        {
            string ListenIP = MultiSocksServerConfiguration.UsePublicIPAddress ? CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();


            #region Redirector

            #region SSX3 PS2
            try
            {
               RedirectorSSX3_NTSC_A = new RedirectorServer(11000, ListenIP, 11051, false, "SSX-ER-PS2-2004", "PS2");
               LoggerAccessor.LogInfo($"[Redirector] SSX3_NTSC_A Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] SSX3_NTSC_A Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorSSX3_PAL = new RedirectorServer(11050, ListenIP, 11051, false, "SSX-ER-PS2-2004", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] SSX3_PAL Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] SSX3_PAL Failed to start! Exception: {ex}");
            }
            #endregion

            #region The Sims Bustin' Out PS2
            try
            {
                RedirectorTSBO_NTSC_A = new RedirectorServer(11100, ListenIP, 11101, false, "TSBO", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] TSBO_NTSC_A Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] TSBO_NTSC_A Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_PAL = new RedirectorServer(11140, ListenIP, 11101, false, "TSBO", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] TSBO_PAL Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] TSBO_PAL Failed to start! Exception: {ex}");
            }
            #endregion

            #region NFL Street PS2
            try
            {
                RedirectorNFLStreet_NTSC = new RedirectorServer(11300, ListenIP, 11301, false, "NFLSTREET-PS2-2005", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] NFL Street NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NFL Street NTSC Failed to start! Exception: {ex}");
            }


            try
            {
                RedirectorNFLStreet2_NTSC = new RedirectorServer(21301, ListenIP, 21302, false, "NFLSTREET-PS2-2005", "PS2");
            {
                LoggerAccessor.LogError($"[Redirector] NFL Street 2 NTSC Failed to start! Exception: {ex}");

            #endregion


            #region Lord Of the Rings: The Return of the King
            try
            {
                RedirectorLordOfTheRingsTheReturnOfTheKing_NTSC = new RedirectorServer(11200, ListenIP, 11201, false, "LOTR", "PS2", false, "ps2rotk04.ea.com", "ps2rotk04@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] LOTR:TROTK PS2 NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] LOTR:TROTK PS2 NTSC Failed to start! Exception: {ex}");
            }
            #endregion

            #region Burnout Paradise PS3
            try
            {
                RedirectorBOP_PS3 = new RedirectorServer(21850, ListenIP, 21851, false, "BURNOUT5", "PS3");
                LoggerAccessor.LogInfo($"[Redirector] BOP PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] BOP PS3 Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOPULTIMATEBOX_PS3 = new RedirectorServer(21870, ListenIP, 21871, false, "BURNOUT5", "PS3");
                LoggerAccessor.LogInfo($"[Redirector] BOPULTIMATEBOX PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] BOPULTIMATEBOX PS3 Failed to start! Exception: {ex}");
            }

            #endregion

            #region Burnout Paradise PC
            try
            {
                RedirectorBOPULTIMATEBOX_PC = new RedirectorServer(21841, ListenIP, 21842, false, "BURNOUT5", "PC", false, "pcburnout08.ea.com", "pcburnout08@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] BOPULTIMATEBOX PC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] BOPULTIMATEBOX PC Failed to start! Exception: {ex}");
            }
            #endregion

            #region Nascar PS3
            try
            {
                RedirectorNASCAR08_PS3 = new RedirectorServer(20651, ListenIP, 20652, false, "NASCAR08", "PS3", true, "ps3nascar08.ea.com", "ps3nascar08@ea.com");
                LoggerAccessor.LogInfo($"[Redirector NASCAR08 PS3] Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector NASCAR08 PS3] Failed to start! Exception: {ex}");
            }


            try
            {
                RedirectorNASCAR09_PS3 = new RedirectorServer(30671, ListenIP, 30672, false, "NASCAR09", "PS3", true, "ps3nascar09.ea.com", "ps3nascar09@ea.com");
                LoggerAccessor.LogInfo($"[Redirector NASCAR09 PS3] Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector NASCAR09 PS3] Failed to start! Exception: {ex}");
            }
            #endregion


            #region NCAA March Madness 06
            try
            {
                RedirectorNCAAMM06_NTSC = new RedirectorServer(30701, ListenIP, 30702, false, "PS2-MM-2006", "PS2", false, "ps2mm06.ea.com", "ps2mm06@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] NCAA March Madness 06 NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NCAA March Madness 06 NTSC Failed to start! Exception: {ex}");
            }
            #endregion



            #region 007: Everything or Nothing
            try
            {
                Redirector007EverythingOrNothing_NTSC = new RedirectorServer(11600, ListenIP, 11601, false, "PS2-BOND-2004", "PS2", false, "ps2bond04.ea.com", "ps2bond04@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] 007 Everything Or Nothing NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] 007 Everything Or Nothing NTSC Failed to start! Exception: {ex}");
            }
            #endregion

            /*
            #region Crysis 3
            try
            {
                RedirectorCRYSIS3_MPBETA_PS3 = new RedirectorServer(42127, ListenIP, 42128, false, "CRYSIS3MPBETA", "PS3", true, "gosredirector.ea.com", "ps3crysis3@ea.com");

                LoggerAccessor.LogInfo($"[Redirector CRYSIS3 MPBETA PS3] Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector CRYSIS3 MPBETA PS3] Failed to start! Exception: {ex}");
            }
            #endregion
            */
            #endregion

            #region Matchmaker

            try
            {
                NFLStreet_NTSCMatchmaker = new MatchmakerServer(11301, true, null, "NFLSTREET-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFL Street NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                NFLStreet2_NTSCMatchmaker = new MatchmakerServer(21302, true, null, "NFLSTREET-PS2-2005", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFL Street NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                LordOfTheRingsTheReturnOfTheKing_NTSC_Matchmaker = new MatchmakerServer(11201, true, null, "PS2_ROTK_2004", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Everything Or Nothing 007 NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                NCAAMM06_NTSC_Matchmaker = new MatchmakerServer(30702, true, null, "PS2-MM-2006", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFL Street NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                EverythingOrNothing007_NTSC_Matchmaker = new MatchmakerServer(11601, true, null, "PS2-BOND-2004", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Everything Or Nothing 007 NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePS3Matchmaker = new MatchmakerServer(21851, true, null, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadise PS3 Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePS3UltimateBoxMatchmaker = new MatchmakerServer(21871, true, null, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadise UltimateBox Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePCUltimateBoxMatchmaker = new MatchmakerServer(21842, true, null, "BURNOUT5", "PC");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadisePCUltimateBox Matchmaker] Failed to start! Exception: {ex}");
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

            #endregion

            LoggerAccessor.LogInfo("DirtySocks Servers initiated...");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose all servers

                    // Redirectors
                    RedirectorSSX3_NTSC_A?.Dispose();
                    RedirectorSSX3_PAL?.Dispose();
                    RedirectorTSBO_NTSC_A?.Dispose();
                    RedirectorTSBO_PAL?.Dispose();
                    RedirectorNCAAMM06_NTSC?.Dispose();
                    RedirectorBOP_PS3?.Dispose();
                    RedirectorBOPULTIMATEBOX_PS3?.Dispose();
                    RedirectorBOPULTIMATEBOX_PC?.Dispose();
                    Redirector007EverythingOrNothing_NTSC?.Dispose();
                    RedirectorNASCAR08_PS3?.Dispose();
                    RedirectorNASCAR09_PS3?.Dispose();
                    RedirectorLordOfTheRingsTheReturnOfTheKing_NTSC?.Dispose();
                    RedirectorNFLStreet_NTSC?.Dispose();

                    //Matchmakers
                    BurnoutParadisePS3Matchmaker?.Dispose();
                    BurnoutParadisePCUltimateBoxMatchmaker?.Dispose();
                    BurnoutParadisePS3UltimateBoxMatchmaker?.Dispose();
                    LordOfTheRingsTheReturnOfTheKing_NTSC_Matchmaker?.Dispose();
                    EverythingOrNothing007_NTSC_Matchmaker?.Dispose();
                    NFLStreet_NTSCMatchmaker?.Dispose();
                    NFLStreet2_NTSCMatchmaker?.Dispose();
                    NCAAMM06_NTSC_Matchmaker?.Dispose();
                    SimsMatchmaker?.Dispose();
                    SSX3Matchmaker?.Dispose();

                    LoggerAccessor.LogWarn("DirtySocks Servers stopped...");
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~DirtySocksServer()
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
