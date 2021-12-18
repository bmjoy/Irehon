using UnityEngine;
using UnityEngine.Events;

namespace DuloGames.UI.Tweens
{
    public struct ColorTween : ITweenValue
    {
        public enum ColorTweenMode
        {
            All,
            RGB,
            Alpha
        }

        public class ColorTweenCallback : UnityEvent<Color> { }
        public class ColorTweenFinishCallback : UnityEvent { }

        private Color m_StartColor;
        private Color m_TargetColor;
        private float m_Duration;
        private bool m_IgnoreTimeScale;
        private TweenEasing m_Easing;
        private ColorTween.ColorTweenMode m_TweenMode;
        private ColorTweenCallback m_Target;
        private ColorTweenFinishCallback m_Finish;

        /// <summary>
        /// Gets or sets the starting color.
        /// </summary>
        /// <value>The start color.</value>
        public Color startColor
        {
            get => this.m_StartColor;
            set => this.m_StartColor = value;
        }

        /// <summary>
        /// Gets or sets the target color.
        /// </summary>
        /// <value>The color of the target.</value>
        public Color targetColor
        {
            get => this.m_TargetColor;
            set => this.m_TargetColor = value;
        }

        /// <summary>
        /// Gets or sets the duration of the tween.
        /// </summary>
        /// <value>The duration.</value>
        public float duration
        {
            get => this.m_Duration;
            set => this.m_Duration = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UnityEngine.UI.Tweens.ColorTween"/> should ignore time scale.
        /// </summary>
        /// <value><c>true</c> if ignore time scale; otherwise, <c>false</c>.</value>
        public bool ignoreTimeScale
        {
            get => this.m_IgnoreTimeScale;
            set => this.m_IgnoreTimeScale = value;
        }

        /// <summary>
        /// Gets or sets the tween easing.
        /// </summary>
        /// <value>The easing.</value>
        public TweenEasing easing
        {
            get => this.m_Easing;
            set => this.m_Easing = value;
        }

        /// <summary>
        /// Gets or sets the tween mode.
        /// </summary>
        /// <value>The tween mode.</value>
        public ColorTween.ColorTweenMode tweenMode
        {
            get => this.m_TweenMode;
            set => this.m_TweenMode = value;
        }

        /// <summary>
        /// Tweens the color based on percentage.
        /// </summary>
        /// <param name="floatPercentage">Float percentage.</param>
        public void TweenValue(float floatPercentage)
        {
            if (!this.ValidTarget())
            {
                return;
            }

            Color arg = Color.Lerp(this.m_StartColor, this.m_TargetColor, floatPercentage);

            if (this.m_TweenMode == ColorTween.ColorTweenMode.Alpha)
            {
                arg.r = this.m_StartColor.r;
                arg.g = this.m_StartColor.g;
                arg.b = this.m_StartColor.b;
            }
            else
            {
                if (this.m_TweenMode == ColorTween.ColorTweenMode.RGB)
                {
                    arg.a = this.m_StartColor.a;
                }
            }

            this.m_Target.Invoke(arg);
        }

        /// <summary>
        /// Adds a on changed callback.
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void AddOnChangedCallback(UnityAction<Color> callback)
        {
            if (this.m_Target == null)
            {
                this.m_Target = new ColorTweenCallback();
            }

            this.m_Target.AddListener(callback);
        }

        /// <summary>
        /// Adds a on finish callback.
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void AddOnFinishCallback(UnityAction callback)
        {
            if (this.m_Finish == null)
            {
                this.m_Finish = new ColorTweenFinishCallback();
            }

            this.m_Finish.AddListener(callback);
        }

        public bool GetIgnoreTimescale()
        {
            return this.m_IgnoreTimeScale;
        }

        public float GetDuration()
        {
            return this.m_Duration;
        }

        public bool ValidTarget()
        {
            return this.m_Target != null;
        }

        /// <summary>
        /// Invokes the on finish callback.
        /// </summary>
        public void Finished()
        {
            if (this.m_Finish != null)
            {
                this.m_Finish.Invoke();
            }
        }
    }
}
