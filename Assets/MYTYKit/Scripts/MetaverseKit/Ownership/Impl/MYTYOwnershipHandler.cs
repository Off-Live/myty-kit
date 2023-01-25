using System.Collections.Generic;
using System.Linq;
using MYTYKit.Scripts.MetaverseKit.Data;
using MYTYKit.Scripts.MetaverseKit.Ownership.Interface;
using MYTYKit.Scripts.MetaverseKit.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.Scripts.MetaverseKit.Ownership.Impl
{
    public class MYTYOwnershipHandler : MonoBehaviour, IOwnershipHandler
    {
        private string MYTYVirtualOwnershipServer = "https://virtual-ownership.myty.space/v0/ownership?address=";

        public List<OwnershipInfo> GetOwnerships(string walletAddress)
        {
            var res = HttpClientUtil.GetAsync(MYTYVirtualOwnershipServer + walletAddress);

            return JsonConvert.DeserializeObject<JArray>(res)?.ToList()
                .Select(_ => _.ToObject<MYTYOwnershipInfo>())
                .Where(info => info != null)
                .Select(info => new OwnershipInfo
                {
                    collectionAddress = info!.collectionAddress,
                    tokenIds = info.avatars!.Select(_ => _.id).ToList()
                }).ToList() ?? new List<OwnershipInfo>();
        }
    }
}