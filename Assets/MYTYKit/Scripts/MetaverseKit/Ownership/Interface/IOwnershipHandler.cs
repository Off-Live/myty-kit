using System.Collections.Generic;
using MYTYKit.Scripts.MetaverseKit.Data;

namespace MYTYKit.Scripts.MetaverseKit.Ownership.Interface
{
    public interface IOwnershipHandler
    {
        public List<OwnershipInfo> GetOwnerships(string collectionAddress);
    }
}