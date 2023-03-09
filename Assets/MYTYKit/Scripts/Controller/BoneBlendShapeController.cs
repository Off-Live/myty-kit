using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MYTYKit.Controllers;
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
            public List<Vector3> basis;
        }

        public List<BlendShapeBasis> blendShapes = new();

        List<Vector3> m_diffBuffer;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
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
                    .Select(pair => bs.weight * (pair.basis - pair.origin.position))
            ).Aggregate((acc, next) =>
                acc.Zip(next, (a, b) => (a, b))
                    .Select(pair =>
                        pair.a + pair.b)).ToList();

        }
    }
}