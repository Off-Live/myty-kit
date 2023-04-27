using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Components;
using MYTYKit.Controllers;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;

namespace MYTYKit.AvatarImporter
{
    public partial class MASImporterAsync
    {
        float m_spriteRendererResumeTs = 0.0f;
        Sprite m_latestSprite = null;
        IEnumerator LoadSpriteRenderer(JObject spriteRendererJO, GameObject spriteGO, int templateId, float timeout)
        {
            var renderer = spriteGO.AddComponent<SpriteRenderer>();
            var skin = spriteGO.AddComponent<SpriteSkin>();

            spriteGO.name = (string)spriteRendererJO["name"];
            renderer.sortingOrder = (int)spriteRendererJO["order"];
            renderer.maskInteraction = (SpriteMaskInteraction)(int)spriteRendererJO["maskInteraction"];

            var matName = (string)spriteRendererJO["material"];

            var mapEntry = m_shaderMap.shaderMapList.FirstOrDefault(item => item.name == matName);
            if (mapEntry != null)
            {
                renderer.material = mapEntry.material;
            }

            var currentTs = Time.realtimeSinceStartup;

            if (currentTs - m_spriteRendererResumeTs > timeout)
            {
                yield return null;
                m_spriteRendererResumeTs = currentTs;
            }
            
            m_useInARModeMap[renderer] = (bool)spriteRendererJO["useInARMode"];
            var spritesJA = spriteRendererJO["sprites"] as JArray;
            var useResolver = (bool)spriteRendererJO["useResolver"];
            if (!useResolver)
            {
                yield return DeserializeAndCreateSprite(spritesJA[0] as JObject, m_textureAtlas, timeout);
                renderer.sprite = m_latestSprite;
            }
            else
            {
                var resolver = spriteGO.AddComponent<MYTYSpriteResolverRuntime>();
                var labels = spriteRendererJO["labels"].ToObject<string[]>();
                var spritesList = new List<Sprite>();
                foreach (var elem in spritesJA)
                {
                    yield return DeserializeAndCreateSprite(elem as JObject, m_textureAtlas, timeout);
                    spritesList.Add(m_latestSprite);
                }

                labels.Zip(spritesList, (label, sprite) => (label, sprite)).ToList().ForEach(pair =>
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
            
        }
        
        IEnumerator DeserializeAndCreateSprite(JObject spriteJO, Texture2D atlas, float timeout)
        {
            var currentTs = Time.realtimeSinceStartup;

            if (currentTs - m_spriteRendererResumeTs > timeout)
            {
                yield return null;
                m_spriteRendererResumeTs = currentTs;
            }

            var rect = spriteJO["rect"].ToObject<Rect>();
            var pixelsPerUnit = (float)spriteJO["pixelsPerUnit"];
            var spriteBones = spriteJO["bones"].ToArray().Select(token =>
            {
                return new SpriteBone()
                {
                    name = (string)token["name"],
                    color = new Color32((byte)token["color"]["r"], (byte)token["color"]["g"], (byte)token["color"]["b"],
                        (byte)token["color"]["a"]),
                    length = (float)token["length"],
                    position = new Vector3((float)token["position"]["x"], (float)token["position"]["y"],
                        (float)token["position"]["z"]),
                    rotation = new Quaternion((float)token["rotation"]["x"], (float)token["rotation"]["y"],
                        (float)token["rotation"]["z"], (float)token["rotation"]["w"]),
                    parentId = (int)token["parentId"]
                };
            }).ToList();

            var bindPoses = spriteJO["bindPose"].ToList().Select(token =>
            {
                var mat = new Matrix4x4();
                var elem = (token as JArray).ToArray().Select(item => (float)item).ToList();
                Enumerable.Range(0, 16).ToList().ForEach(idx => mat[idx] = elem[idx]);
                return mat;
            }).ToList();

            var positions = spriteJO["position"].ToObject<List<Vector3>>();
            var boneWeights = spriteJO["boneWeight"].Select(token =>
            {
                return new BoneWeight()
                {
                    weight0 = (float)token["weight0"],
                    weight1 = (float)token["weight1"],
                    weight2 = (float)token["weight2"],
                    weight3 = (float)token["weight3"],
                    boneIndex0 = (int)token["boneIndex0"],
                    boneIndex1 = (int)token["boneIndex1"],
                    boneIndex2 = (int)token["boneIndex2"],
                    boneIndex3 = (int)token["boneIndex3"]

                };
            }).ToList();
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

            m_latestSprite = sprite;
        }

        void UpdateSpriteControllers()
        {
            templateRoot.GetComponentsInChildren<MSRSpriteController>().ToList()
                .ForEach(controller => controller.UpdateRuntimeResolvers(m_avatarTransformMap));
        }
        
    }
}