using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MYTYKit.AvatarImporter.MASUtil;
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
    public class MASImporterAsync : MonoBehaviour
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
        
        bool m_isAROnly = false;
        int m_templateId;
        string m_id;


        void Start()
        {
            m_shaderMap = Resources.Load<ShaderMapAsset>("ShaderMap");
        }
        
        public IEnumerator LoadCollectionMetadata(string filePath, float timeout = 0.005f)
        {
            yield return LoadCollectionMetadata(File.ReadAllBytes(filePath),  5.0f);
        }

        public IEnumerator LoadCollectionMetadata(byte[] bytes, float timeout = 0.005f)
        {
            var initTimestamp = Time.realtimeSinceStartup;
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


            // var metadataJson = JObject.Parse(jsonText);
            //
            // var cameraJo = metadataJson["mainCamera"] as JObject;
            // var cameraGo = new GameObject();
            // cameraGo.name = "MainCamera";
            // cameraGo.transform.parent = templateRoot;
            // cameraGo.AddComponent<Camera>().DeserializeFromJObject(cameraJo);
            //
            // var templates = metadataJson["templates"].ToList();
            // templates.ForEach(template =>
            // {
            //     m_rootBones.Add(LoadSkeleton(template["skeleton"] as JObject, templateRoot));
            //     LoadJointPhysics(template["jointPhysicsComponents"].ToObject<JObject[]>());
            // });
            //
            // metadataJson["rootControllers"].ToList().ForEach(
            //     rootCon => { m_rootControllers.Add(LoadRootController(rootCon as JObject, templateRoot)); });
            //
            // LoadMotionTemplates(metadataJson["mapper"] as JObject, templateRoot);
            //
            // var adapterRoot = new GameObject()
            // {
            //     name = "MotionAdapters",
            //     transform =
            //     {
            //         parent = templateRoot
            //     }
            // };
            //
            // metadataJson["adapters"].ToList().ForEach(
            //     adapter => { LoadMotionAdapter(adapter as JObject, adapterRoot.transform); });
            //
            // // motionSource.motionTemplateMapperList.Add(m_motionTemplateMapper);
            // // motionSource.UpdateMotionAndTemplates();
            //
            // if(metadataJson.ContainsKey("ARFaceData")) LoadARFaceData(metadataJson["ARFaceData"] as JObject, templateRoot);

        }

       
       
    }
}