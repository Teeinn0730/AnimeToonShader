using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthHairCaster : ScriptableRenderPass
{
    // Profiling Tag
    private static string m_ProfilerTag = "AnimeToon_CasterHairShadow";
    private static ProfilingSampler m_ProfilingSampler = new(m_ProfilerTag);

    // Private Variables
    private RTHandle m_RenderTarget;
    private readonly ShaderTagId m_ShaderTagID = new(m_ProfilerTag);

    internal DepthHairCaster(RenderPassEvent renderPassEvent)
    {
        this.renderPassEvent = renderPassEvent;
        profilingSampler = m_ProfilingSampler;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var MainCamera = renderingData.cameraData.camera;
        var desc = new RenderTextureDescriptor(MainCamera.scaledPixelWidth, MainCamera.scaledPixelHeight,
            RenderTextureFormat.Depth);
        desc.depthBufferBits = 32;
        RenderingUtils.ReAllocateIfNeeded(ref m_RenderTarget, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AnimeToon_HairDepthTexture");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            // To show the correct name on FrameDebugger.
            cmd.SetRenderTarget(m_RenderTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_RenderTarget, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(true, true, Color.black);
            cmd.SetGlobalTexture(m_RenderTarget.name, m_RenderTarget.nameID);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // Draw the objects that are using materials associated with this pass.
            var drawingSettings = CreateDrawingSettings(m_ShaderTagID, ref renderingData, SortingCriteria.CommonOpaque);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        // To show the correct name on FrameDebugger.
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        m_RenderTarget?.Release();
    }
}