using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.Profiler.Auditors
{
    public static class VRPhysicsAuditor
    {
        public static List<VROptimizationIssue> Audit(VRPerformanceProfile profile)
        {
            var issues = new List<VROptimizationIssue>();

            float targetHz = profile switch
            {
                VRPerformanceProfile.MetaQuest2 => 72f,
                VRPerformanceProfile.MetaQuest3 => 90f,
                VRPerformanceProfile.PCVR => 90f,
                _ => 72f
            };

            float idealDeltaTime = 1f / targetHz;
            float currentFixedDelta = Time.fixedDeltaTime;

            // Check if fixedDeltaTime deviates by more than 0.002 from ideal
            if (Mathf.Abs(currentFixedDelta - idealDeltaTime) > 0.002f)
            {
                issues.Add(new VROptimizationIssue(
                    "physics_fixed_timestep",
                    $"Fixed Timestep Desynchronized ({1f / currentFixedDelta:0}Hz vs {targetHz:0}Hz Target)",
                    $"Time.fixedDeltaTime is currently {currentFixedDelta:F4}s (~{1f / currentFixedDelta:0}Hz). In VR, if physics timestep does not match the headset refresh rate ({targetHz:0}Hz = {idealDeltaTime:F4}s), grabbed objects, hand interactions, and physics simulations will suffer visible micro-stuttering / judder.",
                    "Causes physics jitter during hand interaction and locomotion.",
                    VROptimizationCategory.PhysicsAndTiming,
                    VROptimizationSeverity.Warning,
                    null,
                    null,
                    () =>
                    {
                        Time.fixedDeltaTime = idealDeltaTime;
                        Debug.Log($"[VR Profiler] Synchronized Fixed Timestep to {idealDeltaTime:F4}s ({targetHz:0}Hz for {profile})");
                    }
                ));
            }

            return issues;
        }
    }
}
