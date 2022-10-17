using System.Collections.Generic;
using System.Linq;
using MYTYKit.Controllers;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MYTYKit
{
    public class Bone2DControllerEditorWindow : EditorWindow
    {
        public VisualTreeAsset UITemplate;

        bool m_isPressed = false;
        Vector2 m_lastPos = new();
        SerializedObject m_conSO;

        [MenuItem("MYTY Kit/Controller/Bone 2D Controller", false, 20)]
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
            listView.makeItem = () => { return new ObjectField(); };

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
                UpdateIndicator();
                
            });

            panel.RegisterCallback<MouseDownEvent>(OnMousePress, TrickleDown.TrickleDown);
            panel.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            panel.RegisterCallback((MouseUpEvent e) => { m_isPressed = false; });
            panel.RegisterCallback((MouseLeaveEvent e) => { m_isPressed = false; });

            rootVisualElement.Q<Toggle>("BTNRegOrigin").RegisterCallback((MouseUpEvent e) =>
            {
                ResetPos();
                HandleRigToggle(e, "orgRig");


            });
            rootVisualElement.Q<Toggle>("BTNRegLeft").RegisterCallback((MouseUpEvent e) =>
            {
                if (HandleRigToggle(e, "xminRig"))
                {
                    var xScaleVE = rootVisualElement.Q<FloatField>("FLTXScale");
                    SetControlPos(new Vector2(-xScaleVE.value, 0));
                }
            });
            rootVisualElement.Q<Toggle>("BTNRegRight").RegisterCallback((MouseUpEvent e) =>
            {
                if (HandleRigToggle(e, "xmaxRig"))
                {
                    var xScaleVE = rootVisualElement.Q<FloatField>("FLTXScale");
                    SetControlPos(new Vector2(xScaleVE.value, 0));
                }
            });
            rootVisualElement.Q<Toggle>("BTNRegUp").RegisterCallback((MouseUpEvent e) =>
            {
                if (HandleRigToggle(e, "ymaxRig"))
                {
                    var yScaleVE = rootVisualElement.Q<FloatField>("FLTYScale");
                    SetControlPos(new Vector2(0, yScaleVE.value));
                }
            });
            rootVisualElement.Q<Toggle>("BTNRegDown").RegisterCallback((MouseUpEvent e) =>
            {
                if (HandleRigToggle(e, "yminRig"))
                {
                    var yScaleVE = rootVisualElement.Q<FloatField>("FLTYScale");
                    SetControlPos(new Vector2(0, -yScaleVE.value));
                }
            });



            if (selectedObjs.Length == 0) return;

            InitializeWithFaceCon(selectedObjs[0]);
        }

        private void InitializeWithFaceCon(Bone2DController obj)
        {
            m_conSO = new SerializedObject(obj);

            var fcObjField = rootVisualElement.Q<ObjectField>("Bone2DController");
            var targetsProp = m_conSO.FindProperty("rigTarget");
            var posController = rootVisualElement.Q<Vector2Field>("ControllerPos");
            var controlPosProp = m_conSO.FindProperty("controlPosition");
            var pointPanel = rootVisualElement.Q<VisualElement>("Panel");

            var xScaleVE = rootVisualElement.Q<FloatField>("FLTXScale");
            var yScaleVE = rootVisualElement.Q<FloatField>("FLTYScale");

            xScaleVE.BindProperty(m_conSO.FindProperty("xScale"));
            yScaleVE.BindProperty(m_conSO.FindProperty("yScale"));

            posController.BindProperty(controlPosProp);
            fcObjField.value = obj;

            List<GameObject> targetObjs = new();

            for (int i = 0; i < targetsProp.arraySize; i++)
            {
                targetObjs.Add(targetsProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
            }

            rootVisualElement.Q<ListView>("TargetList").itemsSource = targetObjs;
            rootVisualElement.Q<ListView>("TargetList").Rebuild();
            pointPanel.RegisterCallback<GeometryChangedEvent>(evt => UpdateIndicator());
            SyncRiggingStatus();

        }

        private bool HandleRigToggle(MouseUpEvent e, string prop)
        {
            if (m_conSO == null) return false;
            var elem = e.target as Toggle;
            var result = true;
            if (elem.value)
            {
                Record(m_conSO.FindProperty(prop));
            }
            else
            {
                m_conSO.FindProperty(prop).arraySize = 0;
                m_conSO.ApplyModifiedProperties();
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
            SyncRiggingHelper("ymaxRig", "UpMarker", "BTNRegUp");
            SyncRiggingHelper("yminRig", "DownMarker", "BTNRegDown");
        }

        private void SyncRiggingHelper(string property, string element, string btnElem)
        {
            if (m_conSO.FindProperty(property).arraySize > 0)
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
            if (m_conSO.FindProperty("orgRig").arraySize > 0) return false;
            if (m_conSO.FindProperty("xminRig").arraySize > 0) return false;
            if (m_conSO.FindProperty("xmaxRig").arraySize > 0) return false;
            if (m_conSO.FindProperty("yminRig").arraySize > 0) return false;
            if (m_conSO.FindProperty("ymaxRig").arraySize > 0) return false;
            return true;
        }

        private void AddTarget()
        {
            var targetsProp = m_conSO.FindProperty("rigTarget");
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

            m_conSO.FindProperty("xminRig").arraySize = 0;
            m_conSO.FindProperty("xmaxRig").arraySize = 0;
            m_conSO.FindProperty("yminRig").arraySize = 0;
            m_conSO.FindProperty("ymaxRig").arraySize = 0;
            m_conSO.FindProperty("orgRig").arraySize = 0;

            m_conSO.ApplyModifiedProperties();
        }

        private void RemoveSelection()
        {
            var targetsProp = m_conSO.FindProperty("rigTarget");
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

            m_conSO.ApplyModifiedProperties();
        }

        private void RemoveAll()
        {
            if (!CheckPivots())
            {
                EditorUtility.DisplayDialog("MYTY Kit", "Reset all pivots first.", "Ok");
                return;
            }

            var targetsProp = m_conSO.FindProperty("rigTarget");
            var listView = rootVisualElement.Q<ListView>("TargetList");
            targetsProp.arraySize = 0;
            listView.itemsSource = new List<GameObject>();
            listView.Rebuild();
            m_conSO.ApplyModifiedProperties();
        }

        private void ResetPos()
        {
            SetControlPos(new Vector2());
        }

        private void SetControlPos(Vector2 pos)
        {
            if (m_conSO != null && m_conSO.targetObject != null)
            {
                var posController = rootVisualElement.Q<Vector2Field>("ControllerPos");
                posController.value = pos;
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

        void UpdateIndicator()
        {
            var controlPosition = m_conSO.FindProperty("controlPosition").vector2Value;
            var pointPanel = rootVisualElement.Q<VisualElement>("Panel");
            var pointerVE = rootVisualElement.Q<VisualElement>("Controller");
            var panelDim = new Vector2(pointPanel.localBound.width, pointPanel.localBound.height);
            var panelPos = CalcPanelPos(controlPosition, panelDim);

            pointerVE.style.left = panelPos.x-10;
            pointerVE.style.bottom = panelPos.y-10;

            if (m_conSO == null) return;
            
            var target = (Bone2DController)m_conSO.targetObject;
            target.InterpolateGUI();
        }
        
        Vector2 CalcPanelPos(Vector2 value, Vector2 panelDim)
        {
            var xScale = m_conSO.FindProperty("xScale").floatValue;
            var yScale = m_conSO.FindProperty("yScale").floatValue;
            var tr = new Vector2(xScale, yScale);
            var bl = new Vector2(-xScale, -yScale);

            var norm = (value - bl) / (tr-bl);
            return panelDim * norm;
        }


        private void Record(SerializedProperty prop)
        {
            var rigProp = m_conSO.FindProperty("rigTarget");
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

            m_conSO.ApplyModifiedProperties();
        }

        private void OnMousePress(MouseDownEvent e)
        {
            m_isPressed = true;
            HandleMousePos(e.localMousePosition);

        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (m_isPressed)
            {
                HandleMousePos(e.localMousePosition);
            }
        }
        
        void HandleMousePos(Vector2 mousePos)
        {
            if (m_conSO == null) return;
            var cpProp = m_conSO.FindProperty("controlPosition");
            var xScale = m_conSO.FindProperty("xScale").floatValue;
            var yScale = m_conSO.FindProperty("yScale").floatValue;
            var pointPanel = rootVisualElement.Q<VisualElement>("Panel");
            var panelDim = new Vector2(pointPanel.localBound.width, pointPanel.localBound.height);

            var tr = new Vector2(xScale, yScale);
            var bl = new Vector2(-xScale, -yScale);
            mousePos.y = panelDim.y - mousePos.y;
        
            var result = (mousePos / panelDim) * (tr - bl) + bl;

            result = new Vector2(
                Mathf.Round(result.x * 1000) / 1000,
                Mathf.Round(result.y * 1000) / 1000);
            cpProp.vector2Value = result;
        
            m_conSO.ApplyModifiedProperties();
        }

        private void OnDestroy()
        {
            ResetPos();
        }

    }
}