using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MYTYKit.Components;

namespace MYTYKit.Controllers
{
    public class Sprite2DNearstControllerMSR : MYTYController, IVec2Input
    {
        public Vector2 bottomLeft = new Vector2(0, 0);
        public Vector2 topRight = new Vector2(1, 1);
        public Vector2 value = new Vector2(0, 0);

        public List<MYTYSpriteResolver> spriteObjects;
        public List<Label2D> labels;

        [SerializeField] private string currentLabel;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (spriteObjects == null || labels == null) return;
            UpdateLabel();
        }

        public void UpdateLabel()
        {

            var selected = "";
            var minDist = float.MaxValue;
            if (labels == null || labels.Count == 0) return;
            foreach (var label2D in labels)
            {
                var dist = (label2D.point - value).magnitude;
                if (dist < minDist)
                {
                    selected = label2D.label;
                    minDist = dist;
                }
            }

            if (selected.Length > 0)
            {
                foreach (var spriteResolver in spriteObjects)
                {
                    if (spriteResolver == null) continue;
                    var catName = spriteResolver.GetCategory();

                    spriteResolver.SetCategoryAndLabel(catName, selected);

                    currentLabel = selected;
                }
            }
        }


        public override void PrepareToSave()
        {
#if UNITY_EDITOR
            for (int i = 0; i < spriteObjects.Count; i++)
            {
                spriteObjects[i] = PrefabUtility.GetCorrespondingObjectFromSource(spriteObjects[i]);
            }
#endif
        }

        public override void PostprocessAfterLoad(Dictionary<GameObject, GameObject> objMap)
        {
            for (int i = 0; i < spriteObjects.Count; i++)
            {
                spriteObjects[i] = objMap[spriteObjects[i].gameObject].GetComponent<MYTYSpriteResolver>();
            }
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                var so = new SerializedObject(this);
                for (int i = 0; i < spriteObjects.Count; i++)
                {
                    so.FindProperty("spriteObjects").GetArrayElementAtIndex(i).objectReferenceValue = spriteObjects[i];
                }

                so.ApplyModifiedProperties();
            }
#endif
        }

        public void SetInput(Vector2 val)
        {
            value = val;
        }

    }
}
