using System.Collections.Generic;

namespace VRCore.Editor.Modules.PackageManager
{
    /// <summary>
    /// Database of VR packages, dependencies, and vendor SDK presets (including Meta XR).
    /// </summary>
    public static class VRPackageDatabase
    {
        public static List<VRPackageDefinition> GetDefaultPackages()
        {
            return new List<VRPackageDefinition>
            {
                // Core XR Essentials
                new VRPackageDefinition(
                    "com.unity.xr.management",
                    "XR Plugin Management",
                    "Manages XR display and input plugins across standalone, PC, mobile, and head-mounted targets.",
                    VRPackageCategory.CoreXR,
                    VRPackageRequirementLevel.Required,
                    documentationUrl: "https://docs.unity3d.com/Packages/com.unity.xr.management@latest"
                ),
                new VRPackageDefinition(
                    "com.unity.xr.openxr",
                    "OpenXR Plugin",
                    "Industry-standard OpenXR runtime plugin for cross-platform VR headsets (Meta Quest, HTC Vive, SteamVR, Pico, Windows Mixed Reality).",
                    VRPackageCategory.CoreXR,
                    VRPackageRequirementLevel.Required,
                    documentationUrl: "https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest"
                ),
                new VRPackageDefinition(
                    "com.unity.xr.interaction.toolkit",
                    "XR Interaction Toolkit (XRI)",
                    "Comprehensive high-level component framework for VR/AR interactions: teleportation, direct grab, ray interactors, UI canvas interaction, and locomotion.",
                    VRPackageCategory.InteractionAndHands,
                    VRPackageRequirementLevel.Recommended,
                    documentationUrl: "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest"
                ),
                new VRPackageDefinition(
                    "com.unity.xr.hands",
                    "XR Hands",
                    "Official Unity package providing access to hand tracking subsystem data, hand mesh visualization, and gestures across supported XR devices.",
                    VRPackageCategory.InteractionAndHands,
                    VRPackageRequirementLevel.Recommended,
                    documentationUrl: "https://docs.unity3d.com/Packages/com.unity.xr.hands@latest"
                ),

                // Meta XR Ecosystem SDKs
                new VRPackageDefinition(
                    "com.meta.xr.sdk.all",
                    "Meta XR All-in-One SDK",
                    "Complete Meta Quest XR package suite bundling Core, Interaction, Audio, and Voice features for Meta Quest 2/3/Pro devices.",
                    VRPackageCategory.MetaXR,
                    VRPackageRequirementLevel.Optional,
                    documentationUrl: "https://developer.oculus.com/documentation/unity/unity-package-manager/"
                ),
                new VRPackageDefinition(
                    "com.meta.xr.sdk.core",
                    "Meta XR Core SDK",
                    "Essential Meta runtime features: Passthrough, Mixed Reality Scene understanding, Spatial Anchors, Guardian/Boundary, and Meta Display Refresh Rate controls.",
                    VRPackageCategory.MetaXR,
                    VRPackageRequirementLevel.Optional,
                    documentationUrl: "https://developer.oculus.com/documentation/unity/unity-isdk-overview/"
                ),
                new VRPackageDefinition(
                    "com.meta.xr.sdk.interaction",
                    "Meta XR Interaction SDK",
                    "Meta's advanced interaction components: comprehensive hand tracking poses, pinch grab, distance grab, raycasters, and palm UI menus.",
                    VRPackageCategory.MetaXR,
                    VRPackageRequirementLevel.Optional,
                    documentationUrl: "https://developer.oculus.com/documentation/unity/unity-isdk-overview/"
                ),
                new VRPackageDefinition(
                    "com.meta.xr.simulator",
                    "Meta XR Simulator",
                    "Simulates Meta Quest headset, hand tracking, passthrough, and controllers directly inside the Unity Editor without wearing a physical headset.",
                    VRPackageCategory.MetaXR,
                    VRPackageRequirementLevel.Optional,
                    documentationUrl: "https://developer.oculus.com/documentation/unity/meta-xr-simulator/"
                ),
                new VRPackageDefinition(
                    "com.meta.xr.sdk.audio",
                    "Meta XR Audio SDK",
                    "High-fidelity 3D spatialized HRTF audio for VR and Mixed Reality environments.",
                    VRPackageCategory.MetaXR,
                    VRPackageRequirementLevel.Optional,
                    documentationUrl: "https://developer.oculus.com/documentation/unity/unity-audio-overview/"
                ),
                new VRPackageDefinition(
                    "com.meta.xr.sdk.voice",
                    "Meta XR Voice SDK",
                    "Voice commands, speech-to-text, and natural language understanding for hands-free VR interactions powered by Wit.ai.",
                    VRPackageCategory.MetaXR,
                    VRPackageRequirementLevel.Optional,
                    documentationUrl: "https://developer.oculus.com/documentation/unity/voice-sdk-overview/"
                ),

                // Rendering & Input
                new VRPackageDefinition(
                    "com.unity.inputsystem",
                    "Unity Input System",
                    "Modern event-driven input architecture required for XR action-based controller mapping and tracking.",
                    VRPackageCategory.RenderingAndInput,
                    VRPackageRequirementLevel.Required,
                    documentationUrl: "https://docs.unity3d.com/Packages/com.unity.inputsystem@latest"
                ),
                new VRPackageDefinition(
                    "com.unity.render-pipelines.universal",
                    "Universal Render Pipeline (URP)",
                    "High-performance, optimized single-pass instanced rendering pipeline tailored for mobile XR and standalone headsets.",
                    VRPackageCategory.RenderingAndInput,
                    VRPackageRequirementLevel.Recommended,
                    documentationUrl: "https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest"
                ),

                // Utilities
                new VRPackageDefinition(
                    "com.unity.ai.navigation",
                    "Unity AI Navigation",
                    "Runtime NavMesh generation and agent navigation for VR NPCs, enemies, or guided pathfinding.",
                    VRPackageCategory.Utilities,
                    VRPackageRequirementLevel.Optional,
                    documentationUrl: "https://docs.unity3d.com/Packages/com.unity.ai.navigation@latest"
                )
            };
        }
    }
}
