using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Modules.SceneSetup.Spawners
{
    public static class VRMechanicalObjectSpawner
    {
        private static Material GetLitMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            return mat;
        }

        public static GameObject SpawnPushButton(Vector3 position)
        {
            GameObject root = new GameObject("VR Push Button");
            root.transform.position = position;

            // Base housing
            GameObject baseHousing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseHousing.name = "Button Base";
            baseHousing.transform.SetParent(root.transform, false);
            baseHousing.transform.localScale = new Vector3(0.12f, 0.02f, 0.12f);
            baseHousing.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.2f, 0.2f, 0.22f));

            // Moving Plunger (Button Top)
            GameObject plunger = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plunger.name = "Button Plunger";
            plunger.transform.SetParent(root.transform, false);
            plunger.transform.localPosition = new Vector3(0, 0.025f, 0);
            plunger.transform.localScale = new Vector3(0.09f, 0.02f, 0.09f);
            plunger.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.9f, 0.25f, 0.25f));

            // Configurable Joint for physical spring button press
            var rb = plunger.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            
            var joint = plunger.AddComponent<ConfigurableJoint>();
            joint.connectedBody = baseHousing.GetComponent<Rigidbody>() ?? baseHousing.AddComponent<Rigidbody>();
            joint.connectedBody.isKinematic = true;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            var limit = joint.linearLimit;
            limit.limit = 0.02f;
            joint.linearLimit = limit;

            var spring = joint.yDrive;
            spring.positionSpring = 400f;
            spring.positionDamper = 15f;
            joint.yDrive = spring;

            Undo.RegisterCreatedObjectUndo(root, "Spawn VR Push Button");
            Selection.activeGameObject = root;
            return root;
        }

        public static GameObject SpawnMechanicalLever(Vector3 position)
        {
            GameObject root = new GameObject("VR Mechanical Lever");
            root.transform.position = position;

            // Lever Base
            GameObject leverBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leverBase.name = "Lever Base";
            leverBase.transform.SetParent(root.transform, false);
            leverBase.transform.localScale = new Vector3(0.15f, 0.08f, 0.25f);
            leverBase.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.2f, 0.22f, 0.25f));

            var baseRb = leverBase.AddComponent<Rigidbody>();
            baseRb.isKinematic = true;

            // Lever Handle / Arm
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Lever Handle";
            handle.transform.SetParent(root.transform, false);
            handle.transform.localPosition = new Vector3(0, 0.15f, 0);
            handle.transform.localScale = new Vector3(0.04f, 0.15f, 0.04f);
            handle.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.95f, 0.6f, 0.15f));

            // Grip knob on top
            GameObject knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            knob.name = "Grip Knob";
            knob.transform.SetParent(handle.transform, false);
            knob.transform.localPosition = new Vector3(0, 1f, 0);
            knob.transform.localScale = new Vector3(2f, 0.5f, 2f);
            knob.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.9f, 0.25f, 0.25f));

            // Hinge Joint
            var handleRb = handle.AddComponent<Rigidbody>();
            handleRb.mass = 1.0f;
            handleRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var hinge = handle.AddComponent<HingeJoint>();
            hinge.connectedBody = baseRb;
            hinge.anchor = new Vector3(0, -1f, 0);
            hinge.axis = Vector3.right;
            hinge.useLimits = true;

            var limits = hinge.limits;
            limits.min = -45f;
            limits.max = 45f;
            hinge.limits = limits;

            Undo.RegisterCreatedObjectUndo(root, "Spawn Mechanical Lever");
            Selection.activeGameObject = root;
            return root;
        }

        public static GameObject SpawnPhysicsDoor(Vector3 position)
        {
            GameObject root = new GameObject("VR Physics Door");
            root.transform.position = position;

            // Door Frame Post
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Door Frame Post";
            frame.transform.SetParent(root.transform, false);
            frame.transform.localScale = new Vector3(0.1f, 2.1f, 0.15f);
            frame.transform.localPosition = new Vector3(-0.45f, 1.05f, 0);
            frame.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.2f, 0.2f, 0.22f));
            var frameRb = frame.AddComponent<Rigidbody>();
            frameRb.isKinematic = true;

            // Door Panel
            GameObject doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorPanel.name = "Door Panel";
            doorPanel.transform.SetParent(root.transform, false);
            doorPanel.transform.localScale = new Vector3(0.8f, 2.0f, 0.05f);
            doorPanel.transform.localPosition = new Vector3(0f, 1.05f, 0);
            doorPanel.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.45f, 0.35f, 0.25f));

            // Door Handle
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Door Handle";
            handle.transform.SetParent(doorPanel.transform, false);
            handle.transform.localPosition = new Vector3(0.35f, 0f, 0.06f);
            handle.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            handle.transform.localScale = new Vector3(0.04f, 0.08f, 0.04f);
            handle.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.85f, 0.75f, 0.2f));

            // Hinge Joint
            var doorRb = doorPanel.AddComponent<Rigidbody>();
            doorRb.mass = 8.0f;
            doorRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var hinge = doorPanel.AddComponent<HingeJoint>();
            hinge.connectedBody = frameRb;
            hinge.anchor = new Vector3(-0.5f, 0, 0);
            hinge.axis = Vector3.up;
            hinge.useLimits = true;

            var limits = hinge.limits;
            limits.min = 0f;
            limits.max = 95f;
            hinge.limits = limits;

            Undo.RegisterCreatedObjectUndo(root, "Spawn Physics Door");
            Selection.activeGameObject = root;
            return root;
        }

        public static GameObject SpawnSlidingDrawer(Vector3 position)
        {
            GameObject root = new GameObject("VR Sliding Drawer");
            root.transform.position = position;

            // Cabinet Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Cabinet Frame";
            frame.transform.SetParent(root.transform, false);
            frame.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            frame.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.25f, 0.25f, 0.28f));
            var frameRb = frame.AddComponent<Rigidbody>();
            frameRb.isKinematic = true;

            // Drawer Box
            GameObject drawer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            drawer.name = "Drawer Box";
            drawer.transform.SetParent(root.transform, false);
            drawer.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
            drawer.transform.localPosition = new Vector3(0, 0, 0.05f);
            drawer.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.4f, 0.32f, 0.24f));

            // Pull Handle
            GameObject pullHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pullHandle.name = "Pull Handle";
            pullHandle.transform.SetParent(drawer.transform, false);
            pullHandle.transform.localScale = new Vector3(0.2f, 0.04f, 0.04f);
            pullHandle.transform.localPosition = new Vector3(0, 0, 0.52f);
            pullHandle.GetComponent<MeshRenderer>().sharedMaterial = GetLitMaterial(new Color(0.9f, 0.6f, 0.15f));

            // Configurable Joint for sliding
            var drawerRb = drawer.AddComponent<Rigidbody>();
            drawerRb.mass = 3.0f;
            drawerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var joint = drawer.AddComponent<ConfigurableJoint>();
            joint.connectedBody = frameRb;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Limited;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            var limit = joint.linearLimit;
            limit.limit = 0.35f;
            joint.linearLimit = limit;

            Undo.RegisterCreatedObjectUndo(root, "Spawn Sliding Drawer");
            Selection.activeGameObject = root;
            return root;
        }
    }
}
