using UnityEditor;
using UnityEngine;

namespace VRCore.Editor.Core
{
    /// <summary>
    /// Centralized UI styling, theme colors, and layout helpers for the VR Central Hub.
    /// </summary>
    public static class VRHubStyles
    {
        // Theme Colors
        public static readonly Color HeaderBackgroundDark = new Color(0.13f, 0.15f, 0.19f, 1f);
        public static readonly Color HeaderBackgroundLight = new Color(0.85f, 0.88f, 0.92f, 1f);
        
        public static readonly Color SidebarBackgroundDark = new Color(0.15f, 0.16f, 0.18f, 1f);
        public static readonly Color SidebarBackgroundLight = new Color(0.9f, 0.9f, 0.92f, 1f);

        public static readonly Color AccentColor = new Color(0.24f, 0.54f, 0.96f, 1f);
        public static readonly Color SuccessColor = new Color(0.22f, 0.72f, 0.38f, 1f);
        public static readonly Color WarningColor = new Color(0.95f, 0.65f, 0.15f, 1f);
        public static readonly Color ErrorColor = new Color(0.92f, 0.28f, 0.28f, 1f);
        public static readonly Color NeutralMutedColor = new Color(0.55f, 0.58f, 0.62f, 1f);

        public static readonly Color CardBackgroundDark = new Color(0.18f, 0.20f, 0.23f, 1f);
        public static readonly Color CardBackgroundLight = new Color(0.96f, 0.96f, 0.97f, 1f);

        public static readonly Color CardBorderDark = new Color(0.26f, 0.28f, 0.32f, 1f);
        public static readonly Color CardBorderLight = new Color(0.82f, 0.82f, 0.85f, 1f);

        public static Color HeaderBg => EditorGUIUtility.isProSkin ? HeaderBackgroundDark : HeaderBackgroundLight;
        public static Color SidebarBg => EditorGUIUtility.isProSkin ? SidebarBackgroundDark : SidebarBackgroundLight;
        public static Color CardBg => EditorGUIUtility.isProSkin ? CardBackgroundDark : CardBackgroundLight;
        public static Color CardBorder => EditorGUIUtility.isProSkin ? CardBorderDark : CardBorderLight;

        // Cached GUIStyles
        private static GUIStyle s_TitleStyle;
        public static GUIStyle TitleStyle
        {
            get
            {
                if (s_TitleStyle == null)
                {
                    s_TitleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 18,
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return s_TitleStyle;
            }
        }

        private static GUIStyle s_SubtitleStyle;
        public static GUIStyle SubtitleStyle
        {
            get
            {
                if (s_SubtitleStyle == null)
                {
                    s_SubtitleStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontSize = 11,
                        wordWrap = true
                    };
                    s_SubtitleStyle.normal.textColor = NeutralMutedColor;
                }
                return s_SubtitleStyle;
            }
        }

        private static GUIStyle s_SectionHeaderStyle;
        public static GUIStyle SectionHeaderStyle
        {
            get
            {
                if (s_SectionHeaderStyle == null)
                {
                    s_SectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 14,
                        margin = new RectOffset(0, 0, 10, 5)
                    };
                }
                return s_SectionHeaderStyle;
            }
        }

        private static GUIStyle s_CardStyle;
        public static GUIStyle CardStyle
        {
            get
            {
                if (s_CardStyle == null)
                {
                    s_CardStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(12, 12, 10, 10),
                        margin = new RectOffset(0, 0, 4, 6)
                    };
                }
                return s_CardStyle;
            }
        }

        private static GUIStyle s_SidebarButtonStyle;
        public static GUIStyle SidebarButtonStyle
        {
            get
            {
                if (s_SidebarButtonStyle == null)
                {
                    s_SidebarButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 12,
                        fixedHeight = 36,
                        padding = new RectOffset(12, 12, 0, 0),
                        margin = new RectOffset(4, 4, 2, 2)
                    };
                }
                return s_SidebarButtonStyle;
            }
        }

        private static GUIStyle s_BadgeStyle;
        public static GUIStyle BadgeStyle
        {
            get
            {
                if (s_BadgeStyle == null)
                {
                    s_BadgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(6, 6, 2, 2)
                    };
                }
                return s_BadgeStyle;
            }
        }

        /// <summary>
        /// Draws a stylized solid color rectangle.
        /// </summary>
        public static void DrawRect(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Draws a badge with rounded appearance and colored text.
        /// </summary>
        public static void DrawBadge(string text, Color backgroundColor, Color textColor, float width = 90f)
        {
            Rect rect = width > 0 
                ? GUILayoutUtility.GetRect(width, 20f, GUILayout.Width(width), GUILayout.Height(20f))
                : GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true), GUILayout.Height(20f));
            
            Color prevColor = GUI.color;
            EditorGUI.DrawRect(rect, backgroundColor);

            GUIStyle badge = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = textColor }
            };
            GUI.Label(rect, text, badge);
            GUI.color = prevColor;
        }

        /// <summary>
        /// Draws a horizontal divider line.
        /// </summary>
        public static void DrawDivider(float height = 1f, float marginY = 8f)
        {
            GUILayout.Space(marginY);
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height), GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, CardBorder);
            GUILayout.Space(marginY);
        }
    }
}
