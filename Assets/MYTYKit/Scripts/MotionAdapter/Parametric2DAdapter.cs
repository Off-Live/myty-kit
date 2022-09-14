using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Parametric2DAdapter : NativeAdapter
{
    public ParametricTemplate template;
    public string xParamName;
    public string yParamName;
    public MYTYController con;
    public float stabilizeTime = 0.1f;
    float m_elapsed = 0;

    void Update()
    {
        var input = con as IVec2Input;
        if (input == null) return;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedVec2());
            return;
        }

        m_elapsed = 0;

        Vector2 val = new Vector2(template.GetValue(xParamName), template.GetValue(yParamName));

        Stabilize(val);
        input.SetInput(GetStabilizedVec2());

    }
}
