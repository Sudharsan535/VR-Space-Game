using System;
using UnityEngine;

namespace VRCore.Editor.Modules.ProjectSettings
{
    /// <summary>
    /// Encapsulates an individual VR project configuration setting, its compliance check, and automated fix.
    /// </summary>
    public class VRSettingItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public VRSettingCategory Category { get; set; }
        public VRSettingSeverity Severity { get; set; }
        public VRPlatformPreset TargetPlatform { get; set; }
        
        public string CurrentValueText { get; private set; } = "Unknown";
        public string RecommendedValueText { get; set; }
        public bool IsCompliant { get; private set; }

        private readonly Func<(bool isCompliant, string currentValue)> _checkFunc;
        private readonly Action _fixAction;

        public VRSettingItem(
            string id,
            string title,
            string description,
            VRSettingCategory category,
            VRSettingSeverity severity,
            VRPlatformPreset targetPlatform,
            string recommendedValueText,
            Func<(bool isCompliant, string currentValue)> checkFunc,
            Action fixAction)
        {
            Id = id;
            Title = title;
            Description = description;
            Category = category;
            Severity = severity;
            TargetPlatform = targetPlatform;
            RecommendedValueText = recommendedValueText;
            _checkFunc = checkFunc;
            _fixAction = fixAction;
        }

        public void Refresh()
        {
            if (_checkFunc == null) return;

            try
            {
                var result = _checkFunc();
                IsCompliant = result.isCompliant;
                CurrentValueText = result.currentValue;
            }
            catch (Exception ex)
            {
                IsCompliant = false;
                CurrentValueText = $"Check error: {ex.Message}";
                Debug.LogWarning($"[VR Central Hub] Setting check failed for '{Title}': {ex.Message}");
            }
        }

        public bool ApplyFix()
        {
            if (_fixAction == null) return false;

            try
            {
                _fixAction();
                Refresh();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VR Central Hub] Failed to fix '{Title}': {ex.Message}");
                return false;
            }
        }
    }
}
