using MYTYKit.Components;
using MYTYKit.Controllers;
using UnityEditor;
using UnityEngine;
using MYTYKit.MotionTemplates;
using UnityEngine.UIElements;

namespace MYTYKit
{
    public class Migration:EditorWindow
    {
        [MenuItem("MYTY Kit/Migrate", false, 41)]

        public static void ShowGUI()
        {
            var wnd = CreateInstance<Migration>();
            wnd.titleContent = new GUIContent("Migrate project to v1.0");
            wnd.minSize = wnd.maxSize = new Vector2(300, 60);
            wnd.ShowUtility();
            
        }

        void CreateGUI()
        {
            var adapterBtn = new Button();
            var controllerBtn = new Button();
            adapterBtn.text = "Migrate Adapters";
            adapterBtn.clicked += () =>
            {
                var selector = FindObjectOfType<AvatarSelector>();
                if (selector == null) return;
                selector.Configure();
                PrepareNewMotionSystem();
                MigrateAdapter();
                RemoveMediapipe();
                EditorUtility.DisplayDialog("MYTY Kit", "Migration Done!", "Ok");
                Close();
            };
            
            controllerBtn.text = "Fix Sprite Controllers";
            controllerBtn.clicked += () =>
            {
                FixSpriteControllers();
            };
            rootVisualElement.Add(adapterBtn);
            rootVisualElement.Add(controllerBtn);
        }

        void PrepareNewMotionSystem()
        {
            var mpGo = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MYTYKit/MotionTemplate/Motion Source Samples/MediapipeMotionPack.prefab")) as GameObject;
            var templateGo = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MYTYKit/MotionTemplate/DefaultMotionTemplate.prefab")) as GameObject;
            var mapper = templateGo.GetComponent<MotionTemplateMapper>();
            var source = mpGo.transform.GetComponentInChildren<MotionSource>();
            source.motionTemplateMapperList = new();
            source.motionTemplateMapperList.Add(mapper);
            
        }
        void MigrateAdapter()
        {
            this.MigrateJointV3ToV2Adapter();
            this.MigrateJointV3ToV1Adapter();
            this.MigrateFacial1DAdapter();
            this.MigrateFacial2DAdapter();
            this.MigrateFacial2DCompound();
            this.MigrateAveragePosAdapter();
            this.MigrateWeightedSum1DAdapter();
            this.MigrateWeightedSum2DAdapter();
            FixBone2DController();
            FixSpriteControllers();
        }

        void FixSpriteControllers()
        {
            this.ConvertSprite1DController();
            this.ConvertSprite2DController();
        }

        void RemoveMediapipe()
        {
            var jointChild = FindObjectOfType<JointModel>();
            if (jointChild == null)
            {
                Debug.LogWarning("Cannot find the legacy mediapipe prefab");
                return;
            }
            DestroyImmediate(PrefabUtility.GetNearestPrefabInstanceRoot(jointChild));
        }
        
        void FixBone2DController()
        {
            var bond2dcons = FindObjectsOfType<Bone2DController>();

            foreach (var con in bond2dcons)
            {
                con.FlipY();
            }
        }
    }
}