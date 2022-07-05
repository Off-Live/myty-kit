using System.Collections.Generic;
using UnityEngine;

public class RootController : MonoBehaviour
{

    private List<BoneController> _bones = new ();
    // Start is called before the first frame update
    void Start()
    {
        FindControllers<BoneController>(_bones,gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        foreach(var con in _bones)
        {
            if (con.orgRig == null || con.orgRig.Count == 0) continue;

            for(int i = 0; i < con.rigTarget.Count; i++)
            {
                con.rigTarget[i].transform.localPosition = con.orgRig[i].position;
                con.rigTarget[i].transform.localScale = con.orgRig[i].scale;
                con.rigTarget[i].transform.localRotation = con.orgRig[i].rotation;
            }
        }

        foreach(var con in _bones)
        {
            if (con.orgRig == null || con.orgRig.Count == 0) continue;
            con.ApplyDiff();
        }
    }

    public List<MYTYController> GetAllDecendant()
    {
        List<MYTYController> controllers = new();
        FindControllers(controllers, gameObject);
        return controllers;
    }
    
    void FindControllers<T>(List<T> conList, GameObject node)
    {
        var controller = node.GetComponent<T>();
        if (controller != null)
        {
            conList.Add(controller);
        }
        for(int i = 0; i < node.transform.childCount; i++)
        {
            FindControllers(conList, node.transform.GetChild(i).gameObject);
        }

    }


}
