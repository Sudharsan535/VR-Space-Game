using System;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.SceneSetup.Spawners
{
    public static class VRSocketHolsterSpawner
    {
        public static GameObject SpawnSnapSocket(Vector3 position, VRFrameworkType framework)
        {
            GameObject socketRoot = new GameObject("VR Snap Socket");
            socketRoot.transform.position = position;

            // Visual Socket Ring
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Socket Ring Visual";
            ring.transform.SetParent(socketRoot.transform, false);
            ring.transform.localScale = new Vector3(0.18f, 0.01f, 0.18f);

            var col = ring.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);

            var renderer = ring.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
                mat.color = new Color(0.2f, 0.8f, 0.95f, 0.6f);
                renderer.sharedMaterial = mat;
            }

            // Trigger Sphere Collider
            var sphereCol = socketRoot.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.12f;

            // Attach Point
            GameObject attachTransform = new GameObject("AttachTransform");
            attachTransform.transform.SetParent(socketRoot.transform, false);

            // Bind XRSocketInteractor if XRI available
            if (framework == VRFrameworkType.XRInteractionToolkit)
            {
                Type socketType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRSocketInteractor, Unity.XR.Interaction.Toolkit");
                if (socketType != null)
                {
                    var socketComp = socketRoot.AddComponent(socketType);
                    var attachProp = socketType.GetProperty("attachTransform");
                    if (attachProp != null) attachProp.SetValue(socketComp, attachTransform.transform);
                }
            }

            Undo.RegisterCreatedObjectUndo(socketRoot, "Spawn Snap Socket");
            Selection.activeGameObject = socketRoot;
            return socketRoot;
        }

        public static GameObject SpawnBodyInventoryHolsters(Transform parentHeadOrOrigin)
        {
            GameObject holsterRoot = new GameObject("VR Body Inventory Holsters");
            if (parentHeadOrOrigin != null)
            {
                holsterRoot.transform.SetParent(parentHeadOrOrigin, false);
            }

            // Left Hip Socket
            GameObject leftHip = new GameObject("Left Hip Holster");
            leftHip.transform.SetParent(holsterRoot.transform, false);
            leftHip.transform.localPosition = new Vector3(-0.25f, -0.45f, 0.05f);
            var leftCol = leftHip.AddComponent<SphereCollider>();
            leftCol.isTrigger = true;
            leftCol.radius = 0.12f;

            // Right Hip Socket
            GameObject rightHip = new GameObject("Right Hip Holster");
            rightHip.transform.SetParent(holsterRoot.transform, false);
            rightHip.transform.localPosition = new Vector3(0.25f, -0.45f, 0.05f);
            var rightCol = rightHip.AddComponent<SphereCollider>();
            rightCol.isTrigger = true;
            rightCol.radius = 0.12f;

            // Back Holster (Shoulder)
            GameObject backHolster = new GameObject("Back Holster (Shoulder)");
            backHolster.transform.SetParent(holsterRoot.transform, false);
            backHolster.transform.localPosition = new Vector3(0.2f, 0.1f, -0.2f);
            var backCol = backHolster.AddComponent<SphereCollider>();
            backCol.isTrigger = true;
            backCol.radius = 0.15f;

            Undo.RegisterCreatedObjectUndo(holsterRoot, "Spawn Body Holsters");
            Selection.activeGameObject = holsterRoot;
            return holsterRoot;
        }
    }
}
