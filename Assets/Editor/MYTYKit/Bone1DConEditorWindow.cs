using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;

public class Bone1DConEditorWindow : EditorWindow
{
    public VisualTreeAsset UITemplate;
    private SerializedObject _conSO;

    [MenuItem("MYTY Kit/Controller/Bone 1D Controller", false, 20)]
    public static void ShowController()
    {
        var wnd = GetWindow<Bone1DConEditorWindow>();
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
        listView.makeItem = () =>
        {
            var objItem = new ObjectField();

            return new ObjectField();
        };

        listView.bindItem = (e, i) =>
        {
            (e as ObjectField).value = listView.itemsSource[i] as GameObject;
            (e as ObjectField).AddToClassList("noEditableObjField");
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

        minVE.RegisterValueChangedCallback((ChangeEvent<float> e) =>
        {
            RescaleSlider();
        });

        maxVE.RegisterValueChangedCallback((ChangeEvent<float> e) =>
        {
            RescaleSlider();
        });

        minVE.RegisterValueChangedCallback((ChangeEvent<float> e) =>
        {
            RescaleSlider();
        });

        maxVE.RegisterValueChangedCallback((ChangeEvent<float> e) =>
        {
            RescaleSlider();
        });

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
            prop.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value = obj.transform.localPosition;
            prop.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue = obj.transform.localRotation;
            prop.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value = obj.transform.localScale;
        }

        _conSO.ApplyModifiedProperties();
    }

    private bool HandleRigToggle(MouseUpEvent e, string prop)
    {
        var elem = e.target as Toggle;
        var result = true;
        if (elem.value)
        {
            Record(_conSO.FindProperty(prop));
        }
        else
        {
            var target = _conSO.targetObject as Bone1DController;
            target.ToOrigin();
            _conSO.FindProperty(prop).arraySize = 0;
            _conSO.ApplyModifiedProperties();
            result = false;
        }
        SyncRiggingStatus();
        return result;
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
        var newSource = new List<GameObject>();
        if (!CheckPivots())
        {
            EditorUtility.DisplayDialog("MYTY Kit", "Reset all pivots first.", "Ok");
            return;
        }

        boneProps.arraySize = bones.Length;
        for (int i = 0; i < bones.Length; i++)
        {
            boneProps.GetArrayElementAtIndex(i).objectReferenceValue = bones[i];
            newSource.Add(bones[i].gameObject);
        }

        _conSO.ApplyModifiedProperties();

        rootVisualElement.Q<ListView>("LSTBoneGO").itemsSource = newSource;
        rootVisualElement.Q<ListView>("LSTBoneGO").Rebuild();

        Record(_conSO.FindProperty("orgRig"));
    }

    void Remove()
    {
        var boneProps = _conSO.FindProperty("rigTarget");
        var listView = rootVisualElement.Q<ListView>("LSTBoneGO");
        var willRemove = listView.selectedIndices.ToList();
        var newList = new List<GameObject>();
        if (willRemove.Count == 0) return;
        if (!CheckPivots())
        {
            EditorUtility.DisplayDialog("MYTY Kit", "Reset all pivots first.", "Ok");
            return;
        }


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

        listView.itemsSource = newList;
        listView.Rebuild();
        _conSO.ApplyModifiedProperties();
        Record(_conSO.FindProperty("orgRig"));
    }

    void RemoveAll()
    {
        if (!CheckPivots())
        {
            EditorUtility.DisplayDialog("MYTY Kit", "Reset all pivots first.", "Ok");
            return;
        }
        var boneProps = _conSO.FindProperty("rigTarget");
        var listView = rootVisualElement.Q<ListView>("LSTBoneGO");
        boneProps.arraySize = 0;
        listView.itemsSource = new List<GameObject>();
        listView.Rebuild();
        _conSO.ApplyModifiedProperties();
    }
}
