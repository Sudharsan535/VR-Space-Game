using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VRCore.Editor.Core
{
    /// <summary>
    /// Discovers and manages all IVRHubModule implementations across the editor assemblies.
    /// Enables zero-config pluggability for new VR modules and developer tools.
    /// </summary>
    public static class VRHubRegistry
    {
        private static List<IVRHubModule> s_CachedModules;
        private static bool s_IsInitialized;

        public static IReadOnlyList<IVRHubModule> GetModules()
        {
            if (!s_IsInitialized || s_CachedModules == null)
            {
                DiscoverModules();
            }
            return s_CachedModules;
        }

        public static void ReloadModules()
        {
            s_IsInitialized = false;
            DiscoverModules();
        }

        private static void DiscoverModules()
        {
            s_CachedModules = new List<IVRHubModule>();
            var interfaceType = typeof(IVRHubModule);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                // Skip system and Unity runtime assemblies for performance
                string fullName = assembly.FullName;
                if (fullName.StartsWith("System") || fullName.StartsWith("mscorlib") || fullName.StartsWith("UnityEngine"))
                {
                    continue;
                }

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type != null && interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        try
                        {
                            if (Activator.CreateInstance(type) is IVRHubModule module)
                            {
                                s_CachedModules.Add(module);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[VR Central Hub] Failed to instantiate module '{type.FullName}': {e.Message}");
                        }
                    }
                }
            }

            s_CachedModules.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            s_IsInitialized = true;
        }
    }
}
