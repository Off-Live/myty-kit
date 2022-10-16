using System.Collections;
using System.Collections.Generic;
using System;
using MYTYKit.Controllers;
using UnityEngine;
using MYTYKit.MotionAdapters;
public class JointVec3ToVec1Adapter : NativeAdapter
{
    public int stabilizeWindow = 8;
    public int smoothWindow = 4;
    public JointModel joint;
    public JointVector from;
    public ComponentIndex component;
    public bool negate = false;
    public MYTYController controller;
    public float stabilizeTime = 0.1f;
    private float m_elapsed = 0;

    private float[] m_floatFilterArray;
    private float[] m_floatStabilizeArray;
    private float m_floatLastValue;


    private bool m_first = true;
    public virtual void Start()
    {
        m_floatFilterArray = new float[smoothWindow];
        m_floatStabilizeArray = new float[stabilizeWindow];
    }
    private void Update()
    {
        if (joint == null) return;
        var input = controller as IFloatInput;
        if (input == null) return;
        Vector3 vec3 = Vector3.zero;
        var scale = negate ? -1 : 1;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedFloat());
            return;
        }

        m_elapsed = 0;

        switch (from)
        {
            case JointVector.LookAt:
                vec3 = joint.lookAt;
                break;
            case JointVector.Up:
                vec3 = joint.upVector;
                break;
        }

        float val = 0;

        switch (component)
        {
            case ComponentIndex.X:
                val = scale * vec3.x;
                break;
            case ComponentIndex.Y:
                val = scale * vec3.y;
                break;
            case ComponentIndex.Z:
                val = scale * vec3.z;
                break;
        }

        Stabilize(val);
        input.SetInput(GetStabilizedFloat());
    }
    
    private float SmoothFilter(float newVal)
    {
        for (int i = 0; i < smoothWindow - 1; i++)
        {
            m_floatFilterArray[i] = m_floatFilterArray[i + 1];
        }

        m_floatFilterArray[smoothWindow - 1] = newVal;

        float sum = 0;
        for (int i = 0; i < smoothWindow; i++)
        {
            sum += m_floatFilterArray[i];
        }

        return sum / smoothWindow;
    }

    protected void Stabilize(float newVal)
    {
        if (m_first)
        {
            m_floatLastValue = newVal;
            for (int i = 0; i < smoothWindow; i++)
            {
                m_floatFilterArray[i] = newVal;
            }

            m_first = false;
        }

        newVal = SmoothFilter(newVal);
        for (int i = 1; i <= stabilizeWindow; i++)
        {
            m_floatStabilizeArray[i - 1] = m_floatLastValue + (newVal - m_floatLastValue) / stabilizeWindow * i;
        }

    }

    protected float GetStabilizedFloat()
    {
        var ret = m_floatStabilizeArray[0];
        for (int i = 0; i < stabilizeWindow - 1; i++)
        {
            m_floatStabilizeArray[i] = m_floatStabilizeArray[i + 1];
        }

        m_floatLastValue = ret;
        return ret;
    }
}
