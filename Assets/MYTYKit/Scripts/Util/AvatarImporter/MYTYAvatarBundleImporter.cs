using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MYTYKit.Components;
using MYTYKit.MotionAdapters;
using MYTYKit.Controllers;
using UnityEngine.U2D.Animation;

namespace MYTYKit.AvatarImporter
{
    public class MYTYAvatarBundleImporter : MonoBehaviour
    {
       
        Dictionary<GameObject, GameObject> m_goMap;
        
        public IEnumerator LoadMYTYAvatarAsync(AssetBundle bundle, GameObject root,
            bool spriteOnly = false)
        {
            if (bundle == null)
            {
                Debug.Log("Failed to load bundle");
                yield break;
            }

            m_goMap = new();
            var avatarSelectorGO = new GameObject("AvatarSelector");
            avatarSelectorGO.transform.parent = root.transform;
            yield return SetupAvatarSelector(root, avatarSelectorGO, bundle);
            var selector = avatarSelectorGO.GetComponent<AvatarSelector>();
            var request = bundle.LoadAssetAsync<DefaultLayoutAsset>("DefaultLayoutAsset.asset");
            yield return request;
            var layout = request.asset as DefaultLayoutAsset;
            SetupLayout(root, selector, layout);

            if (spriteOnly) yield break;


            yield return SetupAvatarAsync(root, avatarSelectorGO,bundle);
        }

        IEnumerator SetupAvatarAsync(GameObject root, GameObject avatarSelectorGO, AssetBundle bundle)
        {
            var motionAdaptersGO = new GameObject("MotionAdapter");
            var selector = avatarSelectorGO.GetComponent<AvatarSelector>();
            motionAdaptersGO.transform.parent = root.transform;

            foreach (var rootConPrefabPath in selector.mytyAssetStorage.rootControllers)
            {

                var request = bundle.LoadAssetAsync<GameObject>(rootConPrefabPath);
                yield return request;
                var rootConPrefab = request.asset as GameObject;
                var rootConGO = GameObject.Instantiate(rootConPrefab);
                rootConGO.name = rootConPrefab.name;
                rootConGO.transform.parent = root.transform;
                RestoreController(rootConGO);
                BuildMap(rootConPrefab, rootConGO);
            }


            var mtRequest = bundle.LoadAssetAsync<GameObject>("MotionTemplate");
            yield return mtRequest;
            var mtPrefabGo = mtRequest.asset as GameObject;
            var motionTemplateGO = GameObject.Instantiate(mtPrefabGo);
            motionTemplateGO.name = "MotionTemplate";
            motionTemplateGO.transform.parent = root.transform;
            BuildMap(mtPrefabGo, motionTemplateGO);
            
            foreach (var motionAdapterPrefabPath in selector.mytyAssetStorage.motionAdapters)
            {
                //yield return new WaitForSeconds(0.1f);
                var request = bundle.LoadAssetAsync<GameObject>(motionAdapterPrefabPath);
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
            var nativeAdapters = motionAdapterGO.GetComponents<NativeAdapter>();
            foreach (var item in nativeAdapters)
            {
                var nativeAdapter = item as ISerializableAdapter;
                if (nativeAdapter == null) return;
                nativeAdapter.Deserialize(m_goMap);
            }
        }

        public (bool isSupported, bool isAROnly) IsARFaceSupported(AssetBundle bundle)
        {
            var asset = bundle.LoadAsset<ARFaceAsset>("ARFaceData.asset");
            if (asset == null) return (false, false);

            if (asset.AROnly) return (true, true);
            return (true, false);
        }

        public IEnumerator LoadARFace(AssetBundle bundle, GameObject gameObject)
        {
            var request = bundle.LoadAssetAsync<ARFaceAsset>("ARFaceData.asset");
            yield return request;
            var asset = request.asset as ARFaceAsset;
            if (gameObject == null)
            {
                gameObject = new GameObject("ARFaceDataHolder");
            }

            var holder = gameObject.AddComponent<ARFaceDataHolder>();
            holder.arFaceAsset = asset;
        }

        public void LockController(GameObject arAssetObj, GameObject rootGo)
        {
            var holder = arAssetObj.GetComponent<ARFaceDataHolder>();
            if (holder == null)
            {
                Debug.LogWarning("LockController : No ARFaceDataHolder in asset object");
                return;
            }

            var asset = holder.arFaceAsset;
            var selector = rootGo.GetComponentInChildren<AvatarSelector>();

            int idx = 0;
            foreach (var item in asset.items)
            {
                if(item.headBone == null) continue;
                var parentBoneList = new List<GameObject>();
                var currBone = m_goMap[item.headBone];
                do
                {
                    parentBoneList.Add(currBone);
                    if (currBone == selector.templates[idx].boneRootObj) break;
                    currBone = currBone.transform.parent.gameObject;
                } while (currBone != null);
                

                var boneControllers = rootGo.GetComponentsInChildren<BoneController>();
                
                var filteredBone = boneControllers.Where(controller =>
                {
                    foreach (var bone in parentBoneList)
                    {
                        if (controller.rigTarget.Contains(bone)) return true;
                    }
                
                    return false;
                });
                foreach (var bone in filteredBone)
                {
                    bone.skip = true;
                }
                ++idx;
            }
            
            
        }
        
        protected void BuildMap(GameObject prefabNode, GameObject goNode)
        {
            m_goMap[prefabNode] = goNode;
            for (int i = 0; i < prefabNode.transform.childCount; i++)
            {
                BuildMap(prefabNode.transform.GetChild(i).gameObject, goNode.transform.GetChild(i).gameObject);
            }
        }
        
        protected IEnumerator SetupAvatarSelector(GameObject root, GameObject avatarSelectorGO, AssetBundle bundle)
        {
            var request = bundle.LoadAssetAsync<MYTYAssetScriptableObject>("mytyassetdata.asset");
            yield return request;
            var mytyasset = request.asset as MYTYAssetScriptableObject;
            var selector = avatarSelectorGO.AddComponent<AvatarSelector>();

            selector.mytyAssetStorage = mytyasset;
            selector.templates = new();

            foreach (var templateInfo in mytyasset.templateInfos)
            {
                var template = new AvatarTemplate();
                template.PSBPath = templateInfo.template;
                var instanceReq = bundle.LoadAssetAsync<GameObject>(templateInfo.instance);
                yield return instanceReq;
                var prefabInstance = instanceReq.asset as GameObject;
                var psb = Instantiate(prefabInstance);
                psb.name = prefabInstance.name;
                BuildMap(prefabInstance, psb);
                template.instance = psb;
                psb.transform.parent = root.transform;
                // var splReq = bundle.LoadAssetAsync<SpriteLibraryAsset>(templateInfo.spriteLibrary);
                // yield return splReq;

                template.spriteLibrary = SpriteLibraryFactoryRuntime.CreateLibraryRuntime(psb);//splReq.asset as SpriteLibraryAsset;

                int boneIdx = template.instance.transform.childCount - 1;
                if (boneIdx >= 0)
                {
                    var boneCandidate = template.instance.transform.GetChild(boneIdx).gameObject;
                    if (boneCandidate.name.ToUpper().StartsWith("BONE")) template.boneRootObj = boneCandidate;
                }

                selector.templates.Add(template);

            }


            selector.id = "";
            selector.Configure();
        }
        
        void RestoreController(GameObject node)
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
        
        public static string GetKitVersionInfo(AssetBundle assetBundle)
        {
            if (assetBundle == null) return "";

            var textAsset = assetBundle.LoadAsset<TextAsset>("VERSION.txt");
            if (textAsset == null) return "";

            return textAsset.text;
        }

        public static string GetEditorVersionInfo(AssetBundle assetBundle)
        {
            if (assetBundle == null) return "";

            var textAsset = assetBundle.LoadAsset<TextAsset>("EditorInfo.txt");
            if (textAsset == null) return "";

            return textAsset.text;
        }
    }
}
