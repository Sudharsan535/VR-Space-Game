using UnityEditor;
using UnityEngine;
using VRCore.Editor.Core;

namespace VRCore.Editor.Modules.SceneSetup
{
    /// <summary>
    /// Extension module for central VR Hub to configure VR scenes, XR Rigs, and Locomotion.
    /// Demonstrates the zero-configuration extensibility of the IVRHubModule architecture.
    /// </summary>
    public class VRSceneSetupModule : VRHubModuleBase
    {
        public override string ModuleId => "vr_scene_setup";
        public override string DisplayName => "Scene & Rig Setup";
        public override string Description => "Initialize VR Rigs, XR Interaction managers, and locomotion in your active scene.";
        public override string IconName => "d_SceneViewTools";
        public override int Priority => 20;

        protected override void DrawContent()
        {
            BeginCard();
            EditorGUILayout.LabelField("XR Scene Initialization (Modular Ready)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This module will allow one-click creation of XR Origin Rigs, Teleportation Areas, Controller Interactors, and Hands Tracking Rigs once XR packages are installed.",
                MessageType.Info);
            
            GUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Basic XR Scene Setup (Coming Soon)", GUILayout.Height(30)))
            {
                EditorUtility.DisplayDialog("VR Scene Setup", "XR packages installation module is active. Once packages are installed, scene setup actions will automate complete XR Rig hierarchy creation.", "Got it");
            }
            EditorGUILayout.EndHorizontal();

            EndCard();
        }
    }
}
