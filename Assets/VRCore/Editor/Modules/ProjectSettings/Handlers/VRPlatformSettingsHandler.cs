using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.ProjectSettings.Handlers
{
    public static class VRPlatformSettingsHandler
    {
        public static List<VRSettingItem> GetSettings()
        {
            var list = new List<VRSettingItem>();

            // --- ANDROID (META QUEST & STANDALONE XR) ---

            // 1. Android Scripting Backend (IL2CPP)
            list.Add(new VRSettingItem(
                "android_scripting_backend",
                "Android Scripting Backend (IL2CPP)",
                "IL2CPP is mandatory for 64-bit ARM builds on Meta Quest and produces high-performance C++ binaries.",
                VRSettingCategory.Android,
                VRSettingSeverity.Critical,
                VRPlatformPreset.AndroidQuest,
                "IL2CPP",
                () =>
                {
#if UNITY_2021_2_OR_NEWER
                    var backend = PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android);
#else
                    var backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
#endif
                    bool compliant = (backend == ScriptingImplementation.IL2CPP);
                    return (compliant, backend.ToString());
                },
                () =>
                {
#if UNITY_2021_2_OR_NEWER
                    PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
#else
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
#endif
                    Debug.Log("[VR Central Hub] Set Android Scripting Backend to IL2CPP.");
                }
            ));

            // 2. Android Target Architecture (ARM64)
            list.Add(new VRSettingItem(
                "android_architecture",
                "Android Target Architecture (ARM64)",
                "Meta Quest 2, 3, and Pro run 64-bit Qualcomm Snapdragon XR processors. ARM64 must be enabled and ARMv7 disabled.",
                VRSettingCategory.Android,
                VRSettingSeverity.Critical,
                VRPlatformPreset.AndroidQuest,
                "ARM64",
                () =>
                {
                    var arch = PlayerSettings.Android.targetArchitectures;
                    bool compliant = (arch == AndroidArchitecture.ARM64 || arch.HasFlag(AndroidArchitecture.ARM64));
                    return (compliant, arch.ToString());
                },
                () =>
                {
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                    Debug.Log("[VR Central Hub] Set Android Target Architecture to ARM64.");
                }
            ));

            // 3. Android Minimum API Level (API 29+)
            list.Add(new VRSettingItem(
                "android_min_sdk",
                "Android Minimum API Level (API 29+)",
                "Meta Quest OS requires at minimum Android 10.0 (API Level 29) or higher to support modern OpenXR features.",
                VRSettingCategory.Android,
                VRSettingSeverity.Critical,
                VRPlatformPreset.AndroidQuest,
                "Android 10.0 (API 29) or higher",
                () =>
                {
                    int minSdk = (int)PlayerSettings.Android.minSdkVersion;
                    bool compliant = (minSdk >= 29);
                    return (compliant, $"API Level {minSdk}");
                },
                () =>
                {
                    PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)29;
                    Debug.Log("[VR Central Hub] Set Android Minimum API Level to 29 (Android 10.0).");
                }
            ));

            // 4. Android Texture Compression (ASTC)
            list.Add(new VRSettingItem(
                "android_astc_compression",
                "Android Texture Compression (ASTC)",
                "ASTC texture compression provides superior visual fidelity, smaller download footprints, and lower memory bandwidth on mobile VR chips.",
                VRSettingCategory.Android,
                VRSettingSeverity.Recommended,
                VRPlatformPreset.AndroidQuest,
                "ASTC",
                () =>
                {
                    var subtarget = EditorUserBuildSettings.androidBuildSubtarget;
                    bool compliant = (subtarget == MobileTextureSubtarget.ASTC);
                    return (compliant, subtarget.ToString());
                },
                () =>
                {
                    EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
                    Debug.Log("[VR Central Hub] Set Android Texture Compression format to ASTC.");
                }
            ));

            // --- WINDOWS (PC VR & STEAMVR / OCULUS LINK) ---

            // 5. Windows Run In Background
            list.Add(new VRSettingItem(
                "windows_run_in_background",
                "Windows: Run In Background",
                "Allows the VR simulation loop and tracking to continue rendering smoothly even when the PC companion window loses OS focus.",
                VRSettingCategory.Windows,
                VRSettingSeverity.Recommended,
                VRPlatformPreset.WindowsPCVR,
                "Enabled (True)",
                () =>
                {
                    bool rib = PlayerSettings.runInBackground;
                    return (rib, rib ? "Enabled" : "Disabled");
                },
                () =>
                {
                    PlayerSettings.runInBackground = true;
                    Debug.Log("[VR Central Hub] Enabled Windows Run In Background.");
                }
            ));

            return list;
        }
    }
}
