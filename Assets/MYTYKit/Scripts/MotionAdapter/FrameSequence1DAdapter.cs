using System.Collections.Generic;
using MYTYKit.Controllers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    public class FrameSequence1DAdapter : NativeAdapter, ISerializableAdapter
    {
        public MYTYController controller;

        public float start = 0.0f;
        public float end = 1.0f;
        public bool repeat = true;
        public bool swing = false;

        public float stepCount = 10;
        public float unitTime = 0.05f;


        private float m_curValue = 0.0f;
        private float m_elapsed = 0.0f;

        private bool m_stopped = false;
        private bool m_forward = true;

        private IComponentWiseInput m_input;

        public void Start()
        {
            m_input = controller as IComponentWiseInput;
            if (m_input == null) return;
            m_input.SetComponent(m_curValue,0);
            m_curValue = start+ (end - start) / stepCount * 0.5f;
        }

        void Update()
        {
            if (m_input == null) return;
            if (m_stopped) return;

            if (m_elapsed < unitTime)
            {
                m_elapsed += Time.deltaTime;
                return;
            }

            m_elapsed = 0.0f;

            var step = (end - start) / stepCount;
            var halfstep = step * 0.5f;
            if (!m_forward) step *= -1;
            m_curValue += step;
            
            if (m_forward)
            {
                if (m_curValue>end)
                {

                    if (!repeat) m_stopped = true;
                    else
                    {
                        if (swing)
                        {
                            m_forward = false;
                            m_curValue = end - halfstep - step;
                        }
                        else m_curValue = start+halfstep;
                    }
                }
            }
            else
            {
                if (m_curValue < start)
                {
                    m_forward = true;
                    m_curValue = start + halfstep*3;
                }
            }
            m_input.SetComponent(m_curValue,0);
        }

        public void Deserialize(Dictionary<GameObject, GameObject> prefabMapping)
        {
            controller = prefabMapping[controller.gameObject].GetComponent<MYTYController>();
        }

        public void SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
        {
            var newAdapter = target.AddComponent<FrameSequence1DAdapter>();
            var prefabConGo = prefabMapping[controller.gameObject];
            newAdapter.controller = prefabConGo.GetComponent<MYTYController>();
            newAdapter.start = start;
            newAdapter.end = end;
            newAdapter.repeat = repeat;
            newAdapter.swing = swing;
            newAdapter.stepCount = stepCount;
            newAdapter.unitTime = unitTime;
        }

        public JObject SerializeToJObject(Dictionary<Transform, int> transformMap)
        {
            return JObject.FromObject(new
            {
                type = "FrameSequence1DAdapter",
                controller = transformMap[controller.transform],
                start,
                end,
                repeat,
                swing,
                stepCount,
                unitTime
            });
      
        }

        public void DeserializeFromJObject(JObject jObject, Dictionary<int, Transform> idTransformMap)
        {
            start = (float) jObject["start"];
            end = (float)jObject["end"];
            repeat = (bool)jObject["repeat"];
            swing = (bool)jObject["swing"];
            stepCount = (int)jObject["stepCount"];
            unitTime = (float)jObject["unitTime"];
            controller = idTransformMap[(int)jObject["controller"]].GetComponent<MYTYController>();
        }
    }
}