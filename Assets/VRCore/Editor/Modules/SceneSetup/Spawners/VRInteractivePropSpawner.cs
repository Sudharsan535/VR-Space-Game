using System;
using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.SceneSetup.Spawners
{
    public static class VRInteractivePropSpawner
    {
        public static GameObject SpawnGrabbableProp(string name, PrimitiveType primitiveType, Vector3 position, Color color, VRFrameworkType framework)
        {
            GameObject prop = GameObject.CreatePrimitive(primitiveType);
            prop.name = name;
            prop.transform.position = position;
            prop.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            // Material
            var renderer = prop.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = color;
                renderer.sharedMaterial = mat;
            }

            // Physics Rigidbody
            var rb = prop.AddComponent<Rigidbody>();
            rb.mass = 1.0f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Custom Attach Point Transform (For precise hand grip alignment)
            GameObject attachPoint = new GameObject("AttachPoint");
            attachPoint.transform.SetParent(prop.transform, false);
            attachPoint.transform.localPosition = Vector3.zero;

            // Try attach XRI / Meta XR Grab Interactable
            if (framework == VRFrameworkType.XRInteractionToolkit)
            {
                Type grabType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable, Unity.XR.Interaction.Toolkit");
                if (grabType != null)
                {
                    var grabComponent = prop.AddComponent(grabType);
                    var attachProp = grabType.GetProperty("attachTransform");
                    if (attachProp != null) attachProp.SetValue(grabComponent, attachPoint.transform);
                }
            }
            else if (framework == VRFrameworkType.MetaXRSDK)
            {
                Type ovrGrabbable = Type.GetType("OVRGrabbable, Meta.XR.Core") ?? Type.GetType("OVRGrabbable, Assembly-CSharp");
                if (ovrGrabbable != null) prop.AddComponent(ovrGrabbable);
            }

            Undo.RegisterCreatedObjectUndo(prop, $"Spawn {name}");
            Selection.activeGameObject = prop;
            return prop;
        }
    }
}
