using UnityEngine;

namespace VRCore.Editor.Modules.SceneSetup.Generators
{
    public static class VREnvironmentGenerator
    {
        public static void BuildEnvironment(VRSceneTemplateType templateType)
        {
            // 1. Root Environment Container
            GameObject envRoot = new GameObject("Environment");

            // 2. Lighting Setup
            CreateDirectionalLighting(envRoot);

            // 3. Materials
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            
            Material floorMat = new Material(litShader) { color = new Color(0.2f, 0.22f, 0.25f) };
            Material tableMat = new Material(litShader) { color = new Color(0.35f, 0.28f, 0.22f) };
            Material blueMat = new Material(litShader) { color = new Color(0.2f, 0.6f, 0.95f) };
            Material orangeMat = new Material(litShader) { color = new Color(0.95f, 0.55f, 0.15f) };
            Material greenMat = new Material(litShader) { color = new Color(0.25f, 0.8f, 0.4f) };
            Material purpleMat = new Material(litShader) { color = new Color(0.7f, 0.3f, 0.9f) };

            // 4. Floor Plane
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor (Teleport Area)";
            floor.transform.SetParent(envRoot.transform, false);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3f, 1f, 3f); // 30x30 meters
            floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            // Template-specific geometry
            switch (templateType)
            {
                case VRSceneTemplateType.InteractionPlayground:
                    BuildPlaygroundProps(envRoot, tableMat, blueMat, orangeMat, greenMat, purpleMat);
                    break;
                case VRSceneTemplateType.LocomotionLab:
                    BuildLocomotionObstacles(envRoot, floorMat, blueMat, greenMat);
                    break;
                case VRSceneTemplateType.MixedRealitySandbox:
                    BuildMixedRealityTable(envRoot, tableMat, blueMat, orangeMat);
                    break;
                case VRSceneTemplateType.PhysicsLab:
                    BuildPhysicsLab(envRoot, tableMat, blueMat, orangeMat, greenMat);
                    break;
                case VRSceneTemplateType.CleanVRTemplate:
                    // Clean: just floor & lighting
                    break;
            }
        }

        private static void CreateDirectionalLighting(GameObject parent)
        {
            GameObject lightGo = new GameObject("Directional Light (VR Optimized)");
            lightGo.transform.SetParent(parent.transform, false);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
        }

        private static void BuildPlaygroundProps(
            GameObject parent, 
            Material tableMat, 
            Material blueMat, 
            Material orangeMat, 
            Material greenMat, 
            Material purpleMat)
        {
            GameObject propsGroup = new GameObject("Interactive Props & Tables");
            propsGroup.transform.SetParent(parent.transform, false);

            // Table in front of user
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Interaction Table";
            table.transform.SetParent(propsGroup.transform, false);
            table.transform.position = new Vector3(0, 0.45f, 1.2f);
            table.transform.localScale = new Vector3(1.4f, 0.9f, 0.8f);
            table.GetComponent<MeshRenderer>().sharedMaterial = tableMat;

            // Grabbable Props on table
            CreatePhysicsProp("Physics Cube (Grabbable)", PrimitiveType.Cube, new Vector3(-0.35f, 0.96f, 1.2f), new Vector3(0.12f, 0.12f, 0.12f), blueMat, propsGroup.transform);
            CreatePhysicsProp("Physics Sphere (Grabbable)", PrimitiveType.Sphere, new Vector3(0f, 0.96f, 1.2f), new Vector3(0.12f, 0.12f, 0.12f), orangeMat, propsGroup.transform);
            CreatePhysicsProp("Physics Cylinder (Grabbable)", PrimitiveType.Cylinder, new Vector3(0.35f, 0.96f, 1.2f), new Vector3(0.1f, 0.12f, 0.1f), greenMat, propsGroup.transform);

            // Pedestal targets
            GameObject targetPedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            targetPedestal.name = "Target Pedestal";
            targetPedestal.transform.SetParent(propsGroup.transform, false);
            targetPedestal.transform.position = new Vector3(1.8f, 0.5f, 1.8f);
            targetPedestal.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            targetPedestal.GetComponent<MeshRenderer>().sharedMaterial = purpleMat;

            CreatePhysicsProp("Target Cube", PrimitiveType.Cube, new Vector3(1.8f, 1.08f, 1.8f), new Vector3(0.15f, 0.15f, 0.15f), purpleMat, propsGroup.transform);
        }

        private static void BuildLocomotionObstacles(GameObject parent, Material floorMat, Material blueMat, Material greenMat)
        {
            GameObject obstaclesGroup = new GameObject("Locomotion Course");
            obstaclesGroup.transform.SetParent(parent.transform, false);

            // Ramp
            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Inclined Ramp";
            ramp.transform.SetParent(obstaclesGroup.transform, false);
            ramp.transform.position = new Vector3(3f, 0.5f, 3f);
            ramp.transform.rotation = Quaternion.Euler(-15f, 45f, 0f);
            ramp.transform.localScale = new Vector3(2f, 0.2f, 4f);
            ramp.GetComponent<MeshRenderer>().sharedMaterial = blueMat;

            // Elevated Platform
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Elevated Platform";
            platform.transform.SetParent(obstaclesGroup.transform, false);
            platform.transform.position = new Vector3(4.5f, 1f, 4.5f);
            platform.transform.localScale = new Vector3(3f, 2f, 3f);
            platform.GetComponent<MeshRenderer>().sharedMaterial = greenMat;

            // Steps
            for (int i = 0; i < 4; i++)
            {
                GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"Step {i + 1}";
                step.transform.SetParent(obstaclesGroup.transform, false);
                step.transform.position = new Vector3(-3f, (i + 1) * 0.2f, 2f + (i * 0.5f));
                step.transform.localScale = new Vector3(1.5f, 0.2f, 0.5f);
                step.GetComponent<MeshRenderer>().sharedMaterial = floorMat;
            }
        }

        private static void BuildMixedRealityTable(GameObject parent, Material tableMat, Material blueMat, Material orangeMat)
        {
            GameObject mrGroup = new GameObject("Mixed Reality Anchors");
            mrGroup.transform.SetParent(parent.transform, false);

            // Table
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "MR Virtual Desk";
            table.transform.SetParent(mrGroup.transform, false);
            table.transform.position = new Vector3(0, 0.4f, 1f);
            table.transform.localScale = new Vector3(1.2f, 0.8f, 0.6f);
            table.GetComponent<MeshRenderer>().sharedMaterial = tableMat;

            CreatePhysicsProp("MR Interactive Widget", PrimitiveType.Cube, new Vector3(-0.2f, 0.86f, 1f), new Vector3(0.1f, 0.1f, 0.1f), blueMat, mrGroup.transform);
            CreatePhysicsProp("MR Sphere Widget", PrimitiveType.Sphere, new Vector3(0.2f, 0.86f, 1f), new Vector3(0.1f, 0.1f, 0.1f), orangeMat, mrGroup.transform);
        }

        private static void BuildPhysicsLab(
            GameObject parent, 
            Material tableMat, 
            Material blueMat, 
            Material orangeMat, 
            Material greenMat)
        {
            GameObject labGroup = new GameObject("Physics Testing Stations");
            labGroup.transform.SetParent(parent.transform, false);

            // Table
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Lab Workbench";
            table.transform.SetParent(labGroup.transform, false);
            table.transform.position = new Vector3(0, 0.45f, 1.2f);
            table.transform.localScale = new Vector3(2f, 0.9f, 0.8f);
            table.GetComponent<MeshRenderer>().sharedMaterial = tableMat;

            // Varied Mass Props
            var prop1 = CreatePhysicsProp("Light Prop (0.5 kg)", PrimitiveType.Cube, new Vector3(-0.6f, 0.96f, 1.2f), new Vector3(0.12f, 0.12f, 0.12f), blueMat, labGroup.transform);
            prop1.GetComponent<Rigidbody>().mass = 0.5f;

            var prop2 = CreatePhysicsProp("Medium Prop (2.0 kg)", PrimitiveType.Cube, new Vector3(0f, 0.96f, 1.2f), new Vector3(0.14f, 0.14f, 0.14f), greenMat, labGroup.transform);
            prop2.GetComponent<Rigidbody>().mass = 2f;

            var prop3 = CreatePhysicsProp("Heavy Prop (10.0 kg)", PrimitiveType.Cube, new Vector3(0.6f, 0.96f, 1.2f), new Vector3(0.18f, 0.18f, 0.18f), orangeMat, labGroup.transform);
            prop3.GetComponent<Rigidbody>().mass = 10f;
        }

        private static GameObject CreatePhysicsProp(
            string name, 
            PrimitiveType primitiveType, 
            Vector3 position, 
            Vector3 scale, 
            Material material, 
            Transform parent)
        {
            GameObject prop = GameObject.CreatePrimitive(primitiveType);
            prop.name = name;
            prop.transform.SetParent(parent, false);
            prop.transform.position = position;
            prop.transform.localScale = scale;

            if (material != null)
            {
                prop.GetComponent<MeshRenderer>().sharedMaterial = material;
            }

            var rb = prop.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            return prop;
        }
    }
}
