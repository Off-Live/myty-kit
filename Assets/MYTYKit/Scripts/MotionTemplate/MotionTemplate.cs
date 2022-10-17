using UnityEngine;
using UnityEngine.Events;

namespace MYTYKit.MotionTemplates
{
    public abstract class MotionTemplate : MonoBehaviour
    {
        UnityEvent m_updateEvent = new ();

        public void SetUpdateCallback(UnityAction action)
        {
            m_updateEvent.AddListener(action);
        }
    
        public void NotifyUpdate()
        {
            m_updateEvent.Invoke();
        }
    }
}