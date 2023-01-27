using System;
using System.Collections;
using System.Linq;
using MYTYKit.Scripts.MetaverseKit.Asset.Impl;
using MYTYKit.Scripts.MetaverseKit.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace MYTYKit.Scripts.MetaverseKit.Example
{
    public class ARAvatarLoading : MonoBehaviour
    {
        [SerializeField]
        MYTYAssetInfoHandler m_assetInfoHandler;
        [SerializeField]
        AvatarLoader m_avatarLoader;
        [SerializeField]
        Material m_arFaceMaterial;
        
        public string targetCollectionAddress;

        void Start()
        {
            var assetInfo = m_assetInfoHandler.GetAssetInfo(
                targetCollectionAddress,
                Enumerable.Range(0, 10000).Select(_ => _.ToString()).ToList(),
                _ => _.platform == AvatarPlatform.Standalone.ToString())
                .OrderBy(_ => DateTime.Parse(_.updatedAt)).Last();
            
            StartCoroutine(CallLoadAvatar(assetInfo));
        }
        
        IEnumerator CallLoadAvatar(AssetInfo asset)
        {
            using (var uwr = UnityWebRequestAssetBundle.GetAssetBundle(asset.assetUri))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Fetching Asset Bundle Failed");
                }
                else
                {
                    var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    m_avatarLoader.LoadAvatar(
                        true, false, bundle, asset.avatarName, null, OnARAvatarLoaded);
                }
            }
        }
        
        private void OnARAvatarLoaded(GameObject arAvatar, RenderTexture arFaceTexture)
        {
            arAvatar.transform.localPosition = new Vector3(0, 0, 0);
            arAvatar.GetComponentInChildren<Camera>().gameObject.SetActive(false);
            m_arFaceMaterial.mainTexture = arFaceTexture;
        }
    }
}