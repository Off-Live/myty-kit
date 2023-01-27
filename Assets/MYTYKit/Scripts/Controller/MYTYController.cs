using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using MYTYKit.Components;
using UnityEngine.U2D.Animation;

namespace MYTYKit.Controllers
{
    [Serializable]
    public class RiggingEntity
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

    }

    public abstract class MYTYController : MonoBehaviour
    {
        public abstract void PrepareToSave();
        public abstract void PostprocessAfterLoad(Dictionary<GameObject, GameObject> objMap);
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
    }

    public abstract class MSRSpriteController : MYTYController
    {
        public List<MYTYSpriteResolver> spriteObjects;
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
    }
}

