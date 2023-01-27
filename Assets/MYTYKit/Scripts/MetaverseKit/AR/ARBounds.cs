using UnityEngine;

namespace MYTYCamera.AR
{
    public class ARBounds : MonoBehaviour
    {
        public MeshRenderer referenceRenderer;
        
        private MeshRenderer m_renderer;
        private bool m_assigned = false;
        
        void Start()
        {
            m_renderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if (!m_assigned)
            {
                if (m_renderer.material.mainTexture != null) return;
                m_renderer.material.mainTexture = referenceRenderer.material.mainTexture;
                m_assigned = true;
            }
        }
    }
}