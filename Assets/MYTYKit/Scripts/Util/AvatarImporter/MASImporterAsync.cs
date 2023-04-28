using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MYTYKit.AvatarImporter.MASUtil;
using MYTYKit.MotionTemplates;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace MYTYKit.AvatarImporter
{
    public partial class MASImporterAsync : MonoBehaviour
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
        public MotionTemplateMapper motionTemplateMapper;
        

        public bool isAROnly => m_isAROnly;
        public string currentId => m_id;
        public RenderTexture currentARRenderTexture => m_arData.Count == 0 ? null : m_arData[m_templateId].arTexture;
        public Camera currentARCamera => m_arData.Count == 0 ? null : m_arData[m_templateId].renderCam;

        ShaderMapAsset m_shaderMap;
        Texture2D m_textureAtlas;
        List<Transform> m_rootBones = new();
        List<Transform> m_rootControllers = new();
        List<ARDataRuntime> m_arData = new();
        
        Dictionary<int, Transform> m_transformMap = new();
        Dictionary<int, Transform> m_avatarTransformMap = new();
        Dictionary<SpriteRenderer, bool> m_useInARModeMap;

        JObject m_collectionMetadataJson;
        
        bool m_isAROnly = false;
        int m_templateId;
        string m_id;

        void Awake()
        {
            m_shaderMap = Resources.Load<ShaderMapAsset>("ShaderMap");
        }

        
        
        public IEnumerator LoadCollectionMetadata(string filePath, float timeout = 0.005f, Action onComplete=null)
        {
            yield return LoadCollectionMetadata(File.ReadAllBytes(filePath),  timeout,onComplete);
        }

        public IEnumerator LoadCollectionMetadata(byte[] bytes, float timeout = 0.005f, Action onComplete = null)
        {
            m_transformMap.Clear();
            m_rootControllers.Clear();
            m_rootBones.Clear();

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

            var parser = new AsyncJsonParser();
            yield return parser.Parse(jsonText, timeout);
            
            m_collectionMetadataJson = parser.parsedObject;

            var cameraJo = m_collectionMetadataJson["mainCamera"] as JObject;
            var cameraGo = new GameObject();
            cameraGo.name = "MainCamera";
            cameraGo.transform.parent = templateRoot;
            cameraGo.AddComponent<Camera>().DeserializeFromJObject(cameraJo);

            foreach (var template in m_collectionMetadataJson["templates"])
            {

                var rootBone = new GameObject();
                var tag = rootBone.AddComponent<MASTransformIdTag>();
                tag.tag = "RootBone";
                rootBone.transform.parent = templateRoot;
                m_skeletonResumeTs = Time.realtimeSinceStartup;
                yield return LoadSkeleton(template["skeleton"] as JObject, rootBone, timeout);
                m_rootBones.Add(rootBone.transform);
                yield return LoadJointPhysics(template["jointPhysicsComponents"].ToObject<JObject[]>(), timeout);
            }

            foreach (var rootCon in m_collectionMetadataJson["rootControllers"])
            {
                var rootConGo = new GameObject();
                var tag = rootConGo.AddComponent<MASTransformIdTag>();
                tag.tag = "RootController";
                rootConGo.transform.parent = templateRoot;
                yield return LoadRootController(rootCon as JObject, rootConGo, timeout);
                m_rootControllers.Add(rootConGo.transform);
            }

            yield return LoadMotionTemplates(m_collectionMetadataJson["mapper"] as JObject, templateRoot);

            var adapterRoot = new GameObject()
            {
                name = "MotionAdapters",
                transform =
                {
                    parent = templateRoot
                }
            };

            var adapterResumeTs = Time.realtimeSinceStartup;
            foreach (var adapter in m_collectionMetadataJson["adapters"])
            {
                LoadMotionAdapter(adapter as JObject, adapterRoot.transform);
                var currentTs = Time.realtimeSinceStartup;
                if (currentTs - adapterResumeTs > timeout)
                {
                    yield return null;
                    adapterResumeTs = Time.realtimeSinceStartup;
                }
            }

            m_collectionMetadataJson["adapters"].ToList().ForEach(
                adapter => { LoadMotionAdapter(adapter as JObject, adapterRoot.transform); });

            if (m_collectionMetadataJson.ContainsKey("ARFaceData"))
                LoadARFaceData(m_collectionMetadataJson["ARFaceData"] as JObject, templateRoot);
            
            
            if(onComplete!=null) onComplete.Invoke();

        }
        
        public void CloneTemplate(GameObject srcTemplateGo, JObject arData=null)
        {
            var templateGo = Instantiate(srcTemplateGo);
            templateRoot = templateGo.transform;
            templateRoot.parent = transform;
            m_rootBones = templateRoot.GetComponentsInChildren<MASTransformIdTag>()
                .Where(tag=> tag.tag=="RootBone")
                .Select(tag=>tag.transform).ToList();
            m_rootControllers = templateRoot.GetComponentsInChildren<MASTransformIdTag>()
                .Where(tag => tag.tag == "RootController")
                .Select(tag => tag.transform).ToList();

            m_transformMap = new();
            foreach (var tag in templateRoot.GetComponentsInChildren<MASTransformIdTag>())
            {
                if (tag.id >= 0)
                {
                    m_transformMap[tag.id] = tag.transform;
                }
            }

            if (arData != null)
            {
                LoadARFaceData(arData,templateRoot);
            }
        }

        public JObject ExtractARMetadata()
        { 
            return m_collectionMetadataJson["ARFaceData"] as JObject;
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

        public IEnumerator LoadAvatar(string filePath, float timeout = 0.005f, Action onComplete=null)
        {
            yield return LoadAvatar(File.ReadAllBytes(filePath), Path.GetFileNameWithoutExtension(filePath), timeout, onComplete);
        }

        public IEnumerator LoadAvatar(byte[] bytes, string fileName, float timeout = 0.005f, Action onComplete=null)
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
            
            var parser = new AsyncJsonParser();
            yield return parser.Parse(jsonText, timeout);
            var avatarJO = parser.parsedObject;
            
            var templateId = (int)avatarJO["templateId"];
            var id = (string)avatarJO["id"];

            m_templateId = templateId;
            m_textureAtlas = new Texture2D(2, 2);
            m_textureAtlas.LoadImage(pngBuffer);
            m_textureAtlas.Compress(true);
            m_avatarTransformMap = new();
            m_useInARModeMap = new();
            yield return null;

            m_spriteRendererResumeTs = Time.realtimeSinceStartup;
            foreach (var item in avatarJO["spriteRenderers"])
            {
                var spriteGo = new GameObject();
                spriteGo.transform.parent = avatarRoot;
                yield return LoadSpriteRenderer(item as JObject, spriteGo,templateId, timeout);
            }
            UpdateSpriteControllers();
            
            if(onComplete!=null) onComplete.Invoke();
        }

        public void UnloadAvatar()
        {
            foreach (var spriteRenderer in avatarRoot.GetComponentsInChildren<SpriteRenderer>())
            {
                Destroy(spriteRenderer.gameObject);
            }
        }



    }
}