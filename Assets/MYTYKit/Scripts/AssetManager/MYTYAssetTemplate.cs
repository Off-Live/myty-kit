using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D.Animation;

#if UNITY_EDITOR
public class MYTYAssetTemplate : MonoBehaviour
{

    public AvatarSelector m_avatarSelector;

    private ExecuteCmdTool cmdTool;

    public MYTYAssetTemplate()
    {
        cmdTool = new ExecuteCmdTool();
        PrefabUtility.prefabInstanceUpdated = (GameObject go) =>
        {
            if (go.transform.parent != null)
            {
                var parentGO = go.transform.parent.gameObject;
                var mytyAT = parentGO.GetComponent<MYTYAssetTemplate>();
                if (mytyAT == null) return;
                mytyAT.OnPrefabUpdated(go);
            }
        };
    }

    public void OnPrefabUpdated(GameObject go)
    {
        Debug.Log("on prefab updated " + go.name + " "+go);
        var variantPrefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
        if (variantPrefab == null) return;
        var origPrefab = PrefabUtility.GetCorrespondingObjectFromSource(variantPrefab);
        var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(origPrefab);
        if (!path.StartsWith(MYTYUtil.AssetPath)) return;
        SpriteLibraryFactory.CreateLibrary(go, GetSpriteLibraryAssetPath(path));
        UpdateAvatarSelector();
        cmdTool.ExecuteLayerTool(path, go);
        
    }
    

    public void ApplyAssetImported(string[] importedAssets, string[] movedFromAssets)
    {
        foreach (var path in importedAssets)
        {
            if (!path.StartsWith(MYTYUtil.AssetPath)) continue;
            if (!path.EndsWith(".psb")) continue;
            var isAleadyInstantiated = false;
            var spriteLibraryAssetPath = GetSpriteLibraryAssetPath(path);
            var asset = AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(spriteLibraryAssetPath);

            if (asset != null)
            {
                Debug.Log("Delete Asset! " + spriteLibraryAssetPath);
                AssetDatabase.DeleteAsset(spriteLibraryAssetPath);
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                var childGO = transform.GetChild(i).gameObject;
                var variantPrefab = PrefabUtility.GetCorrespondingObjectFromSource(childGO);
                if (variantPrefab == null) continue;
                var origPrefab = PrefabUtility.GetCorrespondingObjectFromSource(variantPrefab);
                var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(origPrefab);
                assetPath = assetPath.Trim();
                if (path.CompareTo(assetPath) == 0)
                {
                    isAleadyInstantiated = true;
                    break;
                }
            }
            if (!isAleadyInstantiated)
            {
                var newGO = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), transform) as GameObject;
                PrefabUtility.InstantiatePrefab(PrefabUtility.SaveAsPrefabAsset(newGO, GetVariantAssetPath(path)),transform);
                GameObject.DestroyImmediate(newGO);
            
            }
  
        }
    }

    public void DeleteAsset(string deleteAssetPath)
    {

        if (!deleteAssetPath.EndsWith(".psb")) return;
        var childrenList = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            childrenList.Add(transform.GetChild(i));
        }
        foreach (var child in childrenList)
        {
            var childGO = child.gameObject;
            var variantPrefab = PrefabUtility.GetCorrespondingObjectFromSource(childGO);
            if (variantPrefab == null) continue;
            var origPrefab = PrefabUtility.GetCorrespondingObjectFromSource(variantPrefab);
            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(origPrefab);
            assetPath = assetPath.Trim();
            if (deleteAssetPath.CompareTo(assetPath) == 0)
            {
                GameObject.DestroyImmediate(childGO);
            }
        }

        var spriteLibraryAssetPath = GetSpriteLibraryAssetPath(deleteAssetPath);
        var asset = AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(spriteLibraryAssetPath);
        if (asset != null)
        {
            AssetDatabase.DeleteAsset(spriteLibraryAssetPath);
        }

        AssetDatabase.DeleteAsset(GetVariantAssetPath(deleteAssetPath));
    }

    public void UpdateAvatarSelector()
    {
        var old_id = 0;
        if (m_avatarSelector != null)
        {
            old_id = m_avatarSelector.id;
            GameObject.DestroyImmediate(m_avatarSelector.gameObject);
        }

        var go = new GameObject("AvatarSelector");
        m_avatarSelector = go.AddComponent<AvatarSelector>();

        var so = new SerializedObject(m_avatarSelector);

        var templatesProp = so.FindProperty("templates");
        templatesProp.arraySize = gameObject.transform.childCount;

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            var templateItemProp = templatesProp.GetArrayElementAtIndex(i);

            var childGO = gameObject.transform.GetChild(i).gameObject;
            var variantPrefab = PrefabUtility.GetCorrespondingObjectFromSource(childGO);
            var origPrefab = PrefabUtility.GetCorrespondingObjectFromSource(variantPrefab);
            var psbPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(origPrefab);

            templateItemProp.FindPropertyRelative("PSBPath").stringValue = psbPath;
            templateItemProp.FindPropertyRelative("instance").objectReferenceValue = childGO;
            templateItemProp.FindPropertyRelative("spriteLibrary").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(GetSpriteLibraryAssetPath(psbPath));

            var topLayerCount = childGO.transform.childCount;
            if (topLayerCount > 1)
            {
                var boneGO = childGO.transform.GetChild(topLayerCount - 1).gameObject;
                if (boneGO.name.ToUpper().StartsWith("BONE"))
                {
                    templateItemProp.FindPropertyRelative("boneRootObj").objectReferenceValue = boneGO;
                }
            }

        }


        so.FindProperty("mytyAssetStorage").objectReferenceValue = AssetDatabase.LoadAssetAtPath<MYTYAssetScriptableObject>(
            MYTYUtil.AssetPath + "/MYTYAssetData.asset");
        so.ApplyModifiedProperties();
        m_avatarSelector.id = old_id;
        m_avatarSelector.Configure();

    }

    private string GetSpriteLibraryAssetPath(string psbPath)
    {
        return psbPath.Substring(0, psbPath.Length - 4) + ".asset";
    }

    private string GetVariantAssetPath(string psbPath)
    {
        return psbPath.Substring(0, psbPath.Length - 4) + ".prefab";
    }

}

#endif