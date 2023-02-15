using UnityEngine;
using UnityEngine.Events;

namespace MYTYKit.MotionAdapters
{
    public class MYTYIKTarget : MonoBehaviour
    {
        public float weight = 1.0f;
        public bool Visible
        {
            get
            {
                return m_visible;
            }
            set
            {
                var oldVal = m_visible;
                m_visible = value;
                if(oldVal!=m_visible) onVisibilityChanged.Invoke(); 
            }
        }
        public UnityEvent onVisibilityChanged;
        bool m_visible;
    }
}