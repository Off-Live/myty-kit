using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MYTYKit.Components;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using MYTYKit.Controllers;

namespace MYTYKit.AvatarImporter
{
    public class MYTYAvatarImporterV2 : MYTYAvatarImporter, IARFaceLoader
    {
        AssetBundle m_assetBundle;
        

        public override IEnumerator LoadMYTYAvatarAsync(GameObject motionSourceGo, AssetBundle bundle, GameObject root,
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
            motionSource.UpdateMotionAndTemplates();

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
            var nativeAdapter = motionAdapterGO.GetComponent<NativeAdapter>() as ISerializableAdapter;
            if (nativeAdapter == null) return;
            nativeAdapter.Deserialize(goMap);
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

    }
}