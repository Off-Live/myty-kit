using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using UnityEditorInternal;
using UnityEngine;

namespace MYTYKit.Components
{
    [DisallowMultipleComponent]
    public class MYTYAvatarBinder : MonoBehaviour
    {
        struct DiffItem
        {
            public Transform targetTransform;
            public Transform sourceTransform;
            public Quaternion sourceQuaternion;
            public Quaternion targetQuaternion;

        }

        bool m_isApplied = false;

        Dictionary<string, Quaternion> m_rootRotationMap = new();
        Dictionary<string, Transform> m_rootTfMap = new();

        Dictionary<Transform, List<DiffItem>> m_diffMap = new();
        Transform m_mainRootBone;

        public bool SetupRootBody(Transform rootBone)
        { 
            var transforms = rootBone.GetComponentsInChildren<Transform>();
            
            var rotMap = transforms.ToDictionary(tf => tf.name, tf => tf.rotation);
            if (transforms.Length != rotMap.Count)
            {
                Debug.LogError("The main body contains the duplicated names");
                return false;
            }

            m_mainRootBone = rootBone;
            m_rootRotationMap = rotMap;
            m_rootTfMap = transforms.ToDictionary(tf => tf.name, tf => tf);
            return true;
        }

        public void Bind(Transform traitRootBone)
        {
            m_diffMap[traitRootBone] = 
            traitRootBone.GetComponentsInChildren<Transform>().Select(traitTf =>
            {
                if (!m_rootRotationMap.ContainsKey(traitTf.name))
                {
                    Debug.LogWarning($"{traitTf.name} is not found in mainbody");
                    
                }
                return new DiffItem()
                {
                    sourceTransform = m_rootTfMap[traitTf.name],
                    sourceQuaternion = m_rootRotationMap[traitTf.name],
                    targetTransform = traitTf,
                    targetQuaternion = traitTf.rotation
                };
            }).ToList();
        }

        public void Unbind(Transform traitRootBone)
        {
            if (m_diffMap.ContainsKey(traitRootBone)) m_diffMap.Remove(traitRootBone);
        }
        
        public void Apply()
        {
            m_diffMap.Keys.ToList().ForEach(trait =>
            {
                trait.position = m_mainRootBone.position;
                m_diffMap[trait].ForEach(diffItem =>
                {
                    var sourceRotation = diffItem.sourceQuaternion;
                    if(sourceRotation==null) sourceRotation = Quaternion.identity;
                    diffItem.targetTransform.rotation = diffItem.sourceTransform.rotation *
                                                             sourceRotation.GetConjugate() *
                                                             diffItem.targetQuaternion;
                });
                
            });
            m_isApplied = true;
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