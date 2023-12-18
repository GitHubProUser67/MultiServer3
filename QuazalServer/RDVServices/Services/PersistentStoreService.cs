using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
	/// <summary>
	/// User friends service
	/// </summary>
	[RMCService(RMCProtocolId.PersistentStoreService)]
	public class PersistentStoreService : RMCServiceBase
	{
		[RMCMethod(1)]
		public void FindByGroup()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(2)]
		public void InsertItem()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(3)]
		public void RemoveItem()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(4)]
		public RMCResult GetItem(uint group, string strTag)
		{
			var reply = new StorageFile();

			var path = Path.Combine(QuazalServerConfiguration.ServerFilesPath, strTag);

			if (File.Exists(path))
			{
				reply.m_buffer = File.ReadAllBytes(path);
				reply.retcode = 1;
			}

			return Result(reply);
		}

		[RMCMethod(5)]
		public void InsertCustomItem()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(6)]
		public void GetCustomItem()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(7)]
		public void FindItemsBySQLQuery()
		{
			UNIMPLEMENTED();
		}

	}
}
