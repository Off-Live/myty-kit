using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;

namespace MYTYKit.Controllers
{
    public class RootPositioningController : MYTYController, IVec3Input, IComponentWiseInput
    {
        public GameObject targetObject;
        public Vector3 displacement;

        Vector3 m_initPos;

        public override void PostprocessAfterLoad(Dictionary<GameObject, GameObject> objMap)
        {
            targetObject = objMap[targetObject];
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                var so = new SerializedObject(this);
                so.FindProperty("targetObject").objectReferenceValue = targetObject;
                so.ApplyModifiedProperties();
            }
#endif
        }

        public override void PrepareToSave()
        {
#if UNITY_EDITOR
            targetObject = PrefabUtility.GetCorrespondingObjectFromSource(targetObject);
#endif
        }

        void Start()
        {
            if (targetObject == null) return;
            m_initPos = targetObject.transform.position;

        }

        private void LateUpdate()
        {
            if (targetObject == null) return;

            targetObject.transform.position = m_initPos + displacement;
        }

        public void SetInput(Vector3 val)
        {
            displacement = val;
        }
        public void SetComponent(float value, int componentIdx)
        {
            displacement[componentIdx] = value;
        }

        public override JObject SerializeToJObject(Dictionary<Transform, int> tfMap)
        {
            return JObject.FromObject(new
            {
                name,
                type = GetType().Name,
                targetObject = tfMap[targetObject.transform],
                displacement = new
                {
                    displacement.x,
                    displacement.y,
                    displacement.z
                }
            });
        }

        public override void DeserializeFromJObject(JObject jObject, Dictionary<int, Transform> idTransformMap)
        {
            name = (string)jObject["name"];
            targetObject = idTransformMap[(int)jObject["targetObject"]].gameObject;
            displacement = jObject["displacement"].ToObject<Vector3>();
        }
    }
}