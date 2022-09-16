

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MYTYAvatarImporterV2 : MYTYAvatarImporter
{
    AssetBundle m_assetBundle;
    Dictionary<GameObject, GameObject> m_goMap;

    public IEnumerator LoadMYTYAvatarAsync(GameObject motionSourceGo, AssetBundle bundle, GameObject root,
        bool spriteOnly = false)
    {
        assetBundle = bundle;
        if (assetBundle == null)
        {
            Debug.Log("Failed to load bundle");
            yield break;
        }

        goMap = new();
        var avatarSelectorGO = new GameObject("AvatarSelector");
        avatarSelectorGO.transform.parent = root.transform;
        yield return SetupAvatarSelector(root, avatarSelectorGO);
        var selector = avatarSelectorGO.GetComponent<AvatarSelector>();
        var request = assetBundle.LoadAssetAsync<DefaultLayoutAsset>("DefaultLayoutAsset.asset");
        yield return request;
        var layout = request.asset as DefaultLayoutAsset;
        SetupLayout(root, selector, layout);

        if (spriteOnly) yield break;


        yield return SetupAvatarAsync(root, avatarSelectorGO, motionSourceGo);
    }

    IEnumerator SetupAvatarAsync(GameObject root, GameObject avatarSelectorGO, GameObject motionSourceGo)
    {
        var motionAdaptersGO = new GameObject("MotionAdapter");
        var selector = avatarSelectorGO.GetComponent<AvatarSelector>();
        motionAdaptersGO.transform.parent = root.transform;

        foreach (var rootConPrefabPath in selector.mytyAssetStorage.rootControllers)
        {

            var request = assetBundle.LoadAssetAsync<GameObject>(rootConPrefabPath);
            yield return request;
            var rootConPrefab = request.asset as GameObject;
            var rootConGO = GameObject.Instantiate(rootConPrefab);
            rootConGO.name = rootConPrefab.name;
            rootConGO.transform.parent = root.transform;
            RestoreController(rootConGO);
            BuildMap(rootConPrefab, rootConGO);
        }

        
        var mtRequest = assetBundle.LoadAssetAsync<GameObject>("MotionTemplate");
        yield return mtRequest;
        var mtPrefabGo = mtRequest.asset as GameObject;
        var motionTemplateGO = GameObject.Instantiate(mtPrefabGo);
        motionTemplateGO.name = "MotionTemplate";
        motionTemplateGO.transform.parent = root.transform;
        BuildMap(mtPrefabGo, motionTemplateGO);

        var motionSource = motionSourceGo.GetComponent<MotionSource>();
        motionSource.motionTemplateMapperList.Add(motionTemplateGO.GetComponent<MotionTemplateMapper>());
        
        foreach (var motionAdapterPrefabPath in selector.mytyAssetStorage.motionAdapters)
        {
            //yield return new WaitForSeconds(0.1f);
            var request = assetBundle.LoadAssetAsync<GameObject>(motionAdapterPrefabPath);
            yield return request;
            var motionAdapterPrefab = request.asset as GameObject;
            var motionAdapterGO = GameObject.Instantiate(motionAdapterPrefab);
            motionAdapterGO.name = motionAdapterPrefab.name;
            motionAdapterGO.transform.parent = motionAdaptersGO.transform;

            SetupMotionAdapter(root, motionAdapterGO, motionTemplateGO);
        }
    }


    void SetupMotionAdapter(GameObject root, GameObject motionAdapterGO, GameObject motionTemplateGO)
    {
        var nativeAdapter = motionAdapterGO.GetComponent<NativeAdapter>();
        if (nativeAdapter == null) return;

        foreach (var field in nativeAdapter.GetType().GetFields())
        {
            if (field.FieldType.IsSubclassOf(typeof(MYTYController)) || field.FieldType.IsEquivalentTo(typeof(MYTYController)))
            {
                var prefab = (field.GetValue(nativeAdapter) as MYTYController).gameObject;
                var conGO = goMap[prefab];
                field.SetValue(nativeAdapter, conGO.GetComponent<MYTYController>());
            }
            else if (field.FieldType.IsSubclassOf(typeof(MotionTemplate)))
            {
                var prefab = (field.GetValue(nativeAdapter) as MotionTemplate).gameObject;
                field.SetValue(nativeAdapter, goMap[prefab].GetComponent<MotionTemplate>());
            }

        }
    }
    
}
