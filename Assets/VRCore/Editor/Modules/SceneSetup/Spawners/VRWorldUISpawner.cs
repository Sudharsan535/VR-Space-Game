using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace VRCore.Editor.Modules.SceneSetup.Spawners
{
    public static class VRWorldUISpawner
    {
        public static GameObject SpawnWorldSpaceVRUI(Vector3 position)
        {
            // Ensure EventSystem exists in scene
            EnsureEventSystem();

            // 1. Root Canvas
            GameObject canvasGo = new GameObject("VR World-Space Menu Canvas");
            canvasGo.transform.position = position;

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3f;

            canvasGo.AddComponent<GraphicRaycaster>();
            AttachTrackedRaycaster(canvasGo);

            var rectTransform = canvasGo.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(800f, 500f);
            rectTransform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f); // ~1.2m wide in VR

            // 2. Background Panel
            GameObject panelGo = new GameObject("Background Panel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(0.12f, 0.14f, 0.18f, 0.92f);
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // 3. Title Text
            GameObject titleGo = new GameObject("Menu Title");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleText = titleGo.AddComponent<Text>();
            titleText.text = "VR INTERACTIVE MENU";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 32;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.75f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.sizeDelta = Vector2.zero;

            // 4. Sample Action Button
            CreateSampleButton("Primary Action Button", "START VR EXPERIENCE", new Vector2(0.5f, 0.5f), new Vector2(320f, 60f), new Color(0.2f, 0.55f, 0.95f), panelGo.transform);
            CreateSampleButton("Secondary Action Button", "OPEN SETTINGS", new Vector2(0.5f, 0.32f), new Vector2(320f, 50f), new Color(0.25f, 0.28f, 0.32f), panelGo.transform);

            Undo.RegisterCreatedObjectUndo(canvasGo, "Spawn VR World UI");
            Selection.activeGameObject = canvasGo;
            return canvasGo;
        }

        private static void CreateSampleButton(string name, string text, Vector2 anchorPos, Vector2 size, Color color, Transform parent)
        {
            GameObject btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);

            var img = btnGo.AddComponent<Image>();
            img.color = color;

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f);
            colors.pressedColor = new Color(color.r * 0.8f, color.g * 0.8f, color.b * 0.8f);
            btn.colors = colors;

            var rect = btnGo.GetComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.sizeDelta = size;

            // Text
            GameObject txtGo = new GameObject("Button Label");
            txtGo.transform.SetParent(btnGo.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.text = text;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 20;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            var txtRect = txtGo.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                
                // Try attach InputSystemUIInputModule or StandaloneInputModule
                Type inputSystemModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputSystemModule != null)
                {
                    esGo.AddComponent(inputSystemModule);
                }
                else
                {
                    esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
                Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
            }
        }

        private static void AttachTrackedRaycaster(GameObject canvasGo)
        {
            Type trackedRaycaster = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
            if (trackedRaycaster != null)
            {
                canvasGo.AddComponent(trackedRaycaster);
            }
        }
    }
}
