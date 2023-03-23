using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace MYTYKit
{
    public class ExtensionInstaller
    {
        static AddAndRemoveRequest m_request;
        static readonly string[] Myty3DPackages =
        {
            "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v0.109.0",
            "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v0.109.0",
            "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.109.0",
            "https://github.com/Off-Live/myty-3d-avatar-extension.git?path=/Assets"
        };
    
    
        [MenuItem("MYTY Kit/Install Extensions/3D Avatar Extension", false, 200)]
        static void InstallMYTY3D()
        {
            m_request = Client.AddAndRemove(Myty3DPackages, null);
            EditorUtility.DisplayProgressBar("MYTY Kit","Installing packages",0.5f);
            EditorApplication.update += Progress;
        }
    
        static void Progress()
        {
            if (m_request.IsCompleted)
            {
                if (m_request.Status == StatusCode.Success)
                {
                    Debug.Log("Installation Done!");
                    EditorApplication.update -= Progress;
                    EditorUtility.ClearProgressBar();
                }
                else if (m_request.Status >= StatusCode.Failure)
                {
                    Debug.LogError(m_request.Error.message);
                    EditorApplication.update -= Progress;
                    EditorUtility.ClearProgressBar();
                }
            
            }
        }
    }
}