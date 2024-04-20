using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HTTPSecureServerLite.PluginManager
{
    public class PluginLoader
    {
        public static List<HTTPSecurePlugin> LoadPluginsFromFolder(string folderPath)
        {
            List<HTTPSecurePlugin> plugins = new();

            if (Directory.Exists(folderPath))
            {
                foreach (string dllFile in Directory.GetFiles(folderPath, "*.dll"))
                {
                    HTTPSecurePlugin? plugin = LoadPlugin(dllFile);
                    if (plugin != null)
                    {
                        CustomLogger.LoggerAccessor.LogInfo($"[HTTPS] - Plugin:{dllFile} Loaded.");
                        plugins.Add(plugin);
                    }
                }
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[HTTPS] - No Plugins Folder found: {folderPath}");

            return plugins;
        }

        public static HTTPSecurePlugin? LoadPlugin(string pluginPath)
        {
            try
            {
                foreach (Type type in Assembly.LoadFrom(pluginPath).GetTypes())
                {
                    if (typeof(HTTPSecurePlugin).IsAssignableFrom(type))
                        return Activator.CreateInstance(type) as HTTPSecurePlugin;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[HTTPS] - Error loading plugin '{pluginPath}': {ex}");
            }

            return null;
        }
    }
}
