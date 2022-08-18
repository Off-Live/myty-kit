using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AveragePosFromPointsAdapter : NativeAdapter
{
    public RawPointsModel pointsModel;
    public MYTYController controller;

    public float stabilizeTime = 0.1f;

    public List<int> targetPoints;

    public Vector3 scale = new Vector3(1,1,1);
    public Vector3 anchor;

    private float m_elapsed = 0;


    // Update is called once per frame
    void Update()
    {
        if (pointsModel == null) return;
        var input = controller as IVec3Input;
        if (input == null) return;
        
        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedVec3());
            return;
        }

        m_elapsed = 0;

        var inputVal = Vector3.zero;

        if(targetPoints!=null && targetPoints.Count > 0)
        {
           
            var count = 0;
            foreach(var idx in targetPoints)
            {
                if (idx < 0 || idx >= pointsModel.points.Length) continue;
                inputVal += pointsModel.points[idx];
                count++;
            }
            if (count == 0) return;
            inputVal /= count;
        }
        else
        {
            if (pointsModel.points.Length == 0) return;
            for(int i = 0; i < pointsModel.points.Length; i++)
            {
                inputVal += pointsModel.points[i];
            }
            inputVal /= pointsModel.points.Length;
        }

        inputVal -= anchor;
        inputVal = Vector3.Scale(inputVal, scale);
        Stabilize(inputVal);
        input.SetInput(GetStabilizedVec3());
    }
}
