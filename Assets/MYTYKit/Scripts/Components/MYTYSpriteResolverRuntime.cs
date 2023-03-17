using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MYTYSpriteResolverRuntime : MonoBehaviour
    {
        public Sprite sprite;
        public List<string> labels = new();
        public string currentLabel;
        
        Dictionary<string, Sprite> m_spriteMap = new();

        SpriteRenderer m_renderer; 
    
        public void AssignSprite(string label, Sprite sprite)
        {
            m_spriteMap[label] = sprite;
            if(!labels.Contains(label)) labels.Add(label);
        }

        public void SetLabel(string label)
        {
            if (m_renderer == null) m_renderer = GetComponent<SpriteRenderer>();
            if (!m_spriteMap.ContainsKey(label)) return;
            sprite = m_spriteMap[label];
            m_renderer.sprite = sprite;
            currentLabel = label;
        }
    }
}