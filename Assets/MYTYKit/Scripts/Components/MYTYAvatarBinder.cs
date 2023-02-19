using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MYTYKit.Components
{
    public class MYTYAvatarBinder : MonoBehaviour
    {
        struct DiffItem
        {
            public Transform targetTransform;
            public Transform sourceTransform;
            public Quaternion sourceQuaternion;
            public Quaternion targetQuaternion;

        }

        struct RootDisplacement
        {
            public Transform sourceRootBone;
            public Transform targetRootBone;
        }

        public List<SkinnedMeshRenderer> mainBodies;
        
        Dictionary<string, List<DiffItem>> m_rotationDiffMap = new();
        Dictionary<string, RootDisplacement> m_rootDispMap = new();

        bool m_isApplied = false;
        
        public void Bind()
        {
            var childrenSmrs = GetComponentsInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer sourceSmr = null;


            foreach (var childSmr in childrenSmrs)
            {
                if(mainBodies.Contains(childSmr))
                {
                    
                    sourceSmr = childSmr;
                    break;
                }
           

            }

            if (sourceSmr == null)
            {
                Debug.LogWarning(
                    "Cannot find main body mesh. Please check the name of mainBodies.");
                return;
            }

            foreach (var childSmr in childrenSmrs)
            {
                // Check if already attached to the correct skeleton
                if (childSmr.rootBone == sourceSmr.rootBone)
                {
                    continue;
                }
                m_rotationDiffMap[childSmr.name] = CalculateDiffList(childSmr.rootBone, sourceSmr.rootBone);
                m_rootDispMap[childSmr.name] = new RootDisplacement()
                {
                    sourceRootBone = sourceSmr.rootBone,
                    targetRootBone = childSmr.rootBone
                };
            }

        }

        public void Apply()
        {
            m_rootDispMap.Keys.ToList().ForEach(key =>{
                var disp =m_rootDispMap[key];
                disp.targetRootBone.position = disp.sourceRootBone.position;
            });
            m_rotationDiffMap.Keys.ToList().ForEach( key => 
                m_rotationDiffMap[key].ForEach( item => 
                    // item.targetTransform.rotation = item.rotDiff* item.sourceTransform.rotation
                    item.targetTransform.rotation = item.sourceTransform.rotation * item.sourceQuaternion.GetConjugate()*item.targetQuaternion
                ));

            m_isApplied = true;
        }

        List<DiffItem> CalculateDiffList(Transform childRootBone, Transform sourceRootBone)
        {
            var list = new List<DiffItem>();

            var dfs = new Stack<Transform>();
            dfs.Push(childRootBone);
            var sourceBones = sourceRootBone.GetComponentsInChildren<Transform>().ToList();
           
            while (dfs.Count > 0)
            {
                var curTF = dfs.Pop();
                var filteredBone = sourceBones.Where(bone => bone.name == curTF.name).ToArray();
                if (filteredBone.Length == 0)
                {
                    Debug.LogWarning($"{curTF.name} is not in target bones");
                }else 
                {
                    if (filteredBone.Length > 1) Debug.LogWarning($"more than one {curTF.name} are in target bones");
                    
                    list.Add(new DiffItem()
                    {
                        targetTransform = curTF,
                        sourceTransform = filteredBone[0],
                        targetQuaternion = curTF.rotation,
                        sourceQuaternion = filteredBone[0].rotation
                    });
                }
                Enumerable.Range(0, curTF.childCount).ToList().ForEach(idx =>
                {
                    dfs.Push(curTF.GetChild(idx));
                });
            }

            return list;

        }

        void Update()
        {
            m_isApplied = false;
        }
        
        void LateUpdate()
        {
           if(!m_isApplied) Apply();
        }
    }
}