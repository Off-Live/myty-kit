using System.Collections.Generic;
using System.Linq;
using MYTYKit.MotionAdapters;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UniGLTF;


namespace MYTYKit.AvatarImporter
{
    [DisallowMultipleComponent]
    public class MYTY3DAvatarImporter : MonoBehaviour
    {
        public MYTY3DAvatarDriver driver;
        public Avatar avatar;
        
        List<RuntimeGltfInstance> m_instances = new();
        SkinnedMeshRenderer m_mainSmr;
        Transform m_rootBone;
        Transform m_avatarRoot;

        Dictionary<Transform, Transform> m_rootBoneMap = new();

        public bool LoadMainbody(byte[] modelData, string jsonString)
        {
            if (driver == null)
            {
                Debug.LogError("The driver is not setup properly");
                return false;
            }
       
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
                Debug.LogWarning("No root bone in mainbody");
                instance.Dispose();
                return false;
            }
            m_avatarRoot = instance.transform;
            m_avatarRoot.parent = driver.transform;
            m_instances.Add(instance);
            
            avatar = HumanoidAvatarBuilder.CreateAvatarFromJson(m_avatarRoot.gameObject, (JObject)jObj["avatar"]);
            driver.GetComponent<Animator>().avatar = avatar;
            driver.binder.SetupRootBody(m_rootBone);
            driver.DeserializeFromJObject((JObject)jObj["driver"]);
            driver.CheckAndSetupBlendShape(m_avatarRoot);
            driver.humanoidAvatarRoot = m_avatarRoot;
            driver.Initialize();
            
            return true;
        }

        public bool LoadTrait(byte[] bytes, string loadName)
        {
            if (m_avatarRoot == null) return false;
            var instance = LoadGlb(bytes, loadName);
            instance.transform.parent = driver.transform;
            m_instances.Add(instance);
            var rootBone = FixAndGetRootBone(instance.transform);
            driver.binder.Bind(rootBone);
            driver.CheckAndSetupBlendShape(instance.transform);
            m_rootBoneMap[instance.transform] = rootBone;
            return true;
        }

        public void UnloadTrait(string name)
        {
            var traitTf = m_instances.FirstOrDefault(instance=> instance.transform.name == name);
            if (traitTf == null) return;
            m_instances.Remove(traitTf);
            driver.binder.Unbind(m_rootBoneMap[traitTf.transform]);
            m_rootBoneMap.Remove(traitTf.transform);
            traitTf.Dispose();
        }
        
        RuntimeGltfInstance LoadGlb(byte[] bytes, string loadName)
        {
            
            using(var glbData = new GlbBinaryParser(bytes, loadName).Parse())
            using (var loader = new ImporterContext(glbData))
            {
                loader.InvertAxis = Axes.X;
                var instance = loader.Load();
                instance.name = loadName;
                instance.EnableUpdateWhenOffscreen();
                instance.ShowMeshes();
                return instance;
            }
        }

        Transform FixAndGetRootBone(Transform instance)
        {
            var rootBoneName = m_rootBone.name;
            var children = instance.GetComponentsInChildren<Transform>();
            var rootTf = children.First(tf => tf.name == rootBoneName);
            if (rootTf == null)
            {
                Debug.LogWarning("Cannot find root bone");
                return null;
            }
            instance.GetComponentsInChildren<SkinnedMeshRenderer>().ToList().ForEach( smr => smr.rootBone = rootTf);
            return rootTf;
        }
        
    }
}