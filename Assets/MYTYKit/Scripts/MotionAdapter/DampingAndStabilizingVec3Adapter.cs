using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.MotionAdapters.Interpolation;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    
    
    public abstract class DampingAndStabilizingVec3Adapter : NativeAdapter
    {
        class HistoryItem
        {
            public Vector3 value;
            public float timestamp;
        }
        public bool isDamping = false;
        public bool isStabilizing = true;
        public bool isUseDampedInputToStabilizer = false;
        
        public int damplingFactor = 1;
        public int dampingWindow = 6;

        public InterpolationMethod stabilizeMethod;

        readonly int m_stabilizeWindowSize = 4;
        
        List<HistoryItem> m_history=new ();
        List<HistoryItem> m_stabilizeHistory = new();
        
        Vector3 m_dampedValue = Vector3.zero;

        float m_startTimestamp = 0.0f;

        void Start()
        {
            m_startTimestamp = Time.realtimeSinceStartup;
        }

        float GetTimestamp()
        {
            return Time.realtimeSinceStartup - m_startTimestamp;
        }

        protected void AddToHistory(Vector3 newValue)
        {
            var timestamp = GetTimestamp();
            m_history.Add(new HistoryItem()
            {
                value = newValue,
                timestamp = timestamp
            });
            if(m_history.Count>dampingWindow) m_history.RemoveAt(0);
            if (isDamping)
            {
                float curFactor = 1.0f/damplingFactor;
                m_dampedValue = Vector3.zero;
                float totalWeight = 0.0f;
                for (var i = m_history.Count - 1; i >= 0; i--)
                {
                    m_dampedValue+=m_history[i].value * curFactor;
                    totalWeight += curFactor;
                    curFactor /= damplingFactor;
                }
                m_dampedValue /= totalWeight;
            }

            if (isUseDampedInputToStabilizer)
            {
                m_stabilizeHistory.Add(new HistoryItem()
                {
                    value = m_dampedValue,
                    timestamp = timestamp
                });
            }
            else
            {
                m_stabilizeHistory.Add(new HistoryItem()
                {
                    value = newValue,
                    timestamp = timestamp
                });
            }
            if(m_stabilizeHistory.Count>m_stabilizeWindowSize) m_stabilizeHistory.RemoveAt(0);
        }

        protected Vector3 GetResult()
        {
            var curTime = GetTimestamp();
            if (isStabilizing)
            {
                var pointList = new List<Vector3>();
                foreach (var item in m_stabilizeHistory)
                {
                    pointList.Add(item.value);
                }
                var interpolator =
                    Activator.CreateInstance(InterpolationConfig.interpMap[stabilizeMethod]) as Interpolator;
                interpolator.SetPoints(pointList);
                var intervalIndex = interpolator.GetInterestedIntervalIndex();
                if (intervalIndex >= 0)
                {
                    var t = CalcStabilizeParam(intervalIndex, curTime);
                    return interpolator.Interpolate(intervalIndex, t);
                }
            }
            
            
            if (isDamping) return m_dampedValue;
            return m_history.Last().value;
        }

        float CalcStabilizeParam(int intervalIndex, float timestamp)
        {
            var n = m_stabilizeHistory.Count;
            var lastTime = m_stabilizeHistory[n - 1].timestamp;
            var timeDelta = timestamp - lastTime;
            var gap = m_stabilizeHistory[intervalIndex+1].timestamp- m_stabilizeHistory[intervalIndex].timestamp;
            return timeDelta / gap;
        }

    }
}