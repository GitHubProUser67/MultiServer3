using Alcatraz.Context;
using Alcatraz.Context.Entities;
using Alcatraz.DTO.Helpers;
using CustomLogger;
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
                case "PS3RaymanLegendsServices":
                case "PCDriverServices":
                case "PS3DriverServices":
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
					LoggerAccessor.LogError($"[DbHelper] - Unknwon: {serviceClass} Class passed to the database!");
					break;
			}

			return null;
		}

        public static bool RegisterUser(string serviceClass, string userName, string password, uint PID, string? NickName = null)
        {
            using (MainDbContext? context = GetDbContext(serviceClass))
            {
                if (context != null)
                {
                    if (!context.Users.Any())
                    {
                        // Add dummy user with starting ID == 1000
                        context.Users.Add(new User()
                        {
                            Id = 1000,
                            Username = "dummy",
                            PlayerNickName = "dummy",
                            Password = "dummy",
                            RewardFlags = 0,
                        });
                        context.SaveChanges();
                    }

                    bool HasNickName = !string.IsNullOrEmpty(NickName);

                    if (!context.Users.Any(x => x.Username == userName || x.Id == PID || (HasNickName && x.PlayerNickName == NickName)))
                    {
                        User dbUser = new User() { Id = PID, Username = userName };

                        if (HasNickName)
                            dbUser.PlayerNickName = NickName;

                        dbUser.Password = SecurePasswordHasher.Hash($"{dbUser.Id}-{password}");

                        try
                        {
                            context.Users.Add(dbUser);
                            context.SaveChanges();

                            return true;
                        }
                        catch (Exception ex)
                        {
                            LoggerAccessor.LogError($"[DBHelper] - An assertion was thrown while adding User:{dbUser} to the database. (Exception: {ex})");
                        }
                    }
                }
            }

            return false;
        }

        public static User? GetUserByUserName(string serviceClass, string userName)
        {
            using (MainDbContext? context = GetDbContext(serviceClass))
            {
                return context?.Users
                    .AsNoTracking()
                    .SingleOrDefault(x => x.Username == userName);
            }
        }

        public static User? GetUserByNickName(string serviceClass, string name)
		{
			using (MainDbContext? context = GetDbContext(serviceClass))
			{
				return context?.Users
					.AsNoTracking()
					.SingleOrDefault(x => x.PlayerNickName == name);
			}
		}

		public static User? GetUserByPID(string serviceClass, uint PID)
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
