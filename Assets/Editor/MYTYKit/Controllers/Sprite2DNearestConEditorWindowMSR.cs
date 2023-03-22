using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using MYTYKit.Components;
using MYTYKit.Controllers;
using Object = UnityEngine.Object;

namespace MYTYKit
{
    public class Sprite2DNearestConEditorWindowMSR : EditorWindow
    {
        SerializedObject m_conSO;
        bool m_isPressed = false;
        double m_lastClickTime = 0.0f;
        

        [MenuItem("MYTY Kit/Controller/Sprite 2D Nearest Controller", false, 20)]
        public static void ShowController()
        {
            var wnd = GetWindow<Sprite2DNearestConEditorWindowMSR>();
            wnd.titleContent = new GUIContent("Sprite 2D Nearest Controller");
        }

        private void CreateGUI()
        {
            var uiTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/MYTYKit/UI/Sprite2DNearestCon.uxml");
            uiTemplate.CloneTree(rootVisualElement);
            var selectedGOs = Selection.GetFiltered<Sprite2DNearestControllerMSR>(SelectionMode.Editable);
            var conVE = rootVisualElement.Q<ObjectField>("OBJController");
            var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
            var addBtn = rootVisualElement.Q<Button>("BTNAdd");
            var removeBtn = rootVisualElement.Q<Button>("BTNRemove");
            var removeAllBtn = rootVisualElement.Q<Button>("BTNRemoveAll");
            var makePivotBtn = rootVisualElement.Q<Button>("BTNPivot");
            var removePivotBtn = rootVisualElement.Q<Button>("BTNPivotRemove");
            var copyPosBtn = rootVisualElement.Q<Button>("BTNCopyPos");
            var pivotPanel = rootVisualElement.Q<VisualElement>("VEPivot");
            var pointPanel = rootVisualElement.Q<VisualElement>("VEPanel");
            var listPivot = rootVisualElement.Q<ListView>("LSTPivot");

            addBtn.clicked += AddSelections;
            removeBtn.clicked += Remove;
            removeAllBtn.clicked += RemoveAll;
            makePivotBtn.clicked += OnMakePivot;
            removePivotBtn.clicked += OnRemovePivot;
            copyPosBtn.clicked += OnCopyPos;

            conVE.objectType = typeof(Sprite2DNearestControllerMSR);
            listView.makeItem = () =>
            {
                return new ObjectField();
            };

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
                InitWithController(e.newValue as Sprite2DNearestControllerMSR);

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
                UpdateIndicator();
            });

            rootVisualElement.Q<Button>("BTNAutoSetup").clicked += AutoSetup;

            var panel = rootVisualElement.Q<VisualElement>("VEClickArea");
            panel.RegisterCallback<MouseDownEvent>(OnMousePress);
            panel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            panel.RegisterCallback((MouseUpEvent e) => { m_isPressed = false; });
            panel.RegisterCallback((MouseLeaveEvent e) => { m_isPressed = false; });
            
            pivotPanel.RegisterCallback<GeometryChangedEvent>( evt => SyncPivotPosition());
            pointPanel.RegisterCallback<GeometryChangedEvent>(evt => UpdateIndicator());

            listPivot.RegisterCallback<ClickEvent>(evt => OnClickPivotList());
            if (selectedGOs.Length == 0) return;

            InitWithController(selectedGOs[0]);
        }

        void InitWithController(Sprite2DNearestControllerMSR controller)
        {
            m_conSO = new SerializedObject(controller);

            var conVE = rootVisualElement.Q<ObjectField>("OBJController");
            var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
            var listSource = new List<GameObject>();
            
            
            var spritesProps = m_conSO.FindProperty("spriteObjects");
            
            
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

            rootVisualElement.Q<Vector2Field>("VEC2BL").BindProperty(m_conSO.FindProperty("bottomLeft"));
            rootVisualElement.Q<Vector2Field>("VEC2TR").BindProperty(m_conSO.FindProperty("topRight"));
            rootVisualElement.Q<Vector2Field>("VEC2Value").BindProperty(m_conSO.FindProperty("value"));
          
            
            SetPanelCoord();
            UpdatePivotList();
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
            var newSprites = Selection.GetFiltered<MYTYSpriteResolver>(SelectionMode.Editable);
            var spritesProps = m_conSO.FindProperty("spriteObjects");
            var newSource = new List<GameObject>();
            var offset = spritesProps.arraySize;
            spritesProps.arraySize += newSprites.Length;
            for (int i = 0; i < newSprites.Length; i++)
            {
                spritesProps.GetArrayElementAtIndex(offset+i).objectReferenceValue = newSprites[i];
            }

            for (int i = 0; i < spritesProps.arraySize; i++)
            {
                newSource.Add((spritesProps.GetArrayElementAtIndex(i).objectReferenceValue as MYTYSpriteResolver).gameObject);
            }

            m_conSO.ApplyModifiedProperties();

            rootVisualElement.Q<ListView>("LSTSpriteGO").itemsSource = newSource;
            rootVisualElement.Q<ListView>("LSTSpriteGO").Rebuild();
        }

        void Remove()
        {
            var spritesProps = m_conSO.FindProperty("spriteObjects");
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
            m_conSO.ApplyModifiedProperties();
        }

        void RemoveAll()
        {
            var spritesProps = m_conSO.FindProperty("spriteObjects");
            var listView = rootVisualElement.Q<ListView>("LSTSpriteGO");
            spritesProps.arraySize = 0;
            listView.itemsSource = new List<GameObject>();
            listView.Rebuild();
            m_conSO.ApplyModifiedProperties();
        }

        void OnMousePress(MouseDownEvent e)
        {
            m_isPressed = true;
            HandleMousePos(e.localMousePosition);
         }

        void OnMouseMove(MouseMoveEvent e)
        {
            if (m_isPressed)
            {
                HandleMousePos(e.localMousePosition);
            }
        }
        
        void HandleMousePos(Vector2 mousePos)
        {
            if (m_conSO == null) return;
            var cpProp = m_conSO.FindProperty("value");
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


        void AutoSetup()
        {
            if (m_conSO == null) return;
            var spritesProp = m_conSO.FindProperty("spriteObjects");
            var labelsProp = m_conSO.FindProperty("labels");
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
            m_conSO.ApplyModifiedProperties();
            UpdatePivotList();
        }
        
        void OnRemovePivot()
        {
            if (m_conSO == null) return;
            var listView = rootVisualElement.Q<ListView>("LSTPivot");
            var pivotsProp = m_conSO.FindProperty("labels");
            var selection = listView.selectedIndex;
            if (selection < 0) return;
            pivotsProp.DeleteArrayElementAtIndex(selection);
            m_conSO.ApplyModifiedProperties();
            UpdatePivotList();
        }   
        
        void OnCopyPos()
        {
            
            rootVisualElement.Q<Vector2Field>("VEC2PivotPos").value =
                rootVisualElement.Q<Vector2Field>("VEC2Value").value;
        }
        void OnMakePivot()
        {
            if (m_conSO == null) return;
            var pivotsProp = m_conSO.FindProperty("labels");

            pivotsProp.arraySize++;

            var index = pivotsProp.arraySize - 1;
            var elemProp = pivotsProp.GetArrayElementAtIndex(index);

            elemProp.FindPropertyRelative("label").stringValue = rootVisualElement.Q<TextField>("TXTPivotName").text;
            elemProp.FindPropertyRelative("point").vector2Value = rootVisualElement.Q<Vector2Field>("VEC2PivotPos").value;

            m_conSO.ApplyModifiedProperties();
            UpdatePivotList();
        }

        void UpdatePivotList()
        {
            if (m_conSO == null) return;
            var listView = rootVisualElement.Q<ListView>("LSTPivot");
            var pivotsProp = m_conSO.FindProperty("labels");
            
            var nameList = new List<string>();
            for (int i = 0; i < pivotsProp.arraySize; i++)
            {
                nameList.Add(pivotsProp.GetArrayElementAtIndex(i).FindPropertyRelative("label").stringValue);
            }
            
            listView.itemsSource = nameList;
            listView.Rebuild();
            SyncPivotPosition();

        }

        void SyncPivotPosition()
        {
            if (m_conSO == null) return;
            var panel = rootVisualElement.Q<VisualElement>("VEPivot");
            var pivotsProp = m_conSO.FindProperty("labels");
            
            panel.Clear();
            
            var panelDim = new Vector2(panel.localBound.width, panel.localBound.height);
            
            for (var i = 0; i < pivotsProp.arraySize; i++)
            {
                var name = pivotsProp.GetArrayElementAtIndex(i).FindPropertyRelative("label").stringValue;
                var position = pivotsProp.GetArrayElementAtIndex(i).FindPropertyRelative("point").vector2Value;
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
            if (m_conSO == null) return;
            var controlPosition = m_conSO.FindProperty("value").vector2Value;
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
            if (m_conSO == null) return;
            var controller = m_conSO.targetObject as Sprite2DNearestControllerMSR;
            controller.Update();
        }

        void OnClickPivotList()
        {
            if (m_conSO == null) return;
            var timestamp = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
            var diff = timestamp - m_lastClickTime;
            m_lastClickTime = timestamp;
            if (diff > 250) return;

            var listPivot = rootVisualElement.Q<ListView>("LSTPivot");
            var pivotsProp = m_conSO.FindProperty("labels");
            var selectedIndex = listPivot.selectedIndex;
            if (selectedIndex < 0) return;
            var label = pivotsProp.GetArrayElementAtIndex(selectedIndex).FindPropertyRelative("label").stringValue;
            var point = pivotsProp.GetArrayElementAtIndex(selectedIndex).FindPropertyRelative("point").vector2Value;

            var modalWindow = CreateInstance<PivotModalDialog>();
            modalWindow.Init(OnChangePivot,label,point);
            modalWindow.titleContent =new GUIContent( "Edit Pivot");
            modalWindow.minSize = modalWindow.maxSize = new Vector2(300, 70);
            modalWindow.ShowModalUtility();
            
        }


        void OnChangePivot(string label, Vector2 point)
        {
            var listPivot = rootVisualElement.Q<ListView>("LSTPivot");
            var pivotsProp = m_conSO.FindProperty("labels");
            var selectedIndex = listPivot.selectedIndex;

            pivotsProp.GetArrayElementAtIndex(selectedIndex).FindPropertyRelative("label").stringValue = label;
            pivotsProp.GetArrayElementAtIndex(selectedIndex).FindPropertyRelative("point").vector2Value = point;
            m_conSO.ApplyModifiedProperties();
            UpdatePivotList();
        }
        
        
        Vector2 CalcPanelPos(Vector2 value, Vector2 panelDim)
        {
            var tr = m_conSO.FindProperty("topRight").vector2Value;
            var bl = m_conSO.FindProperty("bottomLeft").vector2Value;

            var norm = (value - bl) / (tr-bl);
            return panelDim * norm;
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
}
