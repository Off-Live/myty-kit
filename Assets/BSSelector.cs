using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BSSelector : MonoBehaviour
{
    public MeFaMoConfig.FaceBlendShape blendShape;

    public bool isNeutral = true;
    Dictionary<string, GameObject> m_objMap= new();
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.localPosition = new Vector3(0,0,0.3f);
            child.localRotation = Quaternion.Euler(0,180,0);
            child.gameObject.SetActive(false);
            m_objMap[child.name] = child.gameObject;
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.gameObject.SetActive(false);
            
        }

        if (isNeutral)
        {
            m_objMap["Neutral"].SetActive(true);
        }
        else
        {
            var key = blendShape + "";
            key = char.ToLower(key.First()) + key.Substring(1);
            m_objMap[key].SetActive(true);
        }
    }
}
