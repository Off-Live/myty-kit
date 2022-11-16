using System.Collections;
using UnityEngine;

namespace MYTYKit.AvatarImporter
{
    public interface IARFaceLoader
    {
        public (bool isSupported, bool isAROnly) IsARFaceSupported(AssetBundle bundle);
        public IEnumerator LoadARFace(AssetBundle bundle, GameObject rootGo);
        public void LockController(GameObject arAssetObj, GameObject rootGo);
    }
}