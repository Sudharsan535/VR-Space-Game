namespace VRCore.Editor.Modules.ProjectSettings
{
    public enum VRSettingSeverity
    {
        Critical,    // Will cause crashes, build failure, or OpenXR initialization failure
        Warning,     // Significant performance degradation or missing VR features
        Recommended, // Standard industry VR best practices (e.g. MSAA 4x, ASTC)
        Info         // Informational / optional optimization
    }

    public enum VRSettingCategory
    {
        All,
        Player,
        XRPlugin,
        Input,
        Android,
        Windows,
        Graphics
    }

    public enum VRPlatformPreset
    {
        All,
        AndroidQuest,
        WindowsPCVR
    }
}
