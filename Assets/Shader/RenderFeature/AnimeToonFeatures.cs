using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AnimeToon.RenderFeature
{
    public class AnimeToonFeatures : ScriptableRendererFeature
    {
        // Private RenderPass Instance
        private DepthHairCaster m_DepthHairCasterPass;
        private CharacterOutline m_CharacterOutlinePass;
        private EyeThroughHair m_EyeThroughHairPass;

        // Public User Setting 
        [Space(20)] public bool enableCharacterOutline = true;
        public RenderPassEvent m_CharacterOutline_RenderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        [Space(20)] public bool enableEyeThroughHair = true;
        public RenderPassEvent m_EyeThroughHair_RenderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        [Space(20)] public bool enableDepthHairCaster = true;
        public RenderPassEvent m_DepthHairCaster_RenderPassEvent = RenderPassEvent.BeforeRenderingOpaques;

        public override void Create()
        {
            CleanPass(); // When every time serialization happens, Create() will be called. We reset every pass instead of null check every pass here.
            m_CharacterOutlinePass = new CharacterOutline(m_CharacterOutline_RenderPassEvent);
            m_EyeThroughHairPass = new EyeThroughHair(m_EyeThroughHair_RenderPassEvent);
            m_DepthHairCasterPass = new DepthHairCaster(m_DepthHairCaster_RenderPassEvent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (enableCharacterOutline) renderer.EnqueuePass(m_CharacterOutlinePass);
            if (enableEyeThroughHair) renderer.EnqueuePass(m_EyeThroughHairPass);
            if (enableDepthHairCaster) renderer.EnqueuePass(m_DepthHairCasterPass);
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
}