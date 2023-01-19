using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UIElements;

using MYTYKit.Controllers;
using MYTYKit.Components;
using MYTYKit.MotionTemplates;
using MYTYKit.MotionAdapters;
using Newtonsoft.Json.Linq;


namespace MYTYKit
{
    public class Exporter : EditorWindow
    {
        [SerializeField] VisualTreeAsset UI;
        Dictionary<GameObject, GameObject> m_objectMap;

        BuildTarget[] m_buildPlatform =
            { BuildTarget.StandaloneOSX, BuildTarget.iOS, BuildTarget.Android, BuildTarget.WebGL };

        readonly string[] m_platforms = { "Standalone(Mac/Win)", "iOS", "Android", "WebGL" };
        readonly string[] m_bundleSurfix = { "_standalone", "_ios", "_android", "_webgl" };

        const string ImportSig= "MYTYAvatarImporterV2";

        [MenuItem("MYTY Kit/Export AssetBundle", false, 1)]
        public static void ShowGUI()
        {
            var wnd = CreateInstance<Exporter>();
            wnd.titleContent = new GUIContent("AssetBundle Exporter");
            wnd.ShowUtility();
        
        }

        void OnFocus()
        {
            var avatarConfigElem = rootVisualElement.Q<ObjectField>("OBJConfig");
            var avatarSelector = FindObjectOfType<AvatarSelector>();
            if (avatarSelector == null) return;
            avatarConfigElem.value = avatarSelector.gameObject;
        }

        void CreateGUI()
        {
            UI.CloneTree(rootVisualElement);

            var avatarConfigElem = rootVisualElement.Q<ObjectField>("OBJConfig");
            var cameraElem = rootVisualElement.Q<ObjectField>("OBJCamera");
            var mtElem = rootVisualElement.Q<ObjectField>("OBJMT");
            var controllerListView = rootVisualElement.Q<ListView>("LSTController");
            var maListView = rootVisualElement.Q<ListView>("LSTMotionAdapter");
            var platformGroup = rootVisualElement.Q<GroupBox>("GRPPlatform");
            var supportedPlatform = GetInfoAboutSupportedPlatform();
            SetupARUI();
        
            for (int i = 0; i < m_platforms.Length; i++)
            {
                var toggle = new Toggle();
                toggle.AddToClassList("platform_item");
                toggle.text = m_platforms[i];
            
                var isSupported = supportedPlatform[m_platforms[i]];
                if (isSupported) toggle.value = true;
                else
                {
                    toggle.value = false;
                    toggle.SetEnabled(false);
                }
                platformGroup.contentContainer.Add(toggle);
            }
        
            avatarConfigElem.objectType = typeof(AvatarSelector);
            cameraElem.objectType = typeof(Camera);
            mtElem.objectType = typeof(MotionTemplateMapper);
            rootVisualElement.Q<Button>("BTNExport").clicked += Export;

            avatarConfigElem.value = FindObjectOfType<AvatarSelector>().gameObject;
            mtElem.value = FindObjectOfType<MotionTemplateMapper>().gameObject;
            cameraElem.value = Camera.main;

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

        void SetupARUI()
        {
            var arToggle = rootVisualElement.Q<Toggle>("CHKIncludeAR");
            var asset = AssetDatabase.LoadAssetAtPath<ARFaceAsset>(MYTYUtil.AssetPath + "/ARFaceData.asset");
            if (asset != null)
            {
                var flag = true;
                foreach (var item in asset.items)
                {
                    flag &= item.isValid;
                }
                arToggle.value = flag;
                arToggle.SetEnabled(flag);
            }
            else
            {
                arToggle.value = false;
                arToggle.SetEnabled(false);
            }
        }

        void Export()
        {
        
            var assetName = new List<string>();
            var avatarConfigVE = rootVisualElement.Q<ObjectField>("OBJConfig");
            var selector = avatarConfigVE.value as AvatarSelector;
            var mytyAsset = selector.mytyAssetStorage;

            if(selector == null)
            {
                EditorUtility.DisplayDialog("MYTY Kit", "Avatar Selector is missing or refreshed. Please reopen exporter dialog.", "Ok");
                return;
            }

            if (!selector.CheckRootBoneValidity())
            {
                EditorUtility.DisplayDialog("MYTY Kit", "RootBoneObjs in Avatar Selector is invalid. They must be assigned or the name of root bone of avatar should be started with BONE(Case insensitive).", "Ok");
                return;
            }

            if (Directory.Exists(MYTYUtil.PrefabPath))
            {
                Directory.Delete(MYTYUtil.PrefabPath, true);
            }

            MYTYUtil.BuildAssetPath(MYTYUtil.PrefabPath);
            AssetDatabase.Refresh();
            assetName.Add(AssetDatabase.GetAssetPath(mytyAsset));
        
            m_objectMap = new();
        
            PrepareMotionTemplateMapper(assetName);
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

            PrepareLayoutAsset(selector, assetName);
            PrepareVersionInfo(assetName);
            PrepareEditorInfo(assetName);
            PrepareImportSig(assetName);
            PrepareARData(assetName);
        
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

            var platformSelection = GetPlatformSelection();
            for (int i = 0; i < m_platforms.Length; i++)
            {
                Debug.Log(m_platforms[i]+" "+platformSelection[i]);
            }

            if (SystemInfo.operatingSystem.StartsWith("Mac"))
            {
                m_buildPlatform[0] = BuildTarget.StandaloneOSX;
            }
            else
            {
                m_buildPlatform[0] = BuildTarget.StandaloneWindows;
            }

            using (ZipArchive zip = ZipFile.Open(MYTYUtil.BundlePath+ "/"+ filename + ".zip", ZipArchiveMode.Create))
            {
                for (int i = 0; i < platformSelection.Length; i++)
                {
                    if (platformSelection[i])
                    {
                        buildMap[0].assetBundleName = filename + m_bundleSurfix[i];
                        BuildPipeline.BuildAssetBundles(MYTYUtil.BundlePath, buildMap, BuildAssetBundleOptions.None,
                            m_buildPlatform[i]);
                        zip.CreateEntryFromFile(MYTYUtil.BundlePath + "/" + filename + m_bundleSurfix[i], filename+m_bundleSurfix[i]);
                    }
                }
            }


            Close();
        }

        void PrepareLayoutAsset(AvatarSelector selector, List<string> assetName)
        {
            var assetSCO = ScriptableObject.CreateInstance<DefaultLayoutAsset>();
            var cameraElem = rootVisualElement.Q<ObjectField>("OBJCamera");
            var mainCamera = cameraElem.value as Camera;

            var path = AssetDatabase.GenerateUniqueAssetPath(MYTYUtil.PrefabPath + "/refCamera.prefab");
            var savedCamera = PrefabUtility.SaveAsPrefabAsset(mainCamera.gameObject, path);
        
            AssetDatabase.CreateAsset(assetSCO, MYTYUtil.PrefabPath + "/" + "DefaultLayoutAsset.asset");
            assetName.Add(path);
            assetName.Add(MYTYUtil.PrefabPath + "/" + "DefaultLayoutAsset.asset");

            assetSCO.camera = savedCamera.GetComponent<Camera>();
            assetSCO.templateTransforms = new();

            for(int i = 0; i < selector.templates.Count; i++)
            {
                assetSCO.templateTransforms.Add(new TransformProperty
                {
                    position = selector.templates[i].instance.transform.position,
                    scale = selector.templates[i].instance.transform.localScale,
                    rotation = selector.templates[i].instance.transform.rotation
                });
            }
            EditorUtility.SetDirty(assetSCO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

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
            DestroyImmediate(copyGO);
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

        private void BuildCloneObjectMap(GameObject node, GameObject clone)
        {
            m_objectMap[node] = PrefabUtility.GetCorrespondingObjectFromSource(clone);
            for(int i = 0; i < node.transform.childCount; i++)
            {
                BuildCloneObjectMap(node.transform.GetChild(i).gameObject, clone.transform.GetChild(i).gameObject);
            }
        }

        private bool PrepareController(MYTYAssetScriptableObject mytyAsset, List<string> assetName)
        {
            var rootCons = rootVisualElement.Q<ListView>("LSTController").itemsSource;
       
            mytyAsset.rootControllers = new();
       
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
                BuildCloneObjectMap(rootConGO, rootConCopy);
                GameObject.DestroyImmediate(rootConCopy);
            }
            return true;
        }

        bool PrepareMotionAdapter(MYTYAssetScriptableObject mytyAsset, List<string> assetName)
        {
            var motionAdapters = rootVisualElement.Q<ListView>("LSTMotionAdapter").itemsSource;
            mytyAsset.motionAdapters = new();
            foreach (var elem in motionAdapters)
            {
                var motionAdapterGo = elem as GameObject;

                var adapterArrayforGo = motionAdapterGo.GetComponents<NativeAdapter>();
                var cloningTargetGo = new GameObject();
                cloningTargetGo.name = motionAdapterGo.name;
                
                foreach (var item in adapterArrayforGo)
                {
                    var serializableAdapter = item as ISerializableAdapter;
                    if (serializableAdapter == null) continue;
                    serializableAdapter.SerializeIntoNewObject(cloningTargetGo, m_objectMap);
                }
                var path = AssetDatabase.GenerateUniqueAssetPath(MYTYUtil.PrefabPath + "/" + motionAdapterGo.name + ".prefab");
                var savedPrefab = PrefabUtility.SaveAsPrefabAsset(cloningTargetGo, path);
                assetName.Add(path);
                mytyAsset.motionAdapters.Add(path);
                DestroyImmediate(cloningTargetGo);
                
            }

            return true;
        }

        void PrepareVersionInfo(List<string> assetName)
        {
            var packageJson = Path.GetFullPath("Packages/com.offlive.myty.myty-kit/package.json");
            var jsonText= File.ReadAllText(packageJson);
            var json = JObject.Parse(jsonText);
            var versionStr = (string)json["version"];
            var writer = File.CreateText(MYTYUtil.PrefabPath + "/VERSION.txt");
            writer.Write(versionStr);
            writer.Close();
            assetName.Add(MYTYUtil.PrefabPath + "/VERSION.txt");
        }

        void PrepareEditorInfo(List<string> assetName)
        {
            var filepath = MYTYUtil.PrefabPath + "/EditorInfo.txt";
            StreamWriter file = new(filepath); 
            file.Write(Application.unityVersion);
            file.Close();
            assetName.Add(filepath);
        }

        void PrepareMotionTemplateMapper(List<string> assetName)
        {
            var mtElem = rootVisualElement.Q<ObjectField>("OBJMT");
            var motionTemplateMapper = mtElem.value as MotionTemplateMapper;
            var mtGo = motionTemplateMapper.gameObject;
            var mtClone = Instantiate(mtGo);
            mtClone.name = mtGo.name;
            var path = AssetDatabase.GenerateUniqueAssetPath(MYTYUtil.PrefabPath + "/MotionTemplate.prefab");
            PrefabUtility.SaveAsPrefabAssetAndConnect(mtClone, path, InteractionMode.UserAction);
            assetName.Add(path);
            BuildCloneObjectMap(mtGo, mtClone);
            DestroyImmediate(mtClone);
        }

        void PrepareImportSig(List<string> assetName)
        {
            var filepath = MYTYUtil.PrefabPath + "/AvatarImportSig.txt";
            StreamWriter file = new(filepath); 
            file.Write(ImportSig);
            file.Close();
            assetName.Add(filepath);
        }

        void PrepareARData(List<string> assetName)
        {
            var filepath = MYTYUtil.AssetPath + "/ARFaceData.asset";
            var arToggle = rootVisualElement.Q<Toggle>("CHKIncludeAR");
            var arOnlyToggle = rootVisualElement.Q<Toggle>("CHKAROnly");
        
            if (!arToggle.value) return;
            var asset = AssetDatabase.LoadAssetAtPath<ARFaceAsset>(filepath);
            asset.AROnly = arOnlyToggle.value;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            assetName.Add(filepath);
        }
    
        Dictionary<string, bool> GetInfoAboutSupportedPlatform()
        {
            var ret = new Dictionary<string, bool>();
            ret["Standalone(Mac/Win)"] = false;
            ret["iOS"] = false;
            ret["Android"] = false;
            ret["WebGL"] = false;
        
            if (SystemInfo.operatingSystem.StartsWith("Mac"))
            {
                if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
                {
                    ret["Standalone(Mac/Win)"] = true;
                }
            }
            else
            {
                if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows))
                {
                    ret["Standalone(Mac/Win)"] = true;
                }
            }
        
            if(BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
            {
                ret["iOS"] = true;
            }
       
            if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                ret["Android"] = true;
            }
            if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                ret["WebGL"] = true;
            }

            return ret;
        }

        bool[] GetPlatformSelection()
        {
            var ret = new bool[m_platforms.Length];
            var platformGroup = rootVisualElement.Q<GroupBox>("GRPPlatform");
            for (int i = 0; i < m_platforms.Length; i++)
            {
                ret[i] = (platformGroup[i] as Toggle).value;
            }

            return ret;
        }

    }
}
