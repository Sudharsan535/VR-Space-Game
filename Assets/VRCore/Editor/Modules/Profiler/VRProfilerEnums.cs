namespace VRCore.Editor.Modules.Profiler
{
    public enum VRPerformanceProfile
    {
        MetaQuest2,    // 72 FPS / 13.8ms frame budget, strict vertex & mobile fillrate limits
        MetaQuest3,    // 90 FPS / 11.1ms frame budget, Snapdragon XR2 Gen 2 target
        PCVR           // 90-120 FPS / 8.3-11.1ms frame budget, High-fidelity desktop GPUs
    }

    public enum VROptimizationCategory
    {
        All,
        MeshAndGeometry,
        TexturesAndVRAM,
        AudioAndMemory,
        SceneAndLighting,
        PhysicsAndTiming
    }

    public enum VROptimizationSeverity
    {
        Critical,    // Causes frame drops, motion sickness, or OOM crashes on mobile VR
        Warning,     // Noticeable performance hit or GPU cache inefficiencies
        Optimization // VR best practice recommendation
    }
}
