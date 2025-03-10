using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing
{
    using SSRReflectionBlendType = ScreenSpaceReflectionModel.SSRReflectionBlendType;
    using SSRResolution = ScreenSpaceReflectionModel.SSRResolution;

    public sealed class ScreenSpaceReflectionComponent : PostProcessingComponentCommandBuffer<ScreenSpaceReflectionModel>
    {
        private static class Uniforms
        {
            internal static readonly int _RayStepSize = Shader.PropertyToID("_RayStepSize");
            internal static readonly int _AdditiveReflection = Shader.PropertyToID("_AdditiveReflection");
            internal static readonly int _BilateralUpsampling = Shader.PropertyToID("_BilateralUpsampling");
            internal static readonly int _TreatBackfaceHitAsMiss = Shader.PropertyToID("_TreatBackfaceHitAsMiss");
            internal static readonly int _AllowBackwardsRays = Shader.PropertyToID("_AllowBackwardsRays");
            internal static readonly int _TraceBehindObjects = Shader.PropertyToID("_TraceBehindObjects");
            internal static readonly int _MaxSteps = Shader.PropertyToID("_MaxSteps");
            internal static readonly int _FullResolutionFiltering = Shader.PropertyToID("_FullResolutionFiltering");
            internal static readonly int _HalfResolution = Shader.PropertyToID("_HalfResolution");
            internal static readonly int _HighlightSuppression = Shader.PropertyToID("_HighlightSuppression");
            internal static readonly int _PixelsPerMeterAtOneMeter = Shader.PropertyToID("_PixelsPerMeterAtOneMeter");
            internal static readonly int _ScreenEdgeFading = Shader.PropertyToID("_ScreenEdgeFading");
            internal static readonly int _ReflectionBlur = Shader.PropertyToID("_ReflectionBlur");
            internal static readonly int _MaxRayTraceDistance = Shader.PropertyToID("_MaxRayTraceDistance");
            internal static readonly int _FadeDistance = Shader.PropertyToID("_FadeDistance");
            internal static readonly int _LayerThickness = Shader.PropertyToID("_LayerThickness");
            internal static readonly int _SSRMultiplier = Shader.PropertyToID("_SSRMultiplier");
            internal static readonly int _FresnelFade = Shader.PropertyToID("_FresnelFade");
            internal static readonly int _FresnelFadePower = Shader.PropertyToID("_FresnelFadePower");
            internal static readonly int _ReflectionBufferSize = Shader.PropertyToID("_ReflectionBufferSize");
            internal static readonly int _ScreenSize = Shader.PropertyToID("_ScreenSize");
            internal static readonly int _InvScreenSize = Shader.PropertyToID("_InvScreenSize");
            internal static readonly int _ProjInfo = Shader.PropertyToID("_ProjInfo");
            internal static readonly int _CameraClipInfo = Shader.PropertyToID("_CameraClipInfo");
            internal static readonly int _ProjectToPixelMatrix = Shader.PropertyToID("_ProjectToPixelMatrix");
            internal static readonly int _WorldToCameraMatrix = Shader.PropertyToID("_WorldToCameraMatrix");
            internal static readonly int _CameraToWorldMatrix = Shader.PropertyToID("_CameraToWorldMatrix");
            internal static readonly int _Axis = Shader.PropertyToID("_Axis");
            internal static readonly int _CurrentMipLevel = Shader.PropertyToID("_CurrentMipLevel");
            internal static readonly int _NormalAndRoughnessTexture = Shader.PropertyToID("_NormalAndRoughnessTexture");
            internal static readonly int _HitPointTexture = Shader.PropertyToID("_HitPointTexture");
            internal static readonly int _BlurTexture = Shader.PropertyToID("_BlurTexture");
            internal static readonly int _FilteredReflections = Shader.PropertyToID("_FilteredReflections");
            internal static readonly int _FinalReflectionTexture = Shader.PropertyToID("_FinalReflectionTexture");
            internal static readonly int _TempTexture = Shader.PropertyToID("_TempTexture");
        }

        // Unexposed variables
        private bool k_HighlightSuppression = false;
        private bool k_TraceBehindObjects = true;
        private bool k_TreatBackfaceHitAsMiss = false;
        private bool k_BilateralUpsample = true;

        private enum PassIndex
        {
            RayTraceStep = 0,
            CompositeFinal = 1,
            Blur = 2,
            CompositeSSR = 3,
            MinMipGeneration = 4,
            HitPointToReflections = 5,
            BilateralKeyPack = 6,
            BlitDepthAsCSZ = 7,
            PoissonBlur = 8,
        }

        private readonly int[] m_ReflectionTextures = new int[5];

        // Not really needed as SSR only works in deferred right now
        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.Depth;
        }

        public override bool active => this.model.enabled
                       && this.context.isGBufferAvailable
                       && !this.context.interrupted;

        public override void OnEnable()
        {
            this.m_ReflectionTextures[0] = Shader.PropertyToID("_ReflectionTexture0");
            this.m_ReflectionTextures[1] = Shader.PropertyToID("_ReflectionTexture1");
            this.m_ReflectionTextures[2] = Shader.PropertyToID("_ReflectionTexture2");
            this.m_ReflectionTextures[3] = Shader.PropertyToID("_ReflectionTexture3");
            this.m_ReflectionTextures[4] = Shader.PropertyToID("_ReflectionTexture4");
        }

        public override string GetName()
        {
            return "Screen Space Reflection";
        }

        public override CameraEvent GetCameraEvent()
        {
            return CameraEvent.AfterFinalPass;
        }

        public override void PopulateCommandBuffer(CommandBuffer cb)
        {
            ScreenSpaceReflectionModel.Settings settings = this.model.settings;
            Camera camera = this.context.camera;

            // Material setup
            int downsampleAmount = (settings.reflection.reflectionQuality == SSRResolution.High) ? 1 : 2;

            int rtW = this.context.width / downsampleAmount;
            int rtH = this.context.height / downsampleAmount;

            float sWidth = this.context.width;
            float sHeight = this.context.height;

            float sx = sWidth / 2f;
            float sy = sHeight / 2f;

            Material material = this.context.materialFactory.Get("Hidden/Post FX/Screen Space Reflection");

            material.SetInt(Uniforms._RayStepSize, settings.reflection.stepSize);
            material.SetInt(Uniforms._AdditiveReflection, settings.reflection.blendType == SSRReflectionBlendType.Additive ? 1 : 0);
            material.SetInt(Uniforms._BilateralUpsampling, this.k_BilateralUpsample ? 1 : 0);
            material.SetInt(Uniforms._TreatBackfaceHitAsMiss, this.k_TreatBackfaceHitAsMiss ? 1 : 0);
            material.SetInt(Uniforms._AllowBackwardsRays, settings.reflection.reflectBackfaces ? 1 : 0);
            material.SetInt(Uniforms._TraceBehindObjects, this.k_TraceBehindObjects ? 1 : 0);
            material.SetInt(Uniforms._MaxSteps, settings.reflection.iterationCount);
            material.SetInt(Uniforms._FullResolutionFiltering, 0);
            material.SetInt(Uniforms._HalfResolution, (settings.reflection.reflectionQuality != SSRResolution.High) ? 1 : 0);
            material.SetInt(Uniforms._HighlightSuppression, this.k_HighlightSuppression ? 1 : 0);

            // The height in pixels of a 1m object if viewed from 1m away.
            float pixelsPerMeterAtOneMeter = sWidth / (-2f * Mathf.Tan(camera.fieldOfView / 180f * Mathf.PI * 0.5f));

            material.SetFloat(Uniforms._PixelsPerMeterAtOneMeter, pixelsPerMeterAtOneMeter);
            material.SetFloat(Uniforms._ScreenEdgeFading, settings.screenEdgeMask.intensity);
            material.SetFloat(Uniforms._ReflectionBlur, settings.reflection.reflectionBlur);
            material.SetFloat(Uniforms._MaxRayTraceDistance, settings.reflection.maxDistance);
            material.SetFloat(Uniforms._FadeDistance, settings.intensity.fadeDistance);
            material.SetFloat(Uniforms._LayerThickness, settings.reflection.widthModifier);
            material.SetFloat(Uniforms._SSRMultiplier, settings.intensity.reflectionMultiplier);
            material.SetFloat(Uniforms._FresnelFade, settings.intensity.fresnelFade);
            material.SetFloat(Uniforms._FresnelFadePower, settings.intensity.fresnelFadePower);

            Matrix4x4 P = camera.projectionMatrix;
            Vector4 projInfo = new Vector4(
                    -2f / (sWidth * P[0]),
                    -2f / (sHeight * P[5]),
                    (1f - P[2]) / P[0],
                    (1f + P[6]) / P[5]
                    );

            Vector3 cameraClipInfo = float.IsPositiveInfinity(camera.farClipPlane) ?
                new Vector3(camera.nearClipPlane, -1f, 1f) :
                new Vector3(camera.nearClipPlane * camera.farClipPlane, camera.nearClipPlane - camera.farClipPlane, camera.farClipPlane);

            material.SetVector(Uniforms._ReflectionBufferSize, new Vector2(rtW, rtH));
            material.SetVector(Uniforms._ScreenSize, new Vector2(sWidth, sHeight));
            material.SetVector(Uniforms._InvScreenSize, new Vector2(1f / sWidth, 1f / sHeight));
            material.SetVector(Uniforms._ProjInfo, projInfo); // used for unprojection

            material.SetVector(Uniforms._CameraClipInfo, cameraClipInfo);

            Matrix4x4 warpToScreenSpaceMatrix = new Matrix4x4();
            warpToScreenSpaceMatrix.SetRow(0, new Vector4(sx, 0f, 0f, sx));
            warpToScreenSpaceMatrix.SetRow(1, new Vector4(0f, sy, 0f, sy));
            warpToScreenSpaceMatrix.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
            warpToScreenSpaceMatrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

            Matrix4x4 projectToPixelMatrix = warpToScreenSpaceMatrix * P;

            material.SetMatrix(Uniforms._ProjectToPixelMatrix, projectToPixelMatrix);
            material.SetMatrix(Uniforms._WorldToCameraMatrix, camera.worldToCameraMatrix);
            material.SetMatrix(Uniforms._CameraToWorldMatrix, camera.worldToCameraMatrix.inverse);

            // Command buffer setup
            RenderTextureFormat intermediateFormat = this.context.isHdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
            const int maxMip = 5;

            int kNormalAndRoughnessTexture = Uniforms._NormalAndRoughnessTexture;
            int kHitPointTexture = Uniforms._HitPointTexture;
            int kBlurTexture = Uniforms._BlurTexture;
            int kFilteredReflections = Uniforms._FilteredReflections;
            int kFinalReflectionTexture = Uniforms._FinalReflectionTexture;
            int kTempTexture = Uniforms._TempTexture;

            // RGB: Normals, A: Roughness.
            // Has the nice benefit of allowing us to control the filtering mode as well.
            cb.GetTemporaryRT(kNormalAndRoughnessTexture, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            cb.GetTemporaryRT(kHitPointTexture, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

            for (int i = 0; i < maxMip; ++i)
            {
                // We explicitly interpolate during bilateral upsampling.
                cb.GetTemporaryRT(this.m_ReflectionTextures[i], rtW >> i, rtH >> i, 0, FilterMode.Bilinear, intermediateFormat);
            }

            cb.GetTemporaryRT(kFilteredReflections, rtW, rtH, 0, this.k_BilateralUpsample ? FilterMode.Point : FilterMode.Bilinear, intermediateFormat);
            cb.GetTemporaryRT(kFinalReflectionTexture, rtW, rtH, 0, FilterMode.Point, intermediateFormat);

            cb.Blit(BuiltinRenderTextureType.CameraTarget, kNormalAndRoughnessTexture, material, (int)PassIndex.BilateralKeyPack);
            cb.Blit(BuiltinRenderTextureType.CameraTarget, kHitPointTexture, material, (int)PassIndex.RayTraceStep);
            cb.Blit(BuiltinRenderTextureType.CameraTarget, kFilteredReflections, material, (int)PassIndex.HitPointToReflections);
            cb.Blit(kFilteredReflections, this.m_ReflectionTextures[0], material, (int)PassIndex.PoissonBlur);

            for (int i = 1; i < maxMip; ++i)
            {
                int inputTex = this.m_ReflectionTextures[i - 1];

                int lowMip = i;

                cb.GetTemporaryRT(kBlurTexture, rtW >> lowMip, rtH >> lowMip, 0, FilterMode.Bilinear, intermediateFormat);
                cb.SetGlobalVector(Uniforms._Axis, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
                cb.SetGlobalFloat(Uniforms._CurrentMipLevel, i - 1.0f);

                cb.Blit(inputTex, kBlurTexture, material, (int)PassIndex.Blur);

                cb.SetGlobalVector(Uniforms._Axis, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));

                inputTex = this.m_ReflectionTextures[i];
                cb.Blit(kBlurTexture, inputTex, material, (int)PassIndex.Blur);
                cb.ReleaseTemporaryRT(kBlurTexture);
            }

            cb.Blit(this.m_ReflectionTextures[0], kFinalReflectionTexture, material, (int)PassIndex.CompositeSSR);

            cb.GetTemporaryRT(kTempTexture, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Bilinear, intermediateFormat);

            cb.Blit(BuiltinRenderTextureType.CameraTarget, kTempTexture, material, (int)PassIndex.CompositeFinal);
            cb.Blit(kTempTexture, BuiltinRenderTextureType.CameraTarget);

            cb.ReleaseTemporaryRT(kTempTexture);
        }
    }
}
