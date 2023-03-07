using MYTYKit.MotionTemplates;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionAdapters{
    public abstract class NativeAdapter : MonoBehaviour
    {
        protected MotionTemplateMapper motionTemplateMapper;
        public void SetMotionTemplateMapper(MotionTemplateMapper mapper)
        {
            motionTemplateMapper = mapper;
        }
    }
}
