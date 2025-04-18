using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AnimeToonFeatures : ScriptableRendererFeature
{
    // Private RenderPass Instance 
    private CharacterOutline m_CharacterOutlinePass;
    [SerializeField] private CharacterOutlineSetting m_CharacterOutlineSetting;
    private EyeThroughHair m_EyeThroughHairPass;
    [SerializeField] private EyeThroughHairSetting m_EyeThroughHairSetting;
    private DepthHairCaster m_DepthHairCasterPass;
    [SerializeField] private DepthHairCasterSetting m_DepthHairCasterSetting;

    public override void Create()
    {
        CleanPass(); // When every time serialization happens, Create() will be called. We reset every pass instead of null check every pass here.
        m_CharacterOutlinePass = new CharacterOutline(ref m_CharacterOutlineSetting);
        m_EyeThroughHairPass = new EyeThroughHair(ref m_EyeThroughHairSetting);
        m_DepthHairCasterPass = new DepthHairCaster(ref m_DepthHairCasterSetting);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_CharacterOutlineSetting.m_Enable) renderer.EnqueuePass(m_CharacterOutlinePass);
        if (m_EyeThroughHairSetting.m_Enable) renderer.EnqueuePass(m_EyeThroughHairPass);
        if (m_DepthHairCasterSetting.m_Enable) renderer.EnqueuePass(m_DepthHairCasterPass);
    }

    private void CleanPass()
    {
        m_CharacterOutlinePass?.Dispose();
        m_CharacterOutlinePass = null;

        m_EyeThroughHairPass?.Dispose();
        m_EyeThroughHairPass = null;

        m_DepthHairCasterPass?.Dispose();
        m_DepthHairCasterPass = null;
    }

    protected override void Dispose(bool disposing)
    {
        m_CharacterOutlinePass?.Dispose();
        m_CharacterOutlinePass = null;

        m_EyeThroughHairPass?.Dispose();
        m_EyeThroughHairPass = null;

        m_DepthHairCasterPass?.Dispose();
        m_DepthHairCasterPass = null;
    }
}