using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.Controllers
{
    public class ByPassController : BoneController
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;

        private Vector3 _orgPosition;
        private Vector3 _orgScale;
        private Quaternion _orgRotation;


        // Start is called before the first frame update
        // void Start()
        // {
        //     if (rigTarget == null || rigTarget.Count==0) return;
        //     _orgPosition = rigTarget[0].transform.localPosition;
        //     _orgScale = rigTarget[0].transform.localScale;
        //     _orgRotation = rigTarget[0].transform.localRotation;
        //
        //     position = new Vector3();
        //     scale = new Vector3(1,1,1);
        //     rotation = Quaternion.identity;
        //
        //     orgRig.Add(new RiggingEntity
        //     {
        //         position = position,
        //         scale = scale,
        //         rotation = rotation
        //     });
        // }

        // Update is called once per frame
        void Update()
        {
            if (rigTarget == null || rigTarget.Count==0) return;
            //rigTarget[0].transform.localPosition = position;
            rigTarget[0].transform.rotation = rotation;
        }

        public override void ApplyDiff()
        {
            // rigTarget[0].transform.localPosition = position + _orgPosition;
            // rigTarget[0].transform.localScale = new Vector3(_orgScale.x * scale.x , _orgScale.y* scale.y, _orgScale.z*scale.z);
            // rigTarget[0].transform.localRotation = rotation * _orgRotation;
        }

        protected override List<RiggingEntity> CalcInterpolate()
        {
            throw new System.NotImplementedException();
        }
    }
}
