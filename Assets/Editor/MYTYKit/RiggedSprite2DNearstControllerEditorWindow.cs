using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class RiggedSprite2DNearstControllerEditorWindow : EditorWindow
{
    public VisualTreeAsset UITemplate;

    SerializedObject m_conSO;
    bool m_isPressed = false;
    
    [UnityEditor.MenuItem("MYTY Kit/Controller/Rigged Sprite 2D Nearst Controller", false, 20)]
    public static void ShowController()
    {
        var wnd = GetWindow<RiggedSprite2DNearstControllerEditorWindow>();
        wnd.titleContent = new GUIContent("Rigged Sprite 2D Nearst Controller");
    }
    
    void CreateGUI()
    {
        UITemplate.CloneTree(rootVisualElement);
        var selectedGOs = Selection.GetFiltered<RiggedSprite2DNearstController>(SelectionMode.Editable);
        var conVE = rootVisualElement.Q<ObjectField>("OBJController");
        var listView = rootVisualElement.Q<ListView>("LSTRiggingGO");
        var addBtn = rootVisualElement.Q<Button>("BTNAdd");
        var removeBtn = rootVisualElement.Q<Button>("BTNRemove");
        var removeAllBtn = rootVisualElement.Q<Button>("BTNRemoveAll");
        var makePivotBtn = rootVisualElement.Q<Button>("BTNPivot");
        var removePivotBtn = rootVisualElement.Q<Button>("BTNPivotRemove");
        var copyPosBtn = rootVisualElement.Q<Button>("BTNCopyPos");
        
        addBtn.clicked += OnAdd;
        removeBtn.clicked += OnRemove;
        removeAllBtn.clicked += OnRemoveAll;
        makePivotBtn.clicked += OnMakePivot;
        removePivotBtn.clicked += OnRemovePivot;
        copyPosBtn.clicked += OnCopyPos;
        
        conVE.objectType = typeof(RiggedSprite2DNearstController);
        listView.makeItem = () =>
        {
            return new ObjectField();
        };
        
        listView.bindItem = (e, i) =>
        {
            (e as ObjectField).value = listView.itemsSource[i] as GameObject;
            (e as ObjectField).AddToClassList("noEditableObjField");
        };
        
        conVE.RegisterValueChangedCallback((ChangeEvent<Object> e) =>
        {
            InitWithController(e.newValue as RiggedSprite2DNearstController);
        
        });
        
        rootVisualElement.Q<Vector2Field>("VEC2BL").RegisterValueChangedCallback((ChangeEvent<Vector2> e) => SetPanelCoord());
        rootVisualElement.Q<Vector2Field>("VEC2TR").RegisterValueChangedCallback((ChangeEvent<Vector2> e) => SetPanelCoord());
        rootVisualElement.Q<Vector2Field>("VEC2Value").RegisterValueChangedCallback((ChangeEvent<Vector2> e) => UpdateIndicator());
        UpdatePanelConfig();
        if (selectedGOs.Length == 0) return;
        
        InitWithController(selectedGOs[0]);
    }

    void InitWithController(RiggedSprite2DNearstController controller)
    {
        m_conSO = new SerializedObject(controller);

        var conVE = rootVisualElement.Q<ObjectField>("OBJController");
        var listView = rootVisualElement.Q<ListView>("LSTRiggingGO");
        var listSource = new List<GameObject>();
        var anchorToggle = rootVisualElement.Q<Toggle>("TGLAnchor");
        var targetProps = m_conSO.FindProperty("rigTarget");
        var pivotPanel = rootVisualElement.Q<VisualElement>("VEPivot");
        var pointPanel = rootVisualElement.Q<VisualElement>("VEPanel");

        for (int i = 0; i < targetProps.arraySize; i++)
        {
            listSource.Add(targetProps.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
        }

        listView.itemsSource = listSource;
        listView.Rebuild();

        conVE.value = controller;
        

        rootVisualElement.Q<Vector2Field>("VEC2BL").BindProperty(m_conSO.FindProperty("bottomLeft"));
        rootVisualElement.Q<Vector2Field>("VEC2TR").BindProperty(m_conSO.FindProperty("topRight"));
        rootVisualElement.Q<Vector2Field>("VEC2Value").BindProperty(m_conSO.FindProperty("controlPosition"));
        
        var panel = rootVisualElement.Q<VisualElement>("VEClickArea");
        panel.RegisterCallback<MouseDownEvent>(OnMousePress);
        panel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        panel.RegisterCallback((MouseUpEvent e) => { m_isPressed = false; });
        panel.RegisterCallback((MouseLeaveEvent e) => { m_isPressed = false; });

        SetPanelCoord();
        
        anchorToggle.SetValueWithoutNotify(!IsPivotEmpty());
        anchorToggle.RegisterValueChangedCallback(OnAnchorToggled);
        UpdatePanelConfig();
        pivotPanel.RegisterCallback<GeometryChangedEvent>( evt => SyncPivotPosition());
        pointPanel.RegisterCallback<GeometryChangedEvent>(evt => UpdateIndicator());
        UpdatePivotList();
    }
    
    void SetPanelCoord()
    {
        var bl = rootVisualElement.Q<Vector2Field>("VEC2BL").value;
        var tr = rootVisualElement.Q<Vector2Field>("VEC2TR").value;
        rootVisualElement.Q<Label>("LBLBL").text = bl.x + ", " + bl.y;
        rootVisualElement.Q<Label>("LBLTR").text = tr.x + ", " + tr.y;
        SyncPivotPosition();
        UpdateIndicator();
    }

    bool IsPivotEmpty()
    {
        Debug.Assert(m_conSO!=null);
        if (m_conSO.FindProperty("orgRig").arraySize > 0) return false;
        return true;
    }
    void FindChild(GameObject parent, List<GameObject> objList)
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

    void OnAnchorToggled(ChangeEvent<bool> e)
    {
        if (m_conSO == null) return;
        UpdatePanelConfig();
        if (e.newValue)
        {
            RegisterAnchor();
        }
        else
        {
            ClearAnchor();
        }
    }

    void RegisterAnchor()
    {
        var anchorToggle = rootVisualElement.Q<Toggle>("TGLAnchor");
        if (!IsPivotEmpty())
        {
            EditorUtility.DisplayDialog("MYTY Kit", "Reset all pivots first.", "Ok");
            return;
        }
        
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
            originalProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value = go.transform.localScale;
        }
        
        m_conSO.ApplyModifiedProperties();
    }

    void ClearAnchor()
    {
        var targetsProp = m_conSO.FindProperty("rigTarget");

        if (targetsProp.arraySize == 0)
        {
            return;
        }
        var controller = m_conSO.targetObject as RiggedSprite2DNearstController;

        for (var i = 0; i < controller.rigTarget.Count; i++)
        {
            controller.rigTarget[i].transform.localPosition = controller.orgRig[i].position;
            controller.rigTarget[i].transform.localScale = controller.orgRig[i].scale;
            controller.rigTarget[i].transform.localRotation = controller.orgRig[i].rotation;
        }
        
        var originalProp = m_conSO.FindProperty("orgRig");
        var pivotsProp = m_conSO.FindProperty("pivots");
        originalProp.arraySize = 0;
        pivotsProp.arraySize = 0;
        
        m_conSO.ApplyModifiedProperties();
        UpdatePivotList();

    }
    void OnAdd()
    {
        var targetsProp = m_conSO.FindProperty("rigTarget");
        var targets = Selection.GetFiltered<GameObject>(SelectionMode.Editable);
        var offset = targetsProp.arraySize;
        var isRecursive = rootVisualElement.Q<Toggle>("CHKRecursive").value;

        if (!IsPivotEmpty())
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

        rootVisualElement.Q<ListView>("LSTRiggingGO").itemsSource = newSource;
        rootVisualElement.Q<ListView>("LSTRiggingGO").Rebuild();
        
        m_conSO.FindProperty("orgRig").arraySize = 0;

        m_conSO.ApplyModifiedProperties();
    }

    void OnRemove()
    {
        var targetsProp = m_conSO.FindProperty("rigTarget");
        var listView = rootVisualElement.Q<ListView>("LSTRiggingGO");
        var willRemove = listView.selectedIndices.ToList();
        var newList = new List<GameObject>();
        if (willRemove.Count == 0) return;
        if (!IsPivotEmpty())
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
        m_conSO.FindProperty("orgRig").arraySize = 0;

        m_conSO.ApplyModifiedProperties();
    }

    void OnRemoveAll()
    {
        if (!IsPivotEmpty())
        {
            EditorUtility.DisplayDialog("MYTY Kit", "Reset all pivots first.", "Ok");
            return;
        }
        var targetsProp = m_conSO.FindProperty("rigTarget");
        var listView = rootVisualElement.Q<ListView>("LSTRiggingGO");
        targetsProp.arraySize = 0;
        listView.itemsSource = new List<GameObject>();
        listView.Rebuild();
        m_conSO.FindProperty("orgRig").arraySize = 0;
        m_conSO.ApplyModifiedProperties();
    }

    void OnRemovePivot()
    {
        var listView = rootVisualElement.Q<ListView>("LSTPivot");
        var pivotsProp = m_conSO.FindProperty("pivots");
        var selection = listView.selectedIndex;
        if (selection < 0) return;
        pivotsProp.DeleteArrayElementAtIndex(selection);
        m_conSO.ApplyModifiedProperties();
        UpdatePivotList();
    }   

    void OnMousePress(MouseDownEvent e)
    {
        m_isPressed = true;
        HandleMousePos(e.localMousePosition);
    }

    void OnMouseMove(MouseMoveEvent e)
    {
        if (!m_isPressed) return;
        HandleMousePos(e.localMousePosition);
    }

    void HandleMousePos(Vector2 mousePos)
    {
        var cpProp = m_conSO.FindProperty("controlPosition");
        var tr = m_conSO.FindProperty("topRight").vector2Value;
        var bl = m_conSO.FindProperty("bottomLeft").vector2Value;
        var pointPanel = rootVisualElement.Q<VisualElement>("VEPanel");
        var panelDim = new Vector2(pointPanel.localBound.width, pointPanel.localBound.height);
        mousePos.y = panelDim.y - mousePos.y;
        
        var result = (mousePos / panelDim) * (tr - bl) + bl;

        result = new Vector2(
            Mathf.Round(result.x * 100) / 100,
            Mathf.Round(result.y * 100) / 100);
        cpProp.vector2Value = result;
        
        m_conSO.ApplyModifiedProperties();
    }

    void OnCopyPos()
    {
        rootVisualElement.Q<Vector2Field>("VEC2PivotPos").value =
            rootVisualElement.Q<Vector2Field>("VEC2Value").value;
    }
    void OnMakePivot()
    {
        if (IsPivotEmpty())
        {
            EditorUtility.DisplayDialog("MYTY Kit", "No anchored pivots", "Ok");
            return;
        }
        var targetsProp = m_conSO.FindProperty("rigTarget");
        var pivotsProp = m_conSO.FindProperty("pivots");

        pivotsProp.arraySize++;

        var index = pivotsProp.arraySize - 1;
        var elemProp = pivotsProp.GetArrayElementAtIndex(index);

        elemProp.FindPropertyRelative("name").stringValue = rootVisualElement.Q<TextField>("TXTPivotName").text;
        elemProp.FindPropertyRelative("position").vector2Value = rootVisualElement.Q<Vector2Field>("VEC2PivotPos").value;
        var rsProps = elemProp.FindPropertyRelative("riggingState");

        rsProps.arraySize = targetsProp.arraySize;

        for (int i = 0; i < rsProps.arraySize; i++)
        {
            var go = targetsProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
            rsProps.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value =
                go.transform.localPosition;
            rsProps.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue =
                go.transform.localRotation;
            rsProps.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value = go.transform.localScale;
        }
        
        m_conSO.ApplyModifiedProperties();
        UpdatePivotList();
    }

    void UpdatePivotList()
    {
        var listView = rootVisualElement.Q<ListView>("LSTPivot");
        var pivotsProp = m_conSO.FindProperty("pivots");

        var nameList = new List<string>();
        for (int i = 0; i < pivotsProp.arraySize; i++)
        {
            nameList.Add(pivotsProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue);
        }

        listView.itemsSource = nameList;
        listView.Rebuild();
        SyncPivotPosition();
        
    }

    void SyncPivotPosition()
    {
        var panel = rootVisualElement.Q<VisualElement>("VEPivot");
        var pivotsProp = m_conSO.FindProperty("pivots");
        
        panel.Clear();

        var panelDim = new Vector2(panel.localBound.width, panel.localBound.height);
        
        for (var i = 0; i < pivotsProp.arraySize; i++)
        {
            var name = pivotsProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
            var position = pivotsProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector2Value;
            var group = new VisualElement();
            var pivotText = new Label();
            var pivotPoint = new Label();
            pivotText.text = name;
            pivotText.AddToClassList("pivotText");
            pivotPoint.AddToClassList("pivotPoint");
            group.style.position = new StyleEnum<Position>(Position.Absolute);

            var panelPos = CalcPanelPos(position, panelDim);
            
            group.style.left = new StyleLength(new Length(panelPos.x - 5, LengthUnit.Pixel));
            group.style.bottom = new StyleLength(new Length(panelPos.y - 5, LengthUnit.Pixel));
            group.Add(pivotText);
            group.Add(pivotPoint);
            panel.Add(group);
        }
    }

    void UpdateIndicator()
    {
        var controlPosition = m_conSO.FindProperty("controlPosition").vector2Value;
        var pointPanel = rootVisualElement.Q<VisualElement>("VEPanel");
        var pointerVE = rootVisualElement.Q<VisualElement>("VEPointer");
        var panelDim = new Vector2(pointPanel.localBound.width, pointPanel.localBound.height);
        var panelPos = CalcPanelPos(controlPosition, panelDim);

        pointerVE.style.left = panelPos.x-5;
        pointerVE.style.bottom = panelPos.y-5;

        UpdateController();
    }

    void UpdateController()
    {
        if (IsPivotEmpty()) return;
        var controller = m_conSO.targetObject as RiggedSprite2DNearstController;

        for (var i = 0; i < controller.rigTarget.Count; i++)
        {
            controller.rigTarget[i].transform.localPosition = controller.orgRig[i].position;
            controller.rigTarget[i].transform.localScale = controller.orgRig[i].scale;
            controller.rigTarget[i].transform.localRotation = controller.orgRig[i].rotation;
        }
        
        controller.Update();
        controller.ApplyDiff();
    }

    void UpdatePanelConfig()
    {
        var anchorToggle = rootVisualElement.Q<Toggle>("TGLAnchor");
        var beforePanel = rootVisualElement.Q<VisualElement>("VEBeforeAnchor");
        var afterPanel = rootVisualElement.Q<VisualElement>("VEAfterAnchor");

        beforePanel.style.display = anchorToggle.value ? DisplayStyle.None : DisplayStyle.Flex;
        afterPanel.style.display = anchorToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
    }
    Vector2 CalcPanelPos(Vector2 value, Vector2 panelDim)
    {
        var tr = m_conSO.FindProperty("topRight").vector2Value;
        var bl = m_conSO.FindProperty("bottomLeft").vector2Value;

        var norm = (value - bl) / (tr-bl);
        return panelDim * norm;
    }
}
