using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

namespace _01Scripts.VisualizationScripts
{
    public class BloomFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public struct PassSettings
        {
            [Range(0.0f, 10.0f)]    public float threshold;
            [Range(0.0f, 1.0f)]     public float softThreshold;
            [Range(1, 16)]          public int downSamples;
            [Range(0.01f, 2.0f)]    public float downSampleDelta;
            [Range(0.01f, 2.0f)]    public float upSampleDelta;
            [Range(0.0f, 10.0f)]    public float bloomIntensity;
            
            public RenderPassEvent renderPassEvent;
        }
        
        // settings
        public PassSettings passSettings = new PassSettings()
        {
            threshold = 1.0f,
            softThreshold = 0.5f,
            downSamples = 1,
            downSampleDelta = 1.0f,
            upSampleDelta = 0.5f,
            bloomIntensity = 1,
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
        
        private BloomPass _bloomPass;
        private Material _bloomMat;
        public override void Create()
        {
            _bloomMat = CoreUtils.CreateEngineMaterial("Hidden/Bloom"); // Hidden/Universal Render Pipeline/Bloom
            _bloomPass = new BloomPass(_bloomMat);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                renderer.EnqueuePass(_bloomPass);
            }
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                // use cameraOpaqueTexture
                _bloomPass.ConfigureInput(ScriptableRenderPassInput.Color);
                _bloomPass.SetTarget(renderer.cameraColorTargetHandle, passSettings);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _bloomPass.Dispose();
            CoreUtils.Destroy(_bloomMat);
        }

        class BloomPass : ScriptableRenderPass
        {
            private Material _bloomMat;
            private PassSettings _passSettings;

            private RenderTextureDescriptor _sourceDescriptor;
            private RTHandle _cameraColorTarget; // source = destination
            
            private RTHandle[] _downSamples;
            private Vector2 _sampleResolution;
            private int _activeDownSampleCount;
            
            public BloomPass(Material bloomMat)
            {
                _bloomMat = bloomMat;
            }

            public void SetTarget(RTHandle cameraColorTarget, PassSettings passSettings)
            {
                _cameraColorTarget = cameraColorTarget;
                
                _passSettings = passSettings;
                renderPassEvent = _passSettings.renderPassEvent;
                if (_downSamples == null || _downSamples.Length != _passSettings.downSamples)
                {
                    _downSamples = new RTHandle[_passSettings.downSamples];
                }
            }
            
            // Gets called by the renderer before executing the pass.
            // Can be used to configure render targets and their clearing state.
            // Can be user to create temporary render target textures.
            // If this method is not overriden, ""the render pass will render to the active camera render target.""
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                _sourceDescriptor = renderingData.cameraData.cameraTargetDescriptor;
                _sourceDescriptor.depthBufferBits = 0;
                ConfigureTarget(_cameraColorTarget);

                _sampleResolution.x = _sourceDescriptor.width;
                _sampleResolution.y = _sourceDescriptor.height;
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if(renderingData.cameraData.camera.cameraType != CameraType.Game) return;
                CommandBuffer cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, new ProfilingSampler("Bloom Pass")))
                {
                    // set shader properties
                    _bloomMat.SetFloat("_Threshold", _passSettings.threshold);
                    _bloomMat.SetFloat("_SoftThreshold", _passSettings.softThreshold);
                    _bloomMat.SetFloat("_DownDelta", _passSettings.downSampleDelta);
                    _bloomMat.SetFloat("_UpDelta", _passSettings.upSampleDelta);
                    _bloomMat.SetTexture("_OriginalTex", _cameraColorTarget.rt); 
                    _bloomMat.SetFloat("_Intensity", _passSettings.bloomIntensity);
                    
                    // blit
                    float scaleFactor = 1f;
                    int i = 0;
                    for (; i < _passSettings.downSamples; i++)
                    {
                        scaleFactor /= 2f;
                        _sampleResolution.y /= 2;
                        
                        if (_sampleResolution.y < 2)
                        {
                            break;
                        }
                        
                        RenderingUtils.ReAllocateIfNeeded(ref _downSamples[i], Vector2.one * scaleFactor, _sourceDescriptor,
                            FilterMode.Bilinear, TextureWrapMode.Clamp);
                        
                        Blit(cmd, i == 0 ? _cameraColorTarget : _downSamples[i-1], _downSamples[i], _bloomMat, i == 0 ? 0 : 1);
                    }
                    
                    for (i -= 1; i >= 1; i--)
                    {
                        
                        Blit(cmd, _downSamples[i], _downSamples[i-1], _bloomMat, 2);
                    }
                    
                    Blit(cmd, _downSamples[0], _cameraColorTarget, _bloomMat, 3);
                }
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            
            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                for (int i = 0; i < _passSettings.downSamples; i++)
                {
                    // _downSamples[i].rt.Release();
                    // _downSamples[i] = null;
                }
            }

            public void Dispose()
            {
                for (int i = 0; i < _passSettings.downSamples; i++)
                {
                    _downSamples[i].Release();
                }
                CoreUtils.Destroy(_bloomMat);
            }
        }
    }
}