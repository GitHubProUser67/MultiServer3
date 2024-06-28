using CustomLogger;
using MultiSocks.Aries.DataStore;
using MultiSocks.Aries.SDK_v1;
using MultiSocks.Aries.SDK_v6;

namespace MultiSocks.Aries
{
    public class AriesServer : IDisposable
    {
        public static IDatabase? Database = null;

        //Redirector
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorSSX3_NTSC_A;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorSSX3_PAL;
        private readonly SDK_v1.AbstractAriesServerV1? RedirectorTSBO_NTSC_A;
        private readonly SDK_v1.AbstractAriesServerV1? RedirectorTSBO_PAL;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorFightNight_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorFightNightR2_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorFifa06_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorNFLStreet_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorNFLStreet2_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorNFLStreet3_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? Redirector007EverythingOrNothing_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorNCAAMM06_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorMaddenNFL06_NTSC;
        private readonly SDK_v1.AbstractAriesServerV1? RedirectorBurnout3Takedown_NTSC;
        private readonly SDK_v1.AbstractAriesServerV1? RedirectorBurnoutRevenge_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorBOP_PS3;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorBOPULTIMATEBOX_PS3;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorBOPULTIMATEBOX_PC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorLordOfTheRingsTheReturnOfTheKing_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectoNFSMWA124_PAL;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorHASBROFAMILYGAMENIGHT_PS3;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorNascarThunder04_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorMarvelNemesis06_NTSC;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorNASCAR08_PS3;
        private readonly SDK_v6.AbstractAriesServerV6? RedirectorNASCAR09_PS3;


        //Matchmaker
        private readonly SDK_v1.AbstractAriesServerV1? Burnout3Takedown_NTSCMatchmaker;
        private readonly SDK_v1.AbstractAriesServerV1? BurnoutRevenge_NTSCMatchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? BurnoutParadisePS3Matchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? BurnoutParadisePS3UltimateBoxMatchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? BurnoutParadisePCUltimateBoxMatchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? EverythingOrNothing007_NTSC_Matchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? LordOfTheRingsTheReturnOfTheKing_NTSC_Matchmaker;
        private readonly SDK_v1.AbstractAriesServerV1? NascarThunder04_NTSC_Matchmaker;
        private readonly SDK_v1.AbstractAriesServerV1? NCAAMM06_NTSC_Matchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? HASBROFAMILYGAMENIGHTPS3Matchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? NFLStreet_NTSCMatchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? NFLStreet2_NTSCMatchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? NFLStreet3_NTSCMatchmaker;
        private readonly SDK_v1.AbstractAriesServerV1? FightNight_NTSCMatchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? FightNightR2_NTSCMatchmaker;
        private readonly SDK_v1.AbstractAriesServerV1? Fifa06_NTSC_Matchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? MaddenNFL06_NTSC_Matchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? NFSMWA124_PAL_Matchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? MarvelNemesis_NTSC_Matchmaker;
        private readonly SDK_v1.AbstractAriesServerV1? SimsMatchmaker;
        private readonly SDK_v6.AbstractAriesServerV6? SSX3Matchmaker;

        private bool disposedValue;

        public AriesServer()
        {
            string ListenIP = MultiSocksServerConfiguration.UsePublicIPAddress ? CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();

            Database = new DirtySocksJSONDatabase();

            #region Redirector

            #region SSX3 PS2
            try
            {
                RedirectorSSX3_NTSC_A = new RedirectorServerV6(11000, ListenIP, 11051, "SSX-ER-PS2-2004", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] SSX3_NTSC_A Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] SSX3_NTSC_A Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorSSX3_PAL = new RedirectorServerV6(11050, ListenIP, 11051, "SSX-ER-PS2-2004", "PS2");
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
                RedirectorTSBO_NTSC_A = new RedirectorServerV1(11100, ListenIP, 11101, "SIMS-BO-NA-PS2-2004", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] TSBO_NTSC_A Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] TSBO_NTSC_A Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorTSBO_PAL = new RedirectorServerV1(11140, ListenIP, 11101, "SIMS-BO-EU-PS2-2004", "PS2");
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
                RedirectorFightNight_NTSC = new RedirectorServerV6(11500, ListenIP, 11501, "KKING-PS2-2004", "PS2", false, "ps2kok04.ea.com", "ps2kok04@ea.com");
                RedirectorFightNightR2_NTSC = new RedirectorServerV6(21500, ListenIP, 21501, "KKING-PS2-2005", "PS2", false, "ps2kok05.ea.com", "ps2kok05@ea.com");

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
                RedirectorNFLStreet_NTSC = new RedirectorServerV6(11300, ListenIP, 11301, "NFLSTREET-PS2-2005", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] NFL Street NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NFL Street NTSC Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNFLStreet2_NTSC = new RedirectorServerV6(21301, ListenIP, 21302, "NFLSTREET-PS2-2005", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] NFL Street 2 NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NFL Street 2 NTSC Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNFLStreet3_NTSC = new RedirectorServerV6(11700, ListenIP, 11701, "NFLSTREET3-PS2-2007", "PS2");
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
                RedirectorLordOfTheRingsTheReturnOfTheKing_NTSC = new RedirectorServerV6(11200, ListenIP, 11201, "LOTR", "PS2", false, "ps2rotk04.ea.com", "ps2rotk04@ea.com");
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
                RedirectorBOP_PS3 = new RedirectorServerV6(21850, ListenIP, 21851, "BURNOUT5", "PS3", false, "ps3burnout08.ea.com", "ps3burnout08@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] BOP PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] BOP PS3 Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBOPULTIMATEBOX_PS3 = new RedirectorServerV6(21870, ListenIP, 21871, "BURNOUT5", "PS3", false, "ps3burnout08.ea.com", "ps3burnout08@ea.com");
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
                RedirectorBOPULTIMATEBOX_PC = new RedirectorServerV6(21841, ListenIP, 21842, "BURNOUT5", "PC", true, "pcburnout08.ea.com", "pcburnout08@ea.com");
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
                RedirectorNascarThunder04_NTSC = new RedirectorServerV6(10600, ListenIP, 10601, "NASCAR-PS2-2004", "PS2", false, "ps2nascar04.ea.com", "ps2nascar04@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] NASCAR Thunder 04 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NASCAR08 Thunder 04 Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNASCAR08_PS3 = new RedirectorServerV6(20651, ListenIP, 20652, "NASCAR08", "PS3", true, "ps3nascar08.ea.com", "ps3nascar08@ea.com");
                LoggerAccessor.LogInfo($"[Redirector] NASCAR08 PS3 Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] NASCAR08 PS3 Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNASCAR09_PS3 = new RedirectorServerV6(30671, ListenIP, 30672, "NASCAR09", "PS3", true, "ps3nascar09.ea.com", "ps3nascar09@ea.com");
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
                RedirectorHASBROFAMILYGAMENIGHT_PS3 = new RedirectorServerV6(32950, ListenIP, 32951, "DPR-09", "PS3");
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
                RedirectorNCAAMM06_NTSC = new RedirectorServerV6(30700, ListenIP, 30701, "PS2-MM-2006", "PS2", false, "ps2mm06.ea.com", "ps2mm06@ea.com");

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
                RedirectorMaddenNFL06_NTSC = new RedirectorServerV6(30000, ListenIP, 30001, "PS2-MM-2006", "PS2", false, "ps2madden06.ea.com", "ps2madden06@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] Madden NFL 06 NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] Madden NFL 06 NTSC Failed to start! Exception: {ex}");
            }
            #endregion

            #region Fifa 06
            try
            {
                RedirectorFifa06_NTSC = new RedirectorServerV6(30400, ListenIP, 30401, "PS2-MM-2006", "PS2", false, "ps2fifa06.ea.com", "ps2fifa06@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] Fifa 06 NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] Fifa 06 NTSC Failed to start! Exception: {ex}");
            }
            #endregion

            #region Marvel Nemesis 
            try
            {
                RedirectorMarvelNemesis06_NTSC = new RedirectorServerV6(31700, ListenIP, 31701, "NFLSTREET-PS2-2005", "PS2");
                LoggerAccessor.LogInfo($"[Redirector] Marvel Nemesis - Rise of the Imperfects NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] Marvel Nemesis - Rise of the Imperfects NTSC Failed to start! Exception: {ex}");
            }

            #endregion

            #region Burnout 3 Takedown
            try
            {
                RedirectorBurnoutRevenge_NTSC = new RedirectorServerV1(21800, ListenIP, 21801, "BR-PS2-2004", "PS2", false, "ps2burnout05.ea.com", "ps2burnout05@ea.com");

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
                RedirectorBurnoutRevenge_NTSC = new RedirectorServerV1(31800, ListenIP, 31801, "BR-PS2-2004", "PS2", false, "ps2burnout06.ea.com", "ps2burnout06@ea.com");

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
                Redirector007EverythingOrNothing_NTSC = new RedirectorServerV6(11600, ListenIP, 11601, "PS2-BOND-2004", "PS2", false, "ps2bond04.ea.com", "ps2bond04@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] 007 Everything Or Nothing NTSC Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] 007 Everything Or Nothing NTSC Failed to start! Exception: {ex}");
            }
            #endregion

            #region NFS:MW A124
            try
            {
                RedirectoNFSMWA124_PAL = new RedirectorServerV6(30900, ListenIP, 30901, "nfs-ps2-2006", "PS2", false, "ps2nfs06.ea.com", "ps2nfs06@ea.com");

                LoggerAccessor.LogInfo($"[Redirector] Need for Speed: Most Wanted Alpha 124 PAL Started!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Redirector] Need for Speed: Most Wanted Alpha 124 PAL Failed to start! Exception: {ex}");
            }
            #endregion

            #endregion

            #region Matchmaker
            try
            {
                NascarThunder04_NTSC_Matchmaker = new MatchmakerServerV1(10601, ListenIP, new List<Tuple<string, bool>>()
                {
                    new("East", true),
                }, "NASCAR-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Nascar Thunder 04 NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                NFLStreet_NTSCMatchmaker = new MatchmakerServerV6(11301, ListenIP, "NFLSTREET-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFL Street NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                NFLStreet2_NTSCMatchmaker = new MatchmakerServerV6(21302, ListenIP, "NFLSTREET-PS2-2005", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFL Street NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                NFLStreet3_NTSCMatchmaker = new MatchmakerServerV6(11701, ListenIP, "NFLSTREET3-PS2-2007", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFL Street NTSC Matchmaker] Failed to start! Exception: {ex}");
            }


            #region Madden NFL 06
            try
            {
                MaddenNFL06_NTSC_Matchmaker = new MatchmakerServerV6(30001, ListenIP, "MADDEN-06", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Madden NFL 06 Matchmaker] Failed to start! Exception: {ex}");
            }
            #endregion

            try
            {
                FightNight_NTSCMatchmaker = new MatchmakerServerV1(11501, ListenIP, null, "KKING-PS2-2004", "PS2", false);
                FightNightR2_NTSCMatchmaker = new MatchmakerServerV6(21501, ListenIP, "KKING-PS2-2005", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Fight Night 04 Matchmaker] Failed to start! Exception: {ex}");
            }


            try
            {
                Burnout3Takedown_NTSCMatchmaker = new MatchmakerServerV1(21801, ListenIP, null, "BR-2005-PS2", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Burnout Revenge Matchmaker] Failed to start! Exception: {ex}");
            }


            try
            {
                MarvelNemesis_NTSC_Matchmaker = new MatchmakerServerV6(31701, ListenIP, "NASCAR-PS2-2004", "PS2");
                /*
                MarvelNemesis_NTSC_Matchmaker = new MatchmakerServer(31701, ListenIP, new List<Tuple<string, bool>>()
                {
                    new("Earth", true)
                    }, "NASCAR-PS2-2004", "PS2");
                */
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Marvel Nemesis NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutRevenge_NTSCMatchmaker = new MatchmakerServerV1(31801, ListenIP, null, "BR-2005-PS2", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Burnout Revenge Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                Fifa06_NTSC_Matchmaker = new MatchmakerServerV1(30401, ListenIP, null, "BR-2005-PS2", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Fifa 06 Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                NFSMWA124_PAL_Matchmaker = new MatchmakerServerV6(30901, ListenIP, "nfs-ps2-2006", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Matchmaker] Need for Speed: Most Wanted A124 Failed to start! Exception: {ex}");
            }

            try
            {
                LordOfTheRingsTheReturnOfTheKing_NTSC_Matchmaker = new MatchmakerServerV6(11201, ListenIP, "PS2_ROTK_2004", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Everything Or Nothing 007 NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                NCAAMM06_NTSC_Matchmaker = new MatchmakerServerV1(30701, ListenIP, null, "MM05", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFL Street NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                HASBROFAMILYGAMENIGHTPS3Matchmaker = new MatchmakerServerV6(32951, ListenIP, "DPR-09", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Hasbro Family Game Night NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                EverythingOrNothing007_NTSC_Matchmaker = new MatchmakerServerV6(11601, ListenIP, "PS2-BOND-2004", "PS2", false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Everything Or Nothing 007 NTSC Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePS3Matchmaker = new MatchmakerServerV6(21851, ListenIP, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadise PS3 Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePS3UltimateBoxMatchmaker = new MatchmakerServerV6(21871, ListenIP, "BURNOUT5", "PS3");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadise PS3 UltimateBox Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                BurnoutParadisePCUltimateBoxMatchmaker = new MatchmakerServerV6(21842, ListenIP, "BURNOUT5", "PC");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadise PC UltimateBox Matchmaker] Failed to start! Exception: {ex}");
            }


            try
            {
                SimsMatchmaker = new MatchmakerServerV1(11101, ListenIP, new List<Tuple<string, bool>>()
                {
                    new("Veronaville", true),
                    new("Strangetown", true),
                    new("Pleasantview", true),
                    new("Belladonna Cove", true),
                    new("Riverblossom Hills", true)
                }, "SIMS-BO-NA-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SimsMatchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                SSX3Matchmaker = new MatchmakerServerV6(11051, ListenIP, "SSX-ER-PS2-2004", "PS2");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SimsMatchmaker] Failed to start! Exception: {ex}");
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
                    RedirectorSSX3_NTSC_A?.Dispose();
                    RedirectorSSX3_PAL?.Dispose();
                    RedirectorTSBO_NTSC_A?.Dispose();
                    RedirectorTSBO_PAL?.Dispose();
                    RedirectorFifa06_NTSC?.Dispose();
                    RedirectorFightNight_NTSC?.Dispose();
                    RedirectorFightNightR2_NTSC?.Dispose();
                    RedirectorNCAAMM06_NTSC?.Dispose();
                    RedirectorMaddenNFL06_NTSC?.Dispose();
                    RedirectorBurnout3Takedown_NTSC?.Dispose();
                    RedirectorBurnoutRevenge_NTSC?.Dispose();
                    RedirectorBOP_PS3?.Dispose();
                    RedirectorBOPULTIMATEBOX_PS3?.Dispose();
                    RedirectorBOPULTIMATEBOX_PC?.Dispose();
                    Redirector007EverythingOrNothing_NTSC?.Dispose();
                    RedirectorMarvelNemesis06_NTSC?.Dispose();
                    RedirectorNascarThunder04_NTSC?.Dispose();
                    RedirectorNASCAR08_PS3?.Dispose();
                    RedirectorNASCAR09_PS3?.Dispose();
                    RedirectoNFSMWA124_PAL?.Dispose();
                    RedirectorHASBROFAMILYGAMENIGHT_PS3?.Dispose();
                    RedirectorLordOfTheRingsTheReturnOfTheKing_NTSC?.Dispose();
                    RedirectorNFLStreet_NTSC?.Dispose();
                    RedirectorNFLStreet2_NTSC?.Dispose();
                    RedirectorNFLStreet3_NTSC?.Dispose();

                    //Matchmakers
                    Burnout3Takedown_NTSCMatchmaker?.Dispose();
                    BurnoutRevenge_NTSCMatchmaker?.Dispose();
                    BurnoutParadisePS3Matchmaker?.Dispose();
                    BurnoutParadisePCUltimateBoxMatchmaker?.Dispose();
                    BurnoutParadisePS3UltimateBoxMatchmaker?.Dispose();
                    Fifa06_NTSC_Matchmaker?.Dispose();
                    FightNight_NTSCMatchmaker?.Dispose();
                    FightNightR2_NTSCMatchmaker?.Dispose();
                    LordOfTheRingsTheReturnOfTheKing_NTSC_Matchmaker?.Dispose();
                    EverythingOrNothing007_NTSC_Matchmaker?.Dispose();
                    NFLStreet_NTSCMatchmaker?.Dispose();
                    NFLStreet2_NTSCMatchmaker?.Dispose();
                    NFLStreet3_NTSCMatchmaker?.Dispose();
                    NascarThunder04_NTSC_Matchmaker?.Dispose();
                    MaddenNFL06_NTSC_Matchmaker?.Dispose();
                    MarvelNemesis_NTSC_Matchmaker?.Dispose();
                    NCAAMM06_NTSC_Matchmaker?.Dispose();
                    NFSMWA124_PAL_Matchmaker?.Dispose();
                    HASBROFAMILYGAMENIGHTPS3Matchmaker?.Dispose();
                    SimsMatchmaker?.Dispose();
                    SSX3Matchmaker?.Dispose();

                    //EA.COM Buddy Server
                    //PS2EAMessenger?.Dispose();
                    //PS3EAMessenger?.Dispose();

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
