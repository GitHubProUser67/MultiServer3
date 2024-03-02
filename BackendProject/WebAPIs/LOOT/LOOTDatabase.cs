using BackendProject.MiscUtils;
using HttpMultipartParser;
using System.Text;

namespace BackendProject.WebAPIs.LOOT
{
    public class LOOTDatabase
    {
        public static string? ProcessDatabaseRequest(byte[] PostData, string ContentType)
        {
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null)
            {
                using MemoryStream ms = new(PostData);
                byte[]? ticketData = null;
                var data = MultipartFormDataParser.Parse(ms, boundary);
                string action = data.GetParameterValue("action");

                foreach (var file in data.Files)
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

                    // Convert the modified data to a string
                    string psnname = Encoding.ASCII.GetString(extractedData).Replace(" ", string.Empty);

                    switch (action)
                    {
                        case "200.000000": // AddAchievementEarned
                            break;
                        case "201.000000": // GetAchievementDetails
                            break;
                        case "206.000000": // GetUserAchievementList
                            break;
                        case "207.000000": // GetUserAchievementCount
                            break;
                        case "604.000000": // UNK
                            break;
                        default:
                            break;

                    }
                }
                ms.Flush();
            }

            return null;
        }
    }
}
