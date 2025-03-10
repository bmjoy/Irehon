using System;

namespace UnityEngine.PostProcessing
{
    [Serializable]
    public class BuiltinDebugViewsModel : PostProcessingModel
    {
        [Serializable]
        public struct DepthSettings
        {
            [Range(0f, 1f), Tooltip("Scales the camera far plane before displaying the depth map.")]
            public float scale;

            public static DepthSettings defaultSettings => new DepthSettings
            {
                scale = 1f
            };
        }

        [Serializable]
        public struct MotionVectorsSettings
        {
            [Range(0f, 1f), Tooltip("Opacity of the source render.")]
            public float sourceOpacity;

            [Range(0f, 1f), Tooltip("Opacity of the per-pixel motion vector colors.")]
            public float motionImageOpacity;

            [Min(0f), Tooltip("Because motion vectors are mainly very small vectors, you can use this setting to make them more visible.")]
            public float motionImageAmplitude;

            [Range(0f, 1f), Tooltip("Opacity for the motion vector arrows.")]
            public float motionVectorsOpacity;

            [Range(8, 64), Tooltip("The arrow density on screen.")]
            public int motionVectorsResolution;

            [Min(0f), Tooltip("Tweaks the arrows length.")]
            public float motionVectorsAmplitude;

            public static MotionVectorsSettings defaultSettings => new MotionVectorsSettings
            {
                sourceOpacity = 1f,

                motionImageOpacity = 0f,
                motionImageAmplitude = 16f,

                motionVectorsOpacity = 1f,
                motionVectorsResolution = 24,
                motionVectorsAmplitude = 64f
            };
        }

        public enum Mode
        {
            None,

            Depth,
            Normals,
            MotionVectors,

            AmbientOcclusion,
            EyeAdaptation,
            FocusPlane,
            PreGradingLog,
            LogLut,
            UserLut
        }

        [Serializable]
        public struct Settings
        {
            public Mode mode;
            public DepthSettings depth;
            public MotionVectorsSettings motionVectors;

            public static Settings defaultSettings => new Settings
            {
                mode = Mode.None,
                depth = DepthSettings.defaultSettings,
                motionVectors = MotionVectorsSettings.defaultSettings
            };
        }

        [SerializeField]
        private Settings m_Settings = Settings.defaultSettings;
        public Settings settings
        {
            get => this.m_Settings;
            set => this.m_Settings = value;
        }

        public bool willInterrupt => !this.IsModeActive(Mode.None)
                       && !this.IsModeActive(Mode.EyeAdaptation)
                       && !this.IsModeActive(Mode.PreGradingLog)
                       && !this.IsModeActive(Mode.LogLut)
                       && !this.IsModeActive(Mode.UserLut);

        public override void Reset()
        {
            this.settings = Settings.defaultSettings;
        }

        public bool IsModeActive(Mode mode)
        {
            return this.m_Settings.mode == mode;
        }
    }
}
