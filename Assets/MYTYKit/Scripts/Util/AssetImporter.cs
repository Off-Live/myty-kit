using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D.Animation;
using System.Collections;

public class AssetImporter : MonoBehaviour
{
    private AssetBundle m_assetBundle;
    private Dictionary<GameObject, GameObject> m_goMap;

    public static void InitAvatarAsset()
    {
        AssetBundle.UnloadAllAssetBundles(true);
    }

    public IEnumerator LoadMYTYAvatarAsync(GameObject motionTemplateGO, AssetBundle bundle, GameObject root, bool spriteOnly = false)
    {
        m_assetBundle = bundle;
        if (m_assetBundle == null)
        {
            Debug.Log("Failed to load bundle");
            yield break;
        }

        m_goMap = new();

        var request = m_assetBundle.LoadAssetAsync<MYTYAssetScriptableObject>("mytyassetdata.asset");
        yield return request;
        var mytyasset = request.asset as MYTYAssetScriptableObject;

        var avatarSelectorGO = new GameObject("AvatarSelector");
        avatarSelectorGO.transform.parent = root.transform;
        var selector = avatarSelectorGO.AddComponent<AvatarSelector>();

        selector.mytyAssetStorage = mytyasset;
        selector.templates = new();


        foreach (var templateInfo in mytyasset.templateInfos)
        {
            var template = new AvatarTemplate();
            template.PSBPath = templateInfo.template;
            var instanceReq = m_assetBundle.LoadAssetAsync<GameObject>(templateInfo.instance);
            yield return instanceReq;
            var prefabInstance = instanceReq.asset as GameObject;
            var psb = Instantiate(prefabInstance);
            psb.name = prefabInstance.name;
            BuildMap(prefabInstance, psb);
            template.instance = psb;
            psb.transform.parent = root.transform;
            var splReq = m_assetBundle.LoadAssetAsync<SpriteLibraryAsset>(templateInfo.spriteLibrary);
            yield return splReq;

            template.spriteLibrary = splReq.asset as SpriteLibraryAsset;

            int boneIdx = template.instance.transform.childCount - 1;
            if (boneIdx >= 0)
            {
                var boneCandidate = template.instance.transform.GetChild(boneIdx).gameObject;
                if (boneCandidate.name.ToUpper().StartsWith("BONE")) template.boneRootObj = boneCandidate;
            }

            selector.templates.Add(template);

        }


        selector.id = 0;
        selector.Configure();
        if (spriteOnly) yield break;

        request = m_assetBundle.LoadAssetAsync<DefaultLayoutAsset>("DefaultLayoutAsset.asset");
        yield return request;
        var layout = request.asset as DefaultLayoutAsset;
        SetupLayout(root, selector, layout);

        yield return SetupAvatarAsync(root, avatarSelectorGO, motionTemplateGO);

    }

    public GameObject LoadMYTYAvatar(GameObject motionTemplateGO, AssetBundle bundle, string loadedName = "avatar", bool spriteOnly = false)
    {

        m_assetBundle = bundle;
        if (m_assetBundle == null)
        {
            Debug.Log("Failed to load bundle");
            return null;
        }

        m_goMap = new();

        var avatarSelectorGO = LoadAvatarSelector();
        var retGO = new GameObject(loadedName);
        var selector = avatarSelectorGO.GetComponent<AvatarSelector>();
        avatarSelectorGO.transform.parent = retGO.transform;
        foreach (var template in selector.templates)
        {
            var psb = GameObject.Instantiate(template.instance);
            psb.name = template.instance.name;
            BuildMap(template.instance, psb);
            template.instance = psb;
            if (template.boneRootObj != null) template.boneRootObj = m_goMap[template.boneRootObj];
            psb.transform.parent = retGO.transform;
        }

        if (spriteOnly) return retGO;
        var layout = m_assetBundle.LoadAsset<DefaultLayoutAsset>("DefaultLayoutAsset.asset");

        SetupLayout(retGO, selector, layout);
        SetupAvatar(retGO, avatarSelectorGO, motionTemplateGO);

        return retGO;
    }

    private IEnumerator SetupAvatarAsync(GameObject root, GameObject avatarSelectorGO, GameObject motionTemplateGO)
    {
        var motionAdaptersGO = new GameObject("MotionAdapter");
        var selector = avatarSelectorGO.GetComponent<AvatarSelector>();
        motionAdaptersGO.transform.parent = root.transform;

        foreach (var rootConPrefabPath in selector.mytyAssetStorage.rootControllers)
        {

            var request = m_assetBundle.LoadAssetAsync<GameObject>(rootConPrefabPath);
            yield return request;
            var rootConPrefab = request.asset as GameObject;
            var rootConGO = GameObject.Instantiate(rootConPrefab);
            rootConGO.name = rootConPrefab.name;
            rootConGO.transform.parent = root.transform;
            RestoreController(rootConGO);
            BuildMap(rootConPrefab, rootConGO);
        }

        foreach (var motionAdapterPrefabPath in selector.mytyAssetStorage.motionAdapters)
        {
            //yield return new WaitForSeconds(0.1f);
            var request = m_assetBundle.LoadAssetAsync<GameObject>(motionAdapterPrefabPath);
            yield return request;
            var motionAdapterPrefab = request.asset as GameObject;
            var motionAdapterGO = GameObject.Instantiate(motionAdapterPrefab);
            motionAdapterGO.name = motionAdapterPrefab.name;
            motionAdapterGO.transform.parent = motionAdaptersGO.transform;

            SetupMotionAdapter(root, motionAdapterGO, motionTemplateGO);
        }
    }

    private void SetupAvatar(GameObject root, GameObject avatarSelectorGO, GameObject motionTemplateGO)
    {
        var motionAdaptersGO = new GameObject("MotionAdapter");
        var selector = avatarSelectorGO.GetComponent<AvatarSelector>();
        motionAdaptersGO.transform.parent = root.transform;

        foreach (var rootConPrefabPath in selector.mytyAssetStorage.rootControllers)
        {
            var rootConPrefab = m_assetBundle.LoadAsset<GameObject>(rootConPrefabPath);
            var rootConGO = GameObject.Instantiate(rootConPrefab);
            rootConGO.name = rootConPrefab.name;
            rootConGO.transform.parent = root.transform;
            RestoreController(rootConGO);
            BuildMap(rootConPrefab, rootConGO);
        }

        foreach (var motionAdapterPrefabPath in selector.mytyAssetStorage.motionAdapters)
        {
            var motionAdapterPrefab = m_assetBundle.LoadAsset<GameObject>(motionAdapterPrefabPath);
            var motionAdapterGO = GameObject.Instantiate(motionAdapterPrefab);
            motionAdapterGO.name = motionAdapterPrefab.name;
            motionAdapterGO.transform.parent = motionAdaptersGO.transform;

            SetupMotionAdapter(root, motionAdapterGO, motionTemplateGO);
        }
    }

    private void SetupMotionAdapter(GameObject root, GameObject motionAdapterGO, GameObject motionTemplateGO)
    {
        var nativeAdapter = motionAdapterGO.GetComponent<NativeAdapter>();
        if (nativeAdapter == null) return;

        foreach (var field in nativeAdapter.GetType().GetFields())
        {
            if (field.FieldType.IsSubclassOf(typeof(MYTYController)) || field.FieldType.IsEquivalentTo(typeof(MYTYController)))
            {
                var prefab = (field.GetValue(nativeAdapter) as MYTYController).gameObject;
                var conGO = m_goMap[prefab];
                field.SetValue(nativeAdapter, conGO.GetComponent<MYTYController>());
            }
            else if (field.FieldType.IsSubclassOf(typeof(RiggingModel)))
            {
                var prefab = (field.GetValue(nativeAdapter) as RiggingModel).gameObject;
                if (motionTemplateGO == null)
                {

                    if (!m_goMap.ContainsKey(prefab))
                    {
                        var rootPrefab = prefab;
                        while (rootPrefab.transform.parent != null)
                        {
                            rootPrefab = rootPrefab.transform.parent.gameObject;
                        }
                        var motionTemplateCloneGO = GameObject.Instantiate(rootPrefab);
                        motionTemplateCloneGO.name = rootPrefab.name;
                        motionTemplateCloneGO.transform.parent = root.transform;
                        BuildMap(rootPrefab, motionTemplateCloneGO);
                    }

                }
                else
                {
                    if (!m_goMap.ContainsKey(prefab))
                    {
                        var rootPrefab = prefab;
                        while (rootPrefab.transform.parent != null)
                        {
                            rootPrefab = rootPrefab.transform.parent.gameObject;
                        }
                        BuildMap(rootPrefab, motionTemplateGO);
                    }

                }
                field.SetValue(nativeAdapter, m_goMap[prefab].GetComponent<RiggingModel>());
            }

        }
    }

    private GameObject LoadAvatarSelector()
    {
        var prefabs = m_assetBundle.LoadAllAssets<GameObject>();
        GameObject selectorGO = null;
        foreach (var prefab in prefabs)
        {
            if (prefab.GetComponent<AvatarSelector>() != null)
            {
                selectorGO = GameObject.Instantiate(prefab);
                selectorGO.name = "AvatarSelector";
            }
        }

        if (selectorGO == null)
        {
            Debug.Log("no prefab");
        }


        return selectorGO;
    }


    private void BuildMap(GameObject prefabNode, GameObject goNode)
    {
        m_goMap[prefabNode] = goNode;
        for (int i = 0; i < prefabNode.transform.childCount; i++)
        {
            BuildMap(prefabNode.transform.GetChild(i).gameObject, goNode.transform.GetChild(i).gameObject);
        }
    }

    private void RestoreController(GameObject node)
    {
        var controller = node.GetComponent<MYTYController>();
        if (controller != null)
        {
            controller.PostprocessAfterLoad(m_goMap);
        }

        for (int i = 0; i < node.transform.childCount; i++)
        {
            RestoreController(node.transform.GetChild(i).gameObject);
        }
    }

    void SetupLayout(GameObject parent, AvatarSelector selector, DefaultLayoutAsset layoutAsset)
    {
        var cameraGO = new GameObject("RenderCam");
        cameraGO.transform.parent = parent.transform;

        var camera = cameraGO.AddComponent<Camera>();
        camera.CopyFrom(layoutAsset.camera);
        cameraGO.transform.localPosition = layoutAsset.camera.transform.position;
        cameraGO.transform.localScale = layoutAsset.camera.transform.localScale;
        cameraGO.transform.localRotation = layoutAsset.camera.transform.rotation;

        for (int i = 0; i < selector.templates.Count; i++)
        {
            selector.templates[i].instance.transform.localPosition = layoutAsset.templateTransforms[i].position;
            selector.templates[i].instance.transform.localScale = layoutAsset.templateTransforms[i].scale;
            selector.templates[i].instance.transform.localRotation = layoutAsset.templateTransforms[i].rotation;
        }
    }

}
