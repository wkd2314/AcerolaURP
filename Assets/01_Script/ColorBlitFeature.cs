using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorBlitFeature : ScriptableRendererFeature
{
    public float Intensity;

    private Material material;
    private ColorBlitPass colorBlitPass;
    
    public override void Create()
    {
        material = CoreUtils.CreateEngineMaterial("Hidden/ColorBlit");
        colorBlitPass = new ColorBlitPass(material);
    }

    // PRECULL ADD SETUP CALLED EVERY FRAME
    public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
    {
        // Debug.Log("OnPreCull");
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(colorBlitPass);
        }
    }

    /// <summary>
    /// Callback after render targets are initialized. This allows for accessing targets from renderer after they are created and ready.
    /// </summary>
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            // enusures that the opaque texture is avaliable to the Render Pass.
            colorBlitPass.ConfigureInput(ScriptableRenderPassInput.Color);
            colorBlitPass.SetTarget(renderer.cameraColorTargetHandle, Intensity);
        }
    }
    
    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
    }

    class ColorBlitPass : ScriptableRenderPass
    {
        private Material _material;
        private RTHandle _cameraColorTarget;
        private float _intensity;
        
        public ColorBlitPass(Material material)
        {
            _material = material;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }
        
        public void SetTarget(RTHandle colorHandle, float intensity)
        {
            _cameraColorTarget = colorHandle;
            _intensity = intensity;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(_cameraColorTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if(cameraData.camera.cameraType != CameraType.Game) return;
            
            if(_material == null) return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ColorBlitPass")))
            {
                _material.SetFloat("_Intensity", _intensity);
                Blit(cmd, _cameraColorTarget, _cameraColorTarget, _material, 0);
                Blit(cmd, _cameraColorTarget, _cameraColorTarget, _material, 1);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {

        }

        public void Dispose()
        {

        }
    }
}
