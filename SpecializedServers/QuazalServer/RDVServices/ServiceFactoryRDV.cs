using QuazalServer.QNetZ.Factory;
using System.Reflection;

namespace QuazalServer.RDVServices
{
	public static class ServiceFactoryRDV
	{
		public static void RegisterRDVServices()
		{
			foreach (Type? protoClass in Assembly.GetExecutingAssembly().GetTypes()
										  .Where(t => string.Equals(t.Namespace, "QuazalServer.RDVServices.Services", StringComparison.Ordinal))
										  .ToArray())
			{
				RMCServiceFactory.RegisterService(protoClass);
			}
        }
	}
}
