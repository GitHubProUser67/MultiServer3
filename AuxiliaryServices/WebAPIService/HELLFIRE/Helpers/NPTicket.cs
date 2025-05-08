using CustomLogger;
using HttpMultipartParser;
using WebAPIService.SSFW;
using System.Text;
using System.IO;
using System;
using NetworkLibrary.Extension;
using NetHasher;

namespace WebAPIService.HELLFIRE.Helpers
{
    public class NPTicket
    {
        public static string RequestNPTicket(byte[] PostData, string boundary)
        {
            string userid = string.Empty;
            string sessionid = string.Empty;
            string resultString = string.Empty;
            byte[] ticketData = null;

            if (PostData != null)
            {
                using (MemoryStream copyStream = new MemoryStream(PostData))
                {
                    foreach (var file in MultipartFormDataParser.Parse(copyStream, boundary).Files)
                    {
                        using (Stream filedata = file.Data)
                        {
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
                    }

                    copyStream.Flush();
                }
            }

            if (ticketData != null)
            {
                // Extract the desired portion of the binary data
                byte[] extractedData = new byte[0x63 - 0x54 + 1];

                // Copy it
                Array.Copy(ticketData, 0x54, extractedData, 0, extractedData.Length);

                // Convert 0x00 bytes to 0x20 so we pad as space.
                for (int i = 0; i < extractedData.Length; i++)
                {
                    if (extractedData[i] == 0x00)
                        extractedData[i] = 0x20;
                }

                if (ByteUtils.FindBytePattern(ticketData, new byte[] { 0x52, 0x50, 0x43, 0x4E }, 184) != -1)
                {
                    LoggerAccessor.LogInfo($"[HFGames] - NovusPrime : User {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on RPCN");

                    // Convert the modified data to a string
                    resultString = Encoding.ASCII.GetString(extractedData) + "RPCN";

                    userid = resultString.Replace(" ", string.Empty);

                    // Calculate the MD5 hash of the result
                    string hash = DotNetHasher.ComputeMD5String(Encoding.ASCII.GetBytes(resultString + "H0mETyc00n!"));

                    // Trim the hash to a specific length
                    hash = hash.Substring(0, 10);

                    // Append the trimmed hash to the result
                    resultString += hash;

                    sessionid = GuidGenerator.SSFWGenerateGuid(hash, resultString);
                }
                else
                {
                    LoggerAccessor.LogInfo($"[HFGames] - NovusPrime : {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on PSN");

                    // Convert the modified data to a string
                    resultString = Encoding.ASCII.GetString(extractedData);

                    userid = resultString.Replace(" ", string.Empty);

                    // Calculate the MD5 hash of the result
                    string hash = DotNetHasher.ComputeMD5String(Encoding.ASCII.GetBytes(resultString + "H0mETyc00n!"));

                    // Trim the hash to a specific length
                    hash = hash.Substring(0, 14);

                    // Append the trimmed hash to the result
                    resultString += hash;

                    sessionid = GuidGenerator.SSFWGenerateGuid(hash, resultString);
                }

                return $"<response><Thing>{userid};{sessionid}</Thing></response>";
            }

            return null;
        }
    }
}
