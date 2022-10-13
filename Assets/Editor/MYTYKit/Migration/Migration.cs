using UnityEditor;
using UnityEngine;
using MYTYKit.MotionTemplates;
using UnityEngine.UIElements;

namespace MYTYKit
{
    public class Migration:EditorWindow
    {
        [MenuItem("MYTY Kit/Migrate", false, 1)]

        public static void ShowGUI()
        {
            var wnd = CreateInstance<Migration>();
            wnd.titleContent = new GUIContent("Migrate project to v1.0");
            wnd.minSize = wnd.maxSize = new Vector2(450, 180);
            wnd.ShowUtility();
            
        }

        void CreateGUI()
        {
            var btn = new Button();
            var motionBtn = new Button();
            btn.text = "Adapter";
            motionBtn.text = "MotionPack";
            btn.clicked += MigrateAdapter;
            motionBtn.clicked += PrepareNewMotionSystem;
            rootVisualElement.Add(btn);
            rootVisualElement.Add(motionBtn);
        }

        void PrepareNewMotionSystem()
        {
            var mpGo = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MYTYKit/Prefabs/MediapipeMotionPack.prefab")) as GameObject;
            var templateGo = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MYTYKit/Prefabs/DefaultMotionTemplate.prefab")) as GameObject;
            var mapper = templateGo.GetComponent<MotionTemplateMapper>();
            var source = mpGo.transform.GetComponentInChildren<MotionSource>();
            source.motionTemplateMapperList = new();
            source.motionTemplateMapperList.Add(mapper);
            
        }
        void MigrateAdapter()
        {
            this.MigrateJointV3ToV2Adapter();
            this.MigrateFacial1DAdapter();
            this.MigrateFacial2DAdapter();
            this.MigrateFacial2DCompound();
        }
    }
}