// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#if UNITY_EDITOR

using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Editor;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace FishBowl
{
    /// <summary>
    /// A custom shader inspector for the "Mixed Reality Toolkit/Standard" shader.
    /// </summary>
    public class SRProjectionShaderGUI : MixedRealityShaderGUI
    {
        protected enum AlbedoAlphaMode
        {
            Transparency,
            Metallic,
            Smoothness
        }

        protected static class Styles
        {
            public static string renderingOptionsTitle = "Rendering Options";
            public static string advancedOptionsTitle = "Advanced Options";
            public static string fluentOptionsTitle = "Fluent Options";
            public static string instancedColorName = "_InstancedColor";
            public static string instancedColorFeatureName = "_INSTANCED_COLOR";
            public static string stencilComparisonName = "_StencilComparison";
            public static string stencilOperationName = "_StencilOperation";
            public static string disableTexMapAName = "_DISABLE_TEX_MAP_A";
            public static string disableTexMapBName = "_DISABLE_TEX_MAP_B";
            public static string disableTexMapCName = "_DISABLE_TEX_MAP_C";
            public static string albedoMapAlphaMetallicName = "_METALLIC_TEXTURE_ALBEDO_CHANNEL_A";
            public static string albedoMapAlphaSmoothnessName = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";
            public static string propertiesComponentHelp = "Use the {0} component(s) to control {1} properties.";
            public static readonly string[] albedoAlphaModeNames = Enum.GetNames(typeof(AlbedoAlphaMode));
            public static GUIContent instancedColor = new GUIContent("Instanced Color", "Enable a Unique Color Per Instance");
            public static GUIContent texA = new GUIContent("Texture A", "Texture A");
            public static GUIContent texB = new GUIContent("Texture B", "Texture B");
            public static GUIContent texC = new GUIContent("Texture C", "Texture C");
            public static GUIContent albedoColor = new GUIContent("Albedo Color", "Albedo Color");
            public static GUIContent albedoAssignedAtRuntime = new GUIContent("Assigned at Runtime", "As an optimization albedo operations are disabled when no albedo texture is specified. If a albedo texture will be specified at runtime enable this option.");
            public static GUIContent alphaCutoff = new GUIContent("Alpha Cutoff", "Threshold for Alpha Cutoff");
            public static GUIContent textureExponent = new GUIContent("Texture Exponent", "Texture Exponent");
            public static GUIContent metallic = new GUIContent("Metallic", "Metallic Value");
            public static GUIContent smoothness = new GUIContent("Smoothness", "Smoothness Value");
            public static GUIContent enableNormalMap = new GUIContent("Normal Map", "Enable Normal Map");
            public static GUIContent normalMap = new GUIContent("Normal Map");
            public static GUIContent normalMapScale = new GUIContent("Scale", "Scales the Normal Map Normal");
            public static GUIContent enableEmission = new GUIContent("Emission", "Enable Emission");
            public static GUIContent emissiveColor = new GUIContent("Color");
            public static GUIContent directionalLight = new GUIContent("Directional Light", "Affected by One Unity Directional Light");
            public static GUIContent specularHighlights = new GUIContent("Specular Highlights", "Calculate Specular Highlights");
            public static GUIContent sphericalHarmonics = new GUIContent("Spherical Harmonics", "Read From Spherical Harmonics Data for Ambient Light");
            public static GUIContent reflections = new GUIContent("Reflections", "Calculate Glossy Reflections");
            public static GUIContent refraction = new GUIContent("Refraction", "Calculate Refraction");
            public static GUIContent refractiveIndex = new GUIContent("Refractive Index", "Ratio of Indices of Refraction at the Surface Interface");
            public static GUIContent rimLight = new GUIContent("Rim Light", "Enable Rim (Fresnel) Lighting");
            public static GUIContent rimColor = new GUIContent("Color", "Rim Highlight Color");
            public static GUIContent rimPower = new GUIContent("Power", "Rim Highlight Saturation");
            public static GUIContent vertexColors = new GUIContent("Vertex Colors", "Enable Vertex Color Tinting");
            public static GUIContent vertexExtrusion = new GUIContent("Vertex Extrusion", "Enable Vertex Extrusion Along the Vertex Normal");
            public static GUIContent vertexExtrusionValue = new GUIContent("Vertex Extrusion Value", "How Far to Extrude the Vertex Along the Vertex Normal");
            public static GUIContent blendedClippingWidth = new GUIContent("Blended Clipping Width", "The Width of the Clipping Primitive Clip Fade Region on Non-Cutout Materials");
            public static GUIContent clippingBorder = new GUIContent("Clipping Border", "Enable a Border Along the Clipping Primitive's Edge");
            public static GUIContent clippingBorderWidth = new GUIContent("Width", "Width of the Clipping Border");
            public static GUIContent clippingBorderColor = new GUIContent("Color", "Interpolated Color of the Clipping Border");
            public static GUIContent nearPlaneFade = new GUIContent("Near Fade", "Objects Disappear (Turn to Black/Transparent) as the Camera (or Hover/Proximity Light) Nears Them");
            public static GUIContent nearLightFade = new GUIContent("Use Light", "A Hover or Proximity Light (Rather Than the Camera) Determines Near Fade Distance");
            public static GUIContent fadeBeginDistance = new GUIContent("Fade Begin", "Distance From Camera (or Hover/Proximity Light) to Begin Fade In");
            public static GUIContent fadeCompleteDistance = new GUIContent("Fade Complete", "Distance From Camera (or Hover/Proximity Light) When Fade is Fully In");
            public static GUIContent fadeMinValue = new GUIContent("Fade Min Value", "Clamps the Fade Amount to a Minimum Value");
            public static GUIContent hoverLight = new GUIContent("Hover Light", "Enable utilization of Hover Light(s)");
            public static GUIContent enableHoverColorOverride = new GUIContent("Override Color", "Override Global Hover Light Color for this Material");
            public static GUIContent hoverColorOverride = new GUIContent("Color", "Override Hover Light Color");
            public static GUIContent proximityLight = new GUIContent("Proximity Light", "Enable utilization of Proximity Light(s)");
            public static GUIContent enableProximityLightColorOverride = new GUIContent("Override Color", "Override Global Proximity Light Color for this Material");
            public static GUIContent proximityLightCenterColorOverride = new GUIContent("Center Color", "The Override Color of the ProximityLight Gradient at the Center (RGB) and (A) is Gradient Extent");
            public static GUIContent proximityLightMiddleColorOverride = new GUIContent("Middle Color", "The Override Color of the ProximityLight Gradient at the Middle (RGB) and (A) is Gradient Extent");
            public static GUIContent proximityLightOuterColorOverride = new GUIContent("Outer Color", "The Override Color of the ProximityLight Gradient at the Outer Edge (RGB) and (A) is Gradient Extent");
            public static GUIContent proximityLightSubtractive = new GUIContent("Subtractive", "Proximity Lights Remove Light from a Surface, Used to Mimic a Shadow");
            public static GUIContent proximityLightTwoSided = new GUIContent("Two Sided", "Proximity Lights Apply to Both Sides of a Surface");
            public static GUIContent fluentLightIntensity = new GUIContent("Light Intensity", "Intensity Scaler for All Hover and Proximity Lights");
            public static GUIContent innerGlow = new GUIContent("Inner Glow", "Enable Inner Glow (Assumes UVs Specify Borders of Surface, Works Best on Unity Cube, Quad, and Plane)");
            public static GUIContent innerGlowColor = new GUIContent("Color", "Inner Glow Color (RGB) and Intensity (A)");
            public static GUIContent innerGlowPower = new GUIContent("Power", "Power Exponent to Control Glow");
            public static GUIContent environmentColoring = new GUIContent("Environment Coloring", "Change Color Based on View");
            public static GUIContent environmentColorThreshold = new GUIContent("Threshold", "Threshold When Environment Coloring Should Appear Based on Surface Normal");
            public static GUIContent environmentColorIntensity = new GUIContent("Intensity", "Intensity (or Brightness) of the Environment Coloring");
            public static GUIContent environmentColorX = new GUIContent("X-Axis Color", "Color Along the World Space X-Axis");
            public static GUIContent environmentColorY = new GUIContent("Y-Axis Color", "Color Along the World Space Y-Axis");
            public static GUIContent environmentColorZ = new GUIContent("Z-Axis Color", "Color Along the World Space Z-Axis");
            public static GUIContent stencil = new GUIContent("Enable Stencil Testing", "Enabled Stencil Testing Operations");
            public static GUIContent stencilReference = new GUIContent("Stencil Reference", "Value to Compared Against (if Comparison is Anything but Always) and/or the Value to be Written to the Buffer (if Either Pass, Fail or ZFail is Set to Replace)");
            public static GUIContent stencilComparison = new GUIContent("Stencil Comparison", "Function to Compare the Reference Value to");
            public static GUIContent stencilOperation = new GUIContent("Stencil Operation", "What to do When the Stencil Test Passes");
        }

        protected MaterialProperty texMapA;
        protected MaterialProperty texMapB;
        protected MaterialProperty texMapC;
        protected MaterialProperty waveA;
        protected MaterialProperty waveB;
        protected MaterialProperty waveC;
        protected MaterialProperty waveStretch;
        protected MaterialProperty waveShift;
        protected MaterialProperty wavePhase;
        protected MaterialProperty instancedColor;
        protected MaterialProperty albedoColor;
        protected MaterialProperty albedoAlphaMode;
        protected MaterialProperty albedoAssignedAtRuntime;
        protected MaterialProperty alphaCutoff;
        protected MaterialProperty textureExponent;
        protected MaterialProperty enableNormalMap;
        protected MaterialProperty normalMap;
        protected MaterialProperty normalMapScale;
        protected MaterialProperty enableEmission;
        protected MaterialProperty emissiveColor;
        protected MaterialProperty metallic;
        protected MaterialProperty smoothness;
        protected MaterialProperty directionalLight;
        protected MaterialProperty specularHighlights;
        protected MaterialProperty sphericalHarmonics;
        protected MaterialProperty reflections;
        protected MaterialProperty refraction;
        protected MaterialProperty refractiveIndex;
        protected MaterialProperty rimLight;
        protected MaterialProperty rimColor;
        protected MaterialProperty rimPower;
        protected MaterialProperty vertexColors;
        protected MaterialProperty vertexExtrusion;
        protected MaterialProperty vertexExtrusionValue;
        protected MaterialProperty blendedClippingWidth;
        protected MaterialProperty clippingBorder;
        protected MaterialProperty clippingBorderWidth;
        protected MaterialProperty clippingBorderColor;
        protected MaterialProperty nearPlaneFade;
        protected MaterialProperty nearLightFade;
        protected MaterialProperty fadeBeginDistance;
        protected MaterialProperty fadeCompleteDistance;
        protected MaterialProperty fadeMinValue;
        protected MaterialProperty hoverLight;
        protected MaterialProperty enableHoverColorOverride;
        protected MaterialProperty hoverColorOverride;
        protected MaterialProperty proximityLight;
        protected MaterialProperty enableProximityLightColorOverride;
        protected MaterialProperty proximityLightCenterColorOverride;
        protected MaterialProperty proximityLightMiddleColorOverride;
        protected MaterialProperty proximityLightOuterColorOverride;
        protected MaterialProperty proximityLightSubtractive;
        protected MaterialProperty proximityLightTwoSided;
        protected MaterialProperty fluentLightIntensity;
        protected MaterialProperty innerGlow;
        protected MaterialProperty innerGlowColor;
        protected MaterialProperty innerGlowPower;
        protected MaterialProperty environmentColoring;
        protected MaterialProperty environmentColorThreshold;
        protected MaterialProperty environmentColorIntensity;
        protected MaterialProperty environmentColorX;
        protected MaterialProperty environmentColorY;
        protected MaterialProperty environmentColorZ;
        protected MaterialProperty stencil;
        protected MaterialProperty stencilReference;
        protected MaterialProperty stencilComparison;
        protected MaterialProperty stencilOperation;

        protected override void FindProperties(MaterialProperty[] props)
        {
            base.FindProperties(props);

            instancedColor = FindProperty(Styles.instancedColorName, props);
            texMapA = FindProperty("_TexA", props);
            texMapB = FindProperty("_TexB", props);
            texMapC = FindProperty("_TexC", props);
            waveA = FindProperty("_WaveA", props);
            waveB = FindProperty("_WaveB", props);
            waveC = FindProperty("_WaveC", props);
            waveStretch = FindProperty("_WaveStretch", props);
            waveShift = FindProperty("_WaveShift", props);
            wavePhase = FindProperty("_WavePhase", props);
            albedoColor = FindProperty("_Color", props);
            albedoAlphaMode = FindProperty("_AlbedoAlphaMode", props);
            albedoAssignedAtRuntime = FindProperty("_AlbedoAssignedAtRuntime", props);
            alphaCutoff = FindProperty("_Cutoff", props);
            textureExponent = FindProperty("_TextureExponent", props);
            metallic = FindProperty("_Metallic", props);
            smoothness = FindProperty("_Smoothness", props);
            enableNormalMap = FindProperty("_EnableNormalMap", props);
            normalMap = FindProperty("_NormalMap", props);
            normalMapScale = FindProperty("_NormalMapScale", props);
            enableEmission = FindProperty("_EnableEmission", props);
            emissiveColor = FindProperty("_EmissiveColor", props);
            directionalLight = FindProperty("_DirectionalLight", props);
            specularHighlights = FindProperty("_SpecularHighlights", props);
            sphericalHarmonics = FindProperty("_SphericalHarmonics", props);
            reflections = FindProperty("_Reflections", props);
            refraction = FindProperty("_Refraction", props);
            refractiveIndex = FindProperty("_RefractiveIndex", props);
            rimLight = FindProperty("_RimLight", props);
            rimColor = FindProperty("_RimColor", props);
            rimPower = FindProperty("_RimPower", props);
            vertexColors = FindProperty("_VertexColors", props);
            vertexExtrusion = FindProperty("_VertexExtrusion", props);
            vertexExtrusionValue = FindProperty("_VertexExtrusionValue", props);
            blendedClippingWidth = FindProperty("_BlendedClippingWidth", props);
            clippingBorder = FindProperty("_ClippingBorder", props);
            clippingBorderWidth = FindProperty("_ClippingBorderWidth", props);
            clippingBorderColor = FindProperty("_ClippingBorderColor", props);
            nearPlaneFade = FindProperty("_NearPlaneFade", props);
            nearLightFade = FindProperty("_NearLightFade", props);
            fadeBeginDistance = FindProperty("_FadeBeginDistance", props);
            fadeCompleteDistance = FindProperty("_FadeCompleteDistance", props);
            fadeMinValue = FindProperty("_FadeMinValue", props);
            hoverLight = FindProperty("_HoverLight", props);
            enableHoverColorOverride = FindProperty("_EnableHoverColorOverride", props);
            hoverColorOverride = FindProperty("_HoverColorOverride", props);
            proximityLight = FindProperty("_ProximityLight", props);
            enableProximityLightColorOverride = FindProperty("_EnableProximityLightColorOverride", props);
            proximityLightCenterColorOverride = FindProperty("_ProximityLightCenterColorOverride", props);
            proximityLightMiddleColorOverride = FindProperty("_ProximityLightMiddleColorOverride", props);
            proximityLightOuterColorOverride = FindProperty("_ProximityLightOuterColorOverride", props);
            proximityLightSubtractive = FindProperty("_ProximityLightSubtractive", props);
            proximityLightTwoSided = FindProperty("_ProximityLightTwoSided", props);
            fluentLightIntensity = FindProperty("_FluentLightIntensity", props);
            innerGlow = FindProperty("_InnerGlow", props);
            innerGlowColor = FindProperty("_InnerGlowColor", props);
            innerGlowPower = FindProperty("_InnerGlowPower", props);
            environmentColoring = FindProperty("_EnvironmentColoring", props);
            environmentColorThreshold = FindProperty("_EnvironmentColorThreshold", props);
            environmentColorIntensity = FindProperty("_EnvironmentColorIntensity", props);
            environmentColorX = FindProperty("_EnvironmentColorX", props);
            environmentColorY = FindProperty("_EnvironmentColorY", props);
            environmentColorZ = FindProperty("_EnvironmentColorZ", props);
            stencil = FindProperty("_Stencil", props);
            stencilReference = FindProperty("_StencilReference", props);
            stencilComparison = FindProperty(Styles.stencilComparisonName, props);
            stencilOperation = FindProperty(Styles.stencilOperationName, props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            Material material = (Material)materialEditor.target;

            base.OnGUI(materialEditor, props);

            SharedMapOptions(materialEditor, material);
            TextureMapOptions(materialEditor, material, texMapA, waveA, Styles.texA, Styles.disableTexMapAName);
            TextureMapOptions(materialEditor, material, texMapB, waveB, Styles.texB, Styles.disableTexMapBName);
            TextureMapOptions(materialEditor, material, texMapC, waveC, Styles.texC, Styles.disableTexMapCName);
            RenderingOptions(materialEditor, material);
            FluentOptions(materialEditor, material);
            AdvancedOptions(materialEditor, material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            // Cache old shader properties with potentially different names than the new shader.
            float? smoothness = GetFloatProperty(material, "_Glossiness");
            float? diffuse = GetFloatProperty(material, "_UseDiffuse");
            float? specularHighlights = GetFloatProperty(material, "_SpecularHighlights");
            float? normalMap = null;
            Texture normalMapTexture = material.GetTexture("_BumpMap");
            float? normalMapScale = GetFloatProperty(material, "_BumpScale");
            float? emission = null;
            Color? emissionColor = GetColorProperty(material, "_EmissionColor");
            float? reflections = null;
            float? rimLighting = null;
            Vector4? textureScaleOffset = null;
            float? cullMode = GetFloatProperty(material, "_Cull");

            if (oldShader)
            {
                if (oldShader.name.Contains("Standard"))
                {
                    normalMap = material.IsKeywordEnabled("_NORMALMAP") ? 1.0f : 0.0f;
                    emission = material.IsKeywordEnabled("_EMISSION") ? 1.0f : 0.0f;
                    reflections = GetFloatProperty(material, "_GlossyReflections");
                }
                else if (oldShader.name.Contains("Fast Configurable"))
                {
                    normalMap = material.IsKeywordEnabled("_USEBUMPMAP_ON") ? 1.0f : 0.0f;
                    emission = GetFloatProperty(material, "_UseEmissionColor");
                    reflections = GetFloatProperty(material, "_UseReflections");
                    rimLighting = GetFloatProperty(material, "_UseRimLighting");
                    textureScaleOffset = GetVectorProperty(material, "_TextureScaleOffset");
                }
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            // Apply old shader properties to the new shader.
            SetShaderFeatureActive(material, null, "_Smoothness", smoothness);
            SetShaderFeatureActive(material, "_DIRECTIONAL_LIGHT", "_DirectionalLight", diffuse);
            SetShaderFeatureActive(material, "_SPECULAR_HIGHLIGHTS", "_SpecularHighlights", specularHighlights);
            SetShaderFeatureActive(material, "_NORMAL_MAP", "_EnableNormalMap", normalMap);

            if (normalMapTexture)
            {
                material.SetTexture("_NormalMap", normalMapTexture);
            }

            SetShaderFeatureActive(material, null, "_NormalMapScale", normalMapScale);
            SetShaderFeatureActive(material, "_EMISSION", "_EnableEmission", emission);
            SetColorProperty(material, "_EmissiveColor", emissionColor);
            SetShaderFeatureActive(material, "_REFLECTIONS", "_Reflections", reflections);
            SetShaderFeatureActive(material, "_RIM_LIGHT", "_RimLight", rimLighting);
            SetShaderFeatureActive(material, null, "_CullMode", cullMode);

            // Setup the rendering mode based on the old shader.
            if (oldShader == null || !oldShader.name.Contains(LegacyShadersPath))
            {
                SetupMaterialWithRenderingMode(material, (RenderingMode)material.GetFloat(BaseStyles.renderingModeName), CustomRenderingMode.Opaque, -1);
            }
            else
            {
                RenderingMode mode = RenderingMode.Opaque;

                if (oldShader.name.Contains(TransparentCutoutShadersPath))
                {
                    mode = RenderingMode.TransparentCutout;
                }
                else if (oldShader.name.Contains(TransparentShadersPath))
                {
                    mode = RenderingMode.Transparent;
                }

                material.SetFloat(BaseStyles.renderingModeName, (float)mode);

                MaterialChanged(material);
            }
        }

        protected override void MaterialChanged(Material material)
        {
            SetupMaterialWithAlbedo(material, texMapA, albedoAlphaMode, albedoAssignedAtRuntime, Styles.disableTexMapAName);
            SetupMaterialWithAlbedo(material, texMapB, albedoAlphaMode, albedoAssignedAtRuntime, Styles.disableTexMapBName);
            SetupMaterialWithAlbedo(material, texMapC, albedoAlphaMode, albedoAssignedAtRuntime, Styles.disableTexMapCName);

            base.MaterialChanged(material);
        }

        protected void SharedMapOptions(MaterialEditor materialEditor, Material material)
        {
            materialEditor.ShaderProperty(albedoColor, Styles.albedoColor);

            materialEditor.ShaderProperty(albedoAssignedAtRuntime, Styles.albedoAssignedAtRuntime, 2);

            materialEditor.ShaderProperty(waveStretch, "Wave Stretch");
            materialEditor.ShaderProperty(waveShift, "Wave Shift");
            materialEditor.ShaderProperty(wavePhase, "Wave Phase");

            albedoAlphaMode.floatValue = EditorGUILayout.Popup(albedoAlphaMode.displayName, (int)albedoAlphaMode.floatValue, Styles.albedoAlphaModeNames);

            if ((RenderingMode)renderingMode.floatValue == RenderingMode.TransparentCutout || 
                (RenderingMode)renderingMode.floatValue == RenderingMode.Custom)
            {
                materialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoff.text);
            }

            if ((AlbedoAlphaMode)albedoAlphaMode.floatValue != AlbedoAlphaMode.Metallic)
            {
                materialEditor.ShaderProperty(metallic, Styles.metallic);
            }

            if ((AlbedoAlphaMode)albedoAlphaMode.floatValue != AlbedoAlphaMode.Smoothness)
            {
                materialEditor.ShaderProperty(smoothness, Styles.smoothness);
            }

            if (PropertyEnabled(directionalLight) ||
                PropertyEnabled(reflections) ||
                PropertyEnabled(rimLight) ||
                PropertyEnabled(environmentColoring))
            {
                materialEditor.ShaderProperty(enableNormalMap, Styles.enableNormalMap);

                if (PropertyEnabled(enableNormalMap))
                {
                    EditorGUI.indentLevel += 2;
                    materialEditor.TexturePropertySingleLine(Styles.normalMap, normalMap, normalMap.textureValue != null ? normalMapScale : null);
                    EditorGUI.indentLevel -= 2;
                }
            }

            materialEditor.ShaderProperty(enableEmission, Styles.enableEmission);

            if (PropertyEnabled(enableEmission))
            {
                materialEditor.ShaderProperty(emissiveColor, Styles.emissiveColor, 2);
            }
        }

        protected void TextureMapOptions(
            MaterialEditor materialEditor,
            Material material,
            MaterialProperty albedoMap,
            MaterialProperty wave,
            GUIContent style,
            string disableMapFeature)
        {
            materialEditor.TexturePropertySingleLine(style, albedoMap);

            SetupMaterialWithAlbedo(material, albedoMap, albedoAlphaMode, albedoAssignedAtRuntime, disableMapFeature);

            EditorGUILayout.Space();

            EditorGUI.indentLevel += 2;

            materialEditor.TextureScaleOffsetProperty(albedoMap);

            materialEditor.ShaderProperty(wave, "Wave Vector");

            EditorGUI.indentLevel -= 2;
        }

        protected void RenderingOptions(MaterialEditor materialEditor, Material material)
        {
            EditorGUILayout.Space();
            GUILayout.Label(Styles.renderingOptionsTitle, EditorStyles.boldLabel);

            materialEditor.ShaderProperty(textureExponent, Styles.textureExponent);

            materialEditor.ShaderProperty(directionalLight, Styles.directionalLight);

            if (PropertyEnabled(directionalLight))
            {
                materialEditor.ShaderProperty(specularHighlights, Styles.specularHighlights, 2);
            }

            materialEditor.ShaderProperty(sphericalHarmonics, Styles.sphericalHarmonics);

            materialEditor.ShaderProperty(reflections, Styles.reflections);

            if (PropertyEnabled(reflections))
            {
                materialEditor.ShaderProperty(refraction, Styles.refraction, 2);

                if (PropertyEnabled(refraction))
                {
                    materialEditor.ShaderProperty(refractiveIndex, Styles.refractiveIndex, 4);
                }
            }

            materialEditor.ShaderProperty(rimLight, Styles.rimLight);

            if (PropertyEnabled(rimLight))
            {
                materialEditor.ShaderProperty(rimColor, Styles.rimColor, 2);
                materialEditor.ShaderProperty(rimPower, Styles.rimPower, 2);
            }

            materialEditor.ShaderProperty(vertexColors, Styles.vertexColors);

            materialEditor.ShaderProperty(vertexExtrusion, Styles.vertexExtrusion);

            if (PropertyEnabled(vertexExtrusion))
            {
                materialEditor.ShaderProperty(vertexExtrusionValue, Styles.vertexExtrusionValue, 2);
            }

            if ((RenderingMode)renderingMode.floatValue != RenderingMode.Opaque &&
                (RenderingMode)renderingMode.floatValue != RenderingMode.TransparentCutout)
            {
                materialEditor.ShaderProperty(blendedClippingWidth, Styles.blendedClippingWidth);
                GUILayout.Box(string.Format(Styles.propertiesComponentHelp, nameof(ClippingPrimitive), "other clipping"), EditorStyles.helpBox, new GUILayoutOption[0]);
            }

            materialEditor.ShaderProperty(clippingBorder, Styles.clippingBorder);

            if (PropertyEnabled(clippingBorder))
            {
                materialEditor.ShaderProperty(clippingBorderWidth, Styles.clippingBorderWidth, 2);
                materialEditor.ShaderProperty(clippingBorderColor, Styles.clippingBorderColor, 2);
                GUILayout.Box(string.Format(Styles.propertiesComponentHelp, nameof(ClippingPrimitive), "other clipping"), EditorStyles.helpBox, new GUILayoutOption[0]);
            }

            materialEditor.ShaderProperty(nearPlaneFade, Styles.nearPlaneFade);

            if (PropertyEnabled(nearPlaneFade))
            {
                materialEditor.ShaderProperty(nearLightFade, Styles.nearLightFade, 2);
                materialEditor.ShaderProperty(fadeBeginDistance, Styles.fadeBeginDistance, 2);
                materialEditor.ShaderProperty(fadeCompleteDistance, Styles.fadeCompleteDistance, 2);
                materialEditor.ShaderProperty(fadeMinValue, Styles.fadeMinValue, 2);
            }
        }

        protected void FluentOptions(MaterialEditor materialEditor, Material material)
        {
            EditorGUILayout.Space();
            GUILayout.Label(Styles.fluentOptionsTitle, EditorStyles.boldLabel);
            RenderingMode mode = (RenderingMode)renderingMode.floatValue;
            CustomRenderingMode customMode = (CustomRenderingMode)customRenderingMode.floatValue;

            materialEditor.ShaderProperty(hoverLight, Styles.hoverLight);

            if (PropertyEnabled(hoverLight))
            {
                GUILayout.Box(string.Format(Styles.propertiesComponentHelp, nameof(HoverLight), Styles.hoverLight.text), EditorStyles.helpBox, new GUILayoutOption[0]);

                materialEditor.ShaderProperty(enableHoverColorOverride, Styles.enableHoverColorOverride, 2);

                if (PropertyEnabled(enableHoverColorOverride))
                {
                    materialEditor.ShaderProperty(hoverColorOverride, Styles.hoverColorOverride, 4);
                }
            }

            materialEditor.ShaderProperty(proximityLight, Styles.proximityLight);

            if (PropertyEnabled(proximityLight))
            {
                materialEditor.ShaderProperty(enableProximityLightColorOverride, Styles.enableProximityLightColorOverride, 2);

                if (PropertyEnabled(enableProximityLightColorOverride))
                {
                    materialEditor.ShaderProperty(proximityLightCenterColorOverride, Styles.proximityLightCenterColorOverride, 4);
                    materialEditor.ShaderProperty(proximityLightMiddleColorOverride, Styles.proximityLightMiddleColorOverride, 4);
                    materialEditor.ShaderProperty(proximityLightOuterColorOverride, Styles.proximityLightOuterColorOverride, 4);
                }

                materialEditor.ShaderProperty(proximityLightSubtractive, Styles.proximityLightSubtractive, 2);
                materialEditor.ShaderProperty(proximityLightTwoSided, Styles.proximityLightTwoSided, 2);
                GUILayout.Box(string.Format(Styles.propertiesComponentHelp, nameof(ProximityLight), Styles.proximityLight.text), EditorStyles.helpBox, new GUILayoutOption[0]);
            }

            if (PropertyEnabled(hoverLight) || PropertyEnabled(proximityLight))
            {
                materialEditor.ShaderProperty(fluentLightIntensity, Styles.fluentLightIntensity);
            }

            materialEditor.ShaderProperty(innerGlow, Styles.innerGlow);

            if (PropertyEnabled(innerGlow))
            {
                materialEditor.ShaderProperty(innerGlowColor, Styles.innerGlowColor, 2);
                materialEditor.ShaderProperty(innerGlowPower, Styles.innerGlowPower, 2);
            }

            materialEditor.ShaderProperty(environmentColoring, Styles.environmentColoring);

            if (PropertyEnabled(environmentColoring))
            {
                materialEditor.ShaderProperty(environmentColorThreshold, Styles.environmentColorThreshold, 2);
                materialEditor.ShaderProperty(environmentColorIntensity, Styles.environmentColorIntensity, 2);
                materialEditor.ShaderProperty(environmentColorX, Styles.environmentColorX, 2);
                materialEditor.ShaderProperty(environmentColorY, Styles.environmentColorY, 2);
                materialEditor.ShaderProperty(environmentColorZ, Styles.environmentColorZ, 2);
            }
        }

        protected void AdvancedOptions(MaterialEditor materialEditor, Material material)
        {
            EditorGUILayout.Space();
            GUILayout.Label(Styles.advancedOptionsTitle, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            materialEditor.ShaderProperty(renderQueueOverride, BaseStyles.renderQueueOverride);

            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material);
            }

            // Show the RenderQueueField but do not allow users to directly manipulate it. That is done via the renderQueueOverride.
            GUI.enabled = false;
            materialEditor.RenderQueueField();

            // Enable instancing to disable batching. Static and dynamic batching will normalize the object scale, which breaks 
            // features which utilize object scale.
            GUI.enabled = true;

            if (!GUI.enabled && !material.enableInstancing)
            {
                material.enableInstancing = true;
            }

            materialEditor.EnableInstancingField();

            if (material.enableInstancing)
            {
                GUI.enabled = true;
                materialEditor.ShaderProperty(instancedColor, Styles.instancedColor, 2);
            }
            else
            {
                // When instancing is disable, disable instanced color.
                SetShaderFeatureActive(material, Styles.instancedColorFeatureName, Styles.instancedColorName, 0.0f);
            }

            materialEditor.ShaderProperty(stencil, Styles.stencil);

            if (PropertyEnabled(stencil))
            {
                materialEditor.ShaderProperty(stencilReference, Styles.stencilReference, 2);
                materialEditor.ShaderProperty(stencilComparison, Styles.stencilComparison, 2);
                materialEditor.ShaderProperty(stencilOperation, Styles.stencilOperation, 2);
            }
            else
            {
                // When stencil is disable, revert to the default stencil operations. Note, when tested on D3D11 hardware the stencil state 
                // is still set even when the CompareFunction.Disabled is selected, but this does not seem to affect performance.
                material.SetInt(Styles.stencilComparisonName, (int)CompareFunction.Disabled);
                material.SetInt(Styles.stencilOperationName, (int)StencilOp.Keep);
            }
        }

        protected static void SetupMaterialWithAlbedo(
            Material material,
            MaterialProperty albedoMap,
            MaterialProperty albedoAlphaMode,
            MaterialProperty albedoAssignedAtRuntime,
            string disableMapFeature)
        {
            if (albedoMap != null && (albedoMap.textureValue || PropertyEnabled(albedoAssignedAtRuntime)))
            {
                material.DisableKeyword(disableMapFeature);
            }
            else
            {
                material.EnableKeyword(disableMapFeature);
            }

            switch ((AlbedoAlphaMode)albedoAlphaMode.floatValue)
            {
                case AlbedoAlphaMode.Transparency:
                    {
                        material.DisableKeyword(Styles.albedoMapAlphaMetallicName);
                        material.DisableKeyword(Styles.albedoMapAlphaSmoothnessName);
                    }
                    break;

                case AlbedoAlphaMode.Metallic:
                    {
                        material.EnableKeyword(Styles.albedoMapAlphaMetallicName);
                        material.DisableKeyword(Styles.albedoMapAlphaSmoothnessName);
                    }
                    break;

                case AlbedoAlphaMode.Smoothness:
                    {
                        material.DisableKeyword(Styles.albedoMapAlphaMetallicName);
                        material.EnableKeyword(Styles.albedoMapAlphaSmoothnessName);
                    }
                    break;
            }
        }

        [MenuItem("Mixed Reality Toolkit/Utilities/Upgrade MRTK Standard Shader for Lightweight Render Pipeline")]
        protected static void UpgradeShaderForLightweightRenderPipeline()
        {
            if (EditorUtility.DisplayDialog("Upgrade MRTK Standard Shader?", 
                                            "This will alter the MRTK Standard Shader for use with Unity's Lightweight Render Pipeline. You cannot undo this action.", 
                                            "Ok", 
                                            "Cancel"))
            {
                string shaderName = "Mixed Reality Toolkit/Standard";
                string path = AssetDatabase.GetAssetPath(Shader.Find(shaderName));

                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        string upgradedShader = File.ReadAllText(path);
                        upgradedShader = upgradedShader.Replace("Tags{ \"RenderType\" = \"Opaque\" \"LightMode\" = \"ForwardBase\" }",
                                                                "Tags{ \"RenderType\" = \"Opaque\" \"LightMode\" = \"LightweightForward\" }");
                        upgradedShader = upgradedShader.Replace("//#define _LIGHTWEIGHT_RENDER_PIPELINE",
                                                                "#define _LIGHTWEIGHT_RENDER_PIPELINE");
                        File.WriteAllText(path, upgradedShader);
                        AssetDatabase.Refresh();

                        Debug.LogFormat("Upgraded {0} for use with the Lightweight Render Pipeline.", path);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Failed to get asset path to: {0}", shaderName);
                }
            }
        }

        [MenuItem("Mixed Reality Toolkit/Utilities/Upgrade MRTK Standard Shader for Lightweight Render Pipeline", true)]
        protected static bool UpgradeShaderForLightweightRenderPipelineValidate()
        {
            // If a scriptable render pipeline is not present, no need to upgrade the shader.
            return GraphicsSettings.renderPipelineAsset != null;
        }
    }
}

#endif