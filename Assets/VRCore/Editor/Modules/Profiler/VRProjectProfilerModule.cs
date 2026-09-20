using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRCore.Editor.Core;
using VRCore.Editor.Modules.Profiler.Auditors;

namespace VRCore.Editor.Modules.Profiler
{
    /// <summary>
    /// Central VR Profiler & Optimization Analyzer module for VR Central Hub.
    /// Audits project assets, textures, meshes, audio, lighting, and physics against VR performance standards.
    /// </summary>
    public class VRProjectProfilerModule : VRHubModuleBase
    {
        public override string ModuleId => "vr_profiler";
        public override string DisplayName => "Profiler & Optimizer";
        public override string Description => "Audit meshes, textures, audio, lighting, and physics against VR frame budgets.";
        public override string IconName => "d_UnityEditor.ProfilerWindow";
        public override int Priority => 18;

        private const float NarrowThreshold = 460f;

        private VRPerformanceProfile _targetProfile = VRPerformanceProfile.MetaQuest2;
        private List<VROptimizationIssue> _issues = new List<VROptimizationIssue>();
        private bool _hasScanned = false;
        private string _searchFilter = "";
        private VROptimizationCategory _selectedCategory = VROptimizationCategory.All;

        public override void OnEnable()
        {
            if (!_hasScanned)
            {
                RunAudit();
            }
        }

        public void RunAudit()
        {
            _issues.Clear();

            _issues.AddRange(VRMeshAuditor.Audit(_targetProfile));
            _issues.AddRange(VRTextureAuditor.Audit(_targetProfile));
            _issues.AddRange(VRAudioAuditor.Audit(_targetProfile));
            _issues.AddRange(VRSceneLightingAuditor.Audit(_targetProfile));
            _issues.AddRange(VRPhysicsAuditor.Audit(_targetProfile));

            _hasScanned = true;
        }

        public void FixAllAutoFixable()
        {
            var fixable = _issues.Where(i => i.CanAutoFix).ToList();
            if (fixable.Count == 0)
            {
                EditorUtility.DisplayDialog("VR Profiler", "No auto-fixable issues currently found.", "OK");
                return;
            }

            int fixedCount = 0;
            foreach (var item in fixable)
            {
                if (item.ExecuteFix())
                {
                    fixedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            RunAudit();
            EditorUtility.DisplayDialog("VR Profiler", $"Optimized and resolved {fixedCount} VR performance issues.", "Great!");
        }

        protected override void DrawContent()
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            bool isNarrow = viewWidth < NarrowThreshold;

            DrawProfileSelector(isNarrow);
            DrawHealthScoreBanner(isNarrow);
            DrawFilters(isNarrow);
            DrawIssueList(isNarrow);
        }

        private void DrawProfileSelector(bool isNarrow)
        {
            BeginCard();
            EditorGUILayout.LabelField("Target VR Hardware Preset", EditorStyles.boldLabel);

            if (isNarrow)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
                var newProfile = (VRPerformanceProfile)EditorGUILayout.EnumPopup(_targetProfile, EditorStyles.popup);
                if (newProfile != _targetProfile)
                {
                    _targetProfile = newProfile;
                    RunAudit();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawProfileButton(VRPerformanceProfile.MetaQuest2, "Meta Quest 2 (72Hz / 13.8ms)");
                DrawProfileButton(VRPerformanceProfile.MetaQuest3, "Meta Quest 3 (90Hz / 11.1ms)");
                DrawProfileButton(VRPerformanceProfile.PCVR, "PC VR (90-120Hz / 8.3ms)");
                EditorGUILayout.EndHorizontal();
            }

            EndCard();
            GUILayout.Space(6);
        }

        private void DrawProfileButton(VRPerformanceProfile profile, string label)
        {
            bool isSelected = (_targetProfile == profile);
            Color prev = GUI.backgroundColor;
            if (isSelected) GUI.backgroundColor = VRHubStyles.AccentColor;

            if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Height(24)))
            {
                _targetProfile = profile;
                RunAudit();
            }

            GUI.backgroundColor = prev;
        }

        private void DrawHealthScoreBanner(bool isNarrow)
        {
            BeginCard();

            int criticalCount = _issues.Count(i => i.Severity == VROptimizationSeverity.Critical);
            int warningCount = _issues.Count(i => i.Severity == VROptimizationSeverity.Warning);
            int optCount = _issues.Count(i => i.Severity == VROptimizationSeverity.Optimization);
            int totalIssues = _issues.Count;
            int fixableCount = _issues.Count(i => i.CanAutoFix);

            // Compute score (100 is perfect)
            int score = Mathf.Clamp(100 - (criticalCount * 20) - (warningCount * 8) - (optCount * 3), 0, 100);
            
            Color scoreColor = score >= 85 ? VRHubStyles.SuccessColor : (score >= 60 ? VRHubStyles.WarningColor : VRHubStyles.ErrorColor);
            string scoreHex = ColorUtility.ToHtmlStringRGB(scoreColor);

            if (isNarrow)
            {
                EditorGUILayout.LabelField("VR Performance Score", EditorStyles.boldLabel);
                
                string scoreText = $"Score: <color=#{scoreHex}><b>{score}/100</b></color> | ";
                if (criticalCount > 0) scoreText += $"<color=#e84c4c><b>{criticalCount} Critical</b></color>";
                else if (totalIssues > 0) scoreText += $"<color=#f2a626>{totalIssues} Issues</color>";
                else scoreText += "<color=#38b860>Optimal</color>";

                GUIStyle richLabel = new GUIStyle(EditorStyles.miniLabel) { richText = true, wordWrap = true };
                EditorGUILayout.LabelField(scoreText, richLabel);

                GUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(" Run Audit", EditorGUIUtility.IconContent("d_Refresh").image), GUILayout.Height(26), GUILayout.ExpandWidth(true)))
                {
                    RunAudit();
                }

                if (fixableCount > 0)
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent($" Auto-Fix ({fixableCount})", EditorGUIUtility.IconContent("d_Checkmark").image), GUILayout.Height(26), GUILayout.ExpandWidth(true)))
                    {
                        FixAllAutoFixable();
                    }
                    GUI.backgroundColor = prev;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("VR Performance Health & Frame Budget", EditorStyles.boldLabel);
                
                string scoreText = $"Optimization Score: <color=#{scoreHex}><b><size=14>{score}/100</size></b></color>  |  " +
                                   $"<color=#e84c4c><b>{criticalCount} Critical</b></color>  |  " +
                                   $"<color=#f2a626>{warningCount} Warnings</color>  |  " +
                                   $"{optCount} Optimizations";

                GUIStyle richLabel = new GUIStyle(EditorStyles.miniLabel) { richText = true, wordWrap = true };
                EditorGUILayout.LabelField(scoreText, richLabel);
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(" Rescan", EditorGUIUtility.IconContent("d_Refresh").image), GUILayout.Height(28), GUILayout.Width(85)))
                {
                    RunAudit();
                }

                if (fixableCount > 0)
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent($" Optimize All Fixable ({fixableCount})", EditorGUIUtility.IconContent("d_Checkmark").image), GUILayout.Height(28), GUILayout.Width(190)))
                    {
                        FixAllAutoFixable();
                    }
                    GUI.backgroundColor = prev;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndHorizontal();
            }

            EndCard();
            GUILayout.Space(6);
        }

        private void DrawFilters(bool isNarrow)
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
                GUILayout.Label("Category:", EditorStyles.miniLabel, GUILayout.Width(55));
            }

            _selectedCategory = (VROptimizationCategory)EditorGUILayout.EnumPopup(_selectedCategory, EditorStyles.toolbarPopup, GUILayout.Width(isNarrow ? 120 : 160));

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        private void DrawIssueList(bool isNarrow)
        {
            var filtered = _issues.Where(i =>
                (_selectedCategory == VROptimizationCategory.All || i.Category == _selectedCategory) &&
                (string.IsNullOrEmpty(_searchFilter) ||
                 i.Title.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 i.Description.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 (!string.IsNullOrEmpty(i.AssetPath) && i.AssetPath.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0))
            ).ToList();

            if (filtered.Count == 0)
            {
                BeginCard();
                EditorGUILayout.LabelField("No Optimization Issues Found!", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox($"All scanned assets and scene parameters comply with VR performance guidelines for {_targetProfile}.", MessageType.Info);
                EndCard();
                return;
            }

            foreach (var item in filtered)
            {
                DrawIssueCard(item, isNarrow);
            }
        }

        private void DrawIssueCard(VROptimizationIssue issue, bool isNarrow)
        {
            BeginCard();

            if (isNarrow)
            {
                // Compact Stacked View
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(issue.Title, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                DrawSeverityBadge(issue.Severity);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"[{issue.Category}]", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(issue.Description, VRHubStyles.SubtitleStyle);
                
                if (!string.IsNullOrEmpty(issue.Impact))
                {
                    EditorGUILayout.LabelField($"Impact: <color=#f2a626>{issue.Impact}</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
                }

                GUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                if (issue.TargetObject != null || !string.IsNullOrEmpty(issue.AssetPath))
                {
                    if (GUILayout.Button("Select Asset", EditorStyles.miniButton, GUILayout.Height(22), GUILayout.ExpandWidth(!issue.CanAutoFix)))
                    {
                        issue.PingTarget();
                    }
                }

                if (issue.CanAutoFix)
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent(" Auto Fix", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(22), GUILayout.ExpandWidth(true)))
                    {
                        if (issue.ExecuteFix())
                        {
                            RunAudit();
                        }
                    }
                    GUI.backgroundColor = prev;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // Wide Row View
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(issue.Title, EditorStyles.boldLabel, GUILayout.MinWidth(200), GUILayout.MaxWidth(340));
                DrawSeverityBadge(issue.Severity);
                EditorGUILayout.LabelField($"[{issue.Category}]", EditorStyles.miniLabel, GUILayout.Width(130));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(issue.Description, VRHubStyles.SubtitleStyle);
                
                if (!string.IsNullOrEmpty(issue.Impact))
                {
                    EditorGUILayout.LabelField($"Impact: <color=#f2a626>{issue.Impact}</color>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
                }

                EditorGUILayout.EndVertical();

                GUILayout.Space(8);

                EditorGUILayout.BeginVertical(GUILayout.Width(130));
                
                if (issue.CanAutoFix)
                {
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent(" Auto Fix", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(22)))
                    {
                        if (issue.ExecuteFix())
                        {
                            RunAudit();
                        }
                    }
                    GUI.backgroundColor = prev;
                }

                if (issue.TargetObject != null || !string.IsNullOrEmpty(issue.AssetPath))
                {
                    if (GUILayout.Button("Select Asset", EditorStyles.miniButton, GUILayout.Height(20)))
                    {
                        issue.PingTarget();
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            EndCard();
        }

        private void DrawSeverityBadge(VROptimizationSeverity severity)
        {
            switch (severity)
            {
                case VROptimizationSeverity.Critical:
                    VRHubStyles.DrawBadge("CRITICAL", new Color(0.85f, 0.25f, 0.25f, 0.35f), VRHubStyles.ErrorColor, 68f);
                    break;
                case VROptimizationSeverity.Warning:
                    VRHubStyles.DrawBadge("WARNING", new Color(0.9f, 0.65f, 0.15f, 0.35f), VRHubStyles.WarningColor, 74f);
                    break;
                case VROptimizationSeverity.Optimization:
                    VRHubStyles.DrawBadge("OPTIMIZE", new Color(0.24f, 0.54f, 0.96f, 0.25f), VRHubStyles.AccentColor, 76f);
                    break;
            }
        }
    }
}
