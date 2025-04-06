using Alcatraz.Context;
using Alcatraz.Context.Entities;
using Alcatraz.DTO.Helpers;
using CustomLogger;
using Microsoft.EntityFrameworkCore;
using QuazalServer.QNetZ;

namespace RDVServices
{
	public static class DBHelper
	{
		public static MainDbContext? GetDbContext(string serviceClass)
		{
            bool initiateSharedUbiPlayers = false;
            string connectionString;
            MainDbContext? retCtx = null;

            switch (serviceClass)
			{
                case "PCDriverServices":
                case "PCUbisoftServices":
					connectionString = $"{Program.configDir}/Quazal/Database/Uplay.sqlite";

					Directory.CreateDirectory(Path.GetDirectoryName(connectionString)!);

                    retCtx = new MainDbContext(MainDbContext.OnContextBuilding(new DbContextOptionsBuilder<MainDbContext>(), 0, $"Data Source={connectionString}").Options);

                    retCtx.Database.Migrate();

                    initiateSharedUbiPlayers = true;
                    break;
                case "v2Services":
                    connectionString = $"{Program.configDir}/Quazal/Database/RendezVous_v2.sqlite";

                    Directory.CreateDirectory(Path.GetDirectoryName(connectionString)!);

                    retCtx = new MainDbContext(MainDbContext.OnContextBuilding(new DbContextOptionsBuilder<MainDbContext>(), 0, $"Data Source={connectionString}").Options);

                    retCtx.Database.Migrate();

                    break;
                default:
					LoggerAccessor.LogError($"[DbHelper] - Unknwon: {serviceClass} Class passed to the database!");
					break;
			}

            if (retCtx != null)
                InitiateDefaultPlayers(retCtx, initiateSharedUbiPlayers);

            return retCtx;
		}

        private static void InitiateDefaultPlayers(MainDbContext context, bool initiateSharedUbiPlayers)
        {
            if (!context.Users.Any())
            {
                // Add dummy user with starting ID == 1000 and two guest accounts.
                context.Users.Add(new User()
                {
                    Id = 1000,
                    Username = "dummy",
                    PlayerNickName = "dummy",
                    Password = "dummy",
                    RewardFlags = 0,
                });
                context.SaveChanges();

                if (initiateSharedUbiPlayers)
                {
                    User newUser = new User()
                    {
                        Username = "AAAABBBB",
                        PlayerNickName = "AAAABBBB",
                        Password = "tmp",
                    };

                    try
                    {
                        context.Users.Add(newUser);
                        context.SaveChanges();
                    }
                    catch
                    {
                        LoggerAccessor.LogError($"[DBHelper] - Unable to add default ubi {newUser.Username} user (internal error)");
                        return;
                    }

                    // update password as user Id is acquired
                    newUser.Password = SecurePasswordHasher.Hash($"{newUser.Id}-CCCCDDDD");
                    context.SaveChanges();

                    newUser = new User()
                    {
                        Username = "sam_the_fisher",
                        PlayerNickName = "sam_the_fisher",
                        Password = "tmp",
                    };

                    try
                    {
                        context.Users.Add(newUser);
                        context.SaveChanges();
                    }
                    catch
                    {
                        LoggerAccessor.LogError($"[DBHelper] - Unable to add default ubi {newUser.Username} user (internal error)");
                        return;
                    }

                    // update password as user Id is acquired
                    newUser.Password = SecurePasswordHasher.Hash($"{newUser.Id}-password1234");
                    context.SaveChanges();
                }
            }
        }

        public static bool RegisterUser(string serviceClass, string userName, string password, uint PID, string? NickName = null)
        {
            using (MainDbContext? context = GetDbContext(serviceClass))
            {
                if (context != null)
                {
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
