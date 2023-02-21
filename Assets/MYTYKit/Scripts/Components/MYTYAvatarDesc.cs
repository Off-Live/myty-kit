using System;
using System.IO;
using System.Linq;
using MYTYKit.MotionAdapters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.Components
{
    public class MYTYAvatarDesc : MonoBehaviour
    {
        public SkinnedMeshRenderer mainBody;
        public Transform rootBone;
        public HumanoidAvatarBuilder avatarBuilder;

        void Start()
        {
            var binder = GetComponent<MYTYAvatarBinder>();
            var driver = GetComponent<MYTY3DAvatarDriver>();
            var anim = GetComponent<Animator>();
            if (anim.avatar == null) anim.avatar = avatarBuilder.avatar;
            if (mainBody != null)
            {
                FixRootBone();
            }

            if (binder != null && mainBody!=null)
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
                rootBoon = rootBone.name,
                avatarRoot = avatarRoot.name,
                referenceScale = new
                {
                    transform.localScale.x,
                    transform.localScale.y,
                    transform.localScale.z  
                },
                avatar = avatarBuilder.ExportToJObject(),
                driver = driver.ExportToJObject()
            });
            return json.ToString(Formatting.Indented);
        }

    }
}