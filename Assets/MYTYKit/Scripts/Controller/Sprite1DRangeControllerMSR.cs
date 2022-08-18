using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEditor;



public class Sprite1DRangeControllerMSR : MYTYController, IFloatInput 
{
    public float min=0;
    public float max=1;
    public float value=0;

    public List<MYTYSpriteResolver> spriteObjects;
    public List<Interval> intervals;

    [SerializeField]
    private string currentLabel;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (spriteObjects == null || intervals == null) return;
        UpdateLabel();
       
    }

    public void UpdateLabel()
    {

        if (max < min) return;
        float scaledValue = min + (max - min) * value;
        if (spriteObjects == null) return;
        foreach (var spriteResolver in spriteObjects)
        {
            if (spriteResolver == null) return;
            var selected = "";
            foreach (var interval in intervals)
            {
                if (interval.min <= scaledValue && interval.max > scaledValue)
                {
                    selected = interval.label;
                    break;
                }
            }

            if (selected.Length > 0)
            {
                spriteResolver.SetCategoryAndLabel(spriteResolver.GetCategory(), selected);
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

    void IFloatInput.SetInput(float val)
    {
        value = val;
    }
}
