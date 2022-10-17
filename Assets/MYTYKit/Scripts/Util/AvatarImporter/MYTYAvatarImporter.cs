using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using System.Collections;

using MYTYKit.Components;
using MYTYKit.MotionAdapters;
using MYTYKit.Controllers;

namespace MYTYKit.AvatarImporter
{
    public class MYTYAvatarImporter : MonoBehaviour, IMYTYAvatarImporter
    {
        protected AssetBundle assetBundle;
        protected Dictionary<GameObject, GameObject> goMap;

        public static void InitAvatarAsset()
        {
            AssetBundle.UnloadAllAssetBundles(true);
        }

        public virtual IEnumerator LoadMYTYAvatarAsync(GameObject motionTemplateGO, AssetBundle bundle, GameObject root,
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

            yield return SetupAvatarAsync(root, avatarSelectorGO, motionTemplateGO);

        }

        protected IEnumerator SetupAvatarSelector(GameObject root, GameObject avatarSelectorGO)
        {
            var request = assetBundle.LoadAssetAsync<MYTYAssetScriptableObject>("mytyassetdata.asset");
            yield return request;
            var mytyasset = request.asset as MYTYAssetScriptableObject;
            var selector = avatarSelectorGO.AddComponent<AvatarSelector>();

            selector.mytyAssetStorage = mytyasset;
            selector.templates = new();

            foreach (var templateInfo in mytyasset.templateInfos)
            {
                var template = new AvatarTemplate();
                template.PSBPath = templateInfo.template;
                var instanceReq = assetBundle.LoadAssetAsync<GameObject>(templateInfo.instance);
                yield return instanceReq;
                var prefabInstance = instanceReq.asset as GameObject;
                var psb = Instantiate(prefabInstance);
                psb.name = prefabInstance.name;
                BuildMap(prefabInstance, psb);
                template.instance = psb;
                psb.transform.parent = root.transform;
                var splReq = assetBundle.LoadAssetAsync<SpriteLibraryAsset>(templateInfo.spriteLibrary);
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
        }

        IEnumerator SetupAvatarAsync(GameObject root, GameObject avatarSelectorGO, GameObject motionTemplateGO)
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
                if (field.FieldType.IsSubclassOf(typeof(MYTYController)) ||
                    field.FieldType.IsEquivalentTo(typeof(MYTYController)))
                {
                    var prefab = (field.GetValue(nativeAdapter) as MYTYController).gameObject;
                    var conGO = goMap[prefab];
                    field.SetValue(nativeAdapter, conGO.GetComponent<MYTYController>());
                }
                else if (field.FieldType.IsSubclassOf(typeof(RiggingModel)))
                {
                    var prefab = (field.GetValue(nativeAdapter) as RiggingModel).gameObject;
                    if (motionTemplateGO == null)
                    {

                        if (!goMap.ContainsKey(prefab))
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
                        if (!goMap.ContainsKey(prefab))
                        {
                            var rootPrefab = prefab;
                            while (rootPrefab.transform.parent != null)
                            {
                                rootPrefab = rootPrefab.transform.parent.gameObject;
                            }

                            BuildMap(rootPrefab, motionTemplateGO);
                        }

                    }

                    field.SetValue(nativeAdapter, goMap[prefab].GetComponent<RiggingModel>());
                }

            }
        }


        protected void BuildMap(GameObject prefabNode, GameObject goNode)
        {
            goMap[prefabNode] = goNode;
            for (int i = 0; i < prefabNode.transform.childCount; i++)
            {
                BuildMap(prefabNode.transform.GetChild(i).gameObject, goNode.transform.GetChild(i).gameObject);
            }
        }

        protected void RestoreController(GameObject node)
        {
            var controller = node.GetComponent<MYTYController>();
            if (controller != null)
            {
                controller.PostprocessAfterLoad(goMap);
            }

            for (int i = 0; i < node.transform.childCount; i++)
            {
                RestoreController(node.transform.GetChild(i).gameObject);
            }
        }

        protected void SetupLayout(GameObject parent, AvatarSelector selector, DefaultLayoutAsset layoutAsset)
        {
            if (layoutAsset == null) return;
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

        public string GetKitVersionInfo()
        {
            if (assetBundle == null) return "";

            var textAsset = assetBundle.LoadAsset<TextAsset>("VERSION.txt");
            if (textAsset == null) return "";

            return textAsset.text;
        }

        public string GetEditorVersionInfo()
        {
            if (assetBundle == null) return "";

            var textAsset = assetBundle.LoadAsset<TextAsset>("EditorInfo.txt");
            if (textAsset == null) return "";

            return textAsset.text;
        }

    }
}
