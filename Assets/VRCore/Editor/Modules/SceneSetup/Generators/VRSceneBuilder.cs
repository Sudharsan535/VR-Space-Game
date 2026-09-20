using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRCore.Editor.Modules.SceneSetup.Generators
{
    public static class VRSceneBuilder
    {
        public static void BuildInActiveScene(
            VRSceneTemplateType templateType,
            VRFrameworkType framework,
            bool includeRig,
            bool includeEnvironment)
        {
            Undo.SetCurrentGroupName("VR Scene Setup");
            int undoGroup = Undo.GetCurrentGroup();

            if (includeEnvironment)
            {
                VREnvironmentGenerator.BuildEnvironment(templateType);
            }

            if (includeRig)
            {
                Spawners.VRRigSpawner.SpawnRig(framework, Vector3.zero);
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        public static bool CreateAndSaveNewScene(
            VRSceneTemplateType templateType,
            VRFrameworkType framework,
            string sceneName,
            string targetFolderRelative,
            bool includeRig,
            bool includeEnvironment)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            // Ensure target folder exists
            string cleanFolder = string.IsNullOrWhiteSpace(targetFolderRelative) ? "Assets/Scenes" : targetFolderRelative.Trim();
            if (!cleanFolder.StartsWith("Assets")) cleanFolder = $"Assets/{cleanFolder.TrimStart('/')}";
            
            string absoluteFolder = Path.Combine(Application.dataPath, cleanFolder.Substring("Assets/".Length).TrimStart('/', '\\'));
            if (!Directory.Exists(absoluteFolder))
            {
                Directory.CreateDirectory(absoluteFolder);
                AssetDatabase.Refresh();
            }

            string cleanName = string.IsNullOrWhiteSpace(sceneName) ? "VR_New_Scene" : sceneName.Trim();
            string savePath = $"{cleanFolder}/{cleanName}.unity";

            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (includeEnvironment)
            {
                VREnvironmentGenerator.BuildEnvironment(templateType);
            }

            if (includeRig)
            {
                Spawners.VRRigSpawner.SpawnRig(framework, Vector3.zero);
            }

            bool saved = EditorSceneManager.SaveScene(newScene, savePath);
            if (saved)
            {
                AssetDatabase.Refresh();
                Debug.Log($"[VR Central Hub] Generated and saved new VR scene: '{savePath}'");
            }

            return saved;
        }
    }
}
