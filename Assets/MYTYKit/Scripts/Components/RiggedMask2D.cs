using UnityEngine.U2D.Animation;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;

namespace MYTYKit.Components
{
    [DisallowMultipleComponent]
    public class RiggedMask2D : MonoBehaviour
    {

        [SerializeField] SpriteMask m_mask;
        [SerializeField] GameObject m_bone;
        
        SpriteRenderer m_renderer;
        
        void Start()
        {
            if (m_bone == null) return;
            m_renderer = GetComponent<SpriteRenderer>();
        }
        
        void Update()
        {
            if (m_bone == null) return;
            m_mask.sprite = m_renderer.sprite;
            
            var bindPoses = m_mask.sprite.GetBindPoses();
            if (bindPoses.Length > 0)
            {
                transform.position = m_bone.transform.rotation * bindPoses[0].GetPosition() +
                                     m_bone.transform.position;
                transform.rotation =  m_bone.transform.rotation;
            }
        }

        public void Fit()
        {
            var renderer = GetComponent<SpriteRenderer>();

            m_mask = gameObject.GetComponent<SpriteMask>();
            if (m_mask == null) m_mask = gameObject.AddComponent<SpriteMask>();
            m_mask.sprite = renderer.sprite;

            var skinner = GetComponent<SpriteSkin>();
            if (skinner == null)
            {
                Debug.LogError("No sprite skin object in this game object.");
                return;
            }

            if (skinner.boneTransforms.Length != 1)
            {
                Debug.LogWarning("Rigged bone count for rigged mask should be 1.");
                return;
            }

            m_bone = skinner.boneTransforms[0].gameObject;
            renderer.enabled = false;
            
#if UNITY_EDITOR
            if (!Application.isEditor) return;
            EditorUtility.SetDirty(gameObject);
#endif
        }
    }
}
