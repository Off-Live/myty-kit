using System.Collections.Generic;

namespace MYTYKit.Scripts.MetaverseKit.Data
{
    public enum AvatarPlatform
    {
        Standalone,
        iOS
    }
    public class AssetInfo
    {
        public string avatarName;
        public string updatedAt;
        public List<string> supportingTokens;
        public string assetUri;
    }

    public class MYTYAssetInfo
    {
        public MYTYAvatar avatar;
        public string platform;
        public string assetUri;
    }

    public class MYTYAvatar
    {
        public string name;
        public List<string> tokenIdsBig;
        public string updatedAt;
        public bool deprecated;
        public KitVersion kitVersion;
    }

    public class KitVersion
    {
        public int major;
        public int minor;
        public int patch;
    }
}