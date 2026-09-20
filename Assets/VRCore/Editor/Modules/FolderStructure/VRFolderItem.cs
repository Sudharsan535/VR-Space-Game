using System;
using System.Collections.Generic;

namespace VRCore.Editor.Modules.FolderStructure
{
    public enum VRFolderPresetType
    {
        StandardVR,
        MetaQuestXR,
        MinimalPrototype,
        Custom
    }

    [Serializable]
    public class VRFolderItem
    {
        public string RelativePath { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsEnabled { get; set; }

        public VRFolderItem(string relativePath, string description, string category, bool isEnabled = true)
        {
            RelativePath = relativePath;
            Description = description;
            Category = category;
            IsEnabled = isEnabled;
        }
    }
}
