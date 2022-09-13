using UnityEditor;
using UnityEngine;

public static class MotionSourceHelper
{
    public static void SetupMotionTemplate(this MotionSource motionSource)
    {
        motionSource.Clear();
        var transform = motionSource.transform;
        var childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            var categoryName = child.name;
            
            for (int j = 0; j < child.transform.childCount; j++)
            {
                var model = child.transform.GetChild(j).GetComponent<RiggingModel>();
                if (model == null) continue;
                motionSource.AddRiggingModel(categoryName,model);
            }
        }

#if UNITY_EDITOR
        if (!Application.isEditor) return;

        var so = new SerializedObject(motionSource);
        var prop = so.FindProperty("motionCategories");
        var categories = motionSource.GetCategoryList();

        prop.arraySize = categories.Count;
        for (var i = 0; i < categories.Count; i++)
        {
            var riggingModels = motionSource.GetRiggingModelsInCategory(categories[i]);
            var riggingModelsProp = prop.GetArrayElementAtIndex(i).FindPropertyRelative("riggingModels");
            prop.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue = categories[i];
            riggingModelsProp.arraySize = riggingModels.Count;
            for (var j = 0; j < riggingModels.Count; j++)
            {
                riggingModelsProp.GetArrayElementAtIndex(j).objectReferenceValue = riggingModels[j];
            }
        }
        so.ApplyModifiedProperties();
#endif
        
    }

    public static void SetupBridge(this MotionSource source)
    {
        
#if UNITY_EDITOR
        if (!Application.isEditor) return;

        var so = new SerializedObject(source);
        var bridgeProp = so.FindProperty("templateBridgeMap");
        var mt = source.motionTemplateMapper;

        var names = mt.GetNames();
        bridgeProp.arraySize = names.Count;
        for (int i = 0; i < bridgeProp.arraySize; i++)
        {
            bridgeProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue = names[i];
            
        }
        
        
        so.ApplyModifiedProperties();
#endif
    }
}