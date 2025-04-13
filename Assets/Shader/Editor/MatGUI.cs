using System;
using UnityEditor;
using UnityEngine;

public class MatGUI
{
    // GUI Properties
    public static GUIStyle MiddleTitleGUIStyle => GetMiddleTitleGUIStyle();
    public static GUIStyle LeftTitleGUIStyle => GetLeftTitleGUIStyle();
    public static GUIStyle RightEnumGUIStyle => GetTitleEnumGUIStyle();
    public static GUIStyle ToggleButtonStyle => GetToggleButtonGUIStyle();

    private static GUIStyle GetMiddleTitleGUIStyle()
    {
        var guiStyle = new GUIStyle
        {
            alignment = TextAnchor.UpperCenter,
            normal =
            {
                textColor = new Color(0.75f, 0.75f, 0.75f, 1)
            },
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };
        return guiStyle;
    }

    private static GUIStyle GetLeftTitleGUIStyle()
    {
        var guiStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            normal =
            {
                textColor = new Color(0.75f, 0.75f, 0.75f, 1)
            },
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };
        return guiStyle;
    }

    private static GUIStyle GetTitleEnumGUIStyle()
    {
        var guiStyle = new GUIStyle("popup")
        {
            alignment = TextAnchor.MiddleCenter
        };
        return guiStyle;
    }

    private static GUIStyle GetToggleButtonGUIStyle()
    {
        var guiStyle = new GUIStyle("button");
        return guiStyle;
    }

    public static void DrawVector2GUI(MaterialProperty property, in string title)
    {
        Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));
        Vector2 vector2 = new Vector2(property.vectorValue.x, property.vectorValue.y);
        using (var changeCheck = new EditorGUI.ChangeCheckScope())
        {
            vector2 = EditorGUI.Vector2Field(rect, title, vector2);
            if (changeCheck.changed)
            {
                property.vectorValue = new Vector4(vector2.x, vector2.y, property.vectorValue.z, property.vectorValue.w);
            }
        }
    }

    public static void DrawVector2SliderGUI(MaterialProperty property, in string title)
    {
        Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));
        Vector2 vector2 = new Vector2(property.vectorValue.x, property.vectorValue.y);
        using (var changeCheck = new EditorGUI.ChangeCheckScope())
        {
            EditorGUI.MinMaxSlider(rect, title, ref vector2.x, ref vector2.y, 0, 1);
            if (changeCheck.changed)
            {
                property.vectorValue = new Vector4(vector2.x, vector2.y, property.vectorValue.z, property.vectorValue.w);
            }
        }
    }

    public static void DrawTexturePropertyGUI(MaterialEditor materialEditor, MaterialProperty property, in string title)
    {
        // Creat Rect 
        var textureRectHeight = EditorGUIUtility.singleLineHeight * 3;
        textureRectHeight += 2;
        Rect textureRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, textureRectHeight));
        Rect titleRect = textureRect;
        titleRect.height = EditorGUIUtility.singleLineHeight;
        Rect tileRect = titleRect;
        tileRect.y += EditorGUIUtility.singleLineHeight;
        tileRect.xMax -= textureRectHeight + 5;
        Rect offsetRect = tileRect;
        offsetRect.y += EditorGUIUtility.singleLineHeight;

        // Draw Title
        GUI.Label(titleRect, title);

        // Draw Tile-Offset
        Vector4 tileOffsetValue = property.textureScaleAndOffset;
        Vector2 tileValue = new Vector2(tileOffsetValue.x, tileOffsetValue.y);
        Vector2 offsetValue = new Vector2(tileOffsetValue.z, tileOffsetValue.w);
        using (var changeCheck = new EditorGUI.ChangeCheckScope())
        {
            tileRect.xMin += 20;
            GUI.Label(tileRect, "Tiling");
            tileRect.xMin += 50;
            tileValue = EditorGUI.Vector2Field(tileRect, string.Empty, tileValue);
            offsetRect.xMin += 20;
            GUI.Label(offsetRect, "Offset");
            offsetRect.xMin += 50;
            offsetValue = EditorGUI.Vector2Field(offsetRect, string.Empty, offsetValue);
            if (changeCheck.changed)
                property.textureScaleAndOffset = new Vector4(tileValue.x, tileValue.y, offsetValue.x, offsetValue.y);
        }

        // Draw Texture Property
        textureRect.xMin = textureRect.xMax - textureRectHeight;
        textureRect.width = textureRectHeight;
        textureRect.height = textureRectHeight;
        using (var changeCheck = new EditorGUI.ChangeCheckScope())
        {
            var tempTexture = (Texture)EditorGUI.ObjectField(textureRect, property.textureValue, typeof(Texture), false);
            if (changeCheck.changed)
            {
                materialEditor.RegisterPropertyChangeUndo(property.name);
                property.textureValue = tempTexture;
            }
        }
    }

    public static void DrawTitleGUI(in string title, in GUIStyle titleStyle)
    {
        Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, 32));
        rect.yMin += 8;
        GUI.Label(rect, title, titleStyle);
    }

    public static void DrawFunctionTitleGUI(MaterialEditor materialEditor, MaterialProperty property, Material material, in string title)
    {
        Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, 32));
        rect.yMin += 8;
        GUI.Label(rect, title, LeftTitleGUIStyle);
        rect.xMin = rect.width - 50;
        BaseFunctionFlag selectionName = (BaseFunctionFlag)property.floatValue;
        using (var changeCheck = new EditorGUI.ChangeCheckScope())
        {
            var temp = Convert.ToSingle(EditorGUI.EnumPopup(rect, selectionName, RightEnumGUIStyle));
            if (changeCheck.changed)
            {
                property.floatValue = temp;
                materialEditor.RegisterPropertyChangeUndo(property.name);
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssetIfDirty(material);
            }
        }
    }

    public static void DrawLineGUI()
    {
        Rect LineRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, 1));
        GUI.DrawTexture(LineRect, Texture2D.grayTexture);
    }

    public static void DrawToggleButtonGUI(MaterialProperty property, in string title)
    {
        property.floatValue = GUILayout.Toggle(property.floatValue > 0.5f, title, ToggleButtonStyle) ? 1 : 0;
    }

    public static void DrawMultiVectorButton(MaterialProperty property, in string title)
    {
        var vectorValue = property.vectorValue;
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(title);
            GUILayout.BeginHorizontal();
            {
                vectorValue.x = GUILayout.Toggle(vectorValue.x > 0.5f, "R", ToggleButtonStyle) ? 1 : 0;
                vectorValue.y = GUILayout.Toggle(vectorValue.y > 0.5f, "G", ToggleButtonStyle) ? 1 : 0;
                vectorValue.z = GUILayout.Toggle(vectorValue.z > 0.5f, "B", ToggleButtonStyle) ? 1 : 0;
                vectorValue.w = GUILayout.Toggle(vectorValue.w > 0.5f, "A", ToggleButtonStyle) ? 1 : 0;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
        property.vectorValue = vectorValue;
    }

    public static void SetDisablePass(Material material, in float value, in string disablePassName)
    {
        bool disable = Convert.ToBoolean(value);
        if (disable)
            material.SetShaderPassEnabled(disablePassName, false);
        else
            material.SetShaderPassEnabled(disablePassName, true);
    }

    public static void SetEnablePass(Material material, in float value, in string enablePassName)
    {
        bool enable = Convert.ToBoolean(value);
        if (enable)
            material.SetShaderPassEnabled(enablePassName, true);
        else
            material.SetShaderPassEnabled(enablePassName, false);
    }

    public enum BaseFunctionFlag
    {
        Disable,
        Enable
    }
}