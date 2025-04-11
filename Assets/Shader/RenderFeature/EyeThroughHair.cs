using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EyeThroughHair : ScriptableRenderPass
{
    // Profiling Tag
    private static string m_ProfilerTag = "AnimeToon_EyeThroughHair";
    private static ProfilingSampler m_ProfilingSampler = new(m_ProfilerTag);

    // Private Variables
    private RTHandle m_RenderTarget_Color;
    private RTHandle m_RenderTarget_Depth;
    private readonly ShaderTagId m_ShaderTagID = new(m_ProfilerTag);

    internal EyeThroughHair(RenderPassEvent renderPassEvent)
    {
        this.renderPassEvent = renderPassEvent;
        profilingSampler = m_ProfilingSampler;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var MainCamera = renderingData.cameraData.camera;
        var ColorDesc = new RenderTextureDescriptor(MainCamera.scaledPixelWidth, MainCamera.scaledPixelHeight, RenderTextureFormat.Default);
        var DepthDesc = new RenderTextureDescriptor(MainCamera.scaledPixelWidth, MainCamera.scaledPixelHeight, RenderTextureFormat.Depth);
        DepthDesc.depthBufferBits = 32;
        RenderingUtils.ReAllocateIfNeeded(ref m_RenderTarget_Color, ColorDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AnimeToon_EyeThroughHair_Color");
        RenderingUtils.ReAllocateIfNeeded(ref m_RenderTarget_Depth, DepthDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AnimeToon_EyeThroughHair_Depth");
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            // To show the correct name on FrameDebugger.
            cmd.SetRenderTarget(m_RenderTarget_Color, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_RenderTarget_Depth, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.SetGlobalTexture(m_RenderTarget_Color.name, m_RenderTarget_Color.nameID);
            cmd.SetGlobalTexture(m_RenderTarget_Depth.name, m_RenderTarget_Depth.nameID);
            cmd.ClearRenderTarget(true, true, new Color(0,0,0,0));
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
        m_RenderTarget_Color?.Release();
        m_RenderTarget_Depth?.Release();
    }
}