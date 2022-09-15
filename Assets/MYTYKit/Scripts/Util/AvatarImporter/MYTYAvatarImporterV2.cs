

using System.Collections;
using UnityEngine;

public class MYTYAvatarImporterV2 : MonoBehaviour, IMYTYAvatarImporter
{
    public IEnumerator LoadMYTYAvatarAsync(GameObject motionTemplateGO, AssetBundle bundle, GameObject root,
        bool spriteOnly = false)
    {
        throw new System.NotImplementedException();
    }

    public GameObject LoadMYTYAvatar(GameObject motionTemplateGO, AssetBundle bundle, string loadedName = "avatar",
        bool spriteOnly = false)
    {
        throw new System.NotImplementedException();
    }

    public string GetKitVersionInfo()
    {
        throw new System.NotImplementedException();
    }

    public string GetEditorVersionInfo()
    {
        throw new System.NotImplementedException();
    }
}
