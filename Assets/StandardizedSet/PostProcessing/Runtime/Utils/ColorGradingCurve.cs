using System;

namespace UnityEngine.PostProcessing
{
    // Small wrapper on top of AnimationCurve to handle zero-key curves and keyframe looping

    [Serializable]
    public sealed class ColorGradingCurve
    {
        public AnimationCurve curve;

        [SerializeField]
        private bool m_Loop;

        [SerializeField]
        private float m_ZeroValue;

        [SerializeField]
        private float m_Range;
        private AnimationCurve m_InternalLoopingCurve;

        public ColorGradingCurve(AnimationCurve curve, float zeroValue, bool loop, Vector2 bounds)
        {
            this.curve = curve;
            this.m_ZeroValue = zeroValue;
            this.m_Loop = loop;
            this.m_Range = bounds.magnitude;
        }

        public void Cache()
        {
            if (!this.m_Loop)
            {
                return;
            }

            int length = this.curve.length;

            if (length < 2)
            {
                return;
            }

            if (this.m_InternalLoopingCurve == null)
            {
                this.m_InternalLoopingCurve = new AnimationCurve();
            }

            Keyframe prev = this.curve[length - 1];
            prev.time -= this.m_Range;
            Keyframe next = this.curve[0];
            next.time += this.m_Range;
            this.m_InternalLoopingCurve.keys = this.curve.keys;
            this.m_InternalLoopingCurve.AddKey(prev);
            this.m_InternalLoopingCurve.AddKey(next);
        }

        public float Evaluate(float t)
        {
            if (this.curve.length == 0)
            {
                return this.m_ZeroValue;
            }

            if (!this.m_Loop || this.curve.length == 1)
            {
                return this.curve.Evaluate(t);
            }

            return this.m_InternalLoopingCurve.Evaluate(t);
        }
    }
}
