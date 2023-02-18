using System;
using System.Linq;
using MYTYKit.MotionAdapters;
using UnityEngine;

namespace MYTYKit.Components
{
    public class MYTYAvatarDesc : MonoBehaviour
    {
        public SkinnedMeshRenderer mainBody;
        public HumanoidAvatarBuilder avatarBuilder;

        void Start()
        {
            var binder = GetComponent<MYTYAvatarBinder>();
            var driver = GetComponent<MYTY3DAvatarDriver>();

            if (mainBody == null) return;
            
            FixRootBone();
            if (binder != null)
            {
                binder.mainBodies = new();
                binder.mainBodies.Add(mainBody);
                binder.Bind();
                
            }

            if (driver != null && avatarBuilder!=null)
            {
                driver.humanoidAvatarRoot = avatarBuilder.avatarRoot;
                driver.Initialize();
               
            }
        }
        
        void FixRootBone()
        {
            var smrs = GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
          
            var rootBoneName = mainBody.rootBone.name;
            
            transform.GetChildrenList().ForEach(modelRoot =>
            {
                var children = modelRoot.GetComponentsInChildren<Transform>();
                var rootTf = children.First(tf => tf.name == rootBoneName);
                if (rootTf == null)
                {
                    Debug.LogWarning("Cannot find root bone");
                    return;
                }
                modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>().ToList().ForEach( smr => smr.rootBone = rootTf);
            });

        }

    }
}