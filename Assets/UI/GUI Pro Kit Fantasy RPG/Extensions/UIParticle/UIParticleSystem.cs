/// Credit glennpow, Zarlang
/// Sourced from - http://forum.unity3d.com/threads/free-script-particle-systems-in-ui-screen-space-overlay.406862/
/// Updated by Zarlang with a more robust implementation, including TextureSheet annimation support

namespace UnityEngine.UI.Extensions
{
#if UNITY_5_3_OR_NEWER
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasRenderer), typeof(ParticleSystem))]
    [AddComponentMenu("UI/Effects/Extensions/UIParticleSystem")]
    public class UIParticleSystem : MaskableGraphic
    {
        [Tooltip("Having this enabled run the system in LateUpdate rather than in Update making it faster but less precise (more clunky)")]
        public bool fixedTime = true;

        [Tooltip("Enables 3d rotation for the particles")]
        public bool use3dRotation = false;

        private Transform _transform;
        private ParticleSystem pSystem;
        private ParticleSystem.Particle[] particles;
        private UIVertex[] _quad = new UIVertex[4];
        private Vector4 imageUV = Vector4.zero;
        private ParticleSystem.TextureSheetAnimationModule textureSheetAnimation;
        private int textureSheetAnimationFrames;
        private Vector2 textureSheetAnimationFrameSize;
        private ParticleSystemRenderer pRenderer;
        private bool isInitialised = false;

        private Material currentMaterial;

        private Texture currentTexture;

#if UNITY_5_5_OR_NEWER
        private ParticleSystem.MainModule mainModule;
#endif

        public override Texture mainTexture => this.currentTexture;

        protected bool Initialize()
        {
            // initialize members
            if (this._transform == null)
            {
                this._transform = this.transform;
            }
            if (this.pSystem == null)
            {
                this.pSystem = this.GetComponent<ParticleSystem>();

                if (this.pSystem == null)
                {
                    return false;
                }

#if UNITY_5_5_OR_NEWER
                this.mainModule = this.pSystem.main;
                if (this.pSystem.main.maxParticles > 14000)
                {
                    this.mainModule.maxParticles = 14000;
                }
#else
                    if (pSystem.maxParticles > 14000)
                        pSystem.maxParticles = 14000;
#endif

                this.pRenderer = this.pSystem.GetComponent<ParticleSystemRenderer>();
                if (this.pRenderer != null)
                {
                    this.pRenderer.enabled = false;
                }

                if (this.material == null)
                {
                    Shader foundShader = Shader.Find("UI Extensions/Particles/Additive");
                    if (foundShader)
                    {
                        this.material = new Material(foundShader);
                    }
                }

                this.currentMaterial = this.material;
                if (this.currentMaterial && this.currentMaterial.HasProperty("_MainTex"))
                {
                    this.currentTexture = this.currentMaterial.mainTexture;
                    if (this.currentTexture == null)
                    {
                        this.currentTexture = Texture2D.whiteTexture;
                    }
                }
                this.material = this.currentMaterial;
                // automatically set scaling
#if UNITY_5_5_OR_NEWER
                this.mainModule.scalingMode = ParticleSystemScalingMode.Hierarchy;
#else
                    pSystem.scalingMode = ParticleSystemScalingMode.Hierarchy;
#endif

                this.particles = null;
            }
#if UNITY_5_5_OR_NEWER
            if (this.particles == null)
            {
                this.particles = new ParticleSystem.Particle[this.pSystem.main.maxParticles];
            }
#else
                if (particles == null)
                    particles = new ParticleSystem.Particle[pSystem.maxParticles];
#endif

            this.imageUV = new Vector4(0, 0, 1, 1);

            // prepare texture sheet animation
            this.textureSheetAnimation = this.pSystem.textureSheetAnimation;
            this.textureSheetAnimationFrames = 0;
            this.textureSheetAnimationFrameSize = Vector2.zero;
            if (this.textureSheetAnimation.enabled)
            {
                this.textureSheetAnimationFrames = this.textureSheetAnimation.numTilesX * this.textureSheetAnimation.numTilesY;
                this.textureSheetAnimationFrameSize = new Vector2(1f / this.textureSheetAnimation.numTilesX, 1f / this.textureSheetAnimation.numTilesY);
            }

            return true;
        }

        protected override void Awake()
        {
            base.Awake();
            if (!this.Initialize())
            {
                this.enabled = false;
            }
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (!this.Initialize())
                {
                    return;
                }
            }
#endif
            // prepare vertices
            vh.Clear();

            if (!this.gameObject.activeInHierarchy)
            {
                return;
            }

            if (!this.isInitialised && !this.pSystem.main.playOnAwake)
            {
                this.pSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                this.isInitialised = true;
            }

            Vector2 temp = Vector2.zero;
            Vector2 corner1 = Vector2.zero;
            Vector2 corner2 = Vector2.zero;
            // iterate through current particles
            int count = this.pSystem.GetParticles(this.particles);

            for (int i = 0; i < count; ++i)
            {
                ParticleSystem.Particle particle = this.particles[i];

                // get particle properties
#if UNITY_5_5_OR_NEWER
                Vector2 position = (this.mainModule.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : this._transform.InverseTransformPoint(particle.position));
#else
                    Vector2 position = (pSystem.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : _transform.InverseTransformPoint(particle.position));
#endif
                float rotation = -particle.rotation * Mathf.Deg2Rad;
                float rotation90 = rotation + Mathf.PI / 2;
                Color32 color = particle.GetCurrentColor(this.pSystem);
                float size = particle.GetCurrentSize(this.pSystem) * 0.5f;

                // apply scale
#if UNITY_5_5_OR_NEWER
                if (this.mainModule.scalingMode == ParticleSystemScalingMode.Shape)
                {
                    position /= this.canvas.scaleFactor;
                }
#else
                    if (pSystem.scalingMode == ParticleSystemScalingMode.Shape)
                        position /= canvas.scaleFactor;
#endif

                // apply texture sheet animation
                Vector4 particleUV = this.imageUV;
                if (this.textureSheetAnimation.enabled)
                {
#if UNITY_5_5_OR_NEWER
                    float frameProgress = 1 - (particle.remainingLifetime / particle.startLifetime);

                    if (this.textureSheetAnimation.frameOverTime.curveMin != null)
                    {
                        frameProgress = this.textureSheetAnimation.frameOverTime.curveMin.Evaluate(1 - (particle.remainingLifetime / particle.startLifetime));
                    }
                    else if (this.textureSheetAnimation.frameOverTime.curve != null)
                    {
                        frameProgress = this.textureSheetAnimation.frameOverTime.curve.Evaluate(1 - (particle.remainingLifetime / particle.startLifetime));
                    }
                    else if (this.textureSheetAnimation.frameOverTime.constant > 0)
                    {
                        frameProgress = this.textureSheetAnimation.frameOverTime.constant - (particle.remainingLifetime / particle.startLifetime);
                    }
#else
                    float frameProgress = 1 - (particle.lifetime / particle.startLifetime);
#endif

                    frameProgress = Mathf.Repeat(frameProgress * this.textureSheetAnimation.cycleCount, 1);
                    int frame = 0;

                    switch (this.textureSheetAnimation.animation)
                    {

                        case ParticleSystemAnimationType.WholeSheet:
                            frame = Mathf.FloorToInt(frameProgress * this.textureSheetAnimationFrames);
                            break;

                        case ParticleSystemAnimationType.SingleRow:
                            frame = Mathf.FloorToInt(frameProgress * this.textureSheetAnimation.numTilesX);

                            int row = this.textureSheetAnimation.rowIndex;
                            //                    if (textureSheetAnimation.useRandomRow) { // FIXME - is this handled internally by rowIndex?
                            //                        row = Random.Range(0, textureSheetAnimation.numTilesY, using: particle.randomSeed);
                            //                    }
                            frame += row * this.textureSheetAnimation.numTilesX;
                            break;

                    }

                    frame %= this.textureSheetAnimationFrames;

                    particleUV.x = (frame % this.textureSheetAnimation.numTilesX) * this.textureSheetAnimationFrameSize.x;
                    particleUV.y = 1.0f - Mathf.FloorToInt(frame / this.textureSheetAnimation.numTilesX) * this.textureSheetAnimationFrameSize.y;
                    particleUV.z = particleUV.x + this.textureSheetAnimationFrameSize.x;
                    particleUV.w = particleUV.y + this.textureSheetAnimationFrameSize.y;
                }

                temp.x = particleUV.x;
                temp.y = particleUV.y;

                this._quad[0] = UIVertex.simpleVert;
                this._quad[0].color = color;
                this._quad[0].uv0 = temp;

                temp.x = particleUV.x;
                temp.y = particleUV.w;
                this._quad[1] = UIVertex.simpleVert;
                this._quad[1].color = color;
                this._quad[1].uv0 = temp;

                temp.x = particleUV.z;
                temp.y = particleUV.w;
                this._quad[2] = UIVertex.simpleVert;
                this._quad[2].color = color;
                this._quad[2].uv0 = temp;

                temp.x = particleUV.z;
                temp.y = particleUV.y;
                this._quad[3] = UIVertex.simpleVert;
                this._quad[3].color = color;
                this._quad[3].uv0 = temp;

                if (rotation == 0)
                {
                    // no rotation
                    corner1.x = position.x - size;
                    corner1.y = position.y - size;
                    corner2.x = position.x + size;
                    corner2.y = position.y + size;

                    temp.x = corner1.x;
                    temp.y = corner1.y;
                    this._quad[0].position = temp;
                    temp.x = corner1.x;
                    temp.y = corner2.y;
                    this._quad[1].position = temp;
                    temp.x = corner2.x;
                    temp.y = corner2.y;
                    this._quad[2].position = temp;
                    temp.x = corner2.x;
                    temp.y = corner1.y;
                    this._quad[3].position = temp;
                }
                else
                {
                    if (this.use3dRotation)
                    {
                        // get particle properties
#if UNITY_5_5_OR_NEWER
                        Vector3 pos3d = (this.mainModule.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : this._transform.InverseTransformPoint(particle.position));
#else
                        Vector3 pos3d = (pSystem.simulationSpace == ParticleSystemSimulationSpace.Local ? particle.position : _transform.InverseTransformPoint(particle.position));
#endif

                        // apply scale
#if UNITY_5_5_OR_NEWER
                        if (this.mainModule.scalingMode == ParticleSystemScalingMode.Shape)
                        {
                            position /= this.canvas.scaleFactor;
                        }
#else
                        if (pSystem.scalingMode == ParticleSystemScalingMode.Shape)
                            position /= canvas.scaleFactor;
#endif

                        Vector3[] verts = new Vector3[4]
                        {
                            new Vector3(-size, -size, 0),
                            new Vector3(-size, size, 0),
                            new Vector3(size, size, 0),
                            new Vector3(size, -size, 0)
                        };

                        Quaternion particleRotation = Quaternion.Euler(particle.rotation3D);

                        this._quad[0].position = pos3d + particleRotation * verts[0];
                        this._quad[1].position = pos3d + particleRotation * verts[1];
                        this._quad[2].position = pos3d + particleRotation * verts[2];
                        this._quad[3].position = pos3d + particleRotation * verts[3];
                    }
                    else
                    {
                        // apply rotation
                        Vector2 right = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) * size;
                        Vector2 up = new Vector2(Mathf.Cos(rotation90), Mathf.Sin(rotation90)) * size;

                        this._quad[0].position = position - right - up;
                        this._quad[1].position = position - right + up;
                        this._quad[2].position = position + right + up;
                        this._quad[3].position = position + right - up;
                    }
                }

                vh.AddUIVertexQuad(this._quad);
            }
        }

        private void Update()
        {
            if (!this.fixedTime && Application.isPlaying)
            {
                this.pSystem.Simulate(Time.unscaledDeltaTime, false, false, true);
                this.SetAllDirty();

                if ((this.currentMaterial != null && this.currentTexture != this.currentMaterial.mainTexture) ||
                    (this.material != null && this.currentMaterial != null && this.material.shader != this.currentMaterial.shader))
                {
                    this.pSystem = null;
                    this.Initialize();
                }
            }
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                this.SetAllDirty();
            }
            else
            {
                if (this.fixedTime)
                {
                    this.pSystem.Simulate(Time.unscaledDeltaTime, false, false, true);
                    this.SetAllDirty();
                    if ((this.currentMaterial != null && this.currentTexture != this.currentMaterial.mainTexture) ||
                        (this.material != null && this.currentMaterial != null && this.material.shader != this.currentMaterial.shader))
                    {
                        this.pSystem = null;
                        this.Initialize();
                    }
                }
            }
            if (this.material == this.currentMaterial)
            {
                return;
            }

            this.pSystem = null;
            this.Initialize();
        }

        protected override void OnDestroy()
        {
            this.currentMaterial = null;
            this.currentTexture = null;
        }

        public void StartParticleEmission()
        {
            this.pSystem.Play();
        }

        public void StopParticleEmission()
        {
            this.pSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void PauseParticleEmission()
        {
            this.pSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }
#endif
}