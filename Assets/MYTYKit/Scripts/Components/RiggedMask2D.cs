using UnityEngine.U2D.Animation;
using UnityEngine;
using UnityEditor;

namespace MYTYKit.Components
{
    [DisallowMultipleComponent]
    public class RiggedMask2D : MonoBehaviour
    {

        [SerializeField] SpriteMask m_mask;
        [SerializeField] GameObject m_bone;

        Vector3 m_offset;
        Quaternion m_rotOffset;

        SpriteRenderer m_renderer;

        // Start is called before the first frame update
        void Start()
        {
            if (m_bone == null) return;

            m_offset = gameObject.transform.position - m_bone.transform.position;
            m_rotOffset = Quaternion.Inverse(m_bone.transform.rotation);
            m_renderer = GetComponent<SpriteRenderer>();

        }

        // Update is called once per frame
        void Update()
        {
            if (m_bone == null) return;
            
            m_mask.sprite = m_renderer.sprite;
            gameObject.transform.rotation = m_bone.transform.rotation * m_rotOffset;
            gameObject.transform.position =
                m_bone.transform.rotation * m_rotOffset * m_offset + m_bone.transform.position;
        }

        public void Fit()
        {
            var renderer = GetComponent<SpriteRenderer>();

            gameObject.transform.position = renderer.bounds.center;
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

#if UNITY_EDITOR
            if (!Application.isEditor) return;
            EditorUtility.SetDirty(gameObject);
#endif
        }
    }
}
