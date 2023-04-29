using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Components;
using Newtonsoft.Json.Linq;
using UnityEngine.U2D.Animation;

namespace MYTYKit.Controllers
{
    [Serializable]
    public class RiggingEntity
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public JObject SerializeToJObject()
        {
            return JObject.FromObject(new
            {
                position = new
                {
                    position.x,
                    position.y,
                    position.z
                },
                rotation = new
                {
                    rotation.x,
                    rotation.y,
                    rotation.z,
                    rotation.w
                },
                scale = new
                {
                    scale.x,
                    scale.y,
                    scale.z,
                }
            });
        }
    }
    [DisallowMultipleComponent]
    public abstract class MYTYController : MonoBehaviour
    {
        public abstract void PrepareToSave();
        public abstract void PostprocessAfterLoad(Dictionary<GameObject, GameObject> objMap);
        public abstract JObject SerializeToJObject(Dictionary<Transform,int> tfMap);
        public abstract void DeserializeFromJObject(JObject jObject, Dictionary<int, Transform> idTransformMap);
    }

    public abstract class BoneController : MYTYController
    {
        public List<GameObject> rigTarget;
        public List<RiggingEntity> orgRig;
        public bool skip = false;
        public abstract void ApplyDiff();
        protected abstract List<RiggingEntity> CalcInterpolate();

        public override void PrepareToSave()
        {
#if UNITY_EDITOR
            for (int i = 0; i < rigTarget.Count; i++)
            {
                if (rigTarget[i] == null) continue;
                rigTarget[i] = PrefabUtility.GetCorrespondingObjectFromSource(rigTarget[i]);
            }
#endif
        }

        public override void PostprocessAfterLoad(Dictionary<GameObject, GameObject> objMap)
        {
            for (int i = 0; i < rigTarget.Count; i++)
            {
                if (rigTarget[i] == null) continue;
                rigTarget[i] = objMap[rigTarget[i]];
            }
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                var so = new SerializedObject(this);
                for (int i = 0; i < rigTarget.Count; i++)
                {
                    if (rigTarget[i] == null) continue;
                    so.FindProperty("rigTarget").GetArrayElementAtIndex(i).objectReferenceValue = rigTarget[i];
                }

                so.ApplyModifiedProperties();
            }
#endif
        }

        public void InterpolateGUI()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var interp = CalcInterpolate();
                if (interp == null) return;
                SetPose(interp);
            }
#endif
        }

        public void ToOrigin()
        {
            if (orgRig == null || orgRig.Count == 0) return;
            for (int i = 0; i < rigTarget.Count; i++)
            {
                if(rigTarget[i]==null) continue;
                rigTarget[i].transform.localPosition = orgRig[i].position;
                rigTarget[i].transform.localRotation = orgRig[i].rotation;
                rigTarget[i].transform.localScale = orgRig[i].scale;
            }
        }

        protected void SetPose(List<RiggingEntity> poseList)
        {
            for (int i = 0; i < rigTarget.Count; i++)
            {
                if(rigTarget[i]==null) continue;
                rigTarget[i].transform.localPosition = poseList[i].position;
                rigTarget[i].transform.localScale = poseList[i].scale;
                rigTarget[i].transform.localRotation = poseList[i].rotation;
            }
        }

        protected void AccumulatePose(List<RiggingEntity> diffList)
        {
            for (int i = 0; i < rigTarget.Count; i++)
            {
                if(rigTarget[i]==null) continue;
                rigTarget[i].transform.localPosition += diffList[i].position;
                rigTarget[i].transform.localRotation = diffList[i].rotation * rigTarget[i].transform.localRotation;

                var scaleX = rigTarget[i].transform.localScale.x * diffList[i].scale.x;
                var scaleY = rigTarget[i].transform.localScale.y * diffList[i].scale.y;
                var scaleZ = rigTarget[i].transform.localScale.z * diffList[i].scale.z;
                rigTarget[i].transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            }
        }

        protected List<RiggingEntity> CalcDiff(List<RiggingEntity> fromList, List<RiggingEntity> toList)
        {

            var diffList = new List<RiggingEntity>();
            for (int i = 0; i < toList.Count; i++)
            {
                var diff = new RiggingEntity();
                diff.position = toList[i].position - fromList[i].position;
                var scaleX = fromList[i].scale.x == 0 ? 0 : toList[i].scale.x / fromList[i].scale.x;
                var scaleY = fromList[i].scale.y == 0 ? 0 : toList[i].scale.y / fromList[i].scale.y;
                var scaleZ = fromList[i].scale.z == 0 ? 0 : toList[i].scale.z / fromList[i].scale.z;
                diff.scale = new Vector3(scaleX, scaleY, scaleZ);
                diff.rotation = toList[i].rotation * Quaternion.Inverse(fromList[i].rotation);

                diffList.Add(diff);
            }

            return diffList;

        }

        public override JObject SerializeToJObject(Dictionary<Transform,int> tfMap)
        {
            return JObject.FromObject(new
            {
                rigTarget = rigTarget.Select(item => tfMap[item.transform]).ToArray(),
                orgRig = orgRig.Select(item => item.SerializeToJObject()),
                skip
            });
        }

        public override void DeserializeFromJObject(JObject jObject, Dictionary<int, Transform> idTransformMap)
        {
            rigTarget = jObject["rigTarget"].ToObject<List<int>>().Select(id => idTransformMap[id].gameObject).ToList();
            orgRig = jObject["orgRig"].ToObject<List<RiggingEntity>>().ToList();
            skip = (bool)jObject["skip"];
        }
    }

    public abstract class SpriteController : MYTYController
    {
        public List<SpriteResolver> spriteObjects;
        
        public override void PrepareToSave()
        {
#if UNITY_EDITOR
            for (int i = 0; i < spriteObjects.Count; i++)
            {
                spriteObjects[i] = PrefabUtility.GetCorrespondingObjectFromSource(spriteObjects[i]);
            }
#endif
        }

        public override void PostprocessAfterLoad(Dictionary<GameObject, GameObject> objMap)
        {
            for (int i = 0; i < spriteObjects.Count; i++)
            {
                spriteObjects[i] = objMap[spriteObjects[i].gameObject].GetComponent<SpriteResolver>();
            }
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                var so = new SerializedObject(this);
                for (int i = 0; i < spriteObjects.Count; i++)
                {
                    so.FindProperty("spriteObjects").GetArrayElementAtIndex(i).objectReferenceValue = spriteObjects[i];
                }

                so.ApplyModifiedProperties();
            }
#endif
        }

        public override JObject SerializeToJObject(Dictionary<Transform, int> tfMap)
        {
            return JObject.FromObject(new
            {
                spriteObjects = spriteObjects.Select(item => tfMap[item.transform]).ToArray(),
            });
        }
    }

    public abstract class MSRSpriteController : MYTYController
    {
        public List<MYTYSpriteResolver> spriteObjects;
        public bool isRuntimeMode = false;

        public List<int> resolverIds { get; set; }

        List<MYTYSpriteResolverRuntime> m_spriteResolverRuntimes = new();
        public override void PrepareToSave()
        {
#if UNITY_EDITOR
            for (int i = 0; i < spriteObjects.Count; i++)
            {
                if(spriteObjects[i]==null) continue;
                spriteObjects[i] = PrefabUtility.GetCorrespondingObjectFromSource(spriteObjects[i]);
            }
#endif
        }

        public override void PostprocessAfterLoad(Dictionary<GameObject, GameObject> objMap)
        {
            for (int i = 0; i < spriteObjects.Count; i++)
            {
                if(spriteObjects[i]==null) continue;
                spriteObjects[i] = objMap[spriteObjects[i].gameObject].GetComponent<MYTYSpriteResolver>();
            }
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                var so = new SerializedObject(this);
                for (int i = 0; i < spriteObjects.Count; i++)
                {
                    if(spriteObjects[i]==null) continue;
                    so.FindProperty("spriteObjects").GetArrayElementAtIndex(i).objectReferenceValue = spriteObjects[i];
                }

                so.ApplyModifiedProperties();
            }
#endif
        }
        public override JObject SerializeToJObject(Dictionary<Transform, int> tfMap)
        {
            return JObject.FromObject(new
            {
                spriteObjects = spriteObjects.Where(item=>item!=null).Select(item => tfMap[item.transform]).ToArray(),
            });
        }

        public override void DeserializeFromJObject(JObject jObject, Dictionary<int, Transform> idTransformMap)
        {
            //Sprite resolvers is not determined when the template is loaded, so we will not use idTransformMap here.
            resolverIds = jObject["spriteObjects"].ToObject<List<int>>();
        }

        public void UpdateLabel(string label)
        {
            if (label?.Length == 0) return;

            if (spriteObjects != null)
            {
                spriteObjects.Where(item => item != null).ToList()
                    .ForEach(resolver => resolver.SetCategoryAndLabel(resolver.GetCategory(), label));
            }

            if (isRuntimeMode)
            {
                m_spriteResolverRuntimes.Where(item=>item!=null)
                    .ToList().ForEach(resolver=>resolver.SetLabel(label));
            }
        }

        public void UpdateRuntimeResolvers(Dictionary<int, Transform> idTransformMap)
        {
            m_spriteResolverRuntimes =
                resolverIds.Select(id =>
                {
                    if (idTransformMap.ContainsKey(id))
                        return idTransformMap[id].GetComponent<MYTYSpriteResolverRuntime>();
                    return null;
                }).ToList();
            isRuntimeMode = true;
        }
    }
}

