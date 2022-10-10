using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MYTYKit
{
    [ExecuteInEditMode]
    public class CopyRig : MonoBehaviour
    {
        public GameObject source;
        public GameObject target;

        public void Copy()
        {
            RecursiveCopy(source, target);
        }

        private void CopyComponent(GameObject src, GameObject dst)
        {
            var components = src.GetComponents<Component>();
            foreach (var com in components)
            {
                Type t = com.GetType();
                if (t == typeof(Transform))
                {
                    continue;
                }

                Debug.Log(com.name + " " + t);
                Component copied = dst.AddComponent(t);

                var fields = GetAllFields(t);
                foreach (var field in fields)
                {
                    Debug.Log(field.Name + " " + field.GetValue(com));
                    if (field.IsStatic) continue;
                    field.SetValue(copied, field.GetValue(com));
                }

                var props = t.GetProperties();
                foreach (var prop in props)
                {

                    if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;

                    prop.SetValue(copied, prop.GetValue(com, null), null);
                }
            }
        }

        public static IEnumerable<FieldInfo> GetAllFields(System.Type t)
        {
            if (t == null)
            {
                return Enumerable.Empty<FieldInfo>();
            }

            BindingFlags flags = BindingFlags.Public;
            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        }

        private void RecursiveCopy(GameObject src, GameObject dst)
        {
            int srcChildCount = src.transform.childCount;
            int dstChildCount = dst.transform.childCount;

            if (srcChildCount != dstChildCount)
            {
                Debug.LogWarning("Childcounts do not match -> src:" + src.name + " dst:" + dst.name);
                return;
            }

            for (int i = 0; i < srcChildCount; i++)
            {
                var srcChild = src.transform.GetChild(i);
                var dstChild = dst.transform.GetChild(i);

                if (srcChild.name != dstChild.name)
                {
                    Debug.LogWarning("Names do not match -> src:" + src.name + " dst:" + dst.name);
                    return;
                }
            }

            CopyComponent(src, dst);
            for (int i = 0; i < srcChildCount; i++)
            {
                var srcChild = src.transform.GetChild(i);
                var dstChild = dst.transform.GetChild(i);
                RecursiveCopy(srcChild.gameObject, dstChild.gameObject);
            }
        }
    }
}