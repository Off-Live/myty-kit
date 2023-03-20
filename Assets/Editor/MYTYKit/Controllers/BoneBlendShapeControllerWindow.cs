using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Components;
using MYTYKit.Controllers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MYTYKit
{
    public class BlendShapeUIItemFactory
    {
        public static VisualElement Build()
        {
            var rootElement = new VisualElement();

            rootElement.style.flexDirection = FlexDirection.Row;
            rootElement.style.paddingLeft = 3;
            rootElement.style.paddingRight = 3;
            var nameField = new Label();
            nameField.style.flexGrow = 1;
            nameField.style.width = 100;
            nameField.style.unityTextAlign = TextAnchor.MiddleLeft;
            var slider = new Slider();
            slider.lowValue = 0.0f;
            slider.highValue = 1.0f;
            slider.value = 0.0f;
            slider.style.flexGrow = 4;
            var setButton = new Button();
            setButton.text = "Set Pose";
            setButton.style.flexGrow = 1;
            rootElement.Add(nameField);
            rootElement.Add(slider);
            rootElement.Add(setButton);
            return rootElement;
        }

        public static void Bind(VisualElement root, SerializedObject so, int bsIndex)
        {
            var controller = so.targetObject as BoneBlendShapeController;
            root.Q<Label>().text = controller.blendShapes[bsIndex].name;
            root.Q<Slider>().BindProperty(so.FindProperty("blendShapes").GetArrayElementAtIndex(bsIndex)
                .FindPropertyRelative("weight"));

            root.Q<Slider>().RegisterValueChangedCallback(evt => { controller.UpdateInEditor(); });

            root.Q<Button>().clicked += () =>
            {
                controller.blendShapes[bsIndex].basis =
                    controller.rigTarget.Select(target => target.transform.localPosition).ToList();
                controller.blendShapes.ForEach(item => item.weight = 0.0f);
                controller.blendShapes[bsIndex].weight = 1.0f;
            };

        }
    }

    public class BoneBlendShapeControllerWindow : EditorWindow
    {
        SerializedObject m_conSO;

        [MenuItem("MYTY Kit/Controller/Bone-based Blendshape Controller", false, 20)]
        public static void ShowController()
        {
            var wnd = GetWindow<BoneBlendShapeControllerWindow>();
            wnd.titleContent = new GUIContent("BlendShape Controller");
        }

        void CreateGUI()
        {
            var uiTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/MYTYKit/UI/BoneBlendShapeController.uxml");
            uiTemplate.CloneTree(rootVisualElement);
            var selectedGOs = Selection.GetFiltered<BoneBlendShapeController>(SelectionMode.Editable);
            var conVE = rootVisualElement.Q<ObjectField>("OBJController");
            var listView = rootVisualElement.Q<ListView>("LSTRiggingGO");
            var addBtn = rootVisualElement.Q<Button>("BTNAdd");
            var removeBtn = rootVisualElement.Q<Button>("BTNRemove");
            var removeAllBtn = rootVisualElement.Q<Button>("BTNRemoveAll");
            var anchorToggle = rootVisualElement.Q<Toggle>("TGLAnchor");
            var resetBtn = rootVisualElement.Q<Button>("BTNReset");
            var bsListView = rootVisualElement.Q<ListView>("LSTBlendShape");

            addBtn.clicked += OnAdd;
            removeBtn.clicked += OnRemove;
            removeAllBtn.clicked += OnRemoveAll;
            resetBtn.clicked += OnReset;


            conVE.objectType = typeof(BoneBlendShapeController);
            listView.makeItem = () => { return new ObjectField(); };

            listView.bindItem = (e, i) =>
            {
                (e as ObjectField).value = listView.itemsSource[i] as GameObject;
                if (listView.itemsSource[i] == null)
                {
                    (e as ObjectField).label = "Deleted or modified.";
                    (e as ObjectField).AddToClassList("deletedObjField");
                    (e as ObjectField).RemoveFromClassList("noEditableObjField");
                }
                else
                {
                    (e as ObjectField).label = "";
                    (e as ObjectField).AddToClassList("noEditableObjField");
                    (e as ObjectField).RemoveFromClassList("deletedObjField");
                }
            };
            
            bsListView.makeItem = BlendShapeUIItemFactory.Build;
            bsListView.bindItem = (e, i) => { BlendShapeUIItemFactory.Bind(e, m_conSO, i); };


            conVE.RegisterValueChangedCallback((ChangeEvent<Object> e) =>
            {
                InitWithController(e.newValue as BoneBlendShapeController);
            });

            anchorToggle.RegisterValueChangedCallback(OnAnchorToggled);

            UpdatePanelConfig();
            if (selectedGOs.Length == 0) return;

            InitWithController(selectedGOs[0]);
        }

        void InitWithController(BoneBlendShapeController controller)
        {
            var conVE = rootVisualElement.Q<ObjectField>("OBJController");
            var listView = rootVisualElement.Q<ListView>("LSTRiggingGO");
            var bsListView = rootVisualElement.Q<ListView>("LSTBlendShape");
            var listSource = new List<GameObject>();
            var anchorToggle = rootVisualElement.Q<Toggle>("TGLAnchor");

            if (controller == null)
            {
                anchorToggle.SetValueWithoutNotify(false);
                listView.itemsSource = listSource;
                listView.Rebuild();
                bsListView.itemsSource = new List<BoneBlendShapeController.BlendShapeBasis>();
                bsListView.Rebuild();
                UpdatePanelConfig();
                m_conSO = null;
                return;
            }
            m_conSO = new SerializedObject(controller);
            var targetProps = m_conSO.FindProperty("rigTarget");

            for (int i = 0; i < targetProps.arraySize; i++)
            {
                listSource.Add(targetProps.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
            }

            listView.itemsSource = listSource;
            listView.Rebuild();

            conVE.value = controller;

            anchorToggle.SetValueWithoutNotify(controller.blendShapes.Count != 0);
            UpdatePanelConfig();
			if(controller.blendShapes.Count>0) InitBlendShapeUI();
        }

        void UpdatePanelConfig()
        {
            var anchorToggle = rootVisualElement.Q<Toggle>("TGLAnchor");
            var beforePanel = rootVisualElement.Q<VisualElement>("VEBeforeAnchor");
            var afterPanel = rootVisualElement.Q<VisualElement>("VEAfterAnchor");

            beforePanel.style.display = anchorToggle.value ? DisplayStyle.None : DisplayStyle.Flex;
            afterPanel.style.display = anchorToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnAnchorToggled(ChangeEvent<bool> e)
        {
            var anchorToggle = rootVisualElement.Q<Toggle>("TGLAnchor");
            if (m_conSO == null)
            {
               
                anchorToggle.SetValueWithoutNotify(e.previousValue);
                return;
            }
            
            if (e.newValue)
            {
                RegisterAnchor();
            }
            else
            {
                if (!EditorUtility.DisplayDialog("MYTY Kit",
                        "The blendshape information will be deleted. Do you want to clear Anchor?", "Yes", "No"))
                    anchorToggle.SetValueWithoutNotify(e.previousValue);
                else
                    ClearAnchor();
            }
            UpdatePanelConfig();
            
        }

        void RegisterAnchor()
        {
            var anchorToggle = rootVisualElement.Q<Toggle>("TGLAnchor");
            var targetsProp = m_conSO.FindProperty("rigTarget");

            if (targetsProp.arraySize == 0)
            {
                EditorUtility.DisplayDialog("MYTY Kit", "At least one rigging target should be added", "Ok");
                anchorToggle.value = false;
                return;
            }

            var originalProp = m_conSO.FindProperty("orgRig");
            originalProp.arraySize = targetsProp.arraySize;

            for (int i = 0; i < originalProp.arraySize; i++)
            {
                var go = targetsProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                originalProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value =
                    go.transform.localPosition;
                originalProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue =
                    go.transform.localRotation;
                originalProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value =
                    go.transform.localScale;
            }

            m_conSO.ApplyModifiedProperties();
			InitController();
            InitBlendShapeUI();
        }

        void ClearAnchor()
        {
            var targetsProp = m_conSO.FindProperty("rigTarget");
            var listView = rootVisualElement.Q<ListView>("LSTBlendShape");
            if (targetsProp.arraySize == 0)
            {
                return;
            }

            var controller = m_conSO.targetObject as BoneBlendShapeController;

            for (var i = 0; i < controller.rigTarget.Count; i++)
            {
                controller.rigTarget[i].transform.localPosition = controller.orgRig[i].position;
                controller.rigTarget[i].transform.localScale = controller.orgRig[i].scale;
                controller.rigTarget[i].transform.localRotation = controller.orgRig[i].rotation;
            }

            var originalProp = m_conSO.FindProperty("orgRig");
            var blendShapeProp = m_conSO.FindProperty("blendShapes");
            originalProp.arraySize = 0;
            blendShapeProp.arraySize = 0;
            m_conSO.ApplyModifiedProperties();
           
        }

        void OnAdd()
        {
            var targetsProp = m_conSO.FindProperty("rigTarget");
            var targets = Selection.GetFiltered<GameObject>(SelectionMode.Editable);
            var offset = targetsProp.arraySize;
            var controller = m_conSO.targetObject as BoneBlendShapeController;

            targetsProp.arraySize += targets.Length;
            for (int i = 0; i < targets.Length; i++)
            {
                targetsProp.GetArrayElementAtIndex(offset + i).objectReferenceValue = targets[i];
            }

            var newSource = new List<GameObject>();

            for (int i = 0; i < targetsProp.arraySize; i++)
            {
                newSource.Add(targetsProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
            }

            rootVisualElement.Q<ListView>("LSTRiggingGO").itemsSource = newSource;
            rootVisualElement.Q<ListView>("LSTRiggingGO").Rebuild();

            m_conSO.FindProperty("orgRig").arraySize = 0;

            m_conSO.ApplyModifiedProperties();
            BoneControllerStorage.Save();
        }

        void OnRemove()
        {
            var targetsProp = m_conSO.FindProperty("rigTarget");
            var listView = rootVisualElement.Q<ListView>("LSTRiggingGO");
            var willRemove = listView.selectedIndices.ToList();
            var newList = new List<GameObject>();
            if (willRemove.Count == 0) return;

            for (int i = 0; i < targetsProp.arraySize; i++)
            {
                if (willRemove.Contains(i)) continue;
                newList.Add(targetsProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
            }

            targetsProp.arraySize = newList.Count();
            for (int i = 0; i < targetsProp.arraySize; i++)
            {
                targetsProp.GetArrayElementAtIndex(i).objectReferenceValue = newList[i];
            }

            listView.itemsSource = newList;
            listView.Rebuild();
            m_conSO.FindProperty("orgRig").arraySize = 0;

            m_conSO.ApplyModifiedProperties();
            BoneControllerStorage.Save();
        }

        void OnRemoveAll()
        {
            var targetsProp = m_conSO.FindProperty("rigTarget");
            var listView = rootVisualElement.Q<ListView>("LSTRiggingGO");
            targetsProp.arraySize = 0;
            listView.itemsSource = new List<GameObject>();
            listView.Rebuild();
            m_conSO.FindProperty("orgRig").arraySize = 0;
            m_conSO.ApplyModifiedProperties();
            BoneControllerStorage.Save();
        }

        void OnReset()
        {
            var controller = m_conSO.targetObject as BoneBlendShapeController;
            if (controller.blendShapes.Count == 0) return;
            controller.blendShapes.ForEach(item => item.weight = 0.0f);
        }

        void InitBlendShapeUI()
        {
            var listView = rootVisualElement.Q<ListView>("LSTBlendShape");
            var controller = m_conSO.targetObject as BoneBlendShapeController;
            listView.itemsSource = controller.blendShapes;
            listView.Rebuild();
        }

		void InitController(){
			var controller = m_conSO.targetObject as BoneBlendShapeController;
			controller.blendShapes.Clear();
            foreach (var value in Enum.GetValues(typeof(BlendShape)))
            {
                controller.blendShapes.Add(new BoneBlendShapeController.BlendShapeBasis()
                {
                    name = value.ToString()
                });
            }

            controller.blendShapes.ForEach(item =>
            {
                item.basis = new();
                Enumerable.Range(0, controller.rigTarget.Count).ToList().ForEach(idx => item.basis.Add(controller.orgRig[idx].position));
            });
            m_conSO.Update();
		}
    }
}