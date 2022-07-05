using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WeightedSum2DAdapter : NativeAdapter
{

    public FaceModel face;
    public List<string> fields;
    public List<float> weights;
    public MYTYController controller;

    public float stabilizeTime = 0.1f;
    private float m_elapsed = 0;

    private List<FieldInfo> m_fields;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if (fields.Count != weights.Count) return;


        m_fields = new List<FieldInfo>();
        foreach (var fieldName in fields)
        {
            m_fields.Add(face.GetType().GetField(fieldName));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fields == null) return;
        foreach (var field in m_fields)
        {
            if (!field.FieldType.Equals(typeof(Vector2))) return;
        }

        var input = controller as IVec2Input;
        if (input == null) return;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedVec2());
            return;
        }

        m_elapsed = 0;

        var weightedSum = Vector2.zero;
        for (int i = 0; i < m_fields.Count; i++)
        {
            weightedSum += weights[i] * (Vector2)m_fields[i].GetValue(face);
        }

        Stabilize(weightedSum);
        input.SetInput(GetStabilizedVec2());
    }
}
