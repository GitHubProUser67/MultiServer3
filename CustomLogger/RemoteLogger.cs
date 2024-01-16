using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace CustomLogger
{
    public static class RemoteLogger
    {
        private static List<string> LogsList = new(100);

        private static string UrlLink = CalculateLinkHMAC512Hash();

        public static bool IsStarted = false;

        public static Task StartRemoteServer(int port)
        {
            if (!LoggerAccessor.initiated)
                return Task.CompletedTask;

            // Http listener on windows is only compatible with administrator account.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
#pragma warning disable
                if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                    return Task.CompletedTask;
#pragma warning restore
            }

            if (HttpListener.IsSupported)
            {
                try
                {
                    // Create an HttpListener instance
                    HttpListener listener = new();
                    listener.Prefixes.Add($"http://*:{port}/");

                    // Start the listener
                    listener.Start();
                    LoggerAccessor.LogInfo($"[RemoteLogger] - Remote Console logs started with UrlLink: /{UrlLink} on port: {port}...");

                    IsStarted = true;

                    while (IsStarted)
                    {
                        try
                        {
                            // Wait for an incoming request
                            HttpListenerContext context = listener.GetContextAsync().Result;

                            // Handle the request in a separate thread
                            Task.Run(() => HandleRequestAsync(context));
                        }
                        catch (HttpListenerException e)
                        {
                            if (e.ErrorCode != 995) LoggerAccessor.LogError("[RemoteLogger] - An Exception Occured: " + e.Message);
                        }
                        catch (Exception e)
                        {
                            LoggerAccessor.LogError("[RemoteLogger] - An Exception Occured: " + e.Message);
                        }
                    }

                    listener.Stop();

                    LoggerAccessor.LogWarn($"[RemoteLogger] - Remote Console logs was stopped!");
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RemoteLogger] - Remote Console logs Failed to start with assertion: {ex}");
                }
            }
            else
                LoggerAccessor.LogWarn("Windows XP SP2 or Server 2003 is required to use the HttpListener class, so Remote Console logs not started.");

            return Task.CompletedTask;
        }

        private static Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                if (context.Request.Url?.AbsolutePath == $"/{UrlLink}")
                {
                    // Set the content type and status code
                    context.Response.ContentType = "text/plain";
                    context.Response.StatusCode = (int)HttpStatusCode.OK;

                    Span<byte> buffer = Encoding.UTF8.GetBytes(FormatListAsString());

                    if (context.Response.OutputStream.CanWrite)
                    {
                        try
                        {
                            context.Response.ContentLength64 = buffer.Length;
                            context.Response.OutputStream.Write(buffer);
                        }
                        catch (Exception)
                        {
                            // Not Important.
                        }
                    }
                }
                else
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            catch (HttpListenerException e) when (e.ErrorCode == 64)
            {
                // Unfortunately, some client side implementation of HTTP (like RPCS3) freeze the interface at regular interval.
                // This will cause server to throw error 64 (network interface not openned anymore)
                // In that case, we send internalservererror so client try again.

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("[RemoteLogger] - REQUEST ERROR: " + e.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            try
            {
                context.Response.OutputStream.Close();
            }
            catch (ObjectDisposedException)
            {
                // outputstream has been disposed already.
            }
            context.Response.Close();

            return Task.CompletedTask;
        }

        private static string GenerateWeeklyString()
        {
            // Get the current date
            DateTime currentDate = DateTime.Now;

            // Generate a unique string using week number and year
            return $"{currentDate.Year}-Week{GetIso8601WeekNumber(currentDate)}";
        }

        private static string CalculateLinkHMAC512Hash()
        {
            using (HMACSHA512 hmacsha512 = new(Encoding.UTF8.GetBytes("My1p0!sssssW0rD")))
            {
                byte[] hashBytes = hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(GenerateWeeklyString() + "S1AlT1d01Nput00000000000000000000000!"));

                StringBuilder sb = new();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                hmacsha512.Clear();

                return sb.ToString();
            }
        }

        private static int GetIso8601WeekNumber(DateTime date)
        {
            // Calculate the ISO 8601 week number
            // Algorithm source: https://stackoverflow.com/a/19947850/3902416

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            int weekOfYear = cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

            // Adjust for leap years
            if (dfi.CalendarWeekRule == CalendarWeekRule.FirstFourDayWeek)
            {
                int dayOfYear = cal.GetDayOfYear(date) - 1;
                int daysInYear = cal.GetDaysInYear(date.Year) - 1;
                int leapYearAdjustment = Array.IndexOf(new byte[] { 1, 2, 3, 5 }, (int)cal.GetDayOfWeek(date));

                if (dayOfYear <= daysInYear - leapYearAdjustment)
                    weekOfYear = (dayOfYear + leapYearAdjustment) / 7 + 1;
                else
                {
                    weekOfYear = (dayOfYear - daysInYear + leapYearAdjustment) / 7 + 1;
                    if (weekOfYear < 1)
                        weekOfYear = 52;
                }
            }

            return weekOfYear;
        }

        private static string FormatListAsString()
        {
            if (LogsList.Count > 0)
            {
                // Using StringBuilder to efficiently build the formatted string
                StringBuilder formattedString = new();

                foreach (string str in LogsList)
                {
                    formattedString.AppendLine(str);
                }

                return formattedString.ToString();
            }

            return "No Logs are Available.";
        }

        public static void IncrementAndRotate(string command)
        {
            // Add the new string to the end of the list
            LogsList.Add(command);
        }
    }
}
