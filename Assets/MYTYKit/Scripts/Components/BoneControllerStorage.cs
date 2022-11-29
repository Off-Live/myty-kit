

using System;
using System.Collections.Generic;
using MYTYKit.Controllers;
using UnityEditor;
using UnityEngine;

namespace MYTYKit.Components
{
    public class BoneControllerStorage : MonoBehaviour
    {
        internal class ControllerState
        {
            public int templateIndex;
            public List<String> rigTargetNames;
            public List<RiggingEntity> rigValues;
        }

        Dictionary<BoneController, ControllerState> controllerStateMap ;

        public static void Save()
        {
#if UNITY_EDITOR
            var controllers = FindObjectsOfType<BoneController>();
            var selector = FindObjectOfType<AvatarSelector>();
            var storage = FindObjectOfType<BoneControllerStorage>();
            if (selector == null) return;
            if (storage == null)
            { 
                var newGo = new GameObject("RiggingStorage");
                storage = newGo.AddComponent<BoneControllerStorage>();
            }
            
            storage.controllerStateMap = new();
            foreach (var boneController in controllers)
            {
                var state = new ControllerState();
                state.rigValues = new List<RiggingEntity>(boneController.orgRig);
                state.rigTargetNames = new();
                foreach (var bone in boneController.rigTarget)
                {
                    if (bone == null)
                    {
                        state.rigTargetNames.Add("");
                        
                        continue;
                    }
                    var rootObj = PrefabUtility.GetNearestPrefabInstanceRoot(bone);
                    var index = -1;
                    for (int i = 0; i < selector.templates.Count; i++)
                    {
                        if (selector.templates[i].instance == rootObj)
                        {
                            index = i;
                            break;
                        }
                    }
                    state.rigTargetNames.Add(bone.name);
                    state.templateIndex = index;
                }
                storage.controllerStateMap[boneController] = state;
            }
#endif
        }

        public static void Restore()
        {
#if UNITY_EDITOR
            var selector = FindObjectOfType<AvatarSelector>();
            var storage = FindObjectOfType<BoneControllerStorage>();
            if (storage == null)
            {
                Debug.LogWarning("No rigging storage to restore");
                return;
            }
            if (selector == null) return;

            foreach (var pair in storage.controllerStateMap)
            {
                var so = new SerializedObject(pair.Key);
                var state = pair.Value;
                
                var targetProp = so.FindProperty("rigTarget");
                var valueProp = so.FindProperty("orgRig");
                targetProp.arraySize = state.rigTargetNames.Count;
                valueProp.arraySize = state.rigTargetNames.Count;
                for (int i = 0; i < state.rigTargetNames.Count; i++)
                {
                    if (state.rigTargetNames[i].Length==0)
                    {
                        continue;
                    }
                    var bone = FindObjectWithName(selector.templates[state.templateIndex].boneRootObj,
                        state.rigTargetNames[i]);
                    targetProp.GetArrayElementAtIndex(i).objectReferenceValue = bone;
                    valueProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value =
                        state.rigValues[i].position;
                    valueProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value =
                        state.rigValues[i].scale;
                    valueProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue =
                        state.rigValues[i].rotation;
                }

                so.ApplyModifiedProperties();
            }
#endif
        }

        static GameObject FindObjectWithName(GameObject root, string name)
        {
            var dfs = new Stack<GameObject>();
            dfs.Push(root);
            while (dfs.Count > 0)
            {
                var curr = dfs.Pop();
                if (curr.name == name) return curr;

                for (int i = 0; i < curr.transform.childCount; i++)
                {
                    dfs.Push(curr.transform.GetChild(i).gameObject);
                }
            }
            return null;
        }

    }
}