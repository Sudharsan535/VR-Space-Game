using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using VRCore.Editor.Core;

namespace VRCore.Editor.Modules.PackageManager
{
    /// <summary>
    /// Responsive module for VR Central Hub that inspects, manages, and automates
    /// the installation of VR dependencies, Unity XR packages, and Meta XR SDKs.
    /// Adapts layout dynamically to small, docked, and wide window sizes.
    /// </summary>
    public class VRPackageManagerModule : VRHubModuleBase
    {
        public override string ModuleId => "vr_package_manager";
        public override string DisplayName => "Package Manager";
        public override string Description => "Verify, install, and manage Unity XR dependencies and Meta SDKs.";
        public override string IconName => "d_Package Manager";
        public override int Priority => 10;

        private const float NarrowThreshold = 460f;

        private List<VRPackageDefinition> _packages;
        private ListRequest _listRequest;
        private AddRequest _addRequest;
        private readonly Queue<string> _installQueue = new Queue<string>();

        private string _activeInstallingPackageId;
        private string _statusMessage = "Ready";
        private bool _isScanning;
        private string _searchFilter = "";
        private VRPackageCategory _selectedCategory = VRPackageCategory.All;

        public override void OnEnable()
        {
            if (_packages == null)
            {
                _packages = VRPackageDatabase.GetDefaultPackages();
            }
            RefreshInstalledPackages();
        }

        public override void OnDisable()
        {
            _listRequest = null;
            _addRequest = null;
            _activeInstallingPackageId = null;
        }

        public override void OnUpdate()
        {
            // Handle List / Query Request
            if (_listRequest != null && _listRequest.IsCompleted)
            {
                _isScanning = false;
                if (_listRequest.Status == StatusCode.Success)
                {
                    UpdatePackageStatus(_listRequest.Result);
                    _statusMessage = "Package list updated.";
                }
                else if (_listRequest.Status >= StatusCode.Failure)
                {
                    _statusMessage = $"Check failed: {_listRequest.Error.message}";
                    Debug.LogError($"[VR Central Hub] Package check error: {_listRequest.Error.message}");
                }
                _listRequest = null;
            }

            // Handle Add / Install Request
            if (_addRequest != null && _addRequest.IsCompleted)
            {
                if (_addRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"[VR Central Hub] Successfully installed: {_addRequest.Result.name} ({_addRequest.Result.version})");
                    _statusMessage = $"Installed {_addRequest.Result.name}";
                }
                else if (_addRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"[VR Central Hub] Failed to install package: {_addRequest.Error.message}");
                    _statusMessage = $"Error: {_addRequest.Error.message}";
                }

                _addRequest = null;
                _activeInstallingPackageId = null;

                if (_installQueue.Count > 0)
                {
                    ProcessNextInstallQueue();
                }
                else
                {
                    RefreshInstalledPackages();
                }
            }
        }

        public void RefreshInstalledPackages()
        {
            if (_isScanning) return;
            _isScanning = true;
            _statusMessage = "Scanning installed packages...";
            _listRequest = Client.List(true);
        }

        public void InstallPackage(string packageId, string targetVersion = null)
        {
            string packageIdentifier = string.IsNullOrEmpty(targetVersion) ? packageId : $"{packageId}@{targetVersion}";
            
            if (_addRequest != null || _activeInstallingPackageId != null)
            {
                if (!_installQueue.Contains(packageIdentifier))
                {
                    _installQueue.Enqueue(packageIdentifier);
                    _statusMessage = $"Queued: {packageId} ({_installQueue.Count} in queue)";
                }
                return;
            }

            ExecuteInstall(packageIdentifier);
        }

        private void ExecuteInstall(string packageIdentifier)
        {
            _activeInstallingPackageId = packageIdentifier;
            _statusMessage = $"Installing {packageIdentifier}...";
            Debug.Log($"[VR Central Hub] Installing package: {packageIdentifier}");
            _addRequest = Client.Add(packageIdentifier);
        }

        private void ProcessNextInstallQueue()
        {
            if (_installQueue.Count > 0)
            {
                string nextPackage = _installQueue.Dequeue();
                ExecuteInstall(nextPackage);
            }
        }

        public void InstallAllMissingEssentials()
        {
            var missingEssentials = _packages.Where(p => 
                (p.RequirementLevel == VRPackageRequirementLevel.Required || p.RequirementLevel == VRPackageRequirementLevel.Recommended) 
                && !p.IsInstalled && p.Category != VRPackageCategory.MetaXR).ToList();

            if (missingEssentials.Count == 0)
            {
                EditorUtility.DisplayDialog("VR Central Hub", "All essential VR packages are already installed!", "OK");
                return;
            }

            foreach (var pkg in missingEssentials)
            {
                InstallPackage(pkg.PackageId, pkg.TargetVersion);
            }
        }

        public void InstallMetaXRSuite()
        {
            InstallPackage("com.meta.xr.sdk.all");
            InstallPackage("com.meta.xr.simulator");
        }

        private void UpdatePackageStatus(PackageCollection installedPackages)
        {
            var installedDict = installedPackages.ToDictionary(p => p.name, p => p.version);

            foreach (var pkg in _packages)
            {
                if (installedDict.TryGetValue(pkg.PackageId, out string version))
                {
                    pkg.IsInstalled = true;
                    pkg.InstalledVersion = version;
                }
                else
                {
                    pkg.IsInstalled = false;
                    pkg.InstalledVersion = string.Empty;
                }
                pkg.IsOperationInProgress = (_activeInstallingPackageId != null && _activeInstallingPackageId.StartsWith(pkg.PackageId));
            }
        }

        protected override void DrawContent()
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            bool isNarrow = viewWidth < NarrowThreshold;

            DrawControlToolbar(isNarrow);
            DrawSummaryBanner(isNarrow);
            DrawCategorySelector(isNarrow);
            DrawPackageList(isNarrow);
        }

        private void DrawControlToolbar(bool isNarrow)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Search Bar
            GUILayout.Label(EditorGUIUtility.IconContent("d_Search Icon"), GUILayout.Width(18));
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.MinWidth(60), GUILayout.ExpandWidth(true));

            if (!string.IsNullOrEmpty(_searchFilter) && GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(18)))
            {
                _searchFilter = "";
                GUI.FocusControl(null);
            }

            if (!isNarrow)
            {
                GUILayout.FlexibleSpace();
                if (_isScanning || _activeInstallingPackageId != null)
                {
                    GUILayout.Label(EditorGUIUtility.IconContent("d_WaitSpin00"), GUILayout.Width(18));
                }
                GUILayout.Label(_statusMessage, EditorStyles.miniLabel);
            }

            // Refresh button
            GUIContent scanBtn = isNarrow
                ? new GUIContent(EditorGUIUtility.IconContent("d_Refresh").image, "Scan Packages")
                : new GUIContent(" Scan", EditorGUIUtility.IconContent("d_Refresh").image);

            if (GUILayout.Button(scanBtn, EditorStyles.toolbarButton, GUILayout.Width(isNarrow ? 26 : 64)))
            {
                RefreshInstalledPackages();
            }

            EditorGUILayout.EndHorizontal();

            if (isNarrow && (_isScanning || _activeInstallingPackageId != null || !string.IsNullOrEmpty(_statusMessage)))
            {
                EditorGUILayout.BeginHorizontal();
                if (_isScanning || _activeInstallingPackageId != null)
                {
                    GUILayout.Label(EditorGUIUtility.IconContent("d_WaitSpin00"), GUILayout.Width(16), GUILayout.Height(16));
                }
                EditorGUILayout.LabelField(_statusMessage, EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(4);
        }

        private void DrawSummaryBanner(bool isNarrow)
        {
            BeginCard();

            int installedCount = _packages.Count(p => p.IsInstalled);
            int totalCount = _packages.Count;
            int missingRequired = _packages.Count(p => p.RequirementLevel == VRPackageRequirementLevel.Required && !p.IsInstalled);
            int missingRecommended = _packages.Count(p => p.RequirementLevel == VRPackageRequirementLevel.Recommended && !p.IsInstalled);

            if (isNarrow)
            {
                // Vertical Stacked Layout for Narrow / Docked View
                EditorGUILayout.LabelField("Environment Overview", EditorStyles.boldLabel);
                
                string statusText = $"Installed: <b>{installedCount}/{totalCount}</b> | ";
                if (missingRequired > 0) statusText += $"<color=#e84c4c><b>{missingRequired} Required Missing</b></color>";
                else statusText += "<color=#38b860><b>Requirements Met</b></color>";

                GUIStyle richLabel = new GUIStyle(EditorStyles.miniLabel) { richText = true, wordWrap = true };
                EditorGUILayout.LabelField(statusText, richLabel);

                GUILayout.Space(4);

                if (missingRequired > 0 || missingRecommended > 0)
                {
                    Color prevColor = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent(" Install Missing XR Essentials", EditorGUIUtility.IconContent("d_CreateAddNew").image), GUILayout.Height(26), GUILayout.ExpandWidth(true)))
                    {
                        InstallAllMissingEssentials();
                    }
                    GUI.backgroundColor = prevColor;
                }

                if (GUILayout.Button(new GUIContent(" Install Meta XR Suite", EditorGUIUtility.IconContent("d_BuildSettings.Android").image), GUILayout.Height(24), GUILayout.ExpandWidth(true)))
                {
                    if (EditorUtility.DisplayDialog("Install Meta XR Suite", "Queue Meta XR All-In-One SDK and Simulator installation?", "Install", "Cancel"))
                    {
                        InstallMetaXRSuite();
                    }
                }
            }
            else
            {
                // Wide Mode Layout
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("VR Environment Overview", EditorStyles.boldLabel);
                
                string overviewText = $"Installed: {installedCount}/{totalCount} packages | ";
                if (missingRequired > 0) overviewText += $"<color=#e84c4c><b>{missingRequired} Required Missing</b></color> | ";
                else overviewText += "<color=#38b860><b>All Core Requirements Met</b></color> | ";

                if (missingRecommended > 0) overviewText += $"<color=#f2a626>{missingRecommended} Recommended Available</color>";
                else overviewText += "<color=#38b860>All Recommended Installed</color>";

                GUIStyle richLabel = new GUIStyle(EditorStyles.miniLabel) { richText = true, wordWrap = true };
                EditorGUILayout.LabelField(overviewText, richLabel);
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical();
                if (missingRequired > 0 || missingRecommended > 0)
                {
                    Color prevColor = GUI.backgroundColor;
                    GUI.backgroundColor = VRHubStyles.SuccessColor;
                    if (GUILayout.Button(new GUIContent(" Install Missing XR Essentials", EditorGUIUtility.IconContent("d_CreateAddNew").image), GUILayout.Height(26)))
                    {
                        InstallAllMissingEssentials();
                    }
                    GUI.backgroundColor = prevColor;
                }

                if (GUILayout.Button(new GUIContent(" Install Meta XR Suite", EditorGUIUtility.IconContent("d_BuildSettings.Android").image), GUILayout.Height(22)))
                {
                    if (EditorUtility.DisplayDialog("Install Meta XR Suite", "Queue Meta XR All-In-One SDK and Simulator installation?", "Install", "Cancel"))
                    {
                        InstallMetaXRSuite();
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            EndCard();
            GUILayout.Space(6);
        }

        private void DrawCategorySelector(bool isNarrow)
        {
            if (isNarrow)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Category:", GUILayout.Width(60));
                _selectedCategory = (VRPackageCategory)EditorGUILayout.EnumPopup(_selectedCategory, EditorStyles.popup);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawCategoryTabButton(VRPackageCategory.All, "All");
                DrawCategoryTabButton(VRPackageCategory.CoreXR, "Core XR");
                DrawCategoryTabButton(VRPackageCategory.InteractionAndHands, "Interaction & Hands");
                DrawCategoryTabButton(VRPackageCategory.MetaXR, "Meta XR");
                DrawCategoryTabButton(VRPackageCategory.RenderingAndInput, "Rendering/Input");
                DrawCategoryTabButton(VRPackageCategory.Utilities, "Utilities");
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(6);
        }

        private void DrawCategoryTabButton(VRPackageCategory category, string label)
        {
            bool isSelected = (_selectedCategory == category);
            Color prevColor = GUI.backgroundColor;
            if (isSelected)
            {
                GUI.backgroundColor = VRHubStyles.AccentColor;
            }

            if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Height(22)))
            {
                _selectedCategory = category;
            }

            GUI.backgroundColor = prevColor;
        }

        private void DrawPackageList(bool isNarrow)
        {
            var filtered = _packages.Where(p =>
                (_selectedCategory == VRPackageCategory.All || p.Category == _selectedCategory) &&
                (string.IsNullOrEmpty(_searchFilter) || 
                 p.DisplayName.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 p.PackageId.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            if (filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No packages match the current filter.", MessageType.Info);
                return;
            }

            foreach (var pkg in filtered)
            {
                DrawPackageCard(pkg, isNarrow);
            }
        }

        private void DrawPackageCard(VRPackageDefinition pkg, bool isNarrow)
        {
            BeginCard();

            if (isNarrow)
            {
                // Responsive Docked / Narrow Card Layout
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(pkg.DisplayName, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                DrawRequirementBadge(pkg.RequirementLevel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(pkg.PackageId, EditorStyles.miniLabel);
                EditorGUILayout.LabelField(pkg.Description, VRHubStyles.SubtitleStyle);

                GUILayout.Space(4);

                // Status & Action row
                EditorGUILayout.BeginHorizontal();
                DrawPackageStatusBadge(pkg, isNarrow);

                if (!string.IsNullOrEmpty(pkg.DocumentationUrl))
                {
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("_Help").image, "View Docs"), GUILayout.Width(24), GUILayout.Height(22)))
                    {
                        Application.OpenURL(pkg.DocumentationUrl);
                    }
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(2);

                // Full-width Action Button
                DrawActionButton(pkg, true);
            }
            else
            {
                // Wide Card Layout
                EditorGUILayout.BeginHorizontal();

                // Left info column
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(pkg.DisplayName, EditorStyles.boldLabel, GUILayout.MinWidth(140), GUILayout.MaxWidth(240));
                DrawRequirementBadge(pkg.RequirementLevel);
                EditorGUILayout.LabelField($"[{pkg.Category}]", EditorStyles.miniLabel, GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(pkg.PackageId, EditorStyles.miniLabel);
                EditorGUILayout.LabelField(pkg.Description, VRHubStyles.SubtitleStyle);
                EditorGUILayout.EndVertical();

                GUILayout.Space(8);

                // Right Actions Column
                EditorGUILayout.BeginVertical(GUILayout.Width(170));
                DrawPackageStatusBadge(pkg, false);

                GUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                if (!string.IsNullOrEmpty(pkg.DocumentationUrl))
                {
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("_Help").image, "View Docs"), GUILayout.Width(24), GUILayout.Height(22)))
                    {
                        Application.OpenURL(pkg.DocumentationUrl);
                    }
                }

                DrawActionButton(pkg, false);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            EndCard();
        }

        private void DrawActionButton(VRPackageDefinition pkg, bool expandWidth)
        {
            bool isBusy = (_activeInstallingPackageId != null && _activeInstallingPackageId.StartsWith(pkg.PackageId));
            if (isBusy)
            {
                GUI.enabled = false;
                GUILayout.Button("Installing...", GUILayout.Height(22), GUILayout.ExpandWidth(expandWidth));
                GUI.enabled = true;
            }
            else if (pkg.IsInstalled)
            {
                if (GUILayout.Button("Reinstall", GUILayout.Height(22), GUILayout.ExpandWidth(expandWidth)))
                {
                    InstallPackage(pkg.PackageId, pkg.TargetVersion);
                }
            }
            else
            {
                Color prevBg = GUI.backgroundColor;
                GUI.backgroundColor = pkg.RequirementLevel == VRPackageRequirementLevel.Required ? VRHubStyles.SuccessColor : Color.white;
                
                if (GUILayout.Button(new GUIContent(" Add Package", EditorGUIUtility.IconContent("d_Toolbar Plus").image), GUILayout.Height(22), GUILayout.ExpandWidth(expandWidth)))
                {
                    InstallPackage(pkg.PackageId, pkg.TargetVersion);
                }
                GUI.backgroundColor = prevBg;
            }
        }

        private void DrawRequirementBadge(VRPackageRequirementLevel level)
        {
            switch (level)
            {
                case VRPackageRequirementLevel.Required:
                    VRHubStyles.DrawBadge("REQUIRED", new Color(0.85f, 0.25f, 0.25f, 0.35f), VRHubStyles.ErrorColor, 68f);
                    break;
                case VRPackageRequirementLevel.Recommended:
                    VRHubStyles.DrawBadge("RECOMMENDED", new Color(0.9f, 0.65f, 0.15f, 0.35f), VRHubStyles.WarningColor, 88f);
                    break;
                case VRPackageRequirementLevel.Optional:
                    VRHubStyles.DrawBadge("OPTIONAL", new Color(0.5f, 0.5f, 0.5f, 0.25f), VRHubStyles.NeutralMutedColor, 62f);
                    break;
            }
        }

        private void DrawPackageStatusBadge(VRPackageDefinition pkg, bool isNarrow)
        {
            float width = isNarrow ? 0 : 140f;
            if (_activeInstallingPackageId != null && _activeInstallingPackageId.StartsWith(pkg.PackageId))
            {
                VRHubStyles.DrawBadge("INSTALLING...", new Color(0.24f, 0.54f, 0.96f, 0.35f), VRHubStyles.AccentColor, width);
            }
            else if (pkg.IsInstalled)
            {
                string text = string.IsNullOrEmpty(pkg.InstalledVersion) ? "INSTALLED" : $"INSTALLED v{pkg.InstalledVersion}";
                VRHubStyles.DrawBadge(text, new Color(0.22f, 0.72f, 0.38f, 0.35f), VRHubStyles.SuccessColor, width);
            }
            else
            {
                VRHubStyles.DrawBadge("NOT INSTALLED", new Color(0.5f, 0.5f, 0.5f, 0.2f), VRHubStyles.NeutralMutedColor, width);
            }
        }
    }
}
