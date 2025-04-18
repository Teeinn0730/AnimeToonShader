using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

[Serializable]
internal class CharacterOutlineSetting
{
    [SerializeField] internal bool m_Enable = true;
    [SerializeField] internal RenderPassEvent m_RenderPassEvent = RenderPassEvent.AfterRenderingOpaques;
}

public class CharacterOutline : ScriptableRenderPass
{
    // Profiling Tag
    private static readonly string m_ProfilerTag = "AnimeToon_CharacterOutline";
    private static readonly ProfilingSampler m_ProfilingSampler = new(m_ProfilerTag);

    // Private Variables
    private readonly ShaderTagId m_ShaderTagID = new(m_ProfilerTag);
    private readonly CharacterOutlineSetting m_CurrentSetting;

    internal CharacterOutline(ref CharacterOutlineSetting featureSettings)
    {
        m_CurrentSetting = featureSettings;
        renderPassEvent = m_CurrentSetting.m_RenderPassEvent;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            // To show the correct name on FrameDebugger.
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

    /*----------------------------------------------------------------------------------------------------------------------------------------
    ------------------------------------------------------------- RENDER-GRAPH --------------------------------------------------------------
    ----------------------------------------------------------------------------------------------------------------------------------------*/

#if UNITY_6000_0_OR_NEWER
    private class PassData
    {
        internal RendererListHandle rendererListHandle;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var cameraData = frameData.Get<UniversalCameraData>();
        var renderingData = frameData.Get<UniversalRenderingData>();
        var lightData = frameData.Get<UniversalLightData>();

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, profilingSampler))
        {
            // Draw the objects that are using materials associated with this pass.
            var drawingSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagID, renderingData, cameraData, lightData, SortingCriteria.CommonOpaque);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            var rendererListParameters = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
            passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParameters);
            builder.UseRendererList(passData.rendererListHandle);

            var resourceData = frameData.Get<UniversalResourceData>();
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
            builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
            builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => { rgContext.cmd.DrawRendererList(passData.rendererListHandle); });
        }
    }
#endif

    /*----------------------------------------------------------------------------------------------------------------------------------------
    ------------------------------------------------------------- RENDER-GRAPH --------------------------------------------------------------
    ----------------------------------------------------------------------------------------------------------------------------------------*/

    public void Dispose()
    {
        // Nothing to execute here, no RT or mat need to be release.
    }
}