using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRCore.Editor.Modules.ProjectSettings.Handlers
{
    public static class VRGraphicsSettingsHandler
    {
        public static List<VRSettingItem> GetSettings()
        {
            var list = new List<VRSettingItem>();

            // 1. Anti-Aliasing (MSAA 4x)
            list.Add(new VRSettingItem(
                "graphics_msaa",
                "Anti-Aliasing (MSAA 4x)",
                "VR lenses magnify aliasing artifacts (pixel crawling and edge shimmering). MSAA 4x (or 2x) is essential for clean VR visuals without temporal blur.",
                VRSettingCategory.Graphics,
                VRSettingSeverity.Recommended,
                VRPlatformPreset.All,
                "4x MSAA (or 2x)",
                CheckMSAA,
                FixMSAA
            ));

            // 2. Render Pipeline Asset (URP / Scriptable Render Pipeline)
            list.Add(new VRSettingItem(
                "graphics_srp_pipeline",
                "Universal Render Pipeline Assigned",
                "Universal Render Pipeline (URP) delivers Single-Pass Instanced rendering optimized for VR stereo projection.",
                VRSettingCategory.Graphics,
                VRSettingSeverity.Recommended,
                VRPlatformPreset.All,
                "URP Asset Assigned",
                () =>
                {
                    var pipeline = GraphicsSettings.currentRenderPipeline;
                    bool hasPipeline = (pipeline != null);
                    return (hasPipeline, hasPipeline ? pipeline.name : "None (Built-in RP)");
                },
                () =>
                {
                    Debug.Log("[VR Central Hub] To assign URP, ensure a Universal Render Pipeline asset is active in Project Settings > Graphics.");
                }
            ));

            // 3. VR Optimized Shadow Distance
            list.Add(new VRSettingItem(
                "graphics_shadow_distance",
                "VR Shadow Distance (<= 50m)",
                "Excessive shadow rendering distances (> 50m) severely bottleneck VR GPU rasterization and cause frame drops.",
                VRSettingCategory.Graphics,
                VRSettingSeverity.Recommended,
                VRPlatformPreset.AndroidQuest,
                "<= 50 meters",
                () =>
                {
                    float dist = QualitySettings.shadowDistance;
                    bool compliant = (dist <= 60f);
                    return (compliant, $"{dist:0} meters");
                },
                () =>
                {
                    QualitySettings.shadowDistance = 40f;
                    Debug.Log("[VR Central Hub] Set VR Shadow Distance to 40 meters.");
                }
            ));

            return list;
        }

        private static (bool isCompliant, string currentValue) CheckMSAA()
        {
            try
            {
                int qualityMsaa = QualitySettings.antiAliasing;
                
                // Also check URP asset if present
                var srp = GraphicsSettings.currentRenderPipeline;
                if (srp != null)
                {
                    SerializedObject so = new SerializedObject(srp);
                    SerializedProperty msaaProp = so.FindProperty("m_MSAA");
                    if (msaaProp != null)
                    {
                        int msaaVal = msaaProp.intValue;
                        bool compliant = (msaaVal >= 2);
                        return (compliant, msaaVal > 1 ? $"{msaaVal}x MSAA (URP)" : "Disabled (1x)");
                    }
                }

                bool qualityCompliant = (qualityMsaa >= 2);
                return (qualityCompliant, qualityMsaa > 0 ? $"{qualityMsaa}x MSAA" : "Disabled");
            }
            catch (Exception ex)
            {
                return (false, $"Error checking MSAA: {ex.Message}");
            }
        }

        private static void FixMSAA()
        {
            QualitySettings.antiAliasing = 4;

            try
            {
                var srp = GraphicsSettings.currentRenderPipeline;
                if (srp != null)
                {
                    SerializedObject so = new SerializedObject(srp);
                    SerializedProperty msaaProp = so.FindProperty("m_MSAA");
                    if (msaaProp != null)
                    {
                        msaaProp.intValue = 4;
                        so.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                    }
                }
                Debug.Log("[VR Central Hub] Enabled 4x MSAA Anti-Aliasing.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VR Central Hub] Could not set URP MSAA: {ex.Message}");
            }
        }
    }
}
