using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.U2D.Animation;



public class Exporter : EditorWindow
{
    [SerializeField]
    VisualTreeAsset UI;

    private Dictionary<GameObject, GameObject> m_controllerMap;

    [MenuItem("MYTY Kit/Export AssetBundle", false, 1)]
    public static void ShowGUI()
    {
        var wnd = GetWindow<Exporter>();
        wnd.titleContent = new GUIContent("AssetBundle Exporter");
    }


    private void CreateGUI()
    {
        UI.CloneTree(rootVisualElement);

        var avatarConfigElem = rootVisualElement.Q<ObjectField>("OBJConfig");
        var controllerListView = rootVisualElement.Q<ListView>("LSTController");
        var maListView = rootVisualElement.Q<ListView>("LSTMotionAdapter");
        avatarConfigElem.objectType = typeof(AvatarSelector);
        rootVisualElement.Q<Button>("BTNExport").clicked += Export;

        avatarConfigElem.value = GameObject.FindObjectOfType<AvatarSelector>().gameObject;

        controllerListView.makeItem = () =>
        {
            var elem = new ObjectField();
            elem.objectType = typeof(RootController);
            return elem;
        };
        controllerListView.bindItem = (visualElem, index) =>
        {
            var objField = visualElem as ObjectField;
            objField.value = controllerListView.itemsSource[index] as RootController;
        };
        controllerListView.itemsSource = GameObject.FindObjectsOfType<RootController>();

        maListView.makeItem = () => new ObjectField();
        maListView.bindItem = (visualElem, index) =>
        {
            var objField = visualElem as ObjectField;
            objField.value = maListView.itemsSource[index] as GameObject;
        };
        var nativeAdapters =  GameObject.FindObjectsOfType<NativeAdapter>();
        var maList = new List<GameObject>();
        
        foreach(var item in nativeAdapters)
        {
            if (maList.Find(go => go.GetInstanceID() == item.gameObject.GetInstanceID()) == null)
            {
                maList.Add(item.gameObject);
            }
        }

        maListView.itemsSource = maList;

    }

    private void SavePrefab(GameObject go, string path, out string savedPath, bool connect = false)
    {
        string localPath = AssetDatabase.GenerateUniqueAssetPath(path);
        if (connect)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, localPath, InteractionMode.UserAction);
        }
        else PrefabUtility.SaveAsPrefabAsset(go, localPath);
        savedPath = localPath;
    }

    private void Export()
    {
        var assetName = new List<string>();
        var avatarConfigVE = rootVisualElement.Q<ObjectField>("OBJConfig");
        var selector = avatarConfigVE.value as AvatarSelector;
        var mytyAsset = selector.mytyAssetStorage;

        if (Directory.Exists(MYTYUtil.PrefabPath))
        {
            Directory.Delete(MYTYUtil.PrefabPath, true);
        }

        MYTYUtil.BuildAssetPath(MYTYUtil.PrefabPath);
        AssetDatabase.Refresh();
        assetName.Add(AssetDatabase.GetAssetPath(mytyAsset));

        if (!PrepareConfigurator(mytyAsset, assetName))
        {
            Debug.Log("error in export config");
            return;
        }

        if (!PrepareController(mytyAsset , assetName))
        {
            Debug.Log("error in export controller");
            return;
        }
        if (!PrepareMotionAdapter(mytyAsset, assetName))
        {
            Debug.Log("error in export motionadapter");
            return;
        }

        EditorUtility.SetDirty(mytyAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var filename = rootVisualElement.Q<TextField>("TXTFilename").text;

        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

        buildMap[0] = new AssetBundleBuild
        {
            assetBundleName = filename,
            assetNames = assetName.ToArray()
        };

        if (Directory.Exists(MYTYUtil.BundlePath))
        {
            Directory.Delete(MYTYUtil.BundlePath, true);
        }
        MYTYUtil.BuildAssetPath(MYTYUtil.BundlePath);

        if (SystemInfo.operatingSystem.StartsWith("Mac"))
        {
            if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
            {
                BuildPipeline.BuildAssetBundles(MYTYUtil.BundlePath, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
            }
            else
            {
                Debug.LogError("macOS support must be installed.");
            }
        }
        else
        {
            if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows))
            {
                BuildPipeline.BuildAssetBundles(MYTYUtil.BundlePath, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            }
            else
            {
                Debug.LogError("Windows support must be installed.");
            }
        }

        buildMap[0].assetBundleName = filename + "_ios";
        if(BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
        {
            BuildPipeline.BuildAssetBundles(MYTYUtil.BundlePath, buildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);
        }
        else
        {
            Debug.LogWarning("You should install iOS support to build ios bundle");
        }

        buildMap[0].assetBundleName = filename + "_android";
        if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
        {
            BuildPipeline.BuildAssetBundles(MYTYUtil.BundlePath, buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
        }
        else
        {
            Debug.LogWarning("You should install iOS support to build ios bundle");
        }

        Close();
    }

    private bool PrepareConfigurator(MYTYAssetScriptableObject mytyAsset, List<string> assetName)
    {
        var avatarConfigVE = rootVisualElement.Q<ObjectField>("OBJConfig");
        if (avatarConfigVE.value == null) return false;

        var original = avatarConfigVE.value as AvatarSelector;
        original.ResetAvatar();
        
        var copyGO = Instantiate(original.gameObject);
        var selector = copyGO.GetComponent<AvatarSelector>();
        mytyAsset.templateInfos = new List<AvatarTemplateInfo>();
        foreach(var template in selector.templates)
        {
            PrefabUtility.ApplyPrefabInstance(template.instance, InteractionMode.UserAction);
            template.instance = PrefabUtility.GetCorrespondingObjectFromSource(template.instance);
            template.boneRootObj = PrefabUtility.GetCorrespondingObjectFromSource(template.boneRootObj);
            var psbObj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(template.instance);
            var psbPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(psbObj);
            var splPath = psbPath.Substring(0, psbPath.Length - 4) + ".asset";
            var newPath = MYTYUtil.PrefabPath + "/" + template.instance.name+".asset";
            var instancePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(template.instance);
            AssetDatabase.CopyAsset(splPath, newPath);
            template.spriteLibrary = AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(newPath);
            assetName.Add(newPath);
            assetName.Add(psbPath);
            assetName.Add(instancePath);
            mytyAsset.templateInfos.Add(new AvatarTemplateInfo
            {
                template = psbPath,
                instance = instancePath,
                spriteLibrary = newPath
            }) ;
        }

        PrefabUtility.SaveAsPrefabAsset(copyGO, MYTYUtil.PrefabPath+"/"+ MYTYUtil.SelectorPrefab);
        GameObject.DestroyImmediate(copyGO);
        assetName.Add(MYTYUtil.PrefabPath + "/" + MYTYUtil.SelectorPrefab);
        return true;
    }

    private void PrepareControllerToSave(GameObject node)
    {
        var mytyCons = node.GetComponents<MYTYController>();
        foreach (var con in mytyCons)
        {
            con.PrepareToSave();
        }

        for(int i = 0; i < node.transform.childCount; i++)
        {
            PrepareControllerToSave(node.transform.GetChild(i).gameObject);
        }
    }

    private void BuildControllerMap(GameObject node, GameObject clone)
    {
        m_controllerMap[node] = PrefabUtility.GetCorrespondingObjectFromSource(clone);
        for(int i = 0; i < node.transform.childCount; i++)
        {
            BuildControllerMap(node.transform.GetChild(i).gameObject, clone.transform.GetChild(i).gameObject);
        }
    }

    private bool PrepareController(MYTYAssetScriptableObject mytyAsset, List<string> assetName)
    {
        var rootCons = rootVisualElement.Q<ListView>("LSTController").itemsSource;
       
        mytyAsset.rootControllers = new();
        m_controllerMap = new();
        foreach (var elem  in rootCons)
        {
            var rootConGO = (elem as RootController).gameObject;
            var rootConCopy = GameObject.Instantiate(rootConGO);
            for(int i = 0; i < rootConCopy.gameObject.transform.childCount; i++)
            {
                PrepareControllerToSave(rootConCopy.gameObject.transform.GetChild(i).gameObject);
            }

            var path = AssetDatabase.GenerateUniqueAssetPath(MYTYUtil.PrefabPath + "/" + rootConGO.name + ".prefab");
            var savedPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(rootConCopy.gameObject, path,InteractionMode.UserAction);
            assetName.Add(path);
            mytyAsset.rootControllers.Add(path);
            BuildControllerMap(rootConGO, rootConCopy);
            GameObject.DestroyImmediate(rootConCopy);
        }
      


        return true;
    }

    private bool PrepareMotionAdapter(MYTYAssetScriptableObject mytyAsset, List<string> assetName)
    {
        var motionAdapters = rootVisualElement.Q<ListView>("LSTMotionAdapter").itemsSource;
        mytyAsset.motionAdapters = new();
        foreach (var elem in motionAdapters)
        {
            var motionAdapter = elem as GameObject;
            var motionAdapterClone = GameObject.Instantiate(motionAdapter);

            var nativeAdapter = motionAdapterClone.GetComponent<NativeAdapter>();
            if (nativeAdapter == null) continue;

            foreach (var fieldInfo in nativeAdapter.GetType().GetFields())
            {
                if (fieldInfo.FieldType.IsSubclassOf(typeof(RiggingModel)))
                {
                    var riggingModel = fieldInfo.GetValue(nativeAdapter) as RiggingModel;
                    if (riggingModel != null)
                    {
                        fieldInfo.SetValue(nativeAdapter, PrefabUtility.GetCorrespondingObjectFromSource(riggingModel));
                    }
                }else if (fieldInfo.FieldType.IsSubclassOf(typeof(MYTYController)) || fieldInfo.FieldType.IsEquivalentTo(typeof(MYTYController)))
                {
                    Debug.Log(fieldInfo.Name);
                    var conGO = (fieldInfo.GetValue(nativeAdapter) as MYTYController).gameObject;
                    var prefabGO = m_controllerMap[conGO];
                    fieldInfo.SetValue(nativeAdapter, prefabGO.GetComponent<MYTYController>());
                }
            }
            
            var path = AssetDatabase.GenerateUniqueAssetPath(MYTYUtil.PrefabPath + "/" + motionAdapter.name + ".prefab");
            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(motionAdapterClone.gameObject, path);
            assetName.Add(path);
            mytyAsset.motionAdapters.Add(path);
            GameObject.DestroyImmediate(motionAdapterClone);
        }

        return true;
    }

}
