using Blaze3SDK;
using Blaze3SDK.Blaze.Authentication;
using Blaze3SDK.Blaze.Util;
using Blaze3SDK.Components;
using BlazeCommon;
using CustomLogger;
using MultiSocks.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSocks.Blaze.Util
{
    internal class AuthComponent : AuthenticationComponentBase.Server
    {
        /// <summary>
        /// You only need to override the base method to handle new requests.
        /// If the request type or/and response type is NullStruct, you can change the request/response types in the Component Base.
        /// </summary>
        public override Task<ConsoleLoginResponse> Ps3LoginAsync(PS3LoginRequest request, BlazeRpcContext context)
        {
            //if needded access underlying connection which issued the request and other stuff
            ProtoFireConnection connection = context.Connection;

            //manually displaying some data
            LoggerAccessor.LogInfo($"[Blaze] - Auth: Connection Id    : {connection.ID}");
            LoggerAccessor.LogInfo($"[Blaze] - Auth: Email     : {request.mEmail}");
            LoggerAccessor.LogInfo($"[Blaze] - Auth: Client Type      : {request.mPS3Ticket}");

            /*
            //if something is wrong with request data - thrown an BlazeRpcException with error code and error data, which will be sent to client (in debug environment disable breaking on this exception)
            if (request.mName == "someValueHere")
            {
                throw new BlazeRpcException(Blaze3RpcError.REDIRECTOR_UNKNOWN_SERVICE_NAME, new ServerInstanceError()
                {
                    mMessages = new List<string>() {
                        "Unknown service name"
                    }
                });
            }
            */


            string ip = MultiSocksServerConfiguration.UsePublicIPAddress ? NetworkLibrary.TCP_IP.IPUtils.GetPublicIPAddress() : NetworkLibrary.TCP_IP.IPUtils.GetLocalIPAddress().ToString();
            uint unixTimeStamp = (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;


            
            ConsoleLoginResponse consoleLoginResponse = new ConsoleLoginResponse()
            {
                mCanAgeUp = false,
                mIsOfLegalContactAge = true,
                mLegalDocHost = "",
                mNeedsLegalDoc = false,
                mPrivacyPolicyUri = "",
                mSessionInfo = new SessionInfo()
                {
                    mBlazeUserId = 1,
                    mEmail = request.mEmail,
                    mIsFirstLogin = true,
                    mLastLoginDateTime = unixTimeStamp,
                    mPersonaDetails = new PersonaDetails()
                    {
                        mDisplayName = "Dust514JumpDev",
                        mExtId = 1,
                        mExtType = ExternalRefType.BLAZE_EXTERNAL_REF_TYPE_PS3,
                        mLastAuthenticated = unixTimeStamp,
                        mPersonaId = 1,
                        mStatus = PersonaStatus.ACTIVE,
                    },
                    mSessionKey = "11229301_9b171d92cc562b293e602ee8325612e7",
                    mUserId = 1,
                },
                mTosHost = "",
                mTermsOfServiceUri = "",
                mTosUri = "",

            };
            

            //return the response
            return Task.FromResult(consoleLoginResponse);
        }




        /// <summary>
        /// You only need to override the base method to handle new requests.
        /// If the request type or/and response type is NullStruct, you can change the request/response types in the Component Base.
        /// </summary>
        public override Task<NullStruct> LogoutAsync(NullStruct request, BlazeRpcContext context)
        {
            //if needded access underlying connection which issued the request and other stuff
            ProtoFireConnection connection = context.Connection;

            //manually displaying some data
            LoggerAccessor.LogInfo($"[Blaze] - Auth: Logout Connection Id    : {connection.ID}");


            //return the response
            return null;
        }
    }
}
