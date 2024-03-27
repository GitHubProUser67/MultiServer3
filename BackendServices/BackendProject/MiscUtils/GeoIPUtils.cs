using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using System.Net;

namespace BackendProject.MiscUtils
{
    public class GeoIPUtils : IDisposable
    {
        public readonly DatabaseReader? Reader;

        private static GeoIPUtils? _instance;

        private static readonly string currentdir = Directory.GetCurrentDirectory();

        public GeoIPUtils(DatabaseReader? reader)
        {
            Reader = reader;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (Reader != null)
                        Reader.Dispose();

                    _instance = null;

                    if (_instance != null)
                    {
                        _instance.Dispose();
                        _instance = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        ~GeoIPUtils()
        {
            Dispose(false);
        }

        public static void Initialize()
        {
            DatabaseReader? reader;

            if (File.Exists($"{currentdir}/static/GeoIP2-Country.mmdb"))
            {
                reader = new DatabaseReader($"{currentdir}/static/GeoIP2-Country.mmdb");
                CustomLogger.LoggerAccessor.LogInfo("[GeoIPUtils] - Loaded GeoIP2-Country.mmdb Database...");
            }
            else if (File.Exists($"{currentdir}/static/GeoLite2-Country.mmdb"))
            {
                reader = new DatabaseReader($"{currentdir}/static/GeoLite2-Country.mmdb");
                CustomLogger.LoggerAccessor.LogInfo("[GeoIPUtils] - Loaded GeoLite2-Country.mmdb Database...");
            }
            else
                reader = null;

            _instance = new GeoIPUtils(reader);
        }

        public static string? GetGeoCodeFromIP(IPAddress IPAddr)
        {
            // Format as follows -> Country-IsInEuropeanUnion.
            if (Instance != null && Instance.Reader != null)
            {
                try
                {
                    if (Instance.Reader.TryCountry(IPAddr, out CountryResponse? countryresponse) && countryresponse != null && !string.IsNullOrEmpty(countryresponse.Country.Name))
                        return countryresponse.Country.Name + $"-{countryresponse.Country.IsInEuropeanUnion}";
                }
                catch (Exception)
                {
                    // Not Important.
                }
            }

            return null;
        }

        public static string? GetISOCodeFromIP(IPAddress IPAddr)
        {
            // Format as follows -> Country-IsInEuropeanUnion.
            if (Instance != null && Instance.Reader != null)
            {
                try
                {
                    if (Instance.Reader.TryCountry(IPAddr, out CountryResponse? countryresponse) && countryresponse != null && !string.IsNullOrEmpty(countryresponse.Country.Name))
                        return countryresponse.Country.IsoCode;
                }
                catch (Exception)
                {
                    // Not Important.
                }
            }

            return null;
        }

        public static GeoIPUtils Instance
        {
            get
            {
                if (_instance == null)
                    throw new ArgumentNullException("Instance", "Initialize() must be called first");

                return _instance;
            }
        }
    }
}
