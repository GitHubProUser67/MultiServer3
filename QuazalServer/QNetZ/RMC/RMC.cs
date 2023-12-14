using CustomLogger;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Factory;
using QuazalServer.QNetZ.Interfaces;
using System.Reflection;

namespace QuazalServer.QNetZ
{
	public static class RMC
	{
		public static void HandlePacket(QPacketHandlerPRUDP handler, QPacket p, QClient client)
		{
			client.SessionID = p.m_bySessionID;

			if (p.uiSeqId > client.SeqCounter)
				client.SeqCounter = p.uiSeqId;

			var rmc = new RMCPacket(p);
			if (rmc.isRequest)
				HandleRequest(handler, client, p, rmc);
			else
				HandleResponse(handler, client, p, rmc);
		}

		public static void HandleResponse(QPacketHandlerPRUDP handler, QClient client, QPacket p, RMCPacket rmc)
		{
			WriteLog(client, $"Received Response : {rmc}", false);
			var message = rmc.success ? "Success" : $"Fail : {rmc.error:X8} for callID = {rmc.callID}";
			WriteLog(client, $"Got response for {rmc.proto} = {message}", false);

			handler.SendACK(p, client);
		}

		public static void HandleRequest(QPacketHandlerPRUDP handler, QClient client, QPacket p, RMCPacket rmc)
		{
			if (rmc.callID > client.CallCounterRMC)
				client.CallCounterRMC = rmc.callID;

			WriteLog(client, "Request : " + rmc.ToString(), false);

			MemoryStream m = new (p.payload);
			m.Seek(rmc._afterProtocolOffset, SeekOrigin.Begin);

            RMCContext rmcContext = new(rmc, handler, client, p);

			// create service instance
			var serviceFactory = RMCServiceFactory.GetServiceFactory(rmc.proto);

			if (serviceFactory == null)
			{
				WriteLog(client, $"No service registered for packet protocol '{rmc.proto}' (protocolId = {(int)rmc.proto})", true);
				handler.SendACK(rmcContext.Packet, client);
				return;
			}

			// set the execution context
			var serviceInstance = serviceFactory();

			serviceInstance.Context = rmcContext;
			var bestMethod = serviceInstance.GetServiceMethodById(rmc.methodID);

			if (bestMethod == null)
			{
				WriteLog(client, $"No method '{ rmc.methodID }' registered for protocol '{ rmc.proto }'", true);
				handler.SendACK(rmcContext.Packet, client);
				return;
			}

			// try invoke method method
			// TODO: extended info
			var typeList = bestMethod.GetParameters().Select(x => x.ParameterType);
			var parameters = DDLSerializer.ReadPropertyValues(typeList.ToArray(), m);

			WriteLog(client, () => "Request parameters: " + DDLSerializer.ObjectToString(parameters), false);

			try
			{
				var returnValue = bestMethod.Invoke(serviceInstance, parameters);

				if (returnValue != null)
				{
					if (typeof(RMCResult).IsAssignableFrom(returnValue.GetType()))
					{
						var rmcResult = (RMCResult)returnValue;

						if (QuazalServerConfiguration.EnableRMCCompression)
                            SendResponseWithACK(
                                handler,
                                rmcContext.Packet,
                                rmcContext.RMC,
                                rmcContext.Client,
                                rmcResult.Response,
                                rmcResult.Compression, rmcResult.Error);
						else // For very old PRUDP clients.
                            SendResponseWithACK(
                                handler,
                                rmcContext.Packet,
                                rmcContext.RMC,
                                rmcContext.Client,
                                rmcResult.Response,
                                false, rmcResult.Error);
                    }
					else
					{
						// TODO: try to cast and create RMCPResponseDDL???
						LoggerAccessor.LogError("something other than RMCResult is cannot be sent yet");
						return;
					}
				}
				else
                    handler.SendACK(rmcContext.Packet, client);
            }
            catch (TargetInvocationException tie)
			{
				handler.SendACK(rmcContext.Packet, client);

				WriteLog(client, $"Exception occurred in {rmc.proto}.{bestMethod.Name}", true);

				Exception? inner = tie.InnerException;
				if (inner != null)
                {
					WriteLog(client, $"{inner.Message}", true);

					if (inner.StackTrace != null)
						WriteLog(client, $"{ inner.StackTrace }", true);
				}
			}
		}

		public static void SendResponseWithACK(QPacketHandlerPRUDP handler, QPacket p, RMCPacket rmc, QClient client, RMCPResponse reply, bool useCompression = true, uint error = 0)
		{
			WriteLog(client, "Response : " + reply.ToString(), false);
			WriteLog(client, () => "Response data : \n" + reply.PayloadToString(), false);

			handler.SendACK(p, client);

			SendResponsePacket(handler, p, rmc, client, reply, useCompression, error);
		}

		public static void SendRMCCall(QPacketHandlerPRUDP handler, QClient client, RMCProtocolId protoId, uint methodId, RMCPRequest requestData)
		{
            QPacket packet = new();

			packet.m_oSourceVPort = new QPacket.VPort(0x31);
			packet.m_oDestinationVPort = new QPacket.VPort(0x3f);

			packet.type = QPacket.PACKETTYPE.DATA;
			packet.flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_RELIABLE | QPacket.PACKETFLAG.FLAG_NEED_ACK };
			packet.payload = new byte[0];
			packet.m_bySessionID = client.SessionID;

			var rmc = new RMCPacket();

			rmc.proto = protoId;
			rmc.methodID = methodId;

			WriteLog(client, $"Sending call { protoId }.{ methodId }", false);
			WriteLog(client, () => "Call data : " + requestData.PayloadToString(), false);

			SendRequestPacket(handler, packet, rmc, client, requestData, true, 0);
		}

		private static void SendResponsePacket(QPacketHandlerPRUDP handler, QPacket p, RMCPacket rmc, QClient client, RMCPResponse reply, bool useCompression, uint error)
		{
			rmc.isRequest = false;
			rmc.response = reply;
			rmc.error = error;

			var rmcResponseData = rmc.ToBuffer();

			QPacket np = new(p.toBuffer());
			np.flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_NEED_ACK, QPacket.PACKETFLAG.FLAG_RELIABLE };
			np.m_oSourceVPort = p.m_oDestinationVPort;
			np.m_oDestinationVPort = p.m_oSourceVPort;
			np.m_uiSignature = client.IDsend;
			np.usesCompression = useCompression;

			handler.MakeAndSend(client, p, np, rmcResponseData);
		}

		public static void SendRequestPacket(QPacketHandlerPRUDP handler, QPacket p, RMCPacket rmc, QClient client, RMCPRequest request, bool useCompression, uint error)
		{
			rmc.isRequest = true;
			rmc.request = request;
			rmc.error = error;
			rmc.callID = ++client.CallCounterRMC;

			var rmcRequestData = rmc.ToBuffer();

			QPacket np = new QPacket(p.toBuffer());
			np.flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_RELIABLE | QPacket.PACKETFLAG.FLAG_NEED_ACK };
			np.m_uiSignature = client.IDsend;
			np.usesCompression = useCompression;

			handler.MakeAndSend(client, p, np, rmcRequestData);
		}

		private static void WriteLog(QClient client, Func<string> resolve, bool err)
        {
			string? unknwnClientName = client.Info != null ? client.Info.Name : "<unkClient>";
			if (err)
                LoggerAccessor.LogError($"[RMC] ({unknwnClientName}) {resolve.Invoke()}");
			else
                LoggerAccessor.LogInfo($"[RMC] ({unknwnClientName}) {resolve.Invoke()}");
        }

        private static void WriteLog(QClient client, string s, bool err)
		{
			string? unknwnClientName = client.Info != null ? client.Info.Name : "<unkClient>";
			if (err)
                LoggerAccessor.LogError($"[RMC] ({unknwnClientName}) {s}");
			else
                LoggerAccessor.LogInfo($"[RMC] ({unknwnClientName}) {s}");
        }
	}
}
