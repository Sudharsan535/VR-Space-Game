using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.ProjectSettings.Handlers
{
    public static class VRPlayerSettingsHandler
    {
        public static List<VRSettingItem> GetSettings()
        {
            var list = new List<VRSettingItem>();

            // 1. Color Space (Linear is required for VR lighting accuracy & OpenXR)
            list.Add(new VRSettingItem(
                "player_color_space",
                "Color Space (Linear)",
                "Linear color space is required for realistic lighting calculations, physically-based rendering, and OpenXR display drivers.",
                VRSettingCategory.Player,
                VRSettingSeverity.Critical,
                VRPlatformPreset.All,
                "Linear",
                () =>
                {
                    bool isLinear = PlayerSettings.colorSpace == ColorSpace.Linear;
                    return (isLinear, PlayerSettings.colorSpace.ToString());
                },
                () =>
                {
                    PlayerSettings.colorSpace = ColorSpace.Linear;
                    Debug.Log("[VR Central Hub] Switched Color Space to Linear.");
                }
            ));

            // 2. Multithreaded Rendering (Android / Mobile)
            list.Add(new VRSettingItem(
                "player_mobile_mt_rendering",
                "Multithreaded Rendering (Android)",
                "Enables parallel rendering threads on mobile XR chipsets (Meta Quest 2/3/Pro) to prevent frame drops.",
                VRSettingCategory.Player,
                VRSettingSeverity.Recommended,
                VRPlatformPreset.AndroidQuest,
                "Enabled (True)",
                () =>
                {
#if UNITY_2021_2_OR_NEWER
                    bool mt = PlayerSettings.GetMobileMTRendering(UnityEditor.Build.NamedBuildTarget.Android);
#else
                    bool mt = PlayerSettings.GetMobileMTRendering(BuildTargetGroup.Android);
#endif
                    return (mt, mt ? "Enabled" : "Disabled");
                },
                () =>
                {
#if UNITY_2021_2_OR_NEWER
                    PlayerSettings.SetMobileMTRendering(UnityEditor.Build.NamedBuildTarget.Android, true);
#else
                    PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
#endif
                    Debug.Log("[VR Central Hub] Enabled Android Multithreaded Rendering.");
                }
            ));

            // 3. Default Orientation for Mobile XR
            list.Add(new VRSettingItem(
                "player_orientation",
                "Default Screen Orientation",
                "Forces Landscape Left orientation for standalone VR head-mounted displays to prevent viewport rotation bugs.",
                VRSettingCategory.Player,
                VRSettingSeverity.Warning,
                VRPlatformPreset.AndroidQuest,
                "Landscape Left",
                () =>
                {
                    bool isLandscape = PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft;
                    return (isLandscape, PlayerSettings.defaultInterfaceOrientation.ToString());
                },
                () =>
                {
                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
                    Debug.Log("[VR Central Hub] Set Default Screen Orientation to Landscape Left.");
                }
            ));

            return list;
        }
    }
}
