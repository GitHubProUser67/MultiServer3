using HttpMultipartParser;
using System.Text;

namespace WebUtils.THQ
{
    public class THQ
    {
        public static string? ProcessUFCUserData(Stream postdata, string? boundary, string apiPath)
        {
            string? returnstring = null;

            if (boundary != null)
            {
                // Create a memory stream to copy the content
                using (MemoryStream copyStream = new())
                {
                    // Copy the input stream to the memory stream
                    postdata.CopyTo(copyStream);

                    // Reset the position of the copy stream to the beginning
                    copyStream.Position = 0;

                    var data = MultipartFormDataParser.Parse(copyStream, boundary);

                    string val2 = string.Empty;

                    string func = data.GetParameterValue("func");

                    string id = data.GetParameterValue("id");

                    byte[]? ticketData = null;

                    foreach (var file in data.Files)
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

                    if (ticketData != null)
                    {
                        try
                        {
                            val2 = data.GetParameterValue("val2");
                        }
                        catch (Exception)
                        {
                            // Sometimes this data is not here, so we catch.
                        }

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

                        if (id == psnname)
                        {
                            Directory.CreateDirectory($"{apiPath}/HOME_THQ/{id}/");

                            string ufcdata = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                        "<UFC>1</UFC><tokens>100000</tokens><books><book value=\"1\" /><set01 value=\"1\"><card001 value=\"1\" /><fb01 value=\"Card one picked up!\" /></set01><set02 " +
                                        "value=\"1\"><card001 value=\"2\" /><fb01 value=\"Card two picked up!\" /></set02></books>";

                            switch (func)
                            {
                                case "read":
                                    if (!File.Exists($"{apiPath}/HOME_THQ/{id}/data.xml"))
                                        File.WriteAllText($"{apiPath}/HOME_THQ/{id}/data.xml", ufcdata);
                                    else if (File.Exists($"{apiPath}/HOME_THQ/{id}/data.xml"))
                                        ufcdata = File.ReadAllText($"{apiPath}/HOME_THQ/{id}/data.xml");
                                    break;
                                case "write":
                                    if (File.Exists($"{apiPath}/HOME_THQ/{id}/data.xml") && func == "write" && val2 != null)
                                    {
                                        ufcdata = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                                $"<UFC>1</UFC><tokens>{val2}</tokens><books><book value=\"1\" /><set01 value=\"1\"><card001 value=\"1\" /><fb01 value=\"Card one picked up!\" /></set01><set02 " +
                                                $"value=\"1\"><card001 value=\"2\" /><fb01 value=\"Card two picked up!\" /></set02></books>";

                                        File.WriteAllText($"{apiPath}/HOME_THQ/{id}/data.xml", ufcdata);
                                    }
                                    break;
                                default:
                                    break;
                            }

                            returnstring = ufcdata;
                        }
                    }

                    copyStream.Flush();
                }
            }

            return returnstring;
        }
    }
}
