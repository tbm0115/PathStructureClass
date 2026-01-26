using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PathStructure.Abstracts
{
    /// <summary>
    /// Discovers and activates plugin implementations from assemblies.
    /// </summary>
    public static class PluginServices
    {
        public sealed class AvailablePlugin
        {
            public AvailablePlugin(string assemblyPath, Type pluginType)
            {
                AssemblyPath = assemblyPath;
                PluginType = pluginType;
            }

            public string AssemblyPath { get; }
            public Type PluginType { get; }
        }

        public static IReadOnlyList<AvailablePlugin> FindPlugins(string directory, Type interfaceType)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Directory was not provided.", nameof(directory));
            }

            if (interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if (!Directory.Exists(directory))
            {
                return Enumerable.Empty<AvailablePlugin>().ToArray();
            }

            var plugins = new List<AvailablePlugin>();
            foreach (var dllPath in Directory.EnumerateFiles(directory, "*.dll"))
            {
                TryLoadPlugins(dllPath, interfaceType, plugins);
            }

            return plugins;
        }

        public static IReadOnlyList<AvailablePlugin> FindPlugins<TInterface>(string directory)
        {
            return FindPlugins(directory, typeof(TInterface));
        }

        public static object CreateInstance(AvailablePlugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            var assembly = Assembly.LoadFrom(plugin.AssemblyPath);
            return Activator.CreateInstance(plugin.PluginType);
        }

        private static void TryLoadPlugins(string assemblyPath, Type interfaceType, ICollection<AvailablePlugin> plugins)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (type.IsAbstract || !interfaceType.IsAssignableFrom(type))
                    {
                        continue;
                    }

                    plugins.Add(new AvailablePlugin(assemblyPath, type));
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Ignore assemblies that cannot be loaded; consumers can log if needed.
            }
            catch (BadImageFormatException)
            {
                // Ignore non-.NET assemblies.
            }
        }
    }
}
