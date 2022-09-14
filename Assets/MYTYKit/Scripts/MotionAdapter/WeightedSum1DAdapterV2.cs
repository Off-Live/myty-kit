using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WeightedSum1DAdapterV2 : NativeAdapter
{

    public ParametricTemplate template;
    public List<string> paramNames;
    public List<float> weights;
    public MYTYController controller;

    public float stabilizeTime = 0.1f;
    float m_elapsed = 0;

    void Update()
    {
        if (paramNames.Count != weights.Count) return;
        var input = controller as IFloatInput;
        if (input == null) return;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedFloat());
            return;
        }

        m_elapsed = 0;

        var weightedSum = 0.0f;
        for(int i = 0; i < weights.Count; i++)
        {
            weightedSum += weights[i] * template.GetValue(paramNames[i]);
        }

        Stabilize(weightedSum);
        input.SetInput(GetStabilizedFloat());
    }
}
