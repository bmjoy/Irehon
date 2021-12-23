using UnityEngine;
using UnityEngine.Events;

namespace DuloGames.UI.Tweens
{
    public struct Vector2Tween : ITweenValue
    {
        public class Vector2TweenCallback : UnityEvent<Vector2> { }
        public class Vector2TweenFinishCallback : UnityEvent { }

        private Vector2 m_StartVector2;
        private Vector2 m_TargetVector2;
        private float m_Duration;
        private bool m_IgnoreTimeScale;
        private TweenEasing m_Easing;
        private Vector2TweenCallback m_Target;
        private Vector2TweenFinishCallback m_Finish;

        /// <summary>
        /// Gets or sets the starting Vector2.
        /// </summary>
        /// <value>The start color.</value>
        public Vector2 startVector2
        {
            get => this.m_StartVector2;
            set => this.m_StartVector2 = value;
        }

        /// <summary>
        /// Gets or sets the target Vector2.
        /// </summary>
        /// <value>The color of the target.</value>
        public Vector2 targetVector2
        {
            get => this.m_TargetVector2;
            set => this.m_TargetVector2 = value;
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
        /// Tweens the color based on percentage.
        /// </summary>
        /// <param name="floatPercentage">Float percentage.</param>
        public void TweenValue(float floatPercentage)
        {
            if (!this.ValidTarget())
            {
                return;
            }

            this.m_Target.Invoke(Vector2.Lerp(this.m_StartVector2, this.m_TargetVector2, floatPercentage));
        }

        /// <summary>
        /// Adds a on changed callback.
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void AddOnChangedCallback(UnityAction<Vector2> callback)
        {
            if (this.m_Target == null)
            {
                this.m_Target = new Vector2TweenCallback();
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
                this.m_Finish = new Vector2TweenFinishCallback();
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
