using System.Collections;
using UnityEngine;

public interface IMYTYAvatarImporter
{
    public IEnumerator LoadMYTYAvatarAsync(GameObject extraGo, AssetBundle bundle, GameObject root,
        bool spriteOnly = false);

    
    public string GetKitVersionInfo();
    public string GetEditorVersionInfo();

}
