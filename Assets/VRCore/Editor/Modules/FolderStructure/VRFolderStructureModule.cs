using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRCore.Editor.Core;

namespace VRCore.Editor.Modules.FolderStructure
{
    /// <summary>
    /// VR Central Hub module for scaffolding standard VR project folder hierarchies,
    /// Git tracking configurations, and developer structure guides.
    /// </summary>
    public class VRFolderStructureModule : VRHubModuleBase
    {
        public override string ModuleId => "vr_folder_structure";
        public override string DisplayName => "Folder Scaffold";
        public override string Description => "Generate standard VR project folder structures, gitkeep files, and developer guides.";
        public override string IconName => "d_Folder Icon";
        public override int Priority => 12;

        private const float NarrowThreshold = 460f;

        private VRFolderPresetType _currentPreset = VRFolderPresetType.StandardVR;
        private List<VRFolderItem> _folderItems = new List<VRFolderItem>();
        
        private bool _useRootPrefix = true;
        private string _rootPrefixName = "_Project";
        private bool _createGitKeep = true;
        private bool _createReadme = true;

        private string _newCustomFolderPath = "";
        private string _newCustomFolderCategory = "Custom";

        public override void OnEnable()
        {
            ApplyPreset(_currentPreset);
        }

        private void ApplyPreset(VRFolderPresetType preset)
        {
            _currentPreset = preset;
            switch (preset)
            {
                case VRFolderPresetType.StandardVR:
                    _folderItems = VRFolderPresets.GetStandardVRPreset();
                    break;
                case VRFolderPresetType.MetaQuestXR:
                    _folderItems = VRFolderPresets.GetMetaQuestPreset();
                    break;
                case VRFolderPresetType.MinimalPrototype:
                    _folderItems = VRFolderPresets.GetMinimalPreset();
                    break;
                case VRFolderPresetType.Custom:
                    if (_folderItems == null || _folderItems.Count == 0)
                    {
                        _folderItems = VRFolderPresets.GetStandardVRPreset();
                    }
                    break;
            }
        }

        protected override void DrawContent()
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            bool isNarrow = viewWidth < NarrowThreshold;

            DrawPresetSelector(isNarrow);
            DrawConfigurationOptions(isNarrow);
            DrawActionBanner(isNarrow);
            DrawFolderList(isNarrow);
        }

        private void DrawPresetSelector(bool isNarrow)
        {
            BeginCard();
            EditorGUILayout.LabelField("Architecture Presets", EditorStyles.boldLabel);

            if (isNarrow)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Preset:", GUILayout.Width(50));
                var newPreset = (VRFolderPresetType)EditorGUILayout.EnumPopup(_currentPreset, EditorStyles.popup);
                if (newPreset != _currentPreset)
                {
                    ApplyPreset(newPreset);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawPresetButton(VRFolderPresetType.StandardVR, "Standard VR Game");
                DrawPresetButton(VRFolderPresetType.MetaQuestXR, "Meta Quest XR");
                DrawPresetButton(VRFolderPresetType.MinimalPrototype, "Minimal Prototype");
                DrawPresetButton(VRFolderPresetType.Custom, "Custom");
                EditorGUILayout.EndHorizontal();
            }

            EndCard();
            GUILayout.Space(6);
        }

        private void DrawPresetButton(VRFolderPresetType preset, string label)
        {
            bool isSelected = (_currentPreset == preset);
            Color prev = GUI.backgroundColor;
            if (isSelected) GUI.backgroundColor = VRHubStyles.AccentColor;

            if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Height(24)))
            {
                ApplyPreset(preset);
            }

            GUI.backgroundColor = prev;
        }

        private void DrawConfigurationOptions(bool isNarrow)
        {
            BeginCard();
            EditorGUILayout.LabelField("Scaffold Configuration", EditorStyles.boldLabel);

            // Root Prefix
            EditorGUILayout.BeginHorizontal();
            _useRootPrefix = EditorGUILayout.ToggleLeft("Use Dedicated Root Folder Prefix", _useRootPrefix, GUILayout.Width(isNarrow ? 210 : 230));
            if (_useRootPrefix)
            {
                EditorGUILayout.LabelField("Assets /", GUILayout.Width(50));
                _rootPrefixName = EditorGUILayout.TextField(_rootPrefixName, GUILayout.MaxWidth(140));
            }
            EditorGUILayout.EndHorizontal();

            if (_useRootPrefix)
            {
                EditorGUILayout.HelpBox($"All project folders will be created inside 'Assets/{_rootPrefixName}/' to keep custom code cleanly separated from third-party plugins.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("Folders will be created directly under 'Assets/'.", MessageType.None);
            }

            GUILayout.Space(4);

            // Utilities toggles
            EditorGUILayout.BeginHorizontal();
            _createGitKeep = EditorGUILayout.ToggleLeft("Generate .gitkeep in empty folders", _createGitKeep);
            _createReadme = EditorGUILayout.ToggleLeft("Generate PROJECT_STRUCTURE.md guide", _createReadme);
            EditorGUILayout.EndHorizontal();

            EndCard();
            GUILayout.Space(6);
        }

        private void DrawActionBanner(bool isNarrow)
        {
            BeginCard();

            int totalSelected = _folderItems.Count(f => f.IsEnabled);
            int totalAvailable = _folderItems.Count;

            if (isNarrow)
            {
                EditorGUILayout.LabelField($"Selected Folders: <b>{totalSelected}/{totalAvailable}</b>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
                GUILayout.Space(4);

                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = VRHubStyles.SuccessColor;
                if (GUILayout.Button(new GUIContent(" Generate Folder Structure", EditorGUIUtility.IconContent("d_Folder Icon").image), GUILayout.Height(28), GUILayout.ExpandWidth(true)))
                {
                    ExecuteGeneration();
                }
                GUI.backgroundColor = prev;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Ready to Generate Structure", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Selected Folders: <b>{totalSelected}/{totalAvailable}</b> will be generated.", new GUIStyle(EditorStyles.miniLabel) { richText = true });
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = VRHubStyles.SuccessColor;
                if (GUILayout.Button(new GUIContent(" Generate Folder Structure", EditorGUIUtility.IconContent("d_Folder Icon").image), GUILayout.Height(28), GUILayout.Width(220)))
                {
                    ExecuteGeneration();
                }
                GUI.backgroundColor = prev;

                EditorGUILayout.EndHorizontal();
            }

            EndCard();
            GUILayout.Space(6);
        }

        private void ExecuteGeneration()
        {
            int selectedCount = _folderItems.Count(f => f.IsEnabled);
            if (selectedCount == 0)
            {
                EditorUtility.DisplayDialog("VR Folder Generator", "Please select at least one folder to generate.", "OK");
                return;
            }

            string root = _useRootPrefix ? _rootPrefixName : "";
            var result = VRFolderStructureGenerator.Generate(_folderItems, root, _createGitKeep, _createReadme);

            EditorUtility.DisplayDialog(
                "Folder Structure Generated",
                $"Successfully created {result.createdCount} new folders ({result.existedCount} already existed) inside '{result.rootPath}'.",
                "Great!");
        }

        private void DrawFolderList(bool isNarrow)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Included Folders Checklist", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select All", EditorStyles.toolbarButton, GUILayout.Width(65)))
            {
                foreach (var item in _folderItems) item.IsEnabled = true;
            }

            if (GUILayout.Button("Deselect All", EditorStyles.toolbarButton, GUILayout.Width(75)))
            {
                foreach (var item in _folderItems) item.IsEnabled = false;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);

            // Group by categories
            var grouped = _folderItems.GroupBy(f => f.Category).ToList();

            foreach (var group in grouped)
            {
                BeginCard();
                EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                VRHubStyles.DrawDivider(1f, 2f);

                foreach (var item in group)
                {
                    EditorGUILayout.BeginHorizontal();
                    item.IsEnabled = EditorGUILayout.Toggle(item.IsEnabled, GUILayout.Width(22));

                    string displayTarget = _useRootPrefix ? $"{_rootPrefixName}/{item.RelativePath}" : item.RelativePath;

                    if (isNarrow)
                    {
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField(displayTarget, EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(item.Description, VRHubStyles.SubtitleStyle);
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.LabelField(displayTarget, EditorStyles.boldLabel, GUILayout.MinWidth(180), GUILayout.MaxWidth(280));
                        EditorGUILayout.LabelField(item.Description, VRHubStyles.SubtitleStyle);
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }

                EndCard();
                GUILayout.Space(4);
            }

            // Custom Folder Addition Form
            DrawCustomFolderInput();
        }

        private void DrawCustomFolderInput()
        {
            BeginCard();
            EditorGUILayout.LabelField("Add Custom Folder", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Path:", GUILayout.Width(40));
            _newCustomFolderPath = EditorGUILayout.TextField(_newCustomFolderPath, GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField("Category:", GUILayout.Width(60));
            _newCustomFolderCategory = EditorGUILayout.TextField(_newCustomFolderCategory, GUILayout.Width(100));

            if (GUILayout.Button(new GUIContent(" Add Folder", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(20), GUILayout.Width(90)))
            {
                if (!string.IsNullOrWhiteSpace(_newCustomFolderPath))
                {
                    _folderItems.Add(new VRFolderItem(
                        _newCustomFolderPath.Trim(),
                        "Custom project directory",
                        string.IsNullOrWhiteSpace(_newCustomFolderCategory) ? "Custom" : _newCustomFolderCategory.Trim(),
                        true));
                    _newCustomFolderPath = "";
                }
            }
            EditorGUILayout.EndHorizontal();
            EndCard();
            GUILayout.Space(8);
        }
    }
}
