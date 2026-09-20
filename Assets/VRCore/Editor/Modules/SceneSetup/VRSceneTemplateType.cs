namespace VRCore.Editor.Modules.SceneSetup
{
    public enum VRSceneTemplateType
    {
        InteractionPlayground,  // VR room with table, grabbable props, physics toys
        LocomotionLab,          // Ramps, stairs, platforms, continuous move & teleport zones
        MixedRealitySandbox,    // Passthrough clear flags, MR virtual tabletop
        PhysicsLab,             // Weight testing, snapping targets, mass variations
        CleanVRTemplate         // Minimalist scene with XR Rig and calibrated lighting only
    }
}
