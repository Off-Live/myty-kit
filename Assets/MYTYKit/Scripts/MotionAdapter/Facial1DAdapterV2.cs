using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

public class Facial1DAdapterV2 : NativeAdapter
{
    public ParametricTemplate face;
    public string paramName;
    public MYTYController con;

    public float stabilizeTime = 0.1f;
    float m_elapsed = 0;
    void Update()
    {
        var input = con as IFloatInput;
        if (input == null) return;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedFloat());
            return;
        }

        m_elapsed = 0;
        
        float val = face.GetValue(paramName);
        Stabilize(val);
        input.SetInput(GetStabilizedFloat());
        
    }
}
