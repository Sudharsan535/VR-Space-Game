using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRCore.Editor.Core;
using VRCore.Editor.Modules.SceneSetup.Generators;
using VRCore.Editor.Modules.SceneSetup.Spawners;

namespace VRCore.Editor.Modules.SceneSetup
{
    public enum VRSceneBuilderMode
    {
        GameplaySpawner,  // Live development palette to spawn interactive components into active scene
        FullSceneBuilder  // Complete scene generation and file creation
    }

    /// <summary>
    /// Dual-Stack (XRI & Meta XR) Scene & Interaction Gameplay Builder for VR Central Hub.
    /// Provides an active authoring palette for spawning gameplay mechanics and assembling VR scenes.
    /// </summary>
    public class VRSceneGeneratorModule : VRHubModuleBase
    {
        public override string ModuleId => "vr_scene_generator";
        public override string DisplayName => "Scene & Gameplay Builder";
        public override string Description => "Dual-Stack (XRI & Meta XR) scene generator and live gameplay interaction spawner.";
        public override string IconName => "d_SceneAsset Icon";
        public override int Priority => 20;

        private const float NarrowThreshold = 480f;

        private VRFrameworkType _targetFramework = VRFrameworkType.XRInteractionToolkit;
        private VRSceneBuilderMode _builderMode = VRSceneBuilderMode.GameplaySpawner;

        private List<VRSceneTemplateDefinition> _templates;
        private int _selectedTemplateIndex = 0;
        private string _sceneName = "VR_Playground";
        private string _targetFolder = "Assets/_Project/Scenes";
        private bool _includeRig = true;
        private bool _includeEnvironment = true;

        public override void OnEnable()
        {
            _templates = VRSceneTemplateDefinition.GetTemplates();
            if (Directory.Exists(Path.Combine(Application.dataPath, "_Project/Scenes")))
            {
                _targetFolder = "Assets/_Project/Scenes";
            }
            else
            {
                _targetFolder = "Assets/Scenes";
            }
            UpdateSceneNameFromTemplate();
        }

        private void UpdateSceneNameFromTemplate()
        {
            if (_templates != null && _selectedTemplateIndex >= 0 && _selectedTemplateIndex < _templates.Count)
            {
                _sceneName = _templates[_selectedTemplateIndex].DefaultSceneName;
            }
        }

        protected override void DrawContent()
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            bool isNarrow = viewWidth < NarrowThreshold;

            DrawFrameworkSelector(isNarrow);
            DrawBuilderModeSelector(isNarrow);

            if (_builderMode == VRSceneBuilderMode.GameplaySpawner)
            {
                DrawGameplaySpawnerView(isNarrow);
            }
            else
            {
                DrawFullSceneBuilderView(isNarrow);
            }
        }

        private void DrawFrameworkSelector(bool isNarrow)
        {
            BeginCard();
            EditorGUILayout.LabelField("Target XR Architecture Stack", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            
            Color prev = GUI.backgroundColor;
            if (_targetFramework == VRFrameworkType.XRInteractionToolkit) GUI.backgroundColor = VRHubStyles.AccentColor;
            if (GUILayout.Button("XR Interaction Toolkit (OpenXR Cross-Platform)", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                _targetFramework = VRFrameworkType.XRInteractionToolkit;
            }
            GUI.backgroundColor = prev;

            if (_targetFramework == VRFrameworkType.MetaXRSDK) GUI.backgroundColor = VRHubStyles.AccentColor;
            if (GUILayout.Button("Meta XR SDK (Quest Dedicated / OVR)", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                _targetFramework = VRFrameworkType.MetaXRSDK;
            }
            GUI.backgroundColor = prev;

            EditorGUILayout.EndHorizontal();
            EndCard();
            GUILayout.Space(6);
        }

        private void DrawBuilderModeSelector(bool isNarrow)
        {
            EditorGUILayout.BeginHorizontal();
            
            Color prev = GUI.backgroundColor;
            if (_builderMode == VRSceneBuilderMode.GameplaySpawner) GUI.backgroundColor = VRHubStyles.SuccessColor;
            if (GUILayout.Button("Live Gameplay Spawner (Active Scene)", EditorStyles.miniButton, GUILayout.Height(26)))
            {
                _builderMode = VRSceneBuilderMode.GameplaySpawner;
            }
            GUI.backgroundColor = prev;

            if (_builderMode == VRSceneBuilderMode.FullSceneBuilder) GUI.backgroundColor = VRHubStyles.SuccessColor;
            if (GUILayout.Button("Full Scene Asset Generator", EditorStyles.miniButton, GUILayout.Height(26)))
            {
                _builderMode = VRSceneBuilderMode.FullSceneBuilder;
            }
            GUI.backgroundColor = prev;

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);
        }

        private void DrawGameplaySpawnerView(bool isNarrow)
        {
            EditorGUILayout.HelpBox(
                "Click any component to instantly spawn and wire it into your currently open scene at your Scene View camera origin.",
                MessageType.Info);

            GUILayout.Space(4);

            // 1. XR Rigs
            BeginCard();
            EditorGUILayout.LabelField("Player & XR Rigs", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(" Spawn XRI Action Rig (Direct + Ray + Locomotion)", EditorGUIUtility.IconContent("d_CustomTool").image), GUILayout.Height(28)))
            {
                VRRigSpawner.SpawnXRIRig(Vector3.zero);
            }
            if (GUILayout.Button(new GUIContent(" Spawn Meta OVRCameraRig (Hands + Passthrough)", EditorGUIUtility.IconContent("d_CustomTool").image), GUILayout.Height(28)))
            {
                VRRigSpawner.SpawnMetaXRRig(Vector3.zero);
            }
            EditorGUILayout.EndHorizontal();
            EndCard();
            GUILayout.Space(4);

            // 2. Grabbable Objects & Tools
            BeginCard();
            EditorGUILayout.LabelField("Physical Grabbables & Tools", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Grabbable Physics Cube", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                VRInteractivePropSpawner.SpawnGrabbableProp("Grabbable Cube", PrimitiveType.Cube, GetSpawnPosition(0.8f), new Color(0.2f, 0.6f, 0.95f), _targetFramework);
            }
            if (GUILayout.Button("Grabbable Sphere", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                VRInteractivePropSpawner.SpawnGrabbableProp("Grabbable Sphere", PrimitiveType.Sphere, GetSpawnPosition(0.8f), new Color(0.95f, 0.5f, 0.15f), _targetFramework);
            }
            if (GUILayout.Button("Grabbable Tool with AttachPoint", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                VRInteractivePropSpawner.SpawnGrabbableProp("Grabbable Tool (AttachPoint)", PrimitiveType.Cylinder, GetSpawnPosition(0.8f), new Color(0.25f, 0.8f, 0.4f), _targetFramework);
            }
            EditorGUILayout.EndHorizontal();
            EndCard();
            GUILayout.Space(4);

            // 3. Mechanical Systems (Buttons, Levers, Doors, Drawers)
            BeginCard();
            EditorGUILayout.LabelField("Mechanical Controls & Physics Systems", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("VR Push Button (Spring)", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                VRMechanicalObjectSpawner.SpawnPushButton(GetSpawnPosition(0.9f));
            }
            if (GUILayout.Button("Mechanical Lever (Hinge)", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                VRMechanicalObjectSpawner.SpawnMechanicalLever(GetSpawnPosition(0.9f));
            }
            if (GUILayout.Button("Physics Hinged Door", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                VRMechanicalObjectSpawner.SpawnPhysicsDoor(GetSpawnPosition(0f));
            }
            if (GUILayout.Button("Sliding Cabinet Drawer", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                VRMechanicalObjectSpawner.SpawnSlidingDrawer(GetSpawnPosition(0.6f));
            }
            EditorGUILayout.EndHorizontal();
            EndCard();
            GUILayout.Space(4);

            // 4. Sockets & Inventory
            BeginCard();
            EditorGUILayout.LabelField("Snap Sockets & Body Inventory", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Snap Socket Receptacle", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                VRSocketHolsterSpawner.SpawnSnapSocket(GetSpawnPosition(0.9f), _targetFramework);
            }
            if (GUILayout.Button("Body Inventory Holsters (Hips + Shoulders)", EditorStyles.miniButton, GUILayout.Height(24)))
            {
                VRSocketHolsterSpawner.SpawnBodyInventoryHolsters(null);
            }
            EditorGUILayout.EndHorizontal();
            EndCard();
            GUILayout.Space(4);

            // 5. World-Space UI
            BeginCard();
            EditorGUILayout.LabelField("Diegetic World-Space VR UI", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(" Spawn World-Space VR Menu Canvas", EditorGUIUtility.IconContent("d_SceneAsset Icon").image), GUILayout.Height(28)))
            {
                VRWorldUISpawner.SpawnWorldSpaceVRUI(GetSpawnPosition(1.3f) + new Vector3(0, 0, 1.2f));
            }
            EditorGUILayout.EndHorizontal();
            EndCard();
        }

        private void DrawFullSceneBuilderView(bool isNarrow)
        {
            // Template Selector
            BeginCard();
            EditorGUILayout.LabelField("Select Scene Archetype", EditorStyles.boldLabel);

            if (isNarrow)
            {
                string[] templateNames = _templates.Select(t => t.Title).ToArray();
                int newIndex = EditorGUILayout.Popup(_selectedTemplateIndex, templateNames, EditorStyles.popup);
                if (newIndex != _selectedTemplateIndex)
                {
                    _selectedTemplateIndex = newIndex;
                    UpdateSceneNameFromTemplate();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < _templates.Count; i++)
                {
                    var t = _templates[i];
                    bool isSelected = (i == _selectedTemplateIndex);
                    Color prev = GUI.backgroundColor;
                    if (isSelected) GUI.backgroundColor = VRHubStyles.AccentColor;

                    if (GUILayout.Button(t.Title, EditorStyles.miniButton, GUILayout.Height(24)))
                    {
                        _selectedTemplateIndex = i;
                        UpdateSceneNameFromTemplate();
                    }
                    GUI.backgroundColor = prev;
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(4);
            var activeTemplate = _templates[_selectedTemplateIndex];
            EditorGUILayout.LabelField(activeTemplate.Title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(activeTemplate.Description, VRHubStyles.SubtitleStyle);
            EndCard();
            GUILayout.Space(6);

            // Scene Config
            BeginCard();
            EditorGUILayout.LabelField("Scene File Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scene Name:", GUILayout.Width(isNarrow ? 100 : 120));
            _sceneName = EditorGUILayout.TextField(_sceneName, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(".unity", GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Save Directory:", GUILayout.Width(isNarrow ? 100 : 120));
            _targetFolder = EditorGUILayout.TextField(_targetFolder, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            _includeRig = EditorGUILayout.ToggleLeft($"Include {_targetFramework} Rig", _includeRig, GUILayout.Width(isNarrow ? 210 : 240));
            _includeEnvironment = EditorGUILayout.ToggleLeft("Include Environment & Lighting", _includeEnvironment);
            EditorGUILayout.EndHorizontal();

            EndCard();
            GUILayout.Space(6);

            // Action Button
            BeginCard();
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = VRHubStyles.SuccessColor;

            if (GUILayout.Button(new GUIContent($" Generate & Open '{_sceneName}' ({_targetFramework})", EditorGUIUtility.IconContent("d_SceneAsset Icon").image), GUILayout.Height(32), GUILayout.ExpandWidth(true)))
            {
                bool success = VRSceneBuilder.CreateAndSaveNewScene(
                    activeTemplate.Type,
                    _targetFramework,
                    _sceneName,
                    _targetFolder,
                    _includeRig,
                    _includeEnvironment);

                if (success)
                {
                    EditorUtility.DisplayDialog("VR Scene Created", $"Successfully generated and opened '{_sceneName}.unity'.", "Awesome!");
                }
            }
            GUI.backgroundColor = prevBg;
            EndCard();
        }

        private Vector3 GetSpawnPosition(float defaultHeight)
        {
            // Try spawn in front of SceneView camera if available, or default
            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
            {
                var camTransform = SceneView.lastActiveSceneView.camera.transform;
                Vector3 pos = camTransform.position + camTransform.forward * 2.0f;
                return new Vector3(pos.x, Mathf.Max(0.1f, pos.y), pos.z);
            }
            return new Vector3(0, defaultHeight, 1.2f);
        }
    }
}
