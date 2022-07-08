using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


public class Bone2DControllerEditorWindow : EditorWindow
{
    public VisualTreeAsset UITemplate;

    bool _isPressed = false;
    Vector2 _lastPos = new();
    SerializedObject faceConSO;

    [MenuItem("MYTY Kit/Bone 2D Controller", false, 0)]
    public static void ShowController()
    {
        var wnd = GetWindow<Bone2DControllerEditorWindow>();
        wnd.titleContent = new GUIContent("Bone 2D Controller");
    }

    private void CreateGUI()
    {
        UITemplate.CloneTree(rootVisualElement);
        var selectedObjs = Selection.GetFiltered<Bone2DController>(SelectionMode.Editable);
        var fcObjField = rootVisualElement.Q<ObjectField>("Bone2DController");
        var listView = rootVisualElement.Q<ListView>("TargetList");
        var addBtn = rootVisualElement.Q<Button>("BTNAddTarget");
        var removeSelectionBtn = rootVisualElement.Q<Button>("BTNRemoveSelection");
        var removeAllBtn = rootVisualElement.Q<Button>("BTNRemoveAll");
        var btnReset = rootVisualElement.Q<Button>("BTNReset");
        var posController = rootVisualElement.Q<Vector2Field>("ControllerPos");
        var controller = rootVisualElement.Q("Controller");
        var panel = controller.parent;
        btnReset.clicked += ResetPos;

        fcObjField.objectType = typeof(Bone2DController);
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

        addBtn.clicked += AddTarget;
        removeSelectionBtn.clicked += RemoveSelection;
        removeAllBtn.clicked += RemoveAll;

        fcObjField.RegisterValueChangedCallback((ChangeEvent<Object> e) =>
        {
            InitializeWithFaceCon((Bone2DController)e.newValue);

        });

        posController.RegisterValueChangedCallback((ChangeEvent<Vector2> e) =>
        {
            if (faceConSO == null) return;
            var target = (Bone2DController)faceConSO.targetObject;
            controller.transform.position = e.newValue * new Vector2(panel.localBound.width / 2, panel.localBound.height / 2);
            target.InterpolateGUI();

        });

        panel.RegisterCallback<MouseDownEvent>(OnMousePress, TrickleDown.TrickleDown);
        panel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        panel.RegisterCallback((MouseUpEvent e) =>
        {
            _isPressed = false;
        });
        panel.RegisterCallback((MouseLeaveEvent e) =>
        {
            _isPressed = false;
        });

        rootVisualElement.Q<Toggle>("BTNRegOrigin").RegisterCallback((MouseUpEvent e) =>
        {
            ResetPos();
            HandleRigToggle(e, "orgRig");


        });
        rootVisualElement.Q<Toggle>("BTNRegLeft").RegisterCallback((MouseUpEvent e) =>
        {
            if (HandleRigToggle(e, "xminRig"))
            {
                SetControlPos(new Vector2(-1, 0));
            }
        });
        rootVisualElement.Q<Toggle>("BTNRegRight").RegisterCallback((MouseUpEvent e) =>
        {
            if (HandleRigToggle(e, "xmaxRig"))
            {
                SetControlPos(new Vector2(1, 0));
            }
        });
        rootVisualElement.Q<Toggle>("BTNRegUp").RegisterCallback((MouseUpEvent e) =>
        {
            if (HandleRigToggle(e, "yminRig"))
            {
                SetControlPos(new Vector2(0, -1));
            }
        });
        rootVisualElement.Q<Toggle>("BTNRegDown").RegisterCallback((MouseUpEvent e) =>
        {
            if (HandleRigToggle(e, "ymaxRig"))
            {
                SetControlPos(new Vector2(0, 1));
            }
        });



        if (selectedObjs.Length == 0) return;
       
        InitializeWithFaceCon(selectedObjs[0]);
    }

    private void InitializeWithFaceCon(Bone2DController obj)
    {
        faceConSO = new SerializedObject(obj);

        var fcObjField = rootVisualElement.Q<ObjectField>("Bone2DController");
        var targetsProp = faceConSO.FindProperty("rigTarget");
        var posController = rootVisualElement.Q<Vector2Field>("ControllerPos");
        var controlPosProp = faceConSO.FindProperty("controlPosition");

        posController.BindProperty(controlPosProp);
        fcObjField.value = obj;

        List<GameObject> targetObjs = new();

        for (int i = 0; i < targetsProp.arraySize; i++)
        {
            targetObjs.Add(targetsProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
        }

        rootVisualElement.Q<ListView>("TargetList").itemsSource = targetObjs;
        rootVisualElement.Q<ListView>("TargetList").Rebuild();


        SyncRiggingStatus();

    }

    private bool HandleRigToggle(MouseUpEvent e, string prop)
    {
        if (faceConSO == null) return false;
        var elem = e.target as Toggle;
        var result = true;
        if (elem.value)
        {
            Record(faceConSO.FindProperty(prop));
        }
        else
        {
            faceConSO.FindProperty(prop).arraySize = 0;
            faceConSO.ApplyModifiedProperties();
            result = false;
        }
        SyncRiggingStatus();
        return result;
    }

    private void SyncRiggingStatus()
    {
        SyncRiggingHelper("orgRig", "OriginMarker", "BTNRegOrigin");
        SyncRiggingHelper("xminRig", "LeftMarker", "BTNRegLeft");
        SyncRiggingHelper("xmaxRig", "RightMarker", "BTNRegRight");
        SyncRiggingHelper("yminRig", "UpMarker", "BTNRegUp");
        SyncRiggingHelper("ymaxRig", "DownMarker", "BTNRegDown");
    }

    private void SyncRiggingHelper(string property, string element, string btnElem)
    {
        if (faceConSO.FindProperty(property).arraySize > 0)
        {
            rootVisualElement.Q(element).RemoveFromClassList("markerUnset");
            rootVisualElement.Q(element).AddToClassList("markerSet");
            rootVisualElement.Q<Toggle>(btnElem).value = true;
        }
        else
        {
            rootVisualElement.Q(element).RemoveFromClassList("markerSet");
            rootVisualElement.Q(element).AddToClassList("markerUnset");
            rootVisualElement.Q<Toggle>(btnElem).value = false;
        }
    }

    private bool CheckPivots()
    {
        if (faceConSO.FindProperty("orgRig").arraySize > 0) return false;
        if (faceConSO.FindProperty("xminRig").arraySize > 0) return false;
        if (faceConSO.FindProperty("xmaxRig").arraySize > 0) return false;
        if (faceConSO.FindProperty("yminRig").arraySize > 0) return false;
        if (faceConSO.FindProperty("ymaxRig").arraySize > 0) return false;
        return true;
    }

    private void AddTarget()
    {
        var targetsProp = faceConSO.FindProperty("rigTarget");
        var targets = Selection.GetFiltered<GameObject>(SelectionMode.Editable);
        var offset = targetsProp.arraySize;
        var isRecursive = rootVisualElement.Q<Toggle>("CHKRecursive").value;

        if (!CheckPivots())
        {
            EditorUtility.DisplayDialog("MYTY Kit", "Reset all pivots first.", "Ok");
            return;
        }

        if (isRecursive)
        {
            var objList = new List<GameObject>();
            foreach (var target in targets)
            {
                var childList = new List<GameObject>();
                FindChild(target, childList);
                objList = (objList.Concat(childList)).ToList();

            }
            targets = objList.ToArray();

        }

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

        rootVisualElement.Q<ListView>("TargetList").itemsSource = newSource;
        rootVisualElement.Q<ListView>("TargetList").Rebuild();

        faceConSO.FindProperty("xminRig").arraySize = 0;
        faceConSO.FindProperty("xmaxRig").arraySize = 0;
        faceConSO.FindProperty("yminRig").arraySize = 0;
        faceConSO.FindProperty("ymaxRig").arraySize = 0;
        faceConSO.FindProperty("orgRig").arraySize = 0;

        faceConSO.ApplyModifiedProperties();
    }

    private void RemoveSelection()
    {
        var targetsProp = faceConSO.FindProperty("rigTarget");
        var listView = rootVisualElement.Q<ListView>("TargetList");
        var willRemove = listView.selectedIndices.ToList();
        var newList = new List<GameObject>();
        if (willRemove.Count == 0) return;
        if (!CheckPivots())
        {
            EditorUtility.DisplayDialog("MYTY Kit", "Reset all pivots first.", "Ok");
            return;
        }

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

        faceConSO.ApplyModifiedProperties();
    }

    private void RemoveAll()
    {
        if (!CheckPivots())
        {
            EditorUtility.DisplayDialog("MYTY Kit", "Reset all pivots first.", "Ok");
            return;
        }
        var targetsProp = faceConSO.FindProperty("rigTarget");
        var listView = rootVisualElement.Q<ListView>("TargetList");
        targetsProp.arraySize = 0;
        listView.itemsSource = new List<GameObject>();
        listView.Rebuild();
        faceConSO.ApplyModifiedProperties();
    }

    private void ResetPos()
    {
        SetControlPos(new Vector2());
    }
    private void SetControlPos(Vector2 pos)
    {
        if (faceConSO != null && faceConSO.targetObject != null)
        {
            var target = (Bone2DController)faceConSO.targetObject;
            faceConSO.FindProperty("controlPosition").vector2Value = pos;
            faceConSO.ApplyModifiedProperties();
            target.InterpolateGUI();
        }
    }

    private void FindChild(GameObject parent, List<GameObject> objList)
    {
        objList.Add(parent);
        if (parent.transform.childCount > 0)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                FindChild(parent.transform.GetChild(i).gameObject, objList);
            }
        }


    }


    private void Record(SerializedProperty prop)
    {
        var rigProp = faceConSO.FindProperty("rigTarget");
        prop.arraySize = rigProp.arraySize;
        for (int i = 0; i < rigProp.arraySize; i++)
        {
            var obj = (GameObject)rigProp.GetArrayElementAtIndex(i).objectReferenceValue;
            prop.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value = obj.transform.localPosition;
            prop.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue = obj.transform.localRotation;
            prop.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value = obj.transform.localScale;
        }

        faceConSO.ApplyModifiedProperties();
    }

    private void OnMousePress(MouseDownEvent e)
    {
        var controller = rootVisualElement.Q("Controller");
        var panel = controller.parent;
        _isPressed = true;
        _lastPos = e.localMousePosition;
        var posController = rootVisualElement.Q<Vector2Field>("ControllerPos");
        posController.value = new Vector2(_lastPos.x / panel.localBound.width * 2 - 1, _lastPos.y / panel.localBound.height * 2 - 1);


    }

    private void OnMouseMove(MouseMoveEvent e)
    {
        if (_isPressed)
        {
            var diff = e.localMousePosition - _lastPos;
            _lastPos = e.localMousePosition;
            var controller = rootVisualElement.Q("Controller");
            var panel = controller.parent;
            var pos = controller.transform.position += new Vector3(diff.x, diff.y, 0);

            if (pos.x < -panel.localBound.width / 2) pos.Set(-panel.localBound.width / 2, pos.y, pos.z);
            else if (pos.x > panel.localBound.width / 2) pos.Set(panel.localBound.width / 2, pos.y, pos.z);

            if (pos.y < -panel.localBound.height / 2) pos.Set(pos.x, -panel.localBound.height / 2, pos.z);
            else if (pos.y > panel.localBound.height / 2) pos.Set(pos.x, panel.localBound.height / 2, pos.z);

            controller.transform.position = pos;

            var posController = rootVisualElement.Q<Vector2Field>("ControllerPos");
            posController.value = new Vector2(pos.x / panel.localBound.width * 2, pos.y / panel.localBound.height * 2);


        }
    }

    private void OnDestroy()
    {
        ResetPos();

        Debug.Log("destroy");
    }

}
