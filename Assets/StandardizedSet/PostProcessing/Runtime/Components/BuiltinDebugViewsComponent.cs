using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing
{
    using Mode = BuiltinDebugViewsModel.Mode;

    public sealed class BuiltinDebugViewsComponent : PostProcessingComponentCommandBuffer<BuiltinDebugViewsModel>
    {
        private static class Uniforms
        {
            internal static readonly int _DepthScale = Shader.PropertyToID("_DepthScale");
            internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
            internal static readonly int _Opacity = Shader.PropertyToID("_Opacity");
            internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");
            internal static readonly int _TempRT2 = Shader.PropertyToID("_TempRT2");
            internal static readonly int _Amplitude = Shader.PropertyToID("_Amplitude");
            internal static readonly int _Scale = Shader.PropertyToID("_Scale");
        }

        private const string k_ShaderString = "Hidden/Post FX/Builtin Debug Views";

        private enum Pass
        {
            Depth,
            Normals,
            MovecOpacity,
            MovecImaging,
            MovecArrows
        }

        private ArrowArray m_Arrows;

        private class ArrowArray
        {
            public Mesh mesh { get; private set; }

            public int columnCount { get; private set; }
            public int rowCount { get; private set; }

            public void BuildMesh(int columns, int rows)
            {
                // Base shape
                Vector3[] arrow = new Vector3[6]
                {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 1f, 0f),
                    new Vector3(0f, 1f, 0f),
                    new Vector3(-1f, 1f, 0f),
                    new Vector3(0f, 1f, 0f),
                    new Vector3(1f, 1f, 0f)
                };

                // make the vertex array
                int vcount = 6 * columns * rows;
                List<Vector3> vertices = new List<Vector3>(vcount);
                List<Vector2> uvs = new List<Vector2>(vcount);

                for (int iy = 0; iy < rows; iy++)
                {
                    for (int ix = 0; ix < columns; ix++)
                    {
                        Vector2 uv = new Vector2(
                                (0.5f + ix) / columns,
                                (0.5f + iy) / rows
                                );

                        for (int i = 0; i < 6; i++)
                        {
                            vertices.Add(arrow[i]);
                            uvs.Add(uv);
                        }
                    }
                }

                // make the index array
                int[] indices = new int[vcount];

                for (int i = 0; i < vcount; i++)
                {
                    indices[i] = i;
                }

                // initialize the mesh object
                this.mesh = new Mesh { hideFlags = HideFlags.DontSave };
                this.mesh.SetVertices(vertices);
                this.mesh.SetUVs(0, uvs);
                this.mesh.SetIndices(indices, MeshTopology.Lines, 0);
                this.mesh.UploadMeshData(true);

                // update the properties
                this.columnCount = columns;
                this.rowCount = rows;
            }

            public void Release()
            {
                GraphicsUtils.Destroy(this.mesh);
                this.mesh = null;
            }
        }

        public override bool active => this.model.IsModeActive(Mode.Depth)
                       || this.model.IsModeActive(Mode.Normals)
                       || this.model.IsModeActive(Mode.MotionVectors);

        public override DepthTextureMode GetCameraFlags()
        {
            Mode mode = this.model.settings.mode;
            DepthTextureMode flags = DepthTextureMode.None;

            switch (mode)
            {
                case Mode.Normals:
                    flags |= DepthTextureMode.DepthNormals;
                    break;
                case Mode.MotionVectors:
                    flags |= DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
                    break;
                case Mode.Depth:
                    flags |= DepthTextureMode.Depth;
                    break;
            }

            return flags;
        }

        public override CameraEvent GetCameraEvent()
        {
            return this.model.settings.mode == Mode.MotionVectors
                   ? CameraEvent.BeforeImageEffects
                   : CameraEvent.BeforeImageEffectsOpaque;
        }

        public override string GetName()
        {
            return "Builtin Debug Views";
        }

        public override void PopulateCommandBuffer(CommandBuffer cb)
        {
            BuiltinDebugViewsModel.Settings settings = this.model.settings;
            Material material = this.context.materialFactory.Get(k_ShaderString);
            material.shaderKeywords = null;

            if (this.context.isGBufferAvailable)
            {
                material.EnableKeyword("SOURCE_GBUFFER");
            }

            switch (settings.mode)
            {
                case Mode.Depth:
                    this.DepthPass(cb);
                    break;
                case Mode.Normals:
                    this.DepthNormalsPass(cb);
                    break;
                case Mode.MotionVectors:
                    this.MotionVectorsPass(cb);
                    break;
            }

            this.context.Interrupt();
        }

        private void DepthPass(CommandBuffer cb)
        {
            Material material = this.context.materialFactory.Get(k_ShaderString);
            BuiltinDebugViewsModel.DepthSettings settings = this.model.settings.depth;

            cb.SetGlobalFloat(Uniforms._DepthScale, 1f / settings.scale);
            cb.Blit(null, BuiltinRenderTextureType.CameraTarget, material, (int)Pass.Depth);
        }

        private void DepthNormalsPass(CommandBuffer cb)
        {
            Material material = this.context.materialFactory.Get(k_ShaderString);
            cb.Blit(null, BuiltinRenderTextureType.CameraTarget, material, (int)Pass.Normals);
        }

        private void MotionVectorsPass(CommandBuffer cb)
        {
#if UNITY_EDITOR
            // Don't render motion vectors preview when the editor is not playing as it can in some
            // cases results in ugly artifacts (i.e. when resizing the game view).
            if (!Application.isPlaying)
            {
                return;
            }
#endif

            Material material = this.context.materialFactory.Get(k_ShaderString);
            BuiltinDebugViewsModel.MotionVectorsSettings settings = this.model.settings.motionVectors;

            // Blit the original source image
            int tempRT = Uniforms._TempRT;
            cb.GetTemporaryRT(tempRT, this.context.width, this.context.height, 0, FilterMode.Bilinear);
            cb.SetGlobalFloat(Uniforms._Opacity, settings.sourceOpacity);
            cb.SetGlobalTexture(Uniforms._MainTex, BuiltinRenderTextureType.CameraTarget);
            cb.Blit(BuiltinRenderTextureType.CameraTarget, tempRT, material, (int)Pass.MovecOpacity);

            // Motion vectors (imaging)
            if (settings.motionImageOpacity > 0f && settings.motionImageAmplitude > 0f)
            {
                int tempRT2 = Uniforms._TempRT2;
                cb.GetTemporaryRT(tempRT2, this.context.width, this.context.height, 0, FilterMode.Bilinear);
                cb.SetGlobalFloat(Uniforms._Opacity, settings.motionImageOpacity);
                cb.SetGlobalFloat(Uniforms._Amplitude, settings.motionImageAmplitude);
                cb.SetGlobalTexture(Uniforms._MainTex, tempRT);
                cb.Blit(tempRT, tempRT2, material, (int)Pass.MovecImaging);
                cb.ReleaseTemporaryRT(tempRT);
                tempRT = tempRT2;
            }

            // Motion vectors (arrows)
            if (settings.motionVectorsOpacity > 0f && settings.motionVectorsAmplitude > 0f)
            {
                this.PrepareArrows();

                float sy = 1f / settings.motionVectorsResolution;
                float sx = sy * this.context.height / this.context.width;

                cb.SetGlobalVector(Uniforms._Scale, new Vector2(sx, sy));
                cb.SetGlobalFloat(Uniforms._Opacity, settings.motionVectorsOpacity);
                cb.SetGlobalFloat(Uniforms._Amplitude, settings.motionVectorsAmplitude);
                cb.DrawMesh(this.m_Arrows.mesh, Matrix4x4.identity, material, 0, (int)Pass.MovecArrows);
            }

            cb.SetGlobalTexture(Uniforms._MainTex, tempRT);
            cb.Blit(tempRT, BuiltinRenderTextureType.CameraTarget);
            cb.ReleaseTemporaryRT(tempRT);
        }

        private void PrepareArrows()
        {
            int row = this.model.settings.motionVectors.motionVectorsResolution;
            int col = row * Screen.width / Screen.height;

            if (this.m_Arrows == null)
            {
                this.m_Arrows = new ArrowArray();
            }

            if (this.m_Arrows.columnCount != col || this.m_Arrows.rowCount != row)
            {
                this.m_Arrows.Release();
                this.m_Arrows.BuildMesh(col, row);
            }
        }

        public override void OnDisable()
        {
            if (this.m_Arrows != null)
            {
                this.m_Arrows.Release();
            }

            this.m_Arrows = null;
        }
    }
}
