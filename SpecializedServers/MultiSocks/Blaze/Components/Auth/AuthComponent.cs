using Blaze3SDK.Blaze.Authentication;
using Blaze3SDK.Components;
using BlazeCommon;
using CustomLogger;
using NetworkLibrary.Extension;
using System.Text;
using XI5;

namespace MultiSocks.Blaze.Util
{
    internal class AuthComponent : AuthenticationComponentBase.Server
    {
        public override Task<ConsoleLoginResponse> Ps3LoginAsync(PS3LoginRequest request, BlazeRpcContext context)
        {
#if DEBUG
            LoggerAccessor.LogInfo($"[Blaze] - Auth: Connection Id    : {context.Connection.ID}");
            LoggerAccessor.LogInfo($"[Blaze] - Auth: Email     : {request.mEmail}");
            LoggerAccessor.LogInfo($"[Blaze] - Auth: XI5Ticket Size      : {request.mPS3Ticket.Length}");
#endif
            uint unixTimeStamp = (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            // Extract the desired portion of the binary data for a npticket 4.0
            byte[] extractedData = new byte[0x63 - 0x54 + 1];

            // Copy it
            Array.Copy(request.mPS3Ticket, 0x54, extractedData, 0, extractedData.Length);

            // Trim null bytes
            int nullByteIndex = Array.IndexOf(extractedData, (byte)0x00);
            if (nullByteIndex >= 0)
            {
                byte[] trimmedData = new byte[nullByteIndex];
                Array.Copy(extractedData, trimmedData, nullByteIndex);
                extractedData = trimmedData;
            }

            string accountName = Encoding.UTF8.GetString(extractedData);

            if (ByteUtils.FindBytePattern(request.mPS3Ticket, new byte[] { 0x52, 0x50, 0x43, 0x4E }, 184) != -1)
            {
                LoggerAccessor.LogInfo($"[Blaze] - Auth: User {accountName} logged in and is on RPCN");

                accountName += "@RPCN";
            }
            else
                LoggerAccessor.LogInfo($"[Blaze] - Auth: User {accountName} logged in and is on PSN");

            return Task.FromResult(new ConsoleLoginResponse()
            {
                mCanAgeUp = false,
                mIsOfLegalContactAge = true,
                mLegalDocHost = string.Empty,
                mNeedsLegalDoc = false,
                mPrivacyPolicyUri = string.Empty,
                mSessionInfo = new SessionInfo()
                {
                    mBlazeUserId = 1,
                    mEmail = request.mEmail,
                    mIsFirstLogin = true,
                    mLastLoginDateTime = unixTimeStamp,
                    mPersonaDetails = new PersonaDetails()
                    {
                        mDisplayName = accountName,
                        mExtId = 1,
                        mExtType = ExternalRefType.BLAZE_EXTERNAL_REF_TYPE_PS3,
                        mLastAuthenticated = unixTimeStamp,
                        mPersonaId = 1,
                        mStatus = PersonaStatus.ACTIVE,
                    },
                    mSessionKey = "11229301_9b171d92cc562b293e602ee8325612e7",
                    mUserId = 1,
                },
                mTosHost = string.Empty,
                mTermsOfServiceUri = string.Empty,
                mTosUri = string.Empty,
            });
        }

        public override Task<NullStruct> LogoutAsync(NullStruct request, BlazeRpcContext context)
        {
#if DEBUG
            LoggerAccessor.LogWarn($"[Blaze] - Auth: Logout Connection Id    : {context.Connection.ID}");
#endif

            return null;
        }
    }
}
