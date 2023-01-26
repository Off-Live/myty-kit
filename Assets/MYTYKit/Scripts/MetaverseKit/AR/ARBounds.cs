using System;
using UnityEngine;

namespace MYTYCamera.AR
{
    public class ARBounds : MonoBehaviour
    {
        public MeshRenderer renderer;
        
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
                m_renderer.material.mainTexture = renderer.material.mainTexture;
                m_assigned = true;
            }
        }
    }
}