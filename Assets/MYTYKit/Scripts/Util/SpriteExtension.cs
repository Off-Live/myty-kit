using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;

namespace MYTYKit
{
    public class SpriteOverrideParameter
    {
        public Rect rect;
        public Vector2 uvScale;
        public Vector2 uvOffset;
    }

    public static class SpriteExtension
    {
        public static JObject Serialize(this Sprite sprite, SpriteOverrideParameter spriteOverrideParameter = null)
        {
            var spriteJO = new JObject();
            if (spriteOverrideParameter == null)
            {
                spriteJO["rect"] = JObject.FromObject(new
                {
                    sprite.rect.x,
                    sprite.rect.y,
                    sprite.rect.width,
                    sprite.rect.height
                });
            }
            else
            {
                var overrideRect = spriteOverrideParameter.rect;
                spriteJO["rect"] = JObject.FromObject(new
                {
                    overrideRect.x,
                    overrideRect.y,
                    overrideRect.width,
                    overrideRect.height
                });
            }

            spriteJO["pixelsPerUnit"] = sprite.pixelsPerUnit;

            var spriteBoneJOList = sprite.GetBones().ToList().Select(spriteBone => JObject.FromObject(new
                {
                    color = new
                    {
                        spriteBone.color.r,
                        spriteBone.color.g,
                        spriteBone.color.b,
                        spriteBone.color.a
                    },
                    spriteBone.length,
                    spriteBone.name,
                    position = new
                    {
                        spriteBone.position.x,
                        spriteBone.position.y,
                        spriteBone.position.z
                    },
                    rotation = new
                    {
                        spriteBone.rotation.x,
                        spriteBone.rotation.y,
                        spriteBone.rotation.z,
                        spriteBone.rotation.w
                    },
                    spriteBone.parentId
                })
            ).ToList();

            var spriteBoneJA = new JArray();
            spriteBoneJOList.ForEach(item => spriteBoneJA.Add(item));
            spriteJO["bones"] = spriteBoneJA;

            var bindPoseJA = new JArray();

            sprite.GetBindPoses().ToList().Select(bindpose =>
            {
                var retJA = new JArray();
                Enumerable.Range(0, 16).Select(idx => bindpose[idx]).ToList().ForEach(item => retJA.Add(item));
                return retJA;
            }).ToList().ForEach(item => bindPoseJA.Add(item));
            spriteJO["bindPose"] = bindPoseJA;

            var positionJA = new JArray();
            sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position).ToList().Select(position =>
                JObject.FromObject(new
                {
                    position.x,
                    position.y,
                    position.z
                })).ToList().ForEach(item => positionJA.Add(item));
            spriteJO["position"] = positionJA;

            var weightJA = new JArray();
            sprite.GetVertexAttribute<BoneWeight>(VertexAttribute.BlendWeight).ToList().Select(weight =>
                JObject.FromObject(new
                {
                    weight.weight0,
                    weight.weight1,
                    weight.weight2,
                    weight.weight3,
                    weight.boneIndex0,
                    weight.boneIndex1,
                    weight.boneIndex2,
                    weight.boneIndex3
                })).ToList().ForEach(item => weightJA.Add(item));
            spriteJO["boneWeight"] = weightJA;

            var uvJA = new JArray();
            sprite.GetVertexAttribute<Vector2>(VertexAttribute.TexCoord0).ToList().Select(uv =>
            {
                var newUv = new Vector2();
                if (spriteOverrideParameter == null)
                {
                    newUv = uv;
                }
                else
                {
                    newUv.x = uv.x * spriteOverrideParameter.uvScale.x + spriteOverrideParameter.uvOffset.x;
                    newUv.y = uv.y * spriteOverrideParameter.uvScale.y + spriteOverrideParameter.uvOffset.y;
                }

                return JObject.FromObject(new
                {
                    newUv.x,
                    newUv.y
                });
            }).ToList().ForEach(item => uvJA.Add(item));
            spriteJO["uv"] = uvJA;

            var indicesJA = new JArray();
            sprite.GetIndices().ToList().ForEach(idx => indicesJA.Add(idx));
            spriteJO["indices"] = indicesJA;

            return spriteJO;
        }

        public static JObject Serialize(this SpriteSkin spriteSkin)
        {
            var spriteSkinJO = new JObject();
            spriteSkinJO["rootBone"] = spriteSkin.rootBone.name;
            var bonenameJA = new JArray();
            spriteSkin.boneTransforms.Select(boneTf => boneTf.name).ToList().ForEach(item => bonenameJA.Add(item));
            spriteSkinJO["boneTransforms"] = bonenameJA;
            return spriteSkinJO;
        }

        public static void Deserialize(this SpriteSkin spriteSkin, JObject skinJO, GameObject rootObj)
        {
            var bones = rootObj.GetComponentsInChildren<Transform>();

            var queryResult = bones.Where(bone => bone.name == (string)skinJO["rootBone"]).ToArray();
            if (queryResult.Length == 0)
            {
                Debug.LogWarning("No root bone named " + (string)skinJO["rootBone"]);
                return;
            }

            var rootBone = queryResult[0];

            var boneTransforms = (skinJO["boneTransforms"] as JArray).ToList().Select(token =>
            {
                var tmp = bones.Where(bone => bone.name == (string)token).ToList();
                if (tmp.Count == 0) return null;
                return tmp[0];
            }).ToArray();

            var rootBoneProperty = typeof(SpriteSkin).GetProperty(nameof(SpriteSkin.rootBone));
            rootBoneProperty!.SetValue(spriteSkin, rootBone, BindingFlags.NonPublic | BindingFlags.Instance, null, null,
                CultureInfo.InvariantCulture);


            var boneTransformsProperty = typeof(SpriteSkin).GetProperty(nameof(SpriteSkin.boneTransforms));
            boneTransformsProperty!.SetValue(spriteSkin, boneTransforms, BindingFlags.NonPublic | BindingFlags.Instance,
                null, null, CultureInfo.InvariantCulture);

        }

    }
}