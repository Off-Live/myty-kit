using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.MotionAdapters.Interpolation;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    
    public abstract class DampingAndStabilizingVec3Adapter : NativeAdapter, ISerializableAdapter
    {
        class HistoryItem
        {
            public Vector3 value;
            public float timestamp;
        }
        public bool isDamping = false;
        public bool isStabilizing = true;
        public bool isUseDampedInputToStabilizer = false;
        
        public float dampingFactor = 2;
        public int dampingWindow = 6;

        public InterpolationMethod stabilizeMethod = InterpolationMethod.BezierInterpolation;

        readonly int m_stabilizeWindowSize = 4;
        
        List<HistoryItem>[] m_history;
        List<HistoryItem>[] m_stabilizeHistory;
        
        Vector3[] m_dampedValue;

        float m_startTimestamp = 0.0f;

        protected virtual void Start()
        {
            m_startTimestamp = Time.realtimeSinceStartup;
        }

        float GetTimestamp()
        {
            return Time.realtimeSinceStartup - m_startTimestamp;
        }

        protected void SetNumInterpolationSlot(int count)
        {
            m_history = new List<HistoryItem>[count];
            m_stabilizeHistory = new List<HistoryItem>[count];
            m_dampedValue = new Vector3[count];

            for (var i = 0; i < count; i++)
            {
                m_history[i] = new();
                m_stabilizeHistory[i] = new();
                m_dampedValue[i] = Vector3.zero;
            }
        }
        protected void AddToHistory(Vector3 newValue, int slotIdx = 0)
        {
            var timestamp = GetTimestamp();
            m_history[slotIdx].Add(new HistoryItem()
            {
                value = newValue,
                timestamp = timestamp
            });
            if(m_history[slotIdx].Count>dampingWindow) m_history[slotIdx].RemoveAt(0);
            if (isDamping)
            {
                float curFactor = 1.0f/dampingFactor;
                m_dampedValue[slotIdx] = Vector3.zero;
                float totalWeight = 0.0f;
                for (var i = m_history[slotIdx].Count - 1; i >= 0; i--)
                {
                    m_dampedValue[slotIdx]+=m_history[slotIdx][i].value * curFactor;
                    totalWeight += curFactor;
                    curFactor /= dampingFactor;
                }
                m_dampedValue[slotIdx] /= totalWeight;
            }

            if (isUseDampedInputToStabilizer)
            {
                m_stabilizeHistory[slotIdx].Add(new HistoryItem()
                {
                    value = m_dampedValue[slotIdx],
                    timestamp = timestamp
                });
            }
            else
            {
                m_stabilizeHistory[slotIdx].Add(new HistoryItem()
                {
                    value = newValue,
                    timestamp = timestamp
                });
            }
            if(m_stabilizeHistory[slotIdx].Count>m_stabilizeWindowSize) m_stabilizeHistory[slotIdx].RemoveAt(0);
        }

        protected Vector3 GetResult(int slotIdx = 0)
        {
            var curTime = GetTimestamp();
            if (isStabilizing)
            {
                var pointList = new List<Vector3>();
                foreach (var item in m_stabilizeHistory[slotIdx])
                {
                    pointList.Add(item.value);
                }
                var interpolator =
                    Activator.CreateInstance(InterpolationConfig.interpMap[stabilizeMethod]) as Interpolator;
                interpolator.SetPoints(pointList);
                var intervalIndex = interpolator.GetInterestedIntervalIndex();
                if (intervalIndex >= 0)
                {
                    var t = CalcStabilizeParam(intervalIndex, curTime, slotIdx);
                    t = Mathf.Clamp01(t);
                    return interpolator.Interpolate(intervalIndex, t);
                }
            }
            
            
            if (isDamping) return m_dampedValue[slotIdx];
            
            if(m_history[slotIdx]==null || m_history[slotIdx].Count==0) return Vector3.zero;
            return m_history[slotIdx].Last().value;
        }

        float CalcStabilizeParam(int intervalIndex, float timestamp , int slotIdx)
        {
            var n = m_stabilizeHistory[slotIdx].Count;
            var lastTime = m_stabilizeHistory[slotIdx][n - 1].timestamp;
            var timeDelta = timestamp - lastTime;
            var gap = m_stabilizeHistory[slotIdx][intervalIndex+1].timestamp- m_stabilizeHistory[slotIdx][intervalIndex].timestamp;
            return timeDelta / gap;
        }

        public void Deserialize(Dictionary<GameObject, GameObject> prefabMapping)
        {
            throw new NotImplementedException();
        }

        public void SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
        {
            throw new NotImplementedException();
        }

        public JObject SerializeToJObject(Dictionary<Transform, int> transformMap)
        {
            return JObject.FromObject(new
            {
                isDamping,
                isStabilizing,
                isUseDampedInputToStabilizer,
                dampingFactor = damplingFactor,
                dampingWindow,
                stabilizeMethod = stabilizeMethod.ToString(),
                name
            });
        }

        public void DeserializeFromJObject(JObject jObject, Dictionary<int, Transform> idTransformMap)
        {
            isDamping = (bool)jObject["isDamping"];
            isStabilizing = (bool)jObject["isStabilizing"];
            isUseDampedInputToStabilizer = (bool)jObject["isUseDampedInputToStabilizer"];
            damplingFactor = (float)jObject["dampingFactor"];
            dampingWindow = (int)jObject["dampingWindow"];
            stabilizeMethod = (InterpolationMethod) Enum.Parse(typeof(InterpolationMethod), (string)jObject["stabilizeMethod"]);
            name = (string)jObject["name"];
        }
    }
}