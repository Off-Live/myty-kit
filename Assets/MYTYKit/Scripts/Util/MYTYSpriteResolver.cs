using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.U2D;

public class MYTYSpriteResolver : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] SpriteLibraryAsset m_spriteLibraryAsset;
    [SerializeField] SpriteRenderer m_renderer;

    [SerializeField] string m_category;
    [SerializeField] string m_label;

    public SpriteLibraryAsset spriteLibraryAsset
    {
        get
        {
            
            return m_spriteLibraryAsset;
        }
        set
        {
            m_spriteLibraryAsset = value;
        }
    }

    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
       
       

        
    }

    public void SetCategoryAndLabel(string category, string label)
    {
        if (m_spriteLibraryAsset == null) return;
        if (m_renderer == null)
        {
            m_renderer = GetComponent<SpriteRenderer>();
            if(m_renderer==null) return;

#if UNITY_EDITOR
            if (Application.isEditor)
            {
                var so = new SerializedObject(this);
                so.FindProperty("m_renderer").objectReferenceValue = m_renderer;
                so.ApplyModifiedProperties();
            }

#endif
        }
        if (m_category == category && m_label == label) return;
        m_category = category;
        m_label = label;
        
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            var so = new SerializedObject(this);
            so.FindProperty("m_category").stringValue = m_category;
            so.FindProperty("m_label").stringValue = m_label;
            so.ApplyModifiedProperties();
        }
#endif
        m_renderer.sprite = m_spriteLibraryAsset.GetSprite(category, label);

    }

    public string GetCategory()
    {
        return m_category;
    }

    public string GetLabel()
    {
        return m_label;
    }
}
