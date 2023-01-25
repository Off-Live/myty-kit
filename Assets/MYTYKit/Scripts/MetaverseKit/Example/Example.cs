using System;
using System.Collections;
using System.Linq;
using MYTYKit.Scripts.MetaverseKit.Asset.Impl;
using MYTYKit.Scripts.MetaverseKit.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace MYTYKit.Scripts.MetaverseKit.Example
{
    public class Example : MonoBehaviour
    {
        [SerializeField]
        MYTYAssetInfoHandler m_assetInfoHandler;
        [SerializeField]
        AvatarLoader m_avatarLoader;
        RenderTexture m_arFaceTexture;

        public string targetCollectionAddress;
        void Start()
        {
            var assetInfos = m_assetInfoHandler.GetAssetInfo(
                targetCollectionAddress,
                Enumerable.Range(0, 10000).Select(_ => _.ToString()).ToList(),
                _ => _.platform == AvatarPlatform.Standalone.ToString());
            
            var selected = assetInfos.OrderBy(_ => DateTime.Parse(_.updatedAt)).Last();
            
            m_arFaceTexture = new RenderTexture(512, 512, 1, RenderTextureFormat.ARGB32);
            StartCoroutine(CallLoadAvatar(selected));
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
                        true, true, bundle, "test",
                        new Vector3(-10, 10, 0), new Vector3(-10, 0, 0),
                        m_arFaceTexture);
                }
            }
        }
    }
}