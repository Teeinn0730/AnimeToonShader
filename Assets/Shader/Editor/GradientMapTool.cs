using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GradientMapTool : EditorWindow
{
    // Data
    private GradientTexture _loadTexture = new GradientTexture();
    [SerializeField] private List<Gradient> _gradient = new List<Gradient>();
    private Gradient[] _restoreGradients;

    // GUI
    private Vector2 _scrollView;
    private bool _preview;
    private SerializedObject _soGradient;
    private SerializedProperty _spGradient;
    private readonly GUIContent[] _gradientOperation = new[] { new GUIContent("Preview"), new GUIContent("EditMode") };
    private int _currentSelectedGradientOperation;


    [MenuItem("Tools/AnimeToon/GradientMap")]
    private static void ShowWindow()
    {
        GetWindow<GradientMapTool>("Generate Gradient Tool");
    }

    private void OnGUI()
    {
        _scrollView = GUILayout.BeginScrollView(_scrollView);
        {
            DrawGradientMapOperationMode();
            DrawSaveOrLoadGUI();
        }
        GUILayout.EndScrollView();
    }

    private void OnEnable()
    {
        _soGradient = new SerializedObject(this);
        _spGradient = _soGradient.FindProperty("_gradient");
    }

    private void OnDestroy()
    {
        CheckTextureIsSaved(_loadTexture);
    }

    private void CheckTextureIsSaved(GradientTexture gradientTexture)
    {
        if (gradientTexture.isCached)
        {
            var goSave = EditorUtility.DisplayDialog("Save texture", "Don't forget to save gradient tex when you leave.", "Save", "Cancel");
            if (goSave)
            {
                SaveTexture(SetGradientMapsToTexture(_gradient.ToArray()));
                gradientTexture.isCached = false;
            }
        }
    }

    private void DrawGradientMapOperationMode()
    {
        GUILayout.BeginVertical("box");
        {
            _currentSelectedGradientOperation = GUILayout.Toolbar(_currentSelectedGradientOperation,
                _gradientOperation,
                new bool[] { true, _loadTexture.gradientTex != null },
                new GUIStyle("button"));

            switch (_currentSelectedGradientOperation)
            {
                case 0:
                    DrawGradientMapGUI_Preview();
                    break;
                case 1:
                    DrawGradientMapGUI_EditMode();
                    break;
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawGradientMapGUI_Preview()
    {
        using var changeCheck = new EditorGUI.ChangeCheckScope();
        EditorGUILayout.PropertyField(_spGradient, new GUIContent("Gradients"), true);

        // Switch texture's color back when 'Preview mode' is enable.
        if (_loadTexture.isCached && _loadTexture.gradientTex != null)
        {
            _loadTexture.gradientTex = SetGradientMapsToTexture(_restoreGradients);
            _loadTexture.isCached = false;
            _soGradient.ApplyModifiedProperties();
            _soGradient.Update();
            Debug.Log("Switch To PreviewMode");
        }

        if (changeCheck.changed)
        {
            _soGradient.ApplyModifiedProperties();
            _soGradient.Update();
        }
    }

    private void DrawGradientMapGUI_EditMode()
    {
        using var changeCheck = new EditorGUI.ChangeCheckScope();
        // Switch gradients color into the loadingTex when 'EditMode' is enable.
        if (_loadTexture.isCached == false && _loadTexture.gradientTex != null)
        {
            _loadTexture.isCached = true;
            _restoreGradients = SetUserDataToGradientMap(_loadTexture.gradientTex).ToArray();
            _loadTexture.gradientTex = SetGradientMapsToTexture(_gradient.ToArray());
            Debug.Log("Switch To EditoMode");
        }
        
        DrawConstantList();
        
        if (changeCheck.changed)
        {
            _soGradient.ApplyModifiedProperties();
            _loadTexture.gradientTex = SetGradientMapsToTexture(_gradient.ToArray());
        }
    }

    private void DrawConstantList()
    {
        _spGradient.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_spGradient.isExpanded, "Gradients");
        if (_spGradient.isExpanded)
        {
            EditorGUI.indentLevel++;

            for (int i = 0; i < _gradient.Count; i++)
            {
                _gradient[i] = EditorGUILayout.GradientField($"Element {i}", _gradient[i]);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private List<Gradient> SetUserDataToGradientMap(Texture2D texture)
    {
        var textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
        if (textureImporter == null)
            return default;
        var userData = textureImporter.userData;

        var spiltTextures = userData.Split("\n");
        int textureCounts = spiltTextures.Length;
        List<Gradient> restoreGradientMap = new List<Gradient>();
        for (var index = 0; index < textureCounts; index++)
        {
            var gradientData = spiltTextures[index];
            if (!string.IsNullOrEmpty(gradientData))
            {
                gradientData = gradientData.Substring($"Tex{index}: ".Length);
                var splitColorData = gradientData.Split(';');
                for (var step = 0; step < splitColorData.Length; step += 3)
                {
                    var splitColors = splitColorData[step];
                    var splitColor = splitColors.Split(',');
                    var colorKeys = new List<GradientColorKey>();
                    for (int i = 0; i < splitColor.Length; i += 2)
                    {
                        colorKeys.Add(new GradientColorKey(HexToColor(splitColor[i]), float.Parse(splitColor[i + 1])));
                    }

                    var spiltAlphas = splitColorData[step + 1];
                    var spiltAlpha = spiltAlphas.Split(',');
                    var alphaKeys = new List<GradientAlphaKey>();
                    for (int i = 0; i < spiltAlpha.Length; i += 2)
                    {
                        alphaKeys.Add(new GradientAlphaKey(float.Parse(spiltAlpha[i]), float.Parse(spiltAlpha[i + 1])));
                    }

                    var spiltBlendMode = (GradientMode)Enum.Parse(typeof(GradientMode), splitColorData[step + 2]);

                    restoreGradientMap.Add(new Gradient()
                    {
                        colorKeys = colorKeys.ToArray(),
                        alphaKeys = alphaKeys.ToArray(),
                        mode = spiltBlendMode
                    });
                }
            }
        }

        return restoreGradientMap;
    }

    private string SetGradientToUserData(params Gradient[] gradients)
    {
        string gradientData = String.Empty;

        for (var index = 0; index < gradients.Length; index++)
        {
            var gradient = gradients[index];
            gradientData += $"Tex{index}: ";

            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                gradientData += $"{ColorToHex(gradient.colorKeys[i].color)},{gradient.colorKeys[i].time},";
            }

            gradientData = gradientData.TrimEnd(',');
            gradientData += ";";

            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                gradientData += $"{gradient.alphaKeys[i].alpha},{gradient.alphaKeys[i].time},";
            }

            gradientData = gradientData.TrimEnd(',');
            gradientData += ";";
            gradientData += $"{gradient.mode}";
            gradientData += "\n";
        }

        return gradientData;
    }

    private string ColorToHex(Color32 color)
    {
        var hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    private Color HexToColor(string hex)
    {
        var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    private void SaveTexture(Texture2D texture)
    {
        // Get assetPath if loadingTex is already exist:
        string loadTexPath = _loadTexture.gradientTex ? Path.GetFullPath(AssetDatabase.GetAssetPath(_loadTexture.gradientTex)) : "Assets";
        string loadTexName = _loadTexture.gradientTex ? Path.GetFileName(loadTexPath) : "GradientMap";

        // Save texture into PNG:
        string saveToAbsolutePath = EditorUtility.SaveFilePanel("Save Gradient Map", loadTexPath, loadTexName, "png");
        if (string.IsNullOrEmpty(saveToAbsolutePath))
            return;

        var encodeToPNG = texture.EncodeToPNG();
        File.WriteAllBytes(saveToAbsolutePath, encodeToPNG);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _loadTexture.isCached = false;

        // Save data in meta:
        string saveToRelativePath = GetRelativePath(saveToAbsolutePath);
        var textureImporter = AssetImporter.GetAtPath(saveToRelativePath) as TextureImporter;
        if (textureImporter == null)
            return;
        textureImporter.isReadable = true;
        textureImporter.mipmapEnabled = false;
        textureImporter.npotScale = TextureImporterNPOTScale.None;
        textureImporter.wrapMode = TextureWrapMode.Clamp;
        textureImporter.filterMode = FilterMode.Bilinear;
        textureImporter.userData = SetGradientToUserData(_gradient.ToArray());
        Debug.Log(textureImporter.userData);
        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings
        {
            format = TextureImporterFormat.RGBA32
        };
        textureImporter.SetPlatformTextureSettings(platformSettings);
        EditorUtility.SetDirty(textureImporter);
        textureImporter.SaveAndReimport();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private string GetRelativePath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath))
            return string.Empty;

        if (absolutePath.StartsWith(Application.dataPath))
        {
            return "Assets" + absolutePath.Substring(Application.dataPath.Length);
        }

        Debug.LogError("The path is outside the Assets folder.");
        return string.Empty;
    }

    private Texture2D SetGradientMapsToTexture(params Gradient[] gradients)
    {
        int counts = gradients.Length;
        int minumanTexSize = 4;
        int texWidth = 64;
        int texHeight = minumanTexSize * counts;
        Texture2D tempTexture;
        if (_loadTexture.gradientTex == null)
            tempTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        else
            tempTexture = _loadTexture.gradientTex;

        for (int i = 0; i < counts; i++)
        {
            int drawCurrentHeight = 4 * i;
            for (int y = 0; y < minumanTexSize; y++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    float currentColor = x / (float)(texWidth - 1);
                    tempTexture.SetPixel(x, (texHeight - 1) - (y + drawCurrentHeight), gradients[i].Evaluate(currentColor));
                }
            }
        }

        tempTexture.Apply();
        return tempTexture;
    }
    
    private Texture2D SetGradientMapsToTexture_FinalSave(params Gradient[] gradients)
    {
        int counts = gradients.Length;
        int minumanTexSize = 4;
        int texWidth = 64;
        int texHeight = minumanTexSize * counts;
        Texture2D tempTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);

        for (int i = 0; i < counts; i++)
        {
            int drawCurrentHeight = 4 * i;
            for (int y = 0; y < minumanTexSize; y++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    float currentColor = x / (float)(texWidth - 1);
                    tempTexture.SetPixel(x, (texHeight - 1) - (y + drawCurrentHeight), gradients[i].Evaluate(currentColor));
                }
            }
        }

        tempTexture.Apply();
        return tempTexture;
    }

    private void DrawSaveOrLoadGUI()
    {
        _loadTexture.gradientTex = (Texture2D)EditorGUILayout.ObjectField("Texture", _loadTexture.gradientTex, typeof(Texture2D), false);
        GUILayout.BeginHorizontal();
        {
            using (new EditorGUI.DisabledScope(_gradient.Count == 0))
            {
                if (GUILayout.Button("Save"))
                {
                    SaveTexture(SetGradientMapsToTexture_FinalSave(_gradient.ToArray()));
                }
            }

            using (new EditorGUI.DisabledScope(_loadTexture.gradientTex == null))
            {
                if (GUILayout.Button("Load"))
                {
                    _gradient = SetUserDataToGradientMap(_loadTexture.gradientTex);
                    _soGradient.ApplyModifiedProperties();
                    _soGradient.Update();
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    class GradientTexture
    {
        public Texture2D gradientTex;
        public bool isCached;
    }
}