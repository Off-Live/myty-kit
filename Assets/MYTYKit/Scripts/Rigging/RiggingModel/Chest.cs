using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : JointModel
{
    private void Update()
    {
        if (rawPoints == null) return;
        //Debug.DrawLine(new Vector3(0, 0, 0), _lookAt, Color.red);
        //Debug.DrawLine(new Vector3(0, 0, 0), _up, Color.green);
        _rotation = Quaternion.LookRotation(_lookAt, _up);
        _translation = new Vector3(0, 0, 0);

    }

    private void LateUpdate()
    {
        if (rawPoints == null) return;
        
        var shoulderLR = rawPoints[11] - rawPoints[12];
        var hipLR = rawPoints[23] - rawPoints[24];
        var shoulderHalf = 0.5f * (rawPoints[11] + rawPoints[12]);
        var hipHalf = 0.5f * (rawPoints[23] + rawPoints[24]);
        _up = shoulderHalf - hipHalf;
        _up.Normalize();
        shoulderLR.Normalize();
        hipLR.Normalize();

        _lookAt = -Vector3.Cross(_up, shoulderLR);
        _up = -Vector3.Cross(shoulderLR, _lookAt);
        
      }
}
