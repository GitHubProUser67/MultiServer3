using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome
{
    public class NpTicketData
    {
        public static byte[] ExtractTicketData(Stream inputStream, string boundary)
        {
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("--" + boundary);
            byte[] endBoundaryBytes = Encoding.ASCII.GetBytes("--" + boundary + "--");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryReader reader = new BinaryReader(inputStream))
                {
                    using (BinaryWriter writer = new BinaryWriter(memoryStream))
                    {
                        bool isTicketData = false;
                        bool isReadingData = false;

                        byte[] lineBytes;
                        while ((lineBytes = Extensions.ReadLine(reader)) != null)
                        {
                            if (Extensions.ByteArrayStartsWith(lineBytes, boundaryBytes))
                            {
                                if (isReadingData)
                                    break;

                                isTicketData = false;
                                isReadingData = false;
                            }
                            else if (Extensions.ByteArrayStartsWith(lineBytes, endBoundaryBytes))
                            {
                                break;
                            }
                            else if (isTicketData && isReadingData)
                            {
                                writer.Write(lineBytes);
                            }
                            else if (Extensions.ByteArrayStartsWith(lineBytes, Encoding.ASCII.GetBytes("Content-Disposition: form-data; name=\"ticket\"; filename=\"ticket.bin\"")))
                            {
                                isTicketData = true;
                            }
                            else if (Extensions.ByteArrayStartsWith(lineBytes, Encoding.ASCII.GetBytes("Content-Type: application/octet-stream")))
                            {
                                isReadingData = true;
                            }
                        }

                        return memoryStream.ToArray();
                    }
                }
            }
        }
    }
}
