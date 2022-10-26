using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MYTYKit
{
    public class PivotModalDialog : EditorWindow
    {
        Action<string, Vector2> m_callback;
        string m_label;
        Vector2 m_position;
        public void Init(Action<string, Vector2> modalCallback, string label, Vector2 position)
        {
            m_callback = modalCallback;
            m_label = label;
            m_position = position;
        }

        void CreateGUI()
        {
            var labelElem = new TextField();
            var positionElem = new Vector2Field();
            var btnArea = new VisualElement();
            btnArea.style.flexDirection = FlexDirection.RowReverse;

            var okBtn = new Button();
            var cancelBtn = new Button();

            labelElem.label = "Pivot Name";
            labelElem.value = m_label;

            positionElem.label = "Pivot Position";
            positionElem.value = m_position;

            okBtn.text = "Ok";
            cancelBtn.text = "Cancel";

            okBtn.clicked += () =>
            {
                m_callback.Invoke(labelElem.value, positionElem.value);
                Close();
            };
            cancelBtn.clicked += Close;
            
            btnArea.Add(cancelBtn);
            btnArea.Add(okBtn);
            
            rootVisualElement.Add(labelElem);
            rootVisualElement.Add(positionElem);
            rootVisualElement.Add(btnArea);
        }
    }
}