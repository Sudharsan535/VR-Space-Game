using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRCore.Editor.Modules.Profiler
{
    public class VROptimizationIssue
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Impact { get; set; }
        public VROptimizationCategory Category { get; set; }
        public VROptimizationSeverity Severity { get; set; }
        
        public string AssetPath { get; set; }
        public Object TargetObject { get; set; }
        public bool CanAutoFix { get; set; }

        private readonly Action _fixAction;

        public VROptimizationIssue(
            string id,
            string title,
            string description,
            string impact,
            VROptimizationCategory category,
            VROptimizationSeverity severity,
            string assetPath = null,
            Object targetObject = null,
            Action fixAction = null)
        {
            Id = id;
            Title = title;
            Description = description;
            Impact = impact;
            Category = category;
            Severity = severity;
            AssetPath = assetPath;
            TargetObject = targetObject;
            _fixAction = fixAction;
            CanAutoFix = fixAction != null;
        }

        public void PingTarget()
        {
            if (TargetObject != null)
            {
                EditorGUIUtility.PingObject(TargetObject);
                Selection.activeObject = TargetObject;
            }
            else if (!string.IsNullOrEmpty(AssetPath))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(AssetPath);
                if (obj != null)
                {
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }
            }
        }

        public bool ExecuteFix()
        {
            if (_fixAction == null) return false;

            try
            {
                _fixAction();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VR Profiler] Failed to auto-fix '{Title}': {ex.Message}");
                return false;
            }
        }
    }
}
