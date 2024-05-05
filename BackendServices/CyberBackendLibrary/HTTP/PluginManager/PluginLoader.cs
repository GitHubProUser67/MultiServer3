using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CyberBackendLibrary.HTTP.PluginManager
{
    public class PluginLoader
    {
        public static List<HTTPPlugin> LoadPluginsFromFolder(string folderPath)
        {
            List<HTTPPlugin> plugins = new List<HTTPPlugin>();

            if (Directory.Exists(folderPath))
            {
                foreach (string dllFile in Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories))
                {
                    HTTPPlugin? plugin = LoadPlugin(dllFile);
                    if (plugin != null)
                    {
                        CustomLogger.LoggerAccessor.LogInfo($"[HTTPS] - Plugin: {dllFile} Loaded.");
                        plugins.Add(plugin);
                    }
                }
            }
            else
                CustomLogger.LoggerAccessor.LogWarn($"[HTTPS] - No Plugins Folder found: {folderPath}");

            return plugins;
        }

        public static HTTPPlugin? LoadPlugin(string pluginPath)
        {
            try
            {
                foreach (Type type in Assembly.LoadFrom(pluginPath).GetTypes())
                {
                    try
                    {
                        if (typeof(HTTPPlugin).IsAssignableFrom(type))
                            return Activator.CreateInstance(type) as HTTPPlugin;
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        CustomLogger.LoggerAccessor.LogWarn($"[HTTPS] - Plugin: {pluginPath} is not compatible with this project, ignoring...");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[HTTPS] - Error loading plugin/dependency '{pluginPath}': {ex}");
            }

            return null;
        }
    }
}
