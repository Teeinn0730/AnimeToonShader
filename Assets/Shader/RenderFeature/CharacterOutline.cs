using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CharacterOutline : ScriptableRenderPass
{
    // Profiling Tag
    private static string m_ProfilerTag = "AnimeToon_CharacterOutline";
    private static ProfilingSampler m_ProfilingSampler = new(m_ProfilerTag);

    // Private Variables
    private readonly ShaderTagId m_ShaderTagID = new(m_ProfilerTag);

    internal CharacterOutline(RenderPassEvent renderPassEvent)
    {
        this.renderPassEvent = renderPassEvent;
        profilingSampler = m_ProfilingSampler;
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

    public void Dispose()
    {
        // Nothing to execute here, no RT or mat need to be release.
    }
}