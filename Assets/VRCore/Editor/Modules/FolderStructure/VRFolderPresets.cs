using System.Collections.Generic;

namespace VRCore.Editor.Modules.FolderStructure
{
    public static class VRFolderPresets
    {
        public static List<VRFolderItem> GetStandardVRPreset()
        {
            return new List<VRFolderItem>
            {
                // Scenes & Levels
                new VRFolderItem("Scenes/Boot", "Initial startup, splash, and bootstrap scene", "Scenes"),
                new VRFolderItem("Scenes/Main", "Primary gameplay levels and core VR environment", "Scenes"),
                new VRFolderItem("Scenes/Sandbox", "Developer testing, mechanics prototyping, and sandbox", "Scenes"),

                // Code & Logic
                new VRFolderItem("Scripts/Core", "Core game loops, architecture, and singletons", "Code & Scripts"),
                new VRFolderItem("Scripts/Interactions", "Grabbable objects, levers, buttons, and custom XR interactors", "Code & Scripts"),
                new VRFolderItem("Scripts/Player", "VR Rig locomotion, head/hand controllers, and player stats", "Code & Scripts"),
                new VRFolderItem("Scripts/Managers", "Audio, UI, game state, and save managers", "Code & Scripts"),
                new VRFolderItem("Scripts/UI", "World-space VR canvases and interactive menus", "Code & Scripts"),
                new VRFolderItem("Scripts/Utils", "Helper scripts, extensions, and math utilities", "Code & Scripts"),

                // Prefabs
                new VRFolderItem("Prefabs/Core", "XR Rig templates, game managers, and persistent objects", "Prefabs"),
                new VRFolderItem("Prefabs/Interactions", "Interactable objects, sockets, tools, and props", "Prefabs"),
                new VRFolderItem("Prefabs/Environment", "World geometry, room bounds, and level modular prefabs", "Prefabs"),
                new VRFolderItem("Prefabs/UI", "VR diegetic buttons, popup panels, and hud elements", "Prefabs"),

                // Art & Assets
                new VRFolderItem("Art/Animations", "Animation controllers, clips, and hand tracking poses", "Art & Assets"),
                new VRFolderItem("Art/Materials", "PBR materials, physics materials, and skyboxes", "Art & Assets"),
                new VRFolderItem("Art/Models", "3D FBX/OBJ meshes, environment props, and controllers", "Art & Assets"),
                new VRFolderItem("Art/Shaders", "Custom VR shaders, shader graphs, and VFX", "Art & Assets"),
                new VRFolderItem("Art/Textures", "Diffuse, normal, metallic, and roughness maps", "Art & Assets"),

                // Audio
                new VRFolderItem("Audio/Music", "Spatialized VR background tracks and ambient music", "Audio"),
                new VRFolderItem("Audio/SFX", "Haptic impact sounds, UI clicks, and interaction Foley", "Audio"),

                // Config & Docs
                new VRFolderItem("Settings", "Input action maps, XR presets, and physics presets", "Configuration"),
                new VRFolderItem("Documentation", "Design documents, developer setup guides, and changelogs", "Documentation")
            };
        }

        public static List<VRFolderItem> GetMetaQuestPreset()
        {
            return new List<VRFolderItem>
            {
                // Meta Quest Specific
                new VRFolderItem("MetaXR/Passthrough", "Passthrough layer setups and MR blend configurations", "Meta XR"),
                new VRFolderItem("MetaXR/HandTracking", "Hand mesh poses, pinch interactors, and gesture configs", "Meta XR"),
                new VRFolderItem("MetaXR/SpatialAnchors", "Persistent cloud and local spatial anchor prefabs", "Meta XR"),
                new VRFolderItem("MetaXR/Optimizations", "LODs, low-poly mobile meshes, and ASTC assets", "Meta XR"),

                // Core Project
                new VRFolderItem("Scenes/Quest_Main", "Main optimized standalone Quest level", "Scenes"),
                new VRFolderItem("Scenes/Quest_MR_Sandbox", "Mixed Reality passthrough test sandbox", "Scenes"),

                new VRFolderItem("Scripts/Gameplay", "VR gameplay systems and mechanics", "Code & Scripts"),
                new VRFolderItem("Scripts/Interaction", "Direct grab, ray grab, and palm UI menus", "Code & Scripts"),
                new VRFolderItem("Prefabs/QuestRigs", "OVR / BuildingBlocks / OpenXR Quest Rig prefabs", "Prefabs"),
                new VRFolderItem("Art/Materials", "Mobile-friendly lightweight URP materials", "Art & Assets"),
                new VRFolderItem("Audio", "Spatialized HRTF audio clips", "Audio"),
                new VRFolderItem("Settings", "Quest build settings and Android manifest overrides", "Configuration")
            };
        }

        public static List<VRFolderItem> GetMinimalPreset()
        {
            return new List<VRFolderItem>
            {
                new VRFolderItem("Scenes", "Game scenes and testing levels", "Core"),
                new VRFolderItem("Scripts", "C# scripts and components", "Core"),
                new VRFolderItem("Prefabs", "Reusable GameObject prefabs", "Core"),
                new VRFolderItem("Materials", "Project materials and textures", "Core"),
                new VRFolderItem("Audio", "Sound effects and music", "Core")
            };
        }
    }
}
