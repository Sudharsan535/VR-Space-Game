using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.Profiler.Auditors
{
    public static class VRMeshAuditor
    {
        public static List<VROptimizationIssue> Audit(VRPerformanceProfile profile)
        {
            var issues = new List<VROptimizationIssue>();
            string[] modelGuids = AssetDatabase.FindAssets("t:Model");

            int polyLimit = profile switch
            {
                VRPerformanceProfile.MetaQuest2 => 10000,
                VRPerformanceProfile.MetaQuest3 => 20000,
                VRPerformanceProfile.PCVR => 45000,
                _ => 15000
            };

            foreach (string guid in modelGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Packages/") || path.Contains("/Editor/")) continue;

                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                // 1. Read/Write Enabled check (Doubles RAM usage)
                if (importer.isReadable)
                {
                    string assetName = Path.GetFileName(path);
                    issues.Add(new VROptimizationIssue(
                        $"mesh_rw_{guid}",
                        $"Mesh Read/Write Enabled: {assetName}",
                        $"'{assetName}' has CPU Read/Write enabled. This keeps a full copy of the vertex buffer in system RAM and VRAM simultaneously.",
                        "Doubles RAM consumption on Quest/Mobile VR headsets.",
                        VROptimizationCategory.MeshAndGeometry,
                        VROptimizationSeverity.Warning,
                        path,
                        null,
                        () =>
                        {
                            importer.isReadable = false;
                            importer.SaveAndReimport();
                            Debug.Log($"[VR Profiler] Disabled Read/Write on '{path}'");
                        }
                    ));
                }

                // 2. High Polygon Count Check
                var mainAsset = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                if (mainAsset != null)
                {
                    var meshFilters = mainAsset.GetComponentsInChildren<MeshFilter>(true);
                    int totalTris = 0;
                    foreach (var mf in meshFilters)
                    {
                        if (mf.sharedMesh != null)
                        {
                            totalTris += mf.sharedMesh.triangles.Length / 3;
                        }
                    }

                    if (totalTris > polyLimit)
                    {
                        string assetName = Path.GetFileName(path);
                        issues.Add(new VROptimizationIssue(
                            $"mesh_poly_{guid}",
                            $"High Polygon Mesh ({totalTris:N0} tris): {assetName}",
                            $"'{assetName}' contains {totalTris:N0} triangles, exceeding the recommended limit of {polyLimit:N0} for {profile}.",
                            "Heavy vertex shader workload and lower frame rates on mobile VR.",
                            VROptimizationCategory.MeshAndGeometry,
                            profile == VRPerformanceProfile.PCVR ? VROptimizationSeverity.Warning : VROptimizationSeverity.Critical,
                            path,
                            mainAsset
                        ));
                    }
                }
            }

            return issues;
        }
    }
}
