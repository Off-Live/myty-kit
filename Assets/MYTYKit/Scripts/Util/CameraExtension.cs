using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit
{
    public static class CameraExtension
    {
        public static JObject SerializeToJObject(this Camera camera)
        {
            return JObject.FromObject(new
            {
                transform = camera.transform.Serialize(),
                camera.clearFlags,
                backgroundColor = new
                {
                    camera.backgroundColor.r,
                    camera.backgroundColor.g,
                    camera.backgroundColor.b,
                    camera.backgroundColor.a,
                },
                camera.cullingMask,
                camera.orthographic,
                camera.orthographicSize,
                camera.farClipPlane,
                camera.nearClipPlane,
                rect = new
                {
                    camera.rect.x,
                    camera.rect.y,
                    camera.rect.width,
                    camera.rect.height
                },
                camera.depth,
                camera.renderingPath,
                camera.useOcclusionCulling,
                camera.allowHDR,
                camera.allowMSAA,
                camera.allowDynamicResolution
            });
        }

        public static void DeserializeFromJObject(this Camera camera, JObject jObject)
        {
            camera.transform.Deserialize(jObject["transform"] as JObject);
            camera.clearFlags = (CameraClearFlags)(int)jObject["clearFlags"] ;
            camera.backgroundColor = jObject["backgroundColor"].ToObject<Color>();
            camera.cullingMask = (int)jObject["cullingMask"];
            camera.orthographic = (bool)jObject["orthographic"];
            camera.orthographicSize = (float)jObject["orthographicSize"];
            camera.farClipPlane = (float)jObject["farClipPlane"];
            camera.nearClipPlane = (float)jObject["nearClipPlane"];
            camera.rect = jObject["rect"].ToObject<Rect>();
            camera.depth = (float)jObject["depth"];
            camera.renderingPath = (RenderingPath)(int)jObject["renderingPath"];
            camera.useOcclusionCulling = (bool)jObject["useOcclusionCulling"];
            camera.allowHDR = (bool)jObject["allowHDR"];
            camera.allowMSAA = (bool)jObject["allowMSAA"];
            camera.allowDynamicResolution = (bool)jObject["allowDynamicResolution"];
        }
    }
    
}