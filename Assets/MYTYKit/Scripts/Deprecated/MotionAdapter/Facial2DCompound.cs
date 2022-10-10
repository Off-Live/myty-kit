using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MYTYKit.Controllers;
using UnityEngine;
using MYTYKit.MotionAdapters;
public class Facial2DCompound : NativeAdapter
{
    public FaceModel face;
    public string xFieldName;
    public string yFieldName;
    public MYTYController con;

    public float stabilizeTime = 0.1f;
    private float m_elapsed = 0;

    private FieldInfo m_xField;
    private FieldInfo m_yField;



    public override void Start()
    {
        base.Start();
        m_xField = face.GetType().GetField(xFieldName);
        m_yField = face.GetType().GetField(yFieldName);

    }

    private void Update()
    {
        if (m_xField == null || m_yField==null) return;
        if (!m_xField.FieldType.Equals(typeof(float))) return;
        if (!m_yField.FieldType.Equals(typeof(float))) return;

        var input = con as IVec2Input;
        if (input == null) return;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedVec2());
            return;
        }

        m_elapsed = 0;

        var x = (float)m_xField.GetValue(face);
        var y = (float)m_yField.GetValue(face);

        Stabilize(new Vector2(x, y));
        input.SetInput(GetStabilizedVec2());

    }
}
