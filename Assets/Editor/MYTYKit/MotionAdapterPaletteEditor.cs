using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MYTYKit
{
    [CustomEditor(typeof(MotionAdapterPalette))]
    public class MotionAdapterPaletteEditor : Editor
    {
        public GameObject boneRotate2d;
        public GameObject boneTiltOrPosition;
        public GameObject boneEyeBlink;
        public GameObject boneEyeBlinkAll;
        public GameObject boneEyeBrow;
        public GameObject boneEyeBrowAll;
        public GameObject boneLeftPupil;
        public GameObject boneRightPupil;
        public GameObject bonePupilAll;
        public GameObject spriteEyeBlink;
        public GameObject spriteEyeBlinkAll;
        public GameObject spriteEyeBrow;
        public GameObject spriteEyeBrowAll;
        public GameObject spriteMouth;
        public GameObject blendshape;
        
        
        List<GameObject> m_boneAdapters;
        List<GameObject> m_spriteAdapters;
        List<GameObject> m_blendshapes;
        
        public static void CreateAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<MotionAdapterPalette>(MYTYPath.MotionAdapterPalettePath);
            if (asset != null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(MYTYPath.MotionAdapterPalettePath) ?? "Assets/MYTYKit/Prefabs");
            var paletteObj = CreateInstance<MotionAdapterPalette>();
            AssetDatabase.CreateAsset(paletteObj, MYTYPath.MotionAdapterPalettePath);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            
            var boneFoldOut = new Foldout();
            var spriteFoldOut = new Foldout();
            var bsFoldOut = new Foldout();

            boneFoldOut.text = "Bone";
            spriteFoldOut.text = "Sprite";
            bsFoldOut.text = "Blendshape";
            
            PrepareAdapterLists();
            
            m_boneAdapters.ForEach(item=>BuildFoldOut(boneFoldOut,item));
            m_spriteAdapters.ForEach(item=>BuildFoldOut(spriteFoldOut,item));
            m_blendshapes.ForEach(item=>BuildFoldOut(bsFoldOut, item));
            
            root.Add(boneFoldOut);
            root.Add(spriteFoldOut);
            root.Add(bsFoldOut);

            return root;
        }

        void BuildFoldOut(Foldout foldout, GameObject item)
        {
            var btn = new Button(() =>
            {
                var go = Instantiate(item);
                go.name = GetMemberName(item);
            });
            btn.text = ConvertFromCamelCaseToWords(GetMemberName(item));
            foldout.Add(btn);
        }

        void PrepareAdapterLists()
        {
            m_boneAdapters = new ()
            {
                boneRotate2d, boneTiltOrPosition, boneEyeBlink, boneEyeBlinkAll, boneEyeBrow, boneEyeBrowAll, boneLeftPupil,
                boneRightPupil, bonePupilAll
            };

            m_spriteAdapters = new()
            {
                spriteEyeBlink, spriteEyeBlinkAll, spriteEyeBrow, spriteEyeBrowAll, spriteMouth
            };

            m_blendshapes = new()
            {
                blendshape
            };
        }

        string ConvertFromCamelCaseToWords(string name)
        {
            string output = Regex.Replace(name, "(\\B[A-Z])", " $1");
            return Char.ToUpper(output[0]) + output.Substring(1);
        }
        
        string GetMemberName(object localVariable)
        {
            Type type = GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.GetValue(this) == localVariable)
                {
                    return field.Name;
                }
            }
            return null; 
        }
        
    }


}
