
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MTMapperHelper
{
    public static void SetupWithDescendants(this MotionTemplateMapper mapper)
    {
        mapper.Clear();
        Stack<GameObject> history = new();
        history.Push(mapper.gameObject);
        List<MTItem> items = new();
        while (history.Count > 0)
        {
            var currentGo = history.Pop();
            var childCount = currentGo.transform.childCount;
            if (childCount == 0)
            {
                var mt = currentGo.GetComponent<MotionTemplate>();
                if (mt != null)
                {
                    mapper.SetTemplate(currentGo.name, mt);
                    items.Add(new MTItem()
                    {
                        name = currentGo.name,
                        template = mt
                    });
                }
            }
            else
            {
                for (int i = 0; i < childCount; i++)
                {
                    history.Push(currentGo.transform.GetChild(i).gameObject);
                }
            }
        }
        
        
#if UNITY_EDITOR
        if (!Application.isEditor) return;

        var so = new SerializedObject(mapper);
        var prop = so.FindProperty("templates");
        prop.arraySize = items.Count;
        
        for (var i = 0; i < prop.arraySize; i++)
        {
            var itemProp = prop.GetArrayElementAtIndex(i).FindPropertyRelative("template").objectReferenceValue = items[i].template;
            prop.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue = items[i].name;
            
        }
        so.ApplyModifiedProperties();
#endif

    }
}
