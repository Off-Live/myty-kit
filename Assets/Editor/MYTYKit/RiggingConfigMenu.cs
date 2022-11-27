using MYTYKit.Components;
using MYTYKit.Controllers;
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    public class RiggingConfigMenu
    {
        [MenuItem("MYTY Kit/Save rigging status")]
        public static void SaveRig()
        {
            BoneControllerStorage.Save();
        }

        [MenuItem("MYTY Kit/Restore rigging status")]
        public static void LoadRig()
        {
            BoneControllerStorage.Restore();
            ;
        }
    }
}