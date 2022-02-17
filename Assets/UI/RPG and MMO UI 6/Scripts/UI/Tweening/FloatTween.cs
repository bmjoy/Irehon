using UnityEngine;
using UnityEngine.Events;

namespace DuloGames.UI.Tweens
{
    public struct FloatTween : ITweenValue
    {
        public class FloatTweenCallback : UnityEvent<float> { }
        public class FloatFinishCallback : UnityEvent { }

        private float m_StartFloat;
        private float m_TargetFloat;
        private float m_Duration;
        private bool m_IgnoreTimeScale;
        private TweenEasing m_Easing;
        private FloatTweenCallback m_Target;
        private FloatFinishCallback m_Finish;

        /// <summary>
        /// Gets or sets the starting float.
        /// </summary>
        /// <value>The start float.</value>
        public float startFloat
        {
            get => this.m_StartFloat;
            set => this.m_StartFloat = value;
        }

        /// <summary>
        /// Gets or sets the target float.
        /// </summary>
        /// <value>The target float.</value>
        public float targetFloat
        {
            get => this.m_TargetFloat;
            set => this.m_TargetFloat = value;
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
        /// Tweens the float based on percentage.
        /// </summary>
        /// <param name="floatPercentage">Float percentage.</param>
        public void TweenValue(float floatPercentage)
        {
            if (!this.ValidTarget())
            {
                return;
            }

            this.m_Target.Invoke(Mathf.Lerp(this.m_StartFloat, this.m_TargetFloat, floatPercentage));
        }

        /// <summary>
        /// Adds a on changed callback.
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void AddOnChangedCallback(UnityAction<float> callback)
        {
            if (this.m_Target == null)
            {
                this.m_Target = new FloatTweenCallback();
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
                this.m_Finish = new FloatFinishCallback();
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
