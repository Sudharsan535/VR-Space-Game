using System;

namespace VRCore.Editor.Modules.PackageManager
{
    [Serializable]
    public class VRPackageDefinition
    {
        public string PackageId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public VRPackageCategory Category { get; set; }
        public VRPackageRequirementLevel RequirementLevel { get; set; }
        public string TargetVersion { get; set; } // Optional: specify version or empty for latest compatible
        public string DocumentationUrl { get; set; }

        public bool IsInstalled { get; set; }
        public string InstalledVersion { get; set; }
        public bool IsOperationInProgress { get; set; }

        public VRPackageDefinition(
            string packageId, 
            string displayName, 
            string description, 
            VRPackageCategory category, 
            VRPackageRequirementLevel requirementLevel, 
            string targetVersion = null,
            string documentationUrl = null)
        {
            PackageId = packageId;
            DisplayName = displayName;
            Description = description;
            Category = category;
            RequirementLevel = requirementLevel;
            TargetVersion = targetVersion;
            DocumentationUrl = documentationUrl;
            IsInstalled = false;
            InstalledVersion = string.Empty;
            IsOperationInProgress = false;
        }
    }
}
