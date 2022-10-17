using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using MYTYKit.Controllers;
using UnityEngine;
using MYTYKit.MotionAdapters;
public class Facial1DAdapter : NativeAdapter
{
    public int stabilizeWindow = 8;
    public int smoothWindow = 4;
    public FaceModel face;
    public string fieldName;
    public MYTYController con;

    public float stabilizeTime = 0.1f;
    private float m_elapsed = 0;

    private FieldInfo m_field;
    
    private float[] m_floatFilterArray;
    private float[] m_floatStabilizeArray;
    private float m_floatLastValue;


    private bool m_first = true;


    public void Start()
    {
        
        m_floatFilterArray = new float[smoothWindow];
        m_floatStabilizeArray = new float[stabilizeWindow];
        m_field = face.GetType().GetField(fieldName);

    }

    private void Update()
    {
        if (m_field == null) return;
        if (!m_field.FieldType.Equals(typeof(System.Single))) return;

        var input = con as IFloatInput;
        if (input == null) return;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedFloat());
            return;
        }

        m_elapsed = 0;


        float val = (float) m_field.GetValue(face);

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
