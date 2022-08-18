using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RawPointsModel : RiggingModel
{
    public Vector3[] points;

    // Update is called once per frame
 

    private void LateUpdate()
    {
        if (rawPoints == null) return;
        points = rawPoints;
    }
}
