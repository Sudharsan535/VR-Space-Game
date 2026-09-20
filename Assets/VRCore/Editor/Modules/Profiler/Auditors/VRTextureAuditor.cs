using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.Profiler.Auditors
{
    public static class VRTextureAuditor
    {
        public static List<VROptimizationIssue> Audit(VRPerformanceProfile profile)
        {
            var issues = new List<VROptimizationIssue>();
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");

            int maxRecommendedSize = profile == VRPerformanceProfile.PCVR ? 4096 : 2048;

            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Packages/") || path.Contains("/Editor/")) continue;

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                string fileName = Path.GetFileName(path);

                // 1. Missing Mipmaps (Essential for VR fresnel lenses)
                if (importer.textureType == TextureImporterType.Default || importer.textureType == TextureImporterType.NormalMap)
                {
                    if (!importer.mipmapEnabled)
                    {
                        issues.Add(new VROptimizationIssue(
                            $"tex_mip_{guid}",
                            $"Missing Mipmaps: {fileName}",
                            $"'{fileName}' does not have Mipmaps enabled. In VR headsets, textures without mipmaps cause severe subpixel crawling, aliasing shimmer, and GPU cache stalls.",
                            "Severe visual aliasing and high GPU texture sampling bandwidth.",
                            VROptimizationCategory.TexturesAndVRAM,
                            VROptimizationSeverity.Critical,
                            path,
                            null,
                            () =>
                            {
                                importer.mipmapEnabled = true;
                                importer.SaveAndReimport();
                                Debug.Log($"[VR Profiler] Enabled Mipmaps on '{path}'");
                            }
                        ));
                    }
                }

                // 2. Oversized Textures for Mobile VR
                if (importer.maxTextureSize > maxRecommendedSize)
                {
                    issues.Add(new VROptimizationIssue(
                        $"tex_size_{guid}",
                        $"Oversized Texture ({importer.maxTextureSize}px): {fileName}",
                        $"'{fileName}' max size is set to {importer.maxTextureSize}px. For {profile}, 2048px (or 1024px) is recommended to prevent VRAM exhaustion and fillrate bottleneck.",
                        "Consumes excessive GPU memory and memory bandwidth on mobile chipsets.",
                        VROptimizationCategory.TexturesAndVRAM,
                        VROptimizationSeverity.Warning,
                        path,
                        null,
                        () =>
                        {
                            importer.maxTextureSize = maxRecommendedSize;
                            importer.SaveAndReimport();
                            Debug.Log($"[VR Profiler] Clamped Max Texture Size to {maxRecommendedSize}px on '{path}'");
                        }
                    ));
                }

                // 3. Uncompressed Texture
                if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    issues.Add(new VROptimizationIssue(
                        $"tex_comp_{guid}",
                        $"Uncompressed Texture: {fileName}",
                        $"'{fileName}' is uncompressed (RGBA32). Compressing to ASTC (Android) or BC7/DXT (PC) saves up to 75% VRAM.",
                        "Wastes GPU memory and increases app download size significantly.",
                        VROptimizationCategory.TexturesAndVRAM,
                        VROptimizationSeverity.Warning,
                        path,
                        null,
                        () =>
                        {
                            importer.textureCompression = TextureImporterCompression.Compressed;
                            importer.SaveAndReimport();
                            Debug.Log($"[VR Profiler] Enabled Compression on '{path}'");
                        }
                    ));
                }
            }

            return issues;
        }
    }
}
