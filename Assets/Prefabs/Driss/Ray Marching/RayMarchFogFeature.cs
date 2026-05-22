using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class RayMarchFogFeature : ScriptableRendererFeature
{
    public Material rayMarchMaterial;
    
    RayMarchFogPass pass;

    public override void Create()
    {
        if (rayMarchMaterial == null)
            return;
            
        pass = new RayMarchFogPass(rayMarchMaterial);
        pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (rayMarchMaterial == null || pass == null)
            return;
        
        // Skip for scene view preview cameras
        if (renderingData.cameraData.cameraType == CameraType.Preview)
            return;
            
        renderer.EnqueuePass(pass);
    }

    class RayMarchFogPass : ScriptableRenderPass
    {
        private Material material;
        private static readonly int BlitTextureID = Shader.PropertyToID("_BlitTexture");

        public RayMarchFogPass(Material mat)
        {
            material = mat;
            profilingSampler = new ProfilingSampler("RayMarch Fog");
        }

        // Render Graph pass data
        private class PassData
        {
            public TextureHandle source;
            public TextureHandle temp;
            public Material material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            TextureHandle source = resourceData.activeColorTexture;
            
            if (!source.IsValid())
                return;

            // Create temp texture
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            
            TextureHandle temp = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph, desc, "_RayMarchFogTemp", false);

            // Blit with material
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("RayMarch Fog", out var passData))
            {
                passData.source = source;
                passData.temp = temp;
                passData.material = material;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(temp, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            // Copy back
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("RayMarch Fog CopyBack", out var passData))
            {
                passData.source = temp;
                passData.temp = source;
                passData.material = material;

                builder.UseTexture(temp, AccessFlags.Read);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }
        }
    }
}
