// using UnityEditor;
// using UnityEngine;
//
// public static class MotionTemplateHelper
// {
//     public static void SetupMotionTemplate(this MotionTemplate motionTemplate)
//     {
//         motionTemplate.Clear();
//         var transform = motionTemplate.transform;
//         var childCount = transform.childCount;
//         for (int i = 0; i < childCount; i++)
//         {
//             var child = transform.GetChild(i).gameObject;
//             var categoryName = child.name;
//             
//             for (int j = 0; j < child.transform.childCount; j++)
//             {
//                 var model = child.transform.GetChild(j).GetComponent<RiggingModel>();
//                 if (model == null) continue;
//                 motionTemplate.AddRiggingModel(categoryName,model);
//             }
//         }
//
// #if UNITY_EDITOR
//         if (!Application.isEditor) return;
//
//         var so = new SerializedObject(motionTemplate);
//         var prop = so.FindProperty("m_motionCategories");
//         var categories = motionTemplate.GetCategoryList();
//
//         prop.arraySize = categories.Count;
//         for (var i = 0; i < categories.Count; i++)
//         {
//             var riggingModels = motionTemplate.GetRiggingModelsInCategory(categories[i]);
//             var riggingModelsProp = prop.GetArrayElementAtIndex(i).FindPropertyRelative("riggingModels");
//             prop.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue = categories[i];
//             riggingModelsProp.arraySize = riggingModels.Count;
//             for (var j = 0; j < riggingModels.Count; j++)
//             {
//                 riggingModelsProp.GetArrayElementAtIndex(j).objectReferenceValue = riggingModels[j];
//             }
//         }
//         so.ApplyModifiedProperties();
// #endif
//         
//     }
// }