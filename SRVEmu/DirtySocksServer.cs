using CustomLogger;

namespace SRVEmu
{
    public class DirtySocksServer
    {
        public static bool IsStarted = false;
        AbstractDirtySockServer? RedirectorTSBO_NTSC_A;
        AbstractDirtySockServer? RedirectorTSBO_PAL;
        AbstractDirtySockServer? RedirectorBOPULTIMATEBOX_PS3;
        AbstractDirtySockServer? RedirectorBOP_PS3;
        AbstractDirtySockServer? RedirectorBOP_PC;
        AbstractDirtySockServer? RedirectorBURNOUT3;
        AbstractDirtySockServer? RedirectorBURNOUT3REVIEW;
        AbstractDirtySockServer? RedirectorJAMESBOND;
        AbstractDirtySockServer? RedirectorFIGHTNIGHT2004;
        AbstractDirtySockServer? RedirectorFIGHTNIGHTROUND2;
        AbstractDirtySockServer? RedirectorMADDEN05;
        AbstractDirtySockServer? RedirectorMOHRS;
        AbstractDirtySockServer? RedirectorNFSMW;
        AbstractDirtySockServer? RedirectorSSX3;
        AbstractDirtySockServer? RedirectorNFSU;
        AbstractDirtySockServer? RedirectorNASCAR08;
        AbstractDirtySockServer? RedirectorNASCAR09;
        AbstractDirtySockServer? BurnoutParadiseUltimateBoxMatchmaker;
        AbstractDirtySockServer? BurnoutParadisePS3Matchmaker;
        AbstractDirtySockServer? BurnoutParadisePCMatchmaker;
        AbstractDirtySockServer? Burnout3Matchmaker;
        AbstractDirtySockServer? Burnout3ReviewMatchmaker;
        AbstractDirtySockServer? JamesBondMatchmaker;
        AbstractDirtySockServer? FightNight2004Matchmaker;
        AbstractDirtySockServer? FightNightRound2Matchmaker;
        AbstractDirtySockServer? Madden05Matchmaker;
        AbstractDirtySockServer? MohRSMatchmaker;
        AbstractDirtySockServer? NfsMWMatchmaker;
        AbstractDirtySockServer? SSX3Matchmaker;
        AbstractDirtySockServer? NFSUMatchmaker;
        AbstractDirtySockServer? Nascar08Matchmaker;
        AbstractDirtySockServer? Nascar09Matchmaker;
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
                RedirectorBOP_PC = new RedirectorServer(21841, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 21842, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBOP_PC] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBURNOUT3 = new RedirectorServer(21800, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 21801, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBURNOUT3] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorBURNOUT3REVIEW = new RedirectorServer(21840, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 21845, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorBURNOUT3REVIEW] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorJAMESBOND = new RedirectorServer(11600, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 11601, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorJAMESBOND] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorFIGHTNIGHT2004 = new RedirectorServer(11500, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 11501, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorFIGHTNIGHT2004] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorFIGHTNIGHTROUND2 = new RedirectorServer(21500, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 21501, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorFIGHTNIGHTROUND2] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorMADDEN05 = new RedirectorServer(20000, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 20001, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorMADDEN05] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorMOHRS = new RedirectorServer(14300, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 14301, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorMOHRS] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNFSMW = new RedirectorServer(30900, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 30901, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorNFSMW] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorSSX3 = new RedirectorServer(11000, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 11001, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorSSX3] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNFSU = new RedirectorServer(10900, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 10901, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorNFSU] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNASCAR08 = new RedirectorServer(20600, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 20601, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorNASCAR08] Failed to start! Exception: {ex}");
            }

            try
            {
                RedirectorNASCAR09 = new RedirectorServer(30600, BackendProject.MiscUtils.GetLocalIPAddress().ToString(), 30601, false);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RedirectorNASCAR09] Failed to start! Exception: {ex}");
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
                BurnoutParadisePCMatchmaker = new MatchmakerServer(21842, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BurnoutParadisePCMatchmaker] Failed to start! Exception: {ex}");
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
                Burnout3Matchmaker = new MatchmakerServer(21801, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Burnout3Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                Burnout3ReviewMatchmaker = new MatchmakerServer(21845, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Burnout3ReviewMatchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                JamesBondMatchmaker = new MatchmakerServer(11601, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[JamesBondMatchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                FightNight2004Matchmaker = new MatchmakerServer(11501, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[FightNight2004Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                FightNightRound2Matchmaker = new MatchmakerServer(21501, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[FightNightRound2Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                Madden05Matchmaker = new MatchmakerServer(20001, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Madden05Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                MohRSMatchmaker = new MatchmakerServer(14301, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[MohRSMatchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                NfsMWMatchmaker = new MatchmakerServer(30901, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NfsMWMatchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                SSX3Matchmaker = new MatchmakerServer(11001, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSX3Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                NFSUMatchmaker = new MatchmakerServer(10901, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NFSUMatchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                Nascar08Matchmaker = new MatchmakerServer(20601, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Nascar08Matchmaker] Failed to start! Exception: {ex}");
            }

            try
            {
                Nascar09Matchmaker = new MatchmakerServer(30601, true);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Nascar09Matchmaker] Failed to start! Exception: {ex}");
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

                SimsMatchmaker = new MatchmakerServer(11101, false, SimsRooms);
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
