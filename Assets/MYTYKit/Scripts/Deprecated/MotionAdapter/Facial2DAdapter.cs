using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MYTYKit.Controllers;
using UnityEngine;
using MYTYKit.MotionAdapters;
public class Facial2DAdapter : NativeAdapter
{
    public int stabilizeWindow = 8;
    public int smoothWindow = 4;
    public FaceModel face;
    public string fieldName;
    public MYTYController con;
    public float stabilizeTime = 0.1f;
    private float m_elapsed = 0;

    private FieldInfo m_field;
    
    private Vector2[] m_vec2FilterArray;
    private Vector2[] m_vec2StabilizeArray;
    private Vector2 m_vec2LastValue;

    private bool m_first = true;


    public void Start()
    {
        m_vec2FilterArray = new Vector2[smoothWindow];
        m_vec2StabilizeArray = new Vector2[stabilizeWindow];
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
