using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VRCore.Editor.Modules.SceneSetup.Generators
{
    public static class VRRigGenerator
    {
        public static GameObject SpawnXROriginRig(Vector3 position, Quaternion rotation)
        {
            // Remove or disable existing default cameras to avoid duplicate AudioListeners
            var existingCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var cam in existingCameras)
            {
                if (cam != null && cam.gameObject.name == "Main Camera" && cam.transform.parent == null)
                {
                    Undo.DestroyObjectImmediate(cam.gameObject);
                }
            }

            // 1. Root XR Origin
            GameObject xrOriginGo = new GameObject("XR Origin (VR Rig)");
            xrOriginGo.transform.position = position;
            xrOriginGo.transform.rotation = rotation;
            Undo.RegisterCreatedObjectUndo(xrOriginGo, "Create XR Origin Rig");

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

            // Try attach TrackedPoseDriver
            AttachTrackedPoseDriver(mainCameraGo, 0); // 0 = CenterEye / Head

            // 4. Left Controller
            GameObject leftControllerGo = new GameObject("Left Controller");
            leftControllerGo.transform.SetParent(cameraOffsetGo.transform, false);
            leftControllerGo.transform.localPosition = new Vector3(-0.25f, -0.2f, 0.4f);
            AttachTrackedPoseDriver(leftControllerGo, 1); // 1 = LeftHand
            CreateControllerVisual(leftControllerGo, new Color(0.2f, 0.5f, 0.9f));

            // 5. Right Controller
            GameObject rightControllerGo = new GameObject("Right Controller");
            rightControllerGo.transform.SetParent(cameraOffsetGo.transform, false);
            rightControllerGo.transform.localPosition = new Vector3(0.25f, -0.2f, 0.4f);
            AttachTrackedPoseDriver(rightControllerGo, 2); // 2 = RightHand
            CreateControllerVisual(rightControllerGo, new Color(0.9f, 0.3f, 0.3f));

            // 6. Locomotion System
            GameObject locomotionGo = new GameObject("Locomotion System");
            locomotionGo.transform.SetParent(xrOriginGo.transform, false);

            // Try attach XROrigin component via reflection if package exists
            AttachXROriginComponent(xrOriginGo, cameraOffsetGo, camera);

            Selection.activeGameObject = xrOriginGo;
            return xrOriginGo;
        }

        private static void CreateControllerVisual(GameObject controllerGo, Color color)
        {
            // Create a small stylized hand / controller visual representation
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Controller Visual";
            visual.transform.SetParent(controllerGo.transform, false);
            visual.transform.localScale = new Vector3(0.08f, 0.08f, 0.12f);
            visual.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);

            // Remove collider on visual so it doesn't interfere with physics
            var col = visual.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            // Apply distinct material
            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = color;
                renderer.sharedMaterial = mat;
            }

            // Small pointing ray / guide
            GameObject ray = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ray.name = "Pointer Ray Guide";
            ray.transform.SetParent(visual.transform, false);
            ray.transform.localPosition = new Vector3(0, 0, 0.3f);
            ray.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            ray.transform.localScale = new Vector3(0.04f, 0.3f, 0.04f);

            var rayCol = ray.GetComponent<Collider>();
            if (rayCol != null) Object.DestroyImmediate(rayCol);

            var rayRen = ray.GetComponent<MeshRenderer>();
            if (rayRen != null)
            {
                var rayMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
                rayMat.color = new Color(color.r, color.g, color.b, 0.6f);
                rayRen.sharedMaterial = rayMat;
            }
        }

        private static void AttachTrackedPoseDriver(GameObject targetGo, int deviceRole)
        {
            // Try UnityEngine.InputSystem.XR.TrackedPoseDriver
            Type inputSystemPoseDriver = Type.GetType("UnityEngine.InputSystem.XR.TrackedPoseDriver, Unity.InputSystem");
            if (inputSystemPoseDriver != null)
            {
                targetGo.AddComponent(inputSystemPoseDriver);
                return;
            }

            // Fallback: UnityEngine.SpatialTracking.TrackedPoseDriver
            Type legacyPoseDriver = Type.GetType("UnityEngine.SpatialTracking.TrackedPoseDriver, UnityEngine.SpatialTracking");
            if (legacyPoseDriver != null)
            {
                targetGo.AddComponent(legacyPoseDriver);
            }
        }

        private static void AttachXROriginComponent(GameObject rootGo, GameObject cameraOffsetGo, Camera camera)
        {
            Type xrOriginType = Type.GetType("Unity.XR.CoreUtils.XROrigin, Unity.XR.CoreUtils");
            if (xrOriginType == null)
            {
                xrOriginType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XROrigin, Unity.XR.Interaction.Toolkit");
            }

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
