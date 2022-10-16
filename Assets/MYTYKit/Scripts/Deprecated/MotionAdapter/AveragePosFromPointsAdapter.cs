using System.Collections;
using System.Collections.Generic;
using MYTYKit.Controllers;
using UnityEngine;
using MYTYKit.MotionAdapters;
public class AveragePosFromPointsAdapter : NativeAdapter
{
    public int stabilizeWindow = 8;
    public int smoothWindow = 4;
    public RawPointsModel pointsModel;
    public MYTYController controller;

    public float stabilizeTime = 0.1f;

    public List<int> targetPoints;

    public Vector3 scale = new Vector3(1,1,1);
    public Vector3 anchor;

    private float m_elapsed = 0;
    private Vector3[] m_vec3FilterArray;
    private Vector3[] m_vec3StabilizeArray;
    private Vector3 m_vec3LastValue;

    private bool m_first = true;
    void Start()
    {
        m_vec3FilterArray = new Vector3[smoothWindow];
        m_vec3StabilizeArray = new Vector3[stabilizeWindow];
    }

    // Update is called once per frame
    void Update()
    {
        if (pointsModel == null) return;
        var input = controller as IVec3Input;
        if (input == null) return;
        
        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedVec3());
            return;
        }

        m_elapsed = 0;

        var inputVal = Vector3.zero;

        if(targetPoints!=null && targetPoints.Count > 0)
        {
           
            var count = 0;
            foreach(var idx in targetPoints)
            {
                if (idx < 0 || idx >= pointsModel.points.Length) continue;
                inputVal += pointsModel.points[idx];
                count++;
            }
            if (count == 0) return;
            inputVal /= count;
        }
        else
        {
            if (pointsModel.points.Length == 0) return;
            for(int i = 0; i < pointsModel.points.Length; i++)
            {
                inputVal += pointsModel.points[i];
            }
            inputVal /= pointsModel.points.Length;
        }

        inputVal -= anchor;
        inputVal = Vector3.Scale(inputVal, scale);
        Stabilize(inputVal);
        input.SetInput(GetStabilizedVec3());
    }
    
         private Vector3 SmoothFilter(Vector3 newVal)
            {
                for (int i = 0; i < smoothWindow - 1; i++)
                {
                    m_vec3FilterArray[i] = m_vec3FilterArray[i + 1];
                }
    
                m_vec3FilterArray[smoothWindow - 1] = newVal;
    
                Vector3 sum = Vector3.zero;
                for (int i = 0; i < smoothWindow; i++)
                {
                    sum += m_vec3FilterArray[i];
                }
    
                return sum / smoothWindow;
            }
    
            protected void Stabilize(Vector3 newVal)
            {
                if (m_first)
                {
                    m_vec3LastValue = newVal;
                    for (int i = 0; i < smoothWindow; i++)
                    {
                        m_vec3FilterArray[i] = newVal;
                    }
    
                    m_first = false;
                }
    
                newVal = SmoothFilter(newVal);
                for (int i = 1; i <= stabilizeWindow; i++)
                {
                    m_vec3StabilizeArray[i - 1] = m_vec3LastValue + (newVal - m_vec3LastValue) / stabilizeWindow * i;
                }
    
            }
    
            protected Vector3 GetStabilizedVec3()
            {
                var ret = m_vec3StabilizeArray[0];
                for (int i = 0; i < stabilizeWindow - 1; i++)
                {
                    m_vec3StabilizeArray[i] = m_vec3StabilizeArray[i + 1];
                }
    
                m_vec3LastValue = ret;
                return ret;
            }
    

}
