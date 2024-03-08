using CustomLogger;
using QuazalServer.QNetZ.Factory;
using System.Reflection;
using System.Text;

namespace QuazalServer.QNetZ
{
	public class RMCPacket
	{
		public RMCProtocolId proto;
		public bool isRequest;
		public bool success;
		public uint error;
		public uint callID;
		public uint methodID;
		public RMCPRequest? request;
		public RMCPResponse? response;
		public uint _afterProtocolOffset;
		public uint requestSize;

		public RMCPacket()
		{

		}

		public RMCPacket(string AccessKey, QPacket p)
		{
			if (p.payload != null)
			{
                MemoryStream m = new(p.payload);

                // the request info size + data size
                requestSize = Helper.ReadU32(m);

                ushort b = Helper.ReadU8(m);
                isRequest = (b >> 7) == 1;

                try
                {
                    if ((b & 0x7F) != 0x7F)
                        proto = (RMCProtocolId)(b & 0x7F);
                    else
                    {
                        b = Helper.ReadU16(m);
                        proto = (RMCProtocolId)(b);
                    }

					switch (AccessKey) // Special servers.
					{
                        case "os4R9pEiy":
                        case "OLjNg84Gh":
                        case "cYoqGd4f":
                        case "QusaPha9":
						case "q1UFc45UwoyI":
                        case "h0rszqTw":
                        case "lON6yKGp":
                            if (proto == RMCProtocolId.SecureConnectionService)
                                proto = RMCProtocolId.PS3SecureConnectionService;
                            break;
                        case "yh64s":
                        case "hg7j1":
                            if (proto == RMCProtocolId.FriendsService)
                                proto = RMCProtocolId.LegacyFriendsService;
                            break;
					}
                }
                catch
                {
                    LoggerAccessor.LogError("[RMC Packet] - Unknown RMC packet protocol 0x" + b.ToString("X2"));
                    return;
                }

                if (isRequest)
                {
                    callID = Helper.ReadU32(m);
                    methodID = Helper.ReadU32(m);
                }
                else
                {
                    // response
                    success = m.ReadByte() == 1;

                    if (success)
                    {
                        callID = Helper.ReadU32(m);
                        methodID = Helper.ReadU32(m) & (~0x8000u);
                    }
                    else
                    {
                        error = Helper.ReadU32(m);
                        callID = Helper.ReadU32(m);
                    }
                }

                _afterProtocolOffset = (uint)m.Position;
            }
		}

		public override string ToString()
		{
			string methodName = methodID.ToString();

			var serviceFactory = RMCServiceFactory.GetServiceFactory(proto);
			MethodInfo? bestMethod = null;
			if (serviceFactory != null)
			{
				var service = serviceFactory();

				bestMethod = service.GetServiceMethodById(methodID);
				if (bestMethod != null)
					methodName = bestMethod.Name;
			}

			return $"[CallID={ callID } { proto }.{ methodName }]";
		}

		public string PayLoadToString()
		{
			StringBuilder sb = new();

			if (request != null)
                sb.Append(request);

            return sb.ToString();
		}

		public byte[] ToBuffer()
		{
            MemoryStream packetData = new();

			if (proto == RMCProtocolId.PS3SecureConnectionService) // We need this workaround, Ubisoft client code expect the correct ID for the service.
																   // Else, it denies the request... great...
				proto = RMCProtocolId.SecureConnectionService;

            if ((ushort)proto < 0x7F)
			{
				// request has 0x80 flag
				uint protoIdent = (uint)proto | (isRequest ? 0x80u : 0x0u);
				Helper.WriteU8(packetData, (byte)protoIdent);
			}
			else
			{
				uint protoIdent = 0x7Fu | (isRequest ? 0x80u : 0x0u);
				Helper.WriteU8(packetData, (byte)protoIdent);
				Helper.WriteU16(packetData, (ushort)proto);
			}

			byte[] buff = Array.Empty<byte>();

			if (isRequest && request != null)
			{
				Helper.WriteU32(packetData, callID);
				Helper.WriteU32(packetData, methodID);

				// write request payload
				buff = request.ToBuffer();
				packetData.Write(buff, 0, buff.Length);
			}
			else
			{
				// response
				success = error == 0;
				packetData.WriteByte((byte)(success ? 1 : 0));

				if (success && response != null)
				{
					Helper.WriteU32(packetData, callID);
					Helper.WriteU32(packetData, methodID | 0x8000);

					// write response payload
					buff = response.ToBuffer();
					packetData.Write(buff, 0, buff.Length);
				}
				else
				{
					Helper.WriteU32(packetData, error | 0x80000000);
					Helper.WriteU32(packetData, callID);
				}
			}

			buff = packetData.ToArray();
			packetData = new MemoryStream();
			
			Helper.WriteU32(packetData, (uint)buff.Length);
			packetData.Write(buff, 0, buff.Length);

			return packetData.ToArray();
		}
	}
}
