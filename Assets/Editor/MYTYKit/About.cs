using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class About : EditorWindow
{
    [MenuItem("MYTY Kit/About", false, 100)]
    public static void ShowWindow()
    {
        var wnd = GetWindow<About>();
        wnd.titleContent = new GUIContent("About");
    }
}
