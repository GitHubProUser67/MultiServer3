using CustomLogger;
using MultiSocks.DirtySocks.DataStore;

namespace MultiSocks.DirtySocks
{
    public class DirtySocksServer : IDisposable
    {
        public static IDatabase? Database = null;
        private readonly AbstractDirtySockServer? RedirectorSSX3_NTSC_A;
        private readonly AbstractDirtySockServer? RedirectorSSX3_PAL;
        private readonly AbstractDirtySockServer? RedirectorTSBO_NTSC_A;
        private readonly AbstractDirtySockServer? RedirectorTSBO_PAL;
        private readonly AbstractDirtySockServer? RedirectorFightNight_NTSC;
        private readonly AbstractDirtySockServer? RedirectorNFLStreet_NTSC;
        private readonly AbstractDirtySockServer? RedirectorNFLStreet2_NTSC;
        private readonly AbstractDirtySockServer? RedirectorNFLStreet3_NTSC;
        private readonly AbstractDirtySockServer? Redirector007EverythingOrNothing_NTSC;
        private readonly AbstractDirtySockServer? RedirectorNCAAMM06_NTSC;
        private readonly AbstractDirtySockServer? RedirectorMaddenNFL06_NTSC;
        private readonly AbstractDirtySockServer? RedirectorBurnout3Takedown_NTSC;
        private readonly AbstractDirtySockServer? RedirectorBurnoutRevenge_NTSC;
        private readonly AbstractDirtySockServer? RedirectorBOP_PS3;
        private readonly AbstractDirtySockServer? RedirectorBOPULTIMATEBOX_PS3;
        private readonly AbstractDirtySockServer? RedirectorBOPULTIMATEBOX_PC;
        private readonly AbstractDirtySockServer? RedirectorLordOfTheRingsTheReturnOfTheKing_NTSC;
        private readonly AbstractDirtySockServer? RedirectorNascarThunder04_NTSC;
        private readonly AbstractDirtySockServer? RedirectorNASCAR08_PS3;
        private readonly AbstractDirtySockServer? RedirectorNASCAR09_PS3;
        private readonly AbstractDirtySockServer? RedirectorHASBROFAMILYGAMENIGHT_PS3;
        private readonly AbstractDirtySockServer? Burnout3Takedown_NTSCMatchmaker;
        private readonly AbstractDirtySockServer? BurnoutRevenge_NTSCMatchmaker;
        private readonly AbstractDirtySockServer? BurnoutParadisePS3Matchmaker;
        private readonly AbstractDirtySockServer? BurnoutParadisePS3UltimateBoxMatchmaker;
        private readonly AbstractDirtySockServer? BurnoutParadisePCUltimateBoxMatchmaker;
        private readonly AbstractDirtySockServer? EverythingOrNothing007_NTSC_Matchmaker;
        private readonly AbstractDirtySockServer? LordOfTheRingsTheReturnOfTheKing_NTSC_Matchmaker;
        private readonly AbstractDirtySockServer? NascarThunder04_NTSC_Matchmaker;
        private readonly AbstractDirtySockServer? NCAAMM06_NTSC_Matchmaker;
        private readonly AbstractDirtySockServer? HASBROFAMILYGAMENIGHTPS3Matchmaker;
        private readonly AbstractDirtySockServer? NFLStreet_NTSCMatchmaker;
        private readonly AbstractDirtySockServer? NFLStreet2_NTSCMatchmaker;
        private readonly AbstractDirtySockServer? NFLStreet3_NTSCMatchmaker;
        private readonly AbstractDirtySockServer? FightNight_NTSCMatchmaker;
        private readonly AbstractDirtySockServer? MaddenNFL06NTSC_Matchmaker;
        private readonly AbstractDirtySockServer? SimsMatchmaker;
        private readonly AbstractDirtySockServer? SSX3Matchmaker;
        private readonly AbstractDirtySockServer? PS2EAMessenger;
        private readonly AbstractDirtySockServer? PS3EAMessenger;
        private bool disposedValue;

        public DirtySocksServer(CancellationToken cancellationToken)
        {
            string ListenIP = MultiSocksServerConfiguration.UsePublicIPAddress ? CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();

            Database = new DirtySocksJSONDatabase();

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

            #region Fight Night 
            try
            {
                RedirectorFightNight_NTSC = new RedirectorServer(11500, ListenIP, 11501, false, "KKING-PS2-2004", "PS2", false, "ps2kok04.ea.com", "ps2kok04@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] Fight Night NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] Fight Night NTSC Failed to start! Exception: {ex}");
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
                LoggerAccessor.LogInfo($"[Redirector] NFL Street 2 NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NFL Street 2 NTSC Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNFLStreet3_NTSC = new RedirectorServer(11700, ListenIP, 11701, false, "NFLSTREET3-PS2-2007", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] NFL Street 3 NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NFL Street 3 NTSC Failed to start! Exception: {ex}");
            }

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
                RedirectorBOP_PS3 = new RedirectorServer(21850, ListenIP, 21851, false, "BURNOUT5", "PS3", false, "ps3burnout08.ea.com", "ps3burnout08@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] BOP PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] BOP PS3 Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOPULTIMATEBOX_PS3 = new RedirectorServer(21870, ListenIP, 21871, false, "BURNOUT5", "PS3", false, "ps3burnout08.ea.com", "ps3burnout08@ea.com");
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
                RedirectorBOPULTIMATEBOX_PC = new RedirectorServer(21841, ListenIP, 21842, false, "BURNOUT5", "PC", true, "pcburnout08.ea.com", "pcburnout08@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] BOPULTIMATEBOX PC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] BOPULTIMATEBOX PC Failed to start! Exception: {ex}");
            }
            #endregion

            #region Nascar

            try
            {
                RedirectorNascarThunder04_NTSC = new RedirectorServer(10600, ListenIP, 10601, false, "NASCAR-PS2-2004", "PS2", false, "ps2nascar04.ea.com", "ps2nascar04@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] NASCAR Thunder 04 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NASCAR08 Thunder 04 Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNASCAR08_PS3 = new RedirectorServer(20651, ListenIP, 20652, false, "NASCAR08", "PS3", true, "ps3nascar08.ea.com", "ps3nascar08@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] NASCAR08 PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NASCAR08 PS3 Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNASCAR09_PS3 = new RedirectorServer(30671, ListenIP, 30672, false, "NASCAR09", "PS3", true, "ps3nascar09.ea.com", "ps3nascar09@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] NASCAR09 PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NASCAR09 PS3 Failed to start! Exception: {ex}");
            }
            #endregion

            #region Hasbro Family Game Night PS3
            try
            {
                RedirectorHASBROFAMILYGAMENIGHT_PS3 = new RedirectorServer(32950, ListenIP, 32951, false, "DPR-09", "PS3");
                LoggerAccessor.LogInfo($"[Redirector] HASBROFAMILYGAMENIGHT PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] HASBROFAMILYGAMENIGHT PS3 Failed to start! Exception: {ex}");
            }
            #endregion

            #region NCAA March Madness 06
            try
            {
                RedirectorNCAAMM06_NTSC = new RedirectorServer(30700, ListenIP, 30701, false, "PS2-MM-2006", "PS2", false, "ps2mm06.ea.com", "ps2mm06@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] NCAA March Madness 06 NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NCAA March Madness 06 NTSC Failed to start! Exception: {ex}");
            }
            #endregion

            #region Madden NFL 06
            try
            {
                RedirectorMaddenNFL06_NTSC = new RedirectorServer(30000, ListenIP, 30001, false, "PS2-MM-2006", "PS2", false, "ps2madden06.ea.com", "ps2madden06@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] Madden NFL 06 NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] Madden NFL 06 NTSC Failed to start! Exception: {ex}");
            }
            #endregion

            #region Burnout 3 Takedown
            try
            {
                RedirectorBurnoutRevenge_NTSC = new RedirectorServer(21800, ListenIP, 21801, false, "BR-PS2-2004", "PS2", false, "ps2burnout05.ea.com", "ps2burnout05@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] Burnout 3 Takedown NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] Burnout 3 Takedown NTSC Failed to start! Exception: {ex}");
            }
            #endregion

            #region Burnout Revenge
            try
            {
                RedirectorBurnoutRevenge_NTSC = new RedirectorServer(31800, ListenIP, 31801, false, "BR-PS2-2004", "PS2", false, "ps2burnout06.ea.com", "ps2burnout06@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] Burnout Revenge NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] Burnout Revenge NTSC Failed to start! Exception: {ex}");
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
			
            #endregion

            #region EA.com Messenger Buddy Server
            try
            {
                PS2EAMessenger = new EAMessengerServer(10899, true, null, null, "PS2");
                LoggerAccessor.LogInfo($"[PS2EAMessenger] Buddy Service Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[PS2EAMessenger] Buddy Service Failed to start! Exception: {ex}");
            }

            try
            {
                PS3EAMessenger = new EAMessengerServer(13505, true, null, null, "PS3");
                LoggerAccessor.LogInfo($"[PS3EAMessenger] Buddy Service Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[PS3EAMessenger] Buddy Service Failed to start! Exception: {ex}");
            }
            #endregion

            #region Matchmaker


            try
            {
                NascarThunder04_NTSC_Matchmaker = new MatchmakerServer(10601, true, null, "NASCAR-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Nascar Thunder 04 NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

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
                NFLStreet3_NTSCMatchmaker = new MatchmakerServer(11701, true, null, "NFLSTREET3-PS2-2007", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFL Street NTSC Matchmaker] Failed to start! Exception: {ex}");
            }


            #region Madden NFL 06
            try
            {
                MaddenNFL06NTSC_Matchmaker = new MatchmakerServer(30001, true, null, "MADDEN-06", "PS2", false);

                LoggerAccessor.LogInfo($"[Madden NFL 06 Matchmaker] Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Madden NFL 06 Matchmaker] Failed to start! Exception: {ex}");
            }
            #endregion

            try
            {
                FightNight_NTSCMatchmaker = new MatchmakerServer(11501, true, null, "MADDEN-06", "PS2", false);

                LoggerAccessor.LogInfo($"[Fight Night 04 Matchmaker] Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Fight Night 04 Matchmaker] Failed to start! Exception: {ex}");
            }


            try
            {
                Burnout3Takedown_NTSCMatchmaker = new MatchmakerServer(21801, true, null, "BR-2005-PS2", "PS2", false);

                LoggerAccessor.LogInfo($"[Burnout Revenge Matchmaker] Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Burnout Revenge Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutRevenge_NTSCMatchmaker = new MatchmakerServer(31801, true, null, "BR-2005-PS2", "PS2", false);

                LoggerAccessor.LogInfo($"[Burnout Revenge Matchmaker] Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Burnout Revenge Matchmaker] Failed to start! Exception: {ex}");
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
                NCAAMM06_NTSC_Matchmaker = new MatchmakerServer(30701, true, null, "MM05", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFL Street NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                HASBROFAMILYGAMENIGHTPS3Matchmaker = new MatchmakerServer(32951, true, null, "DPR-09", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Hasbro Family Game Night NTSC Matchmaker] Failed to start! Exception: {ex}");
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
                LoggerAccessor.LogError($"[BurnoutParadise PS3 UltimateBox Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePCUltimateBoxMatchmaker = new MatchmakerServer(21842, true, null, "BURNOUT5", "PC");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadise PC UltimateBox Matchmaker] Failed to start! Exception: {ex}");
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
                    RedirectorFightNight_NTSC?.Dispose();
                    RedirectorNCAAMM06_NTSC?.Dispose();
                    RedirectorMaddenNFL06_NTSC?.Dispose();
                    RedirectorBurnoutRevenge_NTSC?.Dispose();
                    RedirectorBOP_PS3?.Dispose();
                    RedirectorBOPULTIMATEBOX_PS3?.Dispose();
                    RedirectorBOPULTIMATEBOX_PC?.Dispose();
                    Redirector007EverythingOrNothing_NTSC?.Dispose();
                    RedirectorNASCAR08_PS3?.Dispose();
                    RedirectorNASCAR09_PS3?.Dispose();
                    RedirectorHASBROFAMILYGAMENIGHT_PS3?.Dispose();
                    RedirectorLordOfTheRingsTheReturnOfTheKing_NTSC?.Dispose();
                    RedirectorNFLStreet_NTSC?.Dispose();
                    RedirectorNFLStreet2_NTSC?.Dispose();
                    RedirectorNFLStreet3_NTSC?.Dispose();

                    //Matchmakers
                    BurnoutRevenge_NTSCMatchmaker?.Dispose();
                    BurnoutParadisePS3Matchmaker?.Dispose();
                    BurnoutParadisePCUltimateBoxMatchmaker?.Dispose();
                    BurnoutParadisePS3UltimateBoxMatchmaker?.Dispose();
                    FightNight_NTSCMatchmaker?.Dispose();
                    LordOfTheRingsTheReturnOfTheKing_NTSC_Matchmaker?.Dispose();
                    EverythingOrNothing007_NTSC_Matchmaker?.Dispose();
                    NFLStreet_NTSCMatchmaker?.Dispose();
                    NFLStreet2_NTSCMatchmaker?.Dispose();
                    NFLStreet3_NTSCMatchmaker?.Dispose();
                    MaddenNFL06NTSC_Matchmaker?.Dispose();
                    NCAAMM06_NTSC_Matchmaker?.Dispose();
                    HASBROFAMILYGAMENIGHTPS3Matchmaker?.Dispose();
                    SimsMatchmaker?.Dispose();
                    SSX3Matchmaker?.Dispose();

                    //EA.COM Buddy Server
                    PS2EAMessenger?.Dispose();
                    PS3EAMessenger?.Dispose();

                    //Database
                    Database = null;

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
