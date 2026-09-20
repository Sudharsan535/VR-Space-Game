using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.Profiler.Auditors
{
    public static class VRAudioAuditor
    {
        public static List<VROptimizationIssue> Audit(VRPerformanceProfile profile)
        {
            var issues = new List<VROptimizationIssue>();
            string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip");

            foreach (string guid in audioGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Packages/") || path.Contains("/Editor/")) continue;

                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;

                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;

                string fileName = Path.GetFileName(path);
                var defaultSetting = importer.defaultSampleSettings;

                // Long audio clips (> 5 seconds) set to DecompressOnLoad cause extreme RAM waste
                if (clip.length > 5f && defaultSetting.loadType == AudioClipLoadType.DecompressOnLoad)
                {
                    bool isLongTrack = clip.length > 30f;
                    AudioClipLoadType recommendedType = isLongTrack 
                        ? AudioClipLoadType.Streaming 
                        : AudioClipLoadType.CompressedInMemory;

                    issues.Add(new VROptimizationIssue(
                        $"audio_load_{guid}",
                        $"Unoptimized Audio Load Type: {fileName} ({clip.length:0.0}s)",
                        $"'{fileName}' is {clip.length:0.0}s long and set to 'Decompress On Load'. This decompresses into raw PCM audio in RAM, consuming up to 10x more memory than necessary.",
                        "Wastes significant RAM on standalone headsets.",
                        VROptimizationCategory.AudioAndMemory,
                        VROptimizationSeverity.Warning,
                        path,
                        clip,
                        () =>
                        {
                            var s = importer.defaultSampleSettings;
                            s.loadType = recommendedType;
                            importer.defaultSampleSettings = s;
                            importer.SaveAndReimport();
                            Debug.Log($"[VR Profiler] Updated Audio Load Type to {recommendedType} on '{path}'");
                        }
                    ));
                }
            }

            return issues;
        }
    }
}
