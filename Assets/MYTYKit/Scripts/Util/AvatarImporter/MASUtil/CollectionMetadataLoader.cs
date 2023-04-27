using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.AvatarImporter
{
    public partial class MASImporterAsync
    {
        float m_skeletonResumeTs = 0.0f;
        float m_controllerResumeTs = 0.0f;
        IEnumerator LoadSkeleton(JObject skeleton, Transform go, float timeout)
        {
            var currentTs = Time.realtimeSinceStartup;
            if (currentTs - m_skeletonResumeTs > timeout)
            {
                yield return null;
                m_skeletonResumeTs = Time.realtimeSinceStartup;
            }
            go.name = (string)skeleton["name"];
            go.transform.Deserialize(skeleton["transform"] as JObject);
            m_transformMap[(int)skeleton["id"]] = go.transform;

            var childrenJA = skeleton["children"] as JArray;
            var childCount = childrenJA.Count;
            for (int i = 0; i < childCount; i++)
            {
                var childGo = new GameObject();
                childGo.transform.parent = go.transform;
                yield return LoadSkeleton(childrenJA[i] as JObject, childGo.transform, timeout);
            }
            
        }

        IEnumerator LoadJointPhysics(JObject[] physicsComponent, float timeout)
        {
            var resumeTs = Time.realtimeSinceStartup;
           
            foreach (var jObject in physicsComponent)
            {
                var tf = m_transformMap[(int)jObject["id"]];
                foreach (var jToken in jObject["unityComponents"].ToArray())
                {
                    var currentTs = Time.realtimeSinceStartup;
                    if (currentTs - resumeTs > timeout)
                    {
                        yield return null;
                        resumeTs = Time.realtimeSinceStartup;
                    }
                    var componentJo = jToken as JObject;
                    var typeKey = (string)componentJo["typeKey"];
                    var typeFullName = (string)componentJo["typeFullName"];
                    if (JointPhysicsSetting.DeserializeActions.ContainsKey(typeKey))
                    {
                        var component = tf.GetComponent(Type.GetType(typeFullName));
                        if (component == null)
                        {
                            component = tf.gameObject.AddComponent(Type.GetType(typeFullName));
                        }

                        JointPhysicsSetting.DeserializeActions[typeKey](component, componentJo, m_transformMap);
                    }
                }
            }
        }
        
        IEnumerator LoadRootController(JObject rootController, GameObject go, float timeout)
        {
            go.name = (string)rootController["name"];
            go.AddComponent<RootController>();
            m_controllerResumeTs = Time.realtimeSinceStartup;
            foreach (var child in rootController["children"])
            {
                var childGo = new GameObject();
                childGo.transform.parent = go.transform;
                yield return LoadController(child as JObject, childGo, timeout);
            }
          
        }

        IEnumerator LoadController(JObject controller, GameObject go, float timeout)
        {
            var currentTs = Time.realtimeSinceStartup;
            if (currentTs - m_controllerResumeTs > timeout)
            {
                yield return null;
                m_controllerResumeTs = Time.realtimeSinceStartup;
            }
            var typeString = (string)controller["type"];
            var assemName = typeof(MYTYController).Assembly.GetName().Name;

            Debug.Assert(!string.IsNullOrEmpty(typeString));
            var qualifiedType = "MYTYKit.Controllers." + typeString + ", " + assemName;

            var component = go.AddComponent(Type.GetType(qualifiedType)) as MYTYController;
            Debug.Assert(component != null);
            component.DeserializeFromJObject(controller, m_transformMap);
            m_transformMap[(int)controller["id"]] = go.transform;

            foreach (var child in controller["children"])
            {
                var childGo = new GameObject();
                childGo.transform.parent = go.transform;
                yield return LoadController(child as JObject, childGo, timeout);
            }
        }
        
        IEnumerator LoadMotionTemplates(JObject jObject, Transform parent)
        {
            var go = new GameObject()
            {
                name = "MotionTemplateMapper",
                transform =
                {
                    parent = parent
                }
            };

            var mapper = go.AddComponent<MotionTemplateMapper>();
            mapper.DeserializeFromJObject(jObject);
            motionTemplateMapper = mapper;
            yield return null;
        }

        void LoadMotionAdapter(JObject jObject, Transform parent)
        {
            var go = new GameObject();
            go.transform.parent = parent;
            var typeName = typeof(NativeAdapter).Namespace + "." + (string)jObject["type"] + ", "
                           + typeof(NativeAdapter).Assembly.GetName().Name;
            var adapter = (NativeAdapter)go.AddComponent(Type.GetType(typeName));
            Debug.Assert(adapter != null);

            adapter.SetMotionTemplateMapper(motionTemplateMapper);
            ((ISerializableAdapter)adapter).DeserializeFromJObject(jObject, m_transformMap);
        }

        void LoadARFaceData(JObject jObject, Transform templateRoot)
        {
            m_isAROnly = (bool)jObject["AROnly"];
            m_arData = jObject["items"].ToList().Select((token,idx)=>
            {
                
                var headBone = m_transformMap[(int)token["headBone"]];
                var cameraGo = new GameObject($"ARRenderCam{idx}");
                cameraGo.transform.parent = templateRoot;
                var camera = cameraGo.AddComponent<Camera>();
                camera.DeserializeFromJObject(token["renderCam"] as JObject);
                var arTexture = new RenderTexture((int)token["ARTexture"]["textureWidth"],
                    (int)token["ARTexture"]["textureHeight"], 0, RenderTextureFormat.ARGB32);
                camera.targetTexture = arTexture;
                return new ARDataRuntime()
                {
                    arTexture = arTexture,
                    headBone = headBone,
                    renderCam = camera,
                    isValid = (bool) token["isValid"]
                };
            }).ToList();
        }

        void LockController()
        {
            var rootBone = m_rootBones[m_templateId];
            var headBone = m_arData[m_templateId].headBone;
            var parentBoneList = new List<Transform>(){rootBone};
            var currBone = headBone;
            while (currBone != rootBone)
            {
                parentBoneList.Add(currBone);
                currBone = currBone.parent;
            }

            var rootController = m_rootControllers[m_templateId];
            
            rootController.GetComponentsInChildren<BoneController>()
                .Where(controller => parentBoneList.Count(bone=> controller.rigTarget.Contains(bone.gameObject))>0)
                .ToList().ForEach(controller=> controller.skip = true);
        }

        void UnlockController()
        {
            var rootController = m_rootControllers[m_templateId];
            foreach (var controller in rootController.GetComponentsInChildren<BoneController>())
            {
                controller.skip = false;
            }
        }
    }
}