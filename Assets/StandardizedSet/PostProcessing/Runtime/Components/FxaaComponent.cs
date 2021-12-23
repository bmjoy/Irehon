namespace UnityEngine.PostProcessing
{
    public sealed class FxaaComponent : PostProcessingComponentRenderTexture<AntialiasingModel>
    {
        private static class Uniforms
        {
            internal static readonly int _QualitySettings = Shader.PropertyToID("_QualitySettings");
            internal static readonly int _ConsoleSettings = Shader.PropertyToID("_ConsoleSettings");
        }

        public override bool active => this.model.enabled
                       && this.model.settings.method == AntialiasingModel.Method.Fxaa
                       && !this.context.interrupted;

        public void Render(RenderTexture source, RenderTexture destination)
        {
            AntialiasingModel.FxaaSettings settings = this.model.settings.fxaaSettings;
            Material material = this.context.materialFactory.Get("Hidden/Post FX/FXAA");
            AntialiasingModel.FxaaQualitySettings qualitySettings = AntialiasingModel.FxaaQualitySettings.presets[(int)settings.preset];
            AntialiasingModel.FxaaConsoleSettings consoleSettings = AntialiasingModel.FxaaConsoleSettings.presets[(int)settings.preset];

            material.SetVector(Uniforms._QualitySettings,
                new Vector3(
                    qualitySettings.subpixelAliasingRemovalAmount,
                    qualitySettings.edgeDetectionThreshold,
                    qualitySettings.minimumRequiredLuminance
                    )
                );

            material.SetVector(Uniforms._ConsoleSettings,
                new Vector4(
                    consoleSettings.subpixelSpreadAmount,
                    consoleSettings.edgeSharpnessAmount,
                    consoleSettings.edgeDetectionThreshold,
                    consoleSettings.minimumRequiredLuminance
                    )
                );

            Graphics.Blit(source, destination, material, 0);
        }
    }
}
