using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Core
{
    /// <summary>
    /// Abstract base class for VR Central Hub modules providing common GUI helper methods,
    /// scroll state management, and lifecycle handling.
    /// </summary>
    public abstract class VRHubModuleBase : IVRHubModule
    {
        public abstract string ModuleId { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        public virtual string IconName => "d_CustomTool";
        public virtual int Priority => 100;

        protected Vector2 ScrollPosition;

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnUpdate() { }

        public virtual void DrawGUI()
        {
            DrawHeader();
            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
            DrawContent();
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Renders the standardized module title and description header.
        /// </summary>
        protected virtual void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            
            GUIContent iconContent = null;
            if (!string.IsNullOrEmpty(IconName))
            {
                try
                {
                    iconContent = EditorGUIUtility.IconContent(IconName);
                }
                catch { }
            }

            if (iconContent == null || iconContent.image == null)
            {
                iconContent = EditorGUIUtility.IconContent("d_CustomTool") ?? EditorGUIUtility.IconContent("CustomTool");
            }

            if (iconContent != null && iconContent.image != null)
            {
                GUILayout.Label(iconContent, GUILayout.Width(32), GUILayout.Height(32));
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(DisplayName, VRHubStyles.TitleStyle);
            EditorGUILayout.LabelField(Description, VRHubStyles.SubtitleStyle);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(8);
        }

        /// <summary>
        /// Main content area implementation for the module.
        /// </summary>
        protected abstract void DrawContent();

        /// <summary>
        /// Helper to draw a modern card container.
        /// </summary>
        protected void BeginCard()
        {
            EditorGUILayout.BeginVertical(VRHubStyles.CardStyle);
        }

        /// <summary>
        /// Helper to close a card container.
        /// </summary>
        protected void EndCard()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
