using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.U2D.Animation;
using UnityEngine.UIElements;

using MYTYKit.Components;

namespace MYTYKit
{
    class MarkingTree
    {
        public bool mark = false;
        public GameObject gameObject;
        public List<MarkingTree> children = new();
    }

    public class ARFaceSetupWindow : EditorWindow
    {
        public VisualTreeAsset UITemplate;
        public Texture placeholder;

        Image m_preview;

        [MenuItem("MYTY Kit/AR Face Mode Setup", false, 20)]

        public static void ShowGUI()
        {
            var wnd = CreateInstance<ARFaceSetupWindow>();
            wnd.titleContent = new GUIContent("AR Face Mode Setup");
            wnd.minSize = wnd.maxSize = new Vector2(450, 180);
            wnd.ShowUtility();
        }

        [MenuItem("MYTY Kit/AR Face Mode Setup", true, 1)]
        static bool ValidateShowGUI()
        {
            var selector = FindObjectOfType<AvatarSelector>();
            return selector != null;
        }


        void CreateGUI()
        {
            UITemplate.CloneTree(rootVisualElement);
            CreateRenderPreview();
            var selectorProp = rootVisualElement.Q<ObjectField>("OBJAvatarSelector");
            var setupBtn = rootVisualElement.Q<Button>("BTNSetup");
            var doneBtn = rootVisualElement.Q<Button>("BTNDone");
            var templateProp = rootVisualElement.Q<ObjectField>("OBJTemplate");
            var assetProp = rootVisualElement.Q<ObjectField>("OBJAsset");
            var boneProp = rootVisualElement.Q<ObjectField>("OBJBone");
            var camProp = rootVisualElement.Q<ObjectField>("OBJRenderCam");
            var removeBtn = rootVisualElement.Q<Button>("BTNRemove");
            var saveBtn = rootVisualElement.Q<Button>("BTNSave");
            var discardBtn = rootVisualElement.Q<Button>("BTNDiscard");

            selectorProp.objectType = typeof(AvatarSelector);
            boneProp.objectType = typeof(GameObject);
            camProp.objectType = typeof(Camera);
            templateProp.objectType = typeof(GameObject);
            assetProp.objectType = typeof(ARFaceAsset);


            setupBtn.clicked += OnSetup;
            doneBtn.clicked += OnDone;
            removeBtn.clicked += OnRemove;
            saveBtn.clicked += OnSave;
            discardBtn.clicked += OnDiscard;

            selectorProp.RegisterValueChangedCallback(evt => OnAvatarSelectionChanged(evt.newValue as AvatarSelector));
            boneProp.RegisterValueChangedCallback(evt => OnBoneChanged(evt.newValue as GameObject));
            camProp.RegisterValueChangedCallback(evt => OnRenderCameraChanged(evt.newValue as Camera));

            var selector = FindObjectOfType<AvatarSelector>();
            selectorProp.value = selector;
        }

        void CreateRenderPreview()
        {
            var previewVE = rootVisualElement.Q<VisualElement>("VERenderPreview");
            m_preview = new Image();
            previewVE.Add(m_preview);
            m_preview.style.width = 80;
            m_preview.style.height = 80;
            m_preview.image = placeholder;
        }

        void ChangeMode(bool isSetupMode)
        {
            var selectPanel = rootVisualElement.Q<VisualElement>("VETemplateSelection");
            var setupPanel = rootVisualElement.Q<VisualElement>("VEARFaceSetup");
            if (isSetupMode)
            {
                selectPanel.style.display = DisplayStyle.None;
                setupPanel.style.display = DisplayStyle.Flex;
                minSize = maxSize = new Vector2(450, 485);
            }
            else
            {
                selectPanel.style.display = DisplayStyle.Flex;
                setupPanel.style.display = DisplayStyle.None;
                minSize = maxSize = new Vector2(450, 180);
            }
        }

        void OnAvatarSelectionChanged(AvatarSelector selector)
        {
            var listView = rootVisualElement.Q<ListView>("LSTTemplate");
            var isTemplateSetup = new bool[selector.templates.Count];
            for (var i = 0; i < isTemplateSetup.Length; i++)
            {
                isTemplateSetup[i] = false;
            }

            var asset = AssetDatabase.LoadAssetAtPath<ARFaceAsset>(MYTYUtil.AssetPath + "/ARFaceData.asset");
            if (asset != null)
            {
                Debug.Assert(asset.items.Length == isTemplateSetup.Length);
                for (var i = 0; i < isTemplateSetup.Length; i++)
                {
                    isTemplateSetup[i] = asset.items[i].isValid;
                }
            }

            var itemList = new List<string>();
            for (var i = 0; i < isTemplateSetup.Length; i++)
            {
                var tag = isTemplateSetup[i] ? "(AR Ready)" : "";
                itemList.Add(selector.templates[i].instance.name + " " + tag);
            }

            listView.itemsSource = itemList;
            listView.Rebuild();
        }

        void OnSetup()
        {
            var listView = rootVisualElement.Q<ListView>("LSTTemplate");
            var selectorProp = rootVisualElement.Q<ObjectField>("OBJAvatarSelector");
            var templateProp = rootVisualElement.Q<ObjectField>("OBJTemplate");
            var renderCamProp = rootVisualElement.Q<ObjectField>("OBJRenderCam");
            var boneProp = rootVisualElement.Q<ObjectField>("OBJBone");
            var assetProp = rootVisualElement.Q<ObjectField>("OBJAsset");
            var traitListView = rootVisualElement.Q<ListView>("LSTTraits");
            var index = listView.selectedIndex;
            var selector = selectorProp.value as AvatarSelector;
            if (index < 0)
            {
                EditorUtility.DisplayDialog("MYTY Kit", "No template is selected!", "Ok");
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<ARFaceAsset>(MYTYUtil.AssetPath + "/ARFaceData.asset");
            if (asset == null)
            {
                var assetSco = CreateInstance<ARFaceAsset>();
                AssetDatabase.CreateAsset(assetSco, MYTYUtil.AssetPath + "/ARFaceData.asset");
                assetSco.items = new ARFaceItem[selector.templates.Count];
                EditorUtility.SetDirty(assetSco);
                AssetDatabase.SaveAssets();
                asset = AssetDatabase.LoadAssetAtPath<ARFaceAsset>(MYTYUtil.AssetPath + "/ARFaceData.asset");
            }

            templateProp.value = selector.templates[index].instance;
            assetProp.value = asset;

            traitListView.itemsSource = null;
            if (asset.items[index].isValid)
            {
                traitListView.itemsSource = asset.items[index].traits;
                renderCamProp.value = asset.items[index].renderCam;
            }
            else
            {
                renderCamProp.value = null;
            }
            boneProp.SetValueWithoutNotify(asset.items[index].headBone);
            traitListView.Rebuild();
            ChangeMode(true);
        }

        void OnDone()
        {
            Close();
        }

        void OnBoneChanged(GameObject bone)
        {
            var templateProp = rootVisualElement.Q<ObjectField>("OBJTemplate");
            var boneProp = rootVisualElement.Q<ObjectField>("OBJBone");
            var traitListView = rootVisualElement.Q<ListView>("LSTTraits");
            var template = templateProp.value as GameObject;
            traitListView.itemsSource = null;
            traitListView.Rebuild();
            if (bone == null) return;
            if (!IsChildOfTemplate(template, bone))
            {
                EditorUtility.DisplayDialog("MYTY Kit", "The selected bone is from the template", "Ok");
                boneProp.SetValueWithoutNotify(null);
                return;
            }

            var boneList = BuildBoneList(bone);
            var markingTree = new MarkingTree();

            BuildMarkingTree(markingTree, template, boneList);

            var traitsList = new List<string>();

            for (var i = 0; i < markingTree.children.Count; i++)
            {
                if (markingTree.children[i].gameObject.name == "Animation") continue;
                ExtractTraits(markingTree.children[i], "", traitsList);
            }

            traitListView.itemsSource = traitsList;
            traitListView.Rebuild();

        }

        void OnRenderCameraChanged(Camera newCamera)
        {
            if (newCamera == null) m_preview.image = placeholder;
            else m_preview.image = newCamera.targetTexture;
        }

        void OnRemove()
        {
            var traitListView = rootVisualElement.Q<ListView>("LSTTraits");
            var indices = traitListView.selectedIndices;
            var traits = traitListView.itemsSource as List<string>;
            var willBeDeleted = new List<string>();
            foreach (var index in indices)
            {
                willBeDeleted.Add(traits[index]);
            }

            foreach (var item in willBeDeleted)
            {
                traits.Remove(item);
            }

            traitListView.itemsSource = traits;
            traitListView.Rebuild();
        }

        void OnSave()
        {
            var listView = rootVisualElement.Q<ListView>("LSTTemplate");
            var renderCamProp = rootVisualElement.Q<ObjectField>("OBJRenderCam");
            var assetProp = rootVisualElement.Q<ObjectField>("OBJAsset");
            var boneProp = rootVisualElement.Q<ObjectField>("OBJBone");
            var traitListView = rootVisualElement.Q<ListView>("LSTTraits");
            var index = listView.selectedIndex;
            var asset = assetProp.value as ARFaceAsset;
            var traits = traitListView.itemsSource as List<string>;

            var renderCam = renderCamProp.value as Camera;
            var camName = "ARFaceCam" + index + ".prefab";
            GameObject savedCamFab = null;
            if (renderCam != null)
            {
                if (PrefabUtility.IsPartOfPrefabAsset(renderCam)) savedCamFab = renderCam.gameObject;
                else savedCamFab = PrefabUtility.SaveAsPrefabAsset(renderCam.gameObject, MYTYUtil.AssetPath + "/" + camName);
            }

            asset.items[index].isValid = true;
            if (savedCamFab != null)
            {
                asset.items[index].renderCam = savedCamFab.GetComponent<Camera>();
            }
            else asset.items[index].renderCam = null;

            asset.items[index].traits = traits;

            var boneInScene = boneProp.value as GameObject;
            asset.items[index].headBone = PrefabUtility.GetCorrespondingObjectFromSource(boneInScene); 

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("MYTY Kit", "Saved!", "Ok");

            var selectorProp = rootVisualElement.Q<ObjectField>("OBJAvatarSelector");
            OnAvatarSelectionChanged(selectorProp.value as AvatarSelector);
            ChangeMode(false);
        }

        void OnDiscard()
        {
            if (!EditorUtility.DisplayDialog("MYTY Kit", "Discard changes and stop?", "Ok", "Cancel"))
            {
                return;
            }

            ChangeMode(false);
        }

        bool IsChildOfTemplate(GameObject template, GameObject bone)
        {
            if (bone.transform.parent == null) return false;
            for (int i = 0; i < template.transform.childCount; i++)
            {
                if (bone == template.transform.GetChild(i).gameObject) return true;
            }

            return IsChildOfTemplate(template, bone.transform.parent.gameObject);
        }

        bool BuildMarkingTree(MarkingTree root, GameObject rootGo, List<GameObject> boneList)
        {
            root.mark = false;
            root.gameObject = rootGo;
            root.children.Clear();

            var spriteSkin = rootGo.GetComponent<SpriteSkin>();
            if (spriteSkin == null)
            {
                if (rootGo.transform.childCount == 0) return false;
            }
            else
            {
                foreach (var spriteBone in spriteSkin.boneTransforms)
                {
                    if (boneList.Contains(spriteBone.gameObject))
                    {
                        root.mark = true;
                    }
                }

                return root.mark;
            }

            Debug.Assert(rootGo.transform.childCount != 0);

            var flag = true;

            for (var i = 0; i < rootGo.transform.childCount; i++)
            {
                var newNode = new MarkingTree();
                root.children.Add(newNode);
                flag &= BuildMarkingTree(newNode, rootGo.transform.GetChild(i).gameObject, boneList);
            }

            root.mark = flag;
            return root.mark;
        }

        List<GameObject> BuildBoneList(GameObject root)
        {
            var ret = new List<GameObject>();
            var stack = new Stack<GameObject>();

            stack.Push(root);

            while (stack.Count > 0)
            {
                GameObject curr = stack.Pop();
                ret.Add(curr);
                for (int i = 0; i < curr.transform.childCount; i++)
                {
                    stack.Push(curr.transform.GetChild(i).gameObject);
                }
            }

            return ret;
        }

        void ExtractTraits(MarkingTree root, string history, List<string> traits)
        {
            history += "/" + root.gameObject.name;

            if (root.mark)
            {
                traits.Add(history.Substring(1)); //Get rid of first '/'
                return;
            }

            for (var i = 0; i < root.children.Count; i++)
            {
                ExtractTraits(root.children[i], history, traits);
            }
        }
    }
}