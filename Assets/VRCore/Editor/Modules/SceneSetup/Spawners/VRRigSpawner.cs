using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRCore.Editor.Modules.SceneSetup.Spawners
{
    public static class VRRigSpawner
    {
        public static GameObject SpawnRig(VRFrameworkType framework, Vector3 position)
        {
            // Clean up standalone default cameras to avoid AudioListener conflicts
            CleanupSceneCameras();

            if (framework == VRFrameworkType.MetaXRSDK)
            {
                return SpawnMetaXRRig(position);
            }
            else
            {
                return SpawnXRIRig(position);
            }
        }

        private static void CleanupSceneCameras()
        {
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var cam in cameras)
            {
                if (cam != null && cam.gameObject.name == "Main Camera" && cam.transform.parent == null)
                {
                    Undo.DestroyObjectImmediate(cam.gameObject);
                }
            }
        }

        public static GameObject SpawnXRIRig(Vector3 position)
        {
            // Ensure XR Interaction Manager exists in scene
            EnsureXRInteractionManager();

            // 1. Root XR Origin
            GameObject xrOriginGo = new GameObject("XR Origin (Action-Based XRI)");
            xrOriginGo.transform.position = position;
            Undo.RegisterCreatedObjectUndo(xrOriginGo, "Spawn XRI Origin Rig");

            // 2. Camera Offset
            GameObject cameraOffsetGo = new GameObject("Camera Offset");
            cameraOffsetGo.transform.SetParent(xrOriginGo.transform, false);
            cameraOffsetGo.transform.localPosition = new Vector3(0, 1.36f, 0);

            // 3. Main Camera
            GameObject mainCameraGo = new GameObject("Main Camera");
            mainCameraGo.transform.SetParent(cameraOffsetGo.transform, false);
            mainCameraGo.transform.localPosition = Vector3.zero;
            mainCameraGo.tag = "MainCamera";

            var camera = mainCameraGo.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 500f;
            mainCameraGo.AddComponent<AudioListener>();
            AttachTrackedPoseDriver(mainCameraGo, 0);

            // 4. Left Controller (Direct + Ray Interactors)
            GameObject leftControllerGo = new GameObject("Left Controller");
            leftControllerGo.transform.SetParent(cameraOffsetGo.transform, false);
            leftControllerGo.transform.localPosition = new Vector3(-0.25f, -0.2f, 0.4f);
            AttachTrackedPoseDriver(leftControllerGo, 1);
            AttachXRControllerAndInteractors(leftControllerGo, true);
            CreateHandVisual(leftControllerGo, new Color(0.2f, 0.5f, 0.95f), "Left Hand");

            // 5. Right Controller (Direct + Ray Interactors)
            GameObject rightControllerGo = new GameObject("Right Controller");
            rightControllerGo.transform.SetParent(cameraOffsetGo.transform, false);
            rightControllerGo.transform.localPosition = new Vector3(0.25f, -0.2f, 0.4f);
            AttachTrackedPoseDriver(rightControllerGo, 2);
            AttachXRControllerAndInteractors(rightControllerGo, false);
            CreateHandVisual(rightControllerGo, new Color(0.95f, 0.35f, 0.35f), "Right Hand");

            // 6. Locomotion System
            GameObject locomotionGo = new GameObject("Locomotion System");
            locomotionGo.transform.SetParent(xrOriginGo.transform, false);
            AttachLocomotionProviders(locomotionGo, xrOriginGo);

            // Bind XROrigin component if available
            AttachXROriginComponent(xrOriginGo, cameraOffsetGo, camera);

            Selection.activeGameObject = xrOriginGo;
            return xrOriginGo;
        }

        public static GameObject SpawnMetaXRRig(Vector3 position)
        {
            // 1. Root OVR Camera Rig
            GameObject ovrRigGo = new GameObject("OVRCameraRig (Meta XR)");
            ovrRigGo.transform.position = position;
            Undo.RegisterCreatedObjectUndo(ovrRigGo, "Spawn Meta OVR Rig");

            // Try attach OVRManager if Meta SDK is installed
            Type ovrManagerType = Type.GetType("OVRManager, Meta.XR.Core") ?? Type.GetType("OVRManager, Assembly-CSharp");
            if (ovrManagerType != null)
            {
                var mgr = ovrRigGo.AddComponent(ovrManagerType);
                var trackingOriginProp = ovrManagerType.GetProperty("trackingOriginType");
                if (trackingOriginProp != null)
                {
                    // 1 = FloorLevel in OVRManager.TrackingOrigin
                    trackingOriginProp.SetValue(mgr, 1);
                }
            }

            // 2. Tracking Space
            GameObject trackingSpace = new GameObject("TrackingSpace");
            trackingSpace.transform.SetParent(ovrRigGo.transform, false);

            // 3. Center Eye Anchor (Camera)
            GameObject centerEye = new GameObject("CenterEyeAnchor");
            centerEye.transform.SetParent(trackingSpace.transform, false);
            centerEye.tag = "MainCamera";
            var camera = centerEye.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 500f;
            centerEye.AddComponent<AudioListener>();

            // 4. Left Hand & Controller Anchors
            GameObject leftHandAnchor = new GameObject("LeftHandAnchor");
            leftHandAnchor.transform.SetParent(trackingSpace.transform, false);
            leftHandAnchor.transform.localPosition = new Vector3(-0.25f, 0, 0.3f);
            CreateHandVisual(leftHandAnchor, new Color(0.2f, 0.6f, 0.95f), "Meta Left Hand");

            // 5. Right Hand & Controller Anchors
            GameObject rightHandAnchor = new GameObject("RightHandAnchor");
            rightHandAnchor.transform.SetParent(trackingSpace.transform, false);
            rightHandAnchor.transform.localPosition = new Vector3(0.25f, 0, 0.3f);
            CreateHandVisual(rightHandAnchor, new Color(0.95f, 0.4f, 0.3f), "Meta Right Hand");

            // 6. Interaction SDK Containers
            GameObject interactionRig = new GameObject("OVRInteractionRig");
            interactionRig.transform.SetParent(ovrRigGo.transform, false);

            Selection.activeGameObject = ovrRigGo;
            return ovrRigGo;
        }

        private static void EnsureXRInteractionManager()
        {
            Type managerType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRInteractionManager, Unity.XR.Interaction.Toolkit");
            if (managerType != null)
            {
                var existing = Object.FindAnyObjectByType(managerType);
                if (existing == null)
                {
                    GameObject mgrGo = new GameObject("XR Interaction Manager");
                    mgrGo.AddComponent(managerType);
                    Undo.RegisterCreatedObjectUndo(mgrGo, "Create XR Interaction Manager");
                }
            }
        }

        private static void AttachXRControllerAndInteractors(GameObject controllerGo, bool isLeft)
        {
            // Direct Interactor Child
            GameObject directGo = new GameObject("Direct Interactor");
            directGo.transform.SetParent(controllerGo.transform, false);
            var directTrigger = directGo.AddComponent<SphereCollider>();
            directTrigger.isTrigger = true;
            directTrigger.radius = 0.1f;

            // Ray Interactor Child
            GameObject rayGo = new GameObject("Ray Interactor");
            rayGo.transform.SetParent(controllerGo.transform, false);
            
            // Try attach official XRI components via reflection
            Type directInteractorType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRDirectInteractor, Unity.XR.Interaction.Toolkit");
            if (directInteractorType != null) directGo.AddComponent(directInteractorType);

            Type rayInteractorType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRRayInteractor, Unity.XR.Interaction.Toolkit");
            if (rayInteractorType != null) rayGo.AddComponent(rayInteractorType);

            Type controllerType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.ActionBasedController, Unity.XR.Interaction.Toolkit");
            if (controllerType != null) controllerGo.AddComponent(controllerType);
        }

        private static void AttachLocomotionProviders(GameObject locomotionGo, GameObject xrOriginGo)
        {
            Type locomotionSystemType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.LocomotionSystem, Unity.XR.Interaction.Toolkit");
            if (locomotionSystemType != null)
            {
                var locSystem = locomotionGo.AddComponent(locomotionSystemType);
                
                // Teleportation Provider
                Type teleportType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.TeleportationProvider, Unity.XR.Interaction.Toolkit");
                if (teleportType != null) locomotionGo.AddComponent(teleportType);

                // Continuous Move Provider
                Type moveType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousMoveProvider, Unity.XR.Interaction.Toolkit");
                if (moveType != null) locomotionGo.AddComponent(moveType);

                // Snap Turn Provider
                Type turnType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.ActionBasedSnapTurnProvider, Unity.XR.Interaction.Toolkit");
                if (turnType != null) locomotionGo.AddComponent(turnType);
            }
        }

        private static void CreateHandVisual(GameObject parent, Color color, string name)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = $"{name} Visual";
            visual.transform.SetParent(parent.transform, false);
            visual.transform.localScale = new Vector3(0.08f, 0.08f, 0.12f);
            visual.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);

            var col = visual.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = color;
                renderer.sharedMaterial = mat;
            }

            // Laser Pointer Guide
            GameObject ray = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ray.name = "Ray Pointer";
            ray.transform.SetParent(visual.transform, false);
            ray.transform.localPosition = new Vector3(0, 0, 0.3f);
            ray.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            ray.transform.localScale = new Vector3(0.02f, 0.3f, 0.02f);

            var rayCol = ray.GetComponent<Collider>();
            if (rayCol != null) Object.DestroyImmediate(rayCol);

            var rayRen = ray.GetComponent<MeshRenderer>();
            if (rayRen != null)
            {
                var rayMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
                rayMat.color = new Color(color.r, color.g, color.b, 0.5f);
                rayRen.sharedMaterial = rayMat;
            }
        }

        private static void AttachTrackedPoseDriver(GameObject targetGo, int deviceRole)
        {
            Type inputSystemPoseDriver = Type.GetType("UnityEngine.InputSystem.XR.TrackedPoseDriver, Unity.InputSystem");
            if (inputSystemPoseDriver != null)
            {
                targetGo.AddComponent(inputSystemPoseDriver);
                return;
            }

            Type legacyPoseDriver = Type.GetType("UnityEngine.SpatialTracking.TrackedPoseDriver, UnityEngine.SpatialTracking");
            if (legacyPoseDriver != null)
            {
                targetGo.AddComponent(legacyPoseDriver);
            }
        }

        private static void AttachXROriginComponent(GameObject rootGo, GameObject cameraOffsetGo, Camera camera)
        {
            Type xrOriginType = Type.GetType("Unity.XR.CoreUtils.XROrigin, Unity.XR.CoreUtils") ??
                                Type.GetType("UnityEngine.XR.Interaction.Toolkit.XROrigin, Unity.XR.Interaction.Toolkit");

            if (xrOriginType != null)
            {
                var component = rootGo.AddComponent(xrOriginType);
                if (component != null)
                {
                    var offsetProp = xrOriginType.GetProperty("CameraFloorOffsetObject");
                    if (offsetProp != null) offsetProp.SetValue(component, cameraOffsetGo);

                    var camProp = xrOriginType.GetProperty("Camera");
                    if (camProp != null) camProp.SetValue(component, camera);
                }
            }
        }
    }
}
