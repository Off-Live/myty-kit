using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using MYTYKit.Controllers;
using UnityEngine;
using MYTYKit.MotionAdapters;
public class JointVec3ToVec2Adapter : NativeAdapter
{
    public int stabilizeWindow = 8;
    public int smoothWindow = 4;
    public JointModel joint;
    public JointVector from;
    public ProjectionPlane plane;
    public bool flip = false;
    public MYTYController controller;

    public float stabilizeTime = 0.1f;

    public bool negate1st = false;
    public bool negate2nd = false;

    public float min1st = -1.0f;
    public float max1st = 1.0f;

    public float min2nd = -1.0f;
    public float max2nd = 1.0f;

    private float m_elapsed = 0;
    
    private Vector2[] m_vec2FilterArray;
    private Vector2[] m_vec2StabilizeArray;
    private Vector2 m_vec2LastValue;

    private bool m_first = true;

    void Start()
    {
        m_vec2FilterArray = new Vector2[smoothWindow];
        m_vec2StabilizeArray = new Vector2[stabilizeWindow];
    }
    private void Update()
    {
        if (joint == null) return;
        var input = controller as IVec2Input;
        if (input == null) return;
        Vector3 vec3 = Vector3.zero;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedVec2());
            return;
        }

        m_elapsed = 0;

        var scale1st = negate1st? -1 : 1;
        var scale2nd = negate2nd ? -1 : 1;

        switch (from)
        {
            case JointVector.LookAt:
                vec3 = joint.lookAt;
                break;
            case JointVector.Up:
                vec3 = joint.upVector;
                break;
        }

        var inputVal = Vector2.zero;

        switch (plane)
        {
            case ProjectionPlane.XY:
                if (flip) inputVal = new Vector2(vec3.y, vec3.x);
                else inputVal = new Vector2(vec3.x, vec3.y);
                break;
            case ProjectionPlane.YZ:
                if (flip) inputVal = new Vector2(vec3.z, vec3.y);
                else inputVal = new Vector2(vec3.y, vec3.z);
                break;
            case ProjectionPlane.XZ:
                if (flip) inputVal = new Vector2(vec3.z, vec3.x);
                else inputVal = new Vector2(vec3.x, vec3.z);
                break;
        }

        var x = Mathf.Clamp(inputVal.x * scale1st, min1st, max1st);
        var y = Mathf.Clamp(inputVal.y * scale2nd, min2nd, max2nd);

        inputVal = new Vector2(x,y);
        Stabilize(inputVal);
        input.SetInput(GetStabilizedVec2());
    }
       private Vector2 SmoothFilter(Vector2 newVal)
            {
                for (int i = 0; i < smoothWindow - 1; i++)
                {
                    m_vec2FilterArray[i] = m_vec2FilterArray[i + 1];
                }
    
                m_vec2FilterArray[smoothWindow - 1] = newVal;
    
                Vector2 sum = Vector2.zero;
                for (int i = 0; i < smoothWindow; i++)
                {
                    sum += m_vec2FilterArray[i];
                }
    
                return sum / smoothWindow;
            }
    
            protected void Stabilize(Vector2 newVal)
            {
                if (m_first)
                {
                    m_vec2LastValue = newVal;
                    for (int i = 0; i < smoothWindow; i++)
                    {
                        m_vec2FilterArray[i] = newVal;
                    }
    
                    m_first = false;
                }
    
                newVal = SmoothFilter(newVal);
                for (int i = 1; i <= stabilizeWindow; i++)
                {
                    m_vec2StabilizeArray[i - 1] = m_vec2LastValue + (newVal - m_vec2LastValue) / stabilizeWindow * i;
                }
    
            }
    
            protected Vector2 GetStabilizedVec2()
            {
                var ret = m_vec2StabilizeArray[0];
                for (int i = 0; i < stabilizeWindow - 1; i++)
                {
                    m_vec2StabilizeArray[i] = m_vec2StabilizeArray[i + 1];
                }
    
                m_vec2LastValue = ret;
                return ret;
            }
}
