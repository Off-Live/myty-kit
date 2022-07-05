using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightMiddleProximal : JointModel
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rawPoints == null) return;
        _rotation = Quaternion.LookRotation(_lookAt, _up);
        _translation = new Vector3(0, 0, 0);
    }
    private void LateUpdate()
    {
        if (rawPoints == null) return;

        var palmAxis1 = rawPoints[5] - rawPoints[0];
        var palmAxis2 = rawPoints[17] - rawPoints[0];
        var palmPlane = -Vector3.Cross(palmAxis1, palmAxis2);
        var proximal = rawPoints[10] - rawPoints[9];

        palmPlane.Normalize();
        proximal.Normalize();

        var axis = Vector3.Cross(palmPlane, proximal);
        axis.Normalize();
        _up = proximal;
        _lookAt = Vector3.Cross(axis, proximal);

    }
}
