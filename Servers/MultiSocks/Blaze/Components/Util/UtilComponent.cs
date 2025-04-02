using Blaze3SDK.Blaze;
using Blaze3SDK.Blaze.Util;
using Blaze3SDK.Components;
using BlazeCommon;
using CustomLogger;

namespace MultiSocks.Blaze.Components.Util
{
    internal class UtilComponent : UtilComponentBase.Server
    {
        public override Task<PreAuthResponse> PreAuthAsync(PreAuthRequest request, BlazeRpcContext context)
        {
#if DEBUG
            LoggerAccessor.LogInfo($"[Blaze] - Util: Connection Id    : {context.Connection.ID}");
            LoggerAccessor.LogInfo($"[Blaze] - Util: Locale     : {request.mClientData.mLocale}");
            LoggerAccessor.LogInfo($"[Blaze] - Util: Client Type      : {request.mClientData.mClientType}");
            LoggerAccessor.LogInfo($"[Blaze] - Util: Client Platform  : {request.mClientInfo.mPlatform}");
            LoggerAccessor.LogInfo($"[Blaze] - Util: Client mEnvironment  : {request.mClientInfo.mEnvironment}");
#endif

            var fetchConfig = new SortedDictionary<string, string>
            {
                { "pingPeriod", "15s" },
                { "voipHeadsetUpdateRate", "1000" },
                { "xlspConnectionIdleTimeout", "300" }
            };

            QosPingSiteInfo qosPingSiteInfoSJC = new QosPingSiteInfo()
            {
                mAddress = "gossjcprod-qos01.ea.com",
                mPort = 17502,
                mSiteName = "prod-sjc",
            };

            QosPingSiteInfo qosPingSiteInfoIAD = new QosPingSiteInfo()
            {
                mAddress = "gosiadprod-qos01.ea.com",
                mPort = 17502,
                mSiteName = "bio-iad-prod-shared",
            };
            QosPingSiteInfo qosPingSiteInfolhr = new QosPingSiteInfo()
            {
                mAddress = "gosgvaprod-qos01.ea.com",
                mPort = 17502,
                mSiteName = "bio-dub-prod-shared",
            };

            var pingSiteInfoByAliasNames = new SortedDictionary<string, QosPingSiteInfo>
            {
                { "ea-sjc", qosPingSiteInfoSJC },
                { "rs-iad", qosPingSiteInfoIAD },
                { "rs-lhr", qosPingSiteInfolhr }
            };

            return Task.FromResult(new PreAuthResponse()
            {
                mAnonymousChildAccountsEnabled = false,
                mAuthenticationSource = "303107",

                //ushort list
                mComponentIds = new List<ushort>()
                {
                    1,
                    4,
                    7,
                    9,
                    15,
                    25,
                    28,
                    2000,
                    30720,
                    30721,
                    30722,
                    30723,
                    30725,
                    30726,
                    63490
                },
                mConfig = new FetchConfigResponse()
                {
                    mConfig = fetchConfig,
                },
                mInstanceName = "masseffect-3-ps3",
                mLegalDocGameIdentifier = "GameID",
                mParentalConsentEntitlementGroupName = "PCEGN",
                mParentalConsentEntitlementTag = "PCETAG",
                mPersonaNamespace = "cem_ea_id",
                mPlatform = "PS3",
                mQosSettings = new()
                {
                    mBandwidthPingSiteInfo = new QosPingSiteInfo()
                    {
                        mAddress = "gossjcprod-qos01.ea.com",
                        mPort = 17502,
                        mSiteName = "prod-sjc",
                    },
                    mNumLatencyProbes = 10,
                    mPingSiteInfoByAliasMap = pingSiteInfoByAliasNames,
                    mServiceId = 1
                },
                mServerVersion = "Blaze 3.15.08.0 (CL# 750727)",
                mUnderageSupported = false,

            });
        }

        public override Task<PostAuthResponse> PostAuthAsync(NullStruct request, BlazeRpcContext context)
        {
            byte[] skey = { 0x5E, 0x8A, 0xCB, 0xDD, 0xF8, 0xEC, 0xC1, 0x95, 0x98, 0x99, 0xF9, 0x94, 0xC0, 0xAD, 0xEE, 0xFC, 0xCE, 0xA4, 0x87, 0xDE, 0x8A, 0xA6, 0xCE, 0xDC, 0xB0, 0xEE, 0xE8, 0xE5, 0xB3, 0xF5, 0xAD, 0x9A, 0xB2, 0xE5, 0xE4, 0xB1, 0x99, 0x86, 0xC7, 0x8E, 0x9B, 0xB0, 0xF4, 0xC0, 0x81, 0xA3, 0xA7, 0x8D, 0x9C, 0xBA, 0xC2, 0x89, 0xD3, 0xC3, 0xAC, 0x98, 0x96, 0xA4, 0xE0, 0xC0, 0x81, 0x83, 0x86, 0x8C, 0x98, 0xB0, 0xE0, 0xCC, 0x89, 0x93, 0xC6, 0xCC, 0x9A, 0xE4, 0xC8, 0x99, 0xE3, 0x82, 0xEE, 0xD8, 0x97, 0xED, 0xC2, 0xCD, 0x9B, 0xD7, 0xCC, 0x99, 0xB3, 0xE5, 0xC6, 0xD1, 0xEB, 0xB2, 0xA6, 0x8B, 0xB8, 0xE3, 0xD8, 0xC4, 0xA1, 0x83, 0xC6, 0x8C, 0x9C, 0xB6, 0xF0, 0xD0, 0xC1, 0x93, 0x87, 0xCB, 0xB2, 0xEE, 0x88, 0x95, 0xD2, 0x80, 0x80 };
            string skeys = string.Empty;
            foreach (byte b in skey)
                skeys += (char)b;

            PssConfig pssConfig = new PssConfig()
            {
                mAddress = "playersyncservices.ea.com",
                mInitialReportTypes = PssReportTypes.None,
                mNpCommSignature = null,
                mOfferIds = null,
                mPort = 443,
                mProjectId = "303107",
                mTitleId = 0,
            };

            GetTelemetryServerResponse getTelemetryServerResponse = new GetTelemetryServerResponse()
            {
                mAddress = "telemetry.ea.com", //"159.153.235.32",
                mDisable = "AD,AF,AG,AI,AL,AM,AN,AO,AQ,AR,AS,AW,AX,AZ,BA,BB,BD,BF,BH,BI,BJ,BM,BN,BO,BR,BS,BT,BV,BW,BY,BZ,CC,CD,CF,CG,CI,CK,CL,CM,CN,CO,CR,CU,CV,CX,DJ,DM,DO,DZ,EC,EG,EH,ER,ET,FJ,FK,FM,FO,GA,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GS,GT,GU,GW,GY,HM,HN,HT,ID,IL,IM,IN,IO,IQ,IR,IS,JE,JM,JO,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LY,MA,MC,MD,ME,MG,MH,ML,MM,MN,MO,MP,MQ,MR,MS,MU,MV,MW,MY,MZ,NA,NC,NE,NF,NG,NI,NP,NR,NU,OM,PA,PE,PF,PG,PH,PK,PM,PN,PS,PW,PY,QA,RE,RS,RW,SA,SB,SC,SD,SG,SH,SJ,SL,SM,SN,SO,SR,ST,SV,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TT,TV,TZ,UA,UG,UM,UY,UZ,VA,VC,VE,VG,VN,VU,WF,WS,YE,YT,ZM,ZW,ZZ",
                mFilter = "-UION/****",
                mIsAnonymous = true,
                mKey = skeys,
                mLocale = 1701725253,
                mNoToggleOk = "US,CA,MX",
                mPort = 9988,
                mSendDelay = 15000,
                mSendPercentage = 75,
                mSessionID = "JMhnT9dXSED",
                mUseServerTime = "",
            };

            GetTickerServerResponse getTickerServerResponse = new GetTickerServerResponse()
            {
                mAddress = "ticker.ea.com",
                mKey = "823287263,10.23.15.2:8999,masseffect-3-ps3,10,50,50,50,50,0,12",
                mPort = 8999
            };

            UserOptions userOptions = new UserOptions()
            {
                mTelemetryOpt = TelemetryOpt.TELEMETRY_OPT_IN,
                mUserId = 1
            };

            return Task.FromResult(new PostAuthResponse()
            {
                mPssConfig = pssConfig,
                mTelemetryServer = getTelemetryServerResponse,
                mTickerServer = getTickerServerResponse,
                mUserOptions = userOptions,

            });
        }

        public override Task<NullStruct> SetClientMetricsAsync(ClientMetrics request, BlazeRpcContext context)
        {
#if DEBUG
            LoggerAccessor.LogInfo($"[Blaze] - ClientMetrics: Blaze Flags         : {request.mBlazeFlags}");
            LoggerAccessor.LogInfo($"[Blaze] - ClientMetrics: Device Info         : {request.mDeviceInfo}");
            LoggerAccessor.LogInfo($"[Blaze] - ClientMetrics: Flags               : {request.mFlags}");
            LoggerAccessor.LogInfo($"[Blaze] - ClientMetrics: Last Result Code    : {request.mLastRsltCode}");
            LoggerAccessor.LogInfo($"[Blaze] - ClientMetrics: Nat Type            : {request.mNatType}");
            LoggerAccessor.LogInfo($"[Blaze] - ClientMetrics: UPNP Status         : {request.mStatus}");
            LoggerAccessor.LogInfo($"[Blaze] - ClientMetrics: WAN IP Addr         : {request.mWanIpAddr}");
#endif
            return Task.FromResult(new NullStruct()
            {

            });
        }

        public override Task<FetchConfigResponse> FetchClientConfigAsync(FetchClientConfigRequest request, BlazeRpcContext context)
        {
#if DEBUG
            LoggerAccessor.LogInfo($"[Blaze] - Util: Connection Id    : {context.Connection.ID}");
            LoggerAccessor.LogInfo($"[Blaze] - Util: mConfigSection    : {request.mConfigSection}");
#endif
            string ME3ClientConfig = Directory.GetCurrentDirectory() + "/static/EA/ME3_CONFIG/";
            string BF4OBClientConfig = Directory.GetCurrentDirectory() + "/static/EA/BF4_OB/";

            Directory.CreateDirectory(ME3ClientConfig);
            Directory.CreateDirectory(BF4OBClientConfig);

            string fileBF4PathFull = BF4OBClientConfig + request.mConfigSection + ".txt";
            string fileME3PathFull = ME3ClientConfig + request.mConfigSection + ".txt";

            var fileClientConfigDictionary = new SortedDictionary<string, string>();

            switch(request.mConfigSection)
            {

                case "IdentityParams":
                    if (File.Exists(fileBF4PathFull))
                    {
                        string[] fileConfig = File.ReadAllLines(fileBF4PathFull);

                        for (int i = 0; i < fileConfig.Length; i++)
                        {
                            string[] parts = fileConfig[i].Split(';');

                            fileClientConfigDictionary.Add(parts[0].Trim(), parts[1].Trim());
                        }
                    }
                    else
                    {
                        LoggerAccessor.LogWarn($"File not found! Path expected: {fileBF4PathFull}");
                    }
                    break;
                case "ME3_DATA":
                    if (File.Exists(fileME3PathFull))
                    {
                        string[] fileConfig = File.ReadAllLines(fileME3PathFull);

                        for (int i = 0; i < fileConfig.Length; i++)
                        {
                            string[] parts = fileConfig[i].Split(';');

                            fileClientConfigDictionary.Add(parts[0].Trim(), parts[1].Trim());
                        }
                    }
                    else
                    {
                        LoggerAccessor.LogWarn($"File not found! Path expected: {fileME3PathFull}");
                    }

                    break;
                case "ME3_MSG":
                    if (File.Exists(fileME3PathFull))
                    {
                        string[] fileConfig = File.ReadAllLines(fileME3PathFull);

                        for (int i = 0; i < fileConfig.Length; i++)
                        {
                            string[] parts = fileConfig[i].Split(';');

                            fileClientConfigDictionary.Add(parts[0].Trim(), parts[1].Trim());
                        }
                    }
                    else
                    {
                        LoggerAccessor.LogWarn($"File not found! Path expected: {fileME3PathFull}");
                    }

                    break;
                case "ME3_ENT":
                    if (File.Exists(fileME3PathFull))
                    {
                        string[] fileConfig = File.ReadAllLines(fileME3PathFull);

                        for (int i = 0; i < fileConfig.Length; i++)
                        {
                            string[] parts;
                            if (!fileConfig[i].Trim().StartsWith("ENT_ENC"))
                                parts = fileConfig[i].Split(';');
                            else
                                parts = fileConfig[i].Split(':');

                            fileClientConfigDictionary.Add(parts[0].Trim(), parts[1].Trim());
                        }
                    }
                    else
                    {
                        LoggerAccessor.LogWarn($"File not found! Path expected: {fileME3PathFull}");
                    }

                    break;
                case "ME3_DIME":
                    if (File.Exists(fileME3PathFull))
                    {
                        string fileConfig = File.ReadAllText(fileME3PathFull);


                        fileClientConfigDictionary.Add("Config", fileConfig);
                    }
                    else
                    {
                        LoggerAccessor.LogWarn($"File not found! Path expected: {fileME3PathFull}");
                    }

                    break;
            }

            return Task.FromResult(new FetchConfigResponse()
            {
                mConfig = fileClientConfigDictionary
            });
        }

        /// <summary>
        /// You only need to override the base method to handle new requests.
        /// If the request type or/and response type is NullStruct, you can change the request/response types in the Component Base.
        /// </summary>
        public override Task<PingResponse> PingAsync(NullStruct request, BlazeRpcContext context)
        {
#if DEBUG
            LoggerAccessor.LogInfo($"[Blaze] - Util: Ping Connection Id    : {context.Connection.ID}");
#endif
            return Task.FromResult(new PingResponse()
            {
                mServerTime = uint.Parse(DateTime.Now.ToString("yyyyMMdd"))
            });
        }
    }
}
