using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MYTYKit.Controllers;
using UnityEngine;
using MYTYKit.MotionAdapters;
public class WeightedSum2DAdapter : NativeAdapter
{
    public int stabilizeWindow = 8;
    public int smoothWindow = 4;
    public FaceModel face;
    public List<string> fields;
    public List<float> weights;
    public MYTYController controller;

    public float stabilizeTime = 0.1f;
    private float m_elapsed = 0;

    private List<FieldInfo> m_fields;

    private Vector2[] m_vec2FilterArray;
    private Vector2[] m_vec2StabilizeArray;
    private Vector2 m_vec2LastValue;

    private bool m_first = true;
    // Start is called before the first frame update
    public void Start()
    {
        
        m_vec2FilterArray = new Vector2[smoothWindow];
        m_vec2StabilizeArray = new Vector2[stabilizeWindow];
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
