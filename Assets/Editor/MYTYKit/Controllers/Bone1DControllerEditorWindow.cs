using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using MYTYKit.Components;
using MYTYKit.Controllers;

namespace MYTYKit
{
    public class Bone1DControllerEditorWindow : EditorWindow
    {
        public VisualTreeAsset UITemplate;
        private SerializedObject _conSO;

        [MenuItem("MYTY Kit/Controller/Bone 1D Controller", false, 20)]
        public static void ShowController()
        {
            var wnd = GetWindow<Bone1DControllerEditorWindow>();
            wnd.titleContent = new GUIContent("Bone 1D Controller");
        }

        private void CreateGUI()
        {
            UITemplate.CloneTree(rootVisualElement);
            var selectedGOs = Selection.GetFiltered<Bone1DController>(SelectionMode.Editable);
            var conVE = rootVisualElement.Q<ObjectField>("OBJController");
            var listView = rootVisualElement.Q<ListView>("LSTBoneGO");
            var addBtn = rootVisualElement.Q<Button>("BTNAdd");
            var removeBtn = rootVisualElement.Q<Button>("BTNRemove");
            var removeAllBtn = rootVisualElement.Q<Button>("BTNRemoveAll");
            var valueSlider = rootVisualElement.Q<Slider>("SLDValue");
            var maxVE = rootVisualElement.Q<FloatField>("FLTMax");
            var minVE = rootVisualElement.Q<FloatField>("FLTMin");

            addBtn.clicked += AddSelections;
            removeBtn.clicked += Remove;
            removeAllBtn.clicked += RemoveAll;

            conVE.objectType = typeof(Bone1DController);
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

            conVE.RegisterValueChangedCallback((ChangeEvent<Object> e) =>
            {
                InitWithController(e.newValue as Bone1DController);
            });

            valueSlider.RegisterValueChangedCallback((ChangeEvent<float> e) =>
            {
                var con = _conSO.targetObject as Bone1DController;
                con.InterpolateGUI();
            });

            rootVisualElement.Q<Toggle>("BTNMin").RegisterCallback((MouseUpEvent e) =>
            {
                HandleRigToggle(e, "xminRig");
                SetControlPos(minVE.value);
            });
            rootVisualElement.Q<Toggle>("BTNMax").RegisterCallback((MouseUpEvent e) =>
            {
                HandleRigToggle(e, "xmaxRig");
                SetControlPos(maxVE.value);
            });

            minVE.RegisterValueChangedCallback((ChangeEvent<float> e) => { RescaleSlider(); });

            maxVE.RegisterValueChangedCallback((ChangeEvent<float> e) => { RescaleSlider(); });

            minVE.RegisterValueChangedCallback((ChangeEvent<float> e) => { RescaleSlider(); });

            maxVE.RegisterValueChangedCallback((ChangeEvent<float> e) => { RescaleSlider(); });

            if (selectedGOs.Length == 0) return;


            InitWithController(selectedGOs[0]);
        }

        void InitWithController(Bone1DController controller)
        {
            _conSO = new SerializedObject(controller);

            var conVE = rootVisualElement.Q<ObjectField>("OBJController");
            var valueSlider = rootVisualElement.Q<Slider>("SLDValue");
            var listView = rootVisualElement.Q<ListView>("LSTBoneGO");
            var listSource = new List<GameObject>();
            var maxVE = rootVisualElement.Q<FloatField>("FLTMax");
            var minVE = rootVisualElement.Q<FloatField>("FLTMin");
            var valueVE = rootVisualElement.Q<FloatField>("FLTValue");

            var rigProps = _conSO.FindProperty("rigTarget");
            for (int i = 0; i < rigProps.arraySize; i++)
            {
                listSource.Add(rigProps.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
            }

            listView.itemsSource = listSource;
            listView.Rebuild();
            conVE.value = controller;
            valueSlider.BindProperty(_conSO.FindProperty("controlValue"));
            maxVE.BindProperty(_conSO.FindProperty("maxValue"));
            minVE.BindProperty(_conSO.FindProperty("minValue"));
            valueVE.BindProperty(_conSO.FindProperty("controlValue"));

            SyncRiggingStatus();
        }

        void RescaleSlider()
        {
            if (_conSO == null) return;
            var valueSlider = rootVisualElement.Q<Slider>("SLDValue");
            var maxVE = rootVisualElement.Q<FloatField>("FLTMax");
            var minVE = rootVisualElement.Q<FloatField>("FLTMin");

            if (maxVE.value < minVE.value) maxVE.value = minVE.value;

            valueSlider.lowValue = minVE.value;
            valueSlider.highValue = maxVE.value;
        }

        private void SetControlPos(float value)
        {
            if (_conSO != null && _conSO.targetObject != null)
            {
                var target = (Bone1DController)_conSO.targetObject;
                _conSO.FindProperty("controlValue").floatValue = value;
                _conSO.ApplyModifiedProperties();
                target.InterpolateGUI();
            }
        }


        private void Record(SerializedProperty prop)
        {
            var rigProp = _conSO.FindProperty("rigTarget");
            prop.arraySize = rigProp.arraySize;
            for (int i = 0; i < rigProp.arraySize; i++)
            {
                var obj = (GameObject)rigProp.GetArrayElementAtIndex(i).objectReferenceValue;
                prop.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value =
                    obj.transform.localPosition;
                prop.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue =
                    obj.transform.localRotation;
                prop.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value = obj.transform.localScale;
            }

            _conSO.ApplyModifiedProperties();
        }

        void SetPropTransform(SerializedProperty prop, int index, Transform transform)
        {
            prop.GetArrayElementAtIndex(index).FindPropertyRelative("position").vector3Value =
                transform.localPosition;
            prop.GetArrayElementAtIndex(index).FindPropertyRelative("rotation").quaternionValue =
                transform.localRotation;
            prop.GetArrayElementAtIndex(index).FindPropertyRelative("scale").vector3Value = transform.localScale;
        }

        private bool HandleRigToggle(MouseUpEvent e, string prop)
        {
            var keepPivot = rootVisualElement.Q<Toggle>("TGLPivotPos");
            var elem = e.target as Toggle;
            if (elem.value)
            {
                Record(_conSO.FindProperty(prop));
            }
            else
            {
                var target = _conSO.targetObject as Bone1DController;
                if (!keepPivot.value) target.ToOrigin();
                _conSO.FindProperty(prop).arraySize = 0;
                _conSO.ApplyModifiedProperties();

            }

            SyncRiggingStatus();
            return elem.value;
        }

        private void SyncRiggingStatus()
        {

            SyncRiggingHelper("xminRig", "BTNMin");
            SyncRiggingHelper("xmaxRig", "BTNMax");

        }

        private void SyncRiggingHelper(string property, string btnElem)
        {
            if (_conSO.FindProperty(property).arraySize > 0)
            {
                rootVisualElement.Q<Toggle>(btnElem).value = true;
            }
            else
            {
                rootVisualElement.Q<Toggle>(btnElem).value = false;
            }
        }

        private bool CheckPivots()
        {
            if (_conSO.FindProperty("xminRig").arraySize > 0) return false;
            if (_conSO.FindProperty("xmaxRig").arraySize > 0) return false;

            return true;
        }

        void AddSelections()
        {
            var bones = Selection.GetFiltered<GameObject>(SelectionMode.Editable);
            var boneProps = _conSO.FindProperty("rigTarget");
            var originProps = _conSO.FindProperty("orgRig");
            var xminProps = _conSO.FindProperty("xminRig");
            var xmaxProps = _conSO.FindProperty("xmaxRig");

            Debug.Assert(boneProps.arraySize == originProps.arraySize &&
                         boneProps.arraySize == xminProps.arraySize &&
                         boneProps.arraySize == xmaxProps.arraySize);

            var newSource = new List<GameObject>();
            var beforeAddSize = boneProps.arraySize;

            boneProps.arraySize = beforeAddSize + bones.Length;
            originProps.arraySize = boneProps.arraySize;
            xminProps.arraySize = boneProps.arraySize;
            xmaxProps.arraySize = boneProps.arraySize;

            for (var i = 0; i < beforeAddSize; i++)
            {
                newSource.Add(boneProps.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
            }

            for (int i = 0; i < bones.Length; i++)
            {
                boneProps.GetArrayElementAtIndex(beforeAddSize + i).objectReferenceValue = bones[i];
                newSource.Add(bones[i].gameObject);

                SetPropTransform(originProps, beforeAddSize + i, bones[i].transform);
                SetPropTransform(xminProps, beforeAddSize + i, bones[i].transform);
                SetPropTransform(xmaxProps, beforeAddSize + i, bones[i].transform);
            }

            _conSO.ApplyModifiedProperties();

            rootVisualElement.Q<ListView>("LSTBoneGO").itemsSource = newSource;
            rootVisualElement.Q<ListView>("LSTBoneGO").Rebuild();

            //Record(_conSO.FindProperty("orgRig"));
            BoneControllerStorage.Save();
        }

        void Remove()
        {
            var boneProps = _conSO.FindProperty("rigTarget");
            var originProps = _conSO.FindProperty("orgRig");
            var xminProps = _conSO.FindProperty("xminRig");
            var xmaxProps = _conSO.FindProperty("xmaxRig");
            var listView = rootVisualElement.Q<ListView>("LSTBoneGO");
            var willRemove = listView.selectedIndices.ToList();
            var newList = new List<GameObject>();
            
            if (willRemove.Count == 0) return;

            Debug.Assert(boneProps.arraySize == originProps.arraySize &&
                         boneProps.arraySize == xminProps.arraySize &&
                         boneProps.arraySize == xmaxProps.arraySize);


            for (int i = 0; i < boneProps.arraySize; i++)
            {
                if (willRemove.Contains(i)) continue;
                newList.Add(boneProps.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
            }

            boneProps.arraySize = newList.Count();
            for (int i = 0; i < boneProps.arraySize; i++)
            {
                boneProps.GetArrayElementAtIndex(i).objectReferenceValue = newList[i];
            }
            
            DeleteRiggingEntityWithIndices(originProps, willRemove);
            DeleteRiggingEntityWithIndices(xminProps, willRemove);
            DeleteRiggingEntityWithIndices(xmaxProps, willRemove);

            listView.itemsSource = newList;
            listView.Rebuild();
            _conSO.ApplyModifiedProperties();
            //Record(_conSO.FindProperty("orgRig"));
            BoneControllerStorage.Save();
        }

        void RemoveAll()
        {
            var boneProps = _conSO.FindProperty("rigTarget");
            var originProps = _conSO.FindProperty("orgRig");
            var xminProps = _conSO.FindProperty("xminRig");
            var xmaxProps = _conSO.FindProperty("xmaxRig");
            var listView = rootVisualElement.Q<ListView>("LSTBoneGO");
            boneProps.arraySize = 0;
            originProps.arraySize = 0;
            xminProps.arraySize = 0;
            xmaxProps.arraySize = 0;
            listView.itemsSource = new List<GameObject>();
            listView.Rebuild();
            _conSO.ApplyModifiedProperties();
            BoneControllerStorage.Save();
        }

        void DeleteRiggingEntityWithIndices(SerializedProperty prop, List<int> indices)
        {
            var newREList = new List<RiggingEntity>();
            for (var i = 0; i < prop.arraySize; i++)
            {
                if (indices.Contains(i)) continue;
                var tempRE = new RiggingEntity();
                tempRE.position = prop.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value;
                tempRE.scale = prop.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value;
                tempRE.rotation = prop.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue;
                newREList.Add(tempRE);
            }

            prop.arraySize = newREList.Count;

            for (var i = 0; i < prop.arraySize; i++)
            {
                prop.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value = newREList[i].position;
                prop.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value= newREList[i].scale;
                prop.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue = newREList[i].rotation;
            }
        }
    }
}