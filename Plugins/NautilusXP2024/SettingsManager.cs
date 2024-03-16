using System;
using System.IO;

namespace NautilusXP2024
{

    public static class SettingsManager
    {
        private static readonly string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");


        public static void SaveSettings(AppSettings settings)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(AppSettings));
            using (var writer = new System.IO.StreamWriter(settingsFilePath))
            {
                serializer.Serialize(writer, settings);
            }
        }

        public static AppSettings LoadSettings()
        {
            if (!System.IO.File.Exists(settingsFilePath))
            {
                return new AppSettings(); // Return default settings if file doesn't exist
            }

            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(AppSettings));
                using (var reader = new System.IO.StreamReader(settingsFilePath))
                {
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    return (AppSettings)serializer.Deserialize(reader);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8603 // Possible null reference return.
                }
            }
            catch (Exception ex)
            {
                // Log the exception if necessary (e.g., to a log file)
                System.Diagnostics.Debug.WriteLine("Error reading settings file: " + ex.Message);

                // If deserialization fails, delete the corrupt settings file
                if (System.IO.File.Exists(settingsFilePath))
                {
                    System.IO.File.Delete(settingsFilePath);
                }

                // Create a new AppSettings instance with default values
                var settings = new AppSettings();
                SaveSettings(settings); // Save the new settings file
                return settings; // Return the new instance with default values
            }
        }
    }

}
