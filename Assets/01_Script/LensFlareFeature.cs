using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LensFlareFeature : ScriptableRendererFeature
{
    public Material material;
    public Mesh mesh;
    
    private LensFlarePass _lensFlarePass;
    
    public override void Create()
    {
        _lensFlarePass = new LensFlarePass(material, mesh);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //  Unity does not enqueue the LensFlarePass pass yet,
        // if the Material and the Mesh properties are null.
        if (material && mesh)
        {
            renderer.EnqueuePass(_lensFlarePass);
        }
    }
    
    class LensFlarePass : ScriptableRenderPass
    {
        private Material _material;
        private Mesh _mesh;

        public LensFlarePass(Material material, Mesh mesh)
        {
            _material = material;
            _mesh = mesh;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("LensFlarePass");

            Camera camera = renderingData.cameraData.camera;
            // set the projection matrix so that cmd draws quad in screen space
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            Vector3 scale = new Vector3(1, camera.aspect, 1);
            foreach (VisibleLight visibleLight in renderingData.lightData.visibleLights)
            {
                Light light = visibleLight.light;
                // convert the position of each light from world to viewport point
                Vector3 position =
                    camera.WorldToViewportPoint(light.transform.position);

                position.z = 0;
                // set the z coordinate of the quads to 0 so that unity draws
                // them on the same plane.
                
                cmd.DrawMesh(_mesh, Matrix4x4.TRS(position, Quaternion.identity, scale), _material, 0, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
