using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.ProjectSettings.Handlers
{
    public static class VRInputSettingsHandler
    {
        public static List<VRSettingItem> GetSettings()
        {
            var list = new List<VRSettingItem>();

            list.Add(new VRSettingItem(
                "input_handling",
                "Active Input Handling",
                "Unity's new Input System Package (or Both) is required for VR controller tracking, action mapping, and XR Interaction Toolkit.",
                VRSettingCategory.Input,
                VRSettingSeverity.Critical,
                VRPlatformPreset.All,
                "Input System Package (New) or Both",
                CheckActiveInputHandling,
                FixActiveInputHandling
            ));

            return list;
        }

        private static (bool isCompliant, string currentValue) CheckActiveInputHandling()
        {
            try
            {
                var playerSettingsAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
                if (playerSettingsAsset != null && playerSettingsAsset.Length > 0)
                {
                    SerializedObject so = new SerializedObject(playerSettingsAsset[0]);
                    SerializedProperty prop = so.FindProperty("activeInputHandler");
                    if (prop != null)
                    {
                        // 0 = Old, 1 = New, 2 = Both
                        int val = prop.intValue;
                        bool compliant = (val == 1 || val == 2);
                        string status = val switch
                        {
                            0 => "Legacy Input Manager (Invalid for VR)",
                            1 => "Input System Package (New)",
                            2 => "Both (New & Legacy)",
                            _ => $"Unknown ({val})"
                        };
                        return (compliant, status);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error reading setting: {ex.Message}");
            }

            return (true, "Input System Verified");
        }

        private static void FixActiveInputHandling()
        {
            try
            {
                var playerSettingsAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
                if (playerSettingsAsset != null && playerSettingsAsset.Length > 0)
                {
                    SerializedObject so = new SerializedObject(playerSettingsAsset[0]);
                    SerializedProperty prop = so.FindProperty("activeInputHandler");
                    if (prop != null)
                    {
                        // Set to 1 (Input System Package New) or 2 (Both)
                        prop.intValue = 1;
                        so.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        Debug.Log("[VR Central Hub] Switched Active Input Handling to 'Input System Package (New)'. Restarting Unity may be required if prompted.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VR Central Hub] Could not update Active Input Handling: {ex.Message}");
            }
        }
    }
}
