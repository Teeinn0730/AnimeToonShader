using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

[Serializable]
internal class DepthHairCasterSetting
{
    [SerializeField] internal bool m_Enable = true;
    [SerializeField] internal RenderPassEvent m_RenderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
}

public class DepthHairCaster : ScriptableRenderPass
{
    // Profiling Tag
    private static readonly string m_ProfilerTag = "AnimeToon_CasterHairShadow";
    private static readonly ProfilingSampler m_ProfilingSampler = new(m_ProfilerTag);

    // Private Variables
    private RTHandle m_RenderTarget;
    private readonly ShaderTagId m_ShaderTagID = new(m_ProfilerTag);

    internal DepthHairCaster(ref DepthHairCasterSetting featureSettings)
    {
        renderPassEvent = featureSettings.m_RenderPassEvent;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var MainCamera = renderingData.cameraData.camera;
        var desc = new RenderTextureDescriptor(MainCamera.scaledPixelWidth, MainCamera.scaledPixelHeight, RenderTextureFormat.Depth,32);
#if UNITY_6000_0_OR_NEWER
        RenderingUtils.ReAllocateHandleIfNeeded(ref m_RenderTarget, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AnimeToon_HairDepthTexture");
#else
        RenderingUtils.ReAllocateIfNeeded(ref m_RenderTarget, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AnimeToon_HairDepthTexture");
#endif
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
            
#if UNITY_6000_0_OR_NEWER
            cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle, renderingData.cameraData.renderer.cameraDepthTargetHandle);
#endif
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
        internal TextureHandle renderTarget_Depth;
    }

    private readonly int renderTarget_Depth_ID = Shader.PropertyToID("_AnimeToon_HairDepthTexture");

    private void CreateRenderTextureHandles(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, out TextureHandle rtDepth)
    {
        var depthDescriptor = cameraData.cameraTargetDescriptor;
        depthDescriptor.depthBufferBits = 32;
        rtDepth = UniversalRenderer.CreateRenderGraphTexture(renderGraph, depthDescriptor, "_AnimeToon_HairDepthTexture", true, FilterMode.Bilinear);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var cameraData = frameData.Get<UniversalCameraData>();
        var renderingData = frameData.Get<UniversalRenderingData>();
        var lightData = frameData.Get<UniversalLightData>();
        var resourceData = frameData.Get<UniversalResourceData>();

        // Create the texture handles
        CreateRenderTextureHandles(renderGraph, resourceData, cameraData, out var rtDepth);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, profilingSampler))
        {
            // Draw the objects that are using materials associated with this pass.
            var drawingSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagID, renderingData, cameraData, lightData, SortingCriteria.CommonOpaque);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            var rendererListParameters = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);

            // Fill in the pass data
            passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParameters);
            passData.renderTarget_Depth = rtDepth;
            builder.UseRendererList(passData.rendererListHandle);

            // Set renderTarget and execute the rendering
            builder.SetRenderAttachmentDepth(passData.renderTarget_Depth);
            builder.SetGlobalTextureAfterPass(passData.renderTarget_Depth, renderTarget_Depth_ID);
            builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => { rgContext.cmd.DrawRendererList(data.rendererListHandle); });
        }
    }
#endif

    /*----------------------------------------------------------------------------------------------------------------------------------------
    ------------------------------------------------------------- RENDER-GRAPH --------------------------------------------------------------
    ----------------------------------------------------------------------------------------------------------------------------------------*/

    public void Dispose()
    {
        m_RenderTarget?.Release();
    }
}