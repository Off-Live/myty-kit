using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using MYTYKit.Components;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D.Animation;
using Object = UnityEngine.Object;


namespace MYTYKit
{
    
    public class MASExporter
    {

        static int m_transformID = 0;
        static Dictionary<Transform, int> m_transformMap;
        static Dictionary<Sprite, Texture2D> m_spriteTextureMap;
        static List<List<string>> m_arTraitList;
        static Dictionary<SpriteRenderer, (string path, int templateIndex)> m_traitPathMap;
        const string ExportPath = "Assets/MYTYAsset/MAS";

        public static void ExportFromCLI()
        {
            Export(true);
        }

        public static void Export(bool fromCLI, List<string> idList=null)
        {
            if (fromCLI && !Application.isBatchMode)
            {
                Debug.LogError("This method can be called from batchmode only.");
                return;
            }
            m_transformID = 0;
            m_transformMap = new();
            m_spriteTextureMap = new();
            EditorSceneManager.OpenScene(PackageExporter.ScenePath, OpenSceneMode.Single);
            
            SpriteResolverCleaner.FixAvatarSprite();
            var selector = GameObject.FindObjectOfType<AvatarSelector>();
            if (selector == null) return;
            selector.Configure();
            selector.PrepareForExporting();

            var tempSlaPaths = new List<string>();
            selector.templates.ForEach(template =>
            {
                var psb = AssetDatabase.LoadAssetAtPath<GameObject>(template.PSBPath);
                var tempPath = "Assets/"+template.instance.name + ".asset";
                tempSlaPaths.Add(tempPath);
                SpriteLibraryFactory.CreateLibrary(psb, tempPath);
                var spriteLib = template.instance.GetComponent<SpriteLibrary>();
                var spriteLibraryAsset =  AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(tempPath);
                spriteLib.spriteLibraryAsset = spriteLibraryAsset;
                template.spriteLibrary = spriteLibraryAsset;
            });
        
            var mapper = Object.FindObjectOfType<MotionTemplateMapper>();
            
            var templates = selector.templates.Select(template => SerializeTemplate(template));
            var rootControllers = Object.FindObjectsOfType<RootController>()
                .Select(BuildRootController);
            var adapters = Object.FindObjectsOfType<NativeAdapter>().Select(adapter => (adapter as ISerializableAdapter)).ToArray();
        
            var exportedJO = JObject.FromObject(new
            {
                mainCamera = Camera.main.SerializeToJObject(),
                templates,
                rootControllers,
                mapper= mapper.SerializeToJObject(),
                adapters = adapters.Select(adapter => adapter.SerializeToJObject(m_transformMap)),
            });

            var arJson = BuildARMetadata(selector.templates.First().instance.transform.parent);
            if (arJson != null) exportedJO["ARFaceData"] = arJson;
            
            GeneratePathForSpriteRenderers(selector);
            
            Directory.CreateDirectory(ExportPath);
            var files = Directory.GetFiles(ExportPath);
            foreach (var file in files)
            {
                File.Delete(file);
            }
            
            using (var zipArchive = ZipFile.Open($"{ExportPath}/collection_mas_metadata.zip", ZipArchiveMode.Create))
            {
                var dataEntry = zipArchive.CreateEntry("collection_mas_metadata.json");
                using (var writer = new StreamWriter(dataEntry.Open()))
                {
                    writer.Write(exportedJO.ToString(Formatting.Indented));
                }
            }

            var commandLineArgs = System.Environment.GetCommandLineArgs();
            var avatarIdArgIndex = Array.IndexOf(commandLineArgs, "-avatarId");

            var avatarIdList = new List<string>();
            
            if (avatarIdArgIndex < 0)
            {
                if (idList == null) avatarIdList = selector.tokenIdArray.ToList();
                else avatarIdList = idList;
            }
            else
            {
                avatarIdList = commandLineArgs[avatarIdArgIndex + 1].Split(",").ToList();
            }
            
            ExportAvatar(avatarIdList,selector);
            foreach (var path in tempSlaPaths)
            {
                AssetDatabase.DeleteAsset(path);
            }

        }

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
                var filename = ConvertIdToFilename(id);
                
                var spriteRenderers = templateRoot.GetComponentsInChildren<SpriteRenderer>()
                    .Where(renderer => renderer.GetComponent<SpriteSkin>() != null).Select(renderer =>
                        SerializeSpriteRenderer(renderer, spriteLibraryAsset, spriteDict)).ToArray();

                var avatarJO = JObject.FromObject(new
                {
                    templateId, id, spriteRenderers
                });


                using (var zipArchive = ZipFile.Open($"{ExportPath}/{filename}.zip", ZipArchiveMode.Create))
                {
                    var atlasEntry = zipArchive.CreateEntry($"{filename}.png");
                    using (var stream = atlasEntry.Open())
                    {
                        stream.Write(currentAtlas.EncodeToPNG());
                    }
                    
                    var dataEntry = zipArchive.CreateEntry($"{filename}.json");
                    using (var writer = new StreamWriter(dataEntry.Open()))
                    {
                        writer.Write(avatarJO.ToString(Formatting.Indented));
                    }
                }
                Object.DestroyImmediate(currentAtlas);
                
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

        static (Sprite sprite, Texture2D subTexture, Rect normRect)
            GenerateTupleFromSprite(Sprite sprite, Texture2D globalAtlas)
        {
            Debug.Assert(sprite != null);
            var rect = sprite.rect;
            var startX = Mathf.RoundToInt(rect.x);
            var startY = Mathf.RoundToInt(rect.y);
            var width = Mathf.RoundToInt(rect.width);
            var height = Mathf.RoundToInt(rect.height);

            Texture2D subTexture = null;

            if (m_spriteTextureMap.ContainsKey(sprite))
            {
                subTexture = m_spriteTextureMap[sprite];
            }
            else
            {
                subTexture = new Texture2D(width, height);
                var pixels = globalAtlas.GetPixels(startX, startY, width, height);
                subTexture.SetPixels(pixels);
                subTexture.Apply();
                m_spriteTextureMap[sprite] = subTexture;
            }
            
            var normRect = new Rect(rect.x / globalAtlas.width, rect.y / globalAtlas.height,
                rect.width / globalAtlas.width, rect.height / globalAtlas.height);
            return (sprite, subTexture, normRect);
        }

        static Texture2D ExtractCurrentTextureAtlas(Texture2D globalAtlas, SpriteLibraryAsset spriteLibraryAsset,
            out Dictionary<Sprite, SpriteOverrideParameter> spriteDict)
        {
            var selector = Object.FindObjectOfType<AvatarSelector>();
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
            
      
            var mergedTuples = tuples.Zip(rects,
                (tuple, rect) => (tuple.sprite, tuple.normRect, rect));

            spriteDict = mergedTuples.Select(tuple =>
            {
                var scaleX = tuple.rect.width / atlasWidth / tuple.normRect.width;
                var scaleY = tuple.rect.height / atlasHeight / tuple.normRect.height;

                if (tuple.normRect.width < 1.0e-6) scaleX = 0;
                if (tuple.normRect.height < 1.0e-6) scaleY = 0;
                
                var offsetX = tuple.rect.x / atlasWidth - tuple.normRect.x * scaleX;
                var offsetY = tuple.rect.y / atlasHeight - tuple.normRect.y * scaleY;

                var param = new SpriteOverrideParameter()
                {
                    rect = tuple.rect,
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
            var jointPhysicsComponents = BuildJointPhysicsJson(template.boneRootObj.transform);
            BuildResolverTransformMap(template.instance);
            return JObject.FromObject(new
            {
                skeleton,jointPhysicsComponents
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

            var useInARMode = false;

            if (m_arTraitList != null)
            {
                var (path, index) = m_traitPathMap[spriteRenderer];
                useInARMode = m_arTraitList[index].Count(item => path.StartsWith(item+"/") || path==item) > 0;
            }

            var hasRigged2DMask = spriteRenderer.GetComponent<RiggedMask2D>() != null;
            
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
                maskInteraction = (int)spriteRenderer.maskInteraction,
                useInARMode,
                hasRigged2DMask
            });


        }

        static JObject BuildBoneJson(GameObject go)
        {
            var jsonObject = new JObject();
            
            jsonObject["transform"] = go.transform.Serialize();
            jsonObject["id"] = m_transformID;
            go.name = go.name + "_ID_" + m_transformID;
            jsonObject["name"] = go.name;
            
            m_transformMap[go.transform] = m_transformID;
            m_transformID++;
            var childrenJsonList = Enumerable.Range(0, go.transform.childCount)
                .Select(idx => BuildBoneJson(go.transform.GetChild(idx).gameObject)).ToList();

            var childrenJA = new JArray();
            childrenJsonList.ForEach(child => childrenJA.Add(child));
            jsonObject["children"] = childrenJA;
            return jsonObject;
        }

        static JObject[] BuildJointPhysicsJson(Transform rootBone)
        {
            return rootBone.GetComponentsInChildren<Transform>()
                .Where(tf => JointPhysicsSetting.SerializeActions.Keys.Count(type => tf.GetComponent(type) != null) > 0)
                .Select(tf =>
                {

                    return JObject.FromObject(new
                    {
                        id = m_transformMap[tf],
                        unityComponents = JointPhysicsSetting.SerializeActions.Keys
                            .Where(type => tf.GetComponent(type) != null)
                            .Select(type =>
                            {
                                var component = tf.GetComponent(type);
                                var componentJo = JointPhysicsSetting.SerializeActions[type](component, m_transformMap);
                                componentJo["typeFullName"] = type.FullName+","+type.Assembly.GetName().Name;
                                componentJo["typeKey"] = type.Name;
                                return componentJo;
                            })
                    });
                }).ToArray();
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
                    .Where(child => child.GetComponent<MYTYController>() != null && child.gameObject.activeSelf)
                    .Select(child => BuildControllers(child.GetComponent<MYTYController>()))
            });
        }

        static JObject BuildControllers(MYTYController controller)
        {
            var conJo = controller.SerializeToJObject(m_transformMap);
            var children = Enumerable.Range(0, controller.transform.childCount)
                .Select(idx => controller.transform.GetChild(idx))
                .Where(child => child.GetComponent<MYTYController>() != null && child.gameObject.activeSelf)
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

        static JObject BuildARMetadata(Transform templateRoot)
        {
            var arAsset = AssetDatabase.LoadAssetAtPath<ARFaceAsset>(MYTYUtil.AssetPath + "/ARFaceData.asset");
            var transformList = templateRoot.GetComponentsInChildren<Transform>().ToList();
            m_arTraitList = null;
            
            if (arAsset == null) return null;
            m_arTraitList = new List<List<string>>();
            
            var items = arAsset.items.Select(item =>
            {
                var headBone = transformList.FirstOrDefault(tf =>
                    PrefabUtility.GetCorrespondingObjectFromSource(tf.gameObject) == item.headBone);
                m_arTraitList.Add(item.traits);

                var textureWidth = 512;
                var textureHeight = 512;

                if (item.renderCam.targetTexture != null)
                {
                    textureWidth = item.renderCam.targetTexture.width;
                    textureHeight = item.renderCam.targetTexture.height;
                }
                
                return JObject.FromObject(new
                {
                    headBone = headBone == null ? -1 : m_transformMap[headBone],
                    isValid = headBone != null && item.isValid,
                    renderCam = item.renderCam.SerializeToJObject(),
                    ARTexture = new
                    {
                        textureWidth,textureHeight
                    }
                });
            });

            return JObject.FromObject(new
            {
                arAsset.AROnly,
                items
            });
        }

        static void GeneratePathForSpriteRenderers(AvatarSelector selector)
        {
            m_traitPathMap = new();
            var idx = 0;
            selector.templates.ForEach(template =>
            {
                foreach (var renderer in template.instance.GetComponentsInChildren<SpriteRenderer>())
                {
                    var curr = renderer.transform;
                    var path = "";
                    while (curr != template.instance.transform)
                    {
                        path = "/" + curr.name + path;
                        curr = curr.parent;
                    }
                    path = path.Substring(1);
                    m_traitPathMap[renderer] = (path, idx);
                }

                idx++;
            });
        }

        static string ConvertIdToFilename(string id)
        {
            return Regex.Replace(id, "[^a-zA-Z0-9_.-]+", "_", RegexOptions.Compiled);
        }
    }
}
