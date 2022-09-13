using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

public class Facial1DAdapter : NativeAdapter
{
    public FaceModel face;
    public string fieldName;
    public MYTYController con;

    public float stabilizeTime = 0.1f;
    private float m_elapsed = 0;

    private FieldInfo m_field;

    public override void Start()
    {
        base.Start();
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
}
