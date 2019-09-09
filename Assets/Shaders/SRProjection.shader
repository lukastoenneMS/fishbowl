// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

Shader "FishBowl/SRProjection"
{
    Properties
    {
        // Main maps.
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)

        _TexA("Tex A", 2D) = "white" {}
        _TexB("Tex B", 2D) = "white" {}
        _TexC("Tex C", 2D) = "white" {}

        _WaveA ("Texture Wave Number A", Vector) = (1, 0, 0, 0)
        _WaveB ("Texture Wave Number B", Vector) = (1, 0, 0, 0)
        _WaveC ("Texture Wave Number C", Vector) = (1, 0, 0, 0)

        _WaveStretch ("Texture Wave Stretch", Float) = 0.0
        _WaveShift ("Texture Wave Shift", Float) = 0.1
        _WavePhase ("Texture Wave Phase", Float) = 0.0

        _TextureExponent("Texture Exponent", Range(0.1, 10.0)) = 1.0

        [Enum(AlbedoAlphaMode)] _AlbedoAlphaMode("Albedo Alpha Mode", Float) = 0 // "Transparency"
        [Toggle] _AlbedoAssignedAtRuntime("Albedo Assigned at Runtime", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        [Toggle(_NORMAL_MAP)] _EnableNormalMap("Enable Normal Map", Float) = 0.0
        [NoScaleOffset] _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalMapScale("Scale", Float) = 1.0
        [Toggle(_EMISSION)] _EnableEmission("Enable Emission", Float) = 0.0
        [HDR]_EmissiveColor("Emissive Color", Color) = (0.0, 0.0, 0.0, 1.0)

        // Rendering options.
        [Toggle(_DIRECTIONAL_LIGHT)] _DirectionalLight("Directional Light", Float) = 1.0
        [Toggle(_SPECULAR_HIGHLIGHTS)] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [Toggle(_SPHERICAL_HARMONICS)] _SphericalHarmonics("Spherical Harmonics", Float) = 0.0
        [Toggle(_REFLECTIONS)] _Reflections("Reflections", Float) = 0.0
        [Toggle(_REFRACTION)] _Refraction("Refraction", Float) = 0.0
        _RefractiveIndex("Refractive Index", Range(0.0, 3.0)) = 0.0
        [Toggle(_RIM_LIGHT)] _RimLight("Rim Light", Float) = 0.0
        _RimColor("Rim Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _RimPower("Rim Power", Range(0.0, 8.0)) = 0.25
        [Toggle(_VERTEX_COLORS)] _VertexColors("Vertex Colors", Float) = 0.0
        [Toggle(_VERTEX_EXTRUSION)] _VertexExtrusion("Vertex Extrusion", Float) = 0.0
        _VertexExtrusionValue("Vertex Extrusion Value", Float) = 0.0
        _BlendedClippingWidth("Blended Clipping With", Range(0.0, 10.0)) = 1.0
        [Toggle(_CLIPPING_BORDER)] _ClippingBorder("Clipping Border", Float) = 0.0
        _ClippingBorderWidth("Clipping Border Width", Range(0.0, 1.0)) = 0.025
        _ClippingBorderColor("Clipping Border Color", Color) = (1.0, 0.2, 0.0, 1.0)
        [Toggle(_NEAR_PLANE_FADE)] _NearPlaneFade("Near Plane Fade", Float) = 0.0
        [Toggle(_NEAR_LIGHT_FADE)] _NearLightFade("Near Light Fade", Float) = 0.0
        _FadeBeginDistance("Fade Begin Distance", Range(0.0, 10.0)) = 0.85
        _FadeCompleteDistance("Fade Complete Distance", Range(0.0, 10.0)) = 0.5
        _FadeMinValue("Fade Min Value", Range(0.0, 1.0)) = 0.0

        // Fluent options.
        [Toggle(_HOVER_LIGHT)] _HoverLight("Hover Light", Float) = 1.0
        [Toggle(_HOVER_COLOR_OVERRIDE)] _EnableHoverColorOverride("Hover Color Override", Float) = 0.0
        _HoverColorOverride("Hover Color Override", Color) = (1.0, 1.0, 1.0, 1.0)
        [Toggle(_PROXIMITY_LIGHT)] _ProximityLight("Proximity Light", Float) = 0.0
        [Toggle(_PROXIMITY_LIGHT_COLOR_OVERRIDE)] _EnableProximityLightColorOverride("Proximity Light Color Override", Float) = 0.0
        [HDR]_ProximityLightCenterColorOverride("Proximity Light Center Color Override", Color) = (1.0, 0.0, 0.0, 0.0)
        [HDR]_ProximityLightMiddleColorOverride("Proximity Light Middle Color Override", Color) = (0.0, 1.0, 0.0, 0.5)
        [HDR]_ProximityLightOuterColorOverride("Proximity Light Outer Color Override", Color) = (0.0, 0.0, 1.0, 1.0)
        [Toggle(_PROXIMITY_LIGHT_SUBTRACTIVE)] _ProximityLightSubtractive("Proximity Light Subtractive", Float) = 0.0
        [Toggle(_PROXIMITY_LIGHT_TWO_SIDED)] _ProximityLightTwoSided("Proximity Light Two Sided", Float) = 0.0
        _FluentLightIntensity("Fluent Light Intensity", Range(0.0, 1.0)) = 1.0
        [Toggle(_INNER_GLOW)] _InnerGlow("Inner Glow", Float) = 0.0
        _InnerGlowColor("Inner Glow Color (RGB) and Intensity (A)", Color) = (1.0, 1.0, 1.0, 0.75)
        _InnerGlowPower("Inner Glow Power", Range(2.0, 32.0)) = 4.0
        [Toggle(_ENVIRONMENT_COLORING)] _EnvironmentColoring("Environment Coloring", Float) = 0.0
        _EnvironmentColorThreshold("Environment Color Threshold", Range(0.0, 3.0)) = 1.5
        _EnvironmentColorIntensity("Environment Color Intensity", Range(0.0, 1.0)) = 0.5
        _EnvironmentColorX("Environment Color X (RGB)", Color) = (1.0, 0.0, 0.0, 1.0)
        _EnvironmentColorY("Environment Color Y (RGB)", Color) = (0.0, 1.0, 0.0, 1.0)
        _EnvironmentColorZ("Environment Color Z (RGB)", Color) = (0.0, 0.0, 1.0, 1.0)

        // Advanced options.
        [Enum(RenderingMode)] _Mode("Rendering Mode", Float) = 0                                     // "Opaque"
        [Enum(CustomRenderingMode)] _CustomMode("Mode", Float) = 0                                   // "Opaque"
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 1                 // "One"
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 0            // "Zero"
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation", Float) = 0                 // "Add"
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Depth Test", Float) = 4                // "LessEqual"
        [Enum(DepthWrite)] _ZWrite("Depth Write", Float) = 1                                         // "On"
        _ZOffsetFactor("Depth Offset Factor", Float) = 0                                             // "Zero"
        _ZOffsetUnits("Depth Offset Units", Float) = 0                                               // "Zero"
        [Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorWriteMask("Color Write Mask", Float) = 15 // "All"
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2                     // "Back"
        _RenderQueueOverride("Render Queue Override", Range(-1.0, 5000)) = -1
        [Toggle(_INSTANCED_COLOR)] _InstancedColor("Instanced Color", Float) = 0.0
        [Toggle(_STENCIL)] _Stencil("Enable Stencil Testing", Float) = 0.0
        _StencilReference("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComparison("Stencil Comparison", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOperation("Stencil Operation", Int) = 0
    }

CGINCLUDE
    // Shared functions for all passes

    float2 ProjectToUV(float3 worldPosition, float4 tiling)
    {
        return float2(worldPosition.x, worldPosition.z) * tiling.xy + tiling.zw;
    }

    static const float pi = 3.1415926535897932384626433832795;

    float2 ApplyWave(float2 position, float2 waveVector, float phase, float stretch, float shift)
    {
        float2 dir = normalize(waveVector);
        float2 forward = float2(dir.x, dir.y);
        float2 right = float2(dir.y, -dir.x);

        float a = dot(waveVector, position) - phase;
        float w = sin(2.0 * pi * a);

        position += w * (stretch * forward + shift * right);

        return position;
    }

    fixed4 CombineTextures(sampler2D texA, float2 uvA, sampler2D texB, float2 uvB, sampler2D texC, float2 uvC)
    {
        fixed4 albedo = fixed4(0.0, 0.0, 0.0, 1.0);
        float weight = 0.0;

#if !defined(_DISABLE_TEX_MAP_A)
        albedo += tex2D(texA, uvA);
        weight += 1.0;
#endif

#if !defined(_DISABLE_TEX_MAP_B)
        albedo += tex2D(texB, uvB);
        weight += 1.0;
#endif

#if !defined(_DISABLE_TEX_MAP_C)
        albedo += tex2D(texC, uvC);
        weight += 1.0;
#endif

        albedo /= weight;

        return albedo;
    }

ENDCG

    SubShader
    {
        Pass
        {
            Name "Main"
            Tags{ "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
            LOD 100
            Blend[_SrcBlend][_DstBlend]
            BlendOp[_BlendOp]
            ZTest[_ZTest]
            ZWrite[_ZWrite]
            Cull[_CullMode]
            Offset[_ZOffsetFactor],[_ZOffsetUnits]
            ColorMask[_ColorWriteMask]

            Stencil
            {
                Ref[_StencilReference]
                Comp[_StencilComparison]
                Pass[_StencilOperation]
            }

            CGPROGRAM

#if defined(SHADER_API_D3D11)
            #pragma target 5.0
#endif
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ _MULTI_HOVER_LIGHT
            #pragma multi_compile _ _CLIPPING_PLANE
            #pragma multi_compile _ _CLIPPING_SPHERE
            #pragma multi_compile _ _CLIPPING_BOX

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON
            #pragma shader_feature _DISABLE_TEX_MAP_A
            #pragma shader_feature _DISABLE_TEX_MAP_B
            #pragma shader_feature _DISABLE_TEX_MAP_C
            #pragma shader_feature _ _METALLIC_TEXTURE_ALBEDO_CHANNEL_A _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _NORMAL_MAP
            #pragma shader_feature _EMISSION
            #pragma shader_feature _DIRECTIONAL_LIGHT
            #pragma shader_feature _SPECULAR_HIGHLIGHTS
            #pragma shader_feature _SPHERICAL_HARMONICS
            #pragma shader_feature _REFLECTIONS
            #pragma shader_feature _REFRACTION
            #pragma shader_feature _RIM_LIGHT
            #pragma shader_feature _VERTEX_COLORS
            #pragma shader_feature _VERTEX_EXTRUSION
            #pragma shader_feature _CLIPPING_BORDER
            #pragma shader_feature _NEAR_PLANE_FADE
            #pragma shader_feature _NEAR_LIGHT_FADE
            #pragma shader_feature _HOVER_LIGHT
            #pragma shader_feature _HOVER_COLOR_OVERRIDE
            #pragma shader_feature _PROXIMITY_LIGHT
            #pragma shader_feature _PROXIMITY_LIGHT_COLOR_OVERRIDE
            #pragma shader_feature _PROXIMITY_LIGHT_SUBTRACTIVE
            #pragma shader_feature _PROXIMITY_LIGHT_TWO_SIDED
            #pragma shader_feature _INNER_GLOW
            #pragma shader_feature _ENVIRONMENT_COLORING
            #pragma shader_feature _INSTANCED_COLOR

            #define IF(a, b, c) lerp(b, c, step((fixed) (a), 0.0)); 

            #include "UnityCG.cginc"
            #include "UnityStandardConfig.cginc"
            #include "UnityStandardUtils.cginc"

            // This define will get commented in by the UpgradeShaderForLightweightRenderPipeline method.
            //#define _LIGHTWEIGHT_RENDER_PIPELINE

            #define _NORMAL
            #define _WORLD_POSITION

#if defined(_CLIPPING_PLANE) || defined(_CLIPPING_SPHERE) || defined(_CLIPPING_BOX)
        #define _CLIPPING_PRIMITIVE
#else
        #undef _CLIPPING_PRIMITIVE
#endif

#if defined(_ALPHATEST_ON) || defined(_CLIPPING_PRIMITIVE)
            #define _ALPHA_CLIP
#else
            #undef _ALPHA_CLIP
#endif

#if defined(_ALPHABLEND_ON)
            #define _TRANSPARENT
            #undef _ALPHA_CLIP
#else
            #undef _TRANSPARENT
#endif

#if defined(_DIRECTIONAL_LIGHT) || defined(_RIM_LIGHT)
            #define _FRESNEL
#else
            #undef _FRESNEL
#endif

#if defined(_INNER_GLOW)
            #define _DISTANCE_TO_EDGE
#else
            #undef _DISTANCE_TO_EDGE
#endif

            #define _UV

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
#if defined(LIGHTMAP_ON)
                float2 lightMapUV : TEXCOORD1;
#endif
#if defined(_VERTEX_COLORS)
                fixed4 color : COLOR0;
#endif
                fixed3 normal : NORMAL;
#if defined(_NORMAL_MAP)
                fixed4 tangent : TANGENT;
#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f 
            {
                float4 position : SV_POSITION;
#if defined(_UV)
                float2 uvA : TEXCOORD0;
                float2 uvB : TEXCOORD1;
                float2 uvC : TEXCOORD2;
#endif
#if defined(LIGHTMAP_ON)
                float2 lightMapUV : TEXCOORD3;
#endif
#if defined(_VERTEX_COLORS)
                fixed4 color : COLOR0;
#endif
#if defined(_SPHERICAL_HARMONICS)
                fixed3 ambient : COLOR1;
#endif
#if defined(_WORLD_POSITION)
#if defined(_NEAR_PLANE_FADE)
                float4 worldPosition : TEXCOORD4;
#else
                float3 worldPosition : TEXCOORD4;
#endif
#endif

#if defined(_NORMAL)
#if defined(_NORMAL_MAP)
                fixed3 tangentX : COLOR3;
                fixed3 tangentY : COLOR4;
                fixed3 tangentZ : COLOR5;
#else
                fixed3 worldNormal : COLOR3;
#endif
#endif
                UNITY_VERTEX_OUTPUT_STEREO
#if defined(_INSTANCED_COLOR)
                UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
            };

#if defined(_INSTANCED_COLOR)
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)
#else
            fixed4 _Color;
#endif
            sampler2D _TexA;
            sampler2D _TexB;
            sampler2D _TexC;
            fixed4 _TexA_ST;
            fixed4 _TexB_ST;
            fixed4 _TexC_ST;
            float4 _WaveA;
            float4 _WaveB;
            float4 _WaveC;
            float _WaveStretch;
            float _WaveShift;
            float _WavePhase;

            float _TextureExponent;

#if defined(_ALPHA_CLIP)
            fixed _Cutoff;
#endif

            fixed _Metallic;
            fixed _Smoothness;

#if defined(_NORMAL_MAP)
            sampler2D _NormalMap;
            float _NormalMapScale;
#endif

#if defined(_EMISSION)
            fixed4 _EmissiveColor;
#endif

#if defined(_DIRECTIONAL_LIGHT)
#if defined(_LIGHTWEIGHT_RENDER_PIPELINE)
            CBUFFER_START(_LightBuffer)
            float4 _MainLightPosition;
            half4 _MainLightColor;
            CBUFFER_END
#else
            fixed4 _LightColor0;
#endif
#endif

#if defined(_REFRACTION)
            fixed _RefractiveIndex;
#endif

#if defined(_RIM_LIGHT)
            fixed3 _RimColor;
            fixed _RimPower;
#endif

#if defined(_VERTEX_EXTRUSION)
            float _VertexExtrusionValue;
#endif

#if defined(_CLIPPING_PLANE)
            fixed _ClipPlaneSide;
            float4 _ClipPlane;
#endif

#if defined(_CLIPPING_SPHERE)
            fixed _ClipSphereSide;
            float4 _ClipSphere;
#endif

#if defined(_CLIPPING_BOX)
            fixed _ClipBoxSide;
            float4 _ClipBoxSize;
            float4x4 _ClipBoxInverseTransform;
#endif

#if defined(_CLIPPING_PRIMITIVE)
            float _BlendedClippingWidth;
#endif

#if defined(_CLIPPING_BORDER)
            fixed _ClippingBorderWidth;
            fixed3 _ClippingBorderColor;
#endif

#if defined(_NEAR_PLANE_FADE)
            float _FadeBeginDistance;
            float _FadeCompleteDistance;
            fixed _FadeMinValue;
#endif

#if defined(_HOVER_LIGHT) || defined(_NEAR_LIGHT_FADE)
#if defined(_MULTI_HOVER_LIGHT)
#define HOVER_LIGHT_COUNT 3
#else
#define HOVER_LIGHT_COUNT 1
#endif
#define HOVER_LIGHT_DATA_SIZE 2
            float4 _HoverLightData[HOVER_LIGHT_COUNT * HOVER_LIGHT_DATA_SIZE];
#if defined(_HOVER_COLOR_OVERRIDE)
            fixed3 _HoverColorOverride;
#endif
#endif

#if defined(_PROXIMITY_LIGHT) || defined(_NEAR_LIGHT_FADE)
#define PROXIMITY_LIGHT_COUNT 2
#define PROXIMITY_LIGHT_DATA_SIZE 6
            float4 _ProximityLightData[PROXIMITY_LIGHT_COUNT * PROXIMITY_LIGHT_DATA_SIZE];
#if defined(_PROXIMITY_LIGHT_COLOR_OVERRIDE)
            float4 _ProximityLightCenterColorOverride;
            float4 _ProximityLightMiddleColorOverride;
            float4 _ProximityLightOuterColorOverride;
#endif
#endif

#if defined(_HOVER_LIGHT) || defined(_PROXIMITY_LIGHT)
            fixed _FluentLightIntensity;
#endif

#if defined(_INNER_GLOW)
            fixed4 _InnerGlowColor;
            fixed _InnerGlowPower;
#endif

#if defined(_ENVIRONMENT_COLORING)
            fixed _EnvironmentColorThreshold;
            fixed _EnvironmentColorIntensity;
            fixed3 _EnvironmentColorX;
            fixed3 _EnvironmentColorY;
            fixed3 _EnvironmentColorZ;
#endif

#if defined(_DIRECTIONAL_LIGHT)
            static const fixed _MinMetallicLightContribution = 0.7;
            static const fixed _IblContribution = 0.1;
#endif

#if defined(_SPECULAR_HIGHLIGHTS)
            static const float _Shininess = 800.0;
#endif

#if defined(_FRESNEL)
            static const float _FresnelPower = 8.0;
#endif

#if defined(_NEAR_LIGHT_FADE)
            static const float _MaxNearLightDistance = 10.0;

            inline float NearLightDistance(float4 light, float3 worldPosition)
            {
                return distance(worldPosition, light.xyz) + ((1.0 - light.w) * _MaxNearLightDistance);
            }
#endif

#if defined(_HOVER_LIGHT)
            inline float HoverLight(float4 hoverLight, float inverseRadius, float3 worldPosition)
            {
                return (1.0 - saturate(length(hoverLight.xyz - worldPosition) * inverseRadius)) * hoverLight.w;
            }
#endif

#if defined(_PROXIMITY_LIGHT)
            inline float ProximityLight(float4 proximityLight, float4 proximityLightParams, float4 proximityLightPulseParams, float3 worldPosition, float3 worldNormal, out fixed colorValue)
            {
                float proximityLightDistance = dot(proximityLight.xyz - worldPosition, worldNormal);
#if defined(_PROXIMITY_LIGHT_TWO_SIDED)
                worldNormal = IF(proximityLightDistance < 0.0, -worldNormal, worldNormal);
                proximityLightDistance = abs(proximityLightDistance);
#endif
                float normalizedProximityLightDistance = saturate(proximityLightDistance * proximityLightParams.y);
                float3 projectedProximityLight = proximityLight.xyz - (worldNormal * abs(proximityLightDistance));
                float projectedProximityLightDistance = length(projectedProximityLight - worldPosition);
                float attenuation = (1.0 - normalizedProximityLightDistance) * proximityLight.w;
                colorValue = saturate(projectedProximityLightDistance * proximityLightParams.z);
                float pulse = step(proximityLightPulseParams.x, projectedProximityLightDistance) * proximityLightPulseParams.y;

                return smoothstep(1.0, 0.0, projectedProximityLightDistance / (proximityLightParams.x * max(pow(normalizedProximityLightDistance, 0.25), proximityLightParams.w))) * pulse * attenuation;
            }

            inline fixed3 MixProximityLightColor(fixed4 centerColor, fixed4 middleColor, fixed4 outerColor, fixed t)
            {
                fixed3 color = lerp(centerColor.rgb, middleColor.rgb, smoothstep(centerColor.a, middleColor.a, t));
                return lerp(color, outerColor, smoothstep(middleColor.a, outerColor.a, t));
            }
#endif

#if defined(_CLIPPING_PLANE)
            inline float PointVsPlane(float3 worldPosition, float4 plane)
            {
                float3 planePosition = plane.xyz * plane.w;
                return dot(worldPosition - planePosition, plane.xyz);
            }
#endif

#if defined(_CLIPPING_SPHERE)
            inline float PointVsSphere(float3 worldPosition, float4 sphere)
            {
                return distance(worldPosition, sphere.xyz) - sphere.w;
            }
#endif

#if defined(_CLIPPING_BOX)
            inline float PointVsBox(float3 worldPosition, float3 boxSize, float4x4 boxInverseTransform)
            {
                float3 distance = abs(mul(boxInverseTransform, float4(worldPosition, 1.0))) - boxSize;
                return length(max(distance, 0.0)) + min(max(distance.x, max(distance.y, distance.z)), 0.0);
            }
#endif

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
#if defined(_INSTANCED_COLOR)
                UNITY_TRANSFER_INSTANCE_ID(v, o);
#endif
                float4 vertexPosition = v.vertex;

#if defined(_WORLD_POSITION) || defined(_VERTEX_EXTRUSION)
                float3 worldVertexPosition = mul(unity_ObjectToWorld, vertexPosition).xyz;
#endif

#if defined(_NORMAL) || defined(_VERTEX_EXTRUSION)
                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
#endif

#if defined(_VERTEX_EXTRUSION)
                worldVertexPosition += worldNormal * _VertexExtrusionValue;
                vertexPosition = mul(unity_WorldToObject, float4(worldVertexPosition, 1.0));
#endif

                o.position = UnityObjectToClipPos(vertexPosition);

#if defined(_WORLD_POSITION)
                o.worldPosition.xyz = worldVertexPosition;
#endif

#if defined(_NEAR_PLANE_FADE)
                float rangeInverse = 1.0 / (_FadeBeginDistance - _FadeCompleteDistance);
#if defined(_NEAR_LIGHT_FADE)
                float fadeDistance = _MaxNearLightDistance;

                [unroll]
                for (int hoverLightIndex = 0; hoverLightIndex < HOVER_LIGHT_COUNT; ++hoverLightIndex)
                {
                    int dataIndex = hoverLightIndex * HOVER_LIGHT_DATA_SIZE;
                    fadeDistance = min(fadeDistance, NearLightDistance(_HoverLightData[dataIndex], o.worldPosition));
                }

                [unroll]
                for (int proximityLightIndex = 0; proximityLightIndex < PROXIMITY_LIGHT_COUNT; ++proximityLightIndex)
                {
                    int dataIndex = proximityLightIndex * PROXIMITY_LIGHT_DATA_SIZE;
                    fadeDistance = min(fadeDistance, NearLightDistance(_ProximityLightData[dataIndex], o.worldPosition));
                }
#else
                float fadeDistance = -UnityObjectToViewPos(vertexPosition).z;
#endif
                o.worldPosition.w = max(saturate(mad(fadeDistance, rangeInverse, -_FadeCompleteDistance * rangeInverse)), _FadeMinValue);
#endif

#if defined(_UV)
                o.uvA = ProjectToUV(o.worldPosition.xyz, _TexA_ST);
                o.uvB = ProjectToUV(o.worldPosition.xyz, _TexB_ST);
                o.uvC = ProjectToUV(o.worldPosition.xyz, _TexC_ST);
#endif

#if defined(LIGHTMAP_ON)
                o.lightMapUV.xy = v.lightMapUV.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif

#if defined(_VERTEX_COLORS)
                o.color = v.color;
#endif

#if defined(_SPHERICAL_HARMONICS)
                o.ambient = ShadeSH9(float4(worldNormal, 1.0));
#endif

#if defined(_NORMAL)
#if defined(_NORMAL_MAP)
                fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                fixed3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
                o.tangentX = fixed3(worldTangent.x, worldBitangent.x, worldNormal.x);
                o.tangentY = fixed3(worldTangent.y, worldBitangent.y, worldNormal.y);
                o.tangentZ = fixed3(worldTangent.z, worldBitangent.z, worldNormal.z);
#else
                o.worldNormal = worldNormal;
#endif
#endif

                return o;
            }

#if defined(SHADER_API_D3D11) && !defined(_ALPHA_CLIP) && !defined(_TRANSPARENT)
            [earlydepthstencil]
#endif
            fixed4 frag(v2f i, fixed facing : VFACE) : SV_Target
            {
#if defined(_INSTANCED_COLOR)
                UNITY_SETUP_INSTANCE_ID(i);
#endif

                // Texturing.
                float2 uvA = ApplyWave(i.uvA, _WaveA.xy, _WavePhase, _WaveStretch, _WaveShift);
                float2 uvB = ApplyWave(i.uvB, _WaveB.xy, _WavePhase, _WaveStretch, _WaveShift);
                float2 uvC = ApplyWave(i.uvC, _WaveC.xy, _WavePhase, _WaveStretch, _WaveShift);
                fixed4 albedo = CombineTextures(_TexA, uvA, _TexB, uvB, _TexC, uvC);
                albedo = pow(albedo, _TextureExponent);

#ifdef LIGHTMAP_ON
                albedo.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightMapUV));
#endif

#if defined(_METALLIC_TEXTURE_ALBEDO_CHANNEL_A)
                _Metallic = albedo.a;
                albedo.a = 1.0;
#elif defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
                _Smoothness = albedo.a;
                albedo.a = 1.0;
#endif 

                // Primitive clipping.
#if defined(_CLIPPING_PRIMITIVE)
                float primitiveDistance = 1.0; 
#if defined(_CLIPPING_PLANE)
                primitiveDistance = min(primitiveDistance, PointVsPlane(i.worldPosition.xyz, _ClipPlane) * _ClipPlaneSide);
#endif
#if defined(_CLIPPING_SPHERE)
                primitiveDistance = min(primitiveDistance, PointVsSphere(i.worldPosition.xyz, _ClipSphere) * _ClipSphereSide);
#endif
#if defined(_CLIPPING_BOX)
                primitiveDistance = min(primitiveDistance, PointVsBox(i.worldPosition.xyz, _ClipBoxSize.xyz, _ClipBoxInverseTransform) * _ClipBoxSide);
#endif
#if defined(_CLIPPING_BORDER)
                fixed3 primitiveBorderColor = lerp(_ClippingBorderColor, fixed3(0.0, 0.0, 0.0), primitiveDistance / _ClippingBorderWidth);
                albedo.rgb += primitiveBorderColor * IF((primitiveDistance < _ClippingBorderWidth), 1.0, 0.0);
#endif
#endif

#if defined(_DISTANCE_TO_EDGE)
                fixed2 distanceToEdge;
                distanceToEdge.x = abs(i.uv.x - 0.5) * 2.0;
                distanceToEdge.y = abs(i.uv.y - 0.5) * 2.0;
#endif

#if defined(_INSTANCED_COLOR)
                albedo *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
#else
                albedo *= _Color;
#endif

#if defined(_VERTEX_COLORS)
                albedo *= i.color;
#endif

                // Normal calculation.
#if defined(_NORMAL)
                fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPosition.xyz));
#if defined(_REFLECTIONS) || defined(_ENVIRONMENT_COLORING)
                fixed3 incident = -worldViewDir;
#endif
                fixed3 worldNormal;

#if defined(_NORMAL_MAP)
                fixed3 tangentNormal = UnpackScaleNormal(tex2D(_NormalMap, i.uv), _NormalMapScale);
                worldNormal.x = dot(i.tangentX, tangentNormal);
                worldNormal.y = dot(i.tangentY, tangentNormal);
                worldNormal.z = dot(i.tangentZ, tangentNormal);
                worldNormal = normalize(worldNormal) * facing;
#else
                worldNormal = normalize(i.worldNormal) * facing;
#endif
#endif

                float albedoFalloff = worldNormal.y;
                albedo *= albedoFalloff;

                fixed pointToLight = 1.0;
                fixed3 fluentLightColor = fixed3(0.0, 0.0, 0.0);

                // Hover light.
#if defined(_HOVER_LIGHT)
                pointToLight = 0.0;

                [unroll]
                for (int hoverLightIndex = 0; hoverLightIndex < HOVER_LIGHT_COUNT; ++hoverLightIndex)
                {
                    int dataIndex = hoverLightIndex * HOVER_LIGHT_DATA_SIZE;
                    fixed hoverValue = HoverLight(_HoverLightData[dataIndex], _HoverLightData[dataIndex + 1].w, i.worldPosition.xyz);
                    pointToLight += hoverValue;
#if !defined(_HOVER_COLOR_OVERRIDE)
                    fluentLightColor += lerp(fixed3(0.0, 0.0, 0.0), _HoverLightData[dataIndex + 1].rgb, hoverValue);
#endif
                }
#if defined(_HOVER_COLOR_OVERRIDE)
                fluentLightColor = _HoverColorOverride.rgb * pointToLight;
#endif
#endif

                // Proximity light.
#if defined(_PROXIMITY_LIGHT)
#if !defined(_HOVER_LIGHT)
                pointToLight = 0.0;
#endif
                [unroll]
                for (int proximityLightIndex = 0; proximityLightIndex < PROXIMITY_LIGHT_COUNT; ++proximityLightIndex)
                {
                    int dataIndex = proximityLightIndex * PROXIMITY_LIGHT_DATA_SIZE;
                    fixed colorValue;
                    fixed proximityValue = ProximityLight(_ProximityLightData[dataIndex], _ProximityLightData[dataIndex + 1], _ProximityLightData[dataIndex + 2], i.worldPosition.xyz, worldNormal, colorValue);
                    pointToLight += proximityValue;
#if defined(_PROXIMITY_LIGHT_COLOR_OVERRIDE)
                    fixed3 proximityColor = MixProximityLightColor(_ProximityLightCenterColorOverride, _ProximityLightMiddleColorOverride, _ProximityLightOuterColorOverride, colorValue);
#else
                    fixed3 proximityColor = MixProximityLightColor(_ProximityLightData[dataIndex + 3], _ProximityLightData[dataIndex + 4], _ProximityLightData[dataIndex + 5], colorValue);
#endif  
#if defined(_PROXIMITY_LIGHT_SUBTRACTIVE)
                    fluentLightColor -= lerp(fixed3(0.0, 0.0, 0.0), proximityColor, proximityValue);
#else
                    fluentLightColor += lerp(fixed3(0.0, 0.0, 0.0), proximityColor, proximityValue);
#endif    
                }
#endif    

#if defined(_ALPHA_CLIP)
#if !defined(_ALPHATEST_ON)
                _Cutoff = 0.5;
#endif
#if defined(_CLIPPING_PRIMITIVE)
                albedo *= (primitiveDistance > 0.0);
#endif
                clip(albedo.a - _Cutoff);
                albedo.a = 1.0;
#endif

                // Blinn phong lighting.
#if defined(_DIRECTIONAL_LIGHT)
#if defined(_LIGHTWEIGHT_RENDER_PIPELINE)
                float4 directionalLightDirection = _MainLightPosition;
#else
                float4 directionalLightDirection = _WorldSpaceLightPos0;
#endif
                fixed diffuse = max(0.0, dot(worldNormal, directionalLightDirection));
#if defined(_SPECULAR_HIGHLIGHTS)
                fixed halfVector = max(0.0, dot(worldNormal, normalize(directionalLightDirection + worldViewDir)));
                fixed specular = saturate(pow(halfVector, _Shininess * pow(_Smoothness, 4.0)) * _Smoothness * 0.5);
#else
                fixed specular = 0.0;
#endif
#endif

                // Image based lighting (attempt to mimic the Standard shader).
#if defined(_REFLECTIONS)
                fixed3 worldReflection = reflect(incident, worldNormal);
                fixed4 iblData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, worldReflection, (1.0 - _Smoothness) * UNITY_SPECCUBE_LOD_STEPS);
                fixed3 ibl = DecodeHDR(iblData, unity_SpecCube0_HDR);
#if defined(_REFRACTION)
                fixed4 refractColor = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, refract(incident, worldNormal, _RefractiveIndex));
                ibl *= DecodeHDR(refractColor, unity_SpecCube0_HDR);
#endif
#else
                fixed3 ibl = unity_IndirectSpecColor.rgb;
#endif

                // Fresnel lighting.
#if defined(_FRESNEL)
                fixed fresnel = 1.0 - saturate(abs(dot(worldViewDir, worldNormal)));
#if defined(_RIM_LIGHT)
                fixed3 fresnelColor = _RimColor * pow(fresnel, _RimPower);
#else
                fixed3 fresnelColor = unity_IndirectSpecColor.rgb * (pow(fresnel, _FresnelPower) * max(_Smoothness, 0.5));
#endif
#endif
                // Final lighting mix.
                fixed4 output = albedo;
#if defined(_SPHERICAL_HARMONICS)
                fixed3 ambient = i.ambient;
#else
                fixed3 ambient = glstate_lightmodel_ambient + fixed3(0.25, 0.25, 0.25);
#endif
                fixed minProperty = min(_Smoothness, _Metallic);
#if defined(_DIRECTIONAL_LIGHT)
                fixed oneMinusMetallic = (1.0 - _Metallic);
                output.rgb = lerp(output.rgb, ibl, minProperty);
#if defined(_LIGHTWEIGHT_RENDER_PIPELINE)
                fixed3 directionalLightColor = _MainLightColor.rgb;
#else
                fixed3 directionalLightColor = _LightColor0.rgb;
#endif
                output.rgb *= lerp((ambient + directionalLightColor * diffuse + directionalLightColor * specular) * max(oneMinusMetallic, _MinMetallicLightContribution), albedo, minProperty);
                output.rgb += (directionalLightColor * albedo * specular) + (directionalLightColor * specular * _Smoothness);
                output.rgb += ibl * oneMinusMetallic * _IblContribution;
#elif defined(_REFLECTIONS)
                output.rgb = lerp(output.rgb, ibl, minProperty);
                output.rgb *= lerp(ambient, albedo, minProperty);
#elif defined(_SPHERICAL_HARMONICS)
                output.rgb *= ambient;
#endif

#if defined(_FRESNEL)
#if defined(_RIM_LIGHT) || !defined(_REFLECTIONS)
                output.rgb += fresnelColor;
#else
                output.rgb += fresnelColor * (1.0 - minProperty);
#endif
#endif

#if defined(_EMISSION)
                output.rgb += _EmissiveColor;
#endif

                // Inner glow.
#if defined(_INNER_GLOW)
                fixed2 uvGlow = pow(distanceToEdge * _InnerGlowColor.a, _InnerGlowPower);
                output.rgb += lerp(fixed3(0.0, 0.0, 0.0), _InnerGlowColor.rgb, uvGlow.x + uvGlow.y);
#endif

                // Environment coloring.
#if defined(_ENVIRONMENT_COLORING)
                fixed3 environmentColor = incident.x * incident.x * _EnvironmentColorX +
                                          incident.y * incident.y * _EnvironmentColorY + 
                                          incident.z * incident.z * _EnvironmentColorZ;
                output.rgb += environmentColor * max(0.0, dot(incident, worldNormal) + _EnvironmentColorThreshold) * _EnvironmentColorIntensity;

#endif

#if defined(_NEAR_PLANE_FADE)
                output *= i.worldPosition.w;
#endif

                // Hover and proximity lighting should occur after near plane fading.
#if defined(_HOVER_LIGHT) || defined(_PROXIMITY_LIGHT)
                output.rgb += fluentLightColor * _FluentLightIntensity * pointToLight;
#endif

                // Perform non-alpha clipped primitive clipping on the final output.
#if defined(_CLIPPING_PRIMITIVE) && !defined(_ALPHA_CLIP)
                output *= saturate(primitiveDistance * (1.0f / _BlendedClippingWidth));
#endif
                return output;
            }

            ENDCG
        }
    }
    
    Fallback "Hidden/InternalErrorShader"
    CustomEditor "FishBowl.SRProjectionShaderGUI"
}
