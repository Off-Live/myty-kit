using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MYTYKit
{
    [CustomEditor(typeof(CameraSource))]
    public class CameraSourceEditor : UnityEditor.Editor
    {
        public VisualTreeAsset UITemplate;

        public override VisualElement CreateInspectorGUI()
        {
            var ui = new VisualElement();
            UITemplate.CloneTree(ui);
            var deviceCMB = ui.Q<DropdownField>();

            ui.Q<PropertyField>("PRPRenderer").BindProperty(serializedObject.FindProperty("previewRenderer"));

            var deviceNames = new List<string>();
            var camNameProp = serializedObject.FindProperty("camDeviceName");

            foreach (var device in WebCamTexture.devices)
            {
                if (device.name.StartsWith("MYTY") || device.name.StartsWith("Off"))
                {
                    continue;
                }

                deviceNames.Add(device.name);
            }

            deviceCMB.choices = deviceNames;
            if (camNameProp.stringValue.Length == 0)
            {
                deviceCMB.index = 0;
            }
            else
            {
                var idx = deviceNames.FindIndex(name => name == camNameProp.stringValue);

                Debug.Log("device index : " + idx);
                if (idx < 0) idx = -1;
                deviceCMB.index = idx;
            }

            deviceCMB.RegisterValueChangedCallback((ChangeEvent<string> e) =>
            {
                camNameProp.stringValue = e.newValue;
                serializedObject.ApplyModifiedProperties();
            });

            return ui;
        }
    }
}
