using UnityEditor;
using UnityEngine;
    
namespace Shader.Editor
{
    public class AnimeToonShaderEditor : ShaderGUI
    {
        // Editor Properties
        private MaterialEditor _materialEditor { get; set; }
        private AnimeToonShaderProperty _animeToonShaderProperty { get; set; }
        private Material _material { get; set; }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // Check if multiple materials are selected
            if (materialEditor.targets.Length > 1)
            {
                EditorGUILayout.HelpBox("Multi-material editing is not supported.", MessageType.Warning);
                return; // Exit without rendering the custom GUI
            }

            _materialEditor = materialEditor;
            _animeToonShaderProperty = new AnimeToonShaderProperty(properties);
            _material = _materialEditor.target as Material;

            DrawEntireGUI(_animeToonShaderProperty);
            _materialEditor.RenderQueueField();
        }

        private void DrawEntireGUI(in AnimeToonShaderProperty shaderProperty)
        {
            DrawMainTexGUI(shaderProperty);
            DrawOutlineGUI(shaderProperty);
            DrawLightMapGUI(shaderProperty);
            DrawEmissionMapGUI(shaderProperty);
            DrawNormalMapGUI(shaderProperty);
            DrawLightGUI(shaderProperty);
            DrawDepthRimGUI(shaderProperty);
            DrawBlendSettingGUI(shaderProperty);
            DrawStencilSettingGUI(shaderProperty);
            DrawDebugModeGUI(shaderProperty);
            DrawAdditionalSettingGUI(shaderProperty);
        }

        private void DrawMainTexGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawTitleGUI("MainTex", MatGUI.LeftTitleGUIStyle);
            MatGUI.DrawLineGUI();
            GUILayout.BeginVertical("helpbox");
            {
                MatGUI.DrawTexturePropertyGUI(_materialEditor, shaderProperty._MainTex, shaderProperty._MainTex.displayName);
                _materialEditor.ShaderProperty(shaderProperty._Color, shaderProperty._Color.displayName);
            }
            GUILayout.EndVertical(); 
        }

        private void DrawOutlineGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawFunctionTitleGUI(_materialEditor, shaderProperty._EnableOutline, _material, "Outline");
            MatGUI.DrawLineGUI();
            MatGUI.SetEnablePass(_material, shaderProperty._EnableOutline.floatValue, "AnimeToon_CharacterOutline");
            if ((MatGUI.BaseFunctionFlag)shaderProperty._EnableOutline.floatValue == MatGUI.BaseFunctionFlag.Disable)
                return;

            GUILayout.BeginVertical("helpbox");
            {
                _materialEditor.ShaderProperty(shaderProperty._OutlineNormalSource, shaderProperty._OutlineNormalSource.displayName);
                _materialEditor.ShaderProperty(shaderProperty._OutlineColor, shaderProperty._OutlineColor.displayName);
                _materialEditor.ShaderProperty(shaderProperty._OutlineWidth, shaderProperty._OutlineWidth.displayName);
                _materialEditor.ShaderProperty(shaderProperty._EnableReceiveLightColor, shaderProperty._EnableReceiveLightColor.displayName);
                _materialEditor.ShaderProperty(shaderProperty._OutlineImpactedByVertexColor, shaderProperty._OutlineImpactedByVertexColor.displayName);
            }
            GUILayout.EndVertical();
        }

        private void DrawLightMapGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawFunctionTitleGUI(_materialEditor, shaderProperty._EnableLightMap, _material, "LightMap");
            MatGUI.DrawLineGUI();
            if ((MatGUI.BaseFunctionFlag)shaderProperty._EnableLightMap.floatValue == MatGUI.BaseFunctionFlag.Disable)
                return;

            GUILayout.BeginVertical("helpbox");
            {
                shaderProperty._LightMapMode.floatValue = GUILayout.Toolbar((int)shaderProperty._LightMapMode.floatValue, new[] { "Normal", "Face SDF" });

                switch (shaderProperty._LightMapMode.floatValue)
                {
                    case 0: // Normal
                        MatGUI.DrawTexturePropertyGUI(_materialEditor, shaderProperty._LightMap, shaderProperty._LightMap.displayName);
                        MatGUI.DrawTexturePropertyGUI(_materialEditor, shaderProperty._LightMap_GradientColor, shaderProperty._LightMap_GradientColor.displayName);
                        MatGUI.DrawTexturePropertyGUI(_materialEditor, shaderProperty._MatCapTex, shaderProperty._MatCapTex.displayName);
                        _materialEditor.ShaderProperty(shaderProperty._Metal, shaderProperty._Metal.displayName);
                        break;
                    case 1: // Face SDF
                        MatGUI.DrawTexturePropertyGUI(_materialEditor, shaderProperty._FaceShadowMap, shaderProperty._FaceShadowMap.displayName);
                        _materialEditor.ShaderProperty(shaderProperty._FaceShadowRange, shaderProperty._FaceShadowRange.displayName);
                        MatGUI.DrawTexturePropertyGUI(_materialEditor, shaderProperty._LightMap_GradientColor, shaderProperty._LightMap_GradientColor.displayName);
                        MatGUI.DrawTexturePropertyGUI(_materialEditor, shaderProperty._MatCapTex, shaderProperty._MatCapTex.displayName);
                        _materialEditor.ShaderProperty(shaderProperty._Metal, shaderProperty._Metal.displayName);
                        break;
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawEmissionMapGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawFunctionTitleGUI(_materialEditor, shaderProperty._EnableEmissionMap, _material, "EmissionMap");
            MatGUI.DrawLineGUI();
            if ((MatGUI.BaseFunctionFlag)shaderProperty._EnableEmissionMap.floatValue == MatGUI.BaseFunctionFlag.Disable)
                return;

            GUILayout.BeginVertical("helpbox");
            {
                MatGUI.DrawTexturePropertyGUI(_materialEditor, shaderProperty._EmissionMap, shaderProperty._EmissionMap.displayName);
                _materialEditor.ShaderProperty(shaderProperty._EmissionColor, shaderProperty._EmissionColor.displayName);
            }
            GUILayout.EndVertical();
        }

        private void DrawNormalMapGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawFunctionTitleGUI(_materialEditor, shaderProperty._EnableNormalMap, _material, "NormalMap");
            MatGUI.DrawLineGUI();
            if ((MatGUI.BaseFunctionFlag)shaderProperty._EnableNormalMap.floatValue == MatGUI.BaseFunctionFlag.Disable)
                return;

            GUILayout.BeginVertical("helpbox");
            {
                MatGUI.DrawTexturePropertyGUI(_materialEditor, shaderProperty._NormalMap, shaderProperty._NormalMap.displayName);
                _materialEditor.ShaderProperty(shaderProperty._NormalIntensity, shaderProperty._NormalIntensity.displayName);
            }
            GUILayout.EndVertical();
        }

        private void DrawLightGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawTitleGUI("Light", MatGUI.LeftTitleGUIStyle);
            MatGUI.DrawLineGUI();
            GUILayout.BeginVertical("helpbox");
            {
                if ((AnimeToonShaderLightMode)shaderProperty._LightMapMode.floatValue == AnimeToonShaderLightMode.FaceShadowSDF)
                    EditorGUILayout.HelpBox("If your lightMap's lightMode is choosing the 'Face SDF', the 'MainLightShadowRange' should be set to 0." +
                                            " \r\nOtherwise these two shadow calculation will be mix together.", MessageType.Info);
                _materialEditor.ShaderProperty(shaderProperty._MainLightShadowRange, shaderProperty._MainLightShadowRange.displayName);
                _materialEditor.ShaderProperty(shaderProperty._AdditionalLightShadowRange, shaderProperty._AdditionalLightShadowRange.displayName);
                MatGUI.DrawVector2SliderGUI(shaderProperty._AdditionalLightClipRange, shaderProperty._AdditionalLightClipRange.displayName);
            }
            GUILayout.EndVertical();
        }

        private void DrawDepthRimGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawFunctionTitleGUI(_materialEditor, shaderProperty._EnableDepthRim, _material, "Depth-Rim");
            MatGUI.DrawLineGUI();
            if ((MatGUI.BaseFunctionFlag)shaderProperty._EnableDepthRim.floatValue == MatGUI.BaseFunctionFlag.Disable)
                return;

            GUILayout.BeginVertical("helpbox");
            {
                _materialEditor.ShaderProperty(shaderProperty._DepthRimMode, shaderProperty._DepthRimMode.displayName);
                MatGUI.DrawVector2GUI(shaderProperty._OffsetDepthRim, shaderProperty._OffsetDepthRim.displayName);
                _materialEditor.ShaderProperty(shaderProperty._RimColor, shaderProperty._RimColor.displayName);
            }
            GUILayout.EndVertical();
        }

        private void DrawBlendSettingGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawTitleGUI("Blend Setting", MatGUI.LeftTitleGUIStyle);
            MatGUI.DrawLineGUI();

            var renderMode = (AnimeToonShaderRenderMode)shaderProperty._RenderMode.floatValue;

            GUILayout.BeginVertical("helpbox");
            {
                _materialEditor.ShaderProperty(shaderProperty._RenderMode, shaderProperty._RenderMode.displayName);
                using (new EditorGUI.DisabledScope(renderMode != AnimeToonShaderRenderMode.Custom))
                {
                    _materialEditor.ShaderProperty(shaderProperty._SourceBlend, shaderProperty._SourceBlend.displayName);
                    _materialEditor.ShaderProperty(shaderProperty._DestBlend, shaderProperty._DestBlend.displayName);
                }

                _materialEditor.ShaderProperty(shaderProperty._Cull, shaderProperty._Cull.displayName);
                using (new EditorGUI.DisabledScope(renderMode == AnimeToonShaderRenderMode.Opaque))
                {
                    _materialEditor.ShaderProperty(shaderProperty._ZWrite, shaderProperty._ZWrite.displayName);
                }

                _materialEditor.ShaderProperty(shaderProperty._ZTest, shaderProperty._ZTest.displayName);
            }
            GUILayout.EndVertical();

            switch (renderMode)
            {
                case AnimeToonShaderRenderMode.Opaque:
                    shaderProperty._SourceBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.One;
                    shaderProperty._DestBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.Zero;
                    shaderProperty._ZWrite.floatValue = 1; // Means 'On'
                    _material.renderQueue = 2000;
                    break;
                case AnimeToonShaderRenderMode.Transparent:
                    shaderProperty._SourceBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
                    shaderProperty._DestBlend.floatValue = (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                    _material.renderQueue = 3000;
                    break;
            }
        }

        private void DrawStencilSettingGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawTitleGUI("Stencil Setting", MatGUI.LeftTitleGUIStyle);
            MatGUI.DrawLineGUI();

            GUILayout.BeginVertical("helpbox");
            {
                _materialEditor.ShaderProperty(shaderProperty._Ref, shaderProperty._Ref.displayName);
                _materialEditor.ShaderProperty(shaderProperty._Comp, shaderProperty._Comp.displayName);
                _materialEditor.ShaderProperty(shaderProperty._Pass, shaderProperty._Pass.displayName);
                _materialEditor.ShaderProperty(shaderProperty._Fail, shaderProperty._Fail.displayName);
                _materialEditor.ShaderProperty(shaderProperty._ZFail, shaderProperty._ZFail.displayName);
            }
            GUILayout.EndVertical();
        }

        private void DrawDebugModeGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawTitleGUI("Debug Mode", MatGUI.LeftTitleGUIStyle);
            MatGUI.DrawLineGUI();

            GUILayout.BeginVertical("helpbox");
            {
                _materialEditor.ShaderProperty(shaderProperty._ShowVertexColor, shaderProperty._ShowVertexColor.displayName);
            }
            GUILayout.EndVertical();
        }

        private void DrawAdditionalSettingGUI(in AnimeToonShaderProperty shaderProperty)
        {
            MatGUI.DrawTitleGUI("Additional Setting", MatGUI.LeftTitleGUIStyle);
            MatGUI.DrawLineGUI();
            
            // Eye Through Hair
            GUILayout.BeginVertical("helpbox");
            {
                MatGUI.DrawTitleGUI("Eye Through Hair", MatGUI.MiddleTitleGUIStyle);
                EditorGUILayout.HelpBox("◆ This function need to enable the 'Eye Through Hair' feature in 'Anime Toon Features' with URP pipeline." +
                                        "\r\n◆ If this material is for 'Face', Turn the 'Write Eye Color' on." +
                                        "\r\n◆ If this material is for 'Hair', Turn the 'Read Eye Color' on." +
                                        "\r\n◆ Use G channel of faceMap to mask the extra eye rendering." +
                                        "\r\n◆ Orthographic mode is not compatible." +
                                        "\r\n◆ RootBone needs to be compatible with unity leftHand coordinate system.",MessageType.Info);  
                GUILayout.BeginHorizontal();    
                {
                    MatGUI.DrawToggleButtonGUI(shaderProperty._ReadEyeColor, shaderProperty._ReadEyeColor.displayName);
                    MatGUI.DrawToggleButtonGUI(shaderProperty._WriteEyeColor, shaderProperty._WriteEyeColor.displayName);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical("box");
                {
                    if (shaderProperty._WriteEyeColor.floatValue > 0)
                    {
                        _materialEditor.ShaderProperty(shaderProperty._WriteEyeColor_Intensity, shaderProperty._WriteEyeColor_Intensity.displayName);
                        _materialEditor.ShaderProperty(shaderProperty._WriteEyeColor_FadeOutRange, shaderProperty._WriteEyeColor_FadeOutRange.displayName);
                    }
                }
                GUILayout.EndVertical();
                
            }
            GUILayout.EndVertical();
            MatGUI.SetEnablePass(_material, shaderProperty._WriteEyeColor.floatValue, "AnimeToon_EyeThroughHair");
            MatGUI.SetEnablePass(_material, shaderProperty._WriteHairDepth.floatValue, "AnimeToon_CasterHairShadow");

            // Hair Shadow Caster
            GUILayout.BeginVertical("helpbox");
            {
                MatGUI.DrawTitleGUI("Hair Shadow Caster", MatGUI.MiddleTitleGUIStyle);
                EditorGUILayout.HelpBox("◆ This function need to enable the 'Depth Hair Caster' feature in 'Anime Toon Features' with URP pipeline." +
                                        "\r\n◆ If this material is for 'Face', Turn the 'Read Hair Depth' on." +
                                        "\r\n◆ If this material is for 'Hair', Turn the 'Write Hair Depth' on." +
                                        "\r\n◆ Orthographic mode is not compatible." +
                                        "\r\n◆ RootBone needs to be compatible with unity leftHand coordinate system.",MessageType.Info);  
                GUILayout.BeginHorizontal();
                {
                    MatGUI.DrawToggleButtonGUI(shaderProperty._ReadHairDepth, shaderProperty._ReadHairDepth.displayName);
                    MatGUI.DrawToggleButtonGUI(shaderProperty._WriteHairDepth, shaderProperty._WriteHairDepth.displayName);
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical("box");
                {
                    if (shaderProperty._ReadHairDepth.floatValue > 0)
                    {
                        MatGUI.DrawVector2GUI(shaderProperty._DepthHairShadowOffset, shaderProperty._DepthHairShadowOffset.displayName);
                        _materialEditor.ShaderProperty(shaderProperty._DepthHairColor, shaderProperty._DepthHairColor.displayName);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        enum AnimeToonShaderRenderMode
        {
            Opaque,
            Transparent,
            Custom
        }

        enum AnimeToonShaderLightMode
        {
            Normal,
            FaceShadowSDF
        }

        private struct AnimeToonShaderProperty
        {
            // Main Texture
            public MaterialProperty _MainTex;
            public MaterialProperty _Color;

            // Outline Setting
            public MaterialProperty _EnableOutline;
            public MaterialProperty _OutlineNormalSource;
            public MaterialProperty _OutlineColor;
            public MaterialProperty _OutlineWidth;
            public MaterialProperty _EnableReceiveLightColor;
            public MaterialProperty _OutlineImpactedByVertexColor;

            // Face ShadowMap
            public MaterialProperty _FaceShadowMap;
            public MaterialProperty _FaceShadowRange;
            public MaterialProperty _ReadEyeColor;
            public MaterialProperty _ReadHairDepth;
            public MaterialProperty _DepthHairShadowOffset;
            public MaterialProperty _DepthHairColor;

            // LightMap
            public MaterialProperty _LightMapMode;
            public MaterialProperty _EnableLightMap;
            public MaterialProperty _LightMap;
            public MaterialProperty _Metal;
            public MaterialProperty _LightMap_GradientColor;
            public MaterialProperty _MatCapTex;

            // Emission
            public MaterialProperty _EnableEmissionMap;
            public MaterialProperty _EmissionMap;
            public MaterialProperty _EmissionColor;

            // Normal
            public MaterialProperty _EnableNormalMap;
            public MaterialProperty _NormalMap;
            public MaterialProperty _NormalIntensity;

            // Light
            public MaterialProperty _MainLightShadowRange;
            public MaterialProperty _AdditionalLightShadowRange;
            public MaterialProperty _AdditionalLightClipRange;

            // Depth-Rim
            public MaterialProperty _EnableDepthRim;
            public MaterialProperty _DepthRimMode;
            public MaterialProperty _OffsetDepthRim;
            public MaterialProperty _RimColor;

            // Additional Setting (Depth Hair Caster)
            public MaterialProperty _WriteHairDepth;
            // Additional Setting (Eye Through Hair)
            public MaterialProperty _WriteEyeColor_FadeOutRange;
            public MaterialProperty _WriteEyeColor_Intensity;
            public MaterialProperty _WriteEyeColor;
            
            // Blend Setting
            public MaterialProperty _RenderMode;
            public MaterialProperty _SourceBlend;
            public MaterialProperty _DestBlend;
            public MaterialProperty _Cull;
            public MaterialProperty _ZWrite;
            public MaterialProperty _ZTest;

            // Stencil Setting
            public MaterialProperty _Ref;
            public MaterialProperty _Comp;
            public MaterialProperty _Pass;
            public MaterialProperty _Fail;
            public MaterialProperty _ZFail;

            // Debug Mode
            public MaterialProperty _ShowVertexColor;

            public AnimeToonShaderProperty(MaterialProperty[] properties)
            {
                // Main Texture
                _MainTex = FindProperty("_MainTex", properties);
                _Color = FindProperty("_Color", properties);

                // Outline Setting
                _EnableOutline = FindProperty("_EnableOutline", properties);
                _OutlineNormalSource = FindProperty("_OutlineNormalSource", properties);
                _OutlineColor = FindProperty("_OutlineColor", properties);
                _OutlineWidth = FindProperty("_OutlineWidth", properties);
                _EnableReceiveLightColor = FindProperty("_EnableReceiveLightColor", properties);
                _OutlineImpactedByVertexColor = FindProperty("_OutlineImpactedByVertexColor", properties);

                // LightMap 
                _LightMapMode = FindProperty("_LightMapMode", properties);
                _EnableLightMap = FindProperty("_EnableLightMap", properties);
                _LightMap = FindProperty("_LightMap", properties);
                _Metal = FindProperty("_Metal", properties);
                _LightMap_GradientColor = FindProperty("_LightMap_GradientColor", properties);
                _MatCapTex = FindProperty("_MatCapTex", properties);
                // LightMap (Face ShadowMap) 
                _FaceShadowMap = FindProperty("_FaceShadowMap", properties);
                _FaceShadowRange = FindProperty("_FaceShadowRange", properties);
                _ReadEyeColor = FindProperty("_ReadEyeColor", properties);
                _ReadHairDepth = FindProperty("_ReadHairDepth", properties);
                _DepthHairShadowOffset = FindProperty("_DepthHairShadowOffset", properties);
                _DepthHairColor = FindProperty("_DepthHairColor", properties);

                // Emission
                _EnableEmissionMap = FindProperty("_EnableEmissionMap", properties);
                _EmissionMap = FindProperty("_EmissionMap", properties);
                _EmissionColor = FindProperty("_EmissionColor", properties);

                // Normal
                _EnableNormalMap = FindProperty("_EnableNormalMap", properties);
                _NormalMap = FindProperty("_NormalMap", properties);
                _NormalIntensity = FindProperty("_NormalIntensity", properties);

                // Light
                _MainLightShadowRange = FindProperty("_MainLightShadowRange", properties);
                _AdditionalLightShadowRange = FindProperty("_AdditionalLightShadowRange", properties);
                _AdditionalLightClipRange = FindProperty("_AdditionalLightClipRange", properties);

                // Depth-Rim
                _EnableDepthRim = FindProperty("_EnableDepthRim", properties);
                _DepthRimMode = FindProperty("_DepthRimMode", properties);
                _OffsetDepthRim = FindProperty("_OffsetDepthRim", properties);
                _RimColor = FindProperty("_RimColor", properties);

                // Additional Setting (Depth Hair Caster)
                _WriteHairDepth = FindProperty("_WriteHairDepth", properties);
                // Additional Setting (Eye Through Hair)
                _WriteEyeColor_FadeOutRange = FindProperty("_WriteEyeColor_FadeOutRange", properties);
                _WriteEyeColor_Intensity = FindProperty("_WriteEyeColor_Intensity", properties);
                _WriteEyeColor = FindProperty("_WriteEyeColor", properties);


                // Blend Setting
                _RenderMode = FindProperty("_RenderMode", properties);
                _SourceBlend = FindProperty("_SourceBlend", properties);
                _DestBlend = FindProperty("_DestBlend", properties);
                _Cull = FindProperty("_Cull", properties);
                _ZWrite = FindProperty("_ZWrite", properties);
                _ZTest = FindProperty("_ZTest", properties);

                // Stencil Setting
                _Ref = FindProperty("_Ref", properties);
                _Comp = FindProperty("_Comp", properties);
                _Pass = FindProperty("_Pass", properties);
                _Fail = FindProperty("_Fail", properties);
                _ZFail = FindProperty("_ZFail", properties);

                // Debug Mode
                _ShowVertexColor = FindProperty("_ShowVertexColor", properties);
            }
        }
    }
}