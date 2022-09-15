using System.Collections;
using UnityEngine;

public interface IMYTYAvatarImporter
{
    public IEnumerator LoadMYTYAvatarAsync(GameObject motionTemplateGO, AssetBundle bundle, GameObject root,
        bool spriteOnly = false);

    public GameObject LoadMYTYAvatar(GameObject motionTemplateGO, AssetBundle bundle, string loadedName = "avatar",
        bool spriteOnly = false);

    public string GetKitVersionInfo();
    public string GetEditorVersionInfo();

}
