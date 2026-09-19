using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Core
{
    /// <summary>
    /// Central dockable Editor Window for VR development, providing access to
    /// package management, XR setup, diagnostics, and developer tools.
    /// Fully responsive for both docked and wide layouts.
    /// </summary>
    public class VRHubWindow : EditorWindow
    {
        private const float NarrowBreakpoint = 520f;
        private const float SidebarWidth = 180f;

        private IReadOnlyList<IVRHubModule> _modules;
        private int _selectedModuleIndex = 0;
        private IVRHubModule _activeModule;
        private Vector2 _sidebarScroll;
        private Vector2 _windowScroll;

        // Dedicated Top-Level Menu (Not under Window)
        [MenuItem("VR Core/VR Central Hub", false, 1)]
        public static void OpenWindow()
        {
            var window = GetWindow<VRHubWindow>("VR Central Hub", true, typeof(EditorWindow));
            window.minSize = new Vector2(280f, 220f);
            window.Show();
        }

        private void OnEnable()
        {
            Texture icon = EditorGUIUtility.IconContent("d_Settings")?.image ?? EditorGUIUtility.IconContent("Settings")?.image;
            titleContent = new GUIContent("VR Central Hub", icon);
            minSize = new Vector2(280f, 220f);
            RefreshModules();
        }

        private void OnDisable()
        {
            if (_activeModule != null)
            {
                _activeModule.OnDisable();
                _activeModule = null;
            }
        }

        private void Update()
        {
            if (_activeModule != null)
            {
                _activeModule.OnUpdate();
            }
        }

        private void RefreshModules()
        {
            VRHubRegistry.ReloadModules();
            _modules = VRHubRegistry.GetModules();

            if (_modules.Count > 0)
            {
                if (_selectedModuleIndex < 0 || _selectedModuleIndex >= _modules.Count)
                {
                    _selectedModuleIndex = 0;
                }
                SetActiveModule(_modules[_selectedModuleIndex]);
            }
            else
            {
                _activeModule = null;
            }
        }

        private void SetActiveModule(IVRHubModule module)
        {
            if (_activeModule == module) return;

            if (_activeModule != null)
            {
                _activeModule.OnDisable();
            }

            _activeModule = module;

            if (_activeModule != null)
            {
                _activeModule.OnEnable();
            }
        }

        private void OnGUI()
        {
            bool isNarrow = position.width < NarrowBreakpoint;

            DrawTopHeader(isNarrow);

            if (isNarrow)
            {
                // Compact Docked Mode: Top Navigation Tabs
                DrawCompactNavBar();
                VRHubStyles.DrawDivider(1f, 2f);

                // Full width content area
                _windowScroll = EditorGUILayout.BeginScrollView(_windowScroll);
                DrawActiveModuleContent();
                EditorGUILayout.EndScrollView();
            }
            else
            {
                // Wide Mode: Sidebar + Content
                EditorGUILayout.BeginHorizontal();
                DrawSidebar();

                // Vertical Separator
                Rect separatorRect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
                EditorGUI.DrawRect(separatorRect, VRHubStyles.CardBorder);

                DrawMainContentArea();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawTopHeader(bool isNarrow)
        {
            float headerHeight = isNarrow ? 36f : 42f;
            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, VRHubStyles.HeaderBg);

            GUILayout.BeginArea(headerRect);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(isNarrow ? 6 : 12);

            GUIContent logoIcon = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
            if (logoIcon != null && logoIcon.image != null)
            {
                GUILayout.Label(logoIcon, GUILayout.Width(22), GUILayout.Height(headerHeight - 6));
            }

            EditorGUILayout.BeginVertical();
            GUILayout.Space(isNarrow ? 2 : 4);
            EditorGUILayout.LabelField("VR CENTRAL HUB", EditorStyles.boldLabel);
            if (!isNarrow)
            {
                EditorGUILayout.LabelField("Modular XR Architecture & Development Suite", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUIContent refreshContent = isNarrow 
                ? new GUIContent(EditorGUIUtility.IconContent("d_Refresh").image, "Refresh Modules") 
                : new GUIContent(" Refresh", EditorGUIUtility.IconContent("d_Refresh").image);

            if (GUILayout.Button(refreshContent, EditorStyles.miniButton, GUILayout.Height(24), GUILayout.Width(isNarrow ? 32 : 80)))
            {
                RefreshModules();
            }

            GUILayout.Space(isNarrow ? 6 : 12);
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();

            VRHubStyles.DrawDivider(1f, 0f);
        }

        private void DrawCompactNavBar()
        {
            if (_modules == null || _modules.Count == 0) return;

            string[] moduleNames = new string[_modules.Count];
            for (int i = 0; i < _modules.Count; i++)
            {
                moduleNames[i] = _modules[i].DisplayName;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(4);

            int newIndex = EditorGUILayout.Popup(_selectedModuleIndex, moduleNames, EditorStyles.toolbarPopup);
            if (newIndex != _selectedModuleIndex && newIndex >= 0 && newIndex < _modules.Count)
            {
                _selectedModuleIndex = newIndex;
                SetActiveModule(_modules[_selectedModuleIndex]);
            }

            GUILayout.Space(4);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(SidebarWidth), GUILayout.ExpandHeight(true));
            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);
            GUILayout.Space(6);

            if (_modules == null || _modules.Count == 0)
            {
                EditorGUILayout.HelpBox("No modules found.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < _modules.Count; i++)
                {
                    var module = _modules[i];
                    bool isSelected = (i == _selectedModuleIndex);

                    Color prevColor = GUI.backgroundColor;
                    if (isSelected)
                    {
                        GUI.backgroundColor = VRHubStyles.AccentColor;
                    }

                    GUIContent icon = null;
                    if (!string.IsNullOrEmpty(module.IconName))
                    {
                        try { icon = EditorGUIUtility.IconContent(module.IconName); } catch { }
                    }
                    if (icon == null || icon.image == null)
                    {
                        icon = EditorGUIUtility.IconContent("d_CustomTool") ?? EditorGUIUtility.IconContent("CustomTool");
                    }

                    GUIContent buttonContent = new GUIContent($"  {module.DisplayName}", icon?.image);
                    if (GUILayout.Button(buttonContent, VRHubStyles.SidebarButtonStyle))
                    {
                        _selectedModuleIndex = i;
                        SetActiveModule(module);
                    }

                    GUI.backgroundColor = prevColor;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawMainContentArea()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(8);

            EditorGUILayout.BeginVertical();
            DrawActiveModuleContent();
            EditorGUILayout.EndVertical();

            GUILayout.Space(8);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);
            EditorGUILayout.EndVertical();
        }

        private void DrawActiveModuleContent()
        {
            if (_activeModule != null)
            {
                _activeModule.DrawGUI();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a module from the menu above.", MessageType.Info);
            }
        }
    }
}
