using System.Linq;
using MYTYKit.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace MYTYKit
{
    public class SpriteResolverCleaner : MonoBehaviour
    {
        public static void FixAvatarSprite()
        {
            var selector = FindObjectOfType<AvatarSelector>(true);
            if (selector == null)
            {
                Debug.LogWarning("No Avatar Selector");
                return;
            }
            selector.templates.ForEach(template => CleanUp(template.instance, template.spriteLibrary));
        }

        static void CleanUp(GameObject avatarRoot, SpriteLibraryAsset spriteLibrary)
        {
            var spriteRenderer = avatarRoot.GetComponentsInChildren<SpriteRenderer>(true);
        
            spriteRenderer.ToList().ForEach(renderer =>
            {
                var sr = renderer.GetComponent<SpriteResolver>();
                if (sr == null) return;
            
                renderer.GetComponents<SpriteResolver>().ToList().ForEach(resolver =>
                {
                    if(resolver!=sr) DestroyImmediate(resolver);
                });
            
                var mytySR = renderer.GetComponent<MYTYSpriteResolver>();
                if (mytySR == null) return;
                mytySR.spriteLibraryAsset = spriteLibrary;
                var so = new SerializedObject(mytySR);
                so.FindProperty("m_spriteLibraryAsset").objectReferenceValue = spriteLibrary;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(mytySR);
                renderer.GetComponents<MYTYSpriteResolver>().ToList().ForEach(mytyresolver =>
                {
                    if(mytyresolver!=mytySR) DestroyImmediate(mytyresolver);
                });
            
            
            });
        }
    }

}