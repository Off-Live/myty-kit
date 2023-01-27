using System;
using System.Collections.Generic;
using MYTYKit.Scripts.MetaverseKit.Data;

namespace MYTYKit.Scripts.MetaverseKit.Asset.Interface
{
    public interface IAssetInfoHandler<T>
    {
        public List<AssetInfo> GetAssetInfo(string collectionAddress, List<string> tokenIds, Func<T, bool> filter);
    }
}