using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRCore.Editor.Modules.Profiler.Auditors
{
    public static class VRSceneLightingAuditor
    {
        public static List<VROptimizationIssue> Audit(VRPerformanceProfile profile)
        {
            var issues = new List<VROptimizationIssue>();

            // 1. Audit Realtime Lights in active scene
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
            var lights = Object.FindObjectsOfType<Light>();
#endif
            int realtimeShadowCastingLights = 0;
            Light secondShadowLight = null;

            foreach (var l in lights)
            {
                if (l.enabled && l.lightmapBakeType != LightmapBakeType.Baked)
                {
                    if (l.shadows != LightShadows.None)
                    {
                        realtimeShadowCastingLights++;
                        if (realtimeShadowCastingLights > 1 && secondShadowLight == null)
                        {
                            secondShadowLight = l;
                        }
                    }
                }
            }

            if (realtimeShadowCastingLights > 1 && profile != VRPerformanceProfile.PCVR)
            {
                issues.Add(new VROptimizationIssue(
                    "scene_multiple_shadow_lights",
                    $"Multiple Realtime Shadow-Casting Lights ({realtimeShadowCastingLights})",
                    $"The active scene contains {realtimeShadowCastingLights} realtime shadow-casting lights. On mobile VR (Quest), each realtime shadow light multiplies draw calls and pixel fillrate per eye, causing severe thermal throttling.",
                    "Cripples GPU fillrate and frame rate on standalone VR.",
                    VROptimizationCategory.SceneAndLighting,
                    VROptimizationSeverity.Critical,
                    null,
                    secondShadowLight,
                    () =>
                    {
                        if (secondShadowLight != null)
                        {
                            secondShadowLight.shadows = LightShadows.None;
                            Debug.Log($"[VR Profiler] Disabled realtime shadows on secondary light '{secondShadowLight.name}'");
                        }
                    }
                ));
            }

            // 2. Camera Far Clip Plane Check
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                if (mainCam.farClipPlane > 600f && profile != VRPerformanceProfile.PCVR)
                {
                    issues.Add(new VROptimizationIssue(
                        "scene_camera_far_clip",
                        $"Camera Far Clip Plane is Large ({mainCam.farClipPlane:0}m)",
                        $"Main Camera far clip plane is set to {mainCam.farClipPlane:0}m. In VR, keeping far clip <= 500m prevents rendering unseen geometry and avoids depth buffer precision z-fighting.",
                        "Increases draw calls and reduces Z-buffer accuracy.",
                        VROptimizationCategory.SceneAndLighting,
                        VROptimizationSeverity.Optimization,
                        null,
                        mainCam,
                        () =>
                        {
                            mainCam.farClipPlane = 400f;
                            EditorUtility.SetDirty(mainCam);
                            Debug.Log("[VR Profiler] Set Main Camera Far Clip to 400m");
                        }
                    ));
                }
            }

            // 3. Materials GPU Instancing Check
            string[] materialGuids = AssetDatabase.FindAssets("t:Material");
            int nonInstancedCount = 0;
            Material firstNonInstanced = null;

            foreach (string guid in materialGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Packages/") || path.Contains("/Editor/")) continue;

                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null && !mat.enableInstancing && mat.shader != null && mat.shader.name.StartsWith("Universal Render Pipeline"))
                {
                    nonInstancedCount++;
                    if (firstNonInstanced == null) firstNonInstanced = mat;
                }
            }

            if (nonInstancedCount > 0)
            {
                issues.Add(new VROptimizationIssue(
                    "scene_gpu_instancing",
                    $"{nonInstancedCount} Materials Missing GPU Instancing",
                    $"Found {nonInstancedCount} URP materials without GPU Instancing enabled. Enabling GPU Instancing allows identical VR props, environment meshes, and interactables to render in a single batch per eye.",
                    "Increases CPU draw call submission overhead in VR.",
                    VROptimizationCategory.SceneAndLighting,
                    VROptimizationSeverity.Optimization,
                    firstNonInstanced != null ? AssetDatabase.GetAssetPath(firstNonInstanced) : null,
                    firstNonInstanced,
                    () =>
                    {
                        foreach (string guid in materialGuids)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guid);
                            if (path.StartsWith("Packages/")) continue;
                            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                            if (mat != null && mat.shader != null && mat.shader.name.StartsWith("Universal Render Pipeline"))
                            {
                                mat.enableInstancing = true;
                                EditorUtility.SetDirty(mat);
                            }
                        }
                        AssetDatabase.SaveAssets();
                        Debug.Log($"[VR Profiler] Enabled GPU Instancing across project URP materials.");
                    }
                ));
            }

            return issues;
        }
    }
}
