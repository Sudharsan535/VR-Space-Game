using UnityEngine;

namespace VRCore.Editor.Core
{
    /// <summary>
    /// Contract for all pluggable modules in the VR Central Hub.
    /// To add a new tool or feature page to VR Central Hub, create a class implementing this interface.
    /// </summary>
    public interface IVRHubModule
    {
        /// <summary>
        /// Unique identifier for this module.
        /// </summary>
        string ModuleId { get; }

        /// <summary>
        /// Name displayed in the navigation sidebar.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Short description of what this module does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Built-in Unity icon name or custom icon identifier.
        /// </summary>
        string IconName { get; }

        /// <summary>
        /// Sort priority in the sidebar (lower numbers appear first).
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Called when the VR Central Hub window is opened or enabled.
        /// </summary>
        void OnEnable();

        /// <summary>
        /// Called when the VR Central Hub window is closed or disabled.
        /// </summary>
        void OnDisable();

        /// <summary>
        /// Called periodically from the editor window Update loop.
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// Renders the module's GUI in the main viewport.
        /// </summary>
        void DrawGUI();
    }
}
