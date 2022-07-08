using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.U2D;
using UnityEditor;
using System;



[System.Serializable]
public class AvatarTemplate
{
    public string PSBPath;
    public GameObject instance;
    public SpriteLibraryAsset spriteLibrary;
    public GameObject boneRootObj;
}

public class AvatarSelector: MonoBehaviour
{
    public List<AvatarTemplate> templates;
    public MYTYAssetScriptableObject mytyAssetStorage;
    public int id;
    
    private GameObject m_activeInstance;
    private SpriteLibraryAsset m_activeSLA;
    private GameObject m_activeBoneRoot;

    private ShaderMapAsset shaderMap;

    private void Start()
    {
        var shaderMapGO = FindObjectOfType<ShaderMap>();
        if (shaderMapGO != null)
        {
            shaderMap = shaderMapGO.shaderMap;
        }
        Configure();
    }

    public void ResetAvatar()
    {
        foreach(var template in templates)
        {
            var childCount = template.instance.transform.childCount;
            template.instance.SetActive(true);
            for(int i = 0; i < childCount; i++)
            {
                FixName(template.instance.transform.GetChild(i).gameObject);
                EnableLayer(template.instance.transform.GetChild(i).gameObject);
            }

        }
    }

    public void Configure()
    {
        m_activeInstance = null;

        var traitItem = mytyAssetStorage.traits[id];

        foreach(var template in templates)
        {

            if(template.PSBPath == traitItem.filename)
            {
                m_activeInstance = template.instance;
                m_activeSLA = template.spriteLibrary;
                m_activeBoneRoot = template.boneRootObj;
            }else
            {
                template.instance.SetActive(false);
        
            }
        }

        if (m_activeInstance == null)
        {
            Debug.Log("Proper psb template is not found. "+traitItem.filename);
            return;
        }
   
        int childCount = m_activeInstance.transform.childCount;
        var library = m_activeInstance.GetComponent<SpriteLibrary>();
        if (library == null) library = m_activeInstance.AddComponent<SpriteLibrary>();
        library.spriteLibraryAsset = m_activeSLA;
        if (m_activeSLA == null)
        {
            Debug.LogWarning("Sprite Library is not created yet!");
            return;
        }
        
        for (int i = 0; i < childCount; i++)
        {
            FixName(m_activeInstance.transform.GetChild(i).gameObject);
        }
                
        for (int i = 0; i < childCount; i++)
        {
            FixLayer(m_activeInstance.transform.GetChild(i).gameObject, new List<string>(), traitItem.traits.ToArray());
        }

    }


    private void FixName(GameObject templateNode)
    {
        int childCount = templateNode.transform.childCount;

        string name = templateNode.name;
        int sufIdx = name.LastIndexOf("_");

        if (sufIdx >= 0)
        {
            string surfix = name.Substring(sufIdx + 1);
            if (int.TryParse(surfix, out _))
            {
                name = name.Substring(0, sufIdx);
            }
        }

        templateNode.name = name;

        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                FixName(templateNode.transform.GetChild(i).gameObject);
            }
        }
    }

    private string HistoryToKey(List<string> history)
    {
        string ret = "";
        foreach (var elem in history)
        {
            ret += "/" + elem;
        }
        return ret;
    }

    private void EnableLayer(GameObject node)
    {
        node.SetActive(true);
        for(int i = 0; i < node.transform.childCount; i++)
        {
            EnableLayer(node.transform.GetChild(i).gameObject);
        }
    }

    private void FixLayer(GameObject templateNode, List<string> history, string[] traits)
    {
        int childCount = templateNode.transform.childCount;
        string name = templateNode.name;
        history.Add(name);

        string key = HistoryToKey(history);

        bool active = false;
        
        for(int i = 0; i < traits.Length; i++)
        {
            string trait = "/" + traits[i];
            if (key==trait || key.StartsWith(trait+"/"))
            {
                active = true;
            }
        }

        if (m_activeBoneRoot!=null && key.StartsWith("/" + m_activeBoneRoot.name)) active = true;

        if (active)
        {
            //Debug.Log(key);
            var node = templateNode.transform;
            while (node != null)
            {
                node.gameObject.SetActive(active);

                node = node.transform.parent;
            }
        }
        else
        {
            templateNode.SetActive(active);
        }

        for (int i = 0; i < childCount; i++)
        {
            FixLayer(templateNode.transform.GetChild(i).gameObject, history, traits);
        }

        if (childCount == 0)
        {
            string catName = "Animation" + key;
            var labels = m_activeSLA.GetCategoryLabelNames(catName);
            var labelList = new List<string>();

            foreach (var label in labels)
            {
                labelList.Add(label);
            }

            if (labelList.Count > 0)
            {
                var resolver = templateNode.GetComponent<SpriteResolver>();
                if (resolver == null) resolver = templateNode.AddComponent<SpriteResolver>();
                resolver.SetCategoryAndLabel(catName, labelList[labelList.Count - 1]);
            }

#if !UNITY_EDITOR
            if (Application.isPlaying)
            {
                var render = templateNode.GetComponent<SpriteRenderer>();
                if (render != null && shaderMap != null)
                {

                    foreach (var item in shaderMap.shaderMapList)
                    {
                        if (render.material != null)
                        {
                            var matName = render.material.name.Split(" ")[0];
                            if (item.name == matName)
                            {
                                render.material = item.material;
                            }
                        }

                    }
                }
            }
#endif
        }



        history.RemoveAt(history.Count - 1);
    }

}
