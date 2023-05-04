using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MYTYKit.Controllers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.Controllers
{
    public class BoneBlendShapeController : BoneController
    {
        [Serializable]
        public class BlendShapeBasis
        {
            public string name;
            public float weight = 0.0f;
            public bool active = false;
            public bool assigned = false;
            public List<Vector3> basis;
        }

        public List<BlendShapeBasis> blendShapes = new();

        List<Vector3> m_diffBuffer;

        void Start()
        {

        }
        
        void Update()
        {
            m_diffBuffer = CalcBlendShape();
        }

        public void UpdateInEditor()
        {

            Update();
            ToOrigin();
            ApplyDiff();

        }

        public override void ApplyDiff()
        {
            if (m_diffBuffer == null || m_diffBuffer.Count == 0) return;
            var diffEntity = m_diffBuffer.Select(item => new RiggingEntity()
            {
                position = item,
                rotation = Quaternion.identity,
                scale = new Vector3(1, 1, 1)
            }).ToList();
            AccumulatePose(diffEntity);
        }

        protected override List<RiggingEntity> CalcInterpolate()
        {
            throw new NotImplementedException();
        }

        List<Vector3> CalcBlendShape()
        {
            if (blendShapes.Count == 0) return null;

            return blendShapes.Select(bs =>
                orgRig.Zip(bs.basis, (origin, basis) => (origin, basis))
                    .Select(pair =>
                    { 
                        return bs.active ? bs.weight * (pair.basis - pair.origin.position) : Vector3.zero;
                    })
            ).Aggregate((acc, next) =>
                acc.Zip(next, (a, b) => (a, b))
                    .Select(pair =>
                        pair.a + pair.b)).ToList();

        }

        public override JObject SerializeToJObject(Dictionary<Transform, int> tfMap)
        {
            var baseJo = base.SerializeToJObject(tfMap);
            var jo = JObject.FromObject(new
            {
                name,
                type = GetType().Name,
                blendShapes = blendShapes.Select( basis => JObject.FromObject(new
                {
                    basis.name,
                    basis.active,
                    basis.assigned,
                    basis = basis.basis.Select(item => JObject.FromObject(new
                    {
                        item.x,
                        item.y,
                        item.z
                    }))
                }))
            });
            baseJo.Merge(jo);
            return baseJo;
        }

        public override void DeserializeFromJObject(JObject jObject, Dictionary<int, Transform> idTransformMap)
        {
            base.DeserializeFromJObject(jObject, idTransformMap);
            name = (string)jObject["name"];
            blendShapes = (jObject["blendShapes"] as JArray).Select(token =>
            {
                var basisJo = token as JObject;
                return new BlendShapeBasis()
                {
                    name = (string)basisJo["name"],
                    active = (bool)basisJo["active"],
                    assigned = (bool)basisJo["assigned"],
                    basis = (basisJo["basis"] as JArray).Select(item =>
                    {
                        var itemJo = item as JObject;
                        return new Vector3((float)itemJo["x"], (float)itemJo["y"], (float)itemJo["z"]);
                    }).ToList()
                };
            }).ToList();
        }
    }
}