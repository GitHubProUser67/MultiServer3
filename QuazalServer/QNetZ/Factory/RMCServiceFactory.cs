using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace QuazalServer.QNetZ.Factory
{
	public static class RMCServiceFactory
	{
		static Dictionary<RMCProtocolId, Func<RMCServiceBase>> s_FactoryFuncs = new();

		public static void RegisterService<T>() where T: RMCServiceBase
		{
			RegisterService(typeof(T));
		}

		public static void RegisterService(Type serviceType)
		{
            RMCServiceAttribute? serviceAttribute = serviceType.GetCustomAttribute<RMCServiceAttribute>();

			if (serviceAttribute == null)
				return; //throw new Exception($"Service type '{ serviceType.Name }' missing 'RMCService' attribute!");

			if (s_FactoryFuncs.ContainsKey(serviceAttribute.ProtocolId))
			{
                CustomLogger.LoggerAccessor.LogError($"[RMCServiceFactory] - Service '{serviceType.Name}' is already registered at protocol number {serviceAttribute.ProtocolId}!");
				return;
            }

            var createFunc = Expression.Lambda<Func<RMCServiceBase>>(
				Expression.New(serviceType.GetConstructor(Type.EmptyTypes))
			).Compile();

			s_FactoryFuncs.Add(serviceAttribute.ProtocolId, createFunc);
		}

		public static Func<RMCServiceBase>? GetServiceFactory(RMCProtocolId protocolId)
		{
			Func<RMCServiceBase>? existingFactory;
			if (s_FactoryFuncs.TryGetValue(protocolId, out existingFactory))
                return existingFactory;

            Assembly asm = Assembly.GetExecutingAssembly();
			var classList = asm.GetTypes()
								  .Where(t => string.Equals(t.Namespace, "QuazalServer.QNetZ.Services", StringComparison.Ordinal))
								  .ToArray();

			// search for new controller
			foreach (var protoClass in classList)
			{
				var protocolAttribute = protoClass.GetCustomAttribute<RMCServiceAttribute>();

				if (protocolAttribute == null)
					continue;

				if (protocolAttribute.ProtocolId == protocolId)
				{
					var createFunc = Expression.Lambda<Func<RMCServiceBase>>(
							Expression.New(protoClass.GetConstructor(Type.EmptyTypes))
						).Compile();

					s_FactoryFuncs.Add(protocolId, createFunc);

					return createFunc;
				}
			}

			return null;
		}

		public static Type? GetServiceType(RMCProtocolId protocolId)
		{
			var factoryFunc = GetServiceFactory(protocolId);
			if (factoryFunc == null)
				return null;

			return factoryFunc.Method.ReturnType;
		}

		public static MethodInfo? GetServiceMethodById(Type rmcServiceType, uint methodId)
		{
			MethodInfo? bestMethod = null;

			// find suitable method which DO have attribute with ID
			var allMethods = rmcServiceType.GetMethods();
			foreach (var method in allMethods)
			{
                RMCMethodAttribute? rmcMethodAttr = (RMCMethodAttribute)method.GetCustomAttributes(typeof(RMCMethodAttribute), true).SingleOrDefault();
				if (rmcMethodAttr != null)
				{
					if (rmcMethodAttr.MethodId == methodId)
					{
						bestMethod = method;
						break;
					}
				}
			}

			return bestMethod;
		}
	}
}
