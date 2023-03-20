using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MYTYKit.MotionAdapters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.Components
{
    [DisallowMultipleComponent]
    public class MYTYAvatarDesc : MonoBehaviour
    {
        public SkinnedMeshRenderer mainBody;
        public Transform rootBone;
        public HumanoidAvatarBuilder avatarBuilder;

        List<Transform> m_traitRootBones = new();
        void Start()
        {
            var binder = GetComponent<MYTYAvatarBinder>();
            var driver = GetComponent<MYTY3DAvatarDriver>();
            var anim = GetComponent<Animator>();
            if (anim.avatar == null) anim.avatar = avatarBuilder.avatar;
            if (mainBody != null)
            {
                FixAndFindRootBone();
                if (binder != null )
                {
                    binder.SetupRootBody(rootBone);
                    m_traitRootBones.ForEach(traitRoot=>binder.Bind(traitRoot));
                }

            }

           
            if (driver != null && avatarBuilder!=null)
            {
                driver.humanoidAvatarRoot = avatarBuilder.avatarRoot;
                driver.Initialize();
               
            }

        }
        
        void FixAndFindRootBone()
        {
            if (mainBody.rootBone == null) mainBody.rootBone = rootBone;
            var rootBoneName = mainBody.rootBone.name;
            mainBody.updateWhenOffscreen = true;
            transform.GetChildrenList().ForEach(modelRoot =>
            {
                var children = modelRoot.GetComponentsInChildren<Transform>();
                var rootTf = children.First(tf => tf.name == rootBoneName);
                if (rootTf == null)
                {
                    Debug.LogWarning("Cannot find root bone");
                    return;
                }
                m_traitRootBones.Add(rootTf);
                modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>().ToList().ForEach( smr =>
                {
                    smr.updateWhenOffscreen = true;
                    smr.rootBone = rootTf;
                });
            });

        }

        public string ExportToJson()
        {
            var avatarRoot = avatarBuilder.avatarRoot;
            var driver = GetComponent<MYTY3DAvatarDriver>();
            
            var json = JObject.FromObject(new
            {
                mainBody = mainBody.name,
                rootBone = rootBone.name,
                avatarRoot = avatarRoot.name,
                referenceScale = new
                {
                    transform.localScale.x,
                    transform.localScale.y,
                    transform.localScale.z  
                },
                avatar = avatarBuilder.ExportToJObject(),
                driver = driver.SerializeToJObject()
            });
            return json.ToString(Formatting.Indented);
        }

    }
}