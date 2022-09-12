using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightLowerLeg : JointModel
{
    private Vector3 _lastLA;
    // Start is called before the first frame update
    void Start()
    {
        _lastLA = Vector3.forward;
    }

    // Update is called once per frame
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
        var upperLeg = rawPoints[24] - rawPoints[26];
        var lowerLeg = rawPoints[26] - rawPoints[28];
        upperLeg.Normalize();
        lowerLeg.Normalize();

        _up = lowerLeg;
        var axis = Vector3.Cross(lowerLeg, upperLeg);
        if (axis.magnitude < 1.0e-6)
        {
            _lookAt = _lastLA;
        }
        else
        {
            _lookAt = Vector3.Cross(axis, _up);
            _lookAt.Normalize();
            _lastLA = _lookAt;
        }

    }
}
