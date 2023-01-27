using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Scripts.MetaverseKit.Asset.Impl;
using MYTYKit.Scripts.MetaverseKit.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace MYTYKit.Scripts.MetaverseKit.Example
{
    public class VRAvatarLoading : MonoBehaviour
    {
        [SerializeField]
        MYTYAssetInfoHandler m_assetInfoHandler;
        [SerializeField]
        AvatarLoader m_avatarLoader;
    
        public List<string> targetCollections;

        private List<AssetInfo> m_assetInfos = new();
        private int m_idx;
        void Start()
        {
            m_assetInfos = targetCollections.Select((address) =>
                m_assetInfoHandler.GetAssetInfo(
                        address,
                        Enumerable.Range(0, 10000).Select(_ => _.ToString()).ToList(),
                        _ => _.platform == AvatarPlatform.Standalone.ToString())
                    .OrderBy(_ => DateTime.Parse(_.updatedAt)).Last()
            ).ToList();
        
            m_idx = 0;
            StartCoroutine(CallLoadAvatar(m_assetInfos[0]));
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
                        false, true, bundle, asset.avatarName, OnVRAvatarLoaded);
                }
            }
        }

        private void OnVRAvatarLoaded(GameObject vrAvatar)
        {
            vrAvatar.transform.localPosition = new Vector3((float)(50 * m_idx), 0, 0);
            m_idx++;
            if (m_idx < m_assetInfos.Count)
            {
                StartCoroutine(CallLoadAvatar(m_assetInfos[m_idx]));
            }
        }
    }
}
