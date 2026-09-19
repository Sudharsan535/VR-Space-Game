using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.ProjectSettings.Handlers
{
    public static class VRXRPluginSettingsHandler
    {
        public static List<VRSettingItem> GetSettings()
        {
            var list = new List<VRSettingItem>();

            // 1. XR Plugin Management Installation
            list.Add(new VRSettingItem(
                "xr_plugin_management_active",
                "XR Plug-in Management Installed",
                "XR Plug-in Management is the unified subsystem layer that initializes OpenXR, Oculus, and VR display/input drivers.",
                VRSettingCategory.XRPlugin,
                VRSettingSeverity.Critical,
                VRPlatformPreset.All,
                "Installed & Active",
                CheckXRManagementInstalled,
                () =>
                {
                    SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
                }
            ));

            // 2. Standalone (Windows) XR Loader (OpenXR / Oculus)
            list.Add(new VRSettingItem(
                "xr_loader_standalone",
                "Windows PC VR: XR Loader Active",
                "An active XR Loader (such as OpenXR or Oculus) is required to initialize the VR runtime when launching on PC.",
                VRSettingCategory.XRPlugin,
                VRSettingSeverity.Critical,
                VRPlatformPreset.WindowsPCVR,
                "OpenXR (or Oculus) Active",
                () => CheckXRLoaderForBuildTarget(BuildTargetGroup.Standalone),
                () =>
                {
                    SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
                }
            ));

            // 3. Android (Meta Quest) XR Loader (OpenXR / Oculus)
            list.Add(new VRSettingItem(
                "xr_loader_android",
                "Android (Quest): XR Loader Active",
                "An active XR Loader for Android is required to initialize headset rendering and tracking on Meta Quest.",
                VRSettingCategory.XRPlugin,
                VRSettingSeverity.Critical,
                VRPlatformPreset.AndroidQuest,
                "OpenXR (or Oculus) Active",
                () => CheckXRLoaderForBuildTarget(BuildTargetGroup.Android),
                () =>
                {
                    SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
                }
            ));

            return list;
        }

        private static (bool isCompliant, string currentValue) CheckXRManagementInstalled()
        {
            try
            {
                var generalSettingsType = Type.GetType("UnityEngine.XR.Management.XRGeneralSettings, Unity.XR.Management");
                if (generalSettingsType != null)
                {
                    return (true, "Installed");
                }
            }
            catch { }

            return (false, "Not Installed (Install via Package Manager)");
        }

        private static (bool isCompliant, string currentValue) CheckXRLoaderForBuildTarget(BuildTargetGroup targetGroup)
        {
            try
            {
                Type settingsPerTargetType = Type.GetType("UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget, Unity.XR.Management.Editor");
                if (settingsPerTargetType == null)
                {
                    return (false, "XR Management package not installed");
                }

                MethodInfo forTargetMethod = settingsPerTargetType.GetMethod("XRGeneralSettingsForBuildTarget", BindingFlags.Public | BindingFlags.Static);
                if (forTargetMethod != null)
                {
                    object generalSettings = forTargetMethod.Invoke(null, new object[] { targetGroup });
                    if (generalSettings != null)
                    {
                        PropertyInfo managerProp = generalSettings.GetType().GetProperty("Manager");
                        if (managerProp != null)
                        {
                            object manager = managerProp.GetValue(generalSettings);
                            if (manager != null)
                            {
                                PropertyInfo activeLoadersProp = manager.GetType().GetProperty("activeLoaders");
                                if (activeLoadersProp != null)
                                {
                                    var loaders = activeLoadersProp.GetValue(manager) as System.Collections.IEnumerable;
                                    if (loaders != null)
                                    {
                                        var loaderNames = new List<string>();
                                        foreach (var loader in loaders)
                                        {
                                            if (loader != null)
                                            {
                                                loaderNames.Add(loader.GetType().Name);
                                            }
                                        }

                                        if (loaderNames.Count > 0)
                                        {
                                            return (true, string.Join(", ", loaderNames));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Check error: {ex.Message}");
            }

            return (false, "No Active XR Loaders configured");
        }
    }
}
