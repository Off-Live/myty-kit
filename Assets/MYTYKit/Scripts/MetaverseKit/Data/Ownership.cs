using System.Collections.Generic;

namespace MYTYKit.Scripts.MetaverseKit.Data
{
    public class OwnershipInfo
    {
        public string collectionAddress;
        public List<string> tokenIds;
    }

    public class MYTYOwnershipInfo
    {
        public string collectionAddress;
        public List<MYTYToken> avatars;
    }

    public class MYTYToken
    {
        public string id;
    }
}