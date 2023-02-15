using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using MYTYKit.Components;
using UnityEngine;
using UniGLTF;
using UnityEditor.Animations;

namespace MYTYKit.AvatarImporter
{
    public class MYTY3DAvatarImporter : MonoBehaviour
    {
        public List<string> mainBodies;

        public AnimatorController animatorController;
        
        List<RuntimeGltfInstance> m_instances = new();
        SkinnedMeshRenderer m_mainSmr;
        GameObject m_avatarRoot;

        public void Start()
        {
            var dir = new DirectoryInfo("Assets/TestAssets/XOCIETY/");
            dir.EnumerateFiles("*.glb").ToList().ForEach(fileInfo =>
                LoadGlb(fileInfo.FullName)
            );
            FixRootBone();

            if (m_mainSmr != null)
            {
                var binder = GetComponent<MYTYAvatarBinder>();
                binder.mainBodies.Clear();
                binder.mainBodies.Add(m_mainSmr);
                binder.Bind();
            }

            CreateAnimator();
        }

        public void LoadGlb(string path)
        {
            var info = new FileInfo(path);
            LoadGlb(File.ReadAllBytes(path), info.Name);
        }

        public void LoadGlb(byte[] bytes, string loadName)
        {
            using(var glbData = new GlbBinaryParser(bytes, loadName).Parse())
            using (var loader = new ImporterContext(glbData))
            {
                var instance = loader.Load();
                instance.name = loadName;
                instance.EnableUpdateWhenOffscreen();
                instance.ShowMeshes();
                instance.transform.parent = transform;
                m_instances.Add(instance);
            }
        }

        void FixRootBone()
        {
            var smrs = GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            m_mainSmr = smrs.First(smr => mainBodies.Contains(smr.name));
            if (m_mainSmr == null)
            {
                Debug.LogWarning("No main body mesh!");
                return;
            }

            var rootBoneName = m_mainSmr.rootBone.name;
            m_avatarRoot = m_mainSmr.rootBone.parent.gameObject;
            
            m_instances.ForEach(instance =>
            {
                var children = instance.GetComponentsInChildren<Transform>();
                var rootTf = children.First(tf => tf.name == rootBoneName);
                if (rootTf == null)
                {
                    Debug.LogWarning("Cannot find root bone");
                    return;
                }
                instance.GetComponentsInChildren<SkinnedMeshRenderer>().ToList().ForEach( smr => smr.rootBone = rootTf);
            });

        }

        void CreateAnimator()
        {
            var animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            animator.avatar = HumanoidAvatarMaker.MakeAvatar(m_avatarRoot);
        }
    }
}