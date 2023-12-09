using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _01Scripts.VisualizationScripts
{
    public class RenderScaleFeature : ScriptableRendererFeature
    {
        [Range(0.1f, 2f)]
        public float renderScale = 1f;
        private RenderScalePass _renderScalePass;

        public override void Create()
        {
            _renderScalePass = new RenderScalePass();
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            throw new System.NotImplementedException();
        }
        
        class RenderScalePass : ScriptableRenderPass
        {
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
