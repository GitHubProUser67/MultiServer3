using System.Reflection;

namespace MultiServer.PluginManager
{
    public class PluginLoader
    {
        public static List<IPlugin> LoadPluginsFromFolder(string folderPath)
        {
            List<IPlugin> plugins = new List<IPlugin>();

            if (Directory.Exists(folderPath))
            {
                string[] dllFiles = Directory.GetFiles(folderPath, "*.dll");

                foreach (string dllFile in dllFiles)
                {
                    IPlugin plugin = LoadPlugin(dllFile);
                    if (plugin != null)
                        plugins.Add(plugin);
                }
            }
            else
                ServerConfiguration.LogWarn($"No Plugins Folder found: {folderPath}");

            return plugins;
        }

        public static IPlugin LoadPlugin(string pluginPath)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(pluginPath);

                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(IPlugin).IsAssignableFrom(type))
                    {
                        var plugin = Activator.CreateInstance(type) as IPlugin;
                        return plugin;
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"Error loading plugin '{pluginPath}': {ex}");
            }

            return null;
        }
    }
}
