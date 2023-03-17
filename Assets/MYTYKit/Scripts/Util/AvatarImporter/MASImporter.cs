using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        class ARDataRuntime
        {
            public Transform headBone;
            public RenderTexture arTexture;
            public Camera renderCam;
            public bool isValid;
        }

        public Transform templateRoot;
        public Transform avatarRoot;
        public MotionSource motionSource;
        public ShaderMapAsset shaderMap;

        public bool isAROnly => m_isAROnly;
        public string currentId => m_id;
        public RenderTexture currentARRenderTexture => m_arData.Count == 0 ? null : m_arData[m_templateId].arTexture;
        public Camera currentARCamera => m_arData.Count == 0 ? null : m_arData[m_templateId].renderCam;
        

        Texture2D m_textureAtlas;
        List<Transform> m_rootBones = new();
        List<Transform> m_rootControllers = new();
        List<ARDataRuntime> m_arData = new();
        MotionTemplateMapper m_motionTemplateMapper;
        Dictionary<int, Transform> m_transformMap = new();
        Dictionary<int, Transform> m_avatarTransformMap = new();
        Dictionary<SpriteRenderer, bool> m_useInARModeMap;
        
        bool m_isAROnly = false;
        int m_templateId;
        string m_id;


        void Start()
        {
            shaderMap = Resources.Load<ShaderMapAsset>("ShaderMap");
        }
        
        public void LoadCollectionMetadata(string filePath)
        {
            LoadCollectionMetadata(File.ReadAllBytes(filePath));
        }

        public void LoadCollectionMetadata(byte[] bytes)
        {
            m_transformMap.Clear();
            m_rootControllers.Clear();
            m_rootBones.Clear();
            try
            {
                var jsonText = "";
                using (var memoryStream = new MemoryStream(bytes))
                {
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                    {
                        var metadataEntry = zipArchive.GetEntry("collection_mas_metadata.json");
                        using (var reader = new StreamReader(metadataEntry.Open()))
                        {
                            jsonText = reader.ReadToEnd();
                        }
                    }
                }
                
                var metadataJson = JObject.Parse(jsonText);
                
                var cameraJo = metadataJson["mainCamera"] as JObject;
                var cameraGo = new GameObject();
                cameraGo.name = "MainCamera";
                cameraGo.transform.parent = templateRoot;
                cameraGo.AddComponent<Camera>().DeserializeFromJObject(cameraJo);
                
                var templates = metadataJson["templates"].ToList();
                templates.ForEach(template =>
                {
                    m_rootBones.Add(LoadSkeleton(template["skeleton"] as JObject, templateRoot));
                    LoadJointPhysics(template["jointPhysicsComponents"].ToObject<JObject[]>());
                });

                metadataJson["rootControllers"].ToList().ForEach(
                    rootCon => { m_rootControllers.Add(LoadRootController(rootCon as JObject, templateRoot)); });

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

                motionSource.motionTemplateMapperList.Add(m_motionTemplateMapper);
                motionSource.UpdateMotionAndTemplates();
                
                if(metadataJson.ContainsKey("ARFaceData")) LoadARFaceData(metadataJson["ARFaceData"] as JObject, templateRoot);
            }
            catch (JsonException e)
            {
                Debug.LogWarning("Failed to load template");
                Debug.Log(e.StackTrace);
            }
        }

        public void LoadAvatar(string filePath)
        {
            LoadAvatar(File.ReadAllBytes(filePath), Path.GetFileNameWithoutExtension(filePath));
        }

        public void LoadAvatar(byte[] bytes, string fileName)
        {
            var jsonText = "";
            byte[] pngBuffer;

            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                {
                    var metadataEntry = zipArchive.GetEntry($"{fileName}.json");
                    using (var reader = new StreamReader(metadataEntry.Open()))
                    {
                        jsonText = reader.ReadToEnd();
                    }

                    var pngEntry = zipArchive.GetEntry($"{fileName}.png");
                    using (var stream = pngEntry.Open())
                    {
                        pngBuffer = new byte[pngEntry.Length];
                        stream.Read(pngBuffer, 0, pngBuffer.Length);
                    }
                }
            }
            
            var avatarJO = JObject.Parse(jsonText);
            var templateId = (int)avatarJO["templateId"];
            var m_id = (string)avatarJO["id"];

            m_templateId = templateId;
            m_textureAtlas = new Texture2D(2, 2);
            m_textureAtlas.LoadImage(pngBuffer);
            m_textureAtlas.Compress(true);
            m_avatarTransformMap = new();
            m_useInARModeMap = new();

            (avatarJO["spriteRenderers"] as JArray).ToList().ForEach(
                item =>
                {
                    var tf = LoadSpriteRenderer(item as JObject, templateId);
                    tf.parent = avatarRoot;
                }
            );
            UpdateSpriteControllers();
        }

        public void UnloadAvatar()
        {
            foreach (var spriteRenderer in avatarRoot.GetComponentsInChildren<SpriteRenderer>())
            {
                Destroy(spriteRenderer.gameObject);
            }
        }

        public void UnloadMetadata()
        {
            foreach (var id in Enumerable.Range(0,templateRoot.childCount))
            {
                Destroy(templateRoot.GetChild(id).gameObject);
            }
        }

        public void SetARMode(bool mode)
        {
            var renderers = Enumerable.Range(0, avatarRoot.childCount)
                .Select(id => avatarRoot.transform.GetChild(id).GetComponent<SpriteRenderer>())
                .ToList();
            renderers.ForEach(renderer =>
            {
                if(!mode) renderer.gameObject.SetActive(true);
                else
                {
                    if (m_useInARModeMap.ContainsKey(renderer))
                    {
                        renderer.gameObject.SetActive(m_useInARModeMap[renderer]);
                    }
                }
            });
            
            if(mode) LockController();
            else UnlockController();
        }

        void LockController()
        {
            var rootBone = m_rootBones[m_templateId];
            var headBone = m_arData[m_templateId].headBone;
            var parentBoneList = new List<Transform>(){rootBone};
            var currBone = headBone;
            while (currBone != rootBone)
            {
                parentBoneList.Add(currBone);
                currBone = currBone.parent;
            }

            var rootController = m_rootControllers[m_templateId];
            
            rootController.GetComponentsInChildren<BoneController>()
                .Where(controller => parentBoneList.Count(bone=> controller.rigTarget.Contains(bone.gameObject))>0)
                .ToList().ForEach(controller=> controller.skip = true);
        }

        void UnlockController()
        {
            var rootController = m_rootControllers[m_templateId];
            foreach (var controller in rootController.GetComponentsInChildren<BoneController>())
            {
                controller.skip = false;
            }
        }

        Transform LoadSpriteRenderer(JObject spriteRendererJO, int templateId)
        {
            var spriteGO = new GameObject();

            var renderer = spriteGO.AddComponent<SpriteRenderer>();
            var skin = spriteGO.AddComponent<SpriteSkin>();

            spriteGO.name = (string)spriteRendererJO["name"];
            renderer.sortingOrder = (int)spriteRendererJO["order"];
            renderer.maskInteraction = (SpriteMaskInteraction)(int)spriteRendererJO["maskInteraction"];

            var matName = (string)spriteRendererJO["material"];

            var mapEntry = shaderMap.shaderMapList.FirstOrDefault(item => item.name == matName);
            if (mapEntry != null)
            {
                renderer.material = mapEntry.material;
            }
            
            m_useInARModeMap[renderer] = (bool)spriteRendererJO["useInARMode"];
            var spritesJA = spriteRendererJO["sprites"] as JArray;
            var useResolver = (bool)spriteRendererJO["useResolver"];
            if (!useResolver)
            {
                renderer.sprite = DeserializeAndCreateSprite(spritesJA[0] as JObject, m_textureAtlas);
            }
            else
            {
                var resolver = spriteGO.AddComponent<MYTYSpriteResolverRuntime>();
                var labels = spriteRendererJO["labels"].ToObject<string[]>();
                var sprites = spritesJA.Select(elem => DeserializeAndCreateSprite(elem as JObject, m_textureAtlas))
                    .ToArray();
                labels.Zip(sprites, (label, sprite) => (label, sprite)).ToList().ForEach(pair =>
                {
                    resolver.AssignSprite(pair.label, pair.sprite);
                });

                resolver.SetLabel(labels[^1]);
                var resolverId = (int)spriteRendererJO["resolverId"];
                m_avatarTransformMap[resolverId] = spriteGO.transform;
            }
            skin.Deserialize(spriteRendererJO["spriteSkin"] as JObject, m_rootBones[templateId].gameObject);
            
            var hasRigged2DMask = (bool)spriteRendererJO["hasRigged2DMask"];
            if (hasRigged2DMask)
            {
                var mask = spriteGO.AddComponent<RiggedMask2D>();
                mask.Fit();
            }
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

        void LoadJointPhysics(JObject[] physicsComponent)
        {
            foreach (var jObject in physicsComponent)
            {
                var tf = m_transformMap[(int)jObject["id"]];
                foreach (var jToken in jObject["unityComponents"].ToArray())
                {
                    var componentJo = jToken as JObject;
                    var typeKey = (string)componentJo["typeKey"];
                    var typeFullName = (string)componentJo["typeFullName"];
                    if (JointPhysicsSetting.DeserializeActions.ContainsKey(typeKey))
                    {
                        var component = tf.GetComponent(Type.GetType(typeFullName));
                        if (component == null)
                        {
                            component = tf.gameObject.AddComponent(Type.GetType(typeFullName));
                        }

                        JointPhysicsSetting.DeserializeActions[typeKey](component, componentJo, m_transformMap);
                    }
                }
            }
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
                .ForEach(controller => controller.UpdateRuntimeResolvers(m_avatarTransformMap));
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
            m_motionTemplateMapper = mapper;
        }

        void LoadMotionAdapter(JObject jObject, Transform parent)
        {
            var go = new GameObject();
            go.transform.parent = parent;
            var typeName = typeof(NativeAdapter).Namespace + "." + (string)jObject["type"] + ", "
                           + typeof(NativeAdapter).Assembly.GetName().Name;
            var adapter = (NativeAdapter)go.AddComponent(Type.GetType(typeName));
            Debug.Assert(adapter != null);

            adapter.SetMotionTemplateMapper(m_motionTemplateMapper);
            ((ISerializableAdapter)adapter).DeserializeFromJObject(jObject, m_transformMap);
        }

        void LoadARFaceData(JObject jObject, Transform templateRoot)
        {
            m_isAROnly = (bool)jObject["AROnly"];
            m_arData = jObject["items"].ToList().Select((token,idx)=>
            {
                
                var headBone = m_transformMap[(int)token["headBone"]];
                var cameraGo = new GameObject($"ARRenderCam{idx}");
                cameraGo.transform.parent = templateRoot;
                var camera = cameraGo.AddComponent<Camera>();
                camera.DeserializeFromJObject(token["renderCam"] as JObject);
                var arTexture = new RenderTexture((int)token["ARTexture"]["textureWidth"],
                    (int)token["ARTexture"]["textureHeight"], 0, RenderTextureFormat.ARGB32);
                camera.targetTexture = arTexture;
                return new ARDataRuntime()
                {
                    arTexture = arTexture,
                    headBone = headBone,
                    renderCam = camera,
                    isValid = (bool) token["isValid"]
                };
            }).ToList();
        }

        Sprite DeserializeAndCreateSprite(JObject spriteJO, Texture2D atlas)
        {
            var rect = spriteJO["rect"].ToObject<Rect>();
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

            Sprite sprite = Sprite.Create(atlas, rect, new Vector2(0.5f,0.5f), pixelsPerUnit);

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

    }
}