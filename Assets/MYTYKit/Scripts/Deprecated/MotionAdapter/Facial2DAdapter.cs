using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Facial2DAdapter : NativeAdapter
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
        if (!m_field.FieldType.Equals(typeof(Vector2))) return;

        var input = con as IVec2Input;
        if (input == null) return;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedVec2());
            return;
        }

        m_elapsed = 0;

        Vector2 val =(Vector2) m_field.GetValue(face);

        Stabilize(val);
        input.SetInput(GetStabilizedVec2());

    }
}
