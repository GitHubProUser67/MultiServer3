using DotNetty.Common.Internal.Logging;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;

namespace PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.Medius.Models
{
    public class Anticheat
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Anticheat>();

        public class CheatAction
        {
            public int mActions;
            public int mWorldIndex;
            public int mCLientIndex;
            public string mSessionKey;
            public int mQueryType;
            public int mSequenceId;
            public int mStartingAddress;
            public int mNBytes;
            public int mQueryData;
            public int mNQueryDataBytes;
            public int mPokeStartAddress;
            public int mNPokeBytes;
            public string mPokeData;
            public DME_SERVER_ENCODING_TYPE mEncodingType;
            public DME_SERVER_LANGUAGE_TYPE mLanguageType;
            public string mMsgBody;
            public int mMsgLength;
            public int mLogDetectedData;
        }

        public void handleLogDetectedAction()
        {
            Logger.Info($"handleLogDetectedAction: AID[%d] ");
        }


    }
}
