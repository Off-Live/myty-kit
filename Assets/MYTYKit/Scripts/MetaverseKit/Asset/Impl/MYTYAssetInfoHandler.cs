using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Scripts.MetaverseKit.Data;
using UnityEngine;
using MYTYKit.Scripts.MetaverseKit.Asset.Interface;
using MYTYKit.Scripts.MetaverseKit.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MYTYKit.Scripts.MetaverseKit.Asset.Impl
{
    public class MYTYAssetInfoHandler : MonoBehaviour, IAssetInfoHandler<MYTYAssetInfo>
    {
        private string MYTYRegistryServer = "https://api-stag.myty.space/v0/avatarAssetVerified?collectionAddress=";

        public List<AssetInfo> GetAssetInfo(
            string collectionAddress,
            List<string> tokenIds,
            Func<MYTYAssetInfo, bool> filter)
        {
            var res = HttpClientUtil.GetAsync(MYTYRegistryServer + collectionAddress);
            var filtered = 
                    JsonConvert.DeserializeObject<JArray>(res)?.ToList()
                        .Select(_ => _.ToObject<MYTYAssetInfo>())
                        .Where(assetInfo => assetInfo != null && filter(assetInfo!))
                        .Where(assetInfo => assetInfo.avatar.tokenIdsBig.Count == 0 ||
                                            assetInfo.avatar.tokenIdsBig.Exists(tokenIds.Contains))
                        .Select(assetInfo => new AssetInfo
                        {
                            avatarName = assetInfo!.avatar.name,
                            updatedAt = assetInfo!.avatar.updatedAt,
                            supportingTokens = assetInfo!.avatar.tokenIdsBig,
                            assetUri = assetInfo!.assetUri
                        });
            return filtered?.ToList() ?? new List<AssetInfo>();
        }
    }
}