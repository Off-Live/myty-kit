using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftLowerArm : JointModel
{
    private Vector3 _lastUp;
    private void Start()
    {
        _lastUp = Vector3.up;
    }
    void Update()
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

        var upperArm = rawPoints[13] - rawPoints[11];
        var lowerArm = rawPoints[15] - rawPoints[13];

        upperArm.Normalize();
        lowerArm.Normalize();

        _up = Vector3.Cross(upperArm, lowerArm);
        if (_up.sqrMagnitude < 1.0e-6)
        {
            _up = _lastUp;
        }
        _up.Normalize();
        _lookAt = Vector3.Cross(lowerArm, _up);
        _lookAt.Normalize();

        _lastUp = _up;

    }
}
