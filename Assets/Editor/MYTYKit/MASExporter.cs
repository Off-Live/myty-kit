using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MYTYKit.Components;
using MYTYKit.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.U2D.Animation;


namespace MYTYKit
{
    
    public class MASExporter : MonoBehaviour
    {

        static int m_transformID = 0;
        static Dictionary<Transform, int> m_transformMap;

        // [MenuItem("TestMenu/Refresh")]
        // public static void Refresh()
        // {
        //     var selector = GameObject.FindObjectOfType<AvatarSelector>();
        //     if (selector == null) return;
        //     
        //     selector.templates.Select(template => template.spriteLibrary).ToList().ForEach(asset =>
        //     {
        //         
        //          // var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset));
        //          // EditorUtility.SetDirty(asset);
        //          // importer.SaveAndReimport();
        //          AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset),ImportAssetOptions.ForceUpdate);
        //
        //     });
        //     AssetDatabase.Refresh();
        //
        // }
        //
        // [MenuItem("TestMenu/NewExporter")]
        // public static void Export()
        // {
        //     m_transformID = 0;
        //     m_transformMap = new();
        //     EditorSceneManager.OpenScene("Assets/Scene/1.0 scene.unity", OpenSceneMode.Single);
        //     
        //     SpriteResolverCleaner.FixAvatarSprite();
        //     var selector = GameObject.FindObjectOfType<AvatarSelector>();
        //     if (selector == null) return;
        //     
        //     selector.templates.ForEach(template =>
        //     {
        //         var psb = AssetDatabase.LoadAssetAtPath<GameObject>(template.PSBPath);
        //         var tempPath = "Assets/"+template.instance.name + ".asset";
        //         SpriteLibraryFactory.CreateLibrary(psb, tempPath);
        //         var spriteLib = template.instance.GetComponent<SpriteLibrary>();
        //         var spriteLibraryAsset =  AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(tempPath);
        //         spriteLib.spriteLibraryAsset = spriteLibraryAsset;
        //         template.spriteLibrary = spriteLibraryAsset;
        //     });
        //
        //     var mapper = FindObjectOfType<MotionTemplateMapper>();
        //     
        //     var templates = selector.templates.Select(template => SerializeTemplate(template));
        //     var rootControllers = FindObjectsOfType<RootController>()
        //         .Select(BuildRootController);
        //     var adapters = FindObjectsOfType<NativeAdapter>().Select(adapter => (adapter as ISerializableAdapter)).ToArray();
        //
        //     var exportedJO = JObject.FromObject(new
        //     {
        //         templates,
        //         rootControllers,
        //         mapper= mapper.SerializeToJObject(),
        //         adapters = adapters.Select(adapter => adapter.SerializeToJObject(m_transformMap))
        //     });
        //     
        //     File.WriteAllText("Assets/mas_metadata.json",exportedJO.ToString(Formatting.Indented));
        //     ExportAvatar(new List<string>(){"1","2","3"},selector);
        //     
        // }

        static void ExportAvatar(List<string> idList, AvatarSelector selector)
        {

            var templateRoot = selector.templates.First().instance.transform.parent;
            var atlasMap = new Dictionary<int, Texture2D>();
            idList.ForEach(id =>
            {
                selector.id = id;
                selector.Configure();
                var templateId = selector.GetCurrentTemplateIndex();
                Texture2D rgbaAtlas = null;

                if (atlasMap.ContainsKey(templateId)) rgbaAtlas = atlasMap[templateId];
                else
                {
                    rgbaAtlas = GetReadableRGBAAtlas(selector);
                    atlasMap[templateId] = rgbaAtlas;
                }

                var spriteLibraryAsset = selector.templates[templateId].spriteLibrary;

                var currentAtlas = ExtractCurrentTextureAtlas(rgbaAtlas, spriteLibraryAsset, out var spriteDict);
                File.WriteAllBytes($"Assets/{IdToFilename(id)}.png", currentAtlas.EncodeToPNG());

                var spriteRenderers = templateRoot.GetComponentsInChildren<SpriteRenderer>()
                    .Where(renderer => renderer.GetComponent<SpriteSkin>() != null).Select(renderer =>
                        SerializeSpriteRenderer(renderer, spriteLibraryAsset, spriteDict)).ToArray();

                var avatarJO = JObject.FromObject(new
                {
                    templateId, id, spriteRenderers
                });

                var filename = IdToFilename(id);
                File.WriteAllText($"Assets/{filename}.json", avatarJO.ToString(Formatting.Indented));
            });
        }

        static Texture2D GetReadableRGBAAtlas(AvatarSelector selector)
        {
            var templateRoot = selector.templates.First().instance.transform.parent;
            var atlasTexture = templateRoot.GetComponentsInChildren<SpriteRenderer>()
                .Where(renderer => renderer.GetComponent<SpriteSkin>() != null).First().sprite.texture;

            var dxt5texture = new Texture2D(atlasTexture.width, atlasTexture.height, atlasTexture.format, false);
            dxt5texture.LoadRawTextureData(atlasTexture.GetRawTextureData());
            dxt5texture.Apply();

            var rgbaAtlas = new Texture2D(atlasTexture.width, atlasTexture.height);
            rgbaAtlas.SetPixels32(dxt5texture.GetPixels32());
            rgbaAtlas.Apply();
            return rgbaAtlas;
        }

        static (Sprite sprite, Texture2D subTexture, Vector2 normPivot, Rect normRect)
            GenerateTupleFromSprite(Sprite sprite, Texture2D globalAtlas)
        {
            Debug.Assert(sprite != null);
            var rect = sprite.rect;
            var startX = Mathf.RoundToInt(rect.x);
            var startY = Mathf.RoundToInt(rect.y);
            var width = Mathf.RoundToInt(rect.width);
            var height = Mathf.RoundToInt(rect.height);
            var subTexture = new Texture2D(width, height);
            var pixels = globalAtlas.GetPixels(startX, startY, width, height);
            subTexture.SetPixels(pixels);
            subTexture.Apply();
            var pivot = sprite.pivot;
            var normPivot = new Vector2(pivot.x / rect.width, pivot.y / rect.height);
            var normRect = new Rect(rect.x / globalAtlas.width, rect.y / globalAtlas.height,
                rect.width / globalAtlas.width, rect.height / globalAtlas.height);
            return (sprite, subTexture, normPivot, normRect);
        }

        static Texture2D ExtractCurrentTextureAtlas(Texture2D globalAtlas, SpriteLibraryAsset spriteLibraryAsset,
            out Dictionary<Sprite, SpriteOverrideParameter> spriteDict)
        {
            var selector = FindObjectOfType<AvatarSelector>();
            var templateRoot = selector.templates.First().instance.transform.parent;
            var sprites = templateRoot.GetComponentsInChildren<SpriteRenderer>()
                .Where(renderer => renderer.GetComponent<SpriteSkin>() != null).Select(
                    renderer =>
                    {
                        var resolver = renderer.GetComponent<MYTYSpriteResolver>();
                        if (resolver == null)
                        {
                            return new[] { renderer.sprite };
                        }
                        else
                        {
                            var catName = resolver.GetCategory();
                            return spriteLibraryAsset.GetCategoryLabelNames(resolver.GetCategory())
                                .Select(name => spriteLibraryAsset.GetSprite(catName, name)).ToArray();
                        }

                    }).Aggregate(new Sprite[] { }, (acc, array) => acc.Concat(array).ToArray());

            var tuples = sprites.Select(sprite => GenerateTupleFromSprite(sprite, globalAtlas));

            var subTextures = tuples.Select(tuple => tuple.subTexture).ToArray();
            var currentAtlas = new Texture2D(2, 2);

            var atlasRects = currentAtlas.PackTextures(subTextures, 0, 2048);
            var atlasWidth = currentAtlas.width;
            var atlasHeight = currentAtlas.height;

            var rects = atlasRects.Select(rect => new Rect()
            {
                x = rect.x * atlasWidth,
                y = rect.y * atlasHeight,
                width = rect.width * atlasWidth,
                height = rect.height * atlasHeight
            }).ToArray();

            var normPivots = tuples.Select(tuple => tuple.normPivot).ToArray();

            var pivots = rects.Zip(normPivots, (rect, normPivot) => (rect, normPivot)).Select(tuple =>
                new Vector2(tuple.normPivot.x * tuple.rect.width, tuple.normPivot.y * tuple.rect.height)).ToArray();

            var mergedTuples = tuples.Zip(rects.Zip(pivots, (rect, pivot) => (rect, pivot)),
                (tuple, pair) => (tuple.sprite, tuple.normRect, pair.rect, pair.pivot));

            spriteDict = mergedTuples.Select(tuple =>
            {
                var scaleX = tuple.rect.width / atlasWidth / tuple.normRect.width;
                var scaleY = tuple.rect.height / atlasHeight / tuple.normRect.height;
                var offsetX = tuple.rect.x / atlasWidth - tuple.normRect.x * scaleX;
                var offsetY = tuple.rect.y / atlasHeight - tuple.normRect.y * scaleY;

                var param = new SpriteOverrideParameter()
                {
                    rect = tuple.rect,
                    pivot = tuple.pivot,
                    uvScale = new Vector2(scaleX, scaleY),
                    uvOffset = new Vector2(offsetX, offsetY)
                };
                return (tuple.sprite, param);
            }).ToDictionary(item => item.sprite, item => item.param);

            return currentAtlas;
        }


        static JObject SerializeTemplate(AvatarTemplate template)
        {
            var skeleton = BuildBoneJson(template.boneRootObj);
            BuildResolverTransformMap(template.instance);
            return JObject.FromObject(new
            {
                skeleton
            });
        }

        static JObject SerializeSpriteRenderer(SpriteRenderer spriteRenderer, SpriteLibraryAsset spriteLibraryAsset,
            Dictionary<Sprite, SpriteOverrideParameter> spriteDict)
        {
            var spriteSkin = spriteRenderer.GetComponent<SpriteSkin>();
            var resolver = spriteRenderer.GetComponent<MYTYSpriteResolver>();

            var resolverId = -1;

            var skinJO = spriteSkin.Serialize();
            var spritesJA = new JArray();

            var labels = new string[] { };
            if (resolver == null)
            {

                var sprite = spriteRenderer.sprite;
                var spriteJO = sprite.Serialize(spriteDict[sprite]);
                spritesJA.Add(spriteJO);
            }
            else
            {
                labels = spriteLibraryAsset.GetCategoryLabelNames(resolver.GetCategory()).ToArray();
                labels.Select(label => spriteLibraryAsset.GetSprite(resolver.GetCategory(), label)).ToList()
                    .ForEach(sprite => spritesJA.Add(sprite.Serialize(spriteDict[sprite])));
                resolverId = m_transformMap[spriteRenderer.transform];
            }

            return JObject.FromObject(new
            {
                useResolver = resolver != null,
                resolverId,
                labels,
                spriteRenderer.name,
                material = spriteRenderer.sharedMaterial.name,
                spriteSkin = skinJO,
                sprites = spritesJA,
                order = spriteRenderer.sortingOrder,
                maskInteraction = (int)spriteRenderer.maskInteraction
            });


        }

        static JObject BuildBoneJson(GameObject go)
        {
            var jsonObject = new JObject();
            jsonObject["name"] = go.name;
            jsonObject["transform"] = go.transform.Serialize();
            jsonObject["id"] = m_transformID;
            m_transformMap[go.transform] = m_transformID;
            m_transformID++;
            var childrenJsonList = Enumerable.Range(0, go.transform.childCount)
                .Select(idx => BuildBoneJson(go.transform.GetChild(idx).gameObject)).ToList();

            var childrenJA = new JArray();
            childrenJsonList.ForEach(child => childrenJA.Add(child));
            jsonObject["children"] = childrenJA;
            return jsonObject;
        }

        static void BuildResolverTransformMap(GameObject templateInstanceRoot)
        {
            var allResolvers = templateInstanceRoot.GetComponentsInChildren<MYTYSpriteResolver>(true);
            foreach (var mytySpriteResolver in allResolvers)
            {
                m_transformMap[mytySpriteResolver.transform] = m_transformID++;
            }
        }

        static JObject BuildRootController(RootController rootCon)
        {
            return JObject.FromObject(new
            {
                rootCon.name,
                children = Enumerable.Range(0, rootCon.transform.childCount)
                    .Select(idx => rootCon.transform.GetChild(idx))
                    .Where(child => child.GetComponent<MYTYController>() != null)
                    .Select(child => BuildControllers(child.GetComponent<MYTYController>()))
            });
        }

        static JObject BuildControllers(MYTYController controller)
        {
            var conJo = controller.SerializeToJObject(m_transformMap);
            var children = Enumerable.Range(0, controller.transform.childCount)
                .Select(idx => controller.transform.GetChild(idx))
                .Where(child => child.GetComponent<MYTYController>() != null)
                .Select(child => BuildControllers(child.GetComponent<MYTYController>()));

            m_transformMap[controller.transform] = m_transformID;

            var childrenJo = JObject.FromObject(new
            {
                id = m_transformID++,
                children
            });

            conJo.Merge(childrenJo);
            return conJo;
        }

        static string IdToFilename(string id)
        {
            return Regex.Replace(id, "[^a-zA-Z0-9_.-]+", "_", RegexOptions.Compiled);
        }
    }
}
