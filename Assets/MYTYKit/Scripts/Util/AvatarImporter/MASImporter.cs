using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MYTYKit.Components;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering;

namespace MYTYKit.AvatarImporter
{
    public class MASImporter : MonoBehaviour
    {
        public List<Transform> rootBones = new();

        // public List<Texture2D> textureAtlases = new();
        public Texture2D textureAtlas;
        public Transform templateRoot;
        public Transform avatarRoot;
        public List<Transform> rootControllers = new();
        public MotionTemplateMapper motionTemplateMapper;
        public MotionSource motionSource;
        Dictionary<int, Transform> m_transformMap = new();

        public void LoadTemplate(Transform templateRoot)
        {
            m_transformMap.Clear();
            rootControllers.Clear();
            rootBones.Clear();
            try
            {
                var metadataJson = JObject.Parse(File.ReadAllText("Assets/mas_metadata.json"));
                var templates = metadataJson["templates"].ToList();

                templates.ForEach(template =>
                {
                    rootBones.Add(LoadSkeleton(template["skeleton"] as JObject, templateRoot));
                });

                metadataJson["rootControllers"].ToList().ForEach(
                    rootCon => { rootControllers.Add(LoadRootController(rootCon as JObject, templateRoot)); });

                LoadMotionTemplates(metadataJson["mapper"] as JObject, templateRoot);

                var adapterRoot = new GameObject()
                {
                    name = "MotionAdapters",
                    transform =
                    {
                        parent = templateRoot
                    }
                };

                metadataJson["adapters"].ToList().ForEach(
                    adapter => { LoadMotionAdapter(adapter as JObject, adapterRoot.transform); });

                motionSource.motionTemplateMapperList.Add(motionTemplateMapper);
                motionSource.UpdateMotionAndTemplates();
            }
            catch (JsonException e)
            {
                Debug.LogWarning("Failed to load template");
                Debug.Log(e.StackTrace);
            }
        }

        public void LoadAvatar(string filename, Transform avatarRoot)
        {
            var avatarJO = JObject.Parse(File.ReadAllText(filename));
            var templateId = (int)avatarJO["templateId"];
            var id = (string)avatarJO["id"];

            textureAtlas = new Texture2D(2, 2);
            textureAtlas.LoadImage(File.ReadAllBytes("Assets/1.png"));
            textureAtlas.Compress(true);

            (avatarJO["spriteRenderers"] as JArray).ToList().ForEach(
                item =>
                {
                    var tf = LoadSpriteRenderer(item as JObject, templateId);
                    tf.parent = avatarRoot;
                }
            );
            UpdateSpriteControllers();
        }

        public Transform LoadSpriteRenderer(JObject spriteRendererJO, int templateId)
        {
            var spriteGO = new GameObject();

            var renderer = spriteGO.AddComponent<SpriteRenderer>();
            var skin = spriteGO.AddComponent<SpriteSkin>();

            spriteGO.name = (string)spriteRendererJO["name"];
            renderer.sortingOrder = (int)spriteRendererJO["order"];
            renderer.maskInteraction = (SpriteMaskInteraction)(int)spriteRendererJO["maskInteraction"];

            var spritesJA = spriteRendererJO["sprites"] as JArray;
            var useResolver = (bool)spriteRendererJO["useResolver"];
            if (!useResolver)
            {
                renderer.sprite = DeserializeAndCreateSprite(spritesJA[0] as JObject, textureAtlas);
            }
            else
            {
                var resolver = spriteGO.AddComponent<MYTYSpriteResolverRuntime>();
                var labels = spriteRendererJO["labels"].ToObject<string[]>();
                var sprites = spritesJA.Select(elem => DeserializeAndCreateSprite(elem as JObject, textureAtlas))
                    .ToArray();
                labels.Zip(sprites, (label, sprite) => (label, sprite)).ToList().ForEach(pair =>
                {
                    resolver.AssignSprite(pair.label, pair.sprite);
                });

                resolver.SetLabel(labels[^1]);
                var resolverId = (int)spriteRendererJO["resolverId"];
                m_transformMap[resolverId] = spriteGO.transform;
            }

            skin.Deserialize(spriteRendererJO["spriteSkin"] as JObject, rootBones[templateId].gameObject);
            return spriteGO.transform;
        }

        Transform LoadSkeleton(JObject skeleton, Transform parent)
        {
            var go = new GameObject();
            go.name = (string)skeleton["name"];
            go.transform.parent = parent;
            go.transform.Deserialize(skeleton["transform"] as JObject);
            m_transformMap[(int)skeleton["id"]] = go.transform;

            var childrenJA = skeleton["children"] as JArray;
            childrenJA.ToList().ForEach(childJson => LoadSkeleton(childJson as JObject, go.transform));
            return go.transform;
        }

        Transform LoadRootController(JObject rootController, Transform parent)
        {
            var go = new GameObject();
            go.name = (string)rootController["name"];
            go.transform.parent = parent;
            go.AddComponent<RootController>();
            rootController["children"].ToList().ForEach(childToken =>
            {
                LoadController(childToken as JObject, go.transform);
            });

            return go.transform;
        }

        void LoadController(JObject controller, Transform parent)
        {
            var go = new GameObject();
            go.transform.parent = parent;
            var typeString = (string)controller["type"];
            var assemName = typeof(MYTYController).Assembly.GetName().Name;

            Debug.Assert(!string.IsNullOrEmpty(typeString));
            var qualifiedType = "MYTYKit.Controllers." + typeString + ", " + assemName;

            var component = go.AddComponent(Type.GetType(qualifiedType)) as MYTYController;
            Debug.Assert(component != null);
            component.DeserializeFromJObject(controller, m_transformMap);
            m_transformMap[(int)controller["id"]] = go.transform;
            controller["children"].ToList().ForEach(childToken =>
            {
                LoadController(childToken as JObject, go.transform);
            });
        }

        void UpdateSpriteControllers()
        {
            templateRoot.GetComponentsInChildren<MSRSpriteController>().ToList()
                .ForEach(controller => controller.UpdateRuntimeResolvers(m_transformMap));
        }

        void LoadMotionTemplates(JObject jObject, Transform parent)
        {
            var go = new GameObject()
            {
                name = "MotionTemplateMapper",
                transform =
                {
                    parent = parent
                }
            };

            var mapper = go.AddComponent<MotionTemplateMapper>();
            mapper.DeserializeFromJObject(jObject);
            motionTemplateMapper = mapper;
        }

        void LoadMotionAdapter(JObject jObject, Transform parent)
        {
            var go = new GameObject();
            go.transform.parent = parent;
            var typeName = typeof(NativeAdapter).Namespace + "." + (string)jObject["type"] + ", "
                           + typeof(NativeAdapter).Assembly.GetName().Name;
            var adapter = (NativeAdapter)go.AddComponent(Type.GetType(typeName));
            Debug.Assert(adapter != null);

            adapter.SetMotionTemplateMapper(motionTemplateMapper);
            ((ISerializableAdapter)adapter).DeserializeFromJObject(jObject, m_transformMap);
        }

        Sprite DeserializeAndCreateSprite(JObject spriteJO, Texture2D atlas)
        {
            var rect = spriteJO["rect"].ToObject<Rect>();
            var pivot = spriteJO["pivot"].ToObject<Vector2>();
            var pixelsPerUnit = (float)spriteJO["pixelsPerUnit"];
            var spriteBones = spriteJO["bones"].ToObject<List<SpriteBone>>();

            var bindPoses = spriteJO["bindPose"].ToList().Select(token =>
            {
                var mat = new Matrix4x4();
                var elem = (token as JArray).ToArray().Select(item => (float)item).ToList();
                Enumerable.Range(0, 16).ToList().ForEach(idx => mat[idx] = elem[idx]);
                return mat;
            }).ToList();

            var positions = spriteJO["position"].ToObject<List<Vector3>>();
            var boneWeights = spriteJO["boneWeight"].ToObject<List<BoneWeight>>();
            var uvs = spriteJO["uv"].ToObject<List<Vector2>>();
            var indices = spriteJO["indices"].ToObject<List<ushort>>();

            Sprite sprite = Sprite.Create(atlas, rect, pivot, pixelsPerUnit);

            var bindPoseBuffer = new NativeArray<Matrix4x4>(bindPoses.ToArray(), Allocator.TempJob);
            var posBuffer = new NativeArray<Vector3>(positions.ToArray(), Allocator.TempJob);
            var weightBuffer = new NativeArray<BoneWeight>(boneWeights.ToArray(), Allocator.TempJob);
            var uvBuffer = new NativeArray<Vector2>(uvs.ToArray(), Allocator.TempJob);
            var indicesBuffer = new NativeArray<ushort>(indices.ToArray(), Allocator.TempJob);

            sprite.SetBones(spriteBones.ToArray());
            sprite.SetBindPoses(bindPoseBuffer);
            sprite.SetVertexCount(positions.Count);
            sprite.SetVertexAttribute(VertexAttribute.Position, posBuffer);
            sprite.SetVertexAttribute(VertexAttribute.BlendWeight, weightBuffer);
            sprite.SetVertexAttribute(VertexAttribute.TexCoord0, uvBuffer);
            sprite.SetIndices(indicesBuffer);

            bindPoseBuffer.Dispose();
            posBuffer.Dispose();
            weightBuffer.Dispose();
            uvBuffer.Dispose();
            indicesBuffer.Dispose();

            return sprite;
        }

        // // Start is called before the first frame update
        // void Start()
        // {
        //     var timestamp = Time.realtimeSinceStartup;
        //     LoadTemplate(templateRoot);
        //     //LoadAvatar("Assets/9022.json", avatarRoot);
        //     Debug.Log($"Collection Ready. {Time.realtimeSinceStartup - timestamp} second ");
        // }
        //
        // void Update()
        // {
        //     if (Input.GetKeyDown("q"))
        //     {
        //         var timestamp = Time.realtimeSinceStartup;
        //         LoadAvatar("Assets/1.json", avatarRoot);
        //         Debug.Log($"elapsed time  : {Time.realtimeSinceStartup - timestamp}");
        //     }
        // }
    }
}