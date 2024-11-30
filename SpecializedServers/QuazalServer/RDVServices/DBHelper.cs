using Alcatraz.Context;
using Alcatraz.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace RDVServices
{
	public static class DBHelper
	{
		public static MainDbContext? GetDbContext(string serviceClass)
		{
			MainDbContext retCtx;
			string connectionString;

            switch (serviceClass)
			{
                case "PCUbisoftServices":
                case "PS3UbisoftServices":
					connectionString = $"{Program.configDir}/Quazal/Database/Uplay.sqlite";

					Directory.CreateDirectory(Path.GetDirectoryName(connectionString)!);

                    retCtx = new MainDbContext(MainDbContext.OnContextBuilding(new DbContextOptionsBuilder<MainDbContext>(), 0, $"Data Source={connectionString}").Options);

                    retCtx.Database.Migrate();

                    return retCtx;
                case "PS3TurokServices":
                    connectionString = $"{Program.configDir}/Quazal/Database/Turok2008_PS3.sqlite";

                    Directory.CreateDirectory(Path.GetDirectoryName(connectionString)!);

                    retCtx = new MainDbContext(MainDbContext.OnContextBuilding(new DbContextOptionsBuilder<MainDbContext>(), 0, $"Data Source={connectionString}").Options);

                    retCtx.Database.Migrate();

                    return retCtx;
                case "PS3GhostbustersServices":
                    connectionString = $"{Program.configDir}/Quazal/Database/Ghostbusters_PS3.sqlite";

                    Directory.CreateDirectory(Path.GetDirectoryName(connectionString)!);

                    retCtx = new MainDbContext(MainDbContext.OnContextBuilding(new DbContextOptionsBuilder<MainDbContext>(), 0, $"Data Source={connectionString}").Options);

                    retCtx.Database.Migrate();

                    return retCtx;
                case "v2Services":
                    connectionString = $"{Program.configDir}/Quazal/Database/RendezVous_v2.sqlite";

                    Directory.CreateDirectory(Path.GetDirectoryName(connectionString)!);

                    retCtx = new MainDbContext(MainDbContext.OnContextBuilding(new DbContextOptionsBuilder<MainDbContext>(), 0, $"Data Source={connectionString}").Options);

                    retCtx.Database.Migrate();

                    return retCtx;
                default:
					CustomLogger.LoggerAccessor.LogError($"[DbHelper] - Unknwon: {serviceClass} Class passed to the database!");
					break;
			}

			return null;
		}

		public static User GetUserByName(string serviceClass, string name)
		{
			using (MainDbContext? context = GetDbContext(serviceClass))
			{
				return context?.Users
					.AsNoTracking()
					.SingleOrDefault(x => x.PlayerNickName == name);
			}
		}

		public static User GetUserByPID(string serviceClass, uint PID)
		{
			using (MainDbContext? context = GetDbContext(serviceClass))
			{
				return context?.Users
					.AsNoTracking()
					.SingleOrDefault(x => x.Id == PID);
			}
		}
	}
}
