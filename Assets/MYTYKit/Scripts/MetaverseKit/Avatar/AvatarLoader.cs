using System;
using System.Collections;
using System.IO;
using System.Linq;
using MYTYKit;
using MYTYKit.AvatarImporter;
using MYTYKit.Components;
using MYTYKit.Scripts.MetaverseKit.Util;
using UnityEngine;

public class AvatarLoader : MonoBehaviour
{
    [SerializeField]
    GameObject m_motionSource;
    
    IMYTYAvatarImporter m_importer;
    
    public void LoadAvatar(
        bool loadAR,
        bool loadVR,
        AssetBundle bundle,
        string assetName,
        Action<GameObject> vrPostProcess = null,
        Action<GameObject, RenderTexture> arPostProcess = null,
        Action<Exception> exceptionHandler = null)
    {
        StartCoroutine(
            CoroutineUtil.RunThrowingIterator(
                LoadAvatarImpl(
                    loadAR, loadVR, bundle, assetName, vrPostProcess, arPostProcess), exceptionHandler));
    }

    private IEnumerator LoadAvatarImpl(
        bool loadAR, 
        bool loadVR, 
        AssetBundle bundle,
        string assetName,
        Action<GameObject> vrPostProcess,
        Action<GameObject, RenderTexture> arPostProcess)
    {
        if (bundle == null)
        {
            Debug.Log($"Asset with {assetName} has bundle as null");
            throw new NullReferenceException();
        }
        
        ImportSigResolver.DetectAndAttachImporter(gameObject, bundle);
        m_importer = gameObject.GetComponent<IMYTYAvatarImporter>();

        var arFaceLoader = m_importer as IARFaceLoader;
        var (hasARMode, onlyARMode) = arFaceLoader?.IsARFaceSupported(bundle) ?? (false, false);
        loadAR = loadAR && hasARMode;
        loadVR = loadVR && !onlyARMode;

        var arFaceTexture = new RenderTexture(512, 512, 1, RenderTextureFormat.ARGB32);

        var traits = bundle.LoadAsset<MYTYAssetScriptableObject>("MYTYAssetData").traits;
        var idList = traits.Select(_ => _.tokenId).ToList();
        idList.Sort(ComparisonUtil.CompareStrings);
        
        var minId = idList.First();

        if (loadAR && loadVR)
        {
            yield return LoadVRAvatar(bundle, assetName, minId, vrPostProcess);
            yield return LoadARAvatar(bundle, assetName, minId, arFaceTexture, arPostProcess);
        }
        else if (loadAR)
        {
            yield return LoadARAvatar(bundle, assetName, minId, arFaceTexture, arPostProcess);
        }
        else if (loadVR)
        {
            yield return LoadVRAvatar(bundle, assetName, minId, vrPostProcess);
        }
        else
        {
            Debug.Log($"Failed to load asset with {assetName}.");
            throw new InvalidDataException();
        }
        
        bundle.Unload(false);
    }

    private IEnumerator LoadARAvatar(
        AssetBundle bundle,
        string assetName,
        string minId,
        RenderTexture arFaceTexture,
        Action<GameObject, RenderTexture> postProcess)
    {
        var arAvatar = new GameObject(assetName + "AR");
            
        yield return m_importer.LoadMYTYAvatarAsync(m_motionSource, bundle, arAvatar);
        
        var arFaceLoader = m_importer as IARFaceLoader;
        yield return arFaceLoader!.LoadARFace(bundle, arAvatar);
        arFaceLoader.LockController(arAvatar, arAvatar);
        var holder = arAvatar.GetComponent<ARFaceDataHolder>();
        ARFaceItem[] arFaceItems = holder.arFaceAsset.items;
                
        foreach (var item in arFaceItems)
        {
            var arCam = Instantiate(item.renderCam, arAvatar.transform);
            arCam.targetTexture = arFaceTexture;
            arCam.clearFlags = CameraClearFlags.Color;
            arCam.backgroundColor = new Color(0, 0, 0, 0);
        }

        var arSelector = arAvatar.GetComponentInChildren<AvatarSelector>();

        var idx = arSelector.GetCurrentTemplateIndex();
        if (idx < 0)
        {
            Debug.Log($"Asset with {assetName} has wrong configuration about AR Mode");
            DestroyImmediate(arAvatar);
            throw new InvalidDataException();
        }

        arSelector.id = minId;
        arSelector.ConfigureARFeature(arFaceItems);
        
        postProcess.Invoke(arAvatar, arFaceTexture);
    }

    private IEnumerator LoadVRAvatar(
        AssetBundle bundle,
        string assetName,
        string minId,
        Action<GameObject> postProcess)
    {
        var vrAvatar = new GameObject(assetName);
            
        yield return m_importer.LoadMYTYAvatarAsync(m_motionSource, bundle, vrAvatar);

        var vrSelector = vrAvatar.GetComponentInChildren<AvatarSelector>(); 
        vrSelector.id = minId;
        vrSelector.Configure();
        postProcess.Invoke(vrAvatar);
    }
}
