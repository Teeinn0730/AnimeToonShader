using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

[Serializable]
internal class EyeThroughHairSetting
{
    [SerializeField] internal bool m_Enable = true;
    [SerializeField] internal RenderPassEvent m_RenderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
}

public class EyeThroughHair : ScriptableRenderPass
{
    // Profiling Tag
    private static readonly string m_ProfilerTag = "AnimeToon_EyeThroughHair";
    private static readonly ProfilingSampler m_ProfilingSampler = new(m_ProfilerTag);

    // Private Variables
    private RTHandle m_RenderTarget_Color;
    private RTHandle m_RenderTarget_Depth;
    private readonly ShaderTagId m_ShaderTagID = new(m_ProfilerTag);
    private readonly EyeThroughHairSetting m_CurrentSetting;

    internal EyeThroughHair(ref EyeThroughHairSetting featureSettings)
    {
        renderPassEvent = featureSettings.m_RenderPassEvent;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var MainCamera = renderingData.cameraData.camera;
        var ColorDesc = new RenderTextureDescriptor(MainCamera.scaledPixelWidth, MainCamera.scaledPixelHeight, RenderTextureFormat.Default);
        var DepthDesc = new RenderTextureDescriptor(MainCamera.scaledPixelWidth, MainCamera.scaledPixelHeight, RenderTextureFormat.Depth, 32);
#if UNITY_6000_0_OR_NEWER
        RenderingUtils.ReAllocateHandleIfNeeded(ref m_RenderTarget_Color, ColorDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AnimeToon_EyeThroughHair_Color");
        RenderingUtils.ReAllocateHandleIfNeeded(ref m_RenderTarget_Depth, DepthDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AnimeToon_EyeThroughHair_Depth");
#else
        RenderingUtils.ReAllocateIfNeeded(ref m_RenderTarget_Color, ColorDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AnimeToon_EyeThroughHair_Color");
        RenderingUtils.ReAllocateIfNeeded(ref m_RenderTarget_Depth, DepthDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_AnimeToon_EyeThroughHair_Depth");
#endif
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
            cmd.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
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
        internal TextureHandle renderTarget_Color;
        internal TextureHandle renderTarget_Depth;
    }

    private readonly int renderTarget_Color_ID = Shader.PropertyToID("_AnimeToon_EyeThroughHair_Color");
    private int renderTarget_Depth_ID = Shader.PropertyToID("_AnimeToon_EyeThroughHair_Depth");

    private void CreateRenderTextureHandles(RenderGraph renderGraph, UniversalResourceData resourceData, UniversalCameraData cameraData, out TextureHandle rtColor, out TextureHandle rtDepth)
    {
        var colorDescriptor = cameraData.cameraTargetDescriptor;
        colorDescriptor.colorFormat = RenderTextureFormat.Default;
        colorDescriptor.depthStencilFormat = GraphicsFormat.None;
        var depthDescriptor = cameraData.cameraTargetDescriptor;
        depthDescriptor.depthBufferBits = 32;
        rtColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, colorDescriptor, "_AnimeToon_EyeThroughHair_Color", true, FilterMode.Bilinear);
        rtDepth = UniversalRenderer.CreateRenderGraphTexture(renderGraph, depthDescriptor, "_AnimeToon_EyeThroughHair_Depth", true, FilterMode.Bilinear);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var cameraData = frameData.Get<UniversalCameraData>();
        var renderingData = frameData.Get<UniversalRenderingData>();
        var lightData = frameData.Get<UniversalLightData>();
        var resourceData = frameData.Get<UniversalResourceData>();

        // Create the texture handles
        CreateRenderTextureHandles(renderGraph, resourceData, cameraData, out var rtColor, out var rtDepth);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, profilingSampler))
        {
            // Draw the objects that are using materials associated with this pass.
            var drawingSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTagID, renderingData, cameraData, lightData, SortingCriteria.CommonOpaque);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            var rendererListParameters = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);

            // Fill in the pass data
            passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParameters);
            passData.renderTarget_Color = rtColor;
            passData.renderTarget_Depth = rtDepth;
            builder.UseRendererList(passData.rendererListHandle);

            // Set renderTarget and execute the rendering
            builder.SetRenderAttachment(passData.renderTarget_Color, 0);
            builder.SetRenderAttachmentDepth(passData.renderTarget_Depth);
            builder.SetGlobalTextureAfterPass(passData.renderTarget_Color, renderTarget_Color_ID);
            builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => { rgContext.cmd.DrawRendererList(data.rendererListHandle); });
        }
    }
#endif

    /*----------------------------------------------------------------------------------------------------------------------------------------
    ------------------------------------------------------------- RENDER-GRAPH --------------------------------------------------------------
    ----------------------------------------------------------------------------------------------------------------------------------------*/

    public void Dispose()
    {
        m_RenderTarget_Color?.Release();
        m_RenderTarget_Depth?.Release();
    }
}