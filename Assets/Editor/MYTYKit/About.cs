using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class About : EditorWindow
{
    public VisualTreeAsset UITemplate;

    [MenuItem("MYTY Kit/About", false, 100)]
    public static void ShowWindow()
    {
        var wnd = GetWindow<About>();
        wnd.titleContent = new GUIContent("About");
    }

    void CreateGUI()
    {
        UITemplate.CloneTree(rootVisualElement);
        maxSize = new Vector2(300, 120);
        minSize = maxSize;

        var versionField = rootVisualElement.Q<Label>("LBLVersion");
        var filename = Application.streamingAssetsPath + "/VERSION.txt";
        var versionStr = File.ReadAllText(filename);
        versionStr = versionStr.Trim();
        versionField.text = versionStr;

    }
}
