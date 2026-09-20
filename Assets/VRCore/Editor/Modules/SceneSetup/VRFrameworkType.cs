namespace VRCore.Editor.Modules.SceneSetup
{
    public enum VRFrameworkType
    {
        XRInteractionToolkit, // Unity OpenXR & XR Interaction Toolkit (Cross-platform)
        MetaXRSDK             // Meta Quest Dedicated SDK (OVR / Interaction SDK / Passthrough)
    }

    public enum VRInteractionItemType
    {
        XROriginRig,
        GrabbablePhysicsProp,
        PhysicalPushButton,
        MechanicalLever,
        PhysicsDoor,
        SlidingDrawer,
        SnapSocket,
        BodyInventoryHolster,
        WorldSpaceVRUI,
        ClimbableSurface,
        TeleportPad
    }
}
