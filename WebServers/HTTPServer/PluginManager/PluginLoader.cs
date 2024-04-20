using System;
using System.Collections.Generic;
using System.IO;
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
                foreach (string dllFile in Directory.GetFiles(folderPath, "*.dll"))
                {
                    HTTPPlugin? plugin = LoadPlugin(dllFile);
                    if (plugin != null)
                    {
                        CustomLogger.LoggerAccessor.LogInfo($"[HTTP] - Plugin:{dllFile} Loaded.");
                        plugins.Add(plugin);
                    }
                }
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[HTTP] - No Plugins Folder found: {folderPath}");

            return plugins;
        }

        public static HTTPPlugin? LoadPlugin(string pluginPath)
        {
            try
            {
                foreach (Type type in Assembly.LoadFrom(pluginPath).GetTypes())
                {
                    if (typeof(HTTPPlugin).IsAssignableFrom(type))
                        return Activator.CreateInstance(type) as HTTPPlugin;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[HTTP] - Error loading plugin '{pluginPath}': {ex}");
            }

            return null;
        }
    }
}
