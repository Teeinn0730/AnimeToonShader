Shader "TNLab/AnimeToonShader"
{
    Properties
    {
        // Main Texture
        _MainTex ("Main Texture", 2D) = "white" {}
        [HDR] _Color ("Main Color", Color) = (1,1,1,1)
        [Toggle(ENABLE_ALPHACLIP)] _EnableAlphaClip ("Enable Alpha Clip", Float) = 0

        // Outline Setting
        [Enum(Off, 0, On, 1)] _EnableOutline ("Enable Outline", Float) = 0
        [Enum(MeshNormal, 0, CustomNormal(UV4), 1)] _OutlineNormalSource ("Outline Normal Source", Float) = 1
        [HDR] _OutlineColor ("Outline Color (Lerp Mode)" , Color) = (0,0,0,0.5)
        _OutlineWidth ("Outline Width" , Range(0,10)) = 1
        [Enum(Off, 0, On, 1)] _EnableReceiveLightColor ("Receive Light Color", Float) = 0
        [Enum(None, 0, R, 1, G, 2, B, 3, A, 4)]_OutlineImpactedByVertexColor ("Impacted By Vertex Color", Float) = 0

        // LightMap (Normal)
        [Enum(Off, 0, On, 1)] _EnableLightMap ("Enable LightMap", Float) = 0
        [Enum(Normal, 0, FaceSDF, 1)] _LightMapMode ("LightMap Mode", Float) = 0
        _LightMap ("LightMap (R: Shadow/ G:None /B:Metal(Matcap) /A: Gradient Color Index)", 2D) = "white"{}
        _LightMap_GradientColor ("LightMap - Gradient Color" , 2D) = "white"{}
        _MatCapTex ("MatCap Tex" , 2D) = "white"{}
        _Metal ("Metal", Range(0,1))= 0.5
        // LightMap (Face Shadow SDF)
        _FaceShadowMap ("Face ShadowMap(R: Shadow/ G:EyePassMask /B: /A:)", 2D) = "white"{}
        _FaceShadowRange ("Face Shadow Range", Range(0, 1)) = 0.1

        // Emission
        [Enum(Off, 0, On, 1)] _EnableEmissionMap ("Enable Emission Map", Float) = 0
        _EmissionMap ("Emission Map", 2D) = "white"{}
        [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)

        // Normal
        [Enum(Off, 0, On, 1)] _EnableNormalMap ("Enable NormalMap", Float) = 0
        _NormalMap ("NormalMap" , 2D) = "bump"{}
        _NormalIntensity ("Normal Intensity", Range(0, 10)) = 1

        // Light
        _MainLightShadowRange ("MainLight Shadow Range", Range(0,1))= 0.5
        _AdditionalLightShadowRange ("AdditionalLight Shadow Range", Range(0,1))= 0.5
        _AdditionalLightClipRange ("AdditionalLight Clip Range", Vector) = (0,1,0,0)

        // Depth-Rim 
        [Enum(Off, 0, On, 1)] _EnableDepthRim ("Enable Depth-Rim", Float) = 0
        [Enum(Additive, 0, Multiply, 1, Replace, 2)] _DepthRimMode ("Depth Rim Mode", Float) = 0
        _OffsetDepthRim ("Offset Depth Rim", Vector) = (0.2,0.2,0,0)
        [HDR] _RimColor ("Rim Color", Color) = (1,1,1,1)

        // Additional Setting (Eye Through Hair)
        [Enum(Off, 0, On, 1)] _WriteEyeColor ("Write Eye Color", Float) = 0
        _WriteEyeColor_Intensity ("Eye PassThrough Hair Intensity", Range(0,1)) = 1
        [IntRange] _WriteEyeColor_FadeOutRange ("Eye Color FadeOut Range", Range(1,10)) = 1
        [Enum(Off, 0, On, 1)] _ReadEyeColor ("Read Eye Color", Float) = 0
        // Additional Setting (Hair Shadow Caster)
        [Enum(Off, 0, On, 1)] _WriteHairDepth ("Write Hair Depth", Float) = 0
        [Enum(Off, 0, On, 1)] _ReadHairDepth ("Read Hair Depth", Float) = 0
        _DepthHairShadowOffset ("Depth Hair Shadow Offset", Vector) = (0.1,0.1,0,0)
        _DepthHairColor ("Depth Hair Color", Color) = (1,1,1,1)

        // Blend Setting
        [Enum(Opaque,0,Transparent,1,Custom,2)] _RenderMode ("Render Mode", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend ("SrcBlend",Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestBlend ("DestBlend",Float) = 10
        [Enum(Off,0,Front,1,Back,2)] _Cull("CullMask",Float) = 0
        [Enum(Off,0,On,1)] _ZWrite("Zwrite",Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest",Float) = 2

        // Stencil Setting
        _Ref ("Ref",Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp ("Comparison",Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass ("Pass ",Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _Fail ("Fail ",Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _ZFail ("ZFail ",Float) = 0

        // Debug Mode
        [Enum(None, 0, R, 1, G, 2, B, 3, A, 4, RGB, 5)] _ShowVertexColor("Show Vertex Color (Debug)", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
            "Queue" = "Geometry"
        }

        Stencil
        {
            Ref [_Ref]
            Comp [_Comp]
            Pass [_Pass]
            Fail [_Fail]
            ZFail [_ZFail]
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        struct VertexInput
        {
            float4 vertex : POSITION;
            half4 vertexColor : COLOR;
            float2 uv : TEXCOORD0;
            float3 normal : NORMAL;
            float4 tangent : TANGENT;
            float3 smoothNormal : TEXCOORD4;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct VertexOutput
        {
            float4 positionCS : SV_POSITION;
            half4 vertexColor : COLOR;
            float3 normal : NORMAL;
            float4 tangent : TANGENT;
            float2 uv : TEXCOORD0;
            float3 positionOS : TEXCOORD1;
            float3 viewDirTS : TEXCOORD2; // DepthNormals
            half3 vertexSH : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        float InverseLerp(float min, float max, float val)
        {
            return saturate((val - min) / (max - min));
        }

        float GetCenterPointToCamDist()
        {
            half3 ViewPointDir = _WorldSpaceCameraPos.xyz - unity_ObjectToWorld._m03_m13_m23;
            return length(ViewPointDir);
        }

        float GetWorldPosToCamDist(float3 positionWS)
        {
            half3 ViewPointDir = _WorldSpaceCameraPos.xyz - positionWS;
            return length(ViewPointDir);
        }

        float GetCameraFOV()
        {
            //https://answers.unity.com/questions/770838/how-can-i-extract-the-fov-information-from-the-pro.html
            float t = unity_CameraProjection._m11;
            float Rad2Deg = 180 / PI;
            float fov = atan(1.0f / t) * 2.0 * Rad2Deg;
            return fov;
        }

        float FixOutline(float3 positionVS)
        {
            const float factor = 0.00005f;

            if (unity_OrthoParams.w == 0)
            {
                float CameraDist = abs(positionVS).z;
                CameraDist = log(CameraDist + 1);
                return clamp(GetCameraFOV() * factor, 0.001f, 1) * CameraDist;
            }
            else
            {
                return 20 * factor;
            }
        }

        float2 FixDepthRim(in half2 offsetDepthRim, in half CameraDist)
        {
            half2 OffsetDepth;
            if (unity_OrthoParams.w == 0)
                OffsetDepth = offsetDepthRim.xy * CameraDist.xx / GetCameraFOV();
            else
                OffsetDepth = offsetDepthRim.xy / GetCameraFOV();

            return OffsetDepth;
        }

        float3 CalcOutlineVectorOS(in float3 outlineNormal, in float3 normalOS, in float4 tangentOS)
        {
            float3 bitangentOS = normalize(cross(normalOS, tangentOS.xyz) * tangentOS.w) * length(tangentOS.xyz);
            float3 outlineVectorTS = outlineNormal;
            float3 outlineVector = outlineVectorTS.x * tangentOS.xyz + outlineVectorTS.y * bitangentOS + outlineVectorTS.z * normalOS;
            return outlineVector;
        }

        float3 CalcOutlineVectorWS(in float3 outlineNormal, in float3 normalOS, in float4 tangentOS)
        {
            float3 normalWS = TransformObjectToWorldNormal(normalOS);
            float3 tangentWS = TransformObjectToWorldDir(tangentOS.xyz);
            float3 bitangentWS = cross(normalWS, tangentWS.xyz) * tangentOS.w * unity_WorldTransformParams.w;

            //float3 outlineVectorTS = color.rgb * 2.0 - 1.0;
            float3 outlineVector = outlineNormal.x * tangentWS.xyz + outlineNormal.y * bitangentWS + outlineNormal.z * normalWS;
            return outlineVector;
        }

        CBUFFER_START(UnityPerMaterial)
        // Shared
        SamplerState linear_repeat_sampler, linear_clamp_sampler, point_repeat_sampler, point_clamp_sampler;

        // MainTex
        Texture2D _MainTex;
        half4 _MainTex_ST, _Color;

        // Outline
        half4 _OutlineColor;
        half _OutlineWidth, _OutlineImpactedByVertexColor;
        uint _EnableReceiveLightColor, _OutlineNormalSource;

        // ScreenSpace Outline
        half _ScreenSpaceOutline_Width;

        // Emission
        Texture2D _EmissionMap;
        half4 _EmissionColor, _EmissionMap_ST;
        uint _EnableEmissionMap;

        // Eye-PassThrough Hair
        Texture2D _AnimeToon_EyeThroughHair_Color;
        half _WriteEyeColor_Intensity, _WriteEyeColor_FadeOutRange;
        uint _ReadEyeColor;

        // Depth Hair Shadow
        uint _ReadHairDepth;
        half4 _DepthHairShadowOffset;

        // Light Setting
        Texture2D _LightMap_GradientColor, _LightMap, _MatCapTex;
        half4 _AdditionalLightClipRange;
        half _MainLightShadowRange, _AdditionalLightShadowRange, _Metal;
        uint _EnableLightMap, _LightMapMode;
        // Light Setting (Face Shadow SDF)
        Texture2D _FaceShadowMap;
        half _FaceShadowRange;

        // Depth-Rim
        Texture2D _CameraDepthTexture;
        half4 _OffsetDepthRim, _RimColor;
        uint _EnableDepthRim, _DepthRimMode;

        // Depth-Hair
        Texture2D _AnimeToon_HairDepthTexture;
        half4 _DepthHairColor;

        // NormalMap
        Texture2D _NormalMap;
        half4 _NormalMap_ST;
        uint _EnableNormalMap;
        half _NormalIntensity;

        // Debug-Mode
        uint _ShowVertexColor;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Tags
            {
                "LightMode" = "AnimeToon_CharacterOutline"
            }

            Blend [_SourceBlend] [_DestBlend]
            ZWrite On
            ZTest LEqual
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(v.vertex.xyz);
                float3 outlineNormal = 1;
                switch (_OutlineNormalSource)
                {
                default:
                    break;
                case 0: // Mesh normal
                    outlineNormal = v.normal;
                    break;
                case 1: // Custom Normal in UV4
                    outlineNormal = CalcOutlineVectorOS(v.smoothNormal.xyz, v.normal, v.tangent);
                    break;
                }
                float3 positionVS = vertexPositionInput.positionVS;
                half outlineMask = _OutlineImpactedByVertexColor == 0 ? 1 : v.vertexColor[_OutlineImpactedByVertexColor - 1];
                float3 positionOS = v.vertex.xyz + outlineNormal * _OutlineWidth * outlineMask * FixOutline(positionVS);
                o.positionCS = TransformObjectToHClip(positionOS);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.vertexColor = v.vertexColor;

                return o;
            }

            half4 frag(VertexOutput o) : SV_Target
            {
                // Sample Main Tex
                half4 SampleMainTex = SAMPLE_TEXTURE2D(_MainTex, linear_repeat_sampler, o.uv);

                // Final Output
                // For more good controllable and art-style , blend outline color with 'Lerp'. 
                half3 FinalColor = lerp(SampleMainTex.rgb * _Color.rgb, _OutlineColor.rgb, _OutlineColor.a);
                half FinalAlpha = SampleMainTex.a * _Color.a;

                // Light
                if (_EnableReceiveLightColor)
                {
                    // Light (Main Light = Direction Light)
                    VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(o.positionOS.xyz);
                    float4 ShadowCoord = GetShadowCoord(vertexPositionInput);
                    Light MainLight = GetMainLight(ShadowCoord);
                    FinalColor *= MainLight.color.rgb;
                    FinalColor *= MainLight.shadowAttenuation;
                }

                return half4(FinalColor, FinalAlpha);
            }
            ENDHLSL
        }
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend[_SourceBlend][_DestBlend]
            ZWrite[_ZWrite]
            ZTest[_ZTest]
            Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.positionOS = v.vertex.xyz;
                o.positionCS = TransformObjectToHClip(o.positionOS);
                o.uv = v.uv;
                o.vertexColor = v.vertexColor;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.vertexSH = SampleSHVertex(o.normal);
                return o;
            }

            half4 frag(VertexOutput o) : SV_Target
            {
                // Debug
                switch (_ShowVertexColor)
                {
                default:
                case 0: // None
                    break;
                case 1: // R
                    return half4(o.vertexColor.rrr, 1);
                case 2: // G
                    return half4(o.vertexColor.ggg, 1);
                case 3: // B
                    return half4(o.vertexColor.bbb, 1);
                case 4: // A
                    return half4(o.vertexColor.aaa, 1);
                case 5: // RGB
                    return half4(o.vertexColor.rgb, 1);
                }

                // Depth Data
                half2 ScreenUV = o.positionCS.xy / _ScreenParams.xy;
                float SampleDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, linear_clamp_sampler, ScreenUV).r;
                half SampleLinear01Depth = Linear01Depth(SampleDepth, _ZBufferParams);
                half SampleObject01Depth = Linear01Depth(o.positionCS.z, _ZBufferParams);

                // Normal Map
                VertexNormalInputs vertexNormalInputs = GetVertexNormalInputs(o.normal, o.tangent);
                o.normal = vertexNormalInputs.normalWS;
                if (_EnableNormalMap)
                {
                    half3x3 tbnMatrix = half3x3(vertexNormalInputs.tangentWS, vertexNormalInputs.bitangentWS, vertexNormalInputs.normalWS);
                    half2 NormalMapUV = o.uv.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
                    half3 SampleNormalMap = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, linear_repeat_sampler, NormalMapUV));
                    SampleNormalMap.xy *= _NormalIntensity;
                    o.normal = mul(SampleNormalMap.xyz, tbnMatrix);
                }

                // Sample Main Tex
                half2 SampleMainTexUV = o.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                half4 SampleMainTex = SAMPLE_TEXTURE2D(_MainTex, linear_repeat_sampler, SampleMainTexUV);
                SampleMainTex *= _Color;

                // ShadowMask
                VertexPositionInputs vertexPositionInput = GetVertexPositionInputs(o.positionOS);
                float4 ShadowCoord = GetShadowCoord(vertexPositionInput);

                // Light (Main Light + Light Data)
                Light MainLight = GetMainLight(ShadowCoord);
                half3 ViewDir = _WorldSpaceCameraPos.xyz - vertexPositionInput.positionWS;
                half3 HalfDir = normalize(ViewDir + MainLight.direction);
                half3 LightDirVS = mul((float3x3)UNITY_MATRIX_V, MainLight.direction);
                ViewDir = normalize(ViewDir);
                half LDotN = saturate(dot(MainLight.direction, o.normal) * _MainLightShadowRange + (1 - _MainLightShadowRange));
                half VDotN = saturate(dot(ViewDir, o.normal));
                half HDotN = saturate(dot(HalfDir, o.normal));

                // Final Output
                half3 FinalColor = SampleMainTex.rgb;
                half FinalAlpha = SampleMainTex.a;

                // Emission
                if (_EnableEmissionMap)
                {
                    half2 SampleEmissionMapUV = o.uv.xy * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
                    half4 SampleEmissionMap = SAMPLE_TEXTURE2D(_EmissionMap, linear_repeat_sampler, SampleEmissionMapUV);
                    FinalColor += SampleEmissionMap.rgb * SampleEmissionMap.a * _EmissionColor.rgb;
                }

                // Light (MainLight)
                FinalColor *= MainLight.color;
                LDotN *= MainLight.shadowAttenuation;

                // Light (Additional Light)
                const uint AdditionalCounts = GetAdditionalLightsCount();
                for (uint index = 0; index < AdditionalCounts; index++)
                {
                    Light AdditionalLight = GetAdditionalLight(index, vertexPositionInput.positionWS, 0);
                    half ADLDotN = saturate(dot(AdditionalLight.direction, o.normal) * _AdditionalLightShadowRange + (1 - _AdditionalLightShadowRange));
                    ADLDotN = InverseLerp(_AdditionalLightClipRange.x, _AdditionalLightClipRange.y, ADLDotN);
                    half3 AdditionalLightVal = ADLDotN * AdditionalLight.color * pow(AdditionalLight.distanceAttenuation, 0.5f) * SampleMainTex.rgb * AdditionalLight.shadowAttenuation;
                    FinalColor += AdditionalLightVal;
                }

                // Depth Hair Shadow
                if (_ReadHairDepth && unity_OrthoParams.w == 0)
                {
                    // Fix shadow of face side with camera rotate
                    half3 ViewPointDir = _WorldSpaceCameraPos.xyz - unity_ObjectToWorld._m03_m13_m23;
                    half VPDotZForward = saturate(dot(normalize(ViewPointDir), unity_ObjectToWorld._m01_m11_m21));
                    VPDotZForward = pow(VPDotZForward, 3);

                    // Fix Depth Offset  
                    float CameraDist = 1 / GetCenterPointToCamDist();
                    _DepthHairShadowOffset.x *= VPDotZForward * LightDirVS.x;
                    _DepthHairShadowOffset.y *= MainLight.direction.y;
                    half2 DepthHairShadowOffset = FixDepthRim(_DepthHairShadowOffset.xy, CameraDist);
                    float SampleOffsetDepth = SAMPLE_TEXTURE2D(_AnimeToon_HairDepthTexture, linear_repeat_sampler, ScreenUV + DepthHairShadowOffset).r;
                    SampleOffsetDepth = Linear01Depth(SampleOffsetDepth, _ZBufferParams);
                    float HairShadow = SampleOffsetDepth - SampleObject01Depth;
                    HairShadow = step(_ProjectionParams.w * 0.01f, HairShadow);
                    FinalColor *= lerp(_DepthHairColor.rgb, 1, HairShadow);
                }

                // Sample LightMap
                if (_EnableLightMap)
                {
                    half4 SampleLightMap = 0;
                    switch (_LightMapMode)
                    {
                    default:
                    case 0: // Normal Light Map
                        SampleLightMap = SAMPLE_TEXTURE2D(_LightMap, linear_repeat_sampler, o.uv);
                        break;
                    case 1: // Face Shadow SDF
                        SampleLightMap = SAMPLE_TEXTURE2D(_FaceShadowMap, linear_repeat_sampler, o.uv);
                        half LightDotForward = dot(MainLight.direction, normalize(unity_ObjectToWorld._m02_m12_m22));
                        half LightDotUp = dot(MainLight.direction, normalize(unity_ObjectToWorld._m01_m11_m21));
                        half2 SampleShadowMapUV = o.uv;
                        if (LightDotForward < 0)
                        {
                            SampleShadowMapUV = half2(-o.uv.x, o.uv.y);
                            LightDotForward = -LightDotForward;
                        }
                        half4 SampleFaceShadowMap = SAMPLE_TEXTURE2D(_FaceShadowMap, linear_repeat_sampler, SampleShadowMapUV);
                        half FixShadowMapFactor = abs(MainLight.direction.z);
                        // Due to light angle will decrease the 'LightDotForward' intensity. so make shadowMap be decreased the same.
                        SampleFaceShadowMap.r *= FixShadowMapFactor;
                        LightDotForward *= FixShadowMapFactor;
                        SampleFaceShadowMap.r = InverseLerp(LightDotForward, LightDotForward + _FaceShadowRange, SampleFaceShadowMap.r);
                        SampleFaceShadowMap.r *= LightDotUp > 0 ? 1 : 0;
                        SampleLightMap.r = SampleFaceShadowMap.r;
                        break;
                    }
                    // R: ShadowMap
                    half LightMap_Shadow = SampleLightMap.r;
                    LDotN *= LightMap_Shadow;

                    // B: Metal (MatCap)
                    half LightMap_Metal = SampleLightMap.b;
                    LightMap_Metal *= _Metal;
                    half2 MatCapUV = mul((float3x3)UNITY_MATRIX_V, o.normal).xy * 0.5f + 0.5f;
                    half3 SampleMatCap = SAMPLE_TEXTURE2D(_MatCapTex, linear_repeat_sampler, MatCapUV).rgb;
                    FinalColor += SampleMatCap * LightMap_Metal * MainLight.color;

                    // A: Gradient Color
                    half LightMap_GradientIndex = SampleLightMap.a;
                    half2 SampleLightGradientUV = half2(LDotN.x, LightMap_GradientIndex.r);

                    // Final Calculate
                    half4 SampleLightGradientMap = SAMPLE_TEXTURE2D(_LightMap_GradientColor, linear_clamp_sampler, SampleLightGradientUV);
                    FinalColor *= SampleLightGradientMap.rgb;
                }
                else
                {
                    FinalColor *= LDotN;
                }

                // Depth-Rim
                if (_EnableDepthRim)
                {
                    float CameraDist = 1 / GetCenterPointToCamDist();
                    _OffsetDepthRim.x *= LightDirVS.x;
                    _OffsetDepthRim.y *= MainLight.direction.y;
                    half2 OffsetDepth = FixDepthRim(_OffsetDepthRim.xy, CameraDist);
                    float SampleOffsetDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, linear_clamp_sampler, ScreenUV + OffsetDepth).r;
                    SampleOffsetDepth = Linear01Depth(SampleOffsetDepth, _ZBufferParams);
                    half Rim = SampleOffsetDepth - SampleObject01Depth;
                    Rim = step(_ProjectionParams.w * 0.1f, Rim);
                    Rim *= MainLight.shadowAttenuation;
                    switch (_DepthRimMode)
                    {
                    default:
                    case 0: // Additive
                        FinalColor += Rim * SampleMainTex.rgb * _RimColor.rgb * MainLight.color;
                        break;
                    case 1: // Multiply
                        FinalColor = lerp(FinalColor, FinalColor * _RimColor.rgb * MainLight.color, _RimColor.a * Rim);
                        break;
                    case 2: // Replace
                        FinalColor = lerp(FinalColor, _RimColor.rgb * MainLight.color, _RimColor.a * Rim);
                        break;
                    }
                }

                // Eye-Through-Hair
                if (_ReadEyeColor && unity_OrthoParams.w == 0)
                {
                    half4 SampleEyeThroughHairTex = SAMPLE_TEXTURE2D(_AnimeToon_EyeThroughHair_Color, linear_clamp_sampler, ScreenUV);
                    FinalColor = lerp(FinalColor, SampleEyeThroughHairTex.rgb, SampleEyeThroughHairTex.a);
                }

                return half4(FinalColor, FinalAlpha);
            }
            ENDHLSL
        }
        Pass
        {
            Tags
            {
                "LightMode" = "AnimeToon_CasterHairShadow"
            }

            Blend One Zero
            ZWrite On
            ZTest Less
            Cull Back
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.positionCS = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(VertexOutput o) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
        Pass
        {
            Tags
            {
                "LightMode" = "AnimeToon_EyeThroughHair"
            }
            Blend One Zero
            ZWrite On
            ZTest Less
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.positionCS = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(VertexOutput o) : SV_Target
            {
                // Sample Custom Shadow
                half4 SampleFaceShadowMap = SAMPLE_TEXTURE2D(_FaceShadowMap, linear_repeat_sampler, o.uv);

                // Sample Main Tex
                half4 SampleMainTex = SAMPLE_TEXTURE2D(_MainTex, linear_repeat_sampler, o.uv);
                SampleMainTex *= _Color;

                // Final Output
                half3 FinalColor = SampleMainTex.rgb;
                half FinalAlpha = SampleFaceShadowMap.g * _WriteEyeColor_Intensity;

                // Main Light
                Light MainLight = GetMainLight();
                FinalColor *= MainLight.color;

                // Fadeout with camera rotate
                half3 ViewPointDir = _WorldSpaceCameraPos.xyz - unity_ObjectToWorld._m03_m13_m23;
                half VPDotZForward = saturate(dot(normalize(ViewPointDir), unity_ObjectToWorld._m01_m11_m21));
                VPDotZForward = pow(VPDotZForward, _WriteEyeColor_FadeOutRange);
                FinalAlpha *= VPDotZForward;

                return half4(FinalColor, FinalAlpha);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex vert_ShadowCaster
            #pragma fragment frag_ShadowCaster

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #pragma shader_feature_local_fragment _ ENABLE_ALPHACLIP

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            float4 GetShadowPositionHClip(float3 positionWS, float3 normalWS)
            {
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            VertexOutput vert_ShadowCaster(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normal);

                o.uv = v.uv;
                o.positionCS = GetShadowPositionHClip(positionWS, normalWS);
                return o;
            }

            half4 frag_ShadowCaster(VertexOutput o) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(o);

                #ifdef ENABLE_ALPHACLIP
                    // Sample Main Tex
                    half2 SampleMainTexUV = o.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                    half4 SampleMainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, SampleMainTexUV);
                    SampleMainTex *= _Color;
                    clip(SampleMainTex.a - 0.5f);
                #endif

                #if defined(LOD_FADE_CROSSFADE)
                    LODFadeCrossFade(o.positionCS);
                #endif

                return 0;
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex vert_DepthOnly
            #pragma fragment frag_DepthOnly

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ ENABLE_ALPHACLIP

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"

            VertexOutput vert_DepthOnly(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.uv = v.uv;
                o.positionCS = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            half frag_DepthOnly(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                #ifdef ENABLE_ALPHACLIP
                    // Sample Main Tex
                    half2 SampleMainTexUV = o.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                    half4 SampleMainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, SampleMainTexUV);
                    SampleMainTex *= _Color;
                    clip(SampleMainTex.a - 0.5f);
                #endif

                #if defined(LOD_FADE_CROSSFADE)
                    LODFadeCrossFade(o.positionCS);
                #endif

                return input.positionCS.z;
            }
            ENDHLSL
        }

        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex vert_DepthNormals
            #pragma fragment frag_DepthNormals

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ _ENABLENORMALMAP_ON
            #pragma shader_feature_local _ ENABLE_ALPHACLIP

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #ifdef _ENABLENORMALMAP_ON
                #define _NORMALMAP
            #endif

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            //--------------------------------------
            // DepthNormals built-in keywords
            #if defined(LOD_FADE_CROSSFADE)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

            // DepthNormals built-in keywords (GLES2 has limited amount of interpolators)
            #if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
                #define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
            #endif

            #if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
                #define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
            #endif

            #if defined(_ALPHATEST_ON) || defined(_PARALLAXMAP) || defined(_NORMALMAP) || defined(_DETAIL)
                #define REQUIRES_UV_INTERPOLATOR
            #endif


            // -------------------------------------
            // Includes

            VertexOutput vert_DepthNormals(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normal, v.tangent);

                o.uv = v.uv;
                o.positionCS = vertexInput.positionCS;
                o.normal = normalInput.normalWS;

                #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                    float sign = v.tangent.w * float(GetOddNegativeScale());
                    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
                #endif

                #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
                    o.tangent = tangentWS;
                #endif

                #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, o.normal, viewDirWS);
                    o.viewDirTS = viewDirTS;
                #endif

                return o;
            }

            half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = half(1.0))
            {
                #ifdef _NORMALMAP
                    half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
                #if BUMP_SCALE_NOT_SUPPORTED
                        return UnpackNormal(n);
                #else
                        return UnpackNormalScale(n, scale);
                #endif
                #else
                return half3(0.0h, 0.0h, 1.0h);
                #endif
            }

            void frag_DepthNormals(
                VertexOutput o
                , out half4 outNormalWS : SV_Target0
                #ifdef _WRITE_RENDERING_LAYERS
                , out float4 outRenderingLayers : SV_Target1
                #endif
            )
            {
                UNITY_SETUP_INSTANCE_ID(o);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o);

                #ifdef ENABLE_ALPHACLIP
                    // Sample Main Tex
                    half2 SampleMainTexUV = o.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                    half4 SampleMainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, SampleMainTexUV);
                    SampleMainTex *= _Color;
                    clip(SampleMainTex.a - 0.5f);
                #endif

                #if defined(LOD_FADE_CROSSFADE)
                    LODFadeCrossFade(o.positionCS);
                #endif

                #if defined(_GBUFFER_NORMALS_OCT)
                    float3 normalWS = normalize(o.normal);
                    float2 octNormalWS = PackNormalOctQuadEncode(normalWS);           // values between [-1, +1], must use fp32 on some platforms
                    float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);   // values between [ 0,  1]
                    half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);      // values between [ 0,  1]
                    outNormalWS = half4(packedNormalWS, 0.0);
                #else

                #if defined(_PARALLAXMAP)
                #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                            half3 viewDirTS = o.viewDirTS;
                #else
                            half3 viewDirTS = GetViewDirectionTangentSpace(o.tangent, o.normal, o.viewDirWS);
                #endif
                            ApplyPerPixelDisplacement(viewDirTS, o.uv);
                #endif

                #if defined(_NORMALMAP)
                        float sgn = o.tangent.w; // should be either +1 or -1
                        float3 bitangent = sgn * cross(o.normal.xyz, o.tangent.xyz);
                        float3 normalTS = SampleNormal(o.uv, TEXTURE2D_ARGS(_NormalMap, linear_repeat_sampler), _NormalIntensity);
                        float3 normalWS = TransformTangentToWorld(normalTS, half3x3(o.tangent.xyz, bitangent.xyz, o.normal.xyz));
                #else
                float3 normalWS = o.normal;
                #endif

                outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
                #endif

                #ifdef _WRITE_RENDERING_LAYERS
                    uint renderingLayers = GetMeshRenderingLayer();
                    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
                #endif
            }
            ENDHLSL
        }
    }
    CustomEditor "AnimeToonShaderEditor"
}