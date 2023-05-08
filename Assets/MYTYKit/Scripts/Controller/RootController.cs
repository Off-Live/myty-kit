using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.Controllers
{
    public class RootController : MonoBehaviour
    {

        List<BoneController> _bones = new();

        // Start is called before the first frame update
        void Start()
        {
            FindControllers<BoneController>(_bones, gameObject);
        }

       

        void LateUpdate()
        {
            foreach (var con in _bones)
            {
                if (con.orgRig == null || con.orgRig.Count == 0) continue;

                for (int i = 0; i < con.rigTarget.Count; i++)
                {
                    if(con.rigTarget[i]==null) continue;
                    con.rigTarget[i].transform.localPosition = con.orgRig[i].position;
                    con.rigTarget[i].transform.localScale = con.orgRig[i].scale;
                    con.rigTarget[i].transform.localRotation = con.orgRig[i].rotation;
                }
            }

            foreach (var con in _bones)
            {
                if (con.orgRig == null || con.orgRig.Count == 0 || con.skip) continue;
                con.ApplyDiff();
            }
        }

        public List<MYTYController> GetAllDecendant()
        {
            List<MYTYController> controllers = new();
            FindControllers(controllers, gameObject);
            return controllers;
        }
        
        public void RefreshBoneList()
        {
            _bones.Clear();
            FindControllers<BoneController>(_bones, gameObject);
        }

        void FindControllers<T>(List<T> conList, GameObject node)
        {
            var controller = node.GetComponent<T>();
            if (controller != null)
            {
                conList.Add(controller);
            }

            for (int i = 0; i < node.transform.childCount; i++)
            {
                FindControllers(conList, node.transform.GetChild(i).gameObject);
            }

        }
    }
}
