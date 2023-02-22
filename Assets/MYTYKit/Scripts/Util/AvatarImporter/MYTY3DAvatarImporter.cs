using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MYTYKit.Components;
using MYTYKit.MotionAdapters;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UniGLTF;
using UnityEditor.Animations;

namespace MYTYKit.AvatarImporter
{
    [DisallowMultipleComponent]
    public class MYTY3DAvatarImporter : MonoBehaviour
    {
        // public List<string> mainBodies;
        //
        
        // public Avatar avatar;


        public AnimatorController animatorController;
        public MYTYIKTarget leftHandTarget;
        public MYTYIKTarget rightHandTarget;

        public bool isAutoBind = false;
        public MYTYAvatarBinder binder;
        
        List<RuntimeGltfInstance> m_instances = new();
        SkinnedMeshRenderer m_mainSmr;
        Transform m_rootBone;
        Transform m_avatarRoot;
        

        // public void Start()
        // {
        //     var dir = new DirectoryInfo("Assets/TestAssets/XOCIETY/");
        //     dir.EnumerateFiles("*.glb").ToList().ForEach(fileInfo =>
        //         LoadGlb(fileInfo.FullName)
        //     );
        //     FixRootBone();
        //
        //     if (m_mainSmr != null)
        //     {
        //         var binder = GetComponent<MYTYAvatarBinder>();
        //         binder.mainBodies.Clear();
        //         binder.mainBodies.Add(m_mainSmr);
        //         binder.Bind();
        //     }
        //     CreateAnimator();
        //
        //     var driver = GetComponent<MYTY3DAvatarDriver>();
        //     driver.humanoidAvatarRoot = m_avatarRoot.transform;
        //     driver.Initialize();
        //     
        // }


        public void Start()
        {
            if (isAutoBind)
            {
                binder = GetComponent<MYTYAvatarBinder>();
                if (binder == null) binder = gameObject.AddComponent<MYTYAvatarBinder>();
            }
            
        }

        public bool LoadMainbody(byte[] modelData, string jsonString)
        {
            try
            {
                var jObj = JObject.Parse(jsonString);
                var mainBodyName = (string)jObj["mainBody"];
                var rootBoneName = (string)jObj["rootBone"];
                var avatarRootName = (string)jObj["avatarRoot"];
                transform.localScale = jObj["referenceScale"].ToObject<Vector3>();
                var instance = LoadGlb(modelData, avatarRootName);

                m_mainSmr = instance.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault(smr => smr.name == mainBodyName);
                if (m_mainSmr == null)
                {
                    instance.Dispose();
                    return false;
                }

                m_rootBone = instance.GetComponentsInChildren<Transform>()
                    .FirstOrDefault(tf => tf.name == rootBoneName);
                if (m_rootBone == null)
                {
                    instance.Dispose();
                    return false;
                }
                m_avatarRoot = instance.transform;
                m_avatarRoot.parent = transform;
                m_instances.Add(instance);
                return true;
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e.StackTrace);
                return false;
            }
        }
        
        
        
        // public void LoadGlb(string path)
        // {
        //     var info = new FileInfo(path);
        //     LoadGlb(File.ReadAllBytes(path), info.Name);
        // }

        public RuntimeGltfInstance LoadGlb(byte[] bytes, string loadName)
        {
            
            using(var glbData = new GlbBinaryParser(bytes, loadName).Parse())
            using (var loader = new ImporterContext(glbData))
            {
                
                var instance = loader.Load();
                instance.name = loadName;
                instance.EnableUpdateWhenOffscreen();
                instance.ShowMeshes();
                return instance;
            }
        }

        // void FixRootBone()
        // {
        //     var smrs = GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
        //     m_mainSmr = smrs.First(smr => mainBodies.Contains(smr.name));
        //     if (m_mainSmr == null)
        //     {
        //         Debug.LogWarning("No main body mesh!");
        //         return;
        //     }
        //
        //     var rootBoneName = m_mainSmr.rootBone.name;
        //     m_avatarRoot = m_instances.First(instance =>
        //         instance.GetComponentsInChildren<SkinnedMeshRenderer>().Contains(m_mainSmr)).gameObject;
        //     m_instances.ForEach(instance =>
        //     {
        //         var children = instance.GetComponentsInChildren<Transform>();
        //         var rootTf = children.First(tf => tf.name == rootBoneName);
        //         if (rootTf == null)
        //         {
        //             Debug.LogWarning("Cannot find root bone");
        //             return;
        //         }
        //         instance.GetComponentsInChildren<SkinnedMeshRenderer>().ToList().ForEach( smr => smr.rootBone = rootTf);
        //     });
        //
        // }
        //
        // void CreateAnimator()
        // {
        //     var animator = gameObject.AddComponent<Animator>();
        //     animator.runtimeAnimatorController = animatorController;
        //     animator.avatar = avatar; //HumanoidAvatarMaker.MakeAvatar(m_avatarRoot);
        // }

       
    }
}