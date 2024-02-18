using System.Reflection;

namespace HTTPServer.PluginManager
{
    public class PluginLoader
    {
        public static List<HTTPPlugin> LoadPluginsFromFolder(string folderPath)
        {
            List<HTTPPlugin> plugins = new();

            if (Directory.Exists(folderPath))
            {
                string[] dllFiles = Directory.GetFiles(folderPath, "*.dll");

                foreach (string dllFile in dllFiles)
                {
                    HTTPPlugin? plugin = LoadPlugin(dllFile);
                    if (plugin != null)
                        plugins.Add(plugin);
                }
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"No Plugins Folder found: {folderPath}");

            return plugins;
        }

        public static HTTPPlugin? LoadPlugin(string pluginPath)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(pluginPath);

                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(HTTPPlugin).IsAssignableFrom(type))
                    {
                        var plugin = Activator.CreateInstance(type) as HTTPPlugin;
                        return plugin;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"Error loading plugin '{pluginPath}': {ex}");
            }

            return null;
        }
    }
}
