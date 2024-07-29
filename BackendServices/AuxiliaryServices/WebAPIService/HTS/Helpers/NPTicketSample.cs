
using CustomLogger;
using HttpMultipartParser;
using System.Security.Cryptography;
using WebAPIService.SSFW;
using System.Text;
using CyberBackendLibrary.DataTypes;
using System.IO;
using System;
using System.Collections;

namespace WebAPIService.HTS.Helpers
{
    public class MyResistanceEula
    {
        public static string? RequestNPTicket(byte[]? PostData, string boundary)
        {
            string userid = string.Empty;
            string sessionid = string.Empty;
            string resultString = string.Empty;

            string region = string.Empty;



            byte[]? ticketData = null;

            if (PostData != null)
            {
                using MemoryStream copyStream = new MemoryStream(PostData);
                foreach (var file in MultipartFormDataParser.Parse(copyStream, boundary).Files)
                {
                    using Stream filedata = file.Data;
                    filedata.Position = 0;

                    // Find the number of bytes in the stream
                    int contentLength = (int)filedata.Length;

                    // Create a byte array
                    byte[] buffer = new byte[contentLength];

                    // Read the contents of the memory stream into the byte array
                    filedata.Read(buffer, 0, contentLength);

                    if (file.FileName == "ticket.bin")
                        ticketData = buffer;

                    filedata.Flush();
                }

                copyStream.Flush();
            }

            if (ticketData != null)
            {



                #region Region
                // Extract part of the byte array from the specific index
                byte[] ticketRegion = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                Array.Copy(ticketData, 0x78, ticketRegion, 0, 4);
                #endregion

                #region Domain
                // Extract part of the byte array from the specific index
                byte[] ticketDomain = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                Array.Copy(ticketData, 0x80, ticketDomain, 0, 4);
                #endregion

                #region ServiceId
                byte serviceIdLen = (byte)ticketData.GetValue(0x87);
                // Extract part of the byte array from the specific index
                byte[] servId = new byte[serviceIdLen];
                Array.Copy(ticketData, 0x88, servId, 0, serviceIdLen);
                #endregion

                // Extract the desired portion of the binary data
                byte[] userOnlineId = new byte[0x63 - 0x54 + 1];

                // Copy it
                Array.Copy(ticketData, 0x54, userOnlineId, 0, userOnlineId.Length);

                // Convert 0x00 bytes to 0x20 so we pad as space.
                for (int i = 0; i < userOnlineId.Length; i++)
                {
                    if (userOnlineId[i] == 0x00)
                        userOnlineId[i] = 0x20;
                }

                if (DataTypesUtils.FindBytePattern(ticketData, new byte[] { 0x52, 0x50, 0x43, 0x4E }) != -1)
                {
                    LoggerAccessor.LogInfo($"[HTS] : User {Encoding.ASCII.GetString(userOnlineId).Replace("H", string.Empty)} logged in and is on RPCN");

                    // Convert the modified data to a string
                    resultString = Encoding.ASCII.GetString(userOnlineId) + "RPCN";

                    userid = resultString.Replace(" ", string.Empty);

                    // Calculate the MD5 hash of the result
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(resultString + "H0mETyc00n!"));
                        string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                        // Trim the hash to a specific length
                        hash = hash[..10];

                        // Append the trimmed hash to the result
                        resultString += hash;

                        sessionid = GuidGenerator.SSFWGenerateGuid(hash, resultString);

                        md5.Clear();
                    }
                }
                else
                {
                    LoggerAccessor.LogInfo($"[HTS] : {Encoding.ASCII.GetString(userOnlineId).Replace("H", string.Empty)} logged in and is on PSN");

                    // Convert the modified data to a string
                    resultString = Encoding.ASCII.GetString(userOnlineId);

                    userid = resultString.Replace(" ", string.Empty);

                    // Calculate the MD5 hash of the result
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(resultString + "HtTeStSamPLe@$!"));
                        string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                        // Trim the hash to a specific length
                        hash = hash[..14];

                        // Append the trimmed hash to the result
                        resultString += hash;

                        sessionid = GuidGenerator.SSFWGenerateGuid(hash, resultString);

                        md5.Clear();
                    }
                }

                return $@"<xml>
                        <npID>{Encoding.UTF8.GetString(userOnlineId)}</npID>
                        <Environment></Environment>
                        <issuerID></issuerID>
                        <Issued></Issued>
                        <Expires></Expires>
                        <ServiceID>{Encoding.UTF8.GetString(servId)}</ServiceID>
                        <Region>{Encoding.UTF8.GetString(ticketRegion)}</Region>
                        <Language></Language>
                        <entitlements></entitlements>
                    </xml>";
            }

            return null;
        }
    }
}
