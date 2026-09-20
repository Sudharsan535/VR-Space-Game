using System.Collections.Generic;

namespace VRCore.Editor.Modules.SceneSetup
{
    public class VRSceneTemplateDefinition
    {
        public VRSceneTemplateType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] KeyFeatures { get; set; }
        public string DefaultSceneName { get; set; }

        public VRSceneTemplateDefinition(
            VRSceneTemplateType type,
            string title,
            string description,
            string[] keyFeatures,
            string defaultSceneName)
        {
            Type = type;
            Title = title;
            Description = description;
            KeyFeatures = keyFeatures;
            DefaultSceneName = defaultSceneName;
        }

        public static List<VRSceneTemplateDefinition> GetTemplates()
        {
            return new List<VRSceneTemplateDefinition>
            {
                new VRSceneTemplateDefinition(
                    VRSceneTemplateType.InteractionPlayground,
                    "VR Interaction Playground",
                    "Complete interactive VR sandbox featuring a stylized grid floor, pedestals, and diverse physics props (cubes, spheres, cylinders, throw targets).",
                    new[] { "Full XR Origin Rig", "Tabletop with grabbable props", "Soft directional VR lighting", "Teleportation-ready floor" },
                    "VR_Playground"
                ),
                new VRSceneTemplateDefinition(
                    VRSceneTemplateType.LocomotionLab,
                    "Locomotion & Teleportation Lab",
                    "Locomotion testing course featuring inclined ramps, elevation stairs, elevated platforms, and obstacle corridors.",
                    new[] { "Elevation ramps & stairs", "High-low teleport pads", "Locomotion collision volumes", "Full XR Origin Rig" },
                    "VR_Locomotion_Lab"
                ),
                new VRSceneTemplateDefinition(
                    VRSceneTemplateType.MixedRealitySandbox,
                    "Mixed Reality / Passthrough Sandbox",
                    "MR-ready template with camera clear flags configured for video passthrough, tabletop anchors, and MR lighting.",
                    new[] { "Passthrough alpha camera clear", "Virtual MR tabletop", "Spatial plane placeholder", "Full XR Origin Rig" },
                    "VR_MR_Passthrough_Sandbox"
                ),
                new VRSceneTemplateDefinition(
                    VRSceneTemplateType.PhysicsLab,
                    "Physics & Socket Lab",
                    "Physics mechanics testing environment with mass blocks (1kg, 5kg, 10kg), target drop zones, and lever/button stations.",
                    new[] { "Calibrated mass physics props", "Target receptacles / drop zones", "Friction test ramps", "Full XR Origin Rig" },
                    "VR_Physics_Lab"
                ),
                new VRSceneTemplateDefinition(
                    VRSceneTemplateType.CleanVRTemplate,
                    "Clean VR Template",
                    "Minimalist, production-ready starting scene containing only the standardized XR Origin Rig and calibrated VR lighting.",
                    new[] { "Clean XR Origin Rig", "Calibrated single directional light", "Floor boundary reference", "Zero clutter" },
                    "VR_Clean_Template"
                )
            };
        }
    }
}
