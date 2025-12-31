using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ShaderHelper.Shadow2D
{
    public class Shadow2DFeature : ScriptableRendererFeature
    {
        public Settings settings = new();
        private ShadowRTRenderPass pass;

        public override void Create()
        {
            pass = new ShadowRTRenderPass(settings);
            pass.renderPassEvent = settings.passEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera)
                return;
            if (ShadowRTDrawer.Instance == null)
                return;

            pass.Setup(ShadowRTDrawer.Instance);
            renderer.EnqueuePass(pass);
        }

        [Serializable]
        public class Settings
        {
            public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingOpaques;
            public string profilerTag = "ShadowRT_2D_Pass";
        }

        private class ShadowRTRenderPass : ScriptableRenderPass
        {
            private readonly MaterialPropertyBlock mpb = new();
            private readonly Settings settings;
            private ShadowRTDrawer drawer;

            public ShadowRTRenderPass(Settings settings)
            {
                this.settings = settings;
            }

            public void Setup(ShadowRTDrawer drawer)
            {
                this.drawer = drawer;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (drawer == null)
                    return;

                if (drawer.whiteMat == null || drawer.casters == null || drawer.casters.Count == 0)
                    return;

                drawer.EnsureRenderTexture();
                drawer.ConfigureCamera();

                var mainCam = renderingData.cameraData.camera;
                if (drawer.shadowCamera == null)
                    return;

                var oldView = mainCam.worldToCameraMatrix;
                var oldProj = mainCam.projectionMatrix;

                var shadowView = drawer.shadowCamera.worldToCameraMatrix;
                var shadowProj = drawer.shadowCamera.projectionMatrix;

                var cmd = CommandBufferPool.Get(settings.profilerTag);

                cmd.SetRenderTarget(drawer.shadowRT);
                cmd.ClearRenderTarget(true, true, Color.black);
                cmd.SetViewProjectionMatrices(shadowView, shadowProj);

                drawer.casters.Sort((a, b) => a.level.CompareTo(b.level));

                foreach (var sr in drawer.casters)
                {
                    if (sr == null || sr.spriteRenderer.sprite == null)
                        continue;

                    var mesh = drawer.GetMesh(sr.spriteRenderer.sprite);
                    if (mesh == null)
                        continue;

                    var flipScale = new Vector3(
                        sr.spriteRenderer.flipX ? -1f : 1f,
                        sr.spriteRenderer.flipY ? -1f : 1f,
                        1f
                    );
                    var flipM = Matrix4x4.Scale(flipScale);


                    mpb.Clear();
                    mpb.SetTexture("_MainTex", sr.spriteRenderer.sprite.texture);
                    mpb.SetVector("_Position", sr.spriteRenderer.transform.position);
                    mpb.SetFloat("_FlipX", sr.spriteRenderer.flipX ? -1f : 1f);
                    mpb.SetColor("_WhiteColor", new Color(1f * sr.level, 1f * sr.level, 1f * sr.level, 1f));

                    var l2w = sr.spriteRenderer.transform.localToWorldMatrix * flipM;
                    cmd.DrawMesh(mesh, l2w, drawer.whiteMat, 0, 0, mpb);
                }

                cmd.SetViewProjectionMatrices(oldView, oldProj);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}