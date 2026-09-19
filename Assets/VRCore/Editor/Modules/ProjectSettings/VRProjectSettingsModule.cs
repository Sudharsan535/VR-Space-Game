using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRCore.Editor.Core;
using VRCore.Editor.Modules.ProjectSettings.Handlers;

namespace VRCore.Editor.Modules.ProjectSettings
{
    /// <summary>
    /// Central Project Settings & Platform Configurator module for VR Central Hub.
    /// Inspects and applies Player, XR Plugin, Input, Android (Quest), Windows (PCVR), and Graphics settings.
    /// </summary>
    public class VRProjectSettingsModule : VRHubModuleBase
    {
        public override string ModuleId => "vr_project_settings";
        public override string DisplayName => "Project Settings";
        public override string Description => "Configure Player, XR Plugin, Input, Android, Windows, and Graphics for VR.";
        public override string IconName => "d_Settings";
        public override int Priority => 15;

        private const float NarrowThreshold = 460f;

        private List<VRSettingItem> _settings;
        private string _searchFilter = "";
        private VRSettingCategory _selectedCategory = VRSettingCategory.All;
        private VRPlatformPreset _selectedPlatform = VRPlatformPreset.All;

        public override void OnEnable()
        {
            LoadAllSettings();
            RefreshAllSettings();
        }

        private void LoadAllSettings()
        {
            _settings = new List<VRSettingItem>();
            _settings.AddRange(VRPlayerSettingsHandler.GetSettings());
            _settings.AddRange(VRInputSettingsHandler.GetSettings());
            _settings.AddRange(VRPlatformSettingsHandler.GetSettings());
            _settings.AddRange(VRGraphicsSettingsHandler.GetSettings());
            _settings.AddRange(VRXRPluginSettingsHandler.GetSettings());
        }

        public void RefreshAllSettings()
        {
            if (_settings == null) return;
            foreach (var item in _settings)
            {
                item.Refresh();
            }
        }

        public void ApplyAllPreset(VRPlatformPreset preset)
        {
            var targets = _settings.Where(s => 
                !s.IsCompliant && 
                (preset == VRPlatformPreset.All || s.TargetPlatform == VRPlatformPreset.All || s.TargetPlatform == preset)
            ).ToList();

            if (targets.Count == 0)
            {
                EditorUtility.DisplayDialog("VR Central Hub", "All inspected settings for this preset are already compliant!", "OK");
                return;
            }

            int fixedCount = 0;
            foreach (var item in targets)
            {
                if (item.ApplyFix())
                {
                    fixedCount++;
                }
            }

            RefreshAllSettings();
            EditorUtility.DisplayDialog("VR Central Hub", $"Applied {fixedCount} VR configuration settings.", "OK");
        }

        protected override void DrawContent()
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            bool isNarrow = viewWidth < NarrowThreshold;

            DrawToolbar(isNarrow);
            DrawSummaryBanner(isNarrow);
            DrawFilters(isNarrow);
            DrawSettingsList(isNarrow);
        }

        private void DrawToolbar(bool isNarrow)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label(EditorGUIUtility.IconContent("d_Search Icon"), GUILayout.Width(18));
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.MinWidth(60), GUILayout.ExpandWidth(true));

            if (!string.IsNullOrEmpty(_searchFilter) && GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(18)))
            {
                _searchFilter = "";
                GUI.FocusControl(null);
            }

            if (!isNarrow)
            {
                GUILayout.FlexibleSpace();
            }

            GUIContent refreshBtn = isNarrow 
                ? new GUIContent(EditorGUIUtility.IconContent("d_Refresh").image, "Refresh Settings") 
                : new GUIContent(" Recheck", EditorGUIUtility.IconContent("d_Refresh").image);

            if (GUILayout.Button(refreshBtn, EditorStyles.toolbarButton, GUILayout.Width(isNarrow ? 26 : 74)))
            {
                RefreshAllSettings();
            }

            if (GUILayout.Button(new GUIContent(isNarrow ? "..." : " Project Settings..."), EditorStyles.toolbarButton, GUILayout.Width(isNarrow ? 30 : 110)))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        private void DrawSummaryBanner(bool isNarrow)
        {
            BeginCard();

            int total = _settings.Count;
            int compliant = _settings.Count(s => s.IsCompliant);
            int nonCompliant = total - compliant;
            int criticalIssues = _settings.Count(s => !s.IsCompliant && s.Severity == VRSettingSeverity.Critical);

            if (isNarrow)
            {
                EditorGUILayout.LabelField("VR Configuration Health", EditorStyles.boldLabel);
                
                string healthText = $"Passed: <b>{compliant}/{total}</b> | ";
                if (criticalIssues > 0)
                {
                    healthText += $"<color=#e84c4c><b>{criticalIssues} Critical</b></color>";
                }
                else if (nonCompliant > 0)
                {
                    healthText += $"<color=#f2a626><b>{nonCompliant} Optimizations</b></color>";
                }
                else
                {
                    healthText += "<color=#38b860><b>100% VR Ready</b></color>";
                }

                GUIStyle richLabel = new GUIStyle(EditorStyles.miniLabel) { richText = true, wordWrap = true };
                EditorGUILayout.LabelField(healthText, richLabel);

                GUILayout.Space(4);

                if (nonCompliant > 0)
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent(" Apply All VR Settings", EditorGUIUtility.IconContent("d_Checkmark").image), GUILayout.Height(26), GUILayout.ExpandWidth(true)))
                    {
                        ApplyAllPreset(VRPlatformPreset.All);
                    }
                    GUI.backgroundColor = prev;
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply Android (Quest)", EditorStyles.miniButton, GUILayout.Height(22), GUILayout.ExpandWidth(true)))
                {
                    ApplyAllPreset(VRPlatformPreset.AndroidQuest);
                }
                if (GUILayout.Button("Apply Windows (PCVR)", EditorStyles.miniButton, GUILayout.Height(22), GUILayout.ExpandWidth(true)))
                {
                    ApplyAllPreset(VRPlatformPreset.WindowsPCVR);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("VR Project Configuration Status", EditorStyles.boldLabel);

                string healthText = $"Compliant: <b>{compliant}/{total}</b> settings | ";
                if (criticalIssues > 0)
                {
                    healthText += $"<color=#e84c4c><b>{criticalIssues} Critical Issues Found</b></color>";
                }
                else if (nonCompliant > 0)
                {
                    healthText += $"<color=#f2a626>{nonCompliant} Recommended Fixes Available</color>";
                }
                else
                {
                    healthText += "<color=#38b860><b>All VR Settings Configured Perfectly</b></color>";
                }

                GUIStyle richLabel = new GUIStyle(EditorStyles.miniLabel) { richText = true, wordWrap = true };
                EditorGUILayout.LabelField(healthText, richLabel);
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical();
                if (nonCompliant > 0)
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent(" Apply All Recommended Settings", EditorGUIUtility.IconContent("d_Checkmark").image), GUILayout.Height(26)))
                    {
                        ApplyAllPreset(VRPlatformPreset.All);
                    }
                    GUI.backgroundColor = prev;
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply Quest Preset", EditorStyles.miniButton, GUILayout.Height(22)))
                {
                    ApplyAllPreset(VRPlatformPreset.AndroidQuest);
                }
                if (GUILayout.Button("Apply PC VR Preset", EditorStyles.miniButton, GUILayout.Height(22)))
                {
                    ApplyAllPreset(VRPlatformPreset.WindowsPCVR);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            EndCard();
            GUILayout.Space(6);
        }

        private void DrawFilters(bool isNarrow)
        {
            if (isNarrow)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Category:", GUILayout.Width(60));
                _selectedCategory = (VRSettingCategory)EditorGUILayout.EnumPopup(_selectedCategory, EditorStyles.popup);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Platform:", GUILayout.Width(60));
                _selectedPlatform = (VRPlatformPreset)EditorGUILayout.EnumPopup(_selectedPlatform, EditorStyles.popup);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                
                // Platform tabs
                GUILayout.Label("Target:", EditorStyles.miniLabel, GUILayout.Width(45));
                DrawPlatformTab(VRPlatformPreset.All, "All");
                DrawPlatformTab(VRPlatformPreset.AndroidQuest, "Android (Quest)");
                DrawPlatformTab(VRPlatformPreset.WindowsPCVR, "Windows (PCVR)");

                GUILayout.FlexibleSpace();

                // Category dropdown
                GUILayout.Label("Category:", EditorStyles.miniLabel, GUILayout.Width(55));
                _selectedCategory = (VRSettingCategory)EditorGUILayout.EnumPopup(_selectedCategory, EditorStyles.toolbarPopup, GUILayout.Width(130));

                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(6);
        }

        private void DrawPlatformTab(VRPlatformPreset preset, string label)
        {
            bool isSelected = (_selectedPlatform == preset);
            Color prev = GUI.backgroundColor;
            if (isSelected) GUI.backgroundColor = VRHubStyles.AccentColor;

            if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Height(20)))
            {
                _selectedPlatform = preset;
            }

            GUI.backgroundColor = prev;
        }

        private void DrawSettingsList(bool isNarrow)
        {
            var filtered = _settings.Where(s =>
                (_selectedCategory == VRSettingCategory.All || s.Category == _selectedCategory) &&
                (_selectedPlatform == VRPlatformPreset.All || s.TargetPlatform == VRPlatformPreset.All || s.TargetPlatform == _selectedPlatform) &&
                (string.IsNullOrEmpty(_searchFilter) ||
                 s.Title.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 s.Description.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            if (filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No settings match the current filter.", MessageType.Info);
                return;
            }

            foreach (var item in filtered)
            {
                DrawSettingCard(item, isNarrow);
            }
        }

        private void DrawSettingCard(VRSettingItem item, bool isNarrow)
        {
            BeginCard();

            if (isNarrow)
            {
                // Compact Stacked View
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.Title, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                DrawSeverityBadge(item.Severity);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"[{item.Category}] - Target: {item.TargetPlatform}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(item.Description, VRHubStyles.SubtitleStyle);

                GUILayout.Space(4);

                // Values
                EditorGUILayout.LabelField($"Current: <b>{item.CurrentValueText}</b>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
                EditorGUILayout.LabelField($"Recommended: {item.RecommendedValueText}", EditorStyles.miniLabel);

                GUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                DrawComplianceBadge(item.IsCompliant, isNarrow);

                if (!item.IsCompliant)
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent(" Fix / Apply", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(22), GUILayout.ExpandWidth(true)))
                    {
                        item.ApplyFix();
                    }
                    GUI.backgroundColor = prev;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // Wide Row View
                EditorGUILayout.BeginHorizontal();

                // Left Info Column
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(item.Title, EditorStyles.boldLabel, GUILayout.MinWidth(180), GUILayout.MaxWidth(280));
                DrawSeverityBadge(item.Severity);
                EditorGUILayout.LabelField($"[{item.Category}]", EditorStyles.miniLabel, GUILayout.Width(90));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(item.Description, VRHubStyles.SubtitleStyle);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Current: <b>{item.CurrentValueText}</b>", new GUIStyle(EditorStyles.miniLabel) { richText = true }, GUILayout.MinWidth(160));
                EditorGUILayout.LabelField($"Recommended: {item.RecommendedValueText}", EditorStyles.miniLabel, GUILayout.MinWidth(180));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                GUILayout.Space(8);

                // Right Actions Column
                EditorGUILayout.BeginVertical(GUILayout.Width(150));
                DrawComplianceBadge(item.IsCompliant, false);

                GUILayout.Space(4);

                if (!item.IsCompliant)
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent(" Fix / Apply", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(22), GUILayout.ExpandWidth(true)))
                    {
                        item.ApplyFix();
                    }
                    GUI.backgroundColor = prev;
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            EndCard();
        }

        private void DrawSeverityBadge(VRSettingSeverity severity)
        {
            switch (severity)
            {
                case VRSettingSeverity.Critical:
                    VRHubStyles.DrawBadge("CRITICAL", new Color(0.85f, 0.25f, 0.25f, 0.35f), VRHubStyles.ErrorColor, 68f);
                    break;
                case VRSettingSeverity.Warning:
                    VRHubStyles.DrawBadge("WARNING", new Color(0.9f, 0.65f, 0.15f, 0.35f), VRHubStyles.WarningColor, 74f);
                    break;
                case VRSettingSeverity.Recommended:
                    VRHubStyles.DrawBadge("RECOMMENDED", new Color(0.24f, 0.54f, 0.96f, 0.25f), VRHubStyles.AccentColor, 92f);
                    break;
                case VRSettingSeverity.Info:
                    VRHubStyles.DrawBadge("INFO", new Color(0.5f, 0.5f, 0.5f, 0.2f), VRHubStyles.NeutralMutedColor, 54f);
                    break;
            }
        }

        private void DrawComplianceBadge(bool isCompliant, bool isNarrow)
        {
            float width = isNarrow ? 0 : 130f;
            if (isCompliant)
            {
                VRHubStyles.DrawBadge("PASSED", new Color(0.22f, 0.72f, 0.38f, 0.35f), VRHubStyles.SuccessColor, width);
            }
            else
            {
                VRHubStyles.DrawBadge("NEEDS FIX", new Color(0.85f, 0.25f, 0.25f, 0.35f), VRHubStyles.ErrorColor, width);
            }
        }
    }
}
