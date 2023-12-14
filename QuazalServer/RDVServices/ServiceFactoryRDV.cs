using QuazalServer.QNetZ.Factory;
using System.Reflection;

namespace QuazalServer.RDVServices
{
	public static class ServiceFactoryRDV
	{
		public static void RegisterRDVServices()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			var classList = asm.GetTypes()
								  .Where(t => string.Equals(t.Namespace, "QuazalServer.RDVServices.Services", StringComparison.Ordinal))
								  .ToArray();

			// search for new controller
			foreach (var protoClass in classList)
			{
				RMCServiceFactory.RegisterService(protoClass);
			}
		}
	}
}
