using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.U2D.Animation;

public class Sprite2DNearstConEditorWindowMSR : EditorWindow
{
    public VisualTreeAsset UITemplate;


    private SerializedObject _conSO;
    private bool _isPressed = false;
    private Vector2 _lastPos = new();

    [MenuItem("MYTY Kit/Controller/Sprite 2D Nearst Controller MSR", false, 20)]
    public static void ShowController()
    {
        var wnd = GetWindow<Sprite2DNearstConEditorWindowMSR>();
        wnd.titleContent = new GUIContent("Sprite 2D Nearst Controller");
    }

    private void CreateGUI()
    {
        UITemplate.CloneTree(rootVisualElement);
        var selectedGOs = Selection.GetFiltered<Sprite2DNearstControllerMSR>(SelectionMode.Editable);
        var conVE = rootVisualElement.Q<ObjectField>("OBJController");
        var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
        var addBtn = rootVisualElement.Q<Button>("BTNAdd");
        var removeBtn = rootVisualElement.Q<Button>("BTNRemove");
        var removeAllBtn = rootVisualElement.Q<Button>("BTNRemoveAll");


        addBtn.clicked += AddSelections;
        removeBtn.clicked += Remove;
        removeAllBtn.clicked += RemoveAll;

        conVE.objectType = typeof(Sprite2DNearstControllerMSR);
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
            InitWithController(e.newValue as Sprite2DNearstControllerMSR);

        });

        rootVisualElement.Q<Vector2Field>("VEC2BL").RegisterValueChangedCallback((ChangeEvent<Vector2> e) =>
        {
            SetPanelCoord();
        });

        rootVisualElement.Q<Vector2Field>("VEC2TR").RegisterValueChangedCallback((ChangeEvent<Vector2> e) =>
        {
            SetPanelCoord();
        });
        rootVisualElement.Q<Vector2Field>("VEC2Value").RegisterValueChangedCallback((ChangeEvent<Vector2> e) =>
        {
            if (_conSO == null) return;
            var controller = _conSO.targetObject as Sprite2DNearstControllerMSR;
            if (controller == null) return;
            controller.UpdateLabel();
        });

        rootVisualElement.Q<Button>("BTNAutoSetup").clicked += AutoSetup;

        if (selectedGOs.Length == 0) return;

        InitWithController(selectedGOs[0]);
    }

    void InitWithController(Sprite2DNearstControllerMSR controller)
    {
        _conSO = new SerializedObject(controller);

        var conVE = rootVisualElement.Q<ObjectField>("OBJController");
        var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
        var listSource = new List<GameObject>();

        var spritesProps = _conSO.FindProperty("spriteObjects");
        for (int i = 0; i < spritesProps.arraySize; i++)
        {
            if (spritesProps.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                listSource.Add(null);
            }
            else listSource.Add((spritesProps.GetArrayElementAtIndex(i).objectReferenceValue as MYTYSpriteResolver).gameObject);
        }

        listView.itemsSource = listSource;
        listView.Rebuild();

        conVE.value = controller;
        

        rootVisualElement.Q<Vector2Field>("VEC2BL").BindProperty(_conSO.FindProperty("bottomLeft"));
        rootVisualElement.Q<Vector2Field>("VEC2TR").BindProperty(_conSO.FindProperty("topRight"));
        rootVisualElement.Q<Vector2Field>("VEC2Value").BindProperty(_conSO.FindProperty("value"));
        rootVisualElement.Q<PropertyField>("PRPLabel2D").BindProperty(_conSO.FindProperty("labels"));

        var panel = rootVisualElement.Q<VisualElement>("VEPanel");
        panel.RegisterCallback<MouseDownEvent>(OnMousePress);
        panel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        panel.RegisterCallback((MouseUpEvent e) => { _isPressed = false; });
        panel.RegisterCallback((MouseLeaveEvent e) => { _isPressed = false; });

        SetPanelCoord();
    }

    void SetPanelCoord()
    {
        var bl = rootVisualElement.Q<Vector2Field>("VEC2BL").value;
        var tr = rootVisualElement.Q<Vector2Field>("VEC2TR").value;
        rootVisualElement.Q<Label>("LBLBL").text = bl.x + ", " + bl.y;
        rootVisualElement.Q<Label>("LBLTR").text = tr.x + ", " + tr.y;
    }

    void AddSelections()
    {
        var sprites = Selection.GetFiltered<MYTYSpriteResolver>(SelectionMode.Editable);
        var spritesProps = _conSO.FindProperty("spriteObjects");
        var newSource = new List<GameObject>();

        spritesProps.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            spritesProps.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            newSource.Add(sprites[i].gameObject);
        }

        _conSO.ApplyModifiedProperties();

        rootVisualElement.Q<ListView>("LSTSpriteGO").itemsSource = newSource;
        rootVisualElement.Q<ListView>("LSTSpriteGO").Rebuild();
    }

    void Remove()
    {
        var spritesProps = _conSO.FindProperty("spriteObjects");
        var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
        var willRemove = listView.selectedIndices.ToList();
        var newList = new List<MYTYSpriteResolver>();
        var newSource = new List<GameObject>();
        if (willRemove.Count == 0) return;

        for (int i = 0; i < spritesProps.arraySize; i++)
        {
            if (willRemove.Contains(i)) continue;
            newList.Add(spritesProps.GetArrayElementAtIndex(i).objectReferenceValue as MYTYSpriteResolver);
        }

        spritesProps.arraySize = newList.Count();
        for (int i = 0; i < spritesProps.arraySize; i++)
        {
            spritesProps.GetArrayElementAtIndex(i).objectReferenceValue = newList[i];
            newSource.Add(newList[i].gameObject);
        }

        listView.itemsSource = newSource;
        listView.Rebuild();
        _conSO.ApplyModifiedProperties();
    }

    void RemoveAll()
    {
        var spritesProps = _conSO.FindProperty("spriteObjects");
        var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
        spritesProps.arraySize = 0;
        listView.itemsSource = new List<GameObject>();
        listView.Rebuild();
        _conSO.ApplyModifiedProperties();
    }

    private void OnMousePress(MouseDownEvent e)
    {
        var bl = rootVisualElement.Q<Vector2Field>("VEC2BL").value;
        var tr = rootVisualElement.Q<Vector2Field>("VEC2TR").value;
        var pointer = rootVisualElement.Q("VEPointer");
        var panel = pointer.parent;
        _isPressed = true;
        _lastPos = e.localMousePosition;
        var posController = rootVisualElement.Q<Vector2Field>("VEC2Value");
        Debug.Log(_lastPos + " " + pointer.transform.position);
        pointer.transform.position = new Vector2(e.localMousePosition.x - panel.localBound.width / 2, e.localMousePosition.y - panel.localBound.height / 2);
        posController.value = new Vector2(bl.x + (tr.x - bl.x) * _lastPos.x / panel.localBound.width,
            bl.y + (tr.y - bl.y) * (1 - _lastPos.y / panel.localBound.height));

    }

    private void OnMouseMove(MouseMoveEvent e)
    {
        if (_isPressed)
        {
            var bl = rootVisualElement.Q<Vector2Field>("VEC2BL").value;
            var tr = rootVisualElement.Q<Vector2Field>("VEC2TR").value;
            var diff = e.localMousePosition - _lastPos;
            _lastPos = e.localMousePosition;
            var pointer = rootVisualElement.Q("VEPointer");
            var panel = pointer.parent;
            var pos = pointer.transform.position += new Vector3(diff.x, diff.y, 0);


            if (pos.x < -panel.localBound.width / 2) pos.Set(-panel.localBound.width / 2, pos.y, pos.z);
            else if (pos.x > panel.localBound.width / 2) pos.Set(panel.localBound.width / 2, pos.y, pos.z);

            if (pos.y < -panel.localBound.height / 2) pos.Set(pos.x, -panel.localBound.height / 2, pos.z);
            else if (pos.y > panel.localBound.height / 2) pos.Set(pos.x, panel.localBound.height / 2, pos.z);

            pointer.transform.position = pos;

            var posController = rootVisualElement.Q<Vector2Field>("VEC2Value");
            posController.value = new Vector2(bl.x + (tr.x - bl.x) * (pos.x / panel.localBound.width + 0.5f),
                bl.y + (tr.y - bl.y) * (0.5f - pos.y / panel.localBound.height));


        }
    }


    void AutoSetup()
    {
        var spritesProp = _conSO.FindProperty("spriteObjects");
        var labelsProp = _conSO.FindProperty("labels");
        SortedSet<string> history = null;

        for (int i = 0; i < spritesProp.arraySize; i++)
        {
            var spriteResolver = spritesProp.GetArrayElementAtIndex(i).objectReferenceValue as MYTYSpriteResolver;
            var cat = spriteResolver.GetCategory();
            var labelIter = spriteResolver.spriteLibraryAsset.GetCategoryLabelNames(cat);
            var labelSet = new SortedSet<string>();
            foreach (var label in labelIter)
            {
                labelSet.Add(label);
            }
            if (history == null) history = labelSet;
            else
            {
                if (!CompareSet(history, labelSet))
                {
                    EditorUtility.DisplayDialog("MYTY Kit", "Labels are not match : " + spriteResolver.gameObject.name, "Ok");
                    return;
                }

            }
        }

        labelsProp.arraySize = history.Count;

        var idx = 0;

        foreach (var label in history)
        {
            labelsProp.GetArrayElementAtIndex(idx).FindPropertyRelative("label").stringValue = label;
            labelsProp.GetArrayElementAtIndex(idx).FindPropertyRelative("point").vector2Value = new Vector2();
            idx++;
        }
        _conSO.ApplyModifiedProperties();
    }

    private bool CompareSet(SortedSet<string> a, SortedSet<string> b)
    {
        if (a.Count != b.Count) return false;
        var aList = new List<string>();
        var bList = new List<string>();

        foreach (var elem in a)
        {
            aList.Add(elem);
        }

        foreach (var elem in b)
        {
            bList.Add(elem);
        }

        for (int i = 0; i < aList.Count; i++)
        {
            if (aList[i] != bList[i])
            {
                return false;
            }
        }

        return true;
    }

}
